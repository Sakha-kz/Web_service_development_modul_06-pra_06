using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавление контроллеров
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Настройка JWT-аутентификации согласно Заданию 3
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["Key"] ?? "SuperSecretKeyForJwtAuthenticationModule06Pra06_MustBeLongEnough!";
var issuer = jwtSection["Issuer"] ?? "ProductSecurityApi";
var audience = jwtSection["Audience"] ?? "ProductSecurityClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Проверка срока действия без дополнительной временной задержки
    };
});

builder.Services.AddAuthorization();

// 3. Настройка Swagger UI с поддержкой API-Key и JWT Bearer токенов
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProductSecurityApi - Module 06 Practical",
        Version = "v1",
        Description = "ASP.NET Core Web API: Защита доступа к API с помощью API Key (X-Api-Key) и JWT Bearer токенов."
    });

    // Определение безопасности для X-Api-Key
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Заголовок API-ключа. Введите значение 'SecretKey123' в заголовок X-Api-Key.",
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Api-Key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    // Определение безопасности для JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Авторизация по JWT токену. Введите токен, полученный в /api/auth/login, в формате: Bearer {токен}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Настройка конвейера обработки HTTP-запросов
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductSecurityApi v1");
        c.RoutePrefix = "swagger";
    });
}

// Задание 3: Обязательный порядок подключения middleware аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
