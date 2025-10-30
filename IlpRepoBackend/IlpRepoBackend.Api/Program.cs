using IlpRepoBackend.Application;
using IlpRepoBackend.Infrastructure;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register Application Services (MediatR, AutoMapper)
builder.Services.AddApplicationServices();

// Register Persistence Services (Repositories and DbContext)
builder.Services.AddPersistenceServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable serving static files (for uploaded documents)
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
