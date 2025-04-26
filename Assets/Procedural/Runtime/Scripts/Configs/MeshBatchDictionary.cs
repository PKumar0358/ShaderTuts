using System.Collections;
using System.Collections.Generic;
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
                if (mesh_idCounter.Count == 0)
                {
                    mesh_idCounter.Add(new []{0,source_mesh_.subMeshCount});
                }
                else
                {
                    int cnt = mesh_idCounter[mesh_idCounter.Count-1][0];// for prev element
                    mesh_idCounter.Add(new []{cnt+source_mesh_.subMeshCount,source_mesh_.subMeshCount});
                }
            }
            mesh_submesh_Map[source_mesh_][submesh_index] = submesh_;
        }

        public bool TryGetSubmeshes(Mesh source_mesh_,out Mesh[] submeshes_)
        {
            return mesh_submesh_Map.TryGetValue(source_mesh_,out submeshes_);
        }
    }
    
}
