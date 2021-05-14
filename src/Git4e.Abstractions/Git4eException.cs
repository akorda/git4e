using System;

namespace Git4e
{
    public class Git4eException : Exception
    {
        public Git4eErrorCode ErrorCode { get; set; }

        public Git4eException(Git4eErrorCode errorCode)
        {
            this.ErrorCode = errorCode;
        }

        public Git4eException(Git4eErrorCode errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public Git4eException(Git4eErrorCode errorCode, string message, Exception inner)
            : base(message, inner)
        {
            this.ErrorCode = errorCode;
        }
    }
}
