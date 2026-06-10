namespace ProjectManagementSystem.Core.Exceptions;

public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string userMessage)
        : base(userMessage, AppErrorKind.BadRequest)
    {
    }
}
