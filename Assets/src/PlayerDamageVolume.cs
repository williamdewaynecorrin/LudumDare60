using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerDamageVolume : MonoBehaviour
{
    [SerializeField]
    private bool causesjump = true;
    [SerializeField]
    private float jumpmult = 0.75f;
    [SerializeField]
    private int damage = 10;
    [SerializeField]
    private AudioClipExt sfxhit;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.Health.TakeDamage(damage);
            AudioManager.Play2D(sfxhit);

            if(causesjump)
            {
                player.JumpExternal(jumpmult);
            }
        }
    }
}
