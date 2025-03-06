using System.Threading.Tasks;
using GatewayService.Handlers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace GatewayService.Controllers;

/// <summary>
/// Controller for the UI schema endpoint
/// </summary>
[Route("/api/[controller]")]
[ApiController]
public class UiSchemaController(IDataHandler dataHandler) : ControllerBase
{
    private readonly IDataHandler _dataHandler = dataHandler;

    /// <summary>
    /// Gets the UI schema for the JsonForm component
    /// </summary>
    /// <response code="200">Returns the UI schema for the JsonForm component</response>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAsync()
    {
        JObject data = await _dataHandler.GetUiSchemaAsync();
        return Ok(data.ToString());
    }
}
