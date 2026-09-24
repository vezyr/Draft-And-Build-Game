using UnityEngine;

namespace DB.Data
{
    [CreateAssetMenu(fileName = "BuildingsInteractionsMatrix", menuName = "Scriptable Objects/BuildingsInteractionsMatrix")]
    public class BuildingsInteractionsMatrix : ScriptableObject
    {
        [SerializeField] private BuildingDefinition[] _buildings;
        [SerializeField] private int[] _values;
    }
}