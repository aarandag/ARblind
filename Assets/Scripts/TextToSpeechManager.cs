using HoloToolkit.Unity;
using UnityEngine;

public class TextToSpeechManager : MonoBehaviour {

    private TextToSpeech textToSpeech;

    private void Awake()
    {
        textToSpeech = GetComponent<TextToSpeech>();
    }

    public void SpeakTime()
    {
        textToSpeech.StartSpeaking("No! You can walk altough you should take care");
    }
}
