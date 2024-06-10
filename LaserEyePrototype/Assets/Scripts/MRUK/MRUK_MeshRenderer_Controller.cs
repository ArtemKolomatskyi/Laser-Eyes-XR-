using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Serialization;

public class MRUK_MeshRenderer_Controller : MonoBehaviour
{
    [SerializeField] private Material _visibleRoomMeshMaterial;

    private void Awake()
    {
        #if UNITY_EDITOR
            foreach (var effectMesh in GameObject.FindObjectsOfType<EffectMesh>())
            {
                effectMesh.MeshMaterial = _visibleRoomMeshMaterial;
            }
        #endif
    }
}
