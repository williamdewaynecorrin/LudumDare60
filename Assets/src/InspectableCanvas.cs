using TMPro;
using UnityEngine;

public class InspectableCanvas : MonoBehaviour
{
    private static readonly Vector3 kScaleZero = new Vector3(0f, 1f, 0f);

    [SerializeField]
    private TMP_Text textheader;
    [SerializeField]
    private TMP_Text textdesc;
    [SerializeField]
    private bool onlyyaw = true;
    [SerializeField]
    [Range(0f, 1f)]
    private float openlerp = 0.1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float closelerp = 0.1f;
    [SerializeField]
    private AudioClipExt sfxactivate;
    [SerializeField]
    private AudioClipExt sfxclose;

    private bool isactive = false;
    private Vector3 initiallocalpos;
    private Vector3 initialscale;

    void Awake()
    {
        isactive = false;
        initiallocalpos = transform.localPosition;
        initialscale = transform.localScale;
        transform.localScale = kScaleZero;
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.rotation.eulerAngles.y, 0f));
    }

    void FixedUpdate()
    {
        if(isactive)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, initialscale, openlerp);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, kScaleZero, closelerp);
        }
    }

    public void SetData(string header, string desc)
    {
        textheader.text = header;
        textdesc.text = desc;
    }

    public void SetActive(bool isactive)
    {
        this.isactive = isactive;

        if(isactive)
        {
            AudioManager.Play2D(sfxactivate);
        }
        else
        {
            AudioManager.Play2D(sfxclose);
        }
    }
}
