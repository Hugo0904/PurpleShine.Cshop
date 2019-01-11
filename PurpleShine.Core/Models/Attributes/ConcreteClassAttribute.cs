using System;

namespace PurpleShine.Core.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ConcreteClassAttribute : Attribute
    {
        public ConcreteClassAttribute(Type concreteType)
        {
            ConcreteType = concreteType;
        }

        public Type ConcreteType { get; }
    }
}
