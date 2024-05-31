using Sleep0.Logic.Game;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sleep0.EditorExtensions
{
    //[Overlay(typeof(SceneView), "Spaceship Stats", true)]
    public class SpaceshipOverlay : Overlay, IUpdatableOverlay
    {
        private Label _currentState;

        private Spaceship _spaceShip;

        public SpaceshipOverlay() { }

        public SpaceshipOverlay(Spaceship spaceship)
        {
            _spaceShip = spaceship;
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { name = "Spaceship Overlay" };

            root.style.width = new StyleLength(new Length(270, LengthUnit.Pixel));

            var label = new Label("Spaceship Stats");
            label.style.flexGrow = 1;
            label.style.flexDirection = FlexDirection.Row;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            root.Add(label);

            _currentState = new Label("Current State: None");
            _currentState.style.flexGrow = 1;
            _currentState.style.flexDirection = FlexDirection.Row;
            _currentState.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(_currentState);

            return root;
        }

        public void Update()
        {
            _currentState.text = $"Current State: {_spaceShip.GetCurrentState()}";
        }
    }
}