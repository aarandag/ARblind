using System;
using HoloToolkit.Unity;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;
using System.Collections;

/// <summary>
/// Speech phase states
/// </summary>
public enum SpeechPhase
{
    /// <summary>
    /// Speech phase is stopped
    /// </summary>
    Stopped = 0,

    /// <summary>
    /// Scanning
    /// </summary>
    Scanning = 1,

    /// <summary>
    /// Ready to tap
    /// </summary>
    Finalizing = 2,

    /// <summary>
    /// Scan completed
    /// </summary>
    Completed = 3
}

/// <summary>
/// PlayProgramManager is a class that runs the scan of the environment
/// </summary>
public class PlayProgramManager : Singleton<PlayProgramManager>, IInputClickHandler
{
    // Config
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

    [Tooltip("Object that allows to understand the things that we are looking at")]
    public SpatialUnderstandingCursor AppCursor;

    /// <summary>
    /// Indicates the current state of the Speech Phase.
    /// </summary>
    public SpeechPhase speechPhase { get; private set; }

    private bool _scanComplete = false;
    private bool _minScanRequirements = false;
    private string spaceQueryDescription;
    private string objectPlacementDescription;
    private uint trackedHandsCount = 0;

    // Properties
    public string SpaceQueryDescription
    {
        get
        {
            return spaceQueryDescription;
        }
        set
        {
            spaceQueryDescription = value;
            objectPlacementDescription = "";
        }
    }

    public string ObjectPlacementDescription
    {
        get
        {
            return objectPlacementDescription;
        }
        set
        {
            objectPlacementDescription = value;
            spaceQueryDescription = "";
        }
    }

    protected override void Awake()
    {
        base.Awake();
        speechPhase = SpeechPhase.Scanning;
    }

    private void Start()
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

    private void Update()
    {
        if (ScanDisplay.gameObject.activeSelf)
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
            // Display the space and object query results (has priority)
            if (!string.IsNullOrEmpty(SpaceQueryDescription))
            {
                return SpaceQueryDescription;
            }
            else if (!string.IsNullOrEmpty(ObjectPlacementDescription))
            {
                return ObjectPlacementDescription;
            }

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
                            return "playspace stats query failed";
                        }

                        // The stats tell us if we could potentially finish
                        if (DoesScanMeetMinBarForCompletion)
                        {
                            if (speechPhase == SpeechPhase.Finalizing)
                            {
                                CreateSpeech("When ready, air tap to finalize your playspace");
                                speechPhase = SpeechPhase.Completed;
                            }
                            return "When ready, air tap to finalize your playspace";
                        }

                        if (speechPhase == SpeechPhase.Scanning)
                        {
                            CreateSpeech("Move around and scan in your playspace");
                            speechPhase = SpeechPhase.Finalizing;
                        }
                        return "Move around and scan in your playspace";
                    case SpatialUnderstanding.ScanStates.Finishing:
                        return "Finalizing scan (please wait)";
                    case SpatialUnderstanding.ScanStates.Done:
                        if (speechPhase == SpeechPhase.Completed)
                        {
                            CreateSpeech("Scan complete");
                            speechPhase = SpeechPhase.Stopped;
                        }
                        return "Scan complete";
                    default:
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

            // If we're looking at the menu, fade it out
            Vector3 hitPos, hitNormal;
            UnityEngine.UI.Button hitButton;
            float alpha = AppCursor.RayCastUI(out hitPos, out hitNormal, out hitButton) ? 0.15f : 1.0f;

            // Special case processing & 
            return (!string.IsNullOrEmpty(SpaceQueryDescription) || !string.IsNullOrEmpty(ObjectPlacementDescription)) ?
                (PrimaryText.Contains("processing") ? new Color(1.0f, 0.0f, 0.0f, 1.0f) : new Color(1.0f, 0.7f, 0.1f, alpha)) :
                new Color(1.0f, 1.0f, 1.0f, alpha);
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
        //textToSpeech.StartSpeaking(msg);
        Debug.Log(speech);
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
            //customMesh.DrawProcessedMesh = false;
        }
    }
}