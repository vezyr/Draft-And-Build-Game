using System;
using System.Collections.Generic;
using UnityEngine;

namespace DB.DependencyInjection
{
    public class Container
    {
        private readonly Dictionary<string, InjectableEntry> _entries = new Dictionary<string, InjectableEntry>();
        
        public bool Has(string componentName, string gameObjectName)
        {
            string key = $"{componentName}.{gameObjectName}";
            return _entries.ContainsKey(key) &&
                   _entries.GetValueOrDefault(key, null) != null;
        }

        public bool HasSingleton(Type componentType)
        {
            foreach (var entry in _entries.Values)
            {
                if (entry.Component.GetType() == componentType)
                    return true;
            }
            return false;
        }

        public void Bind(string componentName, string gameObjectName, Component component)
        {
            if (Has(componentName, gameObjectName))
            {
                Debug.LogWarning("Component already registered in scene: " + componentName + " on " + gameObjectName);
                return;
            }
            _entries.Add(
                $"{componentName}.{gameObjectName}", 
                new InjectableEntry(componentName, gameObjectName, component.gameObject.GetInstanceID(), component)
            );
        }
    }
}