using UnityEngine;

public abstract class Controllable : MonoBehaviour
{
    protected bool activated;

    public abstract void Switch();
}
