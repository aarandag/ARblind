using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Unity.SpatialMapping;

/// <summary>
/// Uses the tap gesture to trigger the transition animation.
/// Uses Gaze hitpoint as center and Gaze direction for direction of the sweep depending on which transitiontype was selected for the shader.
/// </summary>
public class TapTransitionAnimator : MonoBehaviour, IInputClickHandler
{
    [Tooltip("The Material with Transition to animate")]
    public Material SurfaceMaterial;

    [Tooltip("Animation speed in meters per second")]
    public float Speed = 1.0f;

    [Tooltip("The maximum distance from the gaze hitpoint in meters.")]
    public float Range = 5.0f;

    private float offset = float.MaxValue;

    // Update is called once per frame
    void Update()
    {
        if (SurfaceMaterial && offset < Range)
        {         
            offset += Speed * Time.deltaTime;
            SurfaceMaterial.SetFloat("_TransitionOffset", offset - 1);
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        offset = 0;
        if (SurfaceMaterial)
        {
            SurfaceMaterial.SetVector("_Center", GazeManager.Instance.HitPosition);
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
