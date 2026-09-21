using UnityEngine;
using System.Collections.Generic;
using DB.DependencyInjection.Attributes;
using DB.Manager.InputManager;

namespace DB.Managers.GridManager
{
	public class GridManager : MonoBehaviour
	{
		private readonly Dictionary<Vector2Int, int> _grid = new Dictionary<Vector2Int, int>();
		private Camera _mainCamera;
		private Plane _groundPlane;
		private float _gridTileSize;
		
		[SerializeField] private GameObject _gridTilePrefab;
		[SerializeField] private GameObject _gridTileSelector;
		[SerializeField] private GameObject _gridContainer;
		[SerializeField] private Vector2Int _gridDimensions;
		[SerializeField] private float _gridTileSpacing = 0.3f;
		
		private IInputManager _inputManager;
		
		void Start()
		{
			_mainCamera = Camera.main;
			_groundPlane = new Plane(Vector3.up, Vector3.zero);
			CalculateGridTileSize();
			GenerateGrid();
		}

		void Update()
		{
			Ray ray = _mainCamera.ScreenPointToRay(_inputManager.GetMousePosition());
			if (_groundPlane.Raycast(ray, out float distance))
			{
				Vector3 hitPoint = ray.GetPoint(distance);
				Vector2Int hoveredTile;
				if (!TryCalculateHoveredTile(hitPoint, out hoveredTile))
				{
					SetGridTileSelectorActive(false);
				}
				else
				{
					SetGridTileSelectorActive(true);
					// @todo: move calculation to separated method and use it in all places.
					_gridTileSelector.transform.position = new Vector3(
						hoveredTile.x * _gridTileSize + _gridTileSpacing * hoveredTile.x,
						0,
						hoveredTile.y * _gridTileSize + _gridTileSpacing * hoveredTile.y
					);
				}
			}
			else
			{
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
				}
			}
		}

		private GameObject CreateGridTile(int x, int y)
		{
			GameObject tile = Instantiate(
				_gridTilePrefab,
				new Vector3(
					x * _gridTileSize + _gridTileSpacing * x,
					0,
					y * _gridTileSize + _gridTileSpacing * y
				),
				Quaternion.identity
			);
			tile.name = $"GridTile_{x}_{y}";
			return tile;
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
