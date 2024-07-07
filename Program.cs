var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "flwn",
        policy =>
        {
            policy.WithOrigins
            ("https://api.flwn.dev",
                "https://flwn.dev",
                "https://m.flwn.dev",
                "https://files.flwn.dev");
        });
});

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();

app.MapControllers();

app.Run();