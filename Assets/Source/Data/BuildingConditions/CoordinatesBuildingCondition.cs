using UnityEngine;

namespace DB.Data.BuildingConditions
{
    [CreateAssetMenu(fileName = "CoordinatesBuildingCondition", menuName = "Scriptable Objects/Building Conditions/Coordinates Building Condition")]
    public class CoordinatesBuildingCondition : BuildingCondition
    {
        [SerializeField] private int _xCoodinateMin;
        [SerializeField] private int _xCoodinateMax;
        [SerializeField] private int _yCoodinateMin;
        [SerializeField] private int _yCoodinateMax;
        [SerializeField] private bool _negate;
        
        public override bool IsMet(BuildingConditionContext context)
        {
            Vector2Int coordinates = context.Coordinates;
            bool coordinatesCheckResult = coordinates.x >= _xCoodinateMin && coordinates.x <= _xCoodinateMax &&
                                          coordinates.y >= _yCoodinateMin && coordinates.y <= _yCoodinateMax;
            return _negate ? !coordinatesCheckResult : coordinatesCheckResult;
        }
    }
}