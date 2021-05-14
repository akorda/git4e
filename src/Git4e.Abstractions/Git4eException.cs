using System;

namespace Git4e
{
    public class Git4eException : Exception
    {
        public Git4eException()
        {
        }

        public Git4eException(string message)
            : base(message)
        {
        }

        public Git4eException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
