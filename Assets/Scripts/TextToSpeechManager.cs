using HoloToolkit.Unity;
using UnityEngine;

public class TextToSpeechManager : MonoBehaviour {

    private TextToSpeech textToSpeech;

    private void Awake()
    {
        textToSpeech = GetComponent<TextToSpeech>();
    }

    // Response whether or not there is someting next to me
    public void IsThereAnythingNextToMe()
    {
        textToSpeech.StartSpeaking("There isn't anything next to me");
    }

    // Response whether or not there is some chair next to me
    public void IsThereAnyChairNextToMe()
    {
        textToSpeech.StartSpeaking("There isn't any chair next to me");
    }

    // Response whether or not there is some table next to me
    public void IsThereAnyTableNextToMe()
    {
        textToSpeech.StartSpeaking("There isn't any table next to me");
    }
}
