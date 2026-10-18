using UnityEngine;

public static class Extensions
{
    // -- math
    public const float kKindaSmallNumber = 0.0001f;

    public static Vector3 NoY(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

    public static Quaternion SmoothDampQuat(Quaternion cur, Quaternion target, ref Vector3 vel, float smootht)
    {
        Vector3 cura = cur.eulerAngles;
        Vector3 targeta = target.eulerAngles;
        return Quaternion.Euler(Mathf.SmoothDampAngle(cura.x, targeta.x, ref vel.x, smootht),
                                Mathf.SmoothDampAngle(cura.y, targeta.y, ref vel.y, smootht),
                                Mathf.SmoothDampAngle(cura.z, targeta.z, ref vel.z, smootht));
    }

    // -- animation
    public static void PlayAnimationTransitioned(ref Animator anim, string animationname, float transitiontime, string layername, int layer, ref string animstatestr)
    {
        animstatestr = animationname;
        string fullstatename = GetFullStateName(animationname, layername);
        anim.CrossFadeInFixedTime(fullstatename, transitiontime, layer);
    }

    public static void PlayAnimationTransitioned(ref Animator anim, string animationname, float transitiontime, SAnimatorLayer layer, ref string animstatestr)
    {
        PlayAnimationTransitioned(ref anim, animationname, transitiontime, layer.Name, layer.Layer, ref animstatestr);
    }

    public static void PlayAnimationDirect(ref Animator anim, string animationname, SAnimatorLayer layer)
    {
        string fullstatename = GetFullStateName(animationname, layer.Name);
        anim.Play(fullstatename);
    }

    public static string GetFullStateName(string statename, string layername)
    {
        return string.Format("{0}.{1}", layername, statename);
    }

    // -- text
    public const string kGlyphIDRedX = "<sprite name=\"tmp_sprite_x\">"; 
    public const string kGlyphIDGreenCheck = "<sprite name=\"tmp_sprite_check\">";
    public const string kGlyphIDNote = "<sprite name=\"tmp_sprite_note\">"; 

    public static string ColoredString(string str, Color c)
    {
        string hex = UnityEngine.ColorUtility.ToHtmlStringRGBA(c);
        string rich = string.Format("<color=#{0}>{1}</color>", hex, str);

        return rich;
    }
}

[System.Serializable]
public class SAnimatorLayer
{
    [SerializeField]
    private string layername;
    private int layervalue;

    public string Name => layername;
    public int Layer => layervalue;

    public void Init(Animator anim)
    {
        layervalue = anim.GetLayerIndex(layername);
    }

    public static readonly SAnimatorLayer kBaseLayer = new SAnimatorLayer()
    {
        layername = "Base Layer",
        layervalue = 0
    };
}