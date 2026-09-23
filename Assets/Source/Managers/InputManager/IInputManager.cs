using System;
using UnityEngine;

namespace DB.Managers.InputManager
{
    public interface IInputManager
    {
        public event Action OnBuildActionPerformed;
        // @todo: Move it to dedicated CheatManager
        public event Action OnCheatBuildHouseActionPerformed;
        
        public Vector2 GetMousePosition();
        
        public void EnableAction(ActionId id);
        public void DisableAction(ActionId id);
    }
}