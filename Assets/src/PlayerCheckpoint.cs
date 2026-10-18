using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerCheckpoint : MonoBehaviour
{
    public static PlayerCheckpoint gLastTouched = null;

    [SerializeField]
    private bool startsreached = false;
    [SerializeField]
    private GameObject inactiveobject;
    [SerializeField]
    private GameObject activeobject;
    [SerializeField]
    private AudioClipExt sfxreached;

    private bool hasreached = false;
    public bool HasReached => hasreached;

    void Start()
    {
        inactiveobject.SetActive(true);
        activeobject.SetActive(false);

        if (startsreached)
            OnReached(false);
    }

    public void OnReached(bool playsfx)
    {
        if (hasreached)
            return;

        hasreached = true;

        if(playsfx)
            AudioManager.Play2D(sfxreached);

        inactiveobject.SetActive(false);
        activeobject.SetActive(true);
        gLastTouched = this;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            OnReached(true);
        }
    }
}
