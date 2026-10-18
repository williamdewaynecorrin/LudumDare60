using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerPostProcess : MonoBehaviour
{
    [SerializeField]
    private PostProcessProfile postprof;
    [SerializeField]
    [Range(0f, 1f)]
    private float vigenttelerp = 0.15f;

    private Vignette vignette;
    private bool ison = false;

    void Awake()
    {
        postprof.TryGetSettings<Vignette>(out vignette);
        vignette.active = false;
        vignette.opacity.value = 0.0f;
    }

    void FixedUpdate()
    {
        if(vignette.active)
        {
            vignette.opacity.value = Mathf.Lerp(vignette.opacity.value, 1.0f, vigenttelerp);
        }
        else
        {
            vignette.opacity.value = Mathf.Lerp(vignette.opacity.value, 0.0f, vigenttelerp);
        }
    }

    public void SetVignetteOn()
    {
        vignette.active = true;
    }

    public void SetVignetteOff()
    {
        vignette.active = false;
    }
}
