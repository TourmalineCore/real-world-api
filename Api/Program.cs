using System.Reflection;
using Api;
using Api.Exceptions;
using Api.Options;
using Application;
using Core;
using LoggingLibrary;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NodaTime;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TourmalineCore.AspNetCore.JwtAuthentication.Core;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Options;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// configure ElasticSearch credentials
builder.Services.Configure<ElasticSearchOptions>(configuration.GetSection("ElasticSearchOptions"));

var elasticSearchOptions = configuration
    .GetSection(nameof(ElasticSearchOptions))
    .Get<ElasticSearchOptions>();

var authenticationOptions = configuration
    .GetSection(nameof(AuthenticationOptions))
    .Get<AuthenticationOptions>();

builder.Services
    .AddJwtAuthentication(authenticationOptions)
    .WithUserClaimsProvider<UserClaimsProvider>(UserClaimsProvider.PermissionClaimType);

// add logging
builder.Services.AddScoped(_ => new LoggingAttribute("Api"));

ElkLogger.SetupLogger(
        elasticSearchOptions.ElasticSearchUrl,
        elasticSearchOptions.ElasticSearchLogin,
        elasticSearchOptions.ElasticSearchPassword
    );

builder.Services.AddControllers(opt =>
{
    opt.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
});

// add tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
            {
                builder
                    .AddSource("Logs.Startup")
                    .SetSampler(new AlwaysOnSampler())
                    .SetResourceBuilder(
                            ResourceBuilder
                                .CreateDefault()
                                .AddService("OpenTelemetry.RampUp.Api.*", serviceVersion: "0.0.1")
                        )
                    .AddAspNetCoreInstrumentation()
                    .AddJaegerExporter(o =>
                            {
                                o.AgentHost = Environment.GetEnvironmentVariable("JAEGER_HOST") ?? "localhost";
                                o.AgentPort = int.TryParse(Environment.GetEnvironmentVariable("JAEGER_PORT"),
                                    out var port) ? port : 6831;
                            }
                        )
                    .AddConsoleExporter();
            }
        );
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication(configuration);
builder.Services.AddTransient<IClock, Clock>();

builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "ToDo API",
                        Description = "An ASP.NET Core Web API for managing ToDo items",
                        Contact = new OpenApiContact
                        {
                            Name = "Website",
                            Url = new Uri("https://www.tourmalinecore.com/en"),
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT",
                            Url = new Uri("https://opensource.org/license/mit"),
                        },
                    }
                );

            c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please insert JWT with Bearer into field",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                    }
                );

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                            },
                            new string[] { }
                        },
                    }
                );
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        }
    );

var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors(
        corsPolicyBuilder => corsPolicyBuilder
            .AllowAnyHeader()
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyOrigin()
    );


app.UseStaticFiles();

app.UseSwagger();

app.UseSwaggerUI();

using (var serviceScope = app.Services.CreateScope())
{
    var context = serviceScope
        .ServiceProvider
        .GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseJwtAuthentication();
app.MapControllers();

app.Run();