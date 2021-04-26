using System;

namespace Git4e
{
    /// <summary>
    /// The base class of all git4e exceptions. Contains an error code of
    /// the error that caused this exception.
    /// </summary>
    public class Git4eException : Exception
    {
        /// <summary>
        /// The git4e error code of the error that caused this exception.
        /// </summary>
        public Git4eErrorCode ErrorCode { get; set; }

        /// <summary>
        /// Initializes a new instance of a git4e exception.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public Git4eException(Git4eErrorCode errorCode)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of a git4e exception.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message that describes the error.</param>
        public Git4eException(Git4eErrorCode errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of a git4e exception.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a <c>null</c> reference (<c>Nothing</c> in Visual Basic) if no inner exception is specified.</param>
        public Git4eException(Git4eErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }
    }
}
