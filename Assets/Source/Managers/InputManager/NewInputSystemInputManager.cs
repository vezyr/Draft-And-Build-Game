using DB.DependencyInjection.Attributes;
using UnityEngine;

namespace DB.Manager.InputManager
{
    [Injectable(singleton: true)]
    public class NewInputSystemInputManager : MonoBehaviour, IInputManager
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public Vector2 GetMousePosition()
        {
            throw new System.NotImplementedException();
        }
    }
}