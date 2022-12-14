using labelbox.Data;
using labelbox.Services;
using System.IO.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
builder.Services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase(databaseName: "LabelboxDb"));
builder.Services.AddHealthChecks().AddDbContextCheck<DataContext>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IDataContext, DataContext>();
builder.Services.AddScoped<IFileSystem, FileSystem>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddSingleton<IExposedQueue, ExposedQueue>();
builder.Services.AddScoped<IQueueProcessor, QueueProcessor>();
builder.Services.AddSingleton<WorkerService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<WorkerService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("corsapp");
app.UseAuthorization();
app.UseHealthChecks("/healthz");
app.MapControllers();
app.Run();
