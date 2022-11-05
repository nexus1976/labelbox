using labelbox.Data;
using labelbox.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
builder.Services.AddHealthChecks().AddDbContextCheck<DataContext>();
builder.Services.AddDbContext<DataContext>();
builder.Services.AddScoped<IDataContext, DataContext>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddSingleton<WorkerService>();
builder.Services.AddSingleton<IExposedQueue, WorkerService>(provider => provider.GetRequiredService<WorkerService>());
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
