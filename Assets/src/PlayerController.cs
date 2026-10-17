using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField]
    private PlayerCamera playercamera;
    [SerializeField]
    private CharacterController character;

    [Header("Movement + Turning")]
    [SerializeField]
    private float acceleration = 1.0f;
    [SerializeField]
    private float maxspeed = 5.0f;
    [SerializeField]
    private float stopsmoothing = 0.5f;
    [SerializeField]
    private float turnsmoothing = 0.1f;

    [Header("Gravity + Jumping")]
    [SerializeField]
    private LayerMask groundmask;
    [SerializeField]
    private float mingroundcastdist = 0.1f;
    [SerializeField]
    private float gravityforce = 0.1f;
    [SerializeField]
    private float jumpforce = 1.0f;
    [SerializeField]
    private float coyotetimeseconds = 0.1f;

    [Header("SFX")]
    [SerializeField]
    private AudioClipExt sfxjump;
    [SerializeField]
    private AudioClipExt sfxland;

    private Vector3 currentvelocity = Vector3.zero;
    private float currentgravity = 0f;
    private EPlayerState state = EPlayerState.eFree;
    private bool grounded = true;
    private bool jumped = false;
    private Coroutine coyotetimethread = null;
    private Vector3 movedir;

    public bool CanJump => !jumped && (grounded || coyotetimethread != null);
    public Vector3 CameraTarget => transform.position + Vector3.up * character.height * 0.5f;

    void Awake()
    {
        movedir = transform.forward;
    }

    void Update()
    {
        if (state == EPlayerState.eFrozen || state == EPlayerState.eDead)
            return;

        // -- vertical plane movement
        GetCapsuleData(out Vector3 p1, out Vector3 p2, out float grounddist);
        if (currentgravity >= 0f && Physics.CapsuleCast(p1, p2, character.radius, Vector3.down, grounddist, groundmask, QueryTriggerInteraction.Ignore))
        {
            currentgravity = 0f;

            if(!grounded)
            {
                // -- just landed
            }

            grounded = true;
        }
        else
        {
            if(grounded)
            {
                // -- just left
                if (coyotetimethread != null)
                    StopCoroutine(coyotetimethread);

                coyotetimethread = StartCoroutine(CoyoteTimeRoutine());
            }

            currentgravity += gravityforce * Time.deltaTime;
            grounded = false;
        }

        CollisionFlags gravityflags = character.Move(Vector3.down * currentgravity);

        // -- xz plane movement
        Vector2 currentinput = new Vector2(Inputs.HorizontalMove(), Inputs.VerticalMove());
        Vector3 framemove = playercamera.PlayerMovementForward * currentinput.y + playercamera.PlayerMovementRight * currentinput.x;
        framemove *= acceleration * Time.deltaTime;

        if (currentinput == Vector2.zero)
        {
            // -- stop
            Vector3 stopvel = Vector3.zero;
            currentvelocity = Vector3.SmoothDamp(currentvelocity, Vector3.zero, ref stopvel, stopsmoothing);
        }
        else
        {
            currentvelocity += framemove;
            currentvelocity = Vector3.ClampMagnitude(currentvelocity, maxspeed);
        }

        CollisionFlags moveflags = character.Move(currentvelocity);

        // -- turn to face movedir
        if (framemove != Vector3.zero)
            movedir = framemove.normalized;

        Vector3 rotvel = Vector3.zero;
        Quaternion targetrot = Quaternion.LookRotation(movedir, Vector3.up);
        transform.localRotation = Extensions.SmoothDampQuat(transform.localRotation, targetrot, ref rotvel, turnsmoothing);
    }

    public void Jump(float mult)
    {
        currentgravity = -jumpforce * mult;
        jumped = true;

        if(coyotetimethread != null)
            StopCoroutine(coyotetimethread);

        coyotetimethread = null;
    }
    
    private IEnumerator CoyoteTimeRoutine()
    {
        yield return new WaitForSeconds(coyotetimeseconds);

        coyotetimethread = null;
        yield break;
    }

    private void GetCapsuleData(out Vector3 p1, out Vector3 p2, out float grounddist)
    {
        p1 = transform.position + character.center - Vector3.up * (character.height * 0.5f - character.radius);
        p2 = transform.position + character.center + Vector3.up * (character.height * 0.5f - character.radius);
        grounddist = Mathf.Max(mingroundcastdist, currentgravity);
    }

    void OnDrawGizmos()
    {
        GetCapsuleData(out Vector3 p1, out Vector3 p2, out float grounddist);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(p1, character.radius);
        Gizmos.DrawSphere(p2, character.radius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p1, p1 + Vector3.down * grounddist);
    }

    void OnGUI()
    {
        const float debuglineheight = 25f;
        Rect debugrect = new Rect(5, 5, 250f, debuglineheight);

        GUI.Label(debugrect, string.Format("Grounded -> {0}", grounded.ToString()));
        debugrect.y += debuglineheight;
        GUI.Label(debugrect, string.Format("State -> {0}", state.ToString()));
        debugrect.y += debuglineheight;
    }
}

public enum EPlayerState
{
    eFree = 0,

    eFrozen = 100,
    eDead = 200
}
