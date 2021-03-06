﻿using System;

namespace PurpleShine.Core.Delegates.Args
{
    public class ValueArgs<T> : EventArgs
    {
        public T Value { get; private set; }

        public ValueArgs(T t)
        {
            Value = t;
        }

        public static implicit operator ValueArgs<T>(T value)
        {
            return new ValueArgs<T>(value);
        }

        public static implicit operator T(ValueArgs<T> v)
        {
            return v.Value;
        }
    }
}
