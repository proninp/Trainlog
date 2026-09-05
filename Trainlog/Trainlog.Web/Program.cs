var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

builder.Services.AddProblemDetails();
app.UseExceptionHandler();

app.Run();