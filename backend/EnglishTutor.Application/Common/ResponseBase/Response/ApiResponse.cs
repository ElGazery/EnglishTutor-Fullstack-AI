namespace EnglishTutor.Application.Common.ResponseBase.Response
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; } = new List<string>();

        // Success constructor
        public ApiResponse(T data, string msg, int statusCode)
        {
            Data = data;
            Message = msg;
            StatusCode = statusCode;
            IsSuccess = true;
        }

        // Failure constructor
        public ApiResponse(string msg, int statusCode, List<string>? errors = null)
        {
            Errors = errors;
            Message = msg;
            StatusCode = statusCode;
            IsSuccess = false;
        }
    }
}
