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
            player.AddScore(score);

            AudioManager.Play2D(sfxcollect);
            GameObject.Destroy(this.gameObject);
        }
    }
}
