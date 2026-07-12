namespace HomeExpenseControl.Api.Common.Errors;

/// <summary>
/// Representa uma tentativa de executar uma operação proibida
/// pelas regras de negócio da aplicação.
/// </summary>
public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}