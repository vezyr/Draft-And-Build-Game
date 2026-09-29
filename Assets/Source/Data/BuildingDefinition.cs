using DB.Controllers;
using DB.Data.BuildingConditions;
using UnityEngine;

namespace DB.Data
{
    [CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Scriptable Objects/BuildingDefinition")]
    public class BuildingDefinition : ScriptableObject
    {
        public string DisplayName;
        public int BaseScore;
        public BuildingController GameObjectPrefab;
        public BuildingCondition[] Conditions;

        public bool AreAllMet(BuildingConditionContext context)
        {
            foreach (BuildingCondition condition in Conditions)
            {
                if (!condition.IsMet(context)) return false;
            }

            return true;
        }
    }
}