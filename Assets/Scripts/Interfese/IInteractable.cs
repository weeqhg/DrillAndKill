public interface IInteractable
{
    void Interact(PlayerInteractor interactor);
    void OnFocus();
    void OnLoseFocus();
    bool IsUsed();
}