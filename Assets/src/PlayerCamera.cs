using UnityEngine;

[DefaultExecutionOrder(ExecutionOrders.PLAYER_CAMERA)]
public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private PlayerController player;
    [SerializeField]
    private Vector3 startingoffset;
    [SerializeField]
    private float smoothspeed = 1.0f;

    private Vector3 targetoffset;
    private Vector3 velocityref;
    private Vector3 playermovementforward;
    private Vector3 playermovementright;

    public Vector3 PlayerMovementForward => playermovementforward;
    public Vector3 PlayerMovementRight => playermovementright;

    void Awake()
    {
        transform.position = player.CameraTarget + startingoffset;
    }

    void Update()
    {
        Vector3 rotatedoffset = startingoffset;
        targetoffset = player.CameraTarget + rotatedoffset;

        Vector3 next = Vector3.SmoothDamp(transform.position, targetoffset, ref velocityref, smoothspeed);
        transform.position = next;

        playermovementforward = transform.forward.NoY().normalized;
        playermovementright = transform.right.NoY().normalized;
    }
}
