using UnityEngine;

public class SinRotate : MonoBehaviour
{
    [SerializeField]
    private float amplitude = 1.0f;
    [SerializeField]
    private float period = 1.0f;
    [SerializeField]
    private float phase = 0.0f;
    [SerializeField]
    private Vector3 axis = Vector3.forward;

    private Quaternion startingrotation;

    void Awake()
    {
        startingrotation = transform.localRotation;
    }

    void Update()
    {
        float sin = Mathf.Sin(Time.time * period + phase * period) * amplitude;
        transform.localRotation = startingrotation * Quaternion.AngleAxis(sin, axis);
    }
}
