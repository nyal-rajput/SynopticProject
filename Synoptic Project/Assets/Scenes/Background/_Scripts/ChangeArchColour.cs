using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeArchColour : MonoBehaviour
{
    public int band;
    public bool left;
    Material _material;
    Color ArchColor;
    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<Renderer> ().materials[0];
        ArchColor = new Color(AudioAnalysis.Instance._bassColour.r - (AudioAnalysis.Instance._bassColour.r - AudioAnalysis.Instance._highsColour.r) * band / AudioAnalysis.bandnumber, AudioAnalysis.Instance._bassColour.g - (AudioAnalysis.Instance._bassColour.g - AudioAnalysis.Instance._highsColour.g) * band / AudioAnalysis.bandnumber, AudioAnalysis.Instance._bassColour.b - (AudioAnalysis.Instance._bassColour.b - AudioAnalysis.Instance._highsColour.b) * band / AudioAnalysis.bandnumber);
    }

    // Update is called once per frame
    void Update()
    {
        if (left) {
            _material.SetColor("_Color", ArchColor * (AudioAnalysis._audioLeftBandBuffer[band] * AudioAnalysis._audioLeftBandBuffer[band] * 7f));
        }
        else {
            _material.SetColor("_Color", ArchColor * (AudioAnalysis._audioRightBandBuffer[band] * AudioAnalysis._audioRightBandBuffer[band] * 7f));
        }
    }
}
