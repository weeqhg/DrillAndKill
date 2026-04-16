using UnityEngine;
using UnityEngine.UI;
using System;

public class CharacterSelector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button _startGame;
    public event Action OnPlayerReady;
    private CharacterSelectorUI _selectorUI;

    public void Initialize()
    {
        _selectorUI = GetComponentInChildren<CharacterSelectorUI>(true);
        _selectorUI.Initialize();

        _startGame.onClick.AddListener(OnStartClick);
    }


    private void OnStartClick()
    {
        PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedCharacter, _selectorUI.GetCurrentCharacterIndex());
        PlayerPrefs.Save();
        OnPlayerReady?.Invoke();
    }

    private void OnDestroy()
    {
        _startGame.onClick.RemoveListener(OnStartClick);
    }
}