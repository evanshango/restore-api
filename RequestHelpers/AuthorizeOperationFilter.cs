using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.RequestHelpers;

public class AuthorizeOperationFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
        var attributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();

        var authorizeAttributes = (attributes ?? Array.Empty<AuthorizeAttribute>()).ToList();
        if (!authorizeAttributes.Any()) return;

        var attr = authorizeAttributes.ToList()[0];
        // Add response types on secure APIs
        operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

        // Add what should be shown inside the security section
        IList<string> securityInfos = new List<string>();
        securityInfos.Add($"{nameof(AuthorizeAttribute.Policy)}:{attr.Policy}");
        securityInfos.Add($"{nameof(AuthorizeAttribute.Roles)}:{attr.Roles}");
        securityInfos.Add($"{nameof(AuthorizeAttribute.AuthenticationSchemes)}:{attr.AuthenticationSchemes}");

        operation.Security = new List<OpenApiSecurityRequirement> {
            new() {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Id = "bearer", // Must fit the defined Id of SecurityDefinition in global config
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    securityInfos
                }
            }
        };
    }
}