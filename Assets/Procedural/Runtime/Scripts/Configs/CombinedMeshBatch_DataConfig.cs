using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PRK.Procedural
{
    #if UNITY_EDITOR
    using System.Linq;
    using System.IO;
    using UnityEditor;

    public class CopyTrs
    {
        public Transform trsCopy;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public Matrix4x4 worldMatrix;
        public Vector3[]vertices;
        public Vector3[] normals;
        public Vector3[]worldVertices;
        public Vector3[] worldNormals;
        private Mesh mesh;

        public CopyTrs(MeshFilter sourceFilter_,out Mesh[] split_MeshArray_)
        {
            Transform source_ = sourceFilter_.transform;
            Position = source_.position;
            Rotation = source_.rotation;
            Scale = source_.lossyScale;
            
            mesh = Mesh.Instantiate(sourceFilter_.sharedMesh);
            mesh.name=sourceFilter_.sharedMesh.name;
            
            GameObject g = GameObject.Instantiate(source_.gameObject) as GameObject;
            g.name = $"Copy_Of_{source_.name}";
            g.transform.ResetTransform();
            trsCopy= g.transform;
            
            worldMatrix = trsCopy.localToWorldMatrix;
            vertices = mesh.vertices;
            normals = mesh.normals;
            worldVertices = vertices.Select(v => worldMatrix.MultiplyPoint3x4(v)).ToArray();
            worldNormals = normals.Length == vertices.Length
                ? normals.Select(n => worldMatrix.MultiplyVector(n)).ToArray()
                : null;
            split_MeshArray_ = new Mesh[sourceFilter_.sharedMesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                split_MeshArray_[i] = Split(i);
                split_MeshArray_[i].name = $"Submesh_{i}__Of__{sourceFilter_.sharedMesh.name}";
            }
            Object.DestroyImmediate(g);
            Object.DestroyImmediate(mesh);
            
        }
        
        public CopyTrs(Transform source_,MeshFilter sourceFilter_,Transform root_)
        {
            Position = source_.position;
            Rotation = source_.rotation;
            Scale = source_.lossyScale;
            GameObject g = GameObject.Instantiate(source_.gameObject) as GameObject;
            g.name = $"Copy_Of_{source_.name}";
            
            mesh = Mesh.Instantiate(sourceFilter_.sharedMesh);
            
            if (sourceFilter_.sharedMesh.subMeshCount > 1)
            {
                g.transform.position = Vector3.zero;
                g.transform.rotation = Quaternion.identity;
                g.transform.localScale = Vector3.one;
                trsCopy = g.transform;
           
                
                mesh.name=sourceFilter_.sharedMesh.name;
                
                var meshRenderer = trsCopy.GetComponent<MeshRenderer>();
                var sharedMaterials = meshRenderer != null ? meshRenderer.sharedMaterials : null;
            
                worldMatrix = trsCopy.localToWorldMatrix;
                vertices = mesh.vertices;
                normals = mesh.normals;
                worldVertices = vertices.Select(v => worldMatrix.MultiplyPoint3x4(v)).ToArray();
                worldNormals = normals.Length == vertices.Length
                    ? normals.Select(n => worldMatrix.MultiplyVector(n)).ToArray()
                    : null;
                    
                Mesh[] new_submeshes = new Mesh[sourceFilter_.sharedMesh.subMeshCount];
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    new_submeshes[i] = Split(i);
                    new_submeshes[i].name = $"{i}__Of_{sourceFilter_.sharedMesh.name}";
                    GameObject gg=GameObjectForNewSubmesh(new_submeshes[i],sharedMaterials,i);
                    gg.transform.SetParent(root_,true);
                }
              
                Object.DestroyImmediate(g);
                
            }
            else if (sourceFilter_.sharedMesh.subMeshCount == 1)
            {
                g.transform.position = Position;
                g.transform.rotation = Rotation;
                g.transform.localScale = Scale;
                g.transform.SetParent(root_,true);
                g.GetComponent<MeshFilter>().mesh = mesh;
            }
        }

        private Mesh Split(int i)
        {
            var indices = mesh.GetTriangles(i);
            var newMesh = new Mesh
            {
                vertices = worldVertices,
                triangles = indices,
                uv = mesh.uv
            };
            if (worldNormals != null) newMesh.normals = worldNormals;
            if (mesh.tangents != null && mesh.tangents.Length == mesh.vertexCount)
                newMesh.tangents = mesh.tangents;
            return newMesh;
        }

        public GameObject GameObjectForNewSubmesh(Mesh newMesh,Material[]sharedMaterials,int i)
        {
            string meshName = $"SplitMesh_[{i}]_{mesh.name}";
            var go = new GameObject(meshName);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.mesh = newMesh;
            if (sharedMaterials != null && i < sharedMaterials.Length)
                mr.sharedMaterial = sharedMaterials[i];
            go.transform.position=Position;
            go.transform.rotation = Rotation;
            go.transform.localScale = Scale;
            return go;
        }

        public GameObject NewGameObject(string gameName_ = "New GameObject")
        {
            var go = new GameObject(gameName_);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go;
        }
    }

    

    public static class Extensions
    {
        public static GameObject GetNewGameObject(this Transform t, string gameName_ = "New GameObject")
        {
            var go = new GameObject(gameName_);
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            go.transform.localScale = t.lossyScale;
            return go;
        }
        public static void ResetTransform(this Transform trs)
        {
            Transform parent_= trs.parent;
            if (parent_ != null)
                trs.parent = null;
            trs.position = Vector3.zero;
            trs.rotation = Quaternion.identity;
            trs.localScale = Vector3.one;
            if(trs.parent != null)
                trs.SetParent(parent_,true);
        }
        public static Vector4 GetScaleOffset(this Material mat_,string Name_ )
        {
            Vector4 scaleoffset = new Vector4(1, 1, 0, 0);
            Vector2 v = mat_.GetTextureScale(Name_);
            scaleoffset.x = v.x;
            scaleoffset.y = v.y;
            v = mat_.GetTextureOffset(Name_);
            scaleoffset.z = v.x;
            scaleoffset.w = v.y;
            return scaleoffset;
        }


        public static void ModiyMeshUVsBasedOnId(this List<Mesh>sourceArray_,int index_)
        {
            Mesh ms=sourceArray_[index_];
            List<Vector3>newUVs = new List<Vector3>();
            for (int i = 0; i < ms.uv.Length; i++)
            {
                Vector3 v = new Vector3(ms.uv[i].x,ms.uv[i].y,index_);
                newUVs.Add(v);
            }
            ms.SetUVs(0,newUVs);
            sourceArray_[index_] = ms;
        }
        public static void GetMeshSubmeshes(this Transform parent_,out Renderer[]renderers,out Dictionary<Mesh,Transform>all_meshes,out Dictionary<Mesh,int[]>mesh_submesh_ids)
        {
            all_meshes=new Dictionary<Mesh,Transform>();
            renderers=parent_.GetComponentsInChildren<Renderer>();
            foreach (var x in renderers)
            {
                if (x.TryGetComponent(out MeshFilter mf))
                {
                    if (mf.sharedMesh != null)
                    {
                        if(!all_meshes.ContainsKey(mf.sharedMesh))
                            all_meshes.Add(mf.sharedMesh,x.transform);
                        else
                            continue;
                    }
                }
            }
            mesh_submesh_ids=new Dictionary<Mesh,int[]>();//
            int submeshID_ = 0;
            foreach (var x in all_meshes)
            {
                int[] ids = new int[x.Key.subMeshCount];
                for (int i = 0; i < x.Key.subMeshCount; i++)
                {
                    ids[i] = submeshID_;
                    submeshID_++;
                }
                mesh_submesh_ids.Add(x.Key,ids);
            }
        }

        public static void New_SplitSubmehes(this Mesh mesh_,out Mesh[]splitArray_)
        {
            splitArray_ = new Mesh[mesh_.subMeshCount];
        }
        public  static int AddToDictionary<T>(this Dictionary<T, int> dict, T ke)
        {
            int id = -1;
            if (dict.ContainsKey(ke))
                id = dict[ke];
            else
            {
                id = dict.Count;
                dict.Add(ke, id);
            }
            return id;
        }
    }
    public partial class CombinedMeshBatch_DataConfig
    {
        private const string localFolderPath = "Assets/Experimental/CombinedMeshData";
      
        [MenuItem("CONTEXT/Transform/Split Submeshes")]
        public static void SplitSubmeshes(MenuCommand menuCommand)
        {
            GameObject splitObjectsParent=new GameObject("SplitObjectsParent");
            splitObjectsParent.transform.position=Vector3.zero;
            splitObjectsParent.transform.rotation=Quaternion.identity;
            splitObjectsParent.transform.localScale=Vector3.one;
         //   MeshBatchDictionary _MeshBatchDictionary = new MeshBatchDictionary();
            Transform selected_tr=menuCommand.context as Transform;
            selected_tr.GetMeshSubmeshes(out var renderers_,out var mesh_submeshes,out var mesh_submesh_ids);
            List<Mesh>newMeshes=new List<Mesh>();
            foreach (var x in mesh_submeshes)
            {
                CopyTrs cpy = new CopyTrs(x.Value.GetComponent<MeshFilter>(),out var tmparr);
                newMeshes.AddRange(tmparr);
            }

            for (int i = 0; i < newMeshes.Count; i++)
            {
                newMeshes.ModiyMeshUVsBasedOnId(i);
            }
            
            var t=selected_tr.gameObject.AddComponent<TestData>();
            t.submeshes=newMeshes.ToArray();
            List<CombineInstance> cmb = new List<CombineInstance>();
            int ii = 0;
            foreach (var x in renderers_)
            {
                if (x.TryGetComponent(out MeshFilter mf))
                {
                    if (mf.sharedMesh != null)
                    {
                        int c=mf.sharedMesh.subMeshCount;
                        for (int i = 0; i < c; i++)
                        {
                            CombineInstance cmb1=new CombineInstance();
                            int mesh_index=mesh_submesh_ids[mf.sharedMesh][i];
                            cmb1.mesh=newMeshes[mesh_index];
                            cmb1.transform = x.transform.localToWorldMatrix;
                            cmb.Add(cmb1);
                            ii++;
                            /*GameObject g = x.transform.GetNewGameObject($"Copy_{x.name}");
                            var mf2=g.AddComponent<MeshFilter>();
                            var rndr=g.AddComponent<MeshRenderer>();
                            mf2.mesh = newMeshes[mesh_index];
                            rndr.material = x.sharedMaterials[i];
                            g.transform.SetParent(splitObjectsParent.transform,true);*/
                        }
                    }
                }
            }
            Mesh mesh = new Mesh();
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(cmb.ToArray(),true,true);
            splitObjectsParent.AddComponent<MeshFilter>().mesh=mesh;
            splitObjectsParent.AddComponent<MeshRenderer>();
            /*var renders = selected.GetComponentsInChildren<MeshRenderer>();
            GameObject splitObjectsParent=new GameObject("SplitObjectsParent");
            splitObjectsParent.transform.position=Vector3.zero;
            splitObjectsParent.transform.rotation=Quaternion.identity;
            splitObjectsParent.transform.localScale=Vector3.one;

            foreach (var x in renders)
            {
                if (x.TryGetComponent(out MeshFilter filter))
                {
                    if (filter.sharedMesh != null)
                    {
                        CopyTrs trsCopy = new CopyTrs(x.transform, filter,splitObjectsParent.transform);
                    }
                }
            }

            var t=splitObjectsParent.AddComponent<TestData>();
            _MeshBatchDictionary.FillSubmeshes(out t.meshes,out t.submeshes);*/
        }

       
        
     
        
        [MenuItem("CONTEXT/Transform/Select/ Multiple Material meshes")]
        public static void SelectMutliMaterialMeshes(MenuCommand menuCommand)
        {
            Transform t=menuCommand.context as Transform;
            var meshes = t.gameObject.GetComponentsInChildren<MeshRenderer>();
            HashSet<GameObject>group = new HashSet<GameObject>();
            foreach (var x in meshes)
            {
                if (x.sharedMaterials.Length > 1)
                {
                    if (!group.Contains(x.gameObject))
                    {
                        group.Add(x.gameObject);
                        Debug.Log($"{x.sharedMaterials.Length}",x);
                    }
                }
            }

          
            Selection.objects = group.ToArray();
        }
        [MenuItem("CONTEXT/Transform/Procedural/Create CombinedMeshBatch DataConfig from Children")]
        public static void CollectMeshDataFromChildren(MenuCommand menuCommand)
        {
            Transform t=menuCommand.context as Transform;
            string projectPath = Directory.GetCurrentDirectory();
            string fullFolderPath = $"{projectPath}/{localFolderPath}";
            if(!Directory.Exists(fullFolderPath))
                Directory.CreateDirectory(fullFolderPath);
            string dataFilePath = $"{localFolderPath}/{t.name}__CombinedMeshBatch_DataConfig.asset";
            CombinedMeshBatch_DataConfig o = AssetDatabase.LoadAssetAtPath<CombinedMeshBatch_DataConfig>(dataFilePath);
            if (o != null)
                AssetDatabase.DeleteAsset(dataFilePath);
            o = new CombinedMeshBatch_DataConfig();
            AssetDatabase.CreateAsset(o, dataFilePath);
            o.CollectMeshDataFromChildren(t);
            AssetDatabase.Refresh();
            Selection.activeObject = o;
            EditorGUIUtility.PingObject(o);
        }

        void CollectMeshDataFromChildren(Transform parent_)
        {
            var renderers = parent_.GetComponentsInChildren<MeshRenderer>();
            Dictionary<Mesh, int> mehs_Idict=new Dictionary<Mesh, int>();
            HashSet<Material>materialsTemp=new HashSet<Material>();
            Dictionary<TexNames,List<Texture2D>>texture_Dict=new Dictionary<TexNames, List<Texture2D>>();
            batch_Tex_IDs=new List<Batch_Tex_IDs>();
            texture_Dict.Add(TexNames._BaseMap, new List<Texture2D>());
            texture_Dict.Add(TexNames._BumpMap, new List<Texture2D>());
            texture_Dict.Add(TexNames._MetallicGlossMap, new List<Texture2D>());
            texture_Dict.Add(TexNames._SpecGlossMap, new List<Texture2D>());
            texture_Dict.Add(TexNames._DetailAlbedoMap, new List<Texture2D>());
            texture_Dict.Add(TexNames._DetailMask, new List<Texture2D>());
            texture_Dict.Add(TexNames._DetailNormalMap, new List<Texture2D>());
            texture_Dict.Add(TexNames._OcclusionMap, new List<Texture2D>());
            texture_Dict.Add(TexNames._EmissionMap, new List<Texture2D>());
         
            
            int count = renderers.Length;
            
            for (int i = 0; i < count; i++)
            {
                var x = renderers[i];
                if (x.TryGetComponent(out MeshFilter filter))
                {
                    if (filter.sharedMesh != null)
                    {
                        if (x.sharedMaterials.Length == 1)
                        {
                            if (x.sharedMaterials[0].shader.name == "Universal Render Pipeline/Lit")
                            {
                                int mid = mehs_Idict.AddToDictionary(filter.sharedMesh);
                                if (!materialsTemp.Contains(x.sharedMaterials[0]))
                                {
                                    Batch_Tex_IDs b1=new Batch_Tex_IDs(texture_Dict,x.sharedMaterials[0]);
                                    batch_Tex_IDs.Add(b1);
                                    materialsTemp.Add(x.sharedMaterials[0]);
                                }
                            }
                        }
                        else if (x.sharedMaterials.Length == 0||x.sharedMaterials.Length > 1)
                        {
                            Debug.Log($"mesh not valid material count {x.sharedMaterials.Length}",x);
                        }
                    }
                }
            }

            material_list = new List<Material>(materialsTemp);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }

    public partial class Batch_Tex_IDs
    {
        public Batch_Tex_IDs()
        {
            
        }
        public Batch_Tex_IDs( Dictionary<TexNames,List<Texture2D>>dict_,Material material_)
        {
            SetTex_ID(TexNames._BaseMap,dict_,ref tex_main,material_);
            SetTex_ID(TexNames._BumpMap,dict_,ref tex_normal_map,material_);
            SetTex_ID(TexNames._MetallicGlossMap,dict_,ref tex_metalic_map,material_);
            SetTex_ID(TexNames._SpecGlossMap,dict_,ref tex_specular_map,material_);
            SetTex_ID(TexNames._DetailAlbedoMap,dict_,ref tex_detail_map,material_);
            SetTex_ID(TexNames._DetailMask,dict_,ref tex_detail_mask_map,material_);
            SetTex_ID(TexNames._DetailNormalMap,dict_,ref tex_detail_normal_map,material_);
            SetTex_ID(TexNames._OcclusionMap,dict_,ref tex_occulussion_map,material_);
            SetTex_ID(TexNames._EmissionMap,dict_,ref tex_emission_map,material_);
        }

        private void SetTex_ID(TexNames txName,Dictionary<TexNames,List<Texture2D>>dict_,ref int idRef,Material mat_)
        {
            Texture2D tx=mat_.GetTexture($"{txName}")as Texture2D;
            if (tx != null)
            {
                idRef =dict_[txName].Count;
                dict_[txName].Add(tx);
            }
        }
    }
    #endif

    [CreateAssetMenu]
    public partial class CombinedMeshBatch_DataConfig : CombinedMeshBatch_DataConfigBase
    {
        
    }
}
