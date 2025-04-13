using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRK.Procedural
{
    public class Testtttt : MonoBehaviour
    {
        [ContextMenu("Test")]
        void Test()
        {
            var ms = transform.GetComponent<MeshFilter>();
            var ms2=Instantiate(ms.sharedMesh);
            ms2.uv3 = new Vector2[ms2.uv.Length];
            
            CombineInstance[] combine = new CombineInstance[1];
            combine[0].mesh = ms2;
            combine[0].transform =transform.localToWorldMatrix;
            
            Mesh ms3=new Mesh();
            ms3.CombineMeshes(combine);
            ms.mesh = ms3;
        }
    }
}
