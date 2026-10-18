using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    private int currentnotes = 0;

    public int CurrentNotes => currentnotes;

    public void AddNotes(int score)
    {
        currentnotes += score;
        currentnotes = Mathf.Max(0, currentnotes);
    }
}
