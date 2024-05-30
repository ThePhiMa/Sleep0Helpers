using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Sleep0.EditorExtensions
{
    //[CustomEditor(typeof(ManagedBehaviour), true)]
    public abstract class CustomEditorOverlayBase<T, U> : Editor
        where T : MonoBehaviour
        where U : Overlay, IUpdatableOverlay, new()
    {
        protected static bool _isInitiallised = false;
        protected T _object;
        protected U _overlay;

        //protected abstract void InitializeOverlay();

        private void OnEnable()
        {
            if (!_isInitiallised)
            {
                _object = (T)target;
                //_overlay = new U();

                _overlay = GameObjectExtensions.CreateInstance<U>(_object);  // I love black voodoo magic like this

                //InitializeOverlay();

                SceneView.AddOverlayToActiveView(_overlay);
                _isInitiallised = true;
            }
        }

        private void OnDisable()
        {
            if (_overlay is not null)
            {
                SceneView.RemoveOverlayFromActiveView(_overlay);
                _overlay = null;
                _isInitiallised = false;
            }
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            if (Selection.activeGameObject == ((T)target).gameObject)
            {
                _overlay.displayed = true;
                _overlay.Update();
            }
            else
            {
                _overlay.displayed = false;
            }
        }
    }
}