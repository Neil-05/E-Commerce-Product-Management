using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Load ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 🔥 Add Ocelot
builder.Services.AddOcelot();

var app = builder.Build();

// 🔥 Use Ocelot
await app.UseOcelot();

app.Run();