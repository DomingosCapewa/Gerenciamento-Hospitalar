using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Contracts.Security;
using Swashbuckle.AspNetCore.Annotations;

namespace Shared.Contracts.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeycloakJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.Audience = configuration["Authentication:Audience"];
                options.RequireHttpsMetadata = configuration.GetValue<bool>("Authentication:RequireHttpsMetadata");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    RoleClaimType = "roles",
                    NameClaimType = "preferred_username"
                };
            });

        services.AddSingleton<IClaimsTransformation, KeycloakRoleClaimsTransformer>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Usuario", p => p.RequireRole("Usuario", "usuario", "user"));
            options.AddPolicy("Medico", p => p.RequireRole("Medico", "medico", "doctor"));
            options.AddPolicy("Admin", p => p.RequireRole("Admin", "admin"));
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services, string title, string version = "v1", string? description = null)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(config =>
        {
            config.EnableAnnotations();
            config.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = description ?? string.Empty,
            });
            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando Bearer. Ex: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        return services;
    }
}
