using System;
using DB.DependencyInjection.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DB.Manager.InputManager
{
    [Injectable(singleton: true)]
    public class NewInputSystemInputManager : MonoBehaviour, IInputManager
    {
        private bool _isMousePositionActionReferenceSet = false;
        [SerializeField] private InputActionReference _mousePositionActionReference;

        void Awake()
        {
            _isMousePositionActionReferenceSet =
                _mousePositionActionReference != null && _mousePositionActionReference.action != null;
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