using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace MXHRM.Api.Common;

// resource filter ทำงาน "ก่อน model binding" → เซ็ต MaxRequestBodySize จาก config ทันเวลา
public sealed class RequestSizeLimitFromConfigAttribute(string ruleName) : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var options = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<FileUploadOptions>>().Value;

        var rule = ruleName == "Document" ? options.Document : options.Photo;

        var feature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (feature is not null && !feature.IsReadOnly)
        {
            // เผื่อ overhead ของ multipart (boundary + fields อื่น) +1MB
            feature.MaxRequestBodySize = rule.MaxBytes + 1024 * 1024;
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}