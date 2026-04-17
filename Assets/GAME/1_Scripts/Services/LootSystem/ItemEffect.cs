using UnityEngine;

public abstract class ItemEffect : MonoBehaviour
{
    public abstract void OnApply(GameObject owner);
    public abstract void OnRemove(GameObject owner);
    public virtual void OnStackChanged(int stack) { }
}
