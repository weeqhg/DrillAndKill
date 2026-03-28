using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Cinemachine;
using System;
using WekenDev.InputSystem;

public class CharacterSelector : MonoBehaviour
{
    [System.Serializable]
    public class CharacterOption
    {
        public string characterName;      // "Воин", "Маг", "Лучник"
        public GameObject characterPrefab;  // Префаб модели
        public Sprite characterIcon;       // Иконка для кнопки
    }
    [Header("Characters")]
    [SerializeField] private List<CharacterOption> characters = new List<CharacterOption>();

    [Header("UI")]
    [SerializeField] private CanvasGroup characterPanel;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Button _startGame;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Transform modelSpawnPoint;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    [Header("Settings")]
    [SerializeField] private RuntimeAnimatorController animationController;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private int defaultCharacter = 0;

    private int _currentIndex = 0;
    private GameObject _currentCharacter;
    private List<Button> _characterButtons = new List<Button>();
    private float _currentRotation = 0f;
    private bool _canRotate = false;
    private AutoPopup[] autoPopups;
    public event Action OnPlayerReady;
    public event Action OnCloseSelector;
    private bool isOpenPanel = false;
    private bool IsSelectorOpen = true;
    private EntityStatsUI entityStatsUI;

    public void Initialize()
    {
        entityStatsUI = GetComponentInChildren<EntityStatsUI>();
        _currentIndex = PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCharacter, defaultCharacter);

        CreateCharacterButtons();

        _startGame.onClick.AddListener(OnStartClick);

        UpdateDisplay();

        HideMenu();

        autoPopups = GetComponentsInChildren<AutoPopup>();
        foreach (AutoPopup autoPopup in autoPopups)
        {
            autoPopup.Initialize();
            autoPopup.OnTogglePanel += (value) =>
            {
                isOpenPanel = value;
                entityStatsUI.UpdateUI();
            };
        }

        InputManager.Instance.Actions.UI.Cancel.performed += ctx => HideAllPopup();

    }
    public void ShowMenu()
    {
        if (IsSelectorOpen) return;
        IsSelectorOpen = true;
        cinemachineCamera.enabled = true;
        characterPanel.alpha = 1f;
        characterPanel.interactable = true;
        characterPanel.blocksRaycasts = true;
    }

    private void HideMenu()
    {
        if (!IsSelectorOpen) return;
        IsSelectorOpen = false;
        cinemachineCamera.enabled = false;
        characterPanel.alpha = 0f;
        characterPanel.interactable = false;
        characterPanel.blocksRaycasts = false;
    }

    private void HideAllPopup()
    {
        if (isOpenPanel)
        {
            foreach (AutoPopup autoPopup in autoPopups)
            {
                autoPopup.ClosePanel();
            }
        }
        else
        {
            HideMenu();
            OnCloseSelector?.Invoke();
        }
    }

    private void Update()
    {
        if (_canRotate)
        {
            _currentRotation += rotationSpeed * Time.deltaTime;
            _currentCharacter.transform.rotation = Quaternion.Euler(0, _currentRotation, 0);
        }
    }
    private void CreateCharacterButtons()
    {
        if (buttonsContainer == null || buttonPrefab == null) return;

        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);

        _characterButtons.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            int index = i;
            GameObject buttonObj = Instantiate(buttonPrefab, buttonsContainer);
            Button button = buttonObj.GetComponent<Button>();
            Image buttonImage = buttonObj.GetComponent<Image>();

            if (buttonImage != null && characters[i].characterIcon != null)
                buttonImage.sprite = characters[i].characterIcon;

            button.onClick.AddListener(() => OnCharacterButtonClick(index));
            _characterButtons.Add(button);
        }
    }

    private void OnCharacterButtonClick(int index)
    {
        if (_currentIndex == index)
        {
            SpawnModel();
        }
        else
        {
            _currentIndex = index;
            UpdateDisplay();
        }
    }

    private void OnStartClick()
    {
        HideMenu();
        PlayerPrefs.SetInt(PlayerPrefsKeys.SelectedCharacter, _currentIndex);
        PlayerPrefs.Save();
        _startGame.onClick.RemoveListener(OnStartClick);
        OnPlayerReady?.Invoke();
    }

    private void UpdateDisplay()
    {
        if (_currentCharacter != null)
            Destroy(_currentCharacter);

        if (characters != null && _currentIndex < characters.Count && characters[_currentIndex].characterPrefab != null)
        {
            Vector3 spawnPos = modelSpawnPoint != null ? modelSpawnPoint.position : Vector3.zero;
            _currentCharacter = Instantiate(characters[_currentIndex].characterPrefab, spawnPos, Quaternion.identity);

            PlayerManager playerManager = _currentCharacter.GetComponent<PlayerManager>();
            entityStatsUI.UpdateComponent(playerManager);

            if (modelSpawnPoint != null)
            {
                _currentCharacter.transform.position = modelSpawnPoint.position;
                _currentCharacter.transform.SetParent(modelSpawnPoint);
            }

            SpawnModel();

        }

        if (characterNameText != null && characters != null && _currentIndex < characters.Count)
            characterNameText.text = characters[_currentIndex].characterName;
    }

    private void SpawnModel()
    {
        _currentRotation = modelSpawnPoint.eulerAngles.y;

        Animator animator = _currentCharacter.GetComponent<Animator>();
        animator.runtimeAnimatorController = animationController;
        animator.enabled = true;

        if (animator != null)
        {
            animator.SetTrigger("Spawn");
        }
    }

    private void OnDestroy()
    {
        foreach (AutoPopup autoPopup in autoPopups)
        {
            autoPopup.Initialize();
            autoPopup.OnTogglePanel -= (value) => isOpenPanel = value;
        }
        _startGame.onClick.RemoveAllListeners();
        InputManager.Instance.Actions.UI.Cancel.performed -= ctx => HideAllPopup();
    }
}