using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class AudioAnalysis : MonoBehaviour
{
    AudioSource _audioSource;

    public static int bandnumber = 21;

    public static float[] _samplesLeft = new float[8192];
    public static float[] _samplesRight = new float[8192];

    public static float[] _freqLeftBand;
    public static float[] _bandLeftbuffer;
    private float[] _bufferLeftDecrease;

    private float[] _freqLeftBandHighest;
    public static float[] _audioLeftBand;
    public static float[] _audioLeftBandBuffer;

    public float[] _freqRightBand;
    public static float[] _bandRightbuffer;
    private float[] _bufferRightDecrease;

    private float[] _freqRightBandHighest;
    public static float[] _audioRightBand;
    public static float[] _audioRightBandBuffer;

    private float[] _bufferDecreaseVals; 

    public static float _LeftAmplitude, _LeftAmplitudeBuffer;
    private float _LeftAmplitudeHighest;

    public static float _RightAmplitude, _RightAmplitudeBuffer;
    private float _RightAmplitudeHighest;

    private int[] sampleCount = {5, 5, 5, 23, 24, 24, 30, 31, 31, 185, 185, 186, 248, 248, 248, 248, 248, 248, 1986, 1987, 1987};   
    // Start is called before the first frame update
    void Start()
    {
        _freqLeftBand = new float[bandnumber];
        _bandLeftbuffer = new float[bandnumber];
        _bufferLeftDecrease = new float[bandnumber];

        _freqLeftBandHighest = new float[bandnumber];
        _audioLeftBand = new float[bandnumber];
        _audioLeftBandBuffer = new float[bandnumber];

        _freqRightBand = new float[bandnumber];
        _bandRightbuffer = new float[bandnumber];
        _bufferRightDecrease = new float[bandnumber];

        _freqRightBandHighest = new float[bandnumber];
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
        int [] sampleBandCounts = {15, 70, 91, 551, 733, 734, 5881};
        
        _audioSource = GetComponent<AudioSource> ();
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
            _audioLeftBandBuffer[i] = _bandLeftbuffer[i] / _freqLeftBandHighest[i];
        }
    }

    void CreateRightAudioBands() {
        for (int i = 0; i < bandnumber; i++) {
            if (_freqRightBand[i] > _freqRightBandHighest[i]) {
                _freqRightBandHighest[i] = _freqRightBand[i];
            }
            _audioRightBand[i] = _freqRightBand[i] / _freqRightBandHighest[i];
            _audioRightBandBuffer[i] = _bandRightbuffer[i] / _freqRightBandHighest[i];
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

