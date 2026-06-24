using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Interfaces.AI;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Settings;
using ProjectManagementSystem.Hosting;
using ProjectManagementSystem.Infrastructure.AI;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Mapping;
using ProjectManagementSystem.Infrastructure.Email;
using ProjectManagementSystem.Infrastructure.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Middleware;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>()!;

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Email:Smtp"));
var emailProvider = builder.Configuration.GetValue<string>("Email:Provider") ?? "Smtp";
if (emailProvider.Equals("Mock", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddScoped<IEmailService, MockEmailService>();
else
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddHttpClient(HttpClientNames.Gemini, client =>
{
    client.BaseAddress = new Uri(ExternalApiDefaults.GeminiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(ExternalApiDefaults.HttpTimeoutSeconds);
});
builder.Services.AddHttpClient(HttpClientNames.Groq, client =>
{
    client.BaseAddress = new Uri(ExternalApiDefaults.GroqBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(ExternalApiDefaults.HttpTimeoutSeconds);
});
builder.Services.AddHttpClient(HttpClientNames.Ollama, client =>
{
    client.BaseAddress = new Uri(ExternalApiDefaults.OllamaBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(ExternalApiDefaults.OllamaHttpTimeoutSeconds);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    ConnectTimeout = TimeSpan.FromSeconds(ExternalApiDefaults.OllamaConnectTimeoutSeconds),
    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
    UseProxy = false
});
builder.Services.AddSingleton<IAiProviderFactory, AiProviderFactory>();


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IAllocationRepository, AllocationRepository>();
builder.Services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<ITimesheetReminderRepository, TimesheetReminderRepository>();

builder.Services.AddApplicationServices();
builder.Services.AddHostedService<SchedulerHostedService>();

builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PRM Tool API", Version = "v1" });

    var secScheme = new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token (without Bearer prefix)"
    };
    c.AddSecurityDefinition("Bearer", secScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseExceptionHandling();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
