    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        #ifdef unity_ObjectToWorld
            #undef unity_ObjectToWorld
        #endif

        #ifdef unity_WorldToObject
            #undef unity_WorldToObject
        #endif
    #endif

void Procedural_DataSetup()
{
    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
    unity_ObjectToWorld=_Instance_Data_Buffer[unity_InstanceID].objToWorld;
    unity_WorldToObject=_Instance_Data_Buffer[unity_InstanceID].worldToObj;
    #endif
}
