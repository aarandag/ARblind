using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

/// <summary>
/// Updates the _LookAtPoint shader parameter so it can visualize gaze location.
/// </summary>
public class GazeAnimator : MonoBehaviour
{

    [Tooltip("The Material with Transition to animate")]
    public Material SurfaceMaterial;

    void Start()
    {
        if (!GazeManager.Instance)
        {
            Debug.LogError("This script expects that you have a GazeManager component in your scene.");
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SurfaceMaterial)
        {
            SurfaceMaterial.SetVector("_LookAtPoint", GazeManager.Instance.HitPosition);
        }
    }

    void OnEnable()
    {
        SpatialMappingMaterialController.OnNewMaterialSelected += MaterialController_OnNewMaterialSelected;
    }

    void OnDisable()
    {
        SpatialMappingMaterialController.OnNewMaterialSelected -= MaterialController_OnNewMaterialSelected;
    }

    private void MaterialController_OnNewMaterialSelected(Material selectedMaterial)
    {
        SurfaceMaterial = selectedMaterial;
    }
}

