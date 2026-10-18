using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private int moveframes = 150;
    [SerializeField]
    private float syncoffset = 0f;
    [SerializeField]
    private float waittime = 2.0f;
    [SerializeField]
    private bool direction = false;
    [SerializeField]
    private Transform start;
    [SerializeField]
    private Transform end;

    private float internaltime = 0f;
    private Vector3 fixedvelocity;

    public Vector3 FixedVelocity => fixedvelocity;

    void Awake()
    {
        start.SetParent(transform.parent, true);
        end.SetParent(transform.parent, true);

        if (direction)
            transform.position = start.position;
        else
            transform.position = end.position;
    }

    void Start()
    {
        StartCoroutine(FullThread());
    }

    private IEnumerator FullThread()
    {
        fixedvelocity = Vector3.zero;
        yield return new WaitForSeconds(syncoffset);

        yield return MainThread();
    }

    private IEnumerator MainThread()
    {
        while (true)
        {
            if (direction)
                yield return FromTo(start, end);
            else
                yield return FromTo(end, start);

            fixedvelocity = Vector3.zero;
            direction = !direction;
            yield return new WaitForSeconds(waittime);
        }
    }

    private IEnumerator FromTo(Transform from, Transform to)
    {
        Vector3 diff = to.position - from.position;
        float dist = diff.magnitude;
        Vector3 dir = diff.normalized;

        Vector3 inc = dir * (dist / (float)moveframes);
        fixedvelocity = inc;
        for(int i = 0; i < moveframes; ++i)
        {
            transform.position += inc;
            yield return new WaitForFixedUpdate();
        }

        fixedvelocity = Vector3.zero;
        transform.position = to.position;
        yield break;
    }
}
