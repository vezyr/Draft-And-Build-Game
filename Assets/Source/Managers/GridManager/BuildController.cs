using System;
using DB.Controllers;
using DB.Data;
using DB.DependencyInjection.Attributes;
using DB.Managers.InputManager;
using DB.Managers.ScoreManager;
using Unity.VisualScripting;
using UnityEngine;

namespace DB.Managers.GridManager
{
    public enum BuildState
    {
        Unknown,
        Idle,
        Placing,
        Build,
    }

    public struct ChangeStateEventData
    {
        public BuildState State { get; private set; }
        public BuildingDefinition[] PossibleBuildings { get; private set; }

        public ChangeStateEventData(BuildState state, BuildingDefinition[] possibleBuildings)
        {
            State = state;
            PossibleBuildings = possibleBuildings;
        }
    }
    
    [Injectable]
    [RequireComponent(typeof(GridManager))]
    public class BuildController : MonoBehaviour
    {
        public Action<ChangeStateEventData> OnStateChange;
        
        private GridManager _gridManager;
        private IInputManager _inputManager;
        private DeckManager.DeckManager _deckManager;
        private ScoreManager.ScoreManager _scoreManager;
        
        private BuildState _buildState = BuildState.Unknown;
        private BuildingDefinition _selectedBuilding;
        private Vector2Int? _lastPlacingTileCoordinates;
        private BuildingController _selectedBuildingGameObject;

        private NeighborhoodScoreCalculationResult? _scoreCalculationResult = null;

        private void Start()
        {
            _gridManager = GetComponent<GridManager>();
            _gridManager.OnHoveredTileChange += HandleHoveredTileChange;
            ChangeState(BuildState.Idle);
        }

        private void OnDestroy()
        {
            _gridManager.OnHoveredTileChange -= HandleHoveredTileChange;
        }

        private void OnEnable()
        {
            RegisterOnBuildActionPerformedListener();
        }

        private void OnDisable()
        {
            UnregisterOnBuildActionPerformedListener();
        }

        public bool IsPlacingBuilding => _buildState == BuildState.Placing;

        public void OnBuildingChosen(BuildingDefinition buildingDefinition)
        {
            if (buildingDefinition == null)
            {
                Debug.LogWarning("Tried to place null building. Aborting.");
                return;
            }
            
            _selectedBuilding = buildingDefinition;
            ChangeState(BuildState.Placing);
        }

        private void ChangeState(BuildState newState)
        {
            if (_buildState == newState)
            {
                return;
            }

            switch (newState)
            {
                case BuildState.Idle:
                    if (_buildState is BuildState.Build or BuildState.Unknown)
                    {
                        OnStateChangeToIdle();
                    } 
                    break;
                case BuildState.Placing:
                    if (_buildState == BuildState.Idle)
                    {
                        OnStateChangeToPlacing();
                    }
                    break;
                case BuildState.Build:
                    if (_buildState == BuildState.Placing)
                    {
                        OnStateChangeToBuild();
                    }
                    break;
            }
        }

        private void OnStateChangeToIdle()
        {
            _selectedBuilding = null;
            _selectedBuildingGameObject = null;
            _scoreCalculationResult = null;

            if (_deckManager == null)
            {
                throw new Exception("Deck manager is not set up.");
            }
            
            _buildState = BuildState.Idle;
            _inputManager.DisableAction(ActionId.Build);

            BuildingDefinition[] possibleBuildings =
                _gridManager.NumberOfFreeTiles() > 0 ? _deckManager.GetPossibleBuildings() : Array.Empty<BuildingDefinition>();
            OnStateChange?.Invoke(new ChangeStateEventData(_buildState, possibleBuildings));
        }

        private void OnStateChangeToPlacing()
        {
            GameObject building = Instantiate(_selectedBuilding.GameObjectPrefab.gameObject);
            _selectedBuildingGameObject = building.GetComponent<BuildingController>();
            HideSelectedBuilding();
            
            _buildState = BuildState.Placing;
            _inputManager.EnableAction(ActionId.Build);
            
            OnStateChange?.Invoke(new ChangeStateEventData(_buildState, null));
        }

        private void OnStateChangeToBuild()
        {
            Vector2Int? coordinates = _gridManager.HoveredTileCoordinates;
            if (!coordinates.HasValue)
            {
                return;
            }
            if (!_gridManager.TryPlaceBuildingOnTile(coordinates.Value, _selectedBuilding))
            {
                return;
            }
            _buildState = BuildState.Build;
            OnStateChange?.Invoke(new ChangeStateEventData(_buildState, null));
            
            if (_deckManager == null)
            {
                throw new Exception("Deck manager is not set up.");
            }

            if (_scoreManager == null)
            {
                throw new Exception("Score manager is not set up.");
            }
            _deckManager.HandleBuildingPlaced(_selectedBuilding);
            _scoreManager.AddPointsToScore(_scoreCalculationResult?.TotalScore ?? 0);
            _selectedBuildingGameObject.ClearDisplayedScores();
            
            ChangeState(BuildState.Idle);
        }

        private void HandleHoveredTileChange(Vector2Int? newTileCoordinates)
        {
            if (_buildState != BuildState.Placing)
            {
                return;
            }
            if (!newTileCoordinates.HasValue)
            {
                HideSelectedBuilding();
                return;
                
            }
            Vector2Int hoveredTileCoordinates = newTileCoordinates.Value;
            UpdateHoveredTile(hoveredTileCoordinates);
        }

        private void UpdateHoveredTile(Vector2Int hoveredTileCoordinates)
        {
            if (!_gridManager.IsTileEmpty(hoveredTileCoordinates))
            {
                HideSelectedBuilding();
                return;
            }
            
            Vector3 tilePosition =
                _gridManager.GetTilePosition(hoveredTileCoordinates.x, hoveredTileCoordinates.y);
            _selectedBuildingGameObject.transform.position = tilePosition;
            _selectedBuildingGameObject.gameObject.SetActive(true);

            if (_scoreManager is null)
            {
                throw new Exception("Score manager is not set up.");
            }

            Neighborhood neighborhood = _gridManager.GetNeighborhood(hoveredTileCoordinates);
            _scoreCalculationResult = _scoreManager.CalculateNeighborhoodScore(_selectedBuilding, neighborhood);
            _selectedBuildingGameObject.UpdateNeighbourhoodScore(_scoreCalculationResult.Value);
        }

        private void HideSelectedBuilding()
        {
            _selectedBuildingGameObject.gameObject.SetActive(false);
            _scoreCalculationResult = null;
        }
        
        private void HandleBuild()
        {
            ChangeState(BuildState.Build);
        }

        private void RegisterOnBuildActionPerformedListener()
        {
            if (_inputManager == null)
            {
                Debug.Log("Input manager not found.");
                return;
            }
            
            _inputManager.OnBuildActionPerformed -= HandleBuild;
            _inputManager.OnBuildActionPerformed += HandleBuild;
        }

        private void UnregisterOnBuildActionPerformedListener()
        {
            if (_inputManager == null)
            {
                Debug.Log("Input manager not found.");
                return;
            }
            
            _inputManager.OnBuildActionPerformed -= HandleBuild;
        }
        
        [Inject(componentType:typeof(NewInputSystemInputManager))]
        private void SetInputManager(IInputManager inputManager)
        {
            _inputManager = inputManager;
            RegisterOnBuildActionPerformedListener();
        }

        [Inject(componentType:typeof(DeckManager.DeckManager))]
        private void SetDeckManager(DeckManager.DeckManager deckManager)
        {
            _deckManager = deckManager;
        }

        [Inject(componentType:typeof(ScoreManager.ScoreManager))]
        private void SetScoreManager(ScoreManager.ScoreManager scoreManager)
        {
            _scoreManager = scoreManager;
        }
    }
}
