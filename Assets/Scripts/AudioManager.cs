using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Egg")]
    public AudioClip[] eggCrush;    // an enemy breaks the egg
    [Range(0f, 1f)] public float eggCrushVolume = 1f;
    public AudioClip[] eggCrack;    // the egg hatching
    [Range(0f, 1f)] public float eggCrackVolume = 1f;

    [Header("Enemies scared away")]
    public AudioClip[] seagullScare;
    [Range(0f, 1f)] public float seagullVolume = 1f;
    public AudioClip[] crabScare;
    [Range(0f, 1f)] public float crabVolume = 1f;

    [Header("Other")]
    public AudioClip[] splash;
    [Range(0f, 1f)] public float splashVolume = 1f;
    public AudioClip[] gameOver;
    [Range(0f, 1f)] public float gameOverVolume = 1f;

    [Header("Overall")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    public float pitchVariation = 0.05f; // tiny random pitch change so repeats don't sound identical
    public int voices = 8;               // how many sounds can overlap

    AudioSource[] pool;
    int next;

    void Awake()
    {
        Instance = this;

        pool = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 0f; // plain 2D sound
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }


    public static void PlayEggCrush()
    {
        if (Instance != null) Instance.Play(Instance.eggCrush, Instance.eggCrushVolume);
    }

    public static void PlayHatch()
    {
        if (Instance != null) Instance.Play(Instance.eggCrack, Instance.eggCrackVolume);
    }

    public static void PlayScare(bool isSeagull)
    {
        if (Instance == null) return;

        if (isSeagull) Instance.Play(Instance.seagullScare, Instance.seagullVolume);
        else Instance.Play(Instance.crabScare, Instance.crabVolume);
    }

    public static void PlaySplash()
    {
        if (Instance != null) Instance.Play(Instance.splash, Instance.splashVolume);
    }

    public static void PlayGameOver()
    {
        if (Instance != null) Instance.Play(Instance.gameOver, Instance.gameOverVolume);
    }


    void Play(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        AudioSource s = pool[next];
        next = (next + 1) % pool.Length;

        s.clip = clip;
        s.volume = volume * masterVolume;
        s.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        s.Play();
    }
}