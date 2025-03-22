

using System;
using System.Linq;
using UnityEngine.Rendering;

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
            if (GUILayout.Button("AddUvs"))
            {
                AddUVs();
            }
            if (GUILayout.Button("print"))
            {
                PrinteMeshUVs();
            }
        }
        static void PrinteMeshUVs()
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

           Debug.Log(meshFilter.sharedMesh.uv3.Count());
        }
        static void AddUVs()
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

            Mesh mesh = MeshGenerator.GenerateMeshFromSource(meshFilter.sharedMesh);
            mesh.uv3=new Vector2[mesh.uv2.Length];
            for (int i = 0; i < mesh.uv2.Length; i++)
                mesh.uv3[i]=new Vector2(1f,0f);
            var uv = mesh.uv;

            for (int i = 0; i < uv.Length; i++)
                uv[i] = uv[i] * 2f;
            mesh.uv = uv;
            meshFilter.mesh = mesh;
            
        }
        


public static class MeshGenerator
{
    public static Mesh GenerateMeshFromSource(Mesh sourceMesh)
    {
        if (sourceMesh == null) return null;

        Mesh newMesh = new Mesh
        {
            vertices = sourceMesh.vertices,
            normals = sourceMesh.normals,
            tangents = sourceMesh.tangents,
            colors = sourceMesh.colors,
            boneWeights = sourceMesh.boneWeights,
            bindposes = sourceMesh.bindposes,
            name = sourceMesh.name + "_Copy"
        };

        // Copy UVs (Unity supports up to 8 UV sets)
        newMesh.uv = sourceMesh.uv;
        newMesh.uv2 = sourceMesh.uv2;
        newMesh.uv3 = sourceMesh.uv3;
        newMesh.uv4 = sourceMesh.uv4;
        newMesh.uv5 = sourceMesh.uv5;
        newMesh.uv6 = sourceMesh.uv6;
        newMesh.uv7 = sourceMesh.uv7;
        newMesh.uv8 = sourceMesh.uv8;

        // Copy submeshes and indices
        newMesh.subMeshCount = sourceMesh.subMeshCount;
        for (int i = 0; i < sourceMesh.subMeshCount; i++)
        {
            newMesh.SetTriangles(sourceMesh.GetTriangles(i), i);
        }

        // Copy blend shapes
        for (int i = 0; i < sourceMesh.blendShapeCount; i++)
        {
            string blendShapeName = sourceMesh.GetBlendShapeName(i);
            int frameCount = sourceMesh.GetBlendShapeFrameCount(i);
            for (int frame = 0; frame < frameCount; frame++)
            {
                float weight = sourceMesh.GetBlendShapeFrameWeight(i, frame);
                Vector3[] deltaVertices = new Vector3[sourceMesh.vertexCount];
                Vector3[] deltaNormals = new Vector3[sourceMesh.vertexCount];
                Vector3[] deltaTangents = new Vector3[sourceMesh.vertexCount];

                sourceMesh.GetBlendShapeFrameVertices(i, frame, deltaVertices, deltaNormals, deltaTangents);
                newMesh.AddBlendShapeFrame(blendShapeName, weight, deltaVertices, deltaNormals, deltaTangents);
            }
        }

        // Copy index format (important for large meshes)
        newMesh.indexFormat = sourceMesh.indexFormat;

        newMesh.RecalculateBounds();
        return newMesh;
    }
}

    }

}
