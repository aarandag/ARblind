using System;
using HoloToolkit.Unity;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

/// <summary>
/// PlayProgramManager is a class that runs the scan of the environment
/// </summary>
public class PlayProgramManager : Singleton<PlayProgramManager>, IInputClickHandler
{
    [Tooltip("Minimum area for complete scan")]
    public float MinAreaForComplete = 30.0f;

    [Tooltip("Minimum horizontal area for complete scan")]
    public float MinHorizAreaForComplete = 20.0f;

    [Tooltip("Minimum wall area for complete scan")]
    public float MinWallAreaForComplete = 5.0f;

    [Tooltip("Object to convert text into speech")]
    public TextToSpeech textToSpeech;

    [Tooltip("Display that shows the user what has to do")]
    public TextMesh ScanDisplay;

    private bool _scanComplete = false;
    private bool _minScanRequirements = false;
    private uint trackedHandsCount = 0;

    void Start()
    {
        SpatialUnderstanding.Instance.ScanStateChanged += Instance_ScanStateChanged;
        SpatialUnderstanding.Instance.RequestBeginScanning();
    }

    private void Instance_ScanStateChanged()
    {
        if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
            && SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        {
            _scanComplete = true;
        }
    }

    void Update()
    {
        if (ScanDisplay != null)
        {
            ScanDisplay.text = PrimaryText;
            ScanDisplay.color = PrimaryColor;
        }
    }

    /// <summary>
    /// Help the user to scan the environment
    /// </summary>
    public string PrimaryText
    {
        get
        {
            // Scan state
            if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                switch (SpatialUnderstanding.Instance.ScanState)
                {
                    case SpatialUnderstanding.ScanStates.Scanning:
                        // Get the scan stats
                        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                        {
                            CreateSpeech("playspace stats query failed");
                            return "playspace stats query failed";
                        }

                        // The stats tell us if we could potentially finish
                        if (DoesScanMeetMinBarForCompletion)
                        {
                            CreateSpeech("When ready, air tap to finalize your playspace");
                            return "When ready, air tap to finalize your playspace";
                        }
                        CreateSpeech("Move around and scan in your playspace");
                        return "Move around and scan in your playspace";
                    case SpatialUnderstanding.ScanStates.Finishing:
                        CreateSpeech("Finalizing scan");
                        return "Finalizing scan (please wait)";
                    case SpatialUnderstanding.ScanStates.Done:
                        CreateSpeech("Scan complete");
                        return "Scan complete";
                    default:
                        CreateSpeech(SpatialUnderstanding.Instance.ScanState.ToString());
                        return "ScanState = " + SpatialUnderstanding.Instance.ScanState.ToString();
                }
            }
            return "";
        }
    }

    /// <summary>
    /// Check whether or not the scan has completed its requirements
    /// </summary>
    public bool DoesScanMeetMinBarForCompletion
    {
        get
        {
            // Only allow this when we are actually scanning
            if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) ||
                (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                return false;
            }

            // Query the current playspace stats
            var statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
            {
                return false;
            }
            var stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

            // Check our preset requirements
            if ((stats.TotalSurfaceArea > MinAreaForComplete) ||
                (stats.HorizSurfaceArea > MinHorizAreaForComplete) ||
                (stats.WallSurfaceArea > MinWallAreaForComplete))
            {
                _minScanRequirements = true;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Change color of the ScanDisplay depending on the scan state
    /// </summary>
    public Color PrimaryColor
    {
        get
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
            {
                if (trackedHandsCount > 0)
                {
                    return DoesScanMeetMinBarForCompletion ? Color.green : Color.red;
                }
                return DoesScanMeetMinBarForCompletion ? Color.yellow : Color.white;
            }
            return Color.white;
        }
    }

    /// <summary>
    /// Converts text to speech
    /// </summary>
    /// <param name="speech"></param>
    private void CreateSpeech(string speech)
    {
        // Create speech
        var msg = string.Format(speech);

        // Speak message
        textToSpeech.StartSpeaking(msg);
    }

    /// <summary>
    /// Tap the text to finalize the scan
    /// </summary>
    /// <param name="eventData"></param>
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
            !SpatialUnderstanding.Instance.ScanStatsReportStillWorking && _minScanRequirements)
        {
            SpatialUnderstanding.Instance.RequestFinishScan();
            _scanComplete = true;
            ScanDisplay.text = "";

            // hide mesh
            var customMesh = SpatialUnderstanding.Instance.GetComponent<SpatialUnderstandingCustomMesh>();
            customMesh.DrawProcessedMesh = false;
        }
    }
}