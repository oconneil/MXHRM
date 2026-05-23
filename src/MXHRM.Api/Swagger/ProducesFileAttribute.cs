namespace MXHRM.Api.Swagger;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ProducesFileAttribute : Attribute
{
    public ProducesFileAttribute(params string[] contentTypes)
    {
        ContentTypes = contentTypes;
    }

    public IReadOnlyList<string> ContentTypes { get; }
}
