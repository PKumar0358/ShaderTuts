using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRK_Procedural
{
#if UNITY_EDITOR
    using UnityEditor;

    [ExecuteAlways]
    public partial class Procedural_Renderer
    {
        [System.NonSerialized]
        private bool m_EditModeRendererIsInitialized = false;
        [SerializeField] private bool m_UseEditorModeRenderer = false;
        protected virtual void OnEditorDrawCommandInitialized(){}
        protected virtual void OnEditorDrawCommand(){}
        protected virtual void OnEditorDrawCommandDisposed(){}
        
        protected virtual void OnEditorUpdate(){}

        private void Update()
        {
            if (m_UseEditorModeRenderer)
            {
                if (Application.isPlaying)
                {
                    if (m_EditModeRendererIsInitialized)
                    {
                        m_EditModeRendererIsInitialized = false;
                        OnEditorDrawCommandDisposed();
                    }
                }
                else
                {
                    if (!m_EditModeRendererIsInitialized)
                    {
                        m_EditModeRendererIsInitialized = true;
                        OnEditorDrawCommandInitialized();
                    }
                    if (m_EditModeRendererIsInitialized)
                        OnEditorDrawCommand();
                }
            }
            else
            {
                if (m_EditModeRendererIsInitialized)
                {
                    m_EditModeRendererIsInitialized = false;
                    OnEditorDrawCommandDisposed();
                }
            }

            OnEditorUpdate();
        }
    }
#endif
    public partial class Procedural_Renderer : MonoBehaviour
    {
       
        
    }
}
