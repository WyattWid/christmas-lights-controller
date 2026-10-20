using System;
using UnityEngine;

public class WaveformVisualizer : MonoBehaviour
{
    public string audioFileName; // Name of the audio file in Resources (without extension)
    public GameObject linePrefab; // Prefab with LineRenderer attached
    public int resolutionPerCell = 10; // Resolution per cell
    public float cellWidth = 1f; // Ensure this has a default value
    public float cellHeight = 1f; // Ensure this has a default value
    public float cellTime = 0.1f;
    public float barHeight = 60f; // Maximum height of the bars
    public float xadjust;
    public float yshift;
    public float xshift;
    public Color averageColor = Color.gray; // Color of the average values
    public Color peakColor = Color.white; // Color of the peak values

    private AudioSource audioSource;
    private float audioLength = 0.1f;
    private int numberOfBars;
    private float[] averageValues;
    private float[] peakValues;
    private LineRenderer[] averageLineRenderers;
    private LineRenderer[] peakLineRenderers;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        xshift = 0;
        if (audioSource.clip == null)
        {
            Debug.LogError("Audio clip is not assigned to AudioSource.");
            return;
        }

        if (linePrefab == null)
        {
            Debug.LogError("Line prefab is not assigned.");
            return;
        }

        audioLength = audioSource.clip.length;
        EventManager.BroadcastMessage("AudioLengthis:" + audioLength);
        EventManager.BroadcastMessage("cellTime:" + cellTime);
        InitializeWaveform();
    }

    void Update()
    {
        // Check for mouse scroll input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
    }
    void resizecells(){
        xshift = 0;
        float cells = audioLength / cellTime;
        int totalCells = (int)Mathf.Ceil(cells);
        numberOfBars = totalCells * resolutionPerCell;

        float widthPerBar = cellWidth / resolutionPerCell;
        float step = ((audioLength * cellWidth) / cellTime) / numberOfBars;
        float highestPeak = Mathf.Max(peakValues);
        float peakRatio = highestPeak / barHeight;
        float highestAverage = Mathf.Max(averageValues);
        float averageRatio = highestAverage / barHeight;
        for (int i = 0; i < numberOfBars; i++)
        {
            float x = i * step - numberOfBars * step / 2;
            float yMinAverage = -averageValues[i] / averageRatio / 2;
            float yMaxAverage = averageValues[i] / averageRatio / 2;
            float yMinPeak = -peakValues[i] / peakRatio / 2;
            float yMaxPeak = peakValues[i] / peakRatio / 2;
            if (xshift == 0)
            {
                xshift = x + 5;
            }

            // Update average LineRenderer
            LineRenderer avgLR = averageLineRenderers[i];
            avgLR.positionCount = 2;
            avgLR.startWidth = widthPerBar / 1.3f;
            avgLR.endWidth = widthPerBar / 1.3f;
            avgLR.startColor = averageColor;
            avgLR.endColor = averageColor;
            avgLR.useWorldSpace = false;

            averageLineRenderers[i] = avgLR;
            avgLR.SetPosition(0, new Vector3(x - xshift, yMinAverage + yshift, 1));
            avgLR.SetPosition(1, new Vector3(x - xshift, yMaxAverage + yshift, 1));

            // Update peak LineRenderer
            LineRenderer peakLR = peakLineRenderers[i];
            peakLR.positionCount = 2;
            peakLR.startWidth = widthPerBar / 1.3f;
            peakLR.endWidth = widthPerBar / 1.3f;
            peakLR.startColor = peakColor;
            peakLR.endColor = peakColor;
            peakLR.useWorldSpace = false;

            peakLineRenderers[i] = peakLR;
            peakLR.SetPosition(0, new Vector3(x - xshift, yMinPeak + yshift, 1));
            peakLR.SetPosition(1, new Vector3(x - xshift, yMaxPeak + yshift, 1));
        }
    }

    
    
    void InitializeWaveform()
    {
        DestroyExistingLines();
        float cells = audioLength / cellTime;
        int totalCells = (int)Mathf.Ceil(cells);
        numberOfBars = totalCells * resolutionPerCell;

        float widthPerBar = cellWidth / resolutionPerCell;
        float[] samples = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(samples, 0);

        // Calculate average and peak values
        (averageValues, peakValues) = CalculateAverageAndPeakValues(samples, audioSource.clip.channels, numberOfBars);

        averageLineRenderers = new LineRenderer[numberOfBars];
        peakLineRenderers = new LineRenderer[numberOfBars];

        for (int i = 0; i < numberOfBars; i++)
        {
            // Create LineRenderer for average values
            GameObject avgLineObject = Instantiate(linePrefab, transform);
            LineRenderer avgLR = avgLineObject.GetComponent<LineRenderer>();

            if (avgLR == null)
            {
                Debug.LogError("Line prefab does not have a LineRenderer component.");
                Destroy(avgLineObject);
                continue;
            }

            avgLR.positionCount = 2;
            avgLR.startWidth = widthPerBar / 1.3f;
            avgLR.endWidth = widthPerBar / 1.3f;
            avgLR.startColor = averageColor;
            avgLR.endColor = averageColor;
            avgLR.useWorldSpace = false;

            averageLineRenderers[i] = avgLR;

            // Create LineRenderer for peak values
            GameObject peakLineObject = Instantiate(linePrefab, transform);
            LineRenderer peakLR = peakLineObject.GetComponent<LineRenderer>();

            if (peakLR == null)
            {
                Debug.LogError("Line prefab does not have a LineRenderer component.");
                Destroy(peakLineObject);
                continue;
            }

            peakLR.positionCount = 2;
            peakLR.startWidth = widthPerBar / 1.3f;
            peakLR.endWidth = widthPerBar / 1.3f;
            peakLR.startColor = peakColor;
            peakLR.endColor = peakColor;
            peakLR.useWorldSpace = false;

            peakLineRenderers[i] = peakLR;
        }

        UpdateWaveformPositions();
    }

    void UpdateWaveformPositions()
    {
        float step = ((audioLength * cellWidth) / cellTime) / numberOfBars;
        float highestPeak = Mathf.Max(peakValues);
        float peakRatio = highestPeak / barHeight;
        float highestAverage = Mathf.Max(averageValues);
        float averageRatio = highestAverage / barHeight;
        for (int i = 0; i < numberOfBars; i++)
        {
            float x = (i+1) * step - numberOfBars * step / 2;
            float yMinAverage = -averageValues[i] / averageRatio / 2;
            float yMaxAverage = averageValues[i] / averageRatio / 2;
            float yMinPeak = -peakValues[i] / peakRatio / 2;
            float yMaxPeak = peakValues[i] / peakRatio / 2;
            if (xshift == 0)
            {
                xshift = x + 6f;
            }

            // Update average LineRenderer
            LineRenderer avgLR = averageLineRenderers[i];
            avgLR.SetPosition(0, new Vector3((x - xshift)+.8f, yMinAverage + yshift, 1));
            avgLR.SetPosition(1, new Vector3((x - xshift)+.8f, yMaxAverage + yshift, 1));

            // Update peak LineRenderer
            LineRenderer peakLR = peakLineRenderers[i];
            peakLR.SetPosition(0, new Vector3((x - xshift)+.8f, yMinPeak + yshift, 1));
            peakLR.SetPosition(1, new Vector3((x - xshift)+.8f, yMaxPeak + yshift, 1));
        }
    }

    (float[] averageValues, float[] peakValues) CalculateAverageAndPeakValues(float[] samples, int channels, int numberOfBars)
    {
        int samplesPerBar = (samples.Length / channels) / numberOfBars;
        float[] averageValues = new float[numberOfBars];
        float[] peakValues = new float[numberOfBars];
        float[] channelDifferences = new float[channels];

        for (int ch = 0; ch < channels; ch++)
        {
            for (int i = 0; i < numberOfBars; i++)
            {
                int startSample = i * samplesPerBar * channels + ch;
                int endSample = Mathf.Min(startSample + samplesPerBar * channels, samples.Length);

                float sum = 0;
                float max = float.MinValue;
                int count = 0;
                for (int j = startSample; j < endSample; j += channels)
                {
                    float sampleValue = Mathf.Abs(samples[j]);
                    sum += sampleValue;
                    if (sampleValue > max)
                    {
                        max = sampleValue;
                    }
                    count++;
                }
                averageValues[i] = sum / count;
                peakValues[i] = max;
            }
            channelDifferences[ch] = GetAverageDifference(samples, ch, channels, numberOfBars);
        }

        // Use the channel with the largest average difference
        int selectedChannel = channelDifferences[0] > channelDifferences[1] ? 0 : 1;
        for (int i = 0; i < numberOfBars; i++)
        {
            int startSample = i * samplesPerBar * channels + selectedChannel;
            int endSample = Mathf.Min(startSample + samplesPerBar * channels, samples.Length);

            float sum = 0;
            float max = float.MinValue;
            int count = 0;
            for (int j = startSample; j < endSample; j += channels)
            {
                float sampleValue = Mathf.Abs(samples[j]);
                sum += sampleValue;
                if (sampleValue > max)
                {
                    max = sampleValue;
                }
                count++;
            }
            averageValues[i] = sum / count;
            peakValues[i] = max;
        }

        return (averageValues, peakValues);
    }

    float GetAverageDifference(float[] samples, int channel, int channels, int numberOfBars)
    {
        int samplesPerBar = (samples.Length / channels) / numberOfBars;
        float totalDifference = 0;

        for (int i = 0; i < numberOfBars; i++)
        {
            int startSample = i * samplesPerBar * channels + channel;
            int endSample = Mathf.Min(startSample + samplesPerBar * channels, samples.Length);

            float min = float.MaxValue;
            float max = float.MinValue;

            for (int j = startSample; j < endSample; j += channels)
            {
                float sampleValue = Mathf.Abs(samples[j]);
                if (sampleValue < min)
                {
                    min = sampleValue;
                }
                if (sampleValue > max)
                {
                    max = sampleValue;
                }
            }

            totalDifference += max - min;
        }

        return totalDifference / numberOfBars;
    }

    void OnEnable()
    {
        // Subscribe to the event
        EventManager.OnMessageBroadcast += HandleMessage;
    }

    void OnDisable()
    {
        // Unsubscribe from the event
        EventManager.OnMessageBroadcast -= HandleMessage;
    }
    void DestroyExistingLines()
    {
        if (averageLineRenderers != null)
        {
            foreach (LineRenderer lr in averageLineRenderers)
            {
                if (lr != null)
                {
                    Destroy(lr.gameObject);
                }
            }
        }

        if (peakLineRenderers != null)
        {
            foreach (LineRenderer lr in peakLineRenderers)
            {
                if (lr != null)
                {
                    Destroy(lr.gameObject);
                }
            }
        }
    }
    void HandleMessage(string message)
    {
        // Handle the received message from EventManager

        // Example: Adjust some properties based on the message received
        if (message.Contains("Scrolled:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float scrollInput))
            {
                float xchange = scrollInput * cellWidth;

                // Update wave positions
                Vector3 localPosition = transform.localPosition;
                transform.localPosition = new Vector3(xchange+xadjust, localPosition.y, localPosition.z);
            }
        }
        if (message.Contains("cellTime:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float time))
            {
                cellTime = time;
                InitializeWaveform();
            }
        }
        if (message.Contains("CellWidthx:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float wid))
            {
                cellWidth = wid;
                resizecells();
            }
        }
        if (message.Contains("CellWidthy:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float wid))
            {
                cellHeight = wid;
            }
        }
    }
}
