using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common;

public static class AuthExtensions
{
    private static readonly string[] ValidIssuers =
    [
        "http://localhost:6001/realms/devchannel",
        "http://keycloak:8080/realms/devchannel",
        "http://id.devchannel.local/realms/devchannel",
    ];

    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(
                serviceName: "keycloak",
                realm: "devchannel",
                options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = "devchannel";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        IssuerValidator = ValidateIssuer,
                        ClockSkew = TimeSpan.Zero,
                    };
                });

        services.AddAuthorizationBuilder();
        
        return services;
    }

    private static string ValidateIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
    {
        var normalizedIssuer = issuer.TrimEnd('/');
        
        foreach (var validIssuer in ValidIssuers)
        {
            if (string.Equals(normalizedIssuer, validIssuer.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                return issuer;
            }
        }

        throw new SecurityTokenInvalidIssuerException($"IDX10205: Issuer validation failed. Issuer: '{issuer}'. Did not match any valid issuers.");
    }
}
