using System;

namespace DB.DependencyInjection.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
        public readonly string GameObjectName;
        public readonly string ComponentName;
        public readonly Type ComponentType;
        
        public InjectAttribute(string gameObjectName = null, string componentName = null, Type componentType = null)
        {
            GameObjectName = gameObjectName;
            ComponentName = componentName;
            ComponentType = componentType;
        }
    }
}