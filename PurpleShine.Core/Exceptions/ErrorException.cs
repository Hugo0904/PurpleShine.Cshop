using System;

namespace PurpleShine.Core.Exceptions
{
    [Serializable]
    public class ErrorException : Exception
    {
        public ErrorException(string message)
        : base(message)
        {
            //
        }

        public static implicit operator string(ErrorException e)
        {
            return e.Message;
        }
    }
}
