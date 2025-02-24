using System.Threading.Tasks;
using GatewayService.Handlers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace GatewayService.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class EventController(IDataHandler dataHandler) : ControllerBase
{
    private readonly IDataHandler _dataHandler = dataHandler;

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        JObject res = await _dataHandler.GetEventHandlerAsync();
        return res.HasValues ? Ok(res.ToString()) : BadRequest();
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> HandleEventAsync(object data)
    {
        JObject? res = await _dataHandler.HandleEventAsync(data);
        return res is null ? BadRequest() : Ok(res.ToString());
    }
}
