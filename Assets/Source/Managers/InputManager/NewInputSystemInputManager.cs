using System;
using System.Collections.Generic;
using DB.DependencyInjection.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DB.Managers.InputManager
{
    [Serializable]
    struct ActionBinding
    {
        public ActionId Id;
        public InputActionReference Reference;
    }
    
    [Injectable(singleton: true)]
    public class NewInputSystemInputManager : MonoBehaviour, IInputManager
    {
        public event Action OnBuildActionPerformed;
        // @todo: Move it to dedicated CheatManager
        public event Action OnCheatBuildHouseActionPerformed;
        
        private bool _isMousePositionActionReferenceSet = false;
        private Dictionary<ActionId, InputActionReference> _actions = new Dictionary<ActionId, InputActionReference>();
        
        [SerializeField] private List<ActionBinding> _actionBindings;

        private void Awake()
        {
            _actionBindings.ForEach(binding => _actions.Add(binding.Id, binding.Reference));

            InputActionReference mousePositionActionReference = _actions[ActionId.MousePosition];
            _isMousePositionActionReferenceSet =
                mousePositionActionReference != null && mousePositionActionReference.action != null;
            
            DisableAction(ActionId.Build);
        }
        
        private void OnEnable()
        {
            if (_actions.TryGetValue(ActionId.Build, out var buildActionReference))
                buildActionReference.action.performed += InvokeOnBuildActionPerformed;

            if (_actions.TryGetValue(ActionId.CheatBuildHouse, out var cheatBuildHouseActionReference))
                cheatBuildHouseActionReference.action.performed += InvokeOnCheatBuildHouseActionPerformed;
        }

        private void OnDisable()
        {
            if (_actions.TryGetValue(ActionId.Build, out var buildActionReference))
                buildActionReference.action.performed -= InvokeOnBuildActionPerformed;

            if (_actions.TryGetValue(ActionId.CheatBuildHouse, out var cheatBuildHouseActionReference))
                cheatBuildHouseActionReference.action.performed -= InvokeOnCheatBuildHouseActionPerformed;
        }

        public Vector2 GetMousePosition()
        {
            return _isMousePositionActionReferenceSet
                ? _actions[ActionId.MousePosition].action.ReadValue<Vector2>()
                : Vector2.zero;
        }

        public void EnableAction(ActionId id)
        {
            if (!_actions.TryGetValue(id, out var actionReference))
            {
                Debug.LogWarning($"Could not enable action. Action {id} not found in the action bindings.");
                return;
            }
            actionReference.action.Enable();
        }

        public void DisableAction(ActionId id)
        {
            if (!_actions.TryGetValue(id, out var actionReference))
            {
                Debug.LogWarning($"Could not disable action. Action {id} not found in the action bindings.");
                return;
            }
            actionReference.action.Disable();
        }

        private void InvokeOnBuildActionPerformed(InputAction.CallbackContext context)
        {
            OnBuildActionPerformed?.Invoke();
        }

        private void InvokeOnCheatBuildHouseActionPerformed(InputAction.CallbackContext context)
        {
            OnCheatBuildHouseActionPerformed?.Invoke();
        }
    }
}