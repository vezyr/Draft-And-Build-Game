using DB.Managers.GridManager;
using UnityEngine;

namespace DB.Data.BuildingConditions
{
    public struct BuildingConditionContext
    {
        public Vector2Int Coordinates { get; private set; }
        public Neighborhood Neighborhood { get; private set; }
        
        public BuildingConditionContext(Vector2Int coordinates, Neighborhood neighborhood)
        {
            Coordinates = coordinates;
            Neighborhood = neighborhood;
        }
    }    
}
