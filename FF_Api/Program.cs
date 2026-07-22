using FF.Architecture.Parsers;
using FF.Architecture.Providers;
using FF_Api.Business;
using FF_DataDB;
using FF_Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
using FF_Business;
using FF_DataDB.Context;
using FF_Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
// Registrar DbContext
builder.Services.AddDbContext<FF_DbContext>();

// Registrar dependencias
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserBusiness, UserBusiness>();
builder.Services.AddScoped<ISourceItemRepository, SourceItemRepository>();
builder.Services.AddScoped<ISourceItemBusiness, SourceItemBusiness>();
builder.Services.AddScoped<ISourceRepository, SourceRepository>();
builder.Services.AddScoped<ISourceBusiness, SourceBusiness>();
builder.Services.AddScoped<IImportExportBusiness, ImportExportBusiness>();


// Configurar JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.Run();