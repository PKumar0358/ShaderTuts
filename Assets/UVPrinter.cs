#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class UVPrinter : EditorWindow
{
    [MenuItem("Tools/Print Mesh UVs")]
    public static void ShowWindow()
    {
        GetWindow<UVPrinter>("Mesh UV Printer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a Mesh to Print UVs", EditorStyles.boldLabel);

        if (GUILayout.Button("Print UVs"))
        {
            PrintUVs();
        }
    }

    private void PrintUVs()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            Debug.LogError("No object selected!");
            return;
        }

        MeshFilter meshFilter = selectedObj.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("No Mesh found on selected object!");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;
        Vector2[] uvs = mesh.uv;

        Debug.Log($"UVs for Mesh: {mesh.name} (Total: {uvs.Length})");

        for (int i = 0; i < uvs.Length; i++)
        {
            Debug.Log($"UV[{i}]: {uvs[i]}");
        }
    }
}
#endif