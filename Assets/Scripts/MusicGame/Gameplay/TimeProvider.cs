using Takana3.MusicGame.LevelSelect;
using UnityEngine;
using UnityEngine.Audio;
using static Takana3.MusicGame.Values;

// 用AudioSettings.dspTime来计算时间的做法目前没有找到比较好的办法来兼容不同倍速，所以先摆烂了？
public class TimeProvider : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private float _audioTime = 0f;
    [SerializeField] private float audioDeviation = 0f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup PitchShiftMixerGroup;
    [SerializeField] private AudioMixerGroup DoublePitchShiftMixerGroup;

    public static TimeProvider Instance => _instance;

    public float AudioTime
    {
        get => _audioTime;
        set
        {
            value = Mathf.Max(Mathf.Min(AudioLength - 0.001f, value), 0);
            _audioTime = value;
            dspTimeStart = (float)AudioSettings.dspTime - _audioTime;
            audioSource.time = value + timeScheduled;
            audioSource.Stop();
            if (!isPaused) audioSource.PlayDelayed(timeScheduled);
        }
    }

    public float ChartTime
    {
        get => _audioTime - audioDeviation - Offset;
        set => AudioTime = value + audioDeviation + Offset;
    }

    public float AudioLength { get; private set; } = 0f;

    public float Offset { get; set; } = 0f;

    public float PlaybackSpeed
    {
        get => _playbackSpeed;
        set
        {
            audioSource.pitch = value;
            //if (value == 1)
            //{
            //    audioSource.outputAudioMixerGroup = null;
            //}
            //else if (value >= 0.5f)
            //{
            //    audioSource.outputAudioMixerGroup = PitchShiftMixerGroup;
            //    PitchShiftMixerGroup.audioMixer.SetFloat("pitchShift", 1f / value);
            //}
            //else
            //{
            //    audioSource.outputAudioMixerGroup = DoublePitchShiftMixerGroup;
            //    PitchShiftMixerGroup.audioMixer.SetFloat("pitchShift", 2f);
            //    PitchShiftMixerGroup.audioMixer.SetFloat("pitchShift2", 0.5f / value);
            //}
            _playbackSpeed = value;
        }
    }

    public float Volume
    {
        get => audioSource.volume;
        set
        {
            float volume = Mathf.Max(0f, Mathf.Min(1f, value));
            audioSource.volume = volume;
        }
    }

    // Private
    private LevelInfo levelInfo;
    private bool isPaused = true;
    private float dspTimeStart = 0f;
    private float _playbackSpeed = 1.0f;

    // Static
    const float timeScheduled = 0.030f; // 用于AudioSource.PlayScheduled().

    private static TimeProvider _instance = null;

    // Defined Functions
    private void Init()
    {
        audioSource.time = 0f;
        audioDeviation = levelInfo.MusicSetting.AudioDeviation;

        dspTimeStart = (float)AudioSettings.dspTime + TimePreAnimation;
        _audioTime = (float)AudioSettings.dspTime - dspTimeStart;

        isPaused = true;
    }

    private void Pause()
    {
        _audioTime = audioSource.time;
        audioSource.Stop();
        audioSource.time = _audioTime;
        //audioSource.time = Mathf.Min(_audioTime + timeScheduled, AudioLength - 0.001f);

        isPaused = true;
    }

    private void Resume()
    {
        if (_audioTime < 0f) audioSource.PlayDelayed(timeScheduled - _audioTime);
        else audioSource.PlayDelayed(timeScheduled);
        dspTimeStart = (float)AudioSettings.dspTime - _audioTime + timeScheduled;

        isPaused = false;
    }

    public float AudioToChartTime(float audioTime) => audioTime - audioDeviation - Offset;

    public float ChartToAudioTime(float chartTime) => chartTime + audioDeviation + Offset;

    // 根据进度条修改时间

    // System Functions
    void Awake()
    {
        _instance = this;
        EventManager.AddListener(EventManager.EventName.LevelInit, Init);
        EventManager.AddListener(EventManager.EventName.Pause, Pause);
        EventManager.AddListener(EventManager.EventName.Resume, Resume);
    }

    void OnEnable()
    {
        levelInfo = InfoReader.ReadInfo<LevelInfo>();
        if (levelInfo == null)
        {
            SongInfo songInfo = new SongList().GetSongInfo(0);
            levelInfo = new(songInfo, 2);
            InfoReader.SetInfo(levelInfo);
            Debug.Log("default levelinfo");
        }
        Offset = levelInfo.ChartInfo.Offset;
        audioSource.clip = levelInfo.Music;
        AudioLength = levelInfo.Music.length;
    }

    void Update()
    {
        if (!isPaused)
        {
            // 摆烂了
            //if (PlaybackSpeed == 1.0f) _audioTime = (float)(AudioSettings.dspTime - dspTimeStart);
            _audioTime = audioSource.time;
            if (_audioTime > AudioLength - 0.001f && !audioSource.isPlaying) EventManager.Trigger(EventManager.EventName.Pause);
        }
    }
}
