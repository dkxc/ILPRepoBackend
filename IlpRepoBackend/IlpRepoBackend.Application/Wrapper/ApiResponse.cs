using IlpRepoBackend.Application.Constents;
using IlpRepoBackend.Application.Dto;
using System;

namespace IlpRepoBackend.Application.Wrapper
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }

        public ApiResponse(int statusCode, T data, string message, bool succeeded = true)
        {
            Status = statusCode;
            Data = data;
            Message = message;

            Succeeded = succeeded;

            Succeeded = statusCode >= 200 && statusCode < 300; // Set Succeeded based on status code
        }

        public ApiResponse(TraineeDto res, string v)
        {
            //this.res = res;
            //this.v = v;

        }

        // Helper methods
        public static ApiResponse<T> Success(T data, string message = "Request processed successfully")
        {
            return new ApiResponse<T>(StatusCode.OK, data, message, succeeded: true);
        }

        public static ApiResponse<T> Created(T data, string message = "Resource created successfully")
        {
            return new ApiResponse<T>(StatusCode.Created, data, message, succeeded: true);
        }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>(StatusCode.BadRequest, default, message, succeeded: false);
        }
    }
}
