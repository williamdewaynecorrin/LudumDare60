using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHUD : MonoBehaviour
{
    [SerializeField]
    private PlayerScore score;
    [SerializeField]
    private TMP_Text scoretext;
    [SerializeField]
    private Transform radiosparent;
    [SerializeField]
    private GameObject radioprefab;

    private Dictionary<int, Image> radiodict = new Dictionary<int, Image>();

    void Start()
    {
    }

    void Update()
    {
        scoretext.text = score.CurrentNotes.ToString();
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

        radiodict[id].color = Color.white;
    }
}
