using System;

namespace DB.DependencyInjection.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InjectableAttribute : Attribute
    {
        public readonly bool Singleton;
        
        public InjectableAttribute(bool singleton = true)
        {
            Singleton = singleton;
        }
    }
}