using System;
using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.MainMenu.UI
{

    public class AuthorUI : MonoBehaviour
    {
        [SerializeField] private Button _back;
        public event Action OnCloseAuthorUI;
        public void Init()
        {
            _back.onClick.AddListener(() =>
            {
                OnCloseAuthorUI?.Invoke();
            });
        }

        private void OnDestroy()
        {
            _back.onClick.RemoveAllListeners();
        }
    }

}