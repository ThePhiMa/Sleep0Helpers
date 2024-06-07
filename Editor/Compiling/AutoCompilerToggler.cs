using UnityEditor;
using UnityEngine;

namespace Sleep0.EditorExtensions
{
    public class AutoCompilerToggler
    {
        [MenuItem("Tools/Toggle AutoCompiler #&a")]
        public static void ToggleAutcompiler()
        {
            int status = EditorPrefs.GetInt("kAutoRefresh");
            if (status == 1)
            {
                EditorPrefs.SetInt("kAutoRefresh", 0);
                Debug.Log("AutoCompiler is now OFF");
            }
            else
            {
                EditorPrefs.SetInt("kAutoRefresh", 1);
                Debug.Log("AutoCompiler is now ON");
            }
        }
    }
}