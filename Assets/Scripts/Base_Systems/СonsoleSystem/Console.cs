using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using WekenDev.InputSystem;

public class Console : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject consolePanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI outputText;

    private int maxMessages = 10;

    private InputManager _inputManager;

    private List<string> _messages = new List<string>();
    private Dictionary<string, Action<string[]>> _commands = new Dictionary<string, System.Action<string[]>>();


    private List<string> _commandList = new List<string>();
    private List<string> _currentMatches = new List<string>();
    private int _selectedIndex = -1;
    private string _currentInput = "";
    private bool isGameMenu = false;

    public void Initialize()
    {
        consolePanel.SetActive(false);
        RegisterCommands();
        // Сохраняем список команд для автодополнения
        _commandList = _commands.Keys.ToList();
        _commandList.Sort();

        if (inputField != null)
        {
            inputField.onSubmit.AddListener(ExecuteCommand);
            inputField.onValueChanged.AddListener(OnInputChanged);
            inputField.onEndEdit.AddListener(OnEndEdit);
        }

        _inputManager = InputManager.Instance;

        if (consolePanel.activeSelf) _inputManager.ChangeInputType(InputType.Player);

        _inputManager.Actions.Player.ConsoleOpen.performed += OnOpenConsole;
        _inputManager.Actions.UI.ConsoleClose.performed += OnCloseConsole;
        _inputManager.Actions.UI.Cancel.performed += OnCloseConsole;

        _inputManager.Actions.UI.Up.performed += OnUpArrowPressed;
        _inputManager.Actions.UI.Down.performed += OnDownArrowPressed;

        GameEvents.OnGameMenu += value => isGameMenu = value;

        // Подписка на Tab для автодополнения
        _inputManager.Actions.UI.Tab.performed += OnTabPressed;

        GameEvents.OnConsoleMessage += AddMessage;

        if (consolePanel.activeSelf)
        {
            ToggleConsole(true);
        }
        else
        {
            ToggleConsole(false);
        }
    }

    private void OnTabPressed(InputAction.CallbackContext context)
    {
        if (!consolePanel.activeSelf) return;

        ShowSuggestions();
    }

    private void OnUpArrowPressed(InputAction.CallbackContext context)
    {
        if (!consolePanel.activeSelf) return;
        if (_currentMatches.Count == 0) return;

        _selectedIndex = (_selectedIndex - 1 + _currentMatches.Count) % _currentMatches.Count;
        UpdateSuggestion();
    }

    private void OnDownArrowPressed(InputAction.CallbackContext context)
    {
        if (!consolePanel.activeSelf) return;
        if (_currentMatches.Count == 0) return;

        _selectedIndex = (_selectedIndex + 1) % _currentMatches.Count;
        UpdateSuggestion();
    }

    private void OnInputChanged(string value)
    {
        _currentInput = value;
        _selectedIndex = -1;
    }

    private void OnEndEdit(string value)
    {

    }

    private void OnOpenConsole(InputAction.CallbackContext context)
    {
        ToggleConsole(true);
    }

    private void OnCloseConsole(InputAction.CallbackContext context)
    {
        ToggleConsole(false);
    }

    private void ShowSuggestions()
    {
        if (string.IsNullOrEmpty(_currentInput))
        {
            _currentMatches = _commandList;
        }
        else
        {
            _currentMatches = _commandList
                .Where(cmd => cmd.StartsWith(_currentInput, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (_currentMatches.Count == 0) return;

        _selectedIndex = 0;
        UpdateSuggestion();

        if (_currentMatches.Count > 1)
        {
            AddMessage($"Found {_currentMatches.Count} matches. Use ↑/↓ to navigate.");
        }
    }

    private void UpdateSuggestion()
    {
        if (_currentMatches.Count == 0 || _selectedIndex < 0) return;

        string suggestion = _currentMatches[_selectedIndex];
        inputField.text = suggestion + " ";
        inputField.caretPosition = inputField.text.Length;
        _currentInput = inputField.text;
    }

    private void ToggleConsole(bool enable)
    {
        if (isGameMenu) return;

        if (enable) _inputManager.ChangeInputType(InputType.UI);
        else _inputManager.ChangeInputType(InputType.Player);

        GameEvents.Console(enable);
        consolePanel.SetActive(enable);

        if (consolePanel.activeSelf)
        {
            _currentInput = "";
            inputField.ActivateInputField();
            inputField.text = "";
        }
    }

    private void RegisterCommands()
    {
        _commands.Add("help", Help);
        _commands.Add("fly", SetFly);
        _commands.Add("frezee", SetFrezee);
        _commands.Add("spawn_enemy", SpawnEnemyCommand);
        _commands.Add("kill_all_enemy", KillEnemyCommand);
        _commands.Add("spawn_player", SpawnPlayerCommand);
        _commands.Add("master_volume", SetMasterVolume);
        _commands.Add("music_volume", SetMusicVolume);
        _commands.Add("sfx_volume", SetSFXVolume);
        _commands.Add("sensitivity", SetSensitivity);
        _commands.Add("volumes", ShowVolumes);
        _commands.Add("scene", LoadSceneCommand);
        _commands.Add("clear", Clear);
    }

    private void ExecuteCommand(string input)
    {
        AddMessage($"> {input}");

        string[] parts = input.Split(' ');
        string command = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts[1..] : new string[0];

        if (_commands.ContainsKey(command))
        {
            _commands[command].Invoke(args);
        }
        else
        {
            AddMessage($"Unknown command: {command}. Type 'help' for available commands.");
        }

        // Сбрасываем состояние автодополнения
        _currentInput = "";
        _currentMatches.Clear();
        _selectedIndex = -1;

        // Очищаем поле и возвращаем фокус
        inputField.text = "";
        inputField.ActivateInputField();
    }

    #region Commands
    private int _helpPageSize = 10;
    private List<string> _helpMessages = new List<string>();
    private void Help(string[] args)
    {
        // Инициализируем список сообщений помощи при первом вызове
        if (_helpMessages.Count == 0)
        {
            _helpMessages = new List<string>
        {
            "help <page>- Show this help",
            "fly <bool> - Player fly toggle",
            "frezee <bool> - Frezee world toggle",
            "spawn_enemy <id> <count> - Spawn enemies",
            "kill_all_enemy - Kill enemies",
            "spawn_player <id> - Spawn player",
            "master_volume <0-100> - Set master volume",
            "music_volume <0-100> - Set music volume",
            "sfx_volume <0-100> - Set SFX volume",
            "sensitivity <0-100> - Set sensitivity",
            "volumes - Show current volumes",
            "scene <name> - Load scene",
            "clear - Clear console"
        };
        }

        int page = 1;
        if (args.Length > 0 && int.TryParse(args[0], out int requestedPage))
        {
            if (requestedPage <= 1)
            {
                page = 1;
            }
            else
            {
                page = requestedPage;
            }
        }

        int totalPages = Mathf.CeilToInt((float)_helpMessages.Count / _helpPageSize);
        page = Mathf.Clamp(page, 1, totalPages);

        int startIndex = (page - 1) * _helpPageSize;
        int endIndex = Mathf.Min(startIndex + _helpPageSize, _helpMessages.Count);

        AddMessage($"=== Available Commands (Page {page}/{totalPages}) ===");
        for (int i = startIndex; i < endIndex; i++)
        {
            AddMessage($"  {_helpMessages[i]}");
        }

        if (page < totalPages)
        {
            AddMessage($"Type 'help {page + 1}' for next page");
        }
    }

    private void SetFly(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: fly <on/off>");
            return;
        }

        bool isFlying = args[0].ToLower() == "on" || args[0] == "1" || args[0].ToLower() == "true";

        GameEvents.CommandPlayerFly(isFlying);
        AddMessage($"Flight mode: {(isFlying ? "ON" : "OFF")}");
    }

    private void SetFrezee(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: frezee <on/off>");
            return;
        }

        bool isFrezee = args[0].ToLower() == "on" || args[0] == "1" || args[0].ToLower() == "true";

        GamePause.SetFrozen(isFrezee);
        AddMessage($"Frezee mode: {(isFrezee ? "ON" : "OFF")}");
    }

    private void SpawnEnemyCommand(string[] args)
    {
        int id = 0;
        int count = 0;

        if (args.Length == 1 && int.TryParse(args[0], out count))
        {
            count = Mathf.Clamp(count, 1, 100);
        }
        else if (args.Length >= 2 && int.TryParse(args[0], out id) && int.TryParse(args[1], out count))
        {
            id = Mathf.Clamp(id, 0, 10);
            count = Mathf.Clamp(count, 1, 100);
        }
        else
        {
            AddMessage("Usage: spawn_enemy <count> or spawn_enemy <id> <count>");
            return;
        }

        GameEvents.CommandEnemySpawn(id, count);
    }

    private void KillEnemyCommand(string[] args)
    {
        GameEvents.CommandKillAllEnemy();
        AddMessage("All enemies destroyed!");
    }

    private void SpawnPlayerCommand(string[] args)
    {
        int id = 0;

        if (args.Length > 0 && int.TryParse(args[0], out int parsedId))
        {
            id = parsedId;
        }

        GameEvents.CommandPlayerSpawn(id);
    }

    private void SetMasterVolume(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: master_volume <0-100>");
            return;
        }

        if (float.TryParse(args[0], out float volume))
        {
            volume = Mathf.Clamp(volume, 0, 100);
            AudioManager.Instance?.SetMasterVolume(volume);
            AddMessage($"Master volume set to {volume}%");
        }
        else
        {
            AddMessage("Invalid number");
        }
    }

    private void SetMusicVolume(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: music_volume <0-100>");
            return;
        }

        if (float.TryParse(args[0], out float volume))
        {
            volume = Mathf.Clamp(volume, 0, 100);
            AudioManager.Instance?.SetMusicVolume(volume);
            AddMessage($"Music volume set to {volume}%");
        }
        else
        {
            AddMessage("Invalid number");
        }
    }

    private void SetSFXVolume(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: sfx_volume <0-100>");
            return;
        }

        if (float.TryParse(args[0], out float volume))
        {
            volume = Mathf.Clamp(volume, 0, 100);
            AudioManager.Instance?.SetSFXVolume(volume);
            AddMessage($"SFX volume set to {volume}%");
        }
        else
        {
            AddMessage("Invalid number");
        }
    }

    private void ShowVolumes(string[] args)
    {
        if (AudioManager.Instance == null)
        {
            AddMessage("AudioManager not found!");
            return;
        }

        AddMessage("=== Current Volumes ===");
        AddMessage($"Master: {AudioManager.Instance.GetMasterVolume():F0}%");
        AddMessage($"Music: {AudioManager.Instance.GetMusicVolume():F0}%");
        AddMessage($"SFX: {AudioManager.Instance.GetSFXVolume():F0}%");
    }

    private void SetSensitivity(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: sensitivity <0-100>");
            return;
        }

        if (float.TryParse(args[0], out float value))
        {
            GameEvents.SensitivityChanged(value);
            AddMessage($"Sensitivity value set to {value}");
        }
        else
        {
            AddMessage("Invalid number");
        }
    }
    private void LoadSceneCommand(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: scene <name>");
            return;
        }

        string sceneName = args[0];
        GameEvents.GameStart(sceneName);
    }

    private void Clear(string[] args)
    {
        _messages.Clear();
        UpdateOutputText();
    }

    #endregion

    private void AddMessage(string message)
    {
        _messages.Add(message);

        while (_messages.Count > maxMessages)
            _messages.RemoveAt(0);

        UpdateOutputText();
    }

    private void UpdateOutputText()
    {
        if (outputText != null)
        {
            outputText.text = string.Join("\n", _messages);
        }
    }


    private void OnDestroy()
    {
        // Отписываемся от всех событий
        GameEvents.OnGameMenu -= value => isGameMenu = value;
        GameEvents.OnConsoleMessage -= AddMessage;

        if (_inputManager != null)
        {
            _inputManager.Actions.Player.ConsoleOpen.performed -= OnOpenConsole;
            _inputManager.Actions.UI.ConsoleClose.performed -= OnCloseConsole;
            _inputManager.Actions.UI.Cancel.performed -= OnCloseConsole;
            _inputManager.Actions.UI.Up.performed -= OnUpArrowPressed;
            _inputManager.Actions.UI.Down.performed -= OnDownArrowPressed;
            _inputManager.Actions.UI.Tab.performed -= OnTabPressed;
        }
    }
}