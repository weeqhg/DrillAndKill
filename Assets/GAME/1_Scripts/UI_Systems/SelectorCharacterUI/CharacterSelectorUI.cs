using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;


public class CharacterSelectorUI : UIWindow
{
    [Header("Characters")]
    [SerializeField] private List<CharacterOption> characters = new List<CharacterOption>();

    [Header("Animation")]
    [SerializeField] private RuntimeAnimatorController animationController;
    private int defaultCharacter = 0;

    [Header("UI")]
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Button _statsButtonComponent;
    [SerializeField] private Button _treeButtonComponent;
    [SerializeField] private StatsControllerUI statsUI;

    [Header("SpawnPoint")]
    [SerializeField] private Transform spawnPoint;

    [System.Serializable]
    public class CharacterOption
    {
        public string characterName;
        public GameObject characterPrefab;  // Префаб модели
        public Sprite characterIcon;       // Иконка для кнопки
    }

    private List<Button> _characterButtons = new List<Button>();
    private GameObject _currentCharacter;
    private float _currentRotation = 0f;
    private float _rotationSpeed = 20f;
    private int _currentIndex = 0;
    private bool _canRotate = false;

    public int GetCurrentCharacterIndex() => _currentIndex;

    public void Initialize()
    {
        _currentIndex = PlayerPrefs.GetInt(PlayerPrefsKeys.SelectedCharacter, defaultCharacter);

        CreateCharacterButtons();

        UpdateModels();

        _statsButtonComponent.gameObject.SetActive(false);
        _treeButtonComponent.gameObject.SetActive(false);
    }
    public override void Show()
    {
        base.Show();
        _statsButtonComponent.gameObject.SetActive(true);
        _treeButtonComponent.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        base.Hide();
        _statsButtonComponent.gameObject.SetActive(false);
        _treeButtonComponent.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_canRotate)
        {
            _currentRotation += _rotationSpeed * Time.deltaTime;
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
            UpdateModels();
        }
    }

    private void SpawnModel()
    {
        Animator animator = _currentCharacter.GetComponent<Animator>();
        animator.runtimeAnimatorController = animationController;
        animator.enabled = true;

        if (animator != null)
        {
            animator.SetTrigger("Spawn");
        }
    }

    private void UpdateModels()
    {
        _treeButtonComponent.onClick.RemoveAllListeners();
        _statsButtonComponent.onClick.RemoveAllListeners();

        if (_currentCharacter != null)
            Destroy(_currentCharacter);

        if (characters != null && _currentIndex < characters.Count && characters[_currentIndex].characterPrefab != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            _currentCharacter = Instantiate(characters[_currentIndex].characterPrefab, spawnPos, Quaternion.identity);

            StatsController statsController = _currentCharacter.GetComponentInChildren<StatsController>();
            SkillTreeStats skillTreeStats = _currentCharacter.GetComponentInChildren<SkillTreeStats>();

            SkillTreeUI skillTreeUI = skillTreeStats.GetComponentInChildren<SkillTreeUI>();

            statsController.Initialize();
            skillTreeStats.Initialize();

            statsUI.Initialize(statsController);
            skillTreeUI.Initialize(skillTreeStats, 192);

            _treeButtonComponent.onClick.AddListener(skillTreeUI.TogglePanel);
            _statsButtonComponent.onClick.AddListener(statsUI.TogglePanel);

            if (spawnPoint != null)
            {
                _currentCharacter.transform.position = spawnPoint.position;
                _currentCharacter.transform.SetParent(spawnPoint);
            }

            SpawnModel();

        }

        if (characterNameText != null && characters != null && _currentIndex < characters.Count)
            characterNameText.text = characters[_currentIndex].characterName;
    }
}
