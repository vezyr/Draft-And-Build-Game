using System;
using DB.Datas;
using DB.DependencyInjection.Attributes;
using DB.Managers.InputManager;
using UnityEngine;

namespace DB.Managers.GridManager
{
    enum BuildState
    {
        Idle,
        Placing,
    }
    
    [RequireComponent(typeof(GridManager))]
    public class BuildController : MonoBehaviour
    {
        private GridManager _gridManager;
        private IInputManager _inputManager;
        private DeckManager.DeckManager _deckManager;
        private BuildState _buildState = BuildState.Idle;
        private BuildingDefinition _selectedBuilding;

        private void Start()
        {
            _gridManager = GetComponent<GridManager>();
        }

        private void OnEnable()
        {
            if (_deckManager != null)
            {
                _deckManager.OnCardSelected += HandleCardSelection;
            }
        }

        private void OnDisable()
        {
            _deckManager.OnCardSelected -= HandleCardSelection;
        }

        public bool IsPlacingBuilding => _buildState == BuildState.Placing;
        
        private void HandleCardSelection(BuildingDefinition buildingDefinition)
        {
            _selectedBuilding = buildingDefinition;
            _buildState = BuildState.Placing;
        }
        
        [Inject(componentName:"NewInputSystemInputManager")]
        private void SetInputManager(IInputManager inputManager)
        {
            _inputManager = inputManager;
        }

        [Inject(componentName:"DeckManager")]
        private void SetDeckManager(DeckManager.DeckManager deckManager)
        {
            _deckManager = deckManager;
            _deckManager.OnCardSelected += HandleCardSelection;
        }
    }
}
