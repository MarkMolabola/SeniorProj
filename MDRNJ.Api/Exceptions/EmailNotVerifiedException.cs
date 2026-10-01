using System;

namespace MDRNJ.Api.Exceptions
{
    // Thrown when a user with a correct password hasn't verified their email yet.
    // ExceptionMiddleware maps this to 403 so the app can show the code screen.
    public class EmailNotVerifiedException : Exception
    {
        public EmailNotVerifiedException(string message) : base(message) { }
    }
}
