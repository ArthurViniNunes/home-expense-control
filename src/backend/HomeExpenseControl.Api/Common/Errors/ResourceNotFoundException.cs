namespace HomeExpenseControl.Api.Common.Errors;

/// <summary>
/// Representa a tentativa de acessar um recurso inexistente.
/// </summary>
public sealed class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message)
        : base(message)
    {
    }
}