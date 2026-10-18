using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    [SerializeField]
    private float rotatespeed = 10f;
    [SerializeField]
    private Vector3 rotateaxis = Vector3.up;
    [SerializeField]
    private Transform childattach;

    private Vector3 lastpos;
    private Quaternion lastrot;

    private Vector3 deltapos;
    private Quaternion deltarot;

    public float RotateSpeed => rotatespeed;
    public Vector3 RotateAxis => rotateaxis;
    public Vector3 DeltaPos => deltapos;
    public Quaternion DeltaRot => deltarot;
    public Transform ChildAttach => childattach;

    void FixedUpdate()
    {
        transform.localRotation *= Quaternion.AngleAxis(rotatespeed * Time.fixedDeltaTime, rotateaxis);

        deltapos = transform.position - lastpos;
        deltarot = transform.localRotation * Quaternion.Inverse(lastrot);

        lastpos = transform.position;
        lastrot = transform.localRotation;
    }
}
