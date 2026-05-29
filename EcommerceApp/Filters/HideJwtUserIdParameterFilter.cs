using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EcommerceApp.Filters
{
    public class HideJwtUserIdParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Find and remove parameters that are using the JwtUserIdBinder
            var parametersToRemove = context.ApiDescription.ParameterDescriptions
                .Where(p => p.ModelMetadata?.BinderType == typeof(JwtUserIdBinder))
                .Select(p => p.Name)
                .ToList();

            foreach (var paramName in parametersToRemove)
            {
                var swaggerParam = operation.Parameters.FirstOrDefault(p => p.Name == paramName);
                if (swaggerParam != null)
                {
                    operation.Parameters.Remove(swaggerParam);
                }
            }
        }
    }

}
