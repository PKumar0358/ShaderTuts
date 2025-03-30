using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombineTool : MonoBehaviour
{
    [SerializeField]
    private MeshFilter[] filters;
    [ContextMenu("Combine")]
    public void CombineMesh()
    {
        Matrix4x4[]trss = new Matrix4x4[filters.Length];
        Mesh[]mss=new Mesh[filters.Length];
        for (int i = 0; i < filters.Length; i++)
        {
            Transform t=filters[i].transform;
            mss[i]=filters[i].sharedMesh;
            trss[i] = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        }
        
        GameObject go = new GameObject();
        MeshFilter filter=go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        filter.sharedMesh = CombineMeshes(mss,trss);
    }
    
    public static Mesh CombineMeshes(Mesh[] meshes, Matrix4x4[] transforms)
    {
        if (meshes == null || meshes.Length == 0) return null;

        int totalSubMeshCount = 0;
        foreach (var mesh in meshes) totalSubMeshCount += mesh.subMeshCount;

        CombineInstance[] combineInstances = new CombineInstance[totalSubMeshCount];
        int combineIndex = 0;

        for (int i = 0; i < meshes.Length; i++)
        {
            if (meshes[i] == null) continue;

            for (int j = 0; j < meshes[i].subMeshCount; j++)
            {
                combineInstances[combineIndex] = new CombineInstance
                {
                    mesh = meshes[i],
                    subMeshIndex = j,
                    transform = transforms != null && transforms.Length > i ? transforms[i] : Matrix4x4.identity
                };
                combineIndex++;
            }
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances, false, true);
        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();

        return combinedMesh;
    }
}
