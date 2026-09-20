using UnityEngine;

namespace DB.DependencyInjection
{
    public class InjectableEntry
    {
        public string ComponentName { get; private set; }
        public string GameObjectName { get; private set; }
        public int GameObjectInstanceID { get; private set; }
        public Component Component { get; private set; }

        public InjectableEntry(string componentName, string gameObjectName, int gameObjectInstanceID,
            Component component)
        {
            ComponentName = componentName;
            GameObjectName = gameObjectName;
            GameObjectInstanceID = gameObjectInstanceID;
            Component = component;
        }
    }
}