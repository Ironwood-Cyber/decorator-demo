using System.Collections.Generic;

namespace GatewayService.Configuration.Service;

/// <summary>
/// Wrapper around an appsettings.json section to configure options at runtime
/// </summary>
public class ServiceHandlerConfig
{
    /// <summary>
    /// The section in the appsettings.json file that this configuration is stored in
    /// </summary>
    public const string ConfigSection = nameof(ServiceHandlerConfig);

    /// <summary>
    /// The base service uri to use for the gateway service
    /// </summary>
    public string BaseServiceUri { get; set; } = string.Empty;

    /// <summary>
    /// The list of decorator services to use for the gateway service
    /// </summary>
    public List<DecoratorServiceDefinition> DecoratorServices { get; set; } = [];
}
