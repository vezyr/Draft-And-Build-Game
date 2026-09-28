using System;
using System.Collections.Generic;
using DB.Data;
using DB.DependencyInjection.Attributes;
using DB.Managers.InputManager;
using UnityEngine;

namespace DB.Managers.DeckManager
{
    [Injectable(singleton:true)]
    public class DeckManager : MonoBehaviour
    {
        private List<BuildingDefinition> _deck;

        [SerializeField] private int _deckSize;
        [SerializeField] private int numberOfPossibleBuildings = 3;
        // @todo: In next version, one of the deck definition could be chosen on UI (with total random option available)
        [SerializeField] private PredefinedDeckDefinition _predefinedDeckDefinition;
        [SerializeField] private BuildingDefinition houseBuildingDefinition;
        
        private void Start()
        {
            if (_deck == null || _deck.Count == 0)
            {
                BuildingDefinition[] availableCards = _predefinedDeckDefinition.GetCards();
                if (availableCards.Length < _deckSize)
                {
                    throw new Exception(
                        $"Deck is too small! Is has to have at least {_deckSize} elements. Please provide a larger deck.");
                }
                ShuffleCards(availableCards);
                _deck = new List<BuildingDefinition>(_deckSize);
                for (int i = 0; i < _deckSize; i++)
                {
                    _deck.Add(availableCards[i]);
                }
            }
        }

        public BuildingDefinition[] GetPossibleBuildings()
        {
            BuildingDefinition[] possibleBuildings = new BuildingDefinition[numberOfPossibleBuildings];
            for (int i = 0; i < numberOfPossibleBuildings; i++)
            {
                possibleBuildings[i] = _deck[i];
            }
            return possibleBuildings;
        }

        public void HandleBuildingPlaced(BuildingDefinition buildingDefinition)
        {
            if (buildingDefinition == null)
            {
                Debug.LogWarning("Handle building placed with null definition.");
                return;
            }
            int indexToRemove = _deck.IndexOf(buildingDefinition);
            if (indexToRemove < 0 || indexToRemove >= _deck.Count)
            {
                Debug.LogWarning("Could not find the building definition in the deck.");
                return;
            }
            _deck.RemoveAt(indexToRemove);
        }

        private void ShuffleCards(BuildingDefinition[] cards)
        {
            for (int i = cards.Length - 1; i >= 1; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }
    }
}

