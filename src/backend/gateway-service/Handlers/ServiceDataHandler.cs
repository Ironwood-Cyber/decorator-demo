using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GatewayService.Configuration;
using GatewayService.Configuration.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Shared;

namespace GatewayService.Handlers;

public class ServiceDataHandler : IDataHandler
{
    private const int RequestTimeoutMs = 5000; // 5 seconds

    private readonly ILogger<ServiceDataHandler> _logger;
    private readonly ServiceHandlerConfig _serviceConfig;
    private readonly Dictionary<string, RestClient> _restClientMap = []; // Key = service URI, Value = RestClient for that service

    public ServiceDataHandler(ILogger<ServiceDataHandler> logger, IOptionsSnapshot<ServiceHandlerConfig> configuration)
    {
        _logger = logger;
        _serviceConfig = configuration.Value;

        List<string> decoratorUris = [.. _serviceConfig.DecoratorServices.Select(service => service.ServiceUri)];
        _logger.LogInformation("Decorator URIs: {Uris}", string.Join(", ", decoratorUris));

        // Add a RestClient for the base service
        RestClientOptions baseServiceOptions = new(_serviceConfig.BaseServiceUri) { ThrowOnAnyError = true, Timeout = TimeSpan.FromMilliseconds(RequestTimeoutMs) };
        _restClientMap.Add(_serviceConfig.BaseServiceUri, new RestClient(baseServiceOptions));

        // Add a RestClient for each decorator service
        foreach (var uri in decoratorUris)
        {
            RestClientOptions options = new(uri) { ThrowOnAnyError = true, Timeout = TimeSpan.FromMilliseconds(RequestTimeoutMs) };
            _restClientMap.Add(uri, new RestClient(options));
        }
    }

    /// <inheritdoc/>
    public async Task<JObject> GetDataAsync()
    {
        JObject data = [];
        foreach (var restClient in _restClientMap.Values)
        {
            JObject? serviceData = await FetchJsonAsync(restClient, Constants.Routes.Data);
            if (serviceData is null)
            {
                continue;
            }
            data.Merge(serviceData);
        }
        return data;
    }

    /// <inheritdoc/>
    public async Task<JObject> GetSchemaAsync()
    {
        JObject schema = [];
        foreach (var restClient in _restClientMap.Values)
        {
            JObject? serviceSchema = await FetchJsonAsync(restClient, Constants.Routes.Schema);
            if (serviceSchema is null)
            {
                continue;
            }
            schema.Merge(serviceSchema);
        }
        return schema;
    }

    /// <inheritdoc/>
    public async Task<JObject> GetUiSchemaAsync()
    {
        JObject? uiSchema = null;
        foreach (var restClient in _restClientMap.Values)
        {
            JObject? serviceUiSchema = await FetchJsonAsync(restClient, Constants.Routes.UiSchema);
            if (serviceUiSchema is null)
            {
                continue;
            }
            if (uiSchema is null)
            {
                uiSchema = serviceUiSchema;
                continue;
            }
            uiSchema = JsonUtils.MergeJsonObjects(uiSchema, serviceUiSchema);
        }
        uiSchema!.SortJsonById();
        return uiSchema!;
    }

    /// <inheritdoc/>
    public async Task<JObject> GetEventHandlerAsync()
    {
        JObject eventHandler = [];
        foreach (var restClient in _restClientMap.Values)
        {
            JObject? serviceEventHandler = await FetchJsonAsync(restClient, Constants.Routes.EventHandler);
            if (serviceEventHandler is null)
            {
                continue;
            }
            eventHandler.Merge(serviceEventHandler);
        }
        return eventHandler;
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
        var overrideClients = _serviceConfig.DecoratorServices.Where(s => s.ExecutionOrder == EventExecutionOrder.OverrideBaseService).Select(s => _restClientMap[s.ServiceUri]);
        var beforeClients = _serviceConfig.DecoratorServices.Where(s => s.ExecutionOrder == EventExecutionOrder.BeforeBaseService).Select(s => _restClientMap[s.ServiceUri]);
        var afterClients = _serviceConfig.DecoratorServices.Where(s => s.ExecutionOrder == EventExecutionOrder.AfterBaseServoce).Select(s => _restClientMap[s.ServiceUri]);

        // Handle services that override the base service
        if (overrideClients.Any())
        {
            var overrideClientResponses = await GetEventResponsesAsync(overrideClients, originalJsonData);
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
        if (beforeClients.Any())
        {
            var beforeClientResponses = await GetEventResponsesAsync(beforeClients, originalJsonData);
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
        var baseServiceResponse = await GetEventResponseAsync(_restClientMap[_serviceConfig.BaseServiceUri], mergedBeforeResponse ?? originalJsonData);
        if (baseServiceResponse is null)
        {
            return null;
        }

        // Handle services that run after the base service using the response from the base service
        if (afterClients.Any())
        {
            var afterClientResponses = await GetEventResponsesAsync(afterClients, baseServiceResponse);
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

        return baseServiceResponse!;
    }

    /// <summary>
    /// Fetches JSON data from the specified path using the given RestClient
    /// </summary>
    /// <param name="restClient">The client to call</param>
    /// <param name="path">The request path for the client</param>
    /// <returns>The restclient response as a JObject</returns>
    private async Task<JObject?> FetchJsonAsync(RestClient restClient, string path)
    {
        // Fetch the JSON data from the specified path using the given RestClient and return it as a JObject
        try
        {
            var response = await restClient.GetAsync(new RestRequest(path));
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var json = JsonConvert.DeserializeObject(response.Content ?? string.Empty);
            return JObject.Parse(json?.ToString() ?? string.Empty);
        }
        catch (JsonReaderException e)
        {
            _logger.LogWarning(e, "Failed to parse JSON response");
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Failed to send request to client");
        }
        catch (TimeoutException e)
        {
            _logger.LogWarning(e, "Request timed out");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "An unexpected error occurred");
        }
        return null;
    }

    /// <summary>
    /// Sends the request data to each client handle event route and returns the responses as a list of JObjects
    /// </summary>
    /// <param name="clients">The list of rest clients</param>
    /// <param name="requestData">The body of the handle event request</param>
    /// <returns>A list of responses from the rest clients handle event route</returns>
    private async Task<IEnumerable<JObject>> GetEventResponsesAsync(IEnumerable<RestClient> clients, JObject requestData)
    {
        var tasks = clients.Select(async client => await GetEventResponseAsync(client, requestData));
        return (await Task.WhenAll(tasks)).Where(response => response != null).Select(response => response!);
    }

    /// <summary>
    /// Sends the request data to the client handle event route and returns the response as a JObject
    /// </summary>
    /// <param name="client">The rest client</param>
    /// <param name="requestData">The request body for the handle event route</param>
    /// <returns>The response body from the rest client handle event route as a JObject</returns>
    private async Task<JObject?> GetEventResponseAsync(RestClient client, JObject requestData)
    {
        try
        {
            // Send the request data to the client handle event route and return the response as a JObject
            var response = await client.PostAsync(new RestRequest(Constants.Routes.Event, Method.Post).AddJsonBody(requestData.ToString()));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Status code: {Code}", response.StatusCode);
                return null;
            }
            var json = JsonConvert.DeserializeObject(response.Content ?? string.Empty);
            JObject serviceResponse = JObject.Parse(json?.ToString() ?? string.Empty);
            return serviceResponse;
        }
        catch (JsonReaderException e)
        {
            _logger.LogWarning(e, "Failed to parse JSON response");
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Failed to send request to client");
        }
        catch (TimeoutException e)
        {
            _logger.LogWarning(e, "Request timed out");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "An unexpected error occurred");
        }
        return null;
    }
}
