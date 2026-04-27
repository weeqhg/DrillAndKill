using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public SkillTreeTooltip tooltip;

    [SerializeField] private Image iconTree;
    [SerializeField] private GameObject nodeButtonFlatPrefab;
    [SerializeField] private GameObject nodeButtonIncreasedPrefab;
    [SerializeField] private GameObject nodeButtonMorePrefab;
    [SerializeField] private GameObject nodeButtonKeystonePrefab;
    [SerializeField] private Transform nodesContainer;
    [SerializeField] private Transform lineContainer;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private Button resetButton;
    [SerializeField] private RectTransform rectTransforms;

    [Header("Localize")]
    [SerializeField] private LocalizedString coastText;
    [SerializeField] private LocalizedString talantedText;

    [Header("Colors")]
    [SerializeField] private Color lockedColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color availableColor = new Color(0.8f, 0.8f, 0.3f);
    [SerializeField] private Color unlockedColor = new Color(0.2f, 0.8f, 0.2f);

    [Header("Navigation")]
    [SerializeField] private RectTransform content;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2f;
    [SerializeField] private Vector2 treeBoundsPadding = new Vector2(180f, 140f);
    [SerializeField] private float viewportPadding = 80f;

    private readonly Dictionary<string, Button> nodeButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Image> nodeImages = new Dictionary<string, Image>();
    private readonly Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();
    private readonly HashSet<string> drawnConnections = new HashSet<string>();

    private SkillTreeStats skillTree;
    private TalentPointsCounter talentPoints;
    private AutoPopup treePopup;
    private RectTransform rect;
    private bool isOpen;
    private bool isDragging;
    private Vector2 lastMousePosition;

    public void Initialize(SkillTreeStats skillTreeStats, int size = 0)
    {
        Unsubscribe();

        skillTree = skillTreeStats;
        if (skillTree == null)
        {
            Debug.LogError("[SkillTreeUI] SkillTreeStats is null.");
            return;
        }

        skillTree.OnNodeUnlocked += OnNodeUnlocked;
        skillTree.OnResetTree += RefreshTreeVisuals;

        if (skillTree.iconCharacter != null)
            iconTree.sprite = skillTree.iconCharacter;

        rect = GetComponent<RectTransform>();
        rect.offsetMin = new Vector2(0, rect.offsetMin.y);
        rect.offsetMax = new Vector2(-size, rect.offsetMax.y);

        treePopup = GetComponent<AutoPopup>();
        treePopup.Initialize();

        talentPoints = skillTree.GetComponentInChildren<TalentPointsCounter>();
        if (talentPoints != null)
        {
            talentPoints.OnPointsChanged += UpdatePointsUI;
            UpdatePointsUI(talentPoints.Points);
        }
        else
        {
            UpdatePointsUI(0);
        }

        resetButton.onClick.RemoveListener(ResetTree);
        resetButton.onClick.AddListener(ResetTree);

        RebuildTreeUI();
        RefreshTreeVisuals();
        content.localScale = Vector3.one;
        CenterContentOnTree();

        HideTooltip();

        gameObject.SetActive(false);

        ConsoleEvents.OnCommandToggleSkillTree -= TogglePanel;
        ConsoleEvents.OnCommandToggleSkillTree += TogglePanel;
    }

    public void TogglePanel()
    {
        if (gameObject.activeSelf)
            G.UIManager.Close(this);
        else
            G.UIManager.OpenOverlay(this);
    }

    public void UpdateTree()
    {
        if (skillTree == null)
            return;

        RebuildTreeUI();
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

    private void Update()
    {
        if (isOpen && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransforms, mousePos))
                G.UIManager.CloseTop();
        }

        HandleZoom();
        HandleDrag();
        ClampContentToTreeBounds();
    }

    private void HandleZoom()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, mousePos, null, out Vector2 localPointBefore);

        float scale = content.localScale.x;
        scale += scroll * zoomSpeed * 0.01f;
        scale = Mathf.Clamp(scale, minZoom, maxZoom);
        content.localScale = Vector3.one * scale;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, mousePos, null, out Vector2 localPointAfter);

        Vector2 delta = localPointAfter - localPointBefore;
        content.anchoredPosition += delta * scale;
        ClampContentToTreeBounds();
    }

    private void HandleDrag()
    {
        if (Mouse.current == null || content == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            isDragging = false;

        if (!isDragging)
            return;

        Vector2 currentMousePosition = Mouse.current.position.ReadValue();
        Vector2 delta = currentMousePosition - lastMousePosition;
        lastMousePosition = currentMousePosition;

        content.anchoredPosition += delta;
        ClampContentToTreeBounds();
    }

    private void ResetTree()
    {
        if (skillTree == null)
            return;

        skillTree.ResetTreeProgress();
        RefreshTreeVisuals();
    }

    private void RebuildTreeUI()
    {
        ClearTreeUI();
        CacheNodePositions();

        foreach (TalentNode node in skillTree.allNodes)
            CreateNodeUI(node);

        DrawConnections();
        ClampContentToTreeBounds();
    }

    private void CacheNodePositions()
    {
        nodePositions.Clear();

        foreach (TalentNode node in skillTree.allNodes)
        {
            if (node == null || node.data == null || string.IsNullOrWhiteSpace(node.data.id))
                continue;

            nodePositions[node.data.id] = GetNodePosition(node);
        }
    }

    private Vector2 GetNodePosition(TalentNode node)
    {
        if (node == null || node.data == null)
            return Vector2.zero;

        return node.data.position;
    }

    private void CreateNodeUI(TalentNode node)
    {
        if (node == null || node.data == null)
            return;

        GameObject prefab = GetPrefab(node);
        GameObject obj = Instantiate(prefab, nodesContainer);

        RectTransform nodeRect = obj.GetComponent<RectTransform>();
        if (nodePositions.TryGetValue(node.data.id, out Vector2 position))
            nodeRect.anchoredPosition = position;

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
        if (IsSpecialNode(node))
            return nodeButtonKeystonePrefab;

        switch (node.data.modifierType)
        {
            case ModifierType.Flat:
                return nodeButtonFlatPrefab;
            case ModifierType.Increased:
                return nodeButtonIncreasedPrefab;
            case ModifierType.More:
                return nodeButtonMorePrefab;
            default:
                return nodeButtonFlatPrefab;
        }
    }

    private void AddTooltipEvents(GameObject obj, TalentNode node)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = obj.AddComponent<EventTrigger>();

        if (trigger.triggers == null)
            trigger.triggers = new List<EventTrigger.Entry>();

        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener(_ => ShowTooltip(node));

        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener(_ => HideTooltip());

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    private void ShowTooltip(TalentNode node)
    {
        if (tooltip != null)
            tooltip.ShowTooltip(node);
    }

    private void HideTooltip()
    {
        if (tooltip != null)
            tooltip.HideTooltip();
    }

    private void TryUnlockNode(string nodeId)
    {
        TalentNode node = skillTree.GetNode(nodeId);
        if (node != null)
            skillTree.UnlockNode(node);
    }

    private void OnNodeUnlocked(TalentNode node)
    {
        if (node != null && node.data != null && node.data.itemEffect != null)
            Debug.Log("Special talent activated: " + node.data.id);

        RefreshTreeVisuals();
    }

    private void UpdateNodeState(TalentNode node)
    {
        if (node == null || node.data == null)
            return;

        if (!nodeButtons.TryGetValue(node.data.id, out Button btn))
            return;

        if (!nodeImages.TryGetValue(node.data.id, out Image img))
            return;

        if (skillTree.allNodes.Count > 0 && node == skillTree.allNodes[0])
        {
            img.enabled = false;
            btn.interactable = false;
            return;
        }

        img.enabled = true;
        img.color = GetStateColor(GetNodeState(node));
        btn.interactable = true;
    }

    private Color GetStateColor(NodeVisualState state)
    {
        switch (state)
        {
            case NodeVisualState.Locked:
                return lockedColor;
            case NodeVisualState.Available:
                return availableColor;
            case NodeVisualState.Unlocked:
                return unlockedColor;
            default:
                return lockedColor;
        }
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
        if (skillTree == null)
            return;

        foreach (TalentNode node in skillTree.allNodes)
            UpdateNodeState(node);
    }

    private void UpdatePointsUI(int points)
    {
        string localizeSkill = talantedText.GetLocalizedString();
        pointsText.text = localizeSkill + ": " + points;
    }

    private void DrawConnections()
    {
        drawnConnections.Clear();

        foreach (TalentNode node in skillTree.allNodes)
        {
            if (node == null || node.data == null || node.data.connections == null)
                continue;

            if (!nodePositions.TryGetValue(node.data.id, out Vector2 start))
                continue;

            foreach (string connId in node.data.connections)
            {
                if (string.IsNullOrWhiteSpace(connId))
                    continue;

                string key = node.data.id + "*" + connId;
                string reverseKey = connId + "*" + node.data.id;

                if (drawnConnections.Contains(key) || drawnConnections.Contains(reverseKey))
                    continue;

                TalentNode connected = skillTree.GetNode(connId);
                if (connected == null)
                    continue;

                if (!nodePositions.TryGetValue(connected.data.id, out Vector2 end))
                    continue;

                DrawLine(start, end);
                drawnConnections.Add(key);
            }
        }
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        GameObject line = new GameObject("Line");
        line.transform.SetParent(lineContainer, false);

        RectTransform lineRect = line.AddComponent<RectTransform>();
        Image image = line.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.2f);

        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0f, 0.5f);

        Vector2 dir = (end - start).normalized;
        float dist = Vector2.Distance(start, end);

        lineRect.anchoredPosition = start;
        lineRect.sizeDelta = new Vector2(dist, 3f);
        lineRect.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    private bool IsSpecialNode(TalentNode node)
    {
        return node != null && node.data != null && node.data.itemEffect != null;
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
        nodePositions.Clear();
    }

    private void ClampContentToTreeBounds()
    {
        if (content == null || nodePositions.Count == 0)
            return;

        RectTransform viewport = content.parent as RectTransform;
        if (viewport == null)
            return;

        GetTreeBounds(out Vector2 min, out Vector2 max);

        float scale = content.localScale.x;
        float halfViewportWidth = Mathf.Max(0f, viewport.rect.width * 0.5f - viewportPadding);
        float halfViewportHeight = Mathf.Max(0f, viewport.rect.height * 0.5f - viewportPadding);

        float minAllowedX = -halfViewportWidth - max.x * scale;
        float maxAllowedX = halfViewportWidth - min.x * scale;
        float minAllowedY = -halfViewportHeight - max.y * scale;
        float maxAllowedY = halfViewportHeight - min.y * scale;

        Vector2 position = content.anchoredPosition;
        position.x = ClampAxis(position.x, minAllowedX, maxAllowedX);
        position.y = ClampAxis(position.y, minAllowedY, maxAllowedY);
        content.anchoredPosition = position;
    }

    private void CenterContentOnTree()
    {
        if (content == null || nodePositions.Count == 0)
            return;

        GetTreeBounds(out Vector2 min, out Vector2 max);
        Vector2 center = (min + max) * 0.5f;
        content.anchoredPosition = -center;
        ClampContentToTreeBounds();
    }

    private void GetTreeBounds(out Vector2 min, out Vector2 max)
    {
        bool hasValue = false;
        min = Vector2.zero;
        max = Vector2.zero;

        foreach (KeyValuePair<string, Vector2> pair in nodePositions)
        {
            Vector2 position = pair.Value;

            if (!hasValue)
            {
                min = position;
                max = position;
                hasValue = true;
                continue;
            }

            min = Vector2.Min(min, position);
            max = Vector2.Max(max, position);
        }

        min -= treeBoundsPadding;
        max += treeBoundsPadding;
    }

    private float ClampAxis(float value, float min, float max)
    {
        if (min > max)
            return (min + max) * 0.5f;

        return Mathf.Clamp(value, min, max);
    }

    private void Unsubscribe()
    {
        if (skillTree != null)
        {
            skillTree.OnNodeUnlocked -= OnNodeUnlocked;
            skillTree.OnResetTree -= RefreshTreeVisuals;
        }

        if (talentPoints != null)
            talentPoints.OnPointsChanged -= UpdatePointsUI;
    }

    private void OnDestroy()
    {
        Unsubscribe();

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetTree);

        ConsoleEvents.OnCommandToggleSkillTree -= TogglePanel;
    }
}
