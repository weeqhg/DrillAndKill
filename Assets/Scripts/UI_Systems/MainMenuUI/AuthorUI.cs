using UnityEngine;
using UnityEngine.UI;



public class AuthorUI : UIWindow
{
    [SerializeField] private Button _back;
    public void Init()
    {
        gameObject.SetActive(false);

        _back.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseTop();
        });
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

    private void OnDestroy()
    {
        _back.onClick.RemoveAllListeners();
    }
}

