using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public enum NodeVisualState
{
    Locked,
    Available,
    Unlocked
}

public class SkillTreeUI : UIWindow
{
    [Header("References")]
    [SerializeField] private Image iconTree;
    [SerializeField] private GameObject nodeButtonFlatPrefab;
    [SerializeField] private GameObject nodeButtonIncreasedPrefab;
    [SerializeField] private GameObject nodeButtonMorePrefab;
    [SerializeField] private GameObject nodeButtonKeystonePrefab;
    [SerializeField] private Transform nodesContainer;
    [SerializeField] private Transform lineContainer;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private Button resetButton;
    [SerializeField] private RectTransform rectTransforms; // 0 - окно, 1 - иконка

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Localize")]
    [SerializeField] private LocalizedString flatText;
    [SerializeField] private LocalizedString increasedText;
    [SerializeField] private LocalizedString moreText;
    [SerializeField] private LocalizedString coastText;
    [SerializeField] private LocalizedString talantedText;

    [Header("Colors")]
    [SerializeField] private Color lockedColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color availableColor = new Color(0.8f, 0.8f, 0.3f);
    [SerializeField] private Color unlockedColor = new Color(0.2f, 0.8f, 0.2f);

    [Header("Navigation")]
    [SerializeField] private RectTransform content; // общий родитель для nodes + lines

    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2f;

    [SerializeField] private float dragSpeed = 1f;

    private Vector2 lastMousePosition;
    private bool isDragging;

    private SkillTreeStats skillTree;

    private Dictionary<string, Button> nodeButtons = new();
    private Dictionary<string, Image> nodeImages = new();

    private Vector2 tooltipOffset = new Vector2(100f, -150f);
    private Dictionary<string, Vector2> layout;
    private HashSet<string> drawnConnections = new();

    private AutoPopup treePopup;
    private RectTransform rect;
    private bool isOpen = false;

    public void Initialize(SkillTreeStats skillTreeStats, int size = 0)
    {
        skillTree = skillTreeStats;
        skillTree.OnNodeUnlocked += OnNodeUnlocked;
        if (skillTree.iconCharacter != null) iconTree.sprite = skillTree.iconCharacter;

        rect = GetComponent<RectTransform>();
        rect.offsetMin = new Vector2(0, rect.offsetMin.y);
        rect.offsetMax = new Vector2(-size, rect.offsetMax.y);

        treePopup = GetComponent<AutoPopup>();
        treePopup.Initialize();

        TalentPointsCounter talentPoints = skillTree.GetComponentInChildren<TalentPointsCounter>();
        talentPoints.OnPointsChanged += UpdatePointsUI;

        BuildParents(skillTree.allNodes);
        CreateTreeUI();
        RefreshTreeVisuals();
        UpdatePointsUI(talentPoints.Points);

        resetButton.onClick.AddListener(ResetTree);
        skillTree.OnResetTree += RefreshTreeVisuals;
        tooltip.SetActive(false);

        gameObject.SetActive(false);
        GameEvents.OnTriggerSkillTree += TogglePanel;
    }

    public void TogglePanel()
    {
        if (gameObject.activeSelf)
        {
            UIManager.Instance.Close(this);
        }
        else
        {
            UIManager.Instance.OpenOverlay(this);
        }
    }

    private void ResetTree() //Тут мы будем сбрасывать за деньги
    {
        skillTree.ResetTreeProgress();
        RefreshTreeVisuals();
    }

    //Нужно отписываться обнавляется в кнопке для тестов
    public void UpdateTree()
    {
        ClearTreeUI();
        BuildParents(skillTree.allNodes);
        CreateTreeUI();
        RefreshTreeVisuals();
    }

    public override void Show()
    {
        base.Show();
        isOpen = true;
        isDragging = false;
        treePopup.OpenPanel();
    }

    public override void Hide()
    {
        base.Hide();
        isOpen = false;
        isDragging = false;
        treePopup.ClosePanel();
    }

    #region  Move on Deck
    private void Update()
    {
        if (isOpen && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransforms, mousePos))
            {
                UIManager.Instance.CloseTop();
            }
        }

        HandleZoom();
        //HandleDrag();
        UpdateTooltipPosition();
    }

    private void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            mousePos,
            null,
            out Vector2 localPointBefore
        );

        float scale = content.localScale.x;
        scale += scroll * zoomSpeed * 0.01f;
        scale = Mathf.Clamp(scale, minZoom, maxZoom);

        content.localScale = Vector3.one * scale;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            mousePos,
            null,
            out Vector2 localPointAfter
        );

        Vector2 delta = localPointAfter - localPointBefore;
        content.anchoredPosition += delta * scale;
    }

    private void HandleDrag()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (!isDragging) return;

        Vector2 currentMousePos = Mouse.current.position.ReadValue();
        Vector2 delta = currentMousePos - lastMousePosition;

        content.anchoredPosition += delta * dragSpeed;

        lastMousePosition = currentMousePos;
    }

    private void UpdateTooltipPosition()
    {
        if (!tooltip.activeSelf) return;

        tooltip.transform.position =
            Mouse.current.position.ReadValue() + tooltipOffset;
    }
    #endregion

    private void BuildParents(List<TalentNode> nodes)
    {
        var dict = new Dictionary<string, TalentNode>();

        foreach (var n in nodes)
            dict[n.data.id] = n;

        foreach (var node in nodes)
        {
            foreach (var connId in node.data.connections)
            {
                if (dict.TryGetValue(connId, out var child))
                {
                    child.parents.Add(node.data.id);
                }
            }
        }
    }

    private void CreateTreeUI()
    {
        layout = GenerateRadialLayout(skillTree.allNodes);

        foreach (var node in skillTree.allNodes)
            CreateNodeUI(node);

        DrawConnections();
    }

    private void CreateNodeUI(TalentNode node)
    {
        GameObject prefab = GetPrefab(node);

        GameObject obj = Instantiate(prefab, nodesContainer);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = layout[node.data.id];

        SetupNodeVisual(obj, node);
        SetupNodeEvents(obj, node);

        nodeButtons[node.data.id] = obj.GetComponent<Button>();
        nodeImages[node.data.id] = obj.GetComponent<Image>();
    }

    private void SetupNodeVisual(GameObject obj, TalentNode node)
    {
        Image icon = obj.transform.GetChild(0).GetComponent<Image>();
        icon.sprite = node.data.icon;
    }

    private void SetupNodeEvents(GameObject obj, TalentNode node)
    {
        Button btn = obj.GetComponent<Button>();

        btn.onClick.AddListener(() => TryUnlockNode(node.data.id));

        AddTooltipEvents(obj, node);
    }

    private GameObject GetPrefab(TalentNode node)
    {
        if (IsKeystone(node))
            return nodeButtonKeystonePrefab;

        return node.data.modifierType switch
        {
            ModifierType.Flat => nodeButtonFlatPrefab,
            ModifierType.Increased => nodeButtonIncreasedPrefab,
            ModifierType.More => nodeButtonMorePrefab,
            _ => nodeButtonFlatPrefab
        };
    }

    private void AddTooltipEvents(GameObject obj, TalentNode node)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => ShowTooltip(node));

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => HideTooltip());

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    private void ShowTooltip(TalentNode node)
    {
        tooltip.SetActive(true);

        tooltipText.text = IsKeystone(node)
            ? GetKeystoneTooltip(node)
            : GetStatTooltip(node);
    }

    private string GetKeystoneTooltip(TalentNode node)
    {
        var ks = node.data.keystoneEffect;

        return $"<b>{ks.title.GetLocalizedString()}</b>\n{ks.description.GetLocalizedString()}";
    }

    private string GetStatTooltip(TalentNode node)
    {
        string name = node.data.nodeName.GetLocalizedString();
        string type = GetLocalizedModifier(node.data.modifierType);
        string value = FormatValue(node);

        return $"<b>{name}</b>\n{type}: {value}";
    }

    private string GetLocalizedModifier(ModifierType type)
    {
        return type switch
        {
            ModifierType.Flat => flatText.GetLocalizedString(),
            ModifierType.Increased => increasedText.GetLocalizedString(),
            ModifierType.More => moreText.GetLocalizedString(),
            _ => type.ToString()
        };
    }
    private string FormatValue(TalentNode node)
    {
        float value = node.data.statValue;

        return node.data.modifierType switch
        {
            ModifierType.Increased => $"+{value}%",
            ModifierType.More => $"x{1f + value}",
            _ => $"+{value}"
        };
    }

    private void HideTooltip()
    {
        tooltip.SetActive(false);
    }

    public Dictionary<string, Vector2> GenerateRadialLayout(List<TalentNode> nodes)
    {
        var result = new Dictionary<string, Vector2>();
        var nodeDict = new Dictionary<string, TalentNode>();

        // Строим словарь id -> node
        foreach (var n in nodes)
        {
            if (n == null || n.data == null) continue;
            nodeDict[n.data.id] = n;

            // сразу ставим кастомную позицию для мостовых нод
            if (n.data.isBridgeNode)
                result[n.data.id] = n.data.customPosition;
        }

        if (nodes.Count == 0 || nodes[0] == null || nodes[0].data == null)
        {
            Debug.LogError("Root node is missing or has no data assigned!");
            return result;
        }

        var root = nodes[0];
        result[root.data.id] = Vector2.zero;

        int branchCount = root.data.connections.Count;
        float totalAngle = 360f;

        for (int i = 0; i < branchCount; i++)
        {
            var connId = root.data.connections[i];
            if (!nodeDict.TryGetValue(connId, out var branchNode)) continue;

            float angle = i * (totalAngle / branchCount);
            BuildRadialBranch(branchNode, nodeDict, result, angle, 25f, 1);
        }

        return result;
    }

    private void BuildRadialBranch(
    TalentNode node,
    Dictionary<string, TalentNode> nodeDict,
    Dictionary<string, Vector2> layout,
    float angle,        // центральный угол для этой ветки
    float angleSpread,  // разброс для детей
    int depth)
    {
        if (layout.ContainsKey(node.data.id))
            return;

        if (node.data.isBridgeNode)
        {
            layout[node.data.id] = node.data.customPosition;
            return;
        }

        float radiusStep = 150f;
        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 pos = dir * depth * radiusStep;

        float spread = 0.1f;
        Vector2 offset = new Vector2(Random.Range(-spread, spread), Random.Range(-spread, spread));
        layout[node.data.id] = pos + offset * radiusStep * 0.3f;

        if (node.data.connections.Count == 0)
            return;

        float childAngleStep = angleSpread / Mathf.Max(1, node.data.connections.Count - 1);

        // Распределяем детей по углу
        float startAngle = angle - angleSpread / 2f;
        for (int i = 0; i < node.data.connections.Count; i++)
        {
            var childId = node.data.connections[i];
            if (nodeDict.TryGetValue(childId, out var child))
            {
                float childAngle = startAngle + i * childAngleStep;
                BuildRadialBranch(child, nodeDict, layout, childAngle, angleSpread * 0.7f, depth + 1);
            }
        }
    }

    private void TryUnlockNode(string nodeId)
    {
        var node = skillTree.GetNode(nodeId);
        if (node != null)
            skillTree.UnlockNode(node);
    }

    private void OnNodeUnlocked(TalentNode node)
    {
        if (node.data.keystoneEffect != null)
        {
            Debug.Log($"Keystone activated: {node.data.nodeName}");
        }

        RefreshTreeVisuals();
    }

    private void UpdateNodeState(TalentNode node)
    {
        if (!nodeButtons.TryGetValue(node.data.id, out var btn)) return;

        Image img = nodeImages[node.data.id];

        if (node == skillTree.allNodes[0])
        {
            img.enabled = false;
            btn.interactable = false;
            return;
        }

        if (IsKeystone(node))
        {
            img.color = GetStateColor(GetNodeState(node));
            return;
        }

        img.color = GetStateColor(GetNodeState(node));
    }

    private Color GetStateColor(NodeVisualState state)
    {
        return state switch
        {
            NodeVisualState.Locked => lockedColor,
            NodeVisualState.Available => availableColor,
            NodeVisualState.Unlocked => unlockedColor,
            _ => lockedColor
        };
    }

    private NodeVisualState GetNodeState(TalentNode node)
    {
        if (node.isUnlocked)
            return NodeVisualState.Unlocked;

        if (skillTree.CanUnlock(node))
            return NodeVisualState.Available;

        return NodeVisualState.Locked;
    }

    private void RefreshTreeVisuals()
    {
        foreach (var node in skillTree.allNodes)
            UpdateNodeState(node);
    }

    private void UpdatePointsUI(int points)
    {
        string localizeSkill = talantedText.GetLocalizedString();
        pointsText.text = $"{localizeSkill}: {points}";
    }

    private void DrawConnections()
    {
        drawnConnections.Clear();

        foreach (var node in skillTree.allNodes)
        {
            foreach (string connId in node.data.connections)
            {
                string key = node.data.id + "_" + connId;
                string reverseKey = connId + "_" + node.data.id;

                if (drawnConnections.Contains(key) || drawnConnections.Contains(reverseKey))
                    continue;

                var connected = skillTree.GetNode(connId);
                if (connected != null)
                {
                    DrawLine(
                        layout[node.data.id],
                        layout[connected.data.id]
                    );

                    drawnConnections.Add(key);
                }
            }
        }
    }
    private void DrawLine(Vector2 start, Vector2 end)
    {
        GameObject line = new GameObject("Line");
        line.transform.SetParent(lineContainer);

        RectTransform rect = line.AddComponent<RectTransform>();
        Image image = line.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.2f);

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);

        Vector2 dir = (end - start).normalized;
        float dist = Vector2.Distance(start, end);

        rect.anchoredPosition = start;
        rect.sizeDelta = new Vector2(dist, 3f);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0, 0, angle);
    }

    private bool IsKeystone(TalentNode node)
    {
        return node.data.keystoneEffect != null;
    }

    private void ClearTreeUI()
    {
        foreach (Transform child in nodesContainer)
            Destroy(child.gameObject);

        foreach (Transform child in lineContainer)
            Destroy(child.gameObject);

        nodeButtons.Clear();
        nodeImages.Clear();
        drawnConnections.Clear();
    }

    private void OnDestroy()
    {
        if (skillTree != null)
        {
            skillTree.OnResetTree -= RefreshTreeVisuals;
            if (resetButton != null) resetButton.onClick.RemoveListener(skillTree.ResetTreeProgress);
        }

        GameEvents.OnTriggerSkillTree -= TogglePanel;
        //GameEvents.OnToggleTree -= OpenSkillTree;
    }
}
