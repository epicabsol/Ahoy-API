using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AhoyAPI
{
    public static class ControllerExtensions
    {
        public static IActionResult CreateSuccess<T>(this Controller controller, T data, int statusCode = StatusCodes.Status200OK)
        {
            return new ObjectResult(new APISuccessResult<T>(data)) { StatusCode = statusCode };
        }

        public static IActionResult CreateFailure(this Controller controller, string message, int statusCode = StatusCodes.Status400BadRequest)
        {
            return new ObjectResult(new APIFailureResult(message)) { StatusCode = statusCode };
        }
    }
}
