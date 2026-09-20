using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace DB.Managers.GridManager
{
	public class GridManager : MonoBehaviour
	{
		private readonly Dictionary<Vector2Int, int> _grid = new Dictionary<Vector2Int, int>();
		private Camera _mainCamera;
		private Plane _groundPlane;

		[SerializeField] private InputActionReference _mousePositionActionReference;
		[SerializeField] private GameObject _gridTilePrefab;
		[SerializeField] private GameObject _gridContainer;
		[SerializeField] private Vector2Int _gridDimensions;
		[SerializeField] private int _gridTileSize = 2;
		[SerializeField] private float _gridTileSpacing = 0.3f;

		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start()
		{
			_mainCamera = Camera.main;
			_groundPlane = new Plane(Vector3.up, Vector3.zero);
			GenerateGrid();
		}

		private void Update()
		{
			Ray ray = _mainCamera.ScreenPointToRay(_mousePositionActionReference.action.ReadValue<Vector2>());
			if (_groundPlane.Raycast(ray, out float distance))
			{
				Vector3 hitPoint = ray.GetPoint(distance);
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
	}
}
