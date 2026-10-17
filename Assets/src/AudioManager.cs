using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static float gMasterVolumeMult = 1.0f;
    public static float gSFXVolumeMult = 1.0f * gMasterVolumeMult;
    public static float gMusicVolumeMult = 1.0f * gMasterVolumeMult;

    public AudioSource Play2D(AudioClipExt clipdata, float volumemult = 1.0f, float pitch = 1.0f, ulong delay = 0u)
    {
        if (!clipdata.IsValid())
            return null;

        AudioSource source = new GameObject(string.Format("audio_oneshot_{0}", clipdata.clip.name)).AddComponent<AudioSource>();
        source.spatialBlend = 0.0f;

        source.volume = volumemult * clipdata.volumemult * gSFXVolumeMult;
        source.pitch = pitch * clipdata.pitch.PickValue();

        source.Play(delay);
        GameObject.Destroy(source.gameObject, clipdata.clip.length);

        return source;
    }

    public AudioSource Play3D(AudioClipExt clipdata, Vector3 pos, float volumemult = 1.0f, float pitch = 1.0f, ulong delay = 0u)
    {
        if (!clipdata.IsValid())
            return null;

        AudioSource source = new GameObject(string.Format("audio_oneshot_{0}", clipdata.clip.name)).AddComponent<AudioSource>();
        source.transform.position = pos;
        source.spatialBlend = 1.0f;

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