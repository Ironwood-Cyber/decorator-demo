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

// builder.Services.Configure<ServiceHandlerConfig>(builder.Configuration.GetSection(ServiceHandlerConfig.ConfigSection)); // This is the old ServiceHandlerConfig class from service based implementation
// builder.Services.AddScoped<IDataHandler, ServiceDataHandler>(); // Service based implementation

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

await app.RunAsync("http://localhost:8081");
