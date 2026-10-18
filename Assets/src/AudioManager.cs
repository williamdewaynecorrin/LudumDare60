using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance => instance;
    private static AudioManager instance = null;

    public static float gMasterVolumeMult = 1.0f;
    public static float gSFXVolumeMult = 1.0f * gMasterVolumeMult;
    public static float gMusicVolumeMult = 1.0f * gMasterVolumeMult;

    [SerializeField]
    private AudioSource musicmain;
    [SerializeField]
    private AudioSource musicboom;
    [Range(0f, 1f)]
    [SerializeField]
    private float boomlerp = 0.1f;

    private float targetboomamount = 0f;
    private float currentboomamount = 0;
    private PlayerController player;

    void Awake()
    {
        musicmain.loop = true;
        musicboom.loop = true;
        musicmain.Play();
        musicboom.Play();

        instance = this;
    }

    void Start()
    {
        player = GameObject.FindFirstObjectByType<PlayerController>();
        if (player == null)
            Debug.LogErrorFormat("AudioManager couldn't find a PlayerController object in the scene!");
    }

    void FixedUpdate()
    {
        currentboomamount = Mathf.Lerp(currentboomamount, targetboomamount, boomlerp);

        float mainvol = 1.0f - currentboomamount;
        musicmain.volume = mainvol *  gMusicVolumeMult;
        musicboom.volume = currentboomamount * gMusicVolumeMult;

        if(player != null)
            musicboom.transform.position = player.transform.position;
    }

    public void SetBoomAmount(float boomamt)
    {
        targetboomamount = Mathf.Clamp01(boomamt);
    }

    public static AudioSource Play2D(AudioClipExt clipdata, float volumemult = 1.0f, float pitch = 1.0f, ulong delay = 0u)
    {
        if (!clipdata.IsValid())
            return null;

        AudioSource source = new GameObject(string.Format("audio_oneshot_{0}", clipdata.clip.name)).AddComponent<AudioSource>();
        source.spatialBlend = 0.0f;

        source.clip = clipdata.clip;
        source.volume = volumemult * clipdata.volumemult * gSFXVolumeMult;
        source.pitch = pitch * clipdata.pitch.PickValue();

        source.Play(delay);
        GameObject.Destroy(source.gameObject, clipdata.clip.length);

        return source;
    }

    public static AudioSource Play3D(AudioClipExt clipdata, Vector3 pos, float volumemult = 1.0f, float pitch = 1.0f, ulong delay = 0u)
    {
        if (!clipdata.IsValid())
            return null;

        AudioSource source = new GameObject(string.Format("audio_oneshot_{0}", clipdata.clip.name)).AddComponent<AudioSource>();
        source.transform.position = pos;
        source.spatialBlend = 1.0f;

        source.clip = clipdata.clip;
        source.volume = volumemult * clipdata.volumemult * gSFXVolumeMult;
        source.pitch = pitch * clipdata.pitch.PickValue();

        source.Play(delay);
        GameObject.Destroy(source.gameObject, clipdata.clip.length);

        return source;
    }
}

[System.Serializable]
public class AudioClipExt
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volumemult = 1.0f;
    public MinMaxFloat pitch;

    public bool IsValid()
    {
        return clip != null;
    }
}

[System.Serializable]
public class MinMaxFloat
{
    public float min = 1f;
    public float max = 1f;

    private float lastpickedvalue = 0f;
    public float LastPickedValue => lastpickedvalue;

    public float PickValue()
    {
        lastpickedvalue = Random.Range(min, max);
        return lastpickedvalue;
    }
}