using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    [SerializeField]
    private float collectpitchtime = 3f;
    [SerializeField]
    private float collectpitchincrease = 0.05f;
    [SerializeField]
    private UIHUD uihud;

    private float collectpitchtimer = 0f;
    private int currentnotes = 0;
    private float currentpitchincrease = 1.0f;
    private bool haspitchstreak = false;

    public int CurrentNotes => currentnotes;
    public float PitchTracker => currentpitchincrease;

    void Awake()
    {
        collectpitchtimer = collectpitchtime;
    }

    void Update()
    {
        if(haspitchstreak)
        {
            collectpitchtimer -= Time.deltaTime;
            if(collectpitchtimer <= 0f)
            {
                collectpitchtimer = collectpitchtime;
                haspitchstreak = false;
                currentpitchincrease = 1f;
            }
        }
    }

    public void AddNotes(int score)
    {
        currentnotes += score;
        currentnotes = Mathf.Max(0, currentnotes);

        haspitchstreak = true;
        currentpitchincrease += collectpitchincrease;
        collectpitchtimer = collectpitchtime;

        uihud.NoteCollected();
    }
}
