#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[System.Serializable]
public class TextureAtlasCreator : EditorWindow
{
    
    private SerializedObject serializedObject;
    private static string[] _properties=new string[]{"width","height","rows","cols","format","textures"};
    private SerializedProperty[] dataProperties;
    
    [SerializeField]
    public int width;
    [SerializeField]
    public int height;
    [SerializeField]
    public int rows;
    [SerializeField]
    public int cols;
    [SerializeField] 
   
   
    public TextureFormat format;
    public List<Texture2D> textures;
    
    [MenuItem("Tools/Texture Atlas Creator")]
    public static void ShowWindow()
    {
        GetWindow<TextureAtlasCreator>("Texture Atlas Creator");
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        dataProperties=new SerializedProperty[_properties.Length];
        for (int i = 0; i < _properties.Length; i++)
            dataProperties[i]=serializedObject.FindProperty(_properties[i]);
       
    }

    private void OnGUI()
    {
       // EditorGUILayout.IntField("",)
        width=EditorGUILayout.IntField("Width", width);
        height=EditorGUILayout.IntField("height", height);
        rows=EditorGUILayout.IntField("rows", rows);
        cols=EditorGUILayout.IntField("cols", cols);
        EditorGUILayout.PropertyField(dataProperties[4], true);
        EditorGUILayout.PropertyField(dataProperties[5], true);
        if (GUILayout.Button("Generate Atlas"))
        {
            PackToAtlas("TestName",textures.ToArray(),width,height,rows,cols,format);
        }
         serializedObject.ApplyModifiedProperties(); 
    }
    
    public static void PackToAtlas(string Name_,Texture2D[]textures,int width,int height,int rows,int cols,TextureFormat format)
    {
        format=textures[0].format;
        Texture2D atlas = new Texture2D(width, height, format, true);
        atlas.PackTextures(textures, 0, 8192);
        atlas.Apply();
        byte[] pngData = atlas.EncodeToPNG();
        string savePath = Path.Combine(Application.dataPath, Name_ + ".png");
        File.WriteAllBytes(savePath, pngData);
        
        AssetDatabase.Refresh();
        Debug.Log("Atlas saved to: " + savePath);
    }
}

#endif