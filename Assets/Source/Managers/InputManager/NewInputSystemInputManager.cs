using System;
using DB.DependencyInjection.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DB.Managers.InputManager
{
    [Injectable(singleton: true)]
    public class NewInputSystemInputManager : MonoBehaviour, IInputManager
    {
        public event Action OnBuildActionPerformed;
        // @todo: Move it to dedicated CheatManager
        public event Action OnCheatBuildHouseActionPerformed;
        
        private bool _isMousePositionActionReferenceSet = false;
        [SerializeField] private InputActionReference _mousePositionActionReference;
        [SerializeField] private InputActionReference _buildActionReference;
        [SerializeField] private InputActionReference _cheatBuildHouseActionReference;

        private void Awake()
        {
            _isMousePositionActionReferenceSet =
                _mousePositionActionReference != null && _mousePositionActionReference.action != null;
        }
        
        private void OnEnable()
        {
            _buildActionReference.action.performed += ctx => OnBuildActionPerformed?.Invoke();
            _cheatBuildHouseActionReference.action.performed += ctx => OnCheatBuildHouseActionPerformed?.Invoke();
        }

        private void OnDisable()
        {
            _buildActionReference.action.performed -= ctx => OnBuildActionPerformed?.Invoke();
            _cheatBuildHouseActionReference.action.performed -= ctx => OnCheatBuildHouseActionPerformed?.Invoke();
        }

        public Vector2 GetMousePosition()
        {
            if (!_isMousePositionActionReferenceSet)
            {
                return Vector2.zero;
            }
            return _mousePositionActionReference.action.ReadValue<Vector2>();
        }
        
        
    }
}