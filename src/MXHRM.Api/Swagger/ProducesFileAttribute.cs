namespace MXHRM.Api.Swagger;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ProducesFileAttribute : Attribute
{
    public ProducesFileAttribute(string contentType)
    {
        ContentType = contentType;
    }

    public string ContentType { get; }
}