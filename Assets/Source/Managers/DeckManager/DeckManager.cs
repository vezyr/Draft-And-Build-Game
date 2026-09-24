using System;
using DB.Data;
using DB.DependencyInjection.Attributes;
using DB.Managers.InputManager;
using UnityEngine;

namespace DB.Managers.DeckManager
{
    [Injectable(singleton:true)]
    public class DeckManager : MonoBehaviour
    {
        public Action<BuildingDefinition> OnCardSelected;
        private IInputManager _inputManager;
        
        [SerializeField] private BuildingDefinition houseBuildingDefinition;
        
        private void Start()
        {
            _inputManager.OnCheatBuildHouseActionPerformed += () => OnCardSelected?.Invoke(houseBuildingDefinition);
        }
        
        private void Update()
        {
        
        }

        [Inject(componentName:"NewInputSystemInputManager")]
        private void SetInputManager(IInputManager inputManager)
        {
            _inputManager = inputManager;    
        }
    }
}

