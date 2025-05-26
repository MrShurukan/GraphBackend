using System.Text;
using GraphBackend;
using GraphBackend.Application;
using GraphBackend.Application.CQRS.Commands;
using GraphBackend.Domain.Common;
using GraphBackend.Domain.Models;
using GraphBackend.Extensions;
using GraphBackend.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = long.MaxValue; // In case of multipart
});
const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
                #if DEBUG
                "http://localhost:5173",
                #endif
                "https://graph.mrshurukan.ru"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Configuration.CheckIfSectionExists("ConnectionStrings.DefaultConnection");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<ApplicationContext>(o =>
{
    o.UseNpgsql(connectionString, b =>
        {
            b.MigrationsAssembly("GraphBackend.Infrastructure");
        })
        .UseSnakeCaseNamingConvention()
#if DEBUG
        .EnableSensitiveDataLogging(true);
#else
        .EnableSensitiveDataLogging(false);
#endif
});

builder.Services.AddScoped<IApplicationContext, ApplicationContext>();

builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssemblyContaining<LoginCommand>();
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var settings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("UserOnly", policy => policy.RequireRole("User"));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<ApplicationContext>()!;
    var pendingMigrations = context.Database.GetPendingMigrations();
    if (pendingMigrations.Any())
    {
        ConsoleWriter.WriteInfoLn("Принимаю миграции...");
        context.Database.Migrate();
        ConsoleWriter.WriteSuccessLn("Миграции приняты успешно");
    }
    else
    {
        ConsoleWriter.WriteInfoLn("Нет новых миграций");
    }
    
    if (await context.Users.AllAsync(x => x.Role != Roles.Admin))
    {
        ConsoleWriter.WriteWarningLn("Нет ни единого администратора, создаю тестового");
        
        var password = BCrypt.Net.BCrypt.HashPassword("admin");
        var user = new User
        {
            Email = "test@admin.com",
            PasswordHash = password,
            Role = Roles.Admin
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.MapGet("/", async context =>
    {
        await Task.Run(() => context.Response.Redirect("./swagger/index.html", permanent: false));
    });

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "GraphBackend.Presentation");
    });
}

app.UseCors(myAllowSpecificOrigins);

app.UseExceptionHandler(a => a.Run(ControllerExceptionHandler.Handle));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
            }
        }
    }
}