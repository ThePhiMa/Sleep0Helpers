using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityToolbarExtender;

namespace Sleep0.EditorExtensions
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Bold
            };
        }
    }

    [InitializeOnLoad]
    public class ToolbarExtensions : MonoBehaviour
    {
        static ToolbarExtensions()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            if (GUILayout.Button(new GUIContent("C", "Compile all scripts"), ToolbarStyles.commandButtonStyle))
            {
                CompilationPipeline.RequestScriptCompilation();
            }

            if (GUILayout.Button(new GUIContent("B", "Build the project"), ToolbarStyles.commandButtonStyle))
            {
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds", BuildTarget.StandaloneWindows64, BuildOptions.None);
            }

            if (GUILayout.Button(new GUIContent("P", "Open the project settings"), ToolbarStyles.commandButtonStyle))
            {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            }

            if (Selection.activeTransform == null) return;

            GUILayout.Space(10);
            GUILayout.Label("Transform", GUILayout.MinWidth(60));

            Transform transform = Selection.activeTransform;

            if (GUILayout.Button(new GUIContent("RP", "Reset Position to Vector.zero"), ToolbarStyles.commandButtonStyle))
            {
                transform.position = Vector3.zero;
            }

            if (GUILayout.Button(new GUIContent("RR", "Reset Rotation to Quaternion.identity"), ToolbarStyles.commandButtonStyle))
            {
                transform.rotation = Quaternion.identity;
            }

            if (GUILayout.Button(new GUIContent("RS", "Reset Scale to Vector.one"), ToolbarStyles.commandButtonStyle))
            {
                transform.localScale = Vector3.one;
            }
        }
    }
}