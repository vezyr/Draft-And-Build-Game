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
        private Vector2Int? _lastPlacingTileCoordinates;
        private GameObject _selectedBuildingGameObject;

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

            if (_inputManager != null)
            {
                _inputManager.OnBuildActionPerformed += HandleBuild;
            }
        }

        private void OnDisable()
        {
            _deckManager.OnCardSelected -= HandleCardSelection;
            _inputManager.OnBuildActionPerformed -= HandleBuild;
        }

        private void Update()
        {
            if (IsPlacingBuilding)
            {
                HandlePlacing();
            }
        }

        public bool IsPlacingBuilding => _buildState == BuildState.Placing;
        
        private void HandleCardSelection(BuildingDefinition buildingDefinition)
        {
            _selectedBuilding = buildingDefinition;
            _selectedBuildingGameObject = Instantiate(_selectedBuilding.GameObjectPrefab);
            _selectedBuildingGameObject.SetActive(false);
            _buildState = BuildState.Placing;
            
            _inputManager.EnableAction(ActionId.Build);
        }

        private void HandlePlacing()
        {
            if (_gridManager.HoveredTileCoordinates.HasValue)
            {
                Vector2Int hoveredTileCoordinates = _gridManager.HoveredTileCoordinates.Value;
                if (hoveredTileCoordinates != _lastPlacingTileCoordinates)
                {
                    UpdateHoveredTile(hoveredTileCoordinates);
                }
            }
            else
            {
                _selectedBuildingGameObject.SetActive(false);
                _lastPlacingTileCoordinates = null;
            }
        }

        private void UpdateHoveredTile(Vector2Int hoveredTileCoordinates)
        {
            _lastPlacingTileCoordinates = hoveredTileCoordinates;

            if (_gridManager.IsTileEmpty(_lastPlacingTileCoordinates.Value))
            {
                Vector3 tilePosition =
                    _gridManager.GetTilePosition(hoveredTileCoordinates.x, hoveredTileCoordinates.y);
                _selectedBuildingGameObject.transform.position = tilePosition;
                _selectedBuildingGameObject.SetActive(true);
            }
            else
            {
                _selectedBuildingGameObject.SetActive(false);
            }
        }
        
        private void HandleBuild()
        {
            if (!_lastPlacingTileCoordinates.HasValue)
            {
                return;
            }

            if (!_gridManager.TryPlaceBuildingOnTile(_lastPlacingTileCoordinates.Value, _selectedBuilding))
            {
                return;
            }

            _selectedBuilding = null;
            _selectedBuildingGameObject = null;
            _buildState = BuildState.Idle;
            _inputManager.DisableAction(ActionId.Build);
        }
        
        [Inject(componentName:"NewInputSystemInputManager")]
        private void SetInputManager(IInputManager inputManager)
        {
            _inputManager = inputManager;
            _inputManager.OnBuildActionPerformed += HandleBuild;
        }

        [Inject(componentName:"DeckManager")]
        private void SetDeckManager(DeckManager.DeckManager deckManager)
        {
            _deckManager = deckManager;
            _deckManager.OnCardSelected += HandleCardSelection;
        }
    }
}
