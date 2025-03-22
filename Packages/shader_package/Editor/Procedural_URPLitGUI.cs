using UnityEditor;
using UnityEngine;

namespace PRK_Procedural.Editor
{
    public class Procedural_URPLitGUI : ShaderGUI
    {
        private bool main_data = true;
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
         
            Material target_ = materialEditor.target as Material;
            main_data = EditorGUILayout.Foldout(main_data, "Main Data", true);
            if (GUI.changed)
                EditorUtility.SetDirty(target_);
        }
    }
}
