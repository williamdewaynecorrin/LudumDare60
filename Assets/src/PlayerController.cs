using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const string kMovementAnimState = "Movement";
    private const string kIdleAnimState = "Idle";
    private const string kInAirAnimState = "InAir";
    private const string kJumpStartAnimState = "JumpStart";
    private const string kJumpEndAnimState = "JumpEnd";

    private const string kMovementSpeedAnimTag = "MovementSpeed";
    private const string kYVelocityAnimTag = "YVelocity";
    private const float kSpeedForIdleAnimationThresh = 0.002f;
    private const float kKillPlaneHeight = -20f;

    private const float kYVelocityMinValue = 0.06f;
    private const float kYVelocityMaxValue = -0.04f;

    [Header("Base Data")]
    [SerializeField]
    private SOGameConstants gameconstants;
    [SerializeField]
    private PlayerCamera playercamera;
    [SerializeField]
    private Health health;
    [SerializeField]
    private CharacterController character;
    [SerializeField]
    private Animator anim;

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

    private float currentspeed = 0f;
    private Vector3 externalvelocity = Vector3.zero;
    private float currentgravity = 0f;
    private EPlayerState state = EPlayerState.eFree;
    private bool grounded = true;
    private bool jumped = false;
    private Coroutine coyotetimethread = null;
    private Vector3 movedir;
    private string animstate = "";

    public bool CanJump => !jumped && (grounded || coyotetimethread != null);
    public Vector3 CameraTarget => transform.position + Vector3.up * character.height * 0.5f;
    public float CurrentSpeed => currentspeed;
    public EPlayerState State => state;
    public bool IsGrounded => grounded;

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
        if (currentgravity >= 0f && Physics.CapsuleCast(p1, p2, character.radius, Vector3.down, out RaycastHit groundhit, grounddist, groundmask, QueryTriggerInteraction.Ignore))
        {
            currentgravity = 0f;

            if(!grounded)
            {
                // -- just landed
                jumped = false;

                if (animstate != kJumpEndAnimState)
                {
                    Extensions.PlayAnimationTransitioned(ref anim, kJumpEndAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);
                }

                AudioManager.Play2D(sfxland);
            }

            character.Move(Vector3.down * groundhit.distance);
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

                // -- only enter in air state if we didn't explicitly jump
                if(!jumped && animstate != kJumpStartAnimState)
                {
                    Extensions.PlayAnimationTransitioned(ref anim, kInAirAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);
                }
            }

            currentgravity += gravityforce * Time.deltaTime;
            grounded = false;
        }

        // -- jump check
        if(grounded && !jumped && Inputs.JumpTriggered())
        {
            Jump();
        }

        CollisionFlags gravityflags = character.Move(Vector3.down * currentgravity);

        // -- xz plane movement
        Vector2 currentinput = new Vector2(Inputs.HorizontalMove(), Inputs.VerticalMove());
        if (currentinput == Vector2.zero)
        {
            // -- stop
            float stopvel = 0f;
            currentspeed = Mathf.SmoothDamp(currentspeed, 0f, ref stopvel, stopsmoothing);

            if(animstate != kIdleAnimState && currentspeed <= kSpeedForIdleAnimationThresh)
            {
                Extensions.PlayAnimationTransitioned(ref anim, kIdleAnimState, 0.5f, SAnimatorLayer.kBaseLayer, ref animstate);
            }
        }
        else
        {
            Vector3 framemove = playercamera.PlayerMovementForward * currentinput.y + playercamera.PlayerMovementRight * currentinput.x;
            movedir = framemove.normalized;

            framemove *= acceleration * Time.deltaTime;

            currentspeed += framemove.magnitude;
            currentspeed = Mathf.Min(currentspeed, maxspeed);

            if(animstate != kMovementAnimState && animstate != kInAirAnimState && animstate != kJumpStartAnimState && animstate != kJumpEndAnimState)
            {
                Extensions.PlayAnimationTransitioned(ref anim, kMovementAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);
            }
        }

        // -- speed anim value
        float speedratio = Mathf.Clamp01(currentspeed / maxspeed);
        anim.SetFloat(kMovementSpeedAnimTag, speedratio);

        // -- yvel anim value
        float yvelocityratio = Mathf.Clamp01(Mathf.InverseLerp(kYVelocityMinValue, kYVelocityMaxValue, currentgravity));
        anim.SetFloat(kYVelocityAnimTag, yvelocityratio);

        CollisionFlags moveflags = character.Move(transform.forward.NoY().normalized * currentspeed);

        Vector3 rotvel = Vector3.zero;
        Quaternion targetrot = Quaternion.LookRotation(movedir, Vector3.up);
        transform.localRotation = Extensions.SmoothDampQuat(transform.localRotation, targetrot, ref rotvel, turnsmoothing);

        if(transform.position.y < kKillPlaneHeight)
        {
            state = EPlayerState.eFallingOffLevel;
            health.TakeDamage(gameconstants.playerkillplanedamage);
            StartCoroutine(RespawnOffCliffRoutine());
        }
    }

    public void Jump(float mult = 1.0f)
    {
        currentgravity = -jumpforce * mult;
        jumped = true;

        if(coyotetimethread != null)
            StopCoroutine(coyotetimethread);

        if (animstate != kJumpStartAnimState)
        {
            Extensions.PlayAnimationTransitioned(ref anim, kJumpStartAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);
        }

        coyotetimethread = null;
        AudioManager.Play2D(sfxjump);
    }

    private IEnumerator RespawnOffCliffRoutine()
    {
        yield return new WaitForSeconds(gameconstants.playerfallrespawntime);
    }
    
    private IEnumerator CoyoteTimeRoutine()
    {
        yield return new WaitForSeconds(coyotetimeseconds);

        coyotetimethread = null;
        yield break;
    }

    public void OnDeath()
    {

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

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(p1, p1 + movedir * 2.0f);
    }

    void OnGUI()
    {
        const float debuglineheight = 25f;
        Rect debugrect = new Rect(5, 5, 250f, debuglineheight);

        GUI.Label(debugrect, string.Format("Grounded -> {0}", grounded.ToString()));
        debugrect.y += debuglineheight;
        GUI.Label(debugrect, string.Format("State -> {0}", state.ToString()));
        debugrect.y += debuglineheight;
        GUI.Label(debugrect, string.Format("Gravity -> {0}", currentgravity.ToString("F2")));
        debugrect.y += debuglineheight;
    }
}

public enum EPlayerState
{
    eFree = 0,

    eFallingOffLevel = 50,
    eFrozen = 100,
    eDead = 200
}
