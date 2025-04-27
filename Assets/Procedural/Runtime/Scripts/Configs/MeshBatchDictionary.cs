using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PRK.Procedural
{
    public class MeshBatchDictionary
    {
        private List<Mesh> mesh_Keys;
        private Dictionary<Mesh, Mesh[]> mesh_submesh_Map;
        
        /// <summary>
        /// this will keep track the sum of previous element's count, and self count
        /// </summary>
        private List<int[]> mesh_idCounter;

        public MeshBatchDictionary()
        {
            mesh_Keys = new List<Mesh>();
            mesh_submesh_Map = new Dictionary<Mesh, Mesh[]>();
            mesh_idCounter = new List<int[]>();
        }

        public void Add(Mesh source_mesh_,Mesh submesh_,int submesh_index)
        {
            if(!mesh_submesh_Map.ContainsKey(source_mesh_))
            {
                Mesh[]tempArray = new Mesh[source_mesh_.subMeshCount];
                mesh_submesh_Map.Add(source_mesh_, tempArray);
                mesh_Keys.Add(source_mesh_);
                AddToMeshIdCounter(tempArray.Length);
            }
            mesh_submesh_Map[source_mesh_][submesh_index] = submesh_;
        }
        public void AddNewSubmeshes(Mesh source_mesh_,Mesh[] submeshes_)
        {
            mesh_submesh_Map.Add(source_mesh_, submeshes_);
            mesh_Keys.Add(source_mesh_);
            AddToMeshIdCounter(submeshes_.Length);
        }

        public void PrintSubmeshIds(Mesh mesh_)
        {
            int idx = mesh_Keys.IndexOf(mesh_);
            int i = 0;
            foreach (var x in mesh_submesh_Map[mesh_])
            {
                Debug.Log($"submesh count {mesh_.subMeshCount}  {idx}  {mesh_idCounter[idx][0]} mesh {mesh_.name}    {mesh_idCounter[idx][0]+i}",mesh_);
                i++;
            }
        }
        private void AddToMeshIdCounter(int submeshCount_)
        {
            if (mesh_idCounter.Count == 0)
                mesh_idCounter.Add(new []{0,submeshCount_});
            else
            {
                int cnt = mesh_idCounter[mesh_idCounter.Count-1][0]+mesh_idCounter[mesh_idCounter.Count-1][1];// 
                mesh_idCounter.Add(new []{cnt,submeshCount_});
            }
        }

        public bool TryGetSubmeshes(Mesh source_mesh_,out Mesh[] submeshes_)
        {
            return mesh_submesh_Map.TryGetValue(source_mesh_,out submeshes_);
        }

        public void FillSubmeshes(out Mesh[] sourceContainer_,out Mesh[] submeshContainer_)
        {
            sourceContainer_ = mesh_submesh_Map.Keys.ToArray();
            List<Mesh>submeshes_ = new List<Mesh>();
            int c=mesh_Keys.Count;
            Debug.Log($"{c}   == {mesh_submesh_Map.Count}");
            for (int i = 0; i < c; i++)
            {
                var arr= mesh_submesh_Map[mesh_Keys[i]];
                int start_i = mesh_idCounter[i][0];
                for (int k = 0; k < arr.Length; k++)
                {
                    int id=start_i+k;
                    Debug.Log($"---- {id} ");
                    submeshes_.Add(arr[k]);
                }
            }
            submeshContainer_=submeshes_.ToArray();
        }
    }
    
}
