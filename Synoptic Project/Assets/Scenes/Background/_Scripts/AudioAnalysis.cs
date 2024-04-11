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

    public float[] _freqLeftBandHighest;
    public static float[] _audioLeftBand;
    public static float[] _audioLeftBandBuffer;

    public static float[] _freqRightBand;
    public static float[] _bandRightbuffer;
    private float[] _bufferRightDecrease;

    public static float[] _freqRightBandHighest;
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

    public float sigmoidfunct (float y, float a) {
        return 1.0f / (1.0f + (float) Math.Exp(- (9 / a) * (y - a / 2)));
    }
    
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

        _freqLeftBandHighest = new float[] {5f, 20f, 40f, 35f, 25f, 25f, 35f, 35f, 45f, 45f, 60f, 70f, 60f, 70f, 80f, 95f, 95f, 95f, 95f, 60f, 10f};
        _audioLeftBand = new float[bandnumber];
        _audioLeftBandBuffer = new float[bandnumber];

        _freqRightBand = new float[bandnumber];
        _bandRightbuffer = new float[bandnumber];
        _bufferRightDecrease = new float[bandnumber];

        _freqRightBandHighest = new float[] {5f, 20f, 40f, 35f, 25f, 25f, 35f, 35f, 45f, 45f, 60f, 70f, 60f, 70f, 80f, 95f, 95f, 95f, 95f, 60f, 10f};
        _audioRightBand = new float[bandnumber];
        _audioRightBandBuffer = new float[bandnumber];

        _bufferDecreaseVals = new float[bandnumber];
        for (int i = 0; i < bandnumber; i++) {
            _bufferDecreaseVals[i] = 0.9f - 0.4f / (bandnumber - i);
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
        GetTime ();
        GetSpectrumAudioSource ();
        CreateLeftAudioBands ();
        CreateRightAudioBands ();
        MakeFrequencyLeftBands ();
        MakeFrequencyRightBands ();
        BandLeftBuffer ();
        BandRightBuffer ();
        GetLeftAmplitude ();
        GetRightAmplitude ();
        UpdateColour();
    }

    void GetTime() {
        time += Time.deltaTime;
    }

        void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
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

    void CreateLeftAudioBands() {
        for (int i = 0; i < bandnumber; i++) {
            if (_freqLeftBand[i] > _freqLeftBandHighest[i]) {
                _freqLeftBandHighest[i] = _freqLeftBand[i];
            }
            // _audioLeftBand[i] = _freqLeftBand[i] / _freqLeftBandHighest[i];
            // _audioLeftBandBuffer[i] = sigmoidfunct(_bandLeftbuffer[i], _freqLeftBandHighest[i]);
            _audioLeftBand[i] = sigmoidfunct(_freqLeftBand[i], _freqLeftBandHighest[i]);
        }
    }

    void CreateRightAudioBands() {
        for (int i = 0; i < bandnumber; i++) {
            if (_freqRightBand[i] > _freqRightBandHighest[i]) {
                _freqRightBandHighest[i] = _freqRightBand[i];
            }
            // _audioRightBand[i] = _freqRightBand[i] / _freqRightBandHighest[i];
            // _audioRightBandBuffer[i] = sigmoidfunct(_bandRightbuffer[i], _freqRightBandHighest[i]);
            _audioRightBand[i] = sigmoidfunct(_freqRightBand[i], _freqRightBandHighest[i]);

        }
    }

    void BandLeftBuffer() {
        for (int i = 0; i < bandnumber; i++) {
            if (_audioLeftBand[i] > _bandLeftbuffer[i]) {
                _bandLeftbuffer[i] = _audioLeftBand[i];
                // _bufferLeftDecrease[i] = _bufferDecreaseVals[i];
            }
            else {
                // _bandLeftbuffer[i] *= _bufferLeftDecrease[i];
                // _bufferLeftDecrease[i] *= 1.2f;
                _bandLeftbuffer[i] *= _bufferDecreaseVals[i];
            }
        }
    }

    void BandRightBuffer() {
        for (int i = 0; i < bandnumber; i++) {
            if (_audioRightBand[i] > _bandRightbuffer[i]) {
                _bandRightbuffer[i] = _audioRightBand[i];
                // _bufferRightDecrease[i] = _bufferDecreaseVals[i];
            }
            if (_audioRightBand[i] < _bandRightbuffer[i]) {
                // _bandRightbuffer[i] *= _bufferRightDecrease[i];
                // _bufferRightDecrease[i] *= 1.2f;
                _bandRightbuffer[i] *= _bufferDecreaseVals[i];
            }
        }
    }

    void GetLeftAmplitude() {
        float _CurrentLeftAmplitude = 0;
        // float _CurrentLeftAmplitudeBuffer = 0;
        for (int i = 0; i < bandnumber; i++) {
            _CurrentLeftAmplitude += _bandLeftbuffer[i];
            // _CurrentLeftAmplitudeBuffer += _bandLeftbuffer[i];
        }
        if (_CurrentLeftAmplitude > _LeftAmplitudeHighest) {
            _LeftAmplitudeHighest = _CurrentLeftAmplitude;
        }
        // _LeftAmplitude = _CurrentLeftAmplitude / _LeftAmplitudeHighest;
        // _LeftAmplitudeBuffer = _CurrentLeftAmplitudeBuffer / _LeftAmplitudeHighest;
        _LeftAmplitude = sigmoidfunct(_CurrentLeftAmplitude, _LeftAmplitudeHighest);
    } 

    void GetRightAmplitude() {
        float _CurrentRightAmplitude = 0;
        // float _CurrentRightAmplitudeBuffer = 0;
        for (int i = 0; i < bandnumber; i++) {
            _CurrentRightAmplitude += _bandRightbuffer[i];
            // _CurrentRightAmplitudeBuffer += _bandRightbuffer[i];
        }
        if (_CurrentRightAmplitude > _RightAmplitudeHighest) {
            _RightAmplitudeHighest = _CurrentRightAmplitude;
        }
        // _RightAmplitude = _CurrentRightAmplitude / _RightAmplitudeHighest;
        // _RightAmplitudeBuffer = _CurrentRightAmplitudeBuffer / _RightAmplitudeHighest;
        _RightAmplitude = sigmoidfunct(_CurrentRightAmplitude, _RightAmplitudeHighest);
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
}

