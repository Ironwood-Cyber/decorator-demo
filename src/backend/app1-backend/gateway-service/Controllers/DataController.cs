using System.Threading.Tasks;
using GatewayService.Handlers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace GatewayService.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class DataController(IDataHandler dataHandler) : ControllerBase
{
    private readonly IDataHandler _dataHandler = dataHandler;

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        JObject data = await _dataHandler.GetDataAsync();
        return Ok(data.ToString());
    }
}
