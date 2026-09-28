using System;
using System.Collections.Generic;
using System.Linq;
using DB.Data;
using DB.DependencyInjection.Attributes;
using DB.Managers.GridManager;
using DB.Managers.ScoreManager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace DB.UI
{
    public class MainUIController : MonoBehaviour
    {
        private Dictionary<Button, BuildingDefinition> _buttonCardDictionary;
        private Label _scoreLabel;
        
        private BuildController _buildController;
        private ScoreManager _scoreManager;
        
        private void OnEnable()
        {
            UIDocument root = GetComponent<UIDocument>();

            _scoreLabel = root.rootVisualElement.Q<Label>("CurrentScore");
            
            List<Button> cardButtons = root.rootVisualElement.Query<VisualElement>("CardsContainer").Children<Button>().ToList();
            if (cardButtons == null || cardButtons.Count <= 0)
            {
                Debug.LogError("No card buttons found on the UI.");
                return;
            }

            if (_buttonCardDictionary == null || _buttonCardDictionary.Count != cardButtons.Count)
            {
                Debug.LogWarning("Number of cards has changed.");
                Dictionary<Button, BuildingDefinition> newButtonCardDictionary = new Dictionary<Button, BuildingDefinition>();
                foreach (Button button in cardButtons)
                {
                    if (_buttonCardDictionary != null && _buttonCardDictionary.TryGetValue(button, out var buildingDefinition))
                    {
                        newButtonCardDictionary.Add(button, buildingDefinition);
                    }
                    else
                    {
                        newButtonCardDictionary.Add(button, null);
                    }
                }
                _buttonCardDictionary = newButtonCardDictionary;
            }
            Debug.Log("Button Card Dictionary is set.");
            
            RegisterCardButtonClickEvents(_buttonCardDictionary.Keys.ToList());
            RegisterOnBuildStateChangeListener();
            RegisterOnScoreChangeHandler();
        }

        private void OnDisable()
        {
            UnregisterCardButtonClickEvents(_buttonCardDictionary.Keys.ToList());
            UnregisterBuildStateChangeListener();
            UnregisterOnScoreChangeHandler();
        }

        private void RegisterCardButtonClickEvents(List<Button> buttons)
        {
            if (buttons == null)
            {
                Debug.LogWarning("Could not register cards' buttons click events. Buttons list is null.");
                return;
            }

            foreach (Button button in buttons)
            {
                button?.RegisterCallback<ClickEvent>(OnCardButtonClicked);
            }
        }

        private void UnregisterCardButtonClickEvents(List<Button> buttons)
        {
            if (buttons == null)
            {
                Debug.LogWarning("Could not unregister cards' buttons click events. Buttons list is null.");
                return;
            } 
            
            foreach (Button button in buttons)
            {
                button?.UnregisterCallback<ClickEvent>(OnCardButtonClicked);
            }
        }

        private void OnCardButtonClicked(ClickEvent evt)
        {
            Button button = evt.target as Button;
            if (button == null)
            {
                Debug.LogError("Button is null.");
                return;
            }

            if (_buttonCardDictionary.TryGetValue(button, out var buildingDefinition) && buildingDefinition != null)
            {
                _buildController.OnBuildingChosen(buildingDefinition);
            }
        }

        private void OnBuildStateChange(ChangeStateEventData data)
        {
            switch (data.State)
            {
                case BuildState.Idle:
                    HandleBuildStateIdle(data);
                    break;
                case BuildState.Placing:
                    HandleBuildStatePlacing(data);
                    break;
                default:
                    Debug.Log($"Invoked build state change to: {data.State}. Nothing to handle.");
                    break;
            }
        }

        private void HandleBuildStateIdle(ChangeStateEventData data)
        {
            if (data.PossibleBuildings.Length > _buttonCardDictionary.Count)
            {
                Debug.LogWarning("There are more possible buildings to build than available build cards. Not all may display.");
            }

            int possibleCardIndex = 0;
            foreach (Button button in _buttonCardDictionary.Keys.ToList())
            {
                if (possibleCardIndex < data.PossibleBuildings.Length)
                {
                    BuildingDefinition buildingDefinition = data.PossibleBuildings[possibleCardIndex];
                    
                    _buttonCardDictionary[button] = buildingDefinition;
                    button.text = buildingDefinition.DisplayName;
                    button.SetEnabled(true);
                }
                else
                {
                    _buttonCardDictionary[button] = null;
                    button.text = "";
                    button.SetEnabled(false);
                }

                possibleCardIndex++;
            }
        }

        private void HandleBuildStatePlacing(ChangeStateEventData data)
        {
            foreach (Button button in _buttonCardDictionary.Keys.ToList())
            {
                _buttonCardDictionary[button] = null;
                button.SetEnabled(false);
            }
        }

        private void HandleScoreChanged(int currentScore)
        {
            if (_scoreLabel == null)
            {
                return;
            }

            _scoreLabel.text = currentScore.ToString();
        }

        private void RegisterOnBuildStateChangeListener()
        {
            if (_buildController == null)
            {
                Debug.Log("Build controller not found.");
                return;
            }
            
            _buildController.OnStateChange -= OnBuildStateChange;
            _buildController.OnStateChange += OnBuildStateChange;
        }

        private void UnregisterBuildStateChangeListener()
        {
            if (_buildController == null)
            {
                Debug.Log("Build controller not found.");
                return;
            }
            _buildController.OnStateChange -= OnBuildStateChange;
        }

        private void RegisterOnScoreChangeHandler()
        {
            if (_scoreManager == null)
            {
                Debug.Log("Score manager not found.");
                return;
            }
            _scoreManager.OnScoreChange -= HandleScoreChanged;
            _scoreManager.OnScoreChange += HandleScoreChanged;
        }
        
        private void UnregisterOnScoreChangeHandler()
        {
            if (_scoreManager == null)
            {
                Debug.Log("Score manager not found.");
                return;
            }
            _scoreManager.OnScoreChange -= HandleScoreChanged;
        }
        
        [Inject(componentType:typeof(BuildController))]
        private void SetBuildController(BuildController buildController)
        {
            _buildController = buildController;
            RegisterOnBuildStateChangeListener();
        }

        [Inject(componentType:typeof(ScoreManager))]
        private void SetScoreManager(ScoreManager scoreManager)
        {
            _scoreManager = scoreManager;
            RegisterOnScoreChangeHandler();
        }
    }
}
