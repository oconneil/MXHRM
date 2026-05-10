namespace MXHRM.Application.Common;

public enum OperationErrorType
{
    None,
    NotFound,
    Validation
}

public class OperationResult
{
    public bool Succeeded { get; init; }
    public OperationErrorType ErrorType { get; init; } = OperationErrorType.None;
    public string? ErrorMessage { get; init; }
    public object? Details { get; init; }

    public static OperationResult Success() => new() { Succeeded = true };

    public static OperationResult Failure(
        OperationErrorType errorType,
        string errorMessage,
        object? details = null) => new()
        {
            Succeeded = false,
            ErrorType = errorType,
            ErrorMessage = errorMessage,
            Details = details
        };
}

public sealed class OperationResult<T> : OperationResult
{
    public T? Value { get; init; }

    public static OperationResult<T> Success(T value) => new()
    {
        Succeeded = true,
        Value = value
    };

    public static new OperationResult<T> Failure(
        OperationErrorType errorType,
        string errorMessage,
        object? details = null) => new()
        {
            Succeeded = false,
            ErrorType = errorType,
            ErrorMessage = errorMessage,
            Details = details
        };
}
