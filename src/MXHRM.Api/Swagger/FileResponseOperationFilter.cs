using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MXHRM.Api.Swagger;

public sealed class FileResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var producesFileAttribute = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<ProducesFileAttribute>()
            .FirstOrDefault();

        if (producesFileAttribute is null)
        {
            return;
        }

        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "File",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                [producesFileAttribute.ContentType] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "binary"
                    }
                }
            }
        };
    }
}
