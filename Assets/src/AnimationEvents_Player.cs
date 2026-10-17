using UnityEngine;

public class AnimationEvents_Player : MonoBehaviour
{
    [SerializeField]
    private PlayerController player;
    [SerializeField]
    private AudioClipExt sfxstepleft;
    [SerializeField]
    private AudioClipExt sfxstepright;

    public void OnFootstepLeft()
    {
        if (!player.IsGrounded)
            return;

        AudioManager.Play2D(sfxstepleft);
    }

    public void OnFootstepRight()
    {
        if (!player.IsGrounded)
            return;

        AudioManager.Play2D(sfxstepright);
    }
}
