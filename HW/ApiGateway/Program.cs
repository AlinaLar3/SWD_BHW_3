var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDevServer",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("http://localhost:5001/swagger/v1/swagger.json", "Order Service");
    options.SwaggerEndpoint("http://localhost:5002/swagger/v1/swagger.json", "Payment Service");
});

app.MapReverseProxy();
app.Run();
