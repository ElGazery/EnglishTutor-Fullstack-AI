namespace EnglishTutor.Domain.Exceptions
{
    /// <summary>
    /// Thrown when a user tries to access a resource that belongs to another user (403 Forbidden).
    /// </summary>
    public class ForbiddenCustomException(string msg) : Exception(msg)
    {
    }
}
