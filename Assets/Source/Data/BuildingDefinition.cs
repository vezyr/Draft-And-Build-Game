using UnityEngine;

namespace DB.Data
{
    [CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Scriptable Objects/BuildingDefinition")]
    public class BuildingDefinition : ScriptableObject
    {
        public string DisplayName;
        public int BaseScore;
        public GameObject GameObjectPrefab;
    }
}