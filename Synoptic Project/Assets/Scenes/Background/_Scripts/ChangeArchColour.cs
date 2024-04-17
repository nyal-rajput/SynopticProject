using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeArchColour : MonoBehaviour
{
    public int band;
    public bool left;
    Material _material;
    Color ArchColor;
    Color nextArchColor;
    Color newArchColor;
    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<Renderer> ().materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        ArchColor = new Color(AudioAnalysis.Instance._LowFrequencyColour.r - (AudioAnalysis.Instance._LowFrequencyColour.r - AudioAnalysis.Instance._HighFrequencyColour.r) * (band + 1) / AudioAnalysis.bandnumber, AudioAnalysis.Instance._LowFrequencyColour.g - (AudioAnalysis.Instance._LowFrequencyColour.g - AudioAnalysis.Instance._HighFrequencyColour.g) * (band + 1) / AudioAnalysis.bandnumber, AudioAnalysis.Instance._LowFrequencyColour.b - (AudioAnalysis.Instance._LowFrequencyColour.b - AudioAnalysis.Instance._HighFrequencyColour.b) * (band + 1) / AudioAnalysis.bandnumber);
        nextArchColor = new Color(AudioAnalysis.Instance._NextLowFrequencyColour.r - (AudioAnalysis.Instance._NextLowFrequencyColour.r - AudioAnalysis.Instance._NextHighFrequencyColour.r) * (band + 1) / AudioAnalysis.bandnumber, AudioAnalysis.Instance._NextLowFrequencyColour.g - (AudioAnalysis.Instance._NextLowFrequencyColour.g - AudioAnalysis.Instance._NextHighFrequencyColour.g) * (band + 1) / AudioAnalysis.bandnumber, AudioAnalysis.Instance._NextLowFrequencyColour.b - (AudioAnalysis.Instance._NextLowFrequencyColour.b - AudioAnalysis.Instance._NextHighFrequencyColour.b) * (band + 1) / AudioAnalysis.bandnumber);    
        newArchColor = new Color(ArchColor.r - (ArchColor.r - nextArchColor.r) * AudioAnalysis.Instance.time / 60, ArchColor.g - (ArchColor.g - nextArchColor.g) * AudioAnalysis.Instance.time / 60, ArchColor.b - (ArchColor.b - nextArchColor.b) * AudioAnalysis.Instance.time / 60);
        if (left) {
            // _material.SetColor("_Color", newArchColor * (float)Math.Pow(AudioAnalysis._audioLeftBandBuffer[band], 2) * 32f);
            // _material.SetColor("_Color", newArchColor * (float)AudioAnalysis._audioLeftBandBuffer[band] * 20f / (AudioAnalysis._LeftAmplitudeBuffer * 5));
            _material.SetColor("_Color", newArchColor * (float)AudioAnalysis._bandLeftbuffer[band] * 20f);
        }
        else {
            // _material.SetColor("_Color", newArchColor * (float)Math.Pow(AudioAnalysis._audioRightBandBuffer[band], 2) * 32f);
            // _material.SetColor("_Color", newArchColor * (float)AudioAnalysis._audioRightBandBuffer[band] * 20f / (AudioAnalysis._RightAmplitudeBuffer * 5));
            _material.SetColor("_Color", newArchColor * (float)AudioAnalysis._bandRightbuffer[band] * 20f);
        
        }
    }
}
