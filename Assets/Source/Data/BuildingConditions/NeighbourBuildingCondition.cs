using DB.Data;
using DB.Data.BuildingConditions;
using UnityEngine;

namespace DB.Data.BuildingConditions
{
    [CreateAssetMenu(fileName = "NeighbourBuildingCondition", menuName = "Scriptable Objects/Building Conditions/Neighbour Building Condition")]
    public class NeighbourBuildingCondition : BuildingCondition
    {
        [SerializeField] private BuildingDefinition[] _neighbours;
        [SerializeField] private bool _negate;
        
        public override bool IsMet(BuildingConditionContext context)
        {
            bool hasNeighbour = false;
            foreach (BuildingDefinition buildingDefinition in _neighbours)
            {
                hasNeighbour |= HasNeighbour(buildingDefinition, context);
            }
            return _negate ? !hasNeighbour : hasNeighbour;
        }

        private bool HasNeighbour(BuildingDefinition buildingDefinition, BuildingConditionContext context)
        {
            return context.Neighborhood.Bottom == buildingDefinition || context.Neighborhood.Top == buildingDefinition ||
                   context.Neighborhood.Left == buildingDefinition || context.Neighborhood.Right == buildingDefinition;
        }
    }
}