using System.Collections.Generic;

namespace GatewayService.Configuration.Dll;

/// <summary>
/// Wrapper around an appsettings.json section to configure options at runtime
/// </summary>
public class DllHandlerConfig
{
    /// <summary>
    /// The section in the appsettings.json file that this configuration is stored in
    /// </summary>
    public const string ConfigSection = nameof(DllHandlerConfig);

    /// <summary>
    /// The base service dll to use for the gateway service
    /// </summary>
    public string BaseServiceDll { get; set; } = string.Empty;

    /// <summary>
    /// The list of decorator service dlls to use for the gateway service
    /// </summary>
    public List<DecoratorServiceDllDefinition> DecoratorServiceDlls { get; set; } = [];
}
