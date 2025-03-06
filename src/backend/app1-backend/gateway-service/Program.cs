using GatewayService.Configuration.Dll;
using GatewayService.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(builder => builder.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

// Uncomment the following lines to switch between service based and dll based implementation
// In service based implementation, the ServiceHandlerConfig class is used to configure the service handler thru appsettings.json
// This implementation requires each downstream service (base-service, decorator-service-1, decorator-service-2, etc.) to be running
// The ServiceHandlerConfig must point to each individual running service in the appsettings.json ServiceHandlerConfig section
// builder.Services.Configure<ServiceHandlerConfig>(builder.Configuration.GetSection(ServiceHandlerConfig.ConfigSection)); // This is the old ServiceHandlerConfig class from service based implementation
// builder.Services.AddScoped<IDataHandler, ServiceDataHandler>(); // Service based implementation

// In dll based implementation, the DllHandlerConfig class is used to configure the dll handler thru appsettings.json
// This implementation does not require each downstream service (base-service, decorator-service-1, decorator-service-2, etc.) to be running
// however, it requires the downstream services to be built and the dlls to be pointed to in the appsettings.json DllHandlerConfig section
builder.Services.Configure<DllHandlerConfig>(builder.Configuration.GetSection(DllHandlerConfig.ConfigSection)); // This is the new DllHandlerConfig class from dll based implementation
builder.Services.AddScoped<IDataHandler, DllDataHandler>(); // DLL based implementation

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

await app.RunAsync("http://localhost:8080");
