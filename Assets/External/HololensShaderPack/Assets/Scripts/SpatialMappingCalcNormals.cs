using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hooks into the current SpatialMappingSource to listen for Surface Object additions or updates 
/// Calculates normals for each Surface Object containing a valid Mesh 
/// </summary>
public class SpatialMappingCalcNormals : MonoBehaviour
{
	void Start ()
    {
        if (!SpatialMappingManager.Instance)
        {
            Debug.LogError("This script expects that you have a SpatialMappingManager component in your scene.");
            Destroy(this);
        }

        var spatialMappingSource = SpatialMappingManager.Instance.Source;
        SpatialMappingManager.Instance.SourceChanged += SpatialMappingManager_SourceChanged;
        if (spatialMappingSource != null)
        {
            AttachSpatialMappingSourceListeners(spatialMappingSource);
        }
	}

    private void SpatialMappingManager_SourceChanged(object sender, HoloToolkit.Unity.PropertyChangedEventArgsEx<SpatialMappingSource> e)
    {
        if (e.OldValue)
        {
            AttachSpatialMappingSourceListeners(e.OldValue, false);
        }

        if (e.NewValue)
        {
            AttachSpatialMappingSourceListeners(e.NewValue);
        }
    }

    private void SpatialMappingSource_SurfaceUpdated(object sender, HoloToolkit.Unity.DataEventArgs<SpatialMappingSource.SurfaceUpdate> e)
    {
        if (e.Data.New.Filter.mesh)
        {
            e.Data.New.Filter.mesh.RecalculateNormals();
        }
    }

    private void SpatialMappingSource_SurfaceAdded(object sender, HoloToolkit.Unity.DataEventArgs<SpatialMappingSource.SurfaceObject> e)
    {
        if (e.Data.Filter.mesh)
        {
            e.Data.Filter.mesh.RecalculateNormals();
        }
    }

    private void AttachSpatialMappingSourceListeners(SpatialMappingSource spatialMappingSource, bool attach= true)
    {
        if (attach)
        {
            spatialMappingSource.SurfaceAdded += SpatialMappingSource_SurfaceAdded;
            spatialMappingSource.SurfaceUpdated += SpatialMappingSource_SurfaceUpdated;
        }
        else
        {
            spatialMappingSource.SurfaceAdded -= SpatialMappingSource_SurfaceAdded;
            spatialMappingSource.SurfaceUpdated -= SpatialMappingSource_SurfaceUpdated;
        }
    }
}
