using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    protected bool isUsed;
    protected OutLine outLine;



    public void Initialize()
    {
        outLine = GetComponent<OutLine>();
        outLine.SetActive(false);
        SetupDerived();
    }

    public abstract string GetHint();

    protected virtual void SetupDerived() { }

    public bool IsUsed()
    {
        return isUsed;
    }

    public virtual void Interact(PlayerInteractor playerInteractor) { }

    public void OnFocus()
    {
        if (isUsed) return;

        outLine.SetActive(true);
    }

    public void OnLoseFocus()
    {
        outLine.SetActive(false);
    }
}
