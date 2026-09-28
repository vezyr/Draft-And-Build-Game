using System;
using UnityEngine;

namespace DB.Data
{
    [CreateAssetMenu(fileName = "BuildingsInteractionsMatrix", menuName = "Scriptable Objects/BuildingsInteractionsMatrix")]
    public class BuildingsInteractionsMatrix : ScriptableObject
    {
        [SerializeField] private BuildingDefinition[] _buildings;
        [SerializeField] private int[] _values;

        public int GetValue(BuildingDefinition building, BuildingDefinition neighbour)
        {
            int buildingIndex = Array.IndexOf(_buildings, building);
            int neighbourIndex = Array.IndexOf(_buildings, neighbour);
            int valueIndex = neighbourIndex + _buildings.Length * buildingIndex;
            if (valueIndex < 0 || valueIndex >= _values.Length)
            {
                return 0;
            }
            return _values[valueIndex];
        }
    }
}