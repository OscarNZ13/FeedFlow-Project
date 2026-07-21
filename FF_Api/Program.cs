using FF.Architecture.Parsers;
using FF.Architecture.Providers;
using FF_Api.Business;
using FF_DataDB;
using FF_Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DB ---
builder.Services.AddDbContext<FeedFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Repositories ---
builder.Services.AddScoped<ISourceRepository, SourceRepository>();
builder.Services.AddScoped<ISourceItemRepository, SourceItemRepository>();
builder.Services.AddScoped<ISourceSecretRepository, SourceSecretRepository>();

// --- Feed fetching / parsing (Architecture layer) ---
builder.Services.AddHttpClient<IFeedFetcher, FeedFetcher>();
builder.Services.AddSingleton<IFeedParserFactory, FeedParserFactory>();

// --- Business ---
builder.Services.AddScoped<IFeedBusiness, FeedBusiness>();

// CORS abierto para que FF_Mvc (y la demo) puedan llamar a la Api sin fricción.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
