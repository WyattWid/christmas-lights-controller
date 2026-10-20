using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private LineRenderer lineRenderer;
    public GameObject audioPrefab;
    public float selectionAdjust = 0.2f;
    public float lineWidth;
    public Color barColor = Color.gray;
    private float lastTime;
    private float xchange = 0f;
    private float cellTime = 0.1f;
    private float selectionStart;
    private float selectionEnd;
    private bool isPlayingSelection = false;
    private bool isPlayingSelectionDistance = false;
    private float screenRight;
    private float cellWidth;
    private bool playing = false;
    public float playbackSpeed = 1f;
    public float volume = 1f; // Range between 0 (mute) to 1 (max volume)


    void Start()
    {
        Application.targetFrameRate = 1000; // Set to a higher frame rate if needed
        audioSource = audioPrefab.GetComponent<AudioSource>();
        audioSource.volume = volume;
        lineRenderer = GetComponent<LineRenderer>();

        lastTime = 0f;

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing.");
            return;
        }

        screenRight = Screen.width / 2f;

        // Initialize LineRenderer properties once
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = barColor;
        lineRenderer.endColor = barColor;
        lineRenderer.useWorldSpace = false;
    }

    public void Play()
    {
        if (audioSource != null)
        {
            lastTime = xchange * cellTime * -10f;
            GoTo(lastTime);
        }
    }

    public void PlayAll()
    {
        if (audioSource != null)
        {
            GoTo(0f);
            xchange = 0;
            EventManager.BroadcastMessage("beginning");
        }
    }

    public void PlayScreen()
    {
        float screenStart = xchange * cellTime * -10f;
        isPlayingSelectionDistance = true;
        GoTo(screenStart);
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            lastTime = Mathf.Floor(audioSource.time * 10) / 10;
            audioSource.Stop();
            isPlayingSelection = false;
            isPlayingSelectionDistance = false;
            playing = false;
            lineRenderer.positionCount = 0; // Make the line disappear
        }
    }

    public void GoTo(float time)
    {
        if (audioSource != null)
        {
            if (time >= 0 && time <= audioSource.clip.length)
            {
                audioSource.time = time;
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                    playing = true;
                }
            }
            else
            {
                Debug.LogWarning("Time is out of range.");
            }
        }
    }

    public void SelectStart()
    {
        if (audioSource != null)
        {
            float tempStart = Mathf.Min(selectionStart, selectionEnd);
            float start = xchange * cellTime * -10f;
            float selectStartTime = (tempStart * cellTime) + start;
            GoTo(selectStartTime);
        }
    }

    public void PlaySelection()
    {
        if (audioSource != null)
        {
            float tempStart = Mathf.Min(selectionStart, selectionEnd) - 3f;
            float tempEnd = Mathf.Max(selectionStart, selectionEnd);
            float dif = tempEnd - tempStart - 2;

            float start = xchange * cellTime * -10f;
            float selectStartTime = (tempStart * cellTime) + selectionAdjust + (start-(cellTime*1.3f))*0;
            Debug.Log(selectStartTime);
            float selectEndTime = (dif * cellTime) + selectionAdjust + selectStartTime;
            Debug.Log(selectEndTime);

            if (selectStartTime >= 0 && selectEndTime <= audioSource.clip.length)
            {
                isPlayingSelection = true;
                GoTo(selectStartTime);
            }
            else if (selectStartTime == selectEndTime)
            {
                isPlayingSelection = true;
                selectionEnd += cellTime;
                GoTo(selectStartTime);
            }
        }
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        if (audioSource != null)
        {
            audioSource.pitch = speed; // Set pitch temporarily to achieve the speed
            // Adjust the playback speed without changing the pitch
            if (audioSource.outputAudioMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup.audioMixer.SetFloat("PitchShift", 1f / speed);
            }
        }
    }

    void Update()
    {
        SetPlaybackSpeed(playbackSpeed); // Play at the desired speed
        if (Input.GetKeyDown(KeyCode.B))
        {
            Play();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayAll();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            PlaySelection();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayScreen();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playing)
            {
                Stop();
            }
            else
            {
                PlaySelection();
            }
        }

        float start = xchange * cellTime * -10f;
        float selectStartTime = (selectionStart * cellTime) + start;
        float selectEndTime = (selectionEnd * cellTime) + start;

        if (isPlayingSelection && audioSource.time >= selectEndTime)
        {
            Stop();
        }
    }

    void FixedUpdate()
{
    if (playing)
    {
        DrawLine();
    }
}

    public void DrawLine()
    {
        float adTime = ((audioSource.time)/(10*cellTime));
        
        adTime = (adTime * 10f * cellWidth) - (5f+(-xchange*cellWidth*10));
        if (lineRenderer.positionCount != 2)
        {
            lineRenderer.positionCount = 2;
        }
        if (adTime >= 9.8f)
        {
            if (isPlayingSelectionDistance)
            {
                Stop();
                return;
            }
            else
            {
                
                EventManager.BroadcastMessage("moveTiles");
                return;
                
            }
        }

        
        lineRenderer.SetPosition(0, new Vector3(adTime, 0.86f, -5));
        lineRenderer.SetPosition(1, new Vector3(adTime, -3.34f, -5));
    }

    void OnEnable()
    {
        EventManager.OnMessageBroadcast += HandleMessage;
    }

    void OnDisable()
    {
        EventManager.OnMessageBroadcast -= HandleMessage;
    }

    void HandleMessage(string message)
    {
        if (message.Contains("Scrolled:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float scrollInput))
            {
                xchange = scrollInput * cellTime;
            }
        }

        if (message.Contains("cellTime:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float time))
            {
                cellTime = time;
            }
        }

        if (message.Contains("SelectionStart:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float start))
            {
                selectionStart = start + 1f;
            }
        }

        if (message.Contains("SelectionEnd:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float end))
            {
                selectionEnd = end + 1f;
            }
        }

        if (message.Contains("CellWidthx:"))
        {
            string[] splits = message.Split(':');
            if (splits.Length > 1 && float.TryParse(splits[1], out float wid))
            {
                cellWidth = wid;
            }
        }
        if (message.Contains("all"))
        {
            PlayAll();
        }
        if (message.Contains("heretoend"))
        {
            Play();
        }
        if (message.Contains("selection"))
        {
            PlaySelection();
        }
        if (message.Contains("visible"))
        {
            PlayScreen();
        }
    }
}