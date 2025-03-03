

namespace ShaderTutsTestData
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class PrintMeshUVs : EditorWindow
    {
        [MenuItem("Tools/Print Mesh UVs")]
        static void Init()
        {
            GetWindow<PrintMeshUVs>("Print Mesh UVs");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Print UVs"))
            {
                PrintUVs();
            }
        }

        static void PrintUVs()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.Log("No object selected.");
                return;
            }
        
            MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.Log("Selected object has no MeshFilter with a valid mesh.");
                return;
            }

            Mesh mesh = meshFilter.sharedMesh;
            for (int channel = 0; channel < 8; channel++)
            {
                if (mesh.HasVertexAttribute((UnityEngine.Rendering.VertexAttribute)(10 + channel)))
                {
                    List<Vector2> uvs = new List<Vector2>(mesh.vertexCount);
                    mesh.GetUVs(channel, uvs);
                    Debug.Log($"UV Channel {channel}:");
                    for (int i = 0; i < uvs.Count; i++)
                    {
                        Debug.Log($"UV[{i}] (Channel {channel}): {uvs[i]}");
                    }
                }
            }
        }
    }

}
