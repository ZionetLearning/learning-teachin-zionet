using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

// this class is used to add to Scalar UI the option to enter a Authentication Bearer token
internal sealed class BearerSecuritySchemeTransformer(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            document.Components ??= new OpenApiComponents();
            var securitySchemeId = "Bearer";
            document.Components.SecuritySchemes.Add(securitySchemeId, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token"
            });
            // Add "Bearer" scheme as a requirement for the API as a whole
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = securitySchemeId, Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
            });
        }
    }
}