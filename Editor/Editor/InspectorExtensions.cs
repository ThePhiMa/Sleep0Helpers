using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameObject))]
public class InspectorExtensions : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Transform Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Reset Transform"))
        {
            PerformActionWithUndo("Reset Transform", ResetTransform);
        }
        if (GUILayout.Button("Reset Position"))
        {
            PerformActionWithUndo("Reset Position", ResetPosition);
        }
        if (GUILayout.Button("Reset Rotation"))
        {
            PerformActionWithUndo("Reset Rotation", ResetRotation);
        }
        if (GUILayout.Button("Reset Scaling"))
        {
            PerformActionWithUndo("Reset Scaling", ResetScale);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void PerformActionWithUndo(string actionName, System.Action<Transform> action)
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, actionName);     // Record the state of the object before changes
            action.Invoke(go.transform);                     // Perform the action
            EditorUtility.SetDirty(go.transform);            // Mark the object as dirty to ensure changes are saved
            PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
        }
    }

    private void ResetTransform(Transform transform)
    {
        transform.transform.position = Vector3.zero;
        transform.transform.rotation = Quaternion.identity;
        transform.transform.localScale = Vector3.one;
    }

    private void ResetPosition(Transform transform)
    {
        transform.transform.position = Vector3.zero;
    }

    private void ResetRotation(Transform transform)
    {
        transform.transform.rotation = Quaternion.identity;
    }

    private void ResetScale(Transform transform)
    {
        transform.transform.localScale = Vector3.one;
    }
}