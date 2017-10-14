using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Continuously animates the shader variables of the specified Material to make it sweep the room.
/// Uses Gaze hitpoint as center of the sweep.
/// </summary>
public class GazeTransitionAnimator : MonoBehaviour {

    [Tooltip("The Material with Transition to animate")]
    public Material SurfaceMaterial;

    [Tooltip("Animation speed in meters per second")]
    public float Speed = 1.0f;

    [Tooltip("The maximum distance from the gaze hitpoint in meters.")]
    public float Range = 5.0f;

    [Tooltip("Ping pong the direction of the animation")]
    public bool PingPong = false;

    protected bool Updating = true;
    protected bool Looping = true;

    void Start ()
    {
        if (!GazeManager.Instance)
        {
            Debug.LogError("This script expects that you have a GazeManager component in your scene.");
            Destroy(this);
        }
    }

    private float previousOffset = 0;
    private bool previousGoingDown = false;
	// Update is called once per frame
	void Update ()
    {
        if (SurfaceMaterial && Updating)
        {
            bool restarted = false;
            float offset = 0;
            if (PingPong)
            {
                offset = Mathf.PingPong(Speed * Time.time, Range);
                restarted = (previousGoingDown && offset > previousOffset);
                previousGoingDown = offset < previousOffset;
            }
            else
            {
                offset = Mathf.Repeat(Speed * Time.time, Range);
                restarted = offset < previousOffset;
            }

            if (restarted)
            {
                if (Looping)
                {
                    SurfaceMaterial.SetVector("_Center", GazeManager.Instance.HitPosition);
                }
                else
                {
                    offset = Range;
                    Updating = false;
                }
            }

            SurfaceMaterial.SetFloat("_TransitionOffset", offset-1);
            previousOffset = offset;
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
