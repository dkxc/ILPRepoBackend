namespace IlpRepoBackend.Application.Wrappers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string message = "Success")
        {
            Success = true;
            Message = message;
            Data = data;
            StatusCode = 200;
        }

        public ApiResponse(string message, int statusCode = 400)
        {
            Success = false;
            Message = message;
            StatusCode = statusCode;
        }
    }
}
