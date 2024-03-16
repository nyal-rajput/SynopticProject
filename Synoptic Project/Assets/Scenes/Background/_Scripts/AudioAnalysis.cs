using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

// [RequireComponent (typeof (AudioSource))]
public class AudioAnalysis : MonoBehaviour
{
    public float time;

    public Color _bassColour;
    public Color _highsColour;
    public Color _nextbassColour;
    public Color _nexthighsColour;
    public static Color nextColour;

    AudioSource _audioSource;

    public bool _liveaudio;
    public static string _audiodevice;
    public string[] _availabledevices;

    public static int bandnumber = 21;

    public static float[] _samplesLeft = new float[8192];
    public static float[] _samplesRight = new float[8192];

    public static float[] _freqLeftBand;
    public static float[] _bandLeftbuffer;
    private float[] _bufferLeftDecrease;

    private float[] _freqLeftBandHighest;
    public static float[] _audioLeftBand;
    public static float[] _audioLeftBandBuffer;

    public static float[] _freqRightBand;
    public static float[] _bandRightbuffer;
    private float[] _bufferRightDecrease;

    public float[] _freqRightBandHighest;
    public static float[] _audioRightBand;
    public static float[] _audioRightBandBuffer;

    private float[] _bufferDecreaseVals; 

    public static float _LeftAmplitude, _LeftAmplitudeBuffer;
    private float _LeftAmplitudeHighest = 0.2f;

    public static float _RightAmplitude, _RightAmplitudeBuffer;
    private float _RightAmplitudeHighest = 0.2f;

    private int[] sampleCount = {5, 5, 5, 23, 24, 24, 30, 31, 31, 185, 185, 186, 248, 248, 248, 248, 248, 248, 1986, 1987, 1987};   
    // Start is called before the first frame update

    public static AudioAnalysis Instance;
    
    void Awake(){
        Instance = this;
    }


    void Start()
    {
        time = 0f;

        _availabledevices = new string[Microphone.devices.Length];

        _freqLeftBand = new float[bandnumber];
        _bandLeftbuffer = new float[bandnumber];
        _bufferLeftDecrease = new float[bandnumber];

        _freqLeftBandHighest = new float[] {0.3f, 2f, 2.8f, 2f, 1.9f, 1.9f, 1.7f, 2.3f, 3.3f, 2f, 3.2f, 3.5f, 4.4f, 5.2f, 6.1f, 5.4f, 6.4f, 6.8f, 7.4f, 5.8f, 1.9f};
        _audioLeftBand = new float[bandnumber];
        _audioLeftBandBuffer = new float[bandnumber];

        _freqRightBand = new float[bandnumber];
        _bandRightbuffer = new float[bandnumber];
        _bufferRightDecrease = new float[bandnumber];

        _freqRightBandHighest = new float[] {0.3f, 2f, 2.8f, 2f, 1.9f, 1.9f, 1.7f, 2.3f, 3.3f, 2f, 3.2f, 3.5f, 4.4f, 5.2f, 6.1f, 5.4f, 6.4f, 6.8f, 7.4f, 5.8f, 1.9f};
        _audioRightBand = new float[bandnumber];
        _audioRightBandBuffer = new float[bandnumber];

        _bufferDecreaseVals = new float[bandnumber];
        for (int i = 0; i < bandnumber; i++) {
            _bufferDecreaseVals[i] = 0.15f / (bandnumber - i);
        }

        /*
        * 22020 hertz / 8196 bands = 2.687hertz per samples
        * Feq Bands - 
        * IGNORE: 7 = 19 hertx (0 - 19)
        * Sub bass: 15 = 60 hertz (20 - 60)
        * Bass: 71 = 190 hertz (61 - 251)
        * Low Mid: 92 = 248 hertz (252 - 499)
        * Mid: 556 = 1501 hertz (500 - 2000)
        * High Mid: 744 = 1999 hertz (2001 - 4000)
        * Presence: 744 = 1999 hertz (4001 - 6000)
        * Brilliance 5967 = 16020 hertz (6001 - 22020)
        */     
        // int [] sampleBandCounts = {15, 71, 92, 551, 733, 734, 5881};

        _audioSource = GetComponent<AudioSource> ();

        for (int i = 0; i < Microphone.devices.Length; i++) {
            _availabledevices[i] = Microphone.devices[i].ToString();
        }
        
        if (_liveaudio) {
            if (Microphone.devices.Length > 0) {
                _audiodevice = Microphone.devices[0].ToString();
                _audioSource.clip = Microphone.Start(_audiodevice, true, 100, 22050);
            }
            else {
                _liveaudio = false;
            }
            _audioSource.Play();
        }

    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource ();
        MakeFrequencyLeftBands ();
        MakeFrequencyRightBands ();
        BandLeftBuffer ();
        BandRightBuffer ();
        CreateLeftAudioBands ();
        CreateRightAudioBands ();
        GetLeftAmplitude ();
        GetRightAmplitude ();
        GetTime ();
        UpdateColour();
    }

    void UpdateColour() {
        if (_bassColour.r == 1 && _bassColour.g <= 0.01 && _bassColour.b == 0) {
            _bassColour = new Color (1, 0, 0);
            _nextbassColour = new Color (1, 0, 1);
        }
        else if (_bassColour.r == 1 && _bassColour.g <= 0.01 && _bassColour.b == 1) {
            _bassColour = new Color (1, 0, 1);
            _nextbassColour = new Color (0, 1, 1);
        }
        // else if (_bassColour.r <= 0.01 && _bassColour.g <= 0.01 && _bassColour.b == 1) {
        //     _bassColour = new Color (0, 0, 1);
        //     _nextbassColour = new Color (0, 1, 1);
        // }       
        else if (_bassColour.r <= 0.01 && _bassColour.g == 1 && _bassColour.b == 1) {
            _bassColour = new Color (0, 1, 1);
            _nextbassColour = new Color (1, 1, 0);
        }   
        // else if (_bassColour.r <= 0.01 && _bassColour.g == 1 && _bassColour.b <= 0.01) {
        //     _bassColour = new Color (0, 1, 0);
        //     _nextbassColour = new Color (1, 1, 0);
        // }   
        else if (_bassColour.r == 1 && _bassColour.g == 1 && _bassColour.b <= 0.01) {
            _bassColour = new Color (1, 1, 0);
            _nextbassColour = new Color (1, 0, 0);
        }   

        if (_highsColour.r == 1 && _highsColour.g <= 0.01 && _highsColour.b <= 0.01) {
            _highsColour = new Color (1, 0, 0);
            _nexthighsColour = new Color (1, 0, 1);
        }
        else if (_highsColour.r == 1 && _highsColour.g <= 0.01 && _highsColour.b == 1) {
            _highsColour = new Color (1, 0, 1);
            _nexthighsColour = new Color (0, 1, 1);
        }
        // else if (_highsColour.r <= 0.01 && _highsColour.g <= 0.01 && _highsColour.b == 1) {
        //     _highsColour = new Color (0, 0, 1);
        //     _nexthighsColour = new Color (0, 1, 1);
        // }       
        else if (_highsColour.r <= 0.01 && _highsColour.g == 1 && _highsColour.b == 1) {
            _highsColour = new Color (0, 1, 1);
            _nexthighsColour = new Color (1, 1, 0);
        }   
        // else if (_highsColour.r <= 0.01 && _highsColour.g == 1 && _highsColour.b <= 0.01) {
        //     _highsColour = new Color (0, 1, 0);
        //     _nexthighsColour = new Color (1, 1, 0);
        // }   
        else if (_highsColour.r == 1 && _highsColour.g == 1 && _highsColour.b <= 0.01) {
            _highsColour = new Color (1, 1, 0);
            _nexthighsColour = new Color (1, 0, 0);
        }   

        if (time > 60) {
            _bassColour = _nextbassColour;
            _highsColour = _nexthighsColour;
            time = 0;
        }
    }

    public float sigmoidfunct (float y, float a) {
        return 1.0f / (1.0f + (float) Math.Exp(- (8 / a) * (y - a / 2)));
    }

    void GetTime() {
        time += Time.deltaTime;
    }

    void GetLeftAmplitude() {
        float _CurrentLeftAmplitude = 0;
        float _CurrentLeftAmplitudeBuffer = 0;
        for (int i = 0; i < bandnumber; i++) {
            _CurrentLeftAmplitude += _audioLeftBand[i];
            _CurrentLeftAmplitudeBuffer += _audioLeftBandBuffer[i];
        }
        if (_CurrentLeftAmplitude > _LeftAmplitudeHighest) {
            _LeftAmplitudeHighest = _CurrentLeftAmplitude;
        }
        _LeftAmplitude = _CurrentLeftAmplitude / _LeftAmplitudeHighest;
        _LeftAmplitudeBuffer = _CurrentLeftAmplitudeBuffer / _LeftAmplitudeHighest;
    } 

    void GetRightAmplitude() {
        float _CurrentRightAmplitude = 0;
        float _CurrentRightAmplitudeBuffer = 0;
        for (int i = 0; i < bandnumber; i++) {
            _CurrentRightAmplitude += _audioRightBand[i];
            _CurrentRightAmplitudeBuffer += _audioRightBandBuffer[i];
        }
        if (_CurrentRightAmplitude > _RightAmplitudeHighest) {
            _RightAmplitudeHighest = _CurrentRightAmplitude;
        }
        _RightAmplitude = _CurrentRightAmplitude / _RightAmplitudeHighest;
        _RightAmplitudeBuffer = _CurrentRightAmplitudeBuffer / _RightAmplitudeHighest;
    } 

    void CreateLeftAudioBands() {
        for (int i = 0; i < bandnumber; i++) {
            if (_freqLeftBand[i] > _freqLeftBandHighest[i]) {
                _freqLeftBandHighest[i] = _freqLeftBand[i];
            }
            _audioLeftBand[i] = _freqLeftBand[i] / _freqLeftBandHighest[i];
            // _audioLeftBandBuffer[i] = _bandLeftbuffer[i] / _freqLeftBandHighest[i];
            _audioLeftBandBuffer[i] = sigmoidfunct(_bandLeftbuffer[i], _freqLeftBandHighest[i]);
        }
    }

    void CreateRightAudioBands() {
        for (int i = 0; i < bandnumber; i++) {
            if (_freqRightBand[i] > _freqRightBandHighest[i]) {
                _freqRightBandHighest[i] = _freqRightBand[i];
            }
            _audioRightBand[i] = _freqRightBand[i] / _freqRightBandHighest[i];
            // _audioRightBandBuffer[i] = _bandRightbuffer[i] / _freqRightBandHighest[i];
            _audioRightBandBuffer[i] = sigmoidfunct(_bandRightbuffer[i], _freqRightBandHighest[i]);
        }
    }

    void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
    }

    void BandLeftBuffer() {
        for (int i = 0; i < bandnumber; i++) {
            if (_freqLeftBand[i] > _bandLeftbuffer[i]) {
                _bandLeftbuffer[i] = _freqLeftBand[i];
                _bufferLeftDecrease[i] = _bufferDecreaseVals[i];
            }
            if (_freqLeftBand[i] < _bandLeftbuffer[i]) {
                _bandLeftbuffer[i] -= _bufferLeftDecrease[i];
                _bufferLeftDecrease[i] *= 1.2f;
            }
        }
    }

    void BandRightBuffer() {
        for (int i = 0; i < bandnumber; i++) {
            if (_freqRightBand[i] > _bandRightbuffer[i]) {
                _bandRightbuffer[i] = _freqRightBand[i];
                _bufferRightDecrease[i] = _bufferDecreaseVals[i];
            }
            if (_freqRightBand[i] < _bandRightbuffer[i]) {
                _bandRightbuffer[i] -= _bufferRightDecrease[i];
                _bufferRightDecrease[i] *= 1.2f;
            }
        }
    }

    void MakeFrequencyLeftBands() {
        int count = 6;
        float average;
        int bandcount;

        for (int i = 0; i < bandnumber; i++) {
            average = 0;
            bandcount = 0;

            for (int j = 0; j < sampleCount[i]; j++) {
                average += (_samplesLeft[count]) * (count + 1); //In Unity higher frequencies have exponentially smaller values, by multiplying by  count, it offsets the smaller numbers that come with high frequencies. Making the output for the _freqBand more normal.
                count++;
                bandcount++;
            }

            average = average / bandcount;

            _freqLeftBand[i] = average * 10;
        }
    }

    void MakeFrequencyRightBands() {
        int count = 6;
        float average;
        int bandcount;

        for (int i = 0; i < bandnumber; i++) {
            average = 0;
            bandcount = 0;

            for (int j = 0; j < sampleCount[i]; j++) {
                average += (_samplesRight[count]) * (count + 1); //In Unity higher frequencies have exponentially smaller values, by multiplying by  count, it offsets the smaller numbers that come with high frequencies. Making the output for the _freqBand more normal.
                count++;
                bandcount++;
            }
            average = average / bandcount;
            

            _freqRightBand[i] = average * 10;
        }
    }
}

