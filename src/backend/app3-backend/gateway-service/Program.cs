using GatewayService.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using Shared.Configuration.Dll;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(builder => builder.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

builder.Services.Configure<DllHandlerConfig>(builder.Configuration.GetSection(DllHandlerConfig.ConfigSection)); // This is the new DllHandlerConfig class from dll based implementation
builder.Services.AddScoped<IDataHandler, DllDataHandler>(); // DLL based implementation

// Add mongo and mass transit services used by the handler
builder.Services.AddMongoDB();
builder.Services.AddRabbitMq(builder.Configuration.GetSection(DllHandlerConfig.ConfigSection).Get<DllHandlerConfig>()!); // This overload of AddRabbitMq takes in the DllHandlerConfig object and loads any consumers from the DLLs

builder.Services.Configure<RouteOptions>(opts => opts.LowercaseUrls = true);

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

app.MapControllers();

await app.RunAsync("http://localhost:8081");
