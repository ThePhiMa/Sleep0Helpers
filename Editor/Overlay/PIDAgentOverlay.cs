using Sleep0.Logic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using static Sleep0.Logic.PIDAgent;

namespace Sleep0.EditorExtensions
{
    //[Overlay(typeof(SceneView), "PID Agent Stats", true)]
    public class PIDAgentOverlay : Overlay, IUpdatableOverlay
    {
        private PIDAgent _pidAgent;

        private Label _currentVelocityLabel;
        private Label _thrustPIDLabel;
        private Label _sideThrustPIDLabel;
        private Label _torquePIDLabel;
        private Label _autoTuningState;

        private float _pValueChangeStep = 0.01f;
        private float _iValueChangeStep = 0.01f;
        private float _dValueChangeStep = 0.01f;

        public PIDAgentOverlay() { }

        public PIDAgentOverlay(PIDAgent pidAgent)
        {
            _pidAgent = pidAgent;
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { name = "PID Agent Overlay" };

            root.style.width = new StyleLength(new Length(270, LengthUnit.Pixel));
            //root.style.height = new StyleLength(new Length(100, LengthUnit.Pixel));

            var label = new Label("PID Agent Stats");
            label.style.flexGrow = 1;
            label.style.flexDirection = FlexDirection.Row;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            root.Add(label);

            _currentVelocityLabel = new Label("Current Velocity");
            _currentVelocityLabel.style.flexGrow = 1;
            _currentVelocityLabel.style.flexDirection = FlexDirection.Row;
            _currentVelocityLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(_currentVelocityLabel);

            _thrustPIDLabel = new Label("Thrust PID");
            _thrustPIDLabel.style.flexGrow = 1;
            _thrustPIDLabel.style.flexDirection = FlexDirection.Row;
            _thrustPIDLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(_thrustPIDLabel);

            _sideThrustPIDLabel = new Label("Side Thrust PID");
            _sideThrustPIDLabel.style.flexGrow = 1;
            _sideThrustPIDLabel.style.flexDirection = FlexDirection.Row;
            _sideThrustPIDLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(_sideThrustPIDLabel);

            _torquePIDLabel = new Label("Torque PID");
            _torquePIDLabel.style.flexGrow = 1;
            _torquePIDLabel.style.flexDirection = FlexDirection.Row;
            _torquePIDLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(_torquePIDLabel);

            var leftColumLayout = new VisualElement();
            leftColumLayout.style.flexGrow = 0.5f;
            leftColumLayout.style.flexDirection = FlexDirection.Column;
            //leftColumLayout.style.width = 100;
            root.Add(leftColumLayout);

            Button addValueButton = new Button(() =>
            {
                OnButtonClicked_ChangePValue(_pValueChangeStep, PIDType.MainThrust);
            });
            addValueButton.text = $"+ {_pValueChangeStep} P Value";
            leftColumLayout.Add(addValueButton);

            Button substractValueButton = new Button(() =>
            {
                OnButtonClicked_ChangePValue(-_pValueChangeStep, PIDType.MainThrust);
            });
            substractValueButton.text = $"- {_pValueChangeStep} P Value";
            leftColumLayout.Add(substractValueButton);

            var rightColumLayout = new VisualElement();
            rightColumLayout.style.flexGrow = 0.5f;
            rightColumLayout.style.flexDirection = FlexDirection.Column;
            //rightColumLayout.style.width = 100;
            root.Add(rightColumLayout);

            Button startAutotuningButton = new Button(() =>
            {
                OnButtonClicked_AutoTuning();
            });
            startAutotuningButton.text = $"Start PID Autotuning";
            rightColumLayout.Add(startAutotuningButton);

            _autoTuningState = new Label("Autotuning: Inactive");
            _autoTuningState.style.flexGrow = 1;
            _autoTuningState.style.flexDirection = FlexDirection.Row;
            _autoTuningState.style.unityTextAlign = TextAnchor.MiddleCenter;
            rightColumLayout.Add(_autoTuningState);

            return root;
        }

        private void OnButtonClicked_ChangePValue(float valueChange, PIDType pidType)
        {
            _pidAgent?.ChangePValue(valueChange, pidType);
        }

        private void OnButtonClicked_AutoTuning()
        {
            _autoTuningState.text = "Autotuning: Active";
            _pidAgent?.StartAutoTuning(() => { _autoTuningState.text = "Autotuning: Done."; });
        }

        private string FormatPIDValues(string prefix, Vector3 values)
        {
            return $"{prefix}: [X {values.x.ToString("F2")}; Y: {values.y.ToString("F2")}; Z: {values.z.ToString("F2")}]";
        }

        private void UpdateValues(Vector3 velocity, Vector3 thrust, Vector3 sideThrust, Vector3 torque)
        {
            if (_currentVelocityLabel == null || _thrustPIDLabel == null || _sideThrustPIDLabel == null || _torquePIDLabel == null)
                return;

            _currentVelocityLabel.text = $"Current Velocity: [X {velocity.x.ToString("F2")}; Y: {velocity.y.ToString("F2")}; Z: {velocity.z.ToString("F2")}]";
            _thrustPIDLabel.text = FormatPIDValues("Thrust", thrust);
            _sideThrustPIDLabel.text = FormatPIDValues("Side Thrust", sideThrust);
            _torquePIDLabel.text = FormatPIDValues("Torque", torque);
        }

        public void Update()
        {
            var localVelocity = _pidAgent.transform.InverseTransformDirection(_pidAgent.CurrentVelocity);
            var localThrust = _pidAgent.transform.InverseTransformDirection(_pidAgent.Thrust);
            UpdateValues(localVelocity, _pidAgent.Thrust, _pidAgent.SideThrust, _pidAgent.Torque);
        }
    }
}