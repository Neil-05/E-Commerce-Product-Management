using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Serilog;
using Serilog.Events;

// 🔥 CREATE LOG FOLDER
Directory.CreateDirectory("Logs");

// 🔥 CONFIGURE SERILOG
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)

    .Enrich.FromLogContext()

    .WriteTo.Console(outputTemplate:
        "{Timestamp:HH:mm:ss} [{Level:u3}] [Gateway] [CID:{CorrelationId}] {Message:lj}{NewLine}{Exception}")

    .WriteTo.File("Logs/gateway-log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [Gateway] [CID:{CorrelationId}] {Message:lj}{NewLine}{Exception}")

    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// 🔥 ATTACH SERILOG
builder.Host.UseSerilog();

// load configs
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: true);

// add services
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// 🔥 CORRELATION ID MIDDLEWARE
app.Use(async (context, next) =>
{
    var headerKey = "X-Correlation-ID";
    var cid = context.Request.Headers[headerKey].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(cid))
        cid = Guid.NewGuid().ToString();

    context.Response.Headers[headerKey] = cid;

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", cid))
    {
        await next();
    }
});

// 🔥 REQUEST / RESPONSE LOGGING
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Gateway");

    logger.LogInformation("[REQUEST] {method} {path}",
        context.Request.Method,
        context.Request.Path);

    await next();

    logger.LogInformation("[RESPONSE] {status}",
        context.Response.StatusCode);
});

// swagger UI
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// ocelot
await app.UseOcelot();

app.Run();