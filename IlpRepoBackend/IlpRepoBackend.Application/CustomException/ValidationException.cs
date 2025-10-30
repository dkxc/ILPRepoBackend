namespace IlpRepoBackend.Application.CustomException
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public ValidationException(IEnumerable<string> errors)
            : base($"Validation failed: {string.Join(", ", errors)}")
        {
        }
    }
}