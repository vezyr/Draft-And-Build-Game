using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DB.DependencyInjection.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DB.DependencyInjection
{
    public class SceneContext : MonoBehaviour
    {
        private readonly Container _container = new Container();
        private void OnEnable()
        {
            var injectables = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type.IsDefined(typeof(InjectableAttribute), false) && typeof(Component).IsAssignableFrom(type));
            
            foreach (var type in injectables)
            {
                InjectableAttribute injectableAttribute = type.GetCustomAttribute<InjectableAttribute>();
                Object[] instances = UnityEngine.Object.FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None);
                
                foreach (var instance in instances)
                {
                    TryBind(instance, injectableAttribute, type);
                }
            }
        }

        private void TryBind(Object instance, InjectableAttribute injectableAttribute, Type type)
        {
            if (!(instance is Component))
            {
                return;
            }
            
            Component component = (Component)instance;
            
            if (injectableAttribute.Singleton && !_container.HasSingleton(component.GetType()))
            {
                _container.Bind(component.GetType().Name, component.gameObject.name, component);
            } 
            else if (!injectableAttribute.Singleton && !_container.Has(component.GetType().Name, component.gameObject.name))
            {
                _container.Bind(component.GetType().Name, component.gameObject.name, component);
            }
            else
            {
                Debug.LogWarning($"{type.Name} already registered in scene in object: {component.gameObject.name}");
            }
        }
    }
}