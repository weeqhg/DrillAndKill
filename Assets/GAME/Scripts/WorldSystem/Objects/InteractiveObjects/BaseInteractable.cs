using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    protected bool isUsed;
    protected OutLine outLine;
    protected DifficultySnapshot Difficulty { get; private set; }

    public void Initialize(DifficultySnapshot snapshot)
    {
        Difficulty = snapshot;
        outLine = GetComponent<OutLine>();
        outLine.SetActive(false);
        SetupDerived();
    }

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
