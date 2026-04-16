using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Console : UIWindow
{
    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI outputText;

    private int maxMessages = 10;

    private List<string> _messages = new List<string>();
    private Dictionary<string, Action<string[]>> _commands = new Dictionary<string, System.Action<string[]>>();

    private List<string> _commandList = new List<string>();
    private List<string> _currentMatches = new List<string>();
    private int _selectedIndex = -1;
    private string _currentInput = "";
    private string _originalInput = "";
    private int _helpPageSize = 10;
    private List<string> _helpMessages = new List<string>();

    public void Initialize()
    {
        Hide();

        RegisterCommands();
        _commandList = _commands.Keys.ToList();
        _commandList.Sort();

        inputField.onSubmit.AddListener(ExecuteCommand);
        inputField.onValueChanged.AddListener(OnInputChanged);

        var input = G.InputManager;
        input.Actions.UI.Console.performed += OnConsole;
        input.Actions.UI.Up.performed += OnUpArrowPressed;
        input.Actions.UI.Down.performed += OnDownArrowPressed;
        input.Actions.UI.Tab.performed += OnTabPressed;

        ConsoleEvents.OnConsoleMessage += AddMessage;
    }

    #region UIWindow

    public override void Show()
    {
        base.Show();

        _currentInput = inputField.text;
        _currentMatches.Clear();
        _selectedIndex = -1;

        inputField.ActivateInputField();
        inputField.caretPosition = inputField.text.Length; // Курсор в конец
    }

    public override void Hide()
    {
        base.Hide();
        _currentMatches.Clear();
        _selectedIndex = -1;
    }

    #endregion

    #region Input
    private void OnConsole(InputAction.CallbackContext context)
    {
        if (G.UIManager.IsOpen<GameMenuUI>())
        {
            return;
        }
        if (G.UIManager.IsOpen<Console>())
        {
            G.UIManager.Close(this);
        }
        else
        {
            G.UIManager.OpenOverlay(this);
        }
    }

    private void OnTabPressed(InputAction.CallbackContext context)
    {
        if (!gameObject.activeSelf) return;

        ShowSuggestions();
    }

    private void OnUpArrowPressed(InputAction.CallbackContext context)
    {
        if (!gameObject.activeSelf) return;
        if (_currentMatches.Count == 0) return;

        _selectedIndex = (_selectedIndex - 1 + _currentMatches.Count) % _currentMatches.Count;
        UpdateSuggestion();
    }

    private void OnDownArrowPressed(InputAction.CallbackContext context)
    {
        if (!gameObject.activeSelf) return;
        if (_currentMatches.Count == 0) return;

        _selectedIndex = (_selectedIndex + 1) % _currentMatches.Count;
        UpdateSuggestion();
    }

    private void OnInputChanged(string value)
    {
        _currentInput = value;
        _selectedIndex = -1;
    }
    #endregion

    #region Logic

    private void ShowSuggestions()
    {
        if (string.IsNullOrEmpty(_currentInput))
        {
            _currentMatches = _commandList;
        }
        else
        {
            _currentMatches = _commandList
                .Where(cmd => cmd.StartsWith(_currentInput, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (_currentMatches.Count == 0) return;

        _selectedIndex = 0;
        _originalInput = _currentInput;
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

        string remaining = suggestion.Substring(_originalInput.Length);
        inputField.text = _originalInput + remaining;

        inputField.caretPosition = inputField.text.Length;
        inputField.selectionAnchorPosition = inputField.caretPosition;
        inputField.selectionFocusPosition = inputField.caretPosition;

        _currentInput = inputField.text;
    }

    private void ExecuteCommand(string input)
    {
        AddMessage($"> {input}");

        string[] parts = input.Split(' ');
        string command = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (_commands.TryGetValue(command, out var cmd))
        {
            cmd.Invoke(args);
        }
        else
        {
            AddMessage($"Unknown command: {command}");
        }

        inputField.text = "";
        inputField.ActivateInputField();

        _currentMatches.Clear();
        _selectedIndex = -1;
        _currentInput = "";
    }

    #endregion

    #region Commands
    private void RegisterCommands()
    {
        _commands.Add("help", Help);
        _commands.Add("spawn_player", CommandPlayerSpawn);
        _commands.Add("spawn_enemy", CommandEnemySpawn);
        _commands.Add("spawn_boer", CommandLaunchBoer);
        _commands.Add("spawn_object", CommandObjectSpawn);
        _commands.Add("kill_player", CommandKillPlayer);
        _commands.Add("kill_all_enemy", CommandKillAllEnemy);
        _commands.Add("fly", CommandPlayerFly);
        _commands.Add("frezee", CommandFreezeGame);
        _commands.Add("difficulty", CommandToggleDifficultyScaler);
        _commands.Add("exp", CommandExp);
        _commands.Add("coin", CommandCoin);
        _commands.Add("reset_tree", CommandResetSkillTree);
        _commands.Add("talent_points", CommandTalentPoints);
        _commands.Add("open_skill", CommandToggleSkillTree);
        _commands.Add("open_level", CommandToggleLevelTree);
        _commands.Add("immortal", CommandImmortalPlayer);
        _commands.Add("clear", Clear);
    }
    private void Help(string[] args)
    {
        // Инициализируем список сообщений помощи при первом вызове
        if (_helpMessages.Count == 0)
        {
            _helpMessages = new List<string>
        {
            "help <page>- Show this help",
            "spawn_player <id> - Spawn player",
            "spawn_enemy <id> <count> - Spawn enemies",
            "spawn_boer - Boer launch on world",
            "spawn_object - Spawn object",
            "kill_player - kill player",
            "kill_all_enemy - Kill enemies",
            "fly <bool> - Player fly toggle",
            "frezee <bool> - Frezee world toggle",
            "difficulty <bool> - Toggle difficulty scaler",
            "exp <amount> - Give exp",
            "coin <amount> - Give coin",
            "reset_tree - Reset skill tree progress",
            "talent_points <amount> - Add talent points",
            "open_skill - Open skill tree UI",
            "open_level - Open level tree UI",
            "immortal <bool> - Player immortal toggle",
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

    private void CommandPlayerSpawn(string[] args)
    {
        int id = 0;

        if (args.Length > 0 && int.TryParse(args[0], out int parsedId))
        {
            id = parsedId;
        }
        else
        {
            AddMessage("Usage: spawn_player <id>");
            return;
        }

        ConsoleEvents.CommandPlayerSpawn(id);
    }

    private void CommandEnemySpawn(string[] args)
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

        ConsoleEvents.CommandEnemySpawn(id, count);
    }

    private void CommandLaunchBoer(string[] args)
    {
        ConsoleEvents.CommandLaunchBoer();
    }

    private void CommandObjectSpawn(string[] args)
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
            AddMessage("Usage: spawn_object <count> or spawn_object <id> <count>");
            return;
        }

        ConsoleEvents.CommandObjectSpawn(id, count);
    }


    private void CommandKillPlayer(string[] args)
    {
        ConsoleEvents.CommandKillPlayer();
        AddMessage("Player kill!");
    }
    private void CommandKillAllEnemy(string[] args)
    {
        ConsoleEvents.CommandKillAllEnemy();
        AddMessage("All enemies destroyed!");
    }

    private void CommandPlayerFly(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: fly <on/off>");
            return;
        }

        bool isFlying = args[0].ToLower() == "on" || args[0] == "1" || args[0].ToLower() == "true";

        ConsoleEvents.CommandPlayerFly(isFlying);
        AddMessage($"Flight mode: {(isFlying ? "ON" : "OFF")}");
    }

    private void CommandFreezeGame(string[] args)
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

    private void CommandToggleDifficultyScaler(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: difficulty <on/off>");
            return;
        }
        bool isTimerRunning = args[0].ToLower() == "on" || args[0] == "1" || args[0].ToLower() == "true";

        ConsoleEvents.CommandToggleDifficultyScaler(isTimerRunning);
        AddMessage($"Difficulty mode: {(isTimerRunning ? "ON" : "OFF")}");
    }

    private void CommandExp(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: addexp <amount>");
            return;
        }

        if (int.TryParse(args[0], out int value))
        {
            ConsoleEvents.CommandExp(value);
            AddMessage($"Added {value} experience!");
        }
    }

    private void CommandCoin(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: coin <amount>");
            return;
        }

        if (int.TryParse(args[0], out int value))
        {
            ConsoleEvents.CommandCoin(value);
            AddMessage($"Added {value} coins!");
        }
    }

    private void CommandResetSkillTree(string[] args)
    {
        ConsoleEvents.CommandResetSkillTree();
        AddMessage("Skill tree progress reset!");
    }

    private void CommandTalentPoints(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: talent_points <amount>");
            return;
        }

        if (int.TryParse(args[0], out int value))
        {
            ConsoleEvents.CommandTalentPoints(value);
            AddMessage($"Added {value} talent points!");
        }
    }

    private void CommandToggleSkillTree(string[] args)
    {
        ConsoleEvents.CommandToggleSkillTree();
        AddMessage("Opened skill tree UI!");
    }

    private void CommandToggleLevelTree(string[] args)
    {
        ConsoleEvents.CommandToggleLevelTree();
        AddMessage("Opened level tree UI!");
    }

    private void CommandImmortalPlayer(string[] args)
    {
        if (args.Length == 0)
        {
            AddMessage("Usage: immortal <on/off>");
            return;
        }
        bool isImmortal = args[0].ToLower() == "on" || args[0] == "1" || args[0].ToLower() == "true";

        ConsoleEvents.CommandImmortalPlayer(isImmortal);
        AddMessage($"Immortal mode: {(isImmortal ? "ON" : "OFF")}");
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
        inputField.onSubmit.RemoveListener(ExecuteCommand);
        inputField.onValueChanged.RemoveListener(OnInputChanged);

        ConsoleEvents.OnConsoleMessage -= AddMessage;

        if (G.InputManager != null)
        {
            var input = G.InputManager;
            input.Actions.UI.Console.performed -= OnConsole;
            input.Actions.UI.Up.performed -= OnUpArrowPressed;
            input.Actions.UI.Down.performed -= OnDownArrowPressed;
            input.Actions.UI.Tab.performed -= OnTabPressed;
        }
    }
}