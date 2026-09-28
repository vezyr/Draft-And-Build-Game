using UnityEngine;

namespace DB.Data
{
    [CreateAssetMenu(fileName = "PredefinedDeckDefinition", menuName = "Scriptable Objects/PredefinedDeckDefinition")]
    public class PredefinedDeckDefinition : ScriptableObject
    {
        [SerializeField] private BuildingDefinition[] Cards;

        public BuildingDefinition[] GetCards()
        {
            return Cards.Clone() as BuildingDefinition[];
        }
    }
}
