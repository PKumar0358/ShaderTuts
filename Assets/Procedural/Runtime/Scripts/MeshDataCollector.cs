using System.Collections.Generic;

using UnityEngine;

namespace PRK.Procedural
{
    public class MeshDataCollector : MonoBehaviour
    {
       
        [SerializeField] 
        private RenderBatchData_Config batch_Config;

        [ContextMenu("SaveBatchData")]
        void SaveBatchData()
        {
            batch_Config?.SaveBatchData(transform);
        }
        
 
        
 
        
 

        
   
       
    }
}
