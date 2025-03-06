using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GatewayService.Configuration;
using GatewayService.Configuration.Dll;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Shared;

namespace GatewayService.Handlers;

public class DllDataHandler : IDataHandler
{
    private readonly ILogger<DllDataHandler> _logger;
    private readonly DllHandlerConfig _dllConfig;
    private readonly Type _schemaHandlerType = typeof(ISchemaHandler);
    private readonly Type _eventHandlerType = typeof(IEventHandler);
    private readonly Dictionary<string, Type> _typeMap = []; // Key = DLL path, Value = Type implementing ISchemaHandler and/or IEventHandler

    public DllDataHandler(ILogger<DllDataHandler> logger, IOptionsSnapshot<DllHandlerConfig> configuration)
    {
        _logger = logger;
        _dllConfig = configuration.Value;

        List<DecoratorServiceDllDefinition> decoratorDlls = _dllConfig.DecoratorServiceDlls;
        _logger.LogInformation("Decorator Dlls: {Dlls}", string.Join(", ", decoratorDlls.Select(dll => dll.ServiceDll)));

        // Add a Type for the base service class using the base service DLL and reflection
        Assembly baseAssembly = Assembly.LoadFrom(_dllConfig.BaseServiceDll);
        Type? baseType =
            baseAssembly.GetTypes().SingleOrDefault(t => !t.IsInterface && !t.IsAbstract && (_schemaHandlerType.IsAssignableFrom(t) || _eventHandlerType.IsAssignableFrom(t)))
            ?? throw new ArgumentException($"Could not find types {_schemaHandlerType.Name} or {_eventHandlerType.Name} in assembly {_dllConfig.BaseServiceDll}");
        _typeMap.Add(_dllConfig.BaseServiceDll, baseType);

        // Add a Type for each decorator service class using the decorator service DLLs and reflection
        foreach (var serviceDll in decoratorDlls.Select(dll => dll.ServiceDll))
        {
            Assembly assembly = Assembly.LoadFrom(serviceDll);
            Type? type =
                assembly.GetTypes().SingleOrDefault(t => !t.IsInterface && !t.IsAbstract && (_schemaHandlerType.IsAssignableFrom(t) || _eventHandlerType.IsAssignableFrom(t)))
                ?? throw new ArgumentException($"Could not find types {_schemaHandlerType.Name} or {_eventHandlerType.Name} in assembly {serviceDll}");
            _typeMap.Add(serviceDll, type);
        }
    }

    /// <inheritdoc/>
    public async Task<JObject> GetDataAsync()
    {
        JObject result = [];

        // Loop thru each type in the dictionary and call the GetData method
        foreach (Type type in _typeMap.Values)
        {
            // Create an instance of the type
            if (Activator.CreateInstance(type) is not ISchemaHandler schemaHandler)
            {
                _logger.LogWarning("Could not create instance of type {Class}", type.FullName);
                continue;
            }

            try
            {
                string data = await schemaHandler.GetDataAsStringAsync();
                // Merge the data into the result
                result.Merge(JObject.Parse(data));
            }
            catch (NotImplementedException)
            {
                _logger.LogWarning("Method {Method} in type {Class} is not implemented", nameof(schemaHandler.GetDataAsStringAsync), type.FullName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error invoking method {Method} in type {Class}", nameof(schemaHandler.GetDataAsStringAsync), type.FullName);
                continue;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<JObject> GetSchemaAsync()
    {
        JObject result = [];

        // Loop thru each type in the dictionary and call the GetSchema method
        foreach (Type type in _typeMap.Values)
        {
            // Create an instance of the type
            if (Activator.CreateInstance(type) is not ISchemaHandler schemaHandler)
            {
                _logger.LogWarning("Could not create instance of type {Class}", type.FullName);
                continue;
            }

            try
            {
                string schema = await schemaHandler.GetSchemaAsStringAsync();
                // Merge the schema into the result
                result.Merge(JObject.Parse(schema));
            }
            catch (NotImplementedException)
            {
                _logger.LogWarning("Method {Method} in type {Class} is not implemented", nameof(schemaHandler.GetSchemaAsStringAsync), type.FullName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error invoking method {Method} in type {Class}", nameof(schemaHandler.GetSchemaAsStringAsync), type.FullName);
                continue;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<JObject> GetUiSchemaAsync()
    {
        JObject? result = null;

        // Loop thru each type in the dictionary and call the GetUiSchema method
        foreach (Type type in _typeMap.Values)
        {
            // Create an instance of the type
            if (Activator.CreateInstance(type) is not ISchemaHandler schemaHandler)
            {
                _logger.LogWarning("Could not create instance of type {Class}", type.FullName);
                continue;
            }

            try
            {
                string data = await schemaHandler.GetUiSchemaAsStringAsync();
                if (result is null)
                {
                    result = JObject.Parse(data);
                }
                else
                {
                    // Merge the data into the result using the JsonUtils helper class
                    result = JsonUtils.MergeJsonObjects(result, JObject.Parse(data));
                }
            }
            catch (NotImplementedException)
            {
                _logger.LogWarning("Method {Method} in type {Class} is not implemented", nameof(schemaHandler.GetUiSchemaAsStringAsync), type.FullName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error invoking method {Method} in type {Class}", nameof(schemaHandler.GetUiSchemaAsStringAsync), type.FullName);
                continue;
            }
        }

        // Sort the result by id using the JsonUtils helper class
        result!.SortJsonById();
        return result!;
    }

    /// <inheritdoc/>
    public async Task<JObject> GetEventHandlerAsync()
    {
        JObject result = [];

        // Loop thru each type in the dictionary and call the GetEventHandler method
        foreach (Type type in _typeMap.Values)
        {
            // Create an instance of the type
            if (Activator.CreateInstance(type) is not IEventHandler eventHandler)
            {
                _logger.LogWarning("Could not create instance of type {Class}", type.FullName);
                continue;
            }

            try
            {
                // The get event handler method returns raw javascript code as a string, so we need to wrap it in a json object with a key of "handler"
                string handler = await eventHandler.GetEventHandlerAsync();
                JObject jsonHandler = [];
                jsonHandler.Add("handler", handler);
                // Merge the handler into the result
                result.Merge(jsonHandler);
            }
            catch (NotImplementedException)
            {
                _logger.LogWarning("Method {Method} in type {Class} is not implemented", nameof(eventHandler.GetEventHandlerAsync), type.FullName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error invoking method {Method} in type {Class}", nameof(eventHandler.GetEventHandlerAsync), type.FullName);
                continue;
            }
        }

        return result;
    }

    /// <inheritdoc/>
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
        var overrideTypes = _dllConfig.DecoratorServiceDlls.Where(s => s.ExecutionOrder == EventExecutionOrder.OverrideBaseService).Select(s => _typeMap[s.ServiceDll]);
        var beforeTypes = _dllConfig.DecoratorServiceDlls.Where(s => s.ExecutionOrder == EventExecutionOrder.BeforeBaseService).Select(s => _typeMap[s.ServiceDll]);
        var afterTypes = _dllConfig.DecoratorServiceDlls.Where(s => s.ExecutionOrder == EventExecutionOrder.AfterBaseServoce).Select(s => _typeMap[s.ServiceDll]);

        // Handle services that override the base service
        if (overrideTypes.Any())
        {
            var overrideClientResponses = GetEventResponsesAsync(overrideTypes, originalJsonData);
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
        if (beforeTypes.Any())
        {
            var beforeClientResponses = GetEventResponsesAsync(beforeTypes, originalJsonData);
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
        var baseServiceResponse = GetEventResponseAsync(_typeMap[_dllConfig.BaseServiceDll], mergedBeforeResponse ?? originalJsonData);
        if (baseServiceResponse is null)
        {
            return null;
        }

        // Handle services that run after the base service using the response from the base service
        if (afterTypes.Any())
        {
            var afterClientResponses = GetEventResponsesAsync(afterTypes, baseServiceResponse);
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

    /// <summary>
    /// Get the event responses from the specified types
    /// </summary>
    /// <param name="types">A list of types</param>
    /// <param name="requestData">The JSON data to send to each type</param>
    /// <returns>A list of JObjects representing the responses from each types event handler function</returns>
    private IEnumerable<JObject> GetEventResponsesAsync(IEnumerable<Type> types, JObject requestData)
    {
        var results = types.Select(type => GetEventResponseAsync(type, requestData));
        return results.Where(response => response != null).Select(response => response!);
    }

    /// <summary>
    /// Get the event response from the specified type
    /// </summary>
    /// <param name="type">The type to call the event handler function for</param>
    /// <param name="requestData">The JSON data to send to the types event handler function</param>
    /// <returns>The JObject result from the types event handler function</returns>
    private JObject? GetEventResponseAsync(Type type, JObject requestData)
    {
        // Create an instance of the type
        if (Activator.CreateInstance(type) is not IEventHandler eventHandler)
        {
            _logger.LogWarning("Could not create instance of type {Class}", type.FullName);
            return null;
        }

        try
        {
            // Call the HandleEvent method and return the result as a JObject
            string? handledEvent = eventHandler.HandleSchemaEvent(requestData);
            if (handledEvent is null)
            {
                _logger.LogError("Could not get result from method {Method} in type {Class}", nameof(eventHandler.HandleSchemaEvent), type.FullName);
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
