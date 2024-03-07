using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFOV : MonoBehaviour
{
    float FOV;
    // Start is called before the first frame update
    void Start()
    {
        FOV = 59f;
    }

    // Update is called once per frame
    void Update()
    {
        FOV = 57f + (AudioAnalysis._LeftAmplitudeBuffer + AudioAnalysis._RightAmplitudeBuffer) * 3f;
        if (FOV > 60) {
            Camera.main.fieldOfView = 60f;
        }
        else if (FOV < 57f) {
            Camera.main.fieldOfView = 57f;
        }
        else {
            Camera.main.fieldOfView = FOV;
        }
    }
}
