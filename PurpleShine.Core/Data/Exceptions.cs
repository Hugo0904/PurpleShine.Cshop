using System;

namespace PurpleShine.Core.Data
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
        //    return reason.IsNonNull() ? new ErrorInfoException(reason) : null;
        //}
    }
}
