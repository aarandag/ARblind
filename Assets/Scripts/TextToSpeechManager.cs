﻿using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;
using HoloToolkit.Unity;
using System.Text.RegularExpressions;
using UnityEngine;

public class TextToSpeechManager : MonoBehaviour {

    private TextToSpeech textToSpeech;
    private TextMesh ScanDisplay;
    private int n_objects;

    private void Awake()
    {
        textToSpeech = GetComponent<TextToSpeech>();
    }

    private void Start()
    {
        ScanDisplay = GameObject.Find("ProgramManager").GetComponentInChildren<TextMesh>();
    }

    // Response whether or not there is someting next to me
    public void IsThereAnythingNextToMe()
    {
        textToSpeech.StartSpeaking("There is something next to me");
        PlayVisualizer.Instance.Query_Shape_FindShapeHalfDims("All Surfaces");
        string number_shapes = ScanDisplay.text;
        var resultString = Regex.Match(number_shapes, @"\d+").Value; // \d + is the regex for an integer number
        n_objects = int.Parse(resultString);
        if (n_objects <= 0)
        {
            textToSpeech.StartSpeaking("There is not anything next to me");
        }
        else
        {
            textToSpeech.StartSpeaking("There is something next to me");
        }
    }

    // Response whether or not there is some chair next to me
    public void IsThereAnyChairNextToMe()
    {
        textToSpeech.StartSpeaking("There is some chair next to me");
        PlayVisualizer.Instance.Query_Shape_FindShapeHalfDims("Sittable");
        string number_shapes = ScanDisplay.text;
        var resultString = Regex.Match(number_shapes, @"\d+").Value; // \d + is the regex for an integer number
        n_objects = int.Parse(resultString);
        if (n_objects <= 0)
        {
            textToSpeech.StartSpeaking("There is no any chair next to me");
        }
        else
        {
            textToSpeech.StartSpeaking("There is some chair next to me");
        }
    }

    // Response whether or not there is some table next to me
    public void IsThereAnyTableNextToMe()
    {
        textToSpeech.StartSpeaking("There is some table next to me");
        PlayVisualizer.Instance.Query_Shape_FindShapeHalfDims("Large Empty Surface");
        string number_shapes = ScanDisplay.text;
        var resultString = Regex.Match(number_shapes, @"\d+").Value; // \d + is the regex for an integer number
        n_objects = int.Parse(resultString);
        if (n_objects <= 0)
        {
            textToSpeech.StartSpeaking("There is no table next to me");
        }
        else
        {
            textToSpeech.StartSpeaking("There is some table next to me");
        }
    }
}
