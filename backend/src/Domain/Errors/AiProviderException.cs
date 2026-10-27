namespace ClosetAPI.Domain.Errors;

public class AiProviderException(string message, Exception? innerException = null)
    : Exception(message, innerException);
