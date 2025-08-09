using Hangfire;
using Microsoft.EntityFrameworkCore;
using ProductManagement.API.Jobs;
using ProductManagement.Core.Interfaces;
using ProductManagement.EF.Data;
using ProductManagement.EF.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Jobs
builder.Services.AddScoped<ProductUpdateJob>();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

var app = builder.Build();

// Migration + Seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    DbInitializer.Seed(context);
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();

// Hangfire Dashboard
app.UseHangfireDashboard();

// Schedule Jobs
RecurringJob.AddOrUpdate<ProductUpdateJob>(
    job => job.RefreshProductsCache(),
    Cron.Hourly
);

app.MapControllers();
app.Run();
