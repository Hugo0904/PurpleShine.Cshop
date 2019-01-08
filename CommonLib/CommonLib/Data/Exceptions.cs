using System;

namespace CommonLib.Data
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

        //public static implicit operator ErrorException(string reason)
        //{
        //    return reason != null ? new ErrorInfoException(reason) : null;
        //}
    }
}
