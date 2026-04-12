using System.Reflection;
using System.Text.Json.Serialization;
using api.Broker;
using api.Data;
using api.Exceptions;
using api.Features;
using api.Features.BankAccount;
using api.Features.Currency;
using api.Features.Idempotency;
using api.Features.Metrics;
using api.Features.Transfers;
using api.Retries;
using api.Services;
using api.Websocket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Racoon Bank Core Api", Version = "v1" });
    options.EnableAnnotations();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    options.OperationFilter<IdempotencyFilter>();
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
        )
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken) &&
                context.HttpContext.Request.Path.StartsWithSegments("/ws"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddSingleton<LogBuffer>();
builder.Services.AddHostedService<LogSenderService>();
builder.Services.AddHostedService<OperationConsumer>();
builder.Services.AddHostedService<TransferConsumer>();

builder.Services.AddHttpClient("monitoring", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MetricsService:HostName"]);
}).AddPolicyHandler(HttpPolicies.GetRetryPolicy())
.AddPolicyHandler(HttpPolicies.GetCircuitBreakerPolicy());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case BankAccountNotFoundException:
                context.Response.StatusCode = 404;
                break;

            case InsufficientFundsException:
            case FailedToFetchExchangeRateException:
                context.Response.StatusCode = 400;
                break;

            default:
                context.Response.StatusCode = 500;
                break;
        }

        await context.Response.WriteAsJsonAsync(new { error = exception?.Message });
    });
});
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<IdempotencyMiddleware>();
app.UseMiddleware<RandomFailureMiddleware>();
app.UseMiddleware<MetricsMiddleware>();

app.MapHub<BankHub>("/ws");

app.MapControllers();
app.Run();
