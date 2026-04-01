public interface IUIWindow
{
    bool CanBeClosed { get; }
    void Show();
    void Hide();
}