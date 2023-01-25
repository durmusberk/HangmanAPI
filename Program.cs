global using Hangman.Services.UserService;
using System.Text;
using FluentValidation;
using Hangman.BusinessLogics;
using Hangman.Data;
using Hangman.Extensions;
using Hangman.Middlewares;
using Hangman.Models.RequestModels;
using Hangman.Profiles;
using Hangman.Services.AuthManager;
using Hangman.Services.SessionService;
using Hangman.Services.WordService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
}, ServiceLifetime.Singleton);

//Configure Logger
Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("log/HangmanLogs.txt", rollingInterval: RollingInterval.Hour).CreateLogger();

builder.Host.UseSerilog();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//Servives
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IAuthManager, AuthManager>();
//BusinessLogic
builder.Services.AddScoped<IGuessBusinessLogic,GuessBusinessLogic>();
//Fluent Validation
builder.Services.AddValidatorsFromAssemblyContaining<GuessRequestValidator>();
//Swagger
builder.Services.AddSwaggerGen();
//AutoMapper
builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    }
    );


var app = builder.Build();

//app.ConfigureExceptionHandler();
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
