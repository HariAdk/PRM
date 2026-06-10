namespace ProjectManagementSystem.Core.Exceptions;

/// <summary>Business rule or state violation the user can understand and correct.</summary>
public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string userMessage)
        : base(userMessage, AppErrorKind.BadRequest)
    {
    }
}
