using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FreelanceMarket.Api.Auth;
using FreelanceMarket.Api.Endpoints;
using FreelanceMarket.Application.Services;
using FreelanceMarket.Application.Services.Observers;
using FreelanceMarket.Application.Services.Strategies;
using FreelanceMarket.Application.Validators;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;
using FreelanceMarket.Infrastructure.Data;
using FreelanceMarket.Infrastructure.Adapters;
using FreelanceMarket.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── JSON: принимать enum как строки ("Customer", "Open" и т.д.) ───
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ─── Database (InMemory для демо) ───
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseInMemoryDatabase("FreelanceMarketDb"));

// ─── Repositories ───
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// ─── Services ───
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectCategoryService, ProjectCategoryService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<IExternalFreelancerProfileAdapter, GithubFreelancerProfileAdapter>();

// Pattern: Observer
builder.Services.AddSingleton(_ => SessionStateManager.Instance);
builder.Services.AddSingleton<IProjectStatusSubject, ProjectStatusPublisher>();
builder.Services.AddSingleton<AuditLogObserver>();
builder.Services.AddSingleton<IProjectObserver, NotificationObserver>();
builder.Services.AddSingleton<IProjectObserver, SessionSyncObserver>();
builder.Services.AddSingleton<IProjectObserver>(sp => sp.GetRequiredService<AuditLogObserver>());

// Pattern: Strategy
builder.Services.AddScoped<IProjectFilterStrategy, StatusFilterStrategy>();
builder.Services.AddScoped<IProjectFilterStrategy, BudgetRangeFilterStrategy>();
builder.Services.AddScoped<IProjectFilterStrategy, KeywordSearchStrategy>();
builder.Services.AddScoped<IProjectFilterStrategy, SkillsFilterStrategy>();
builder.Services.AddScoped<IProjectFilterStrategy, ProjectSortStrategy>();
builder.Services.AddScoped<ProjectFilterContext>();

// Pattern: Command
builder.Services.AddSingleton<ProposalCommandInvoker>();

// ─── FluentValidation ───
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ─── JWT Authentication ───
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// ─── CORS (для фронтенда Next.js на localhost:3000) ───
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ─── Swagger ───
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FreelanceMarket API",
        Version = "v1",
        Description = "API биржи фриланса с паттернами Singleton, Prototype, Builder, Factory Method"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
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

var publisher = app.Services.GetRequiredService<IProjectStatusSubject>();
foreach (var observer in app.Services.GetServices<IProjectObserver>())
{
    publisher.Subscribe(observer);
}

// ─── Middleware ───
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ─── Map Minimal API Endpoints ───
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapProjectEndpoints();
app.MapProposalEndpoints();
app.MapReviewEndpoints();

// Health check
app.MapGet("/", () => Results.Ok(new { status = "FreelanceMarket API is running" }));

app.Run();
