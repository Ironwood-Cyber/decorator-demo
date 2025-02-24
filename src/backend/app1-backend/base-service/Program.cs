using BaseService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
app.MapGet(Constants.Routes.Data, async (ISchemaHandler handler) => Results.Text(await handler.GetDataAsStringAsync(), Application.Json));
app.MapGet(Constants.Routes.Schema, async (ISchemaHandler handler) => Results.Text(await handler.GetSchemaAsStringAsync(), Application.Json));
app.MapGet(Constants.Routes.UiSchema, async (ISchemaHandler handler) => Results.Text(await handler.GetUiSchemaAsStringAsync(), Application.Json));

// Event handler endpoints
app.MapPost(
    Constants.Routes.Event,
    (IEventHandler handler, object data) =>
    {
        string? result = handler.HandleSchemaEvent(data);
        return result is null ? Results.BadRequest() : Results.Text(result, Application.Json);
    }
);

await app.RunAsync("http://localhost:5000");
