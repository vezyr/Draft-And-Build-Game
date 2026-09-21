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

        private void Start()
        {
            Dictionary<Type, List<MethodInfo>> methodsCache = new Dictionary<Type, List<MethodInfo>>();
            Object[] instances = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var instance in instances)
            {
                Type instanceType = instance.GetType();

                if (!methodsCache.ContainsKey(instanceType))
                {
                    List<MethodInfo> methodsWithInjectAttribute = instanceType
                        .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(method => method.IsDefined(typeof(InjectAttribute), false))
                        .ToList();
                    methodsCache[instanceType] = methodsWithInjectAttribute;
                }
                
                List<MethodInfo> methodsToProcess = methodsCache[instanceType];
                foreach (var method in methodsToProcess)
                {
                    InjectAttribute injectAttribute = method.GetCustomAttribute<InjectAttribute>();
                    if (injectAttribute.ComponentType != null)
                    {
                        throw new NotImplementedException("Component type injection is not implemented yet.");
                    }
                    InjectableEntry entry = _container.Get(injectAttribute.ComponentName, injectAttribute.GameObjectName);
                    method.Invoke(instance, new[] { entry.Component });
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