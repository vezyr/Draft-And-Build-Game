using System;
using System.Collections.Generic;
using DB.Controllers;
using DB.Data;
using DB.Data.BuildingConditions;
using DB.DependencyInjection.Attributes;
using DB.Managers.InputManager;
using DB.Managers.ScoreManager;
using UnityEngine;

namespace DB.Managers.GridManager
{
    public enum BuildState
    {
        Unknown,
        Idle,
        Placing,
        Build,
        CancelBuild,
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

    readonly struct Transition : IEquatable<Transition>
    {
        public readonly BuildState From;
        public readonly BuildState To;

        public Transition(BuildState to, BuildState from)
        {
            From = from;
            To = to;
        }

        public bool Equals(Transition other)
        {
            return From == other.From && To == other.To;
        }

        public override bool Equals(object obj)
        {
            return obj is Transition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)From, (int)To);
        }
    }
    
    [Injectable]
    [RequireComponent(typeof(GridManager))]
    public class BuildController : MonoBehaviour
    {
        public Action<ChangeStateEventData> OnStateChange;

        private readonly Dictionary<Transition, Action> _transitions = new Dictionary<Transition, Action>();
        
        private GridManager _gridManager;
        private IInputManager _inputManager;
        private DeckManager.DeckManager _deckManager;
        private ScoreManager.ScoreManager _scoreManager;
        
        private BuildState _buildState = BuildState.Unknown;
        private BuildingDefinition _selectedBuilding;
        private Vector2Int? _lastPlacingTileCoordinates;
        private BuildingController _selectedBuildingGameObject;

        private NeighborhoodScoreCalculationResult? _scoreCalculationResult = null;

        private void Awake()
        {
            _transitions.Add(new Transition(BuildState.Idle, BuildState.Unknown), OnStateChangeToIdle);
            _transitions.Add(new Transition(BuildState.Idle, BuildState.Build), OnStateChangeToIdle);
            _transitions.Add(new Transition(BuildState.Idle, BuildState.CancelBuild), OnStateChangeToIdle);
            _transitions.Add(new Transition(BuildState.Placing, BuildState.Idle), OnStateChangeToPlacing);
            _transitions.Add(new Transition(BuildState.Build, BuildState.Placing), OnStateChangeToBuild);
            _transitions.Add(new Transition(BuildState.CancelBuild, BuildState.Placing), OnStateChangeToCancelBuild);
        }
        
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
            RegisterOnCancelBuildActionPerformedListener();
        }

        private void OnDisable()
        {
            UnregisterOnBuildActionPerformedListener();
            UnregisterOnCancelBuildActionPerformedListener();
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

            if (!_transitions.TryGetValue(new Transition(newState, _buildState), out var transitionAction))
            {
                Debug.LogWarning($"No transition registered for state change from {_buildState} to {newState}.");
                return;
            }

            transitionAction.Invoke();
        }

        private void OnStateChangeToIdle()
        {
            _selectedBuilding = null;
            _selectedBuildingGameObject = null;
            _scoreCalculationResult = null;

            ValidateInjectedManagers();
            
            _buildState = BuildState.Idle;
            _inputManager.DisableAction(ActionId.Build);
            _inputManager.DisableAction(ActionId.CancelBuild);

            BuildingDefinition[] possibleBuildings =
                _gridManager.NumberOfFreeTiles() > 0 ? _deckManager.GetPossibleBuildings() : Array.Empty<BuildingDefinition>();
            OnStateChange?.Invoke(new ChangeStateEventData(_buildState, possibleBuildings));
        }

        private void OnStateChangeToPlacing()
        {
            GameObject building = Instantiate(_selectedBuilding.GameObjectPrefab.gameObject);
            _selectedBuildingGameObject = building.GetComponent<BuildingController>();
            HideSelectedBuilding();
            
            ValidateInjectedManagers();
            
            _buildState = BuildState.Placing;
            _inputManager.EnableAction(ActionId.Build);
            _inputManager.EnableAction(ActionId.CancelBuild);
            
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
            
            ValidateInjectedManagers();
            
            _deckManager.HandleBuildingPlaced(_selectedBuilding);
            _scoreManager.AddPointsToScore(_scoreCalculationResult?.TotalScore ?? 0);
            _selectedBuildingGameObject.ClearDisplayedScores();
            
            ChangeState(BuildState.Idle);
        }

        private void OnStateChangeToCancelBuild()
        {
            _buildState = BuildState.CancelBuild;
            if (_selectedBuildingGameObject != null)
            {
                Destroy(_selectedBuildingGameObject.gameObject);
            }
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
            BuildingConditionContext buildingConditionContext = new BuildingConditionContext(
                hoveredTileCoordinates, _gridManager.GetNeighborhood(hoveredTileCoordinates));
            _gridManager.UpdateGridTileSelector(_gridManager.IsBuildPossible(hoveredTileCoordinates) &&
                                                _selectedBuilding.AreAllMet(buildingConditionContext));
            
            if (!_gridManager.IsTileEmpty(hoveredTileCoordinates))
            {
                HideSelectedBuilding();
                return;
            }
            
            Vector3 tilePosition =
                _gridManager.GetTilePosition(hoveredTileCoordinates.x, hoveredTileCoordinates.y);
            _selectedBuildingGameObject.transform.position = tilePosition;
            _selectedBuildingGameObject.gameObject.SetActive(true);

            ValidateInjectedManagers();

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
            if (_buildState != BuildState.Placing) return;
            ValidateInjectedManagers();
            
            Vector2Int? hoveredTileCoordinates = _gridManager.HoveredTileCoordinates;
            if (!hoveredTileCoordinates.HasValue)
            {
                Debug.Log("Cannot build without a hovered tile.");
                return;
            }

            BuildingConditionContext buildingConditionContext = new BuildingConditionContext(
                hoveredTileCoordinates.Value, _gridManager.GetNeighborhood(hoveredTileCoordinates.Value));
            if (!_gridManager.IsBuildPossible(hoveredTileCoordinates.Value) || !_selectedBuilding.AreAllMet(buildingConditionContext))
            {
                Debug.Log("Cannot build on this tile. The tile is already taken or building conditions aren't met.");
                return;
            }
            ChangeState(BuildState.Build);
        }

        private void HandleCancelBuild()
        {
            if (_buildState != BuildState.Placing) return;
            ValidateInjectedManagers();
            
            ChangeState(BuildState.CancelBuild);
        }

        private void RegisterOnBuildActionPerformedListener()
        {
            if (_inputManager is null)
            {
                Debug.Log("Input manager not found.");
                return;
            }
            
            _inputManager.OnBuildActionPerformed -= HandleBuild;
            _inputManager.OnBuildActionPerformed += HandleBuild;
        }

        private void UnregisterOnBuildActionPerformedListener()
        {
            if (_inputManager is null)
            {
                Debug.Log("Input manager not found.");
                return;
            }
            
            _inputManager.OnBuildActionPerformed -= HandleBuild;
        }

        private void RegisterOnCancelBuildActionPerformedListener()
        {
            if (_inputManager is null)
            {
                Debug.Log("Input manager not found.");
                return;
            }
            
            _inputManager.OnCancelBuildActionPerformed -= HandleCancelBuild;
            _inputManager.OnCancelBuildActionPerformed += HandleCancelBuild;
        }

        private void UnregisterOnCancelBuildActionPerformedListener()
        {
            if (_inputManager is null)
            {
                Debug.Log("Input manager not found.");
                return;
            }
            
            _inputManager.OnCancelBuildActionPerformed -= HandleCancelBuild;
        }

        private void ValidateInjectedManagers()
        {
            if (_deckManager is null) throw new Exception("Deck manager is not set up.");
            if (_inputManager is null) throw new Exception("Input manager is not set up.");
            if (_scoreManager is null) throw new Exception("Score manager is not set up.");
        }
        
        [Inject(componentType:typeof(NewInputSystemInputManager))]
        private void SetInputManager(IInputManager inputManager)
        {
            _inputManager = inputManager;
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
