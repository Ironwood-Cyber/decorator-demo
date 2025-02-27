using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Shared;

namespace GatewayService.Handlers;

public class MefDataHandler : IDataHandler
{
    private readonly ILogger<MefDataHandler> _logger;

    [ImportMany]
#pragma warning disable S1104 // Fields should not have public accessibility
    public IEnumerable<Lazy<ISchemaHandler, IHandlerData>> _schemaHandlers;
#pragma warning restore S1104 // Fields should not have public accessibility

    [ImportMany]
#pragma warning disable S1104 // Fields should not have public accessibility
    public IEnumerable<Lazy<IEventHandler, IHandlerData>> _eventHandlers;
#pragma warning restore S1104 // Fields should not have public accessibility

    public MefDataHandler(ILogger<MefDataHandler> logger)
    {
        _logger = logger;

        var catalog = new AggregateCatalog();
        catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory + "/bin/Debug/Extensions/base-service/net8.0"));
        // var files = Directory.GetFiles(Environment.CurrentDirectory + "/bin/Debug/Extensions", "*.dll", SearchOption.AllDirectories);
        // foreach (var dllFile in files)
        // {
        //     try
        //     {
        //         var assembly = Assembly.LoadFile(dllFile);
        //         var assemblyCatalog = new AssemblyCatalog(assembly);
        //         catalog.Catalogs.Add(assemblyCatalog);
        //     }
        //     catch (Exception)
        //     {
        //         // this happens if the given dll file is not  a .NET framework file or corrupted.
        //     }
        // }

        // Create the CompositionContainer with the parts in the catalog
        var container = new CompositionContainer(catalog);
        container.ComposeParts(this);

        // There must be a single base service in the handlers
        if (_schemaHandlers!.Count(e => e.Metadata.ServiceType == ServiceType.Base) != 1)
            throw new InvalidOperationException("There must be a single base service in the handlers");
        if (_eventHandlers!.Count(e => e.Metadata.ServiceType == ServiceType.Base) != 1)
            throw new InvalidOperationException("There must be a single base service in the handlers");
    }

    public async Task<JObject> GetDataAsync()
    {
        JObject result = [];

        foreach (var schemaHandler in _schemaHandlers)
        {
            try
            {
                string data = await schemaHandler.Value.GetDataAsStringAsync();
                result.Merge(JObject.Parse(data));
            }
            catch (NotImplementedException)
            {
                // pass
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting data from schema handler");
            }
        }

        return result;
    }

    public async Task<JObject> GetSchemaAsync()
    {
        JObject result = [];

        foreach (var schemaHandler in _schemaHandlers)
        {
            try
            {
                string data = await schemaHandler.Value.GetSchemaAsStringAsync();
                result.Merge(JObject.Parse(data));
            }
            catch (NotImplementedException)
            {
                // pass
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting schema from schema handler");
            }
        }

        return result;
    }

    public async Task<JObject> GetUiSchemaAsync()
    {
        JObject? result = null;

        foreach (var schemaHandler in _schemaHandlers)
        {
            try
            {
                string data = await schemaHandler.Value.GetUiSchemaAsStringAsync();
                if (result is null)
                {
                    result = JObject.Parse(data);
                }
                else
                {
                    result = JsonUtils.MergeJsonObjects(result, JObject.Parse(data));
                }
            }
            catch (NotImplementedException)
            {
                // pass
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting UI schema from schema handler");
            }
        }

        result!.SortJsonById();
        return result!;
    }

    public async Task<JObject> GetEventHandlerAsync()
    {
        JObject result = [];

        foreach (var eventHandler in _eventHandlers)
        {
            try
            {
                string handler = await eventHandler.Value.GetEventHandlerAsync();
                JObject jsonHandler = [];
                jsonHandler.Add("handler", handler);
                result.Merge(jsonHandler);
            }
            catch (NotImplementedException)
            {
                // pass
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting event handler from event handler");
            }
        }

        return result;
    }

    public async Task<JObject?> HandleEventAsync(object data)
    {
        // Validate the data
        JObject originalJsonData = JObject.Parse(data?.ToString() ?? string.Empty);
        if (originalJsonData is null || !originalJsonData.HasValues)
        {
            return null;
        }
        _logger.LogInformation("Received {Type} data: {Data}", data!.GetType().Name, originalJsonData.ToString());

        // Group all the decorator services by their execution order
        var overrideHandlers = _eventHandlers.Where(e => e.Metadata.ServiceType == ServiceType.Decorator).Where(e => e.Metadata.ExecutionOrder == EventExecutionOrder.OverrideBaseService);
        var beforeHandlers = _eventHandlers.Where(e => e.Metadata.ServiceType == ServiceType.Decorator).Where(e => e.Metadata.ExecutionOrder == EventExecutionOrder.BeforeBaseService);
        var afterHandlers = _eventHandlers.Where(e => e.Metadata.ServiceType == ServiceType.Decorator).Where(e => e.Metadata.ExecutionOrder == EventExecutionOrder.AfterBaseService);

        // Handle services that override the base service
        if (overrideHandlers.Any())
        {
            var overrideClientResponses = GetEventResponsesAsync(overrideHandlers, originalJsonData);
            if (overrideClientResponses.Any())
            {
                // Return the first item in the list if only one item, otherwise merge the responses if multiple
                if (overrideClientResponses.Count() == 1)
                {
                    return overrideClientResponses.First();
                }

                JObject mergedResponse = overrideClientResponses.First();
                for (int i = 1; i < overrideClientResponses.Count(); i++)
                {
                    mergedResponse.Merge(overrideClientResponses.ElementAt(i));
                }
                return mergedResponse;
            }
        }

        // Handle services that run before the base service
        JObject? mergedBeforeResponse = null;
        if (beforeHandlers.Any())
        {
            var beforeClientResponses = GetEventResponsesAsync(beforeHandlers, originalJsonData);
            if (beforeClientResponses.Any())
            {
                // Process these responses, merge if multiple, send that data to the base service for further processing
                if (beforeClientResponses.Count() == 1)
                {
                    mergedBeforeResponse = beforeClientResponses.First();
                }
                else
                {
                    mergedBeforeResponse = beforeClientResponses.First();
                    for (int i = 1; i < beforeClientResponses.Count(); i++)
                    {
                        mergedBeforeResponse.Merge(beforeClientResponses.ElementAt(i));
                    }
                }
            }
        }

        // Send the data to the base service
        var baseService = _eventHandlers.Single(e => e.Metadata.ServiceType == ServiceType.Base);
        var baseServiceResponse = GetEventResponseAsync(baseService, mergedBeforeResponse ?? originalJsonData);
        if (baseServiceResponse is null)
        {
            return null;
        }

        // Handle services that run after the base service using the response from the base service
        if (afterHandlers.Any())
        {
            var afterClientResponses = GetEventResponsesAsync(afterHandlers, baseServiceResponse);
            if (afterClientResponses.Any())
            {
                // Process these responses, merge if multiple, send that data to the base service for further processing
                if (afterClientResponses.Count() == 1)
                {
                    return afterClientResponses.First();
                }
                JObject mergedAfterResponse = afterClientResponses.First();
                for (int i = 1; i < afterClientResponses.Count(); i++)
                {
                    mergedAfterResponse.Merge(afterClientResponses.ElementAt(i));
                }
                return mergedAfterResponse;
            }
        }

        // Use task to get rid of warning
        return await Task.Run(() => baseServiceResponse!);
    }

    private IEnumerable<JObject> GetEventResponsesAsync(IEnumerable<Lazy<IEventHandler, IHandlerData>> handlers, JObject requestData)
    {
        var results = handlers.Select(handler => GetEventResponseAsync(handler, requestData));
        return results.Where(response => response != null).Select(response => response!);
    }

    private JObject? GetEventResponseAsync(Lazy<IEventHandler, IHandlerData> handler, JObject requestData)
    {
        try
        {
            string? handledEvent = handler.Value.HandleSchemaEvent(requestData);
            if (handledEvent is null)
            {
                _logger.LogError("Could not get result from event handler");
                return null;
            }
            return JObject.Parse(handledEvent);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "An unexpected error occurred");
        }

        return null;
    }
}
