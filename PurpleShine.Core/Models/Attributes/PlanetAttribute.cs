using System;

namespace PurpleShine.Core.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PlanetAttribute : Attribute
    {
        public PlanetAttribute(params object[] value)
        {
            this.value = value;
        }
        public object[] value { get; private set; }
    }
}