using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRK.Procedural
{
    #if UNITY_EDITOR
    using UnityEditor;
    public static class HelperMenues 
    {
        [MenuItem("Test/Test")]
        public static void Test()
        {
            Mesh sourceMesh =Object.Instantiate(Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh);
            if (sourceMesh == null)
            {
                Debug.LogWarning("Please select a Mesh asset in the Project window.");
                return;
            }

            CombineInstance[] combine = new CombineInstance[sourceMesh.subMeshCount];
            for (int i = 0; i < sourceMesh.subMeshCount; i++)
            {
                combine[i] = new CombineInstance
                {
                    mesh = sourceMesh,
                    subMeshIndex = i,
                    transform = Matrix4x4.identity
                };
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.name = sourceMesh.name + "_Combined";
            combinedMesh.CombineMeshes(combine, true, false); // true = merge into one submesh

            
            Selection.activeGameObject.GetComponent<MeshFilter>().mesh=combinedMesh;
        }
    }
    #endif
}
