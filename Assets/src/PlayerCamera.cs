using UnityEngine;

[DefaultExecutionOrder(ExecutionOrders.PLAYER_CAMERA)]
public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private PlayerController player;
    [SerializeField]
    private Vector3 startingoffset;
    [SerializeField]
    private float sensitivityyaw = 1.0f;
    [SerializeField]
    private float forcedyawthresh = 0.25f;
    [SerializeField]
    private float sensitivitypitch = 1.0f;
    [SerializeField]
    private bool autocamenabled = false;
    [SerializeField]
    private float smoothspeed = 1.0f;
    [SerializeField]
    private float facingplayerdotthresh = 0.0f;
    [SerializeField]
    private float smoothrotationspeed = 1.0f;
    [Range(-1f, 1f)]
    [SerializeField]
    private float rotationverticalcontribution = 0.1f;

    private Vector3 targetoffset;
    private Vector3 velocityref;
    private Quaternion startingplayerrot;
    private Vector3 playermovementforward;
    private Vector3 playermovementright;
    private float manualyaw = 0f;
    private float forcedyaw = 0f;

    public Vector3 PlayerMovementForward => playermovementforward;
    public Vector3 PlayerMovementRight => playermovementright;

    void Awake()
    {
        transform.position = player.CameraTarget + startingoffset;
        startingplayerrot = player.transform.localRotation;
    }

    void LateUpdate()
    {
        // -- rotation early
        if(Inputs.RotateCameraHold())
        {
            forcedyaw += Inputs.RotateCameraYAW() * sensitivityyaw;
        }

        Vector3 targetselfpos = transform.position;
        if (autocamenabled)
        {
            // -- rotation late
            // -- try to detect if the camera should auto-target behind the player's back
            if (player.CurrentSpeed > 0.01f && Vector3.Dot(player.transform.forward.NoY().normalized, playermovementforward) > facingplayerdotthresh)
            {
                Quaternion diff = player.transform.localRotation * Quaternion.Inverse(startingplayerrot);
                forcedyaw = diff.eulerAngles.y;
            }
        }   

        Vector3 totarg = player.CameraTarget - targetselfpos + Vector3.up * rotationverticalcontribution;
        Vector3 lookat = totarg.normalized;
        Quaternion targetrotation = Quaternion.LookRotation(lookat, Vector3.up);

        Vector3 lrotvel = Vector3.zero;
        transform.localRotation = Extensions.SmoothDampQuat(transform.localRotation, targetrotation, ref lrotvel, smoothrotationspeed);

        Quaternion yawaddrot = Quaternion.Euler(0f, forcedyaw, 0f);

        // -- position
        switch(player.State)
        {
            case EPlayerState.eFree:
                {
                    Vector3 rotatedoffset = yawaddrot * startingoffset;
                    targetoffset = player.CameraTarget + rotatedoffset;

                    Vector3 next = Vector3.SmoothDamp(transform.position, targetoffset, ref velocityref, smoothspeed);
                    transform.position = next;

                    playermovementforward = transform.forward.NoY().normalized;
                    playermovementright = transform.right.NoY().normalized;
                    break;
                }
        }
    }

    void OnGUI()
    {
        const float debuglineheight = 25f;
        Rect debugrect = new Rect(5, 400f, 250f, debuglineheight);

        GUI.Label(debugrect, string.Format("Starting Y -> {0}", startingplayerrot.eulerAngles.y.ToString("F1")));
        debugrect.y += debuglineheight;
        GUI.Label(debugrect, string.Format("Current Y -> {0}", player.transform.localRotation.eulerAngles.y.ToString("F1")));
        debugrect.y += debuglineheight;
    }
}
