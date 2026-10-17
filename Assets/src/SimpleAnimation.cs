using UnityEngine;

public class SimpleAnimation : MonoBehaviour
{
    [SerializeField]
    private Vector3 bobaxis = Vector3.up;
    [SerializeField]
    private float bobamplitude = 1.0f;
    [SerializeField]
    private float bobperiod = 1.0f;
    [SerializeField]
    private bool absbounce = false;
    [SerializeField]
    private Vector3 rotationaxis = Vector3.up;
    [SerializeField]
    private float rotationspeed = 1.0f;

    private float currenttime = 0f;
    private float currentrotation = 0f;
    private Vector3 initialpos;
    private Quaternion initialrot;

    void Awake()
    {
        initialpos = transform.localPosition;
        initialrot = transform.localRotation;
    }

    void Update()
    {
        // -- pos
        currenttime += Time.deltaTime;
        float bobamount = Mathf.Sin(currenttime * bobperiod) * bobamplitude;
        if (absbounce)
            bobamount = Mathf.Abs(bobamount);

        transform.localPosition = initialpos + bobaxis * bobamount;

        // -- rot
        currentrotation += rotationspeed * Time.deltaTime;
        transform.localRotation = initialrot * Quaternion.AngleAxis(currentrotation, rotationaxis);
    }
}
