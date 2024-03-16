using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFOV : MonoBehaviour
{
    float FOV;
    // Start is called before the first frame update
    void Start()
    {
        FOV = 56f;
    }

    // Update is called once per frame
    void Update()
    {
        FOV = 56f + (AudioAnalysis._LeftAmplitudeBuffer + AudioAnalysis._RightAmplitudeBuffer) * 2.5f;
        if (FOV > 60) {
            Camera.main.fieldOfView = 60f;
        }
        else if (FOV < 56f) {
            Camera.main.fieldOfView = 56f;
        }
        else {
            Camera.main.fieldOfView = FOV;
        }
    }
}
