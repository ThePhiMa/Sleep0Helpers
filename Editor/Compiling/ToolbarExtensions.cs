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
        }
    }
}