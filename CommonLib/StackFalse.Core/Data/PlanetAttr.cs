using System;

namespace StackFalse.Core.Data
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PlanetAttr : Attribute
    {
        public PlanetAttr(params object[] value)
        {
            this.value = value;
        }
        public object[] value { get; private set; }
    }
}