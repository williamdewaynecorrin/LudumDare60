using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField]
    private int score = 1;
    [SerializeField]
    private AudioClipExt sfxcollect;

    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerScore>(out PlayerScore player))
        {
            player.AddNotes(score);

            AudioManager.Play2D(sfxcollect, 1.0f, player.PitchTracker);
            GameObject.Destroy(this.gameObject);
        }
    }
}
