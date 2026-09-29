using UnityEngine;

namespace DB.Data.BuildingConditions
{
    public abstract class BuildingCondition : ScriptableObject
    {
        public abstract bool IsMet(BuildingConditionContext context);
    }    
}