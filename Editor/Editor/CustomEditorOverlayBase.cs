using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Sleep0.EditorExtensions
{
    /// <summary>
    /// Base class for custom editors that display an overlay in the scene view when the GameObject is selected.
    /// </summary>
    /// <typeparam name="T">Type of the object for the custom editor</typeparam>
    /// <typeparam name="U">Type of the custom overlay to display</typeparam>
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