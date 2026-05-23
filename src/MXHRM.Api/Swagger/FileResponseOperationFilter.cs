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

        operation.Responses ??= new OpenApiResponses();

        var content = producesFileAttribute.ContentTypes
            .Where(contentType => !string.IsNullOrWhiteSpace(contentType))
            .ToDictionary(
                contentType => contentType,
                _ => new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "binary"
                    }
                });

        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "File",
            Content = content
        };
    }
}
