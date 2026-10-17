using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    private int currentscore = 0;

    public void AddScore(int score)
    {
        currentscore += score;
        currentscore = Mathf.Max(0, currentscore);
    }
}
