using UnityEngine;

public static class Extensions
{
    public static Vector3 NoY(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.y);
    }

    public static Quaternion SmoothDampQuat(Quaternion cur, Quaternion target, ref Vector3 vel, float smootht)
    {
        Vector3 cura = cur.eulerAngles;
        Vector3 targeta = target.eulerAngles;
        return Quaternion.Euler(Mathf.SmoothDampAngle(cura.x, targeta.x, ref vel.x, smootht),
                                Mathf.SmoothDampAngle(cura.y, targeta.y, ref vel.y, smootht),
                                Mathf.SmoothDampAngle(cura.z, targeta.z, ref vel.z, smootht));
    }
}
