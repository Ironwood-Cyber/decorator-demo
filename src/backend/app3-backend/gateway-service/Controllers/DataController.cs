using System.Threading.Tasks;
using GatewayService.Handlers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace GatewayService.Controllers;

/// <summary>
/// Controller for the data endpoint
/// </summary>
[Route("/api/[controller]")]
[ApiController]
public class DataController(IDataHandler dataHandler) : ControllerBase
{
    private readonly IDataHandler _dataHandler = dataHandler;

    /// <summary>
    /// Gets the data for the JsonForm component
    /// </summary>
    /// <response code="200">Returns the data for the JsonForm component</response>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAsync()
    {
        JObject data = await _dataHandler.GetDataAsync();
        return Ok(data.ToString());
    }
}
