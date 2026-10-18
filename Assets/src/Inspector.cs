using UnityEngine;

public class Inspector : MonoBehaviour
{
    private Inspectable currentinspectable = null;

    public void OnInspectableShow(Inspectable inspect)
    {
        if (currentinspectable != null)
            OnInspectableHide(currentinspectable);

        currentinspectable = inspect;
        currentinspectable.Show();
    }

    public void OnInspectableHide(Inspectable inspect)
    {
        inspect.Hide();

        currentinspectable = null;
    }
}
