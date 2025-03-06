using System.Threading.Tasks;
using GatewayService.Handlers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace GatewayService.Controllers;

/// <summary>
/// Controller for the event endpoints
/// </summary>
[Route("/api/[controller]")]
[ApiController]
public class EventController(IDataHandler dataHandler) : ControllerBase
{
    private readonly IDataHandler _dataHandler = dataHandler;

    /// <summary>
    /// Gets the client side event handler code
    /// </summary>
    /// <response code="200">Returns the client side event handler code</response>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAsync()
    {
        JObject res = await _dataHandler.GetEventHandlerAsync();
        return res.HasValues ? Ok(res.ToString()) : BadRequest();
    }

    /// <summary>
    /// Handles the event on the server side
    /// </summary>
    /// <param name="data">The event data</param>
    /// <response code="200">Returns the response from the server side event handler</response>
    /// <response code="400">If the event data is invalid</response>
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> HandleEventAsync(object data)
    {
        JObject? res = await _dataHandler.HandleEventAsync(data);
        return res is null ? BadRequest() : Ok(res.ToString());
    }
}
