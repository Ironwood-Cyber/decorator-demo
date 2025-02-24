using DecoratorService4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Shared;
using static System.Net.Mime.MediaTypeNames;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(builder => builder.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

builder.Services.AddSingleton<ISchemaHandler, SchemaHandler>();
builder.Services.AddSingleton<IEventHandler, SchemaHandler>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

// Schema endpoints
app.MapGet(Constants.Routes.UiSchema, async (ISchemaHandler handler) => Results.Text(await handler.GetUiSchemaAsStringAsync(), Application.Json));

// Event handler endpoints
app.MapGet(
    Constants.Routes.EventHandler,
    async (IEventHandler handler) =>
    {
        string eventHandler = await handler.GetEventHandlerAsync();
        JObject eventHandlerJson = [];
        eventHandlerJson.Add("handler", eventHandler); // format will be json encoded javascript... gross but it works ({"handler": "function(event) { ... }"})
        return Results.Text(eventHandlerJson.ToString(), Application.Json);
    }
);

await app.RunAsync("http://localhost:5004");
