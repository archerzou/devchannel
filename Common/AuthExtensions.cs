using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common;

public static class AuthExtensions
{
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
                        ValidIssuers = [
                            "http://localhost:6001/realms/devchannel",
                            "http://keycloak:8080/realms/devchannel",
                            "http://id.devchannel.local/realms/devchannel",
                        ],
                        ClockSkew = TimeSpan.Zero,
                    };
                });

        services.AddAuthorizationBuilder();
        
        return services;
    }
}
