using UnityEngine;

public class BouncePad : MonoBehaviour
{
    private const string kBounceAnimState = "Bounce";

    [SerializeField]
    private float jumpmult = 0.75f;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private AudioClipExt sfxbounce;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            Extensions.PlayAnimationDirect(ref anim, kBounceAnimState, SAnimatorLayer.kBaseLayer);
            player.JumpExternal(jumpmult);

            AudioManager.Play2D(sfxbounce);
        }
    }
}
