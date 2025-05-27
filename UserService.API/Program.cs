using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.Business.Services.Interfaces;
using UserService.Data.Repositories.Interfaces;
using UserService.Data.Repositories;
using Microsoft.OpenApi.Models;

using UserService.API.Middleware;
using UserService.Business.Services;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


// Add services to the container.
// ?? Add Azure Key Vault with your vault name
//builder.Configuration.AddAzureKeyVault(
//    new Uri("https://mydemokeyvault1234.vault.azure.net/"),
//    new DefaultAzureCredential(includeInteractiveCredentials: true));

//var config = builder.Configuration;
//var connectionString = config["DefaultConnection"];

//string keyVaultUrl = "https://mydemokeyvault1234.vault.azure.net/";
//var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential(includeInteractiveCredentials: true));
//string secretName = "DefaultConnection";
//KeyVaultSecret secret = await client.GetSecretAsync(secretName);
//Console.WriteLine($"Secret: {secret.Value}");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Accept PascalCase from Angular
    });


// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserActionQueueService, UserActionQueueService>();
builder.Services.AddScoped<IUserService, UserService.Business.Services.UserService>();

builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]);


// Initialize Azure Queue on startup
var serviceProvider = builder.Services.BuildServiceProvider();
var queueService = serviceProvider.GetRequiredService<IUserActionQueueService>();
await queueService.EnsureQueueExistsAsync();



// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        // Add this to let exceptions flow to your middleware
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Let the exception propagate to your middleware
                if (context.Exception is SecurityTokenException)
                {
                    throw context.Exception;
                }
                return Task.CompletedTask;
            }
        };



    });





builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Profile API", Version = "v1" });

    // Configure Swagger to use JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});




var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);




app.UseAuthentication(); // This line must come before UseAuthorization

// Add JWT exception handling
app.UseMiddleware<JwtExceptionMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();



app.UseAuthorization();


app.MapControllers();

app.Run();
