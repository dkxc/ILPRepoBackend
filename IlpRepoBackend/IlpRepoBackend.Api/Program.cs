<<<<<<< HEAD
using IlpRepoBackend.Application;
using IlpRepoBackend.Infrastructure;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
=======
using IlpRepoBackend.Api.Middleware;
using IlpRepoBackend.Application;
using IlpRepoBackend.Infrastructure;
using Microsoft.OpenApi.Models;
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  // your frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // optional, only if you're using cookies or auth headers
    });
});
// Add services to the container
builder.Services.AddControllers();
<<<<<<< HEAD

// Register Application Services (MediatR, AutoMapper)
builder.Services.AddApplicationServices();

// Register Persistence Services (Repositories and DbContext)
builder.Services.AddPersistenceServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
=======
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Application and Infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);

var app = builder.Build();
//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

<<<<<<< HEAD
// Enable serving static files (for uploaded documents)
app.UseStaticFiles();

=======
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowFrontend");
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();