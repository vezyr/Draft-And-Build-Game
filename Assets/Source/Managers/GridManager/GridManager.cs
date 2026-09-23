using UnityEngine;
using System.Collections.Generic;
using DB.Datas;
using DB.DependencyInjection.Attributes;
using DB.Managers.InputManager;

namespace DB.Managers.GridManager
{
	[RequireComponent(typeof(BuildController))]
	public class GridManager : MonoBehaviour
	{
		public Vector2Int? HoveredTileCoordinates { get; private set; }
		
		private readonly Dictionary<Vector2Int, BuildingDefinition> _grid = new Dictionary<Vector2Int, BuildingDefinition>();
		private Camera _mainCamera;
		private Plane _groundPlane;
		private float _gridTileSize;
		private BuildController _buildController;
		
		[SerializeField] private GameObject _gridTilePrefab;
		[SerializeField] private GameObject _gridTileSelector;
		[SerializeField] private GameObject _gridContainer;
		[SerializeField] private Vector2Int _gridDimensions;
		[SerializeField] private float _gridTileSpacing = 0.3f;
		
		private IInputManager _inputManager;
		
		private void Start()
		{
			_mainCamera = Camera.main;
			_groundPlane = new Plane(Vector3.up, Vector3.zero);
			_buildController = GetComponent<BuildController>();
			CalculateGridTileSize();
			GenerateGrid();
		}

		private void Update()
		{
			if (_buildController.IsPlacingBuilding)
			{
				HighlightHoveredTile();
			}
			else
			{
				if (_gridTileSelector.activeSelf)
				{
					HoveredTileCoordinates = null;
					SetGridTileSelectorActive(false);
				}
			}
		}
		
		public Vector3 GetTilePosition(int x, int y)
		{
			return new Vector3(
				x * _gridTileSize + _gridTileSpacing * x,
				0,
				y * _gridTileSize + _gridTileSpacing * y
			);
		}
		
		public bool IsTileEmpty(Vector2Int coordinates) => _grid[coordinates] == null;

		public bool TryPlaceBuildingOnTile(Vector2Int coordinates, BuildingDefinition buildingDefinition)
		{
			if (!IsTileEmpty(coordinates))
			{
				Debug.LogError($"Can not place build on tile {coordinates}. Tile already occupied!");
				return false;
			}
			_grid[coordinates] = buildingDefinition;
			return true;
		}
		
		private void CalculateGridTileSize()
		{
			BoxCollider gridTileBoxCollider = _gridTilePrefab.GetComponentInChildren<BoxCollider>();
			if (gridTileBoxCollider != null)
			{
				Vector3 size = Vector3.Scale(gridTileBoxCollider.size, gridTileBoxCollider.transform.localScale);
				if (!Mathf.Approximately(size.x, size.z))
				{
					Debug.LogWarning("GridTile size X and Z are not equal, grid may not render correctly.");
				}

				_gridTileSize = size.x;
			}

			if (Mathf.Approximately(_gridTileSize, 0))
			{
				Debug.LogWarning("GridTile size is not set correctly, grid may not render correctly.");
			}
		}
		
		private void GenerateGrid()
		{
			for (int x = 0; x < _gridDimensions.x; x++)
			{
				for (int y = 0; y < _gridDimensions.y; y++)
				{
					GameObject tile = CreateGridTile(x, y);
					if (_gridContainer != null)
					{
						tile.transform.parent = _gridContainer.transform;
					}
					_grid.Add(new Vector2Int(x, y), null);
				}
			}
		}

		private GameObject CreateGridTile(int x, int y)
		{
			GameObject tile = Instantiate(
				_gridTilePrefab,
				GetTilePosition(x, y),
				Quaternion.identity
			);
			tile.name = $"GridTile_{x}_{y}";
			return tile;
		}
		
		private void HighlightHoveredTile()
		{
			Ray ray = _mainCamera.ScreenPointToRay(_inputManager.GetMousePosition());
			if (_groundPlane.Raycast(ray, out float distance))
			{
				HandeRaycastHit(ray, distance);
			}
			else
			{
				HoveredTileCoordinates = null;
				SetGridTileSelectorActive(false);
			}
		}

		private void HandeRaycastHit(Ray ray, float distance)
		{
			Vector3 hitPoint = ray.GetPoint(distance);
			Vector2Int hoveredTile;
			if (TryCalculateHoveredTile(hitPoint, out hoveredTile))
			{
				HoveredTileCoordinates = hoveredTile;
				if (IsTileEmpty(hoveredTile))
				{
					SetGridTileSelectorActive(true);
					_gridTileSelector.transform.position = GetTilePosition(hoveredTile.x, hoveredTile.y);
				}
				else
				{
					SetGridTileSelectorActive(false);
				}
			}
			else
			{
				HoveredTileCoordinates = null;
				SetGridTileSelectorActive(false);
			}
		}
		
		private bool TryCalculateHoveredTile(Vector3 hitPoint, out Vector2Int tileCoordinates)
		{
			int x = Mathf.FloorToInt(hitPoint.x / (_gridTileSize + _gridTileSpacing));
			int y = Mathf.FloorToInt(hitPoint.z / (_gridTileSize + _gridTileSpacing));

			if (x < 0 || x >= _gridDimensions.x || y < 0 || y >= _gridDimensions.y)
			{
				tileCoordinates = new Vector2Int(-1, -1);
				return false;
			}
			tileCoordinates = new Vector2Int(x, y);
			return true;
		}

		private void SetGridTileSelectorActive(bool active)
		{
			if (_gridTileSelector.activeSelf != active)
			{
				_gridTileSelector.SetActive(active);
			}
		}

		[Inject(componentName:"NewInputSystemInputManager")]
		private void SetInputManager(IInputManager inputManager)
		{
			_inputManager = inputManager;
		}
	}
}
