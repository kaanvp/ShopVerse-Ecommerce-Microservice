using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Core
{
    /// <summary>
    /// İşlem sonucunu temsil eden generic result sınıfıdır.
    /// Başarı veya hata durumunu, HTTP durum kodunu, hata mesajını ve dönen veriyi
    /// kapsülleyerek standart bir response modeli sağlar.
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public int StatusCode { get; }
        public T? Data { get; }

        private Result(bool isSuccess, string? error, int statusCode, T? data)
        {
            IsSuccess = isSuccess;
            Error = error;
            StatusCode = statusCode;
            Data = data;
        }

        public static Result<T> Success(T data, int statusCode = 200) =>
            new(true, null, statusCode, data);

        public static Result<T> Failure(string error, int statusCode = 400, CancellationToken cancellationToken = default) =>
            new(false, error, statusCode, default);
    }
}
