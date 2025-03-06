using DecoratorService3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using static System.Net.Mime.MediaTypeNames;

// This file is the entry point for the application. It sets up the application's services and endpoints.
// Only used when running the application as a standalone service for the service-based implementation.

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Set up controllers, swagger, and CORS
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(builder => builder.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

// Add mongo and mass transit services used by the handlers
builder.Services.AddMongoDB();
builder.Services.AddRabbitMq();

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
app.MapPost(
    Constants.Routes.Event,
    (IEventHandler handler, object data) =>
    {
        string? result = handler.HandleSchemaEvent(data);
        return result is null ? Results.BadRequest() : Results.Text(result, Application.Json);
    }
);

await app.RunAsync("http://localhost:5003");
