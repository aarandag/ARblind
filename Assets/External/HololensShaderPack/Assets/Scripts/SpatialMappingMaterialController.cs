using HoloToolkit.Unity.SpatialMapping;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Steps through the provided materials 
/// </summary>
public class SpatialMappingMaterialController : MonoBehaviour
{
    public List<Material> Materials;
    public Material SelectedMaterial { get; private set; }
    private int MaterialIndex = -1;

    public delegate void NewMaterialSelectedAction(Material selectedMaterial);
    public static event NewMaterialSelectedAction OnNewMaterialSelected;

    // Use this for initialization
    void Start()
    {
        if (!SpatialMappingManager.Instance)
        {
            Debug.LogError("This script expects that you have a SpatialMappingManager component in your scene.");
            Destroy(this);
        }
    }

    public void SelectNextMaterial()
    {
        MaterialIndex++;
        if (MaterialIndex >= Materials.Count)
        {
            MaterialIndex = Materials.Count > 0 ? 0 : -1;
        }

        if (MaterialIndex >= 0 && MaterialIndex < Materials.Count)
        {
            SelectedMaterial = Materials[MaterialIndex];

            SpatialMappingManager.Instance.SurfaceMaterial = SelectedMaterial;

            if (OnNewMaterialSelected != null)
            {
                OnNewMaterialSelected(SelectedMaterial);
            }
        }
    }
}
