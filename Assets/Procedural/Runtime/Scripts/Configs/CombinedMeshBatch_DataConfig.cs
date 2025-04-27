using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public CopyTrs(Transform source_,MeshFilter meshFilter,Transform root_,MeshBatchDictionary _MeshBatchDictionary)
        {
            Position = source_.position;
            Rotation = source_.rotation;
            Scale = source_.lossyScale;
            GameObject g = GameObject.Instantiate(source_.gameObject) as GameObject;
            mesh = Mesh.Instantiate(meshFilter.sharedMesh);
            g.name = $"Copy_Of_{source_.name}";
            
            if (meshFilter.sharedMesh.subMeshCount > 1)
            {
                g.transform.position = Vector3.zero;
                g.transform.rotation = Quaternion.identity;
                g.transform.localScale = Vector3.one;
                trsCopy = g.transform;
           
                
                mesh.name=meshFilter.sharedMesh.name;
                var meshRenderer = trsCopy.GetComponent<MeshRenderer>();
                var sharedMaterials = meshRenderer != null ? meshRenderer.sharedMaterials : null;
            
                worldMatrix = trsCopy.localToWorldMatrix;
                vertices = mesh.vertices;
                normals = mesh.normals;
                worldVertices = vertices.Select(v => worldMatrix.MultiplyPoint3x4(v)).ToArray();
                worldNormals = normals.Length == vertices.Length
                    ? normals.Select(n => worldMatrix.MultiplyVector(n)).ToArray()
                    : null;
                    
                Mesh[] new_submeshes = new Mesh[meshFilter.sharedMesh.subMeshCount];
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    new_submeshes[i] = Split(i);
                    new_submeshes[i].name = $"{i}__Of_{meshFilter.sharedMesh.name}";
                    GameObject gg=GameObjectForNewSubmesh(new_submeshes[i],sharedMaterials,i);
                    gg.transform.SetParent(root_,true);
                }
                if (!_MeshBatchDictionary.TryGetSubmeshes(meshFilter.sharedMesh, out Mesh[] submeshes_))
                {
                    _MeshBatchDictionary.AddNewSubmeshes(meshFilter.sharedMesh, new_submeshes);
                    _MeshBatchDictionary.PrintSubmeshIds(meshFilter.sharedMesh);
                }
                Object.DestroyImmediate(g);
                
            }
            else if (meshFilter.sharedMesh.subMeshCount == 1)
            {
                g.transform.position = Position;
                g.transform.rotation = Rotation;
                g.transform.localScale = Scale;
                g.transform.SetParent(root_,true);
                g.GetComponent<MeshFilter>().mesh = mesh;
                if (!_MeshBatchDictionary.TryGetSubmeshes(meshFilter.sharedMesh, out Mesh[] submeshes_))
                {
                    _MeshBatchDictionary.Add(meshFilter.sharedMesh, mesh,0);
                    _MeshBatchDictionary.PrintSubmeshIds(meshFilter.sharedMesh);
                }
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
           // newMesh.name=
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
    }

    

    public static class Extensions
    {
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
            MeshBatchDictionary _MeshBatchDictionary = new MeshBatchDictionary();
            Transform selected=menuCommand.context as Transform;
            var renders = selected.GetComponentsInChildren<MeshRenderer>();
            GameObject splitObjectsParent=new GameObject("SplitObjectsParent");
            splitObjectsParent.transform.position=Vector3.zero;
            splitObjectsParent.transform.rotation=Quaternion.identity;
            splitObjectsParent.transform.localScale=Vector3.one;
           // Dictionary<Mesh,List<>>
            foreach (var x in renders)
            {
                if (x.TryGetComponent(out MeshFilter filter))
                {
                    if (filter.sharedMesh != null)
                    {
                        CopyTrs trsCopy = new CopyTrs(x.transform, filter,splitObjectsParent.transform,_MeshBatchDictionary);
                    }
                }
            }

            var t=splitObjectsParent.AddComponent<TestData>();
            _MeshBatchDictionary.FillSubmeshes(out t.meshes,out t.submeshes);
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
