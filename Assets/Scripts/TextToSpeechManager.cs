﻿using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;
using HoloToolkit.Unity;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class TextToSpeechManager : MonoBehaviour {

    private TextToSpeech textToSpeech;
    private TextMesh textCursor;
    private int n_objects;

    private void Awake()
    {
        textToSpeech = GetComponent<TextToSpeech>();
    }

    private void Start()
    {
        textCursor = GameObject.Find("Cursor").GetComponentInChildren<TextMesh>();
    }

    // Response whether or not there is someting next to me
    public void IsThereAnythingNextToMe()
    {
        PlayVisualizer.Instance.Query_Shape_FindShapeHalfDims("All Surfaces");
        StartCoroutine(WaitForObtainNumberOfObjects("Anything"));
        
    }

    // Response whether or not there is some chair next to me
    public void IsThereAnyChairNextToMe()
    {
        PlayVisualizer.Instance.Query_Shape_FindShapeHalfDims("Sittable");
        StartCoroutine(WaitForObtainNumberOfObjects("Sittable"));
        
    }

    // Response whether or not there is some table next to me
    public void IsThereAnyTableNextToMe()
    {
        PlayVisualizer.Instance.Query_Shape_FindShapeHalfDims("Large Empty Surface");
        StartCoroutine(WaitForObtainNumberOfObjects("Table"));
        
    }

    // Response the distance between person and object
    public void WhatIsTheDistanceBetweenPersonObject()
    {
        Vector3 headPosition = Camera.main.transform.position;
        Vector3 headDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;

        if(Physics.Raycast(headPosition, headDirection, out hitInfo))
        {
            float rounded = (float)(System.Math.Round((double)hitInfo.distance, 2));
            textToSpeech.StartSpeaking("The distance between " + textCursor.text + " and you is " + rounded);
            PlayProgramManager.Instance.SpaceQueryDescription = "Distance to "+ textCursor.text + ": "+rounded;

        }
        else
        {
            textToSpeech.StartSpeaking("The distance cannot be recognized");
        }
    }

    /// <summary>
    /// Wait for 3 seconds to obtain the number of the objects that have been found
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForObtainNumberOfObjects(string type)
    {
        yield return new WaitForSeconds(3.0f);
        string number_shapes = PlayProgramManager.Instance.SpaceQueryDescription;
        var resultString = Regex.Match(number_shapes, @"\d+").Value; // \d + is the regex for an integer number
        n_objects = int.Parse(resultString);
        switch (type)
        {
            case "Anything":
                if (n_objects <= 0)
                {
                    textToSpeech.StartSpeaking("There is no surface found");
                }
                else
                {
                    textToSpeech.StartSpeaking("There are some surfaces found");
                }
                break;
            case "Table":
                if (n_objects <= 0)
                {
                    textToSpeech.StartSpeaking("There is no table found");
                }
                else
                {
                    textToSpeech.StartSpeaking("There are some tables found");
                }
                break;
            case "Sittable":
                if (n_objects <= 0)
                {
                    textToSpeech.StartSpeaking("There is no any chair found");
                }
                else
                {
                    textToSpeech.StartSpeaking("There are some chairs found");
                }
                break;
        }

    }
}
