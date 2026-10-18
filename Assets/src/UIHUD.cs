using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIHUD : MonoBehaviour
{
    [SerializeField]
    private PlayerScore score;
    [SerializeField]
    private PlayerController player;
    [SerializeField]
    private TMP_Text scoretext;
    [SerializeField]
    private GameObject deathobject;
    [SerializeField]
    private GameObject winobject;
    [SerializeField]
    private Transform radiosparent;
    [SerializeField]
    private GameObject radioprefab;
    [SerializeField]
    private TMP_Text onofftext;
    [SerializeField]
    private TutorialEntry[] tutorialentries;
    [SerializeField]
    private AudioClipExt sfxwin;
    [SerializeField]
    private AudioClipExt sfxlose;

    private Dictionary<int, Image> radiodict = new Dictionary<int, Image>();
    private int tutorialidx = 0;
    private bool anynotecollected;
    private bool anyradiorestored;

    void Start()
    {
        SetOff();
        deathobject.SetActive(false);
        winobject.SetActive(false);

        tutorialentries[0].completioncheck += ()=> Inputs.RotateCameraPitch() + Inputs.RotateCameraYAW() != 0f;
        tutorialentries[1].completioncheck += () => Inputs.HorizontalMove() + Inputs.VerticalMove() != 0f;
        tutorialentries[2].completioncheck += () => anynotecollected == true;
        tutorialentries[3].completioncheck += () => player.State == EPlayerState.eBoomin;
        tutorialentries[4].completioncheck += () => anyradiorestored == true;

        tutorialidx = -1;

        StartCoroutine(ShowTutorial());
    }

    void Update()
    {
        scoretext.text = string.Format("{0}{1}", Extensions.kGlyphIDNote, score.CurrentNotes.ToString());

        if(tutorialidx >= 0 && tutorialidx < tutorialentries.Length)
        {
            if (tutorialentries[tutorialidx].completioncheck())
            {
                tutorialentries[tutorialidx].Hide();
                ++tutorialidx;

                if(tutorialidx < tutorialentries.Length)
                    tutorialentries[tutorialidx].Show();
            }
        }
    }

    private IEnumerator ShowTutorial()
    {
        yield return new WaitForSeconds(0.25f);
        tutorialidx = 0;
        tutorialentries[tutorialidx].Show();
    }

    public void SetDead()
    {
        if (winobject.activeInHierarchy)
            return;

        deathobject.SetActive(true);
        AudioManager.Play2D(sfxlose);
    }

    public void SetWin()
    {
        if (deathobject.activeInHierarchy)
            return;

        winobject.SetActive(true);
    }

    public void NoteCollected()
    {
        if (tutorialidx >= 2)
            anynotecollected = true;
    }

    public void RegisterRadio(int id)
    {
        if (radiodict.ContainsKey(id))
        {
            Debug.LogErrorFormat("Id {0} already is registered!", id);
            return;
        }

        Image inst = GameObject.Instantiate(radioprefab, radiosparent).GetComponent<Image>();
        inst.color = Color.black;
        radiodict.Add(id, inst);
    }

    public void SetRadioRepaired(int id)
    {
        if (!radiodict.ContainsKey(id))
        {
            Debug.LogErrorFormat("Id {0} is not registered!", id);
            return;
        }

        anyradiorestored = true;
        radiodict[id].color = Color.white;

        CheckGameWin();
    }

    private void CheckGameWin()
    {
        bool success = true;
        foreach(int key in radiodict.Keys)
        {
            if (radiodict[key].color == Color.black)
            {
                success = false;
                break;
            }
        }

        if(success)
        {
            SetWin();
            player.SetWin();
            AudioManager.Play2D(sfxwin);
        }
    }

    public void SetOff()
    {
        onofftext.text = Extensions.ColoredString("OFF", Color.red);
    }

    public void SetOn()
    {
        onofftext.text = Extensions.ColoredString("ON", Color.green);
    }
}

public delegate bool TBooleanArg0();

[System.Serializable]
public class TutorialEntry
{
    public GameObject graphics;
    public TBooleanArg0 completioncheck;

    public void Show()
    {
        graphics.SetActive(true);
    }

    public void Hide()
    {
        graphics.SetActive(false);
    }
}