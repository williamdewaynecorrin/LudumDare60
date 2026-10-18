using UnityEngine;

public class Inspectable : MonoBehaviour
{
    [SerializeField]
    private InspectableCanvas inspectable;
    [SerializeField]
    private string header;
    [SerializeField]
    private string desc;

    public string Header => header;
    public string Description => desc;
    public InspectableCanvas Canvas => inspectable;

    void Awake()
    {
        inspectable.SetData(header, desc);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Inspector>(out Inspector playerinspect))
        {
            playerinspect.OnInspectableShow(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Inspector>(out Inspector playerinspect))
        {
            playerinspect.OnInspectableHide(this);
        }
    }

    public void Show()
    {
        inspectable.SetActive(true);
    }

    public void Hide()
    {
        inspectable.SetActive(false);
    }
}
