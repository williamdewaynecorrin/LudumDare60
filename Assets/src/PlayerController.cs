using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private const string kMovementAnimState = "Movement";
    private const string kIdleAnimState = "Idle";
    private const string kInAirAnimState = "InAir";
    private const string kJumpStartAnimState = "JumpStart"; 
    private const string kDoubleJumpAnimState = "DoubleJump"; 
    private const string kJumpEndAnimState = "JumpEnd"; 
    private const string kBoominAnimState = "Boomin"; 
    private const string kSitAnimState = "Sitting"; 

    private const string kMovementSpeedAnimTag = "MovementSpeed";
    private const string kYVelocityAnimTag = "YVelocity";
    private const float kSpeedForIdleAnimationThresh = 0.002f;
    private const float kKillPlaneHeight = -20f;

    private const float kYVelocityMinValue = 16f;
    private const float kYVelocityMaxValue = -12f;

    [Header("Base Data")]
    [SerializeField]
    private SOGameConstants gameconstants;
    [SerializeField]
    private PlayerCamera playercamera;
    [SerializeField]
    private UIHUD uihud;
    [SerializeField]
    private PlayerPostProcess postprocess;
    [SerializeField]
    private Health health;
    [SerializeField]
    private CharacterController character;
    [SerializeField]
    private PlayerScore score;
    [SerializeField]
    private Animator anim;

    [Header("Movement + Turning")]
    [SerializeField]
    private EPlayerState state = EPlayerState.eFree;
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
    private float doublejumpmult = 0.66f;
    [SerializeField]
    private float coyotetimeseconds = 0.1f;

    [Header("Abilities")]
    [SerializeField]
    [Range(0f, 1f)]
    private float boomamount = 0.8f;
    [SerializeField]
    private ParticleSystem boomparticles;

    [Header("SFX")]
    [SerializeField]
    private AudioClipExt sfxjump;
    [SerializeField]
    private AudioClipExt sfxdoublejump;
    [SerializeField]
    private AudioClipExt sfxland;
    [SerializeField]
    private AudioClipExt sfxledgefall;
    [SerializeField]
    private AudioClipExt sfxstartboom;
    [SerializeField]
    private AudioClipExt sfxstopboom;

    private float currentspeed = 0f;
    private Vector3 externalvelocity = Vector3.zero;
    private float currentgravity = 0f;
    private bool grounded = true;
    private bool jumped = false;
    private bool doublejumped = false;
    private Coroutine coyotetimethread = null;
    private Vector3 movedir;
    private string animstate = "";
    private RadioObject currentradio = null;
    private MovingPlatform currentmovingplatform = null;
    private RotatingPlatform currentrotatingplatform = null;
    private bool hascachedmovingplatformvelocity = false;
    private Vector3 cachedmovingplatformvelocity;
    private Quaternion additionalrot = Quaternion.identity;

    public Health Health => health;
    public bool CanJump => !jumped && (grounded || coyotetimethread != null);
    public Vector3 CameraTarget => transform.position + Vector3.up * character.height * 0.5f;
    public float CurrentSpeed => currentspeed;
    public EPlayerState State => state;
    public bool IsGrounded => grounded;

    void Awake()
    {
        movedir = transform.forward;
        boomparticles.Stop();

        if(state == EPlayerState.eFrozen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Extensions.PlayAnimationTransitioned(ref anim, kSitAnimState, 0f, SAnimatorLayer.kBaseLayer, ref animstate);
            boomparticles.Play();
            boomparticles.transform.localScale *= 0.3f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
    }

    void Start()
    {
        if(state == EPlayerState.eFrozen)
        {
           AudioManager.Instance.SetBoomAmount(1f);
        }
    }

    void Update()
    {
        if (state == EPlayerState.eFrozen || state == EPlayerState.eDead)
            return;

        // -- vertical plane movement
        currentmovingplatform = null;
        GetCapsuleData(out Vector3 p1, out Vector3 p2, out float grounddist);
        if (currentgravity >= 0f && Physics.CapsuleCast(p1, p2, character.radius, Vector3.down, out RaycastHit groundhit, grounddist, groundmask, QueryTriggerInteraction.Ignore))
        {
            currentgravity = 0f;

            if(!grounded)
            {
                // -- just landed
                jumped = false;
                doublejumped = false;

                if (animstate != kJumpEndAnimState)
                {
                    Extensions.PlayAnimationTransitioned(ref anim, kJumpEndAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);
                }

                AudioManager.Play2D(sfxland);
            }

            // -- moving platform
            if (groundhit.collider.TryGetComponent<MovingPlatform>(out MovingPlatform mplat))
            {
                currentmovingplatform = mplat;
                cachedmovingplatformvelocity = currentmovingplatform.FixedVelocity;
                hascachedmovingplatformvelocity = true;
            }
            else
            {
                cachedmovingplatformvelocity = Vector3.zero;
                currentmovingplatform = null;
                hascachedmovingplatformvelocity = false;
            }

            // -- rotating platform
            if(groundhit.collider.TryGetComponent< RotatingPlatform >(out RotatingPlatform rplat))
            {
                currentrotatingplatform = rplat;
            }
            else
            {
                currentrotatingplatform = null;
            }

            character.Move(Vector3.down * groundhit.distance);
            grounded = true;
        }
        else
        {
            currentrotatingplatform = null;

            if (grounded)
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

        if (state == EPlayerState.eFree)
        {
            // -- jump check
            if (grounded && !jumped && Inputs.JumpTriggered())
            {
                Jump();
            }
            if (!grounded && !doublejumped && (jumped || coyotetimethread == null) && Inputs.JumpTriggered())
            {
                Jump(doublejumpmult, true);
            }
        }

        CollisionFlags gravityflags = character.Move(Vector3.down * currentgravity * Time.deltaTime);

        // -- xz plane movement
        Vector2 currentinput = new Vector2(Inputs.HorizontalMove(), Inputs.VerticalMove());
        if (currentinput == Vector2.zero || state == EPlayerState.eBoomin)
        {
            // -- stop
            float stopvel = 0f;
            currentspeed = Mathf.SmoothDamp(currentspeed, 0f, ref stopvel, stopsmoothing);

            if (state != EPlayerState.eBoomin && grounded && animstate != kIdleAnimState && currentspeed <= kSpeedForIdleAnimationThresh && animstate != kInAirAnimState && animstate != kJumpStartAnimState && animstate != kJumpEndAnimState)
            {
                Extensions.PlayAnimationTransitioned(ref anim, kIdleAnimState, 0.5f, SAnimatorLayer.kBaseLayer, ref animstate);
            }
        }
        else
        {
            Vector3 framemove = playercamera.PlayerMovementForward * currentinput.y + playercamera.PlayerMovementRight * currentinput.x;
            movedir = framemove.normalized;

            currentspeed += acceleration * Time.deltaTime;
            currentspeed = Mathf.Min(currentspeed, maxspeed);

            if (state != EPlayerState.eBoomin && grounded && animstate != kMovementAnimState && animstate != kInAirAnimState && animstate != kJumpStartAnimState && animstate != kJumpEndAnimState)
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

        CollisionFlags moveflags = character.Move(transform.forward.NoY().normalized * currentspeed * Time.deltaTime);
        Vector3 rotvel = Vector3.zero;
        Quaternion targetrot = Quaternion.LookRotation(movedir, Vector3.up);
        transform.localRotation = Extensions.SmoothDampQuat(transform.localRotation, targetrot, ref rotvel, turnsmoothing);
        if(transform.position.y < kKillPlaneHeight && state != EPlayerState.eFallingOffLevel)
        {
            state = EPlayerState.eFallingOffLevel;
            health.TakeDamage(gameconstants.playerkillplanedamage);
            hascachedmovingplatformvelocity = false;
            StartCoroutine(RespawnOffCliffRoutine());
        }

        if(grounded && state != EPlayerState.eFallingOffLevel)
        {
            if(state == EPlayerState.eFree && Inputs.ToggleBoom())
            {
                OnStartBoomin();
            }
            else if(state == EPlayerState.eBoomin && (Inputs.ToggleBoom() || Inputs.MovementTriggered()))
            {
                OnStopBoomin();
            }
        }
    }

    void FixedUpdate()
    {
        if(state == EPlayerState.eFree || state == EPlayerState.eBoomin)
        {
            if (currentmovingplatform != null)
            {
                CollisionFlags moveflags = character.Move(currentmovingplatform.FixedVelocity);
            }
            else if (hascachedmovingplatformvelocity)
            {
                CollisionFlags moveflags = character.Move(cachedmovingplatformvelocity);
            }

            if(currentrotatingplatform != null)
            {
                Vector3 localpos = transform.position - currentrotatingplatform.transform.position;
                Vector3 rotpos = currentrotatingplatform.DeltaRot * localpos;
                Vector3 rotmove = rotpos - localpos;

                CollisionFlags moveflags = character.Move(rotmove + currentrotatingplatform.DeltaPos);

                movedir = currentrotatingplatform.DeltaRot * movedir;
            }
        }
    }

    private void OnStartBoomin()
    {
        state = EPlayerState.eBoomin;
        uihud.SetOn();
        boomparticles.Play();
        AudioManager.Play2D(sfxstopboom);
        AudioManager.Instance.SetBoomAmount(boomamount);

        Extensions.PlayAnimationTransitioned(ref anim, kBoominAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);

        if (currentradio != null)
        {
            if(currentradio.NoteRequirement <= score.CurrentNotes)
            {
                currentradio.RestoreRadio();
            }
        }
    }

    private void OnStopBoomin()
    {
        state = EPlayerState.eFree;
        uihud.SetOff();
        boomparticles.Stop();
        AudioManager.Play2D(sfxstartboom);
        AudioManager.Instance.SetBoomAmount(0f);

        Extensions.PlayAnimationTransitioned(ref anim, kIdleAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);
    }

    public void SetCurrentRadio(RadioObject radio)
    {
        currentradio = radio;
    }

    public void Jump(float mult = 1.0f, bool isdoublejump = false, bool playsfx = true)
    {
        jumped = true;

        if(coyotetimethread != null)
            StopCoroutine(coyotetimethread);

        coyotetimethread = null;

        if(isdoublejump)
        {
            if (currentgravity >= 0f)
                currentgravity = -jumpforce * mult;
            else
                currentgravity = -jumpforce;

            if (animstate != kDoubleJumpAnimState)
            {
                Extensions.PlayAnimationTransitioned(ref anim, kDoubleJumpAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);
            }

            doublejumped = true;

            if(playsfx)
                AudioManager.Play2D(sfxdoublejump);
        }
        else
        {
            currentgravity = -jumpforce * mult;

            if (animstate != kJumpStartAnimState)
            {
                Extensions.PlayAnimationTransitioned(ref anim, kJumpStartAnimState, 0.1f, SAnimatorLayer.kBaseLayer, ref animstate);
            }

            if(playsfx)
                AudioManager.Play2D(sfxjump);
        }
    }

    public void JumpExternal(float mult)
    {
        Jump(mult, false, false);
        doublejumped = false;
    }

    private IEnumerator RespawnOffCliffRoutine()
    {
        AudioManager.Play2D(sfxledgefall);
        yield return new WaitForSeconds(gameconstants.playerfallrespawntime);

        currentgravity = 0f;
        state = health.IsDead ? EPlayerState.eDead : EPlayerState.eFree;
        if (PlayerCheckpoint.gLastTouched != null)
        {
            Teleport(PlayerCheckpoint.gLastTouched.transform.position, PlayerCheckpoint.gLastTouched.transform.forward);
        }
        else
        {
            Teleport(Vector3.zero, Vector3.forward);
        }
    }
    
    private IEnumerator CoyoteTimeRoutine()
    {
        yield return new WaitForSeconds(coyotetimeseconds);

        coyotetimethread = null;
        yield break;
    }

    public void Teleport(Vector3 pos, Vector3 forward)
    {
        character.enabled = false;
        transform.localPosition = pos;
        transform.localRotation = Quaternion.LookRotation(forward, Vector3.up);
        character.enabled = true;
    }

    public void OnDeath()
    {
        state = EPlayerState.eDead;
        uihud.SetDead();
        StartCoroutine(BackToMenu(2.0f));
    }

    public void SetWin()
    {
        state = EPlayerState.eFrozen;
        StartCoroutine(BackToMenu(3.0f));
        score.RecordScoreAsFinished();
    }

    private IEnumerator BackToMenu(float preloadwait)
    {
        yield return new WaitForSeconds(preloadwait);
        postprocess.SetVignetteOn();
        yield return new WaitForSeconds(gameconstants.deathmenuload);
        SceneManager.LoadScene(gameconstants.mainmenulevel);
    }

    private void GetCapsuleData(out Vector3 p1, out Vector3 p2, out float grounddist)
    {
        p1 = transform.position + character.center - Vector3.up * (character.height * 0.5f - character.radius);
        p2 = transform.position + character.center + Vector3.up * (character.height * 0.5f - character.radius);
        grounddist = Mathf.Max(mingroundcastdist, currentgravity * Time.deltaTime);
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
        if (!Application.isEditor)
            return;

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
    eBoomin = 10,
    eFallingOffLevel = 50,
    eFrozen = 100,
    eDead = 200
}
