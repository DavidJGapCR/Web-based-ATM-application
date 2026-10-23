using Atm.Api.Endpoints;
using Atm.Api.Errors;
using Atm.Application;
using Atm.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Atm")
    ?? throw new InvalidOperationException("Connection string 'Atm' is not configured.");

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

// Surface request-binding failures (bad JSON, wrong types) to the exception handler
// in every environment so they become consistent ProblemDetails responses.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapAccountEndpoints();
app.MapTransferEndpoints();

app.Run();

public partial class Program;
