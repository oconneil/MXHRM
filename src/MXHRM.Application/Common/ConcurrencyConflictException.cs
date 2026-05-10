namespace MXHRM.Application.Common;

public class ConcurrencyConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);
