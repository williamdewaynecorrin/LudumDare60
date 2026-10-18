using UnityEngine;

public class RadioObject : MonoBehaviour
{
    private static int gRadioInstanceCount = 0;

    [SerializeField]
    private Inspectable inspect;
    [SerializeField]
    private int noterequirement = 1;
    [SerializeField]
    private AudioClipExt sfxrestore;

    private bool isrestored = false;
    private UIHUD uihud;
    private int radioid;

    public int NoteRequirement => noterequirement;

    void Start()
    {
        UpdateUI();
        uihud = GameObject.FindFirstObjectByType<UIHUD>();
        if(uihud == null)
        {
            Debug.LogErrorFormat("Failed to find valid PlayerScore component in scene!");
        }

        radioid = gRadioInstanceCount++;
        uihud.RegisterRadio(radioid);
    }

    public void RestoreRadio()
    {
        if (isrestored)
            return;

        AudioManager.Play2D(sfxrestore);
        isrestored = true;
        UpdateUI();
        uihud.SetRadioRepaired(radioid);
    }

    private void UpdateUI()
    {
        string radiostr = "";
        string radiodesc = "";
        if (isrestored)
        {
            radiostr = Extensions.ColoredString("RADIO - FIXED", Color.green);
            radiodesc = string.Format("{0}A clean, fixed-up radio from the 1970s", Extensions.kGlyphIDGreenCheck);
        }
        else
        {
            radiostr = Extensions.ColoredString("RADIO - DEAD", Color.red);
            radiodesc = "A broken-down, fixed-up radio from the 1970s";
            radiodesc = string.Format("{0}Requires {1}{2} to repair", Extensions.kGlyphIDRedX, Extensions.kGlyphIDNote, noterequirement);
        }

        inspect.Canvas.SetData(radiostr, radiodesc);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.SetCurrentRadio(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.SetCurrentRadio(null);
        }
    }
}
