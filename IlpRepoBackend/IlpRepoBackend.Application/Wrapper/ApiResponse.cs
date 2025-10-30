using IlpRepoBackend.Application.Constents;
using IlpRepoBackend.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Wrapper
{
    public class ApiResponse<T>
    {
        private TraineeDto res;
        private string v;

        public int Status { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }

        public ApiResponse(int statusCode, T data, string message)
        {
            Status = statusCode;
            Data = data;
            Message = message;
        }

        public ApiResponse(TraineeDto res, string v)
        {
            this.res = res;
            this.v = v;
        }

        // Helper methods
        public static ApiResponse<T> Success(T data, string message = "Request processed successfully")
        {
            return new ApiResponse<T>(StatusCode.OK, data, message);
        }

        public static ApiResponse<T> Created(T data, string message = "Resource created successfully")
        {
            return new ApiResponse<T>(StatusCode.Created, data, message);
        }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>(StatusCode.BadRequest, default, message);
        }

    }
}
