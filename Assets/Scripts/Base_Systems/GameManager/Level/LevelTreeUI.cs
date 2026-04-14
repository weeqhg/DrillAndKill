using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using System.Linq;

public class LevelTreeUI : UIWindow, ICloseBlocker
{
    [SerializeField] private GameObject nodeButtonEnemyPrefab;
    [SerializeField] private GameObject nodeButtonShopPrefab;
    [SerializeField] private GameObject nodeButtonSecretPrefab;
    [SerializeField] private GameObject nodeButtonFinalPrefab;

    [SerializeField] private Transform nodesContainer;
    [SerializeField] private Transform lineContainer;

    [SerializeField] private RectTransform rectTransforms;

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
    [SerializeField] private Color unlockedColor = new Color(0.8f, 0.8f, 0.3f);

    [Header("Navigation")]
    [SerializeField] private RectTransform content; // общий родитель для nodes + lines

    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2f;

    private LevelTree levelTree;

    private Dictionary<string, Button> nodeButtons = new();
    private Dictionary<string, Image> nodeImages = new();
    private Vector2 tooltipOffset = new Vector2(100f, -150f);
    private Dictionary<string, Vector2> layout;
    private HashSet<string> drawnConnections = new();

    private AutoPopup levelPopup;

    public void Initialize(LevelTree tree)
    {
        levelTree = tree;
        levelPopup = GetComponent<AutoPopup>();
        levelPopup.Initialize();

        BuildParents(levelTree.allNodes);
        CreateTreeUI();
        RefreshTreeVisuals();

        tooltip.SetActive(false);

        gameObject.SetActive(false);

        GameEvents.OnTriggerLevelTree += TogglePanel;
    }

    public void ResetTreeLevel()
    {
        ClearTreeUI();
        BuildParents(levelTree.allNodes);
        CreateTreeUI();
        RefreshTreeVisuals();
    }

    private void RefreshTreeVisuals()
    {
        foreach (var node in levelTree.allNodes)
            UpdateNodeState(node);
    }

    private void UpdateNodeState(LevelNode node)
    {
        if (!nodeButtons.TryGetValue(node.data.id, out var btn)) return;

        Image img = nodeImages[node.data.id];

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

    private NodeVisualState GetNodeState(LevelNode node)
    {
        if (node.isUnlocked)
            return NodeVisualState.Unlocked;

        if (levelTree.CanUnlock(node))
            return NodeVisualState.Available;

        return NodeVisualState.Locked;
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

    public override void Show()
    {
        base.Show();
        levelPopup.OpenPanel();
    }

    public override void Hide()
    {
        base.Hide();
        levelPopup.ClosePanel();
    }

    #region  Move on Deck
    private void Update()
    {
        HandleZoom();
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

    private void UpdateTooltipPosition()
    {
        if (!tooltip.activeSelf) return;

        tooltip.transform.position =
            Mouse.current.position.ReadValue() + tooltipOffset;
    }
    #endregion

    private void BuildParents(List<LevelNode> level)
    {
        var dict = new Dictionary<string, LevelNode>();

        foreach (var n in level)
        {
            dict[n.data.id] = n;
            n.parents.Clear();
        }

        foreach (var node in level)
        {
            foreach (var connId in node.connections)
            {
                if (dict.TryGetValue(connId, out var child))
                {
                    if (!child.parents.Contains(node.data.id))
                        child.parents.Add(node.data.id);
                }
            }
        }
    }

    private void CreateTreeUI()
    {
        layout = GenerateTreeLayout(levelTree.allNodes);

        foreach (var node in levelTree.allNodes)
            CreateNodeUI(node);

        DrawConnections();
    }

    private Dictionary<string, Vector2> GenerateTreeLayout(List<LevelNode> nodes)
    {
        Dictionary<string, Vector2> positions = new();
        if (nodes.Count == 0) return positions;

        var depths = CalculateDepths(nodes);

        float levelHeight = 200f;
        float nodeSpacing = 250f;

        // --- группировка по уровням
        Dictionary<int, List<LevelNode>> levels = new();
        foreach (var node in nodes)
        {
            int depth = depths[node.data.id];
            if (!levels.ContainsKey(depth))
                levels[depth] = new List<LevelNode>();

            levels[depth].Add(node);
        }

        // --- расстановка нод уровнями
        foreach (var kvp in levels.OrderBy(k => k.Key))
        {
            int level = kvp.Key;
            var levelNodes = kvp.Value;

            // для первого уровня просто ставим слева направо
            if (level == 0)
            {
                // одна нода — просто ставим X = 0 (центр)
                positions[levelNodes[0].data.id] = new Vector2(-250, level * levelHeight);
                continue;
            }

            // для остальных уровней центрируем по родителям
            foreach (var node in levelNodes)
            {
                if (node.parents == null || node.parents.Count == 0)
                {
                    positions[node.data.id] = new Vector2(0, level * levelHeight);
                    continue;
                }

                // средняя X позиция родителей
                float avgX = node.parents
                    .Where(pId => positions.ContainsKey(pId))
                    .Select(pId => positions[pId].x)
                    .DefaultIfEmpty(0)
                    .Average();

                positions[node.data.id] = new Vector2(avgX, level * levelHeight);
            }

            // --- минимальное расстояние между соседними нодами
            levelNodes.Sort((a, b) => positions[a.data.id].x.CompareTo(positions[b.data.id].x));
            for (int i = 1; i < levelNodes.Count; i++)
            {
                var left = positions[levelNodes[i - 1].data.id];
                var right = positions[levelNodes[i].data.id];

                if (right.x - left.x < nodeSpacing)
                    positions[levelNodes[i].data.id] = new Vector2(left.x + nodeSpacing, right.y);
            }
        }

        // --- финальное центрирование всего дерева по X
        float minX = positions.Min(p => p.Value.x);
        float maxX = positions.Max(p => p.Value.x);
        float centerOffset = (minX + maxX) / 2f;

        var finalPositions = new Dictionary<string, Vector2>();
        foreach (var kvp in positions)
        {
            if (depths[kvp.Key] == 0)
                finalPositions[kvp.Key] = kvp.Value; // Start node остаётся на X = 0
            else
                finalPositions[kvp.Key] = new Vector2(kvp.Value.x - centerOffset, kvp.Value.y);
        }

        return finalPositions;
    }

    private Dictionary<string, int> CalculateDepths(List<LevelNode> nodes)
    {
        Dictionary<string, int> depths = new();
        foreach (var node in nodes)
            depths[node.data.id] = 0;

        Queue<LevelNode> queue = new Queue<LevelNode>();

        // Находим стартовую ноду
        var startNode = nodes.Find(n => n.data.id == "Start");
        if (startNode != null)
        {
            depths[startNode.data.id] = 0;
            queue.Enqueue(startNode);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int currentDepth = depths[current.data.id];

            foreach (var childId in current.connections)
            {
                var child = nodes.Find(n => n.data.id == childId);
                if (child != null)
                {
                    if (depths[child.data.id] < currentDepth + 1)
                    {
                        depths[child.data.id] = currentDepth + 1;
                        queue.Enqueue(child);
                    }
                }
            }
        }

        return depths;
    }

    private void CreateNodeUI(LevelNode node)
    {
        GameObject prefab = GetPrefab(node);

        GameObject obj = Instantiate(prefab, nodesContainer);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = layout[node.data.id];

        SetupNodeEvents(obj, node);

        nodeButtons[node.data.id] = obj.GetComponent<Button>();
        nodeImages[node.data.id] = obj.GetComponent<Image>();
    }

    private GameObject GetPrefab(LevelNode node)
    {
        return node.data.sceneType switch
        {
            SceneType.Arena => nodeButtonEnemyPrefab,
            SceneType.Shop => nodeButtonShopPrefab,
            SceneType.Secret => nodeButtonSecretPrefab,
            _ => nodeButtonFinalPrefab
        };
    }

    private void SetupNodeEvents(GameObject obj, LevelNode node)
    {
        Button btn = obj.GetComponent<Button>();

        btn.onClick.AddListener(() => TryUnlockNode(node.data.id));

        AddTooltipEvents(obj, node);
    }

    private void TryUnlockNode(string nodeId)
    {
        var node = levelTree.GetNode(nodeId);

        if (node != null)
        {
            levelTree.UnlockNode(node);

            RefreshTreeVisuals();
        }
    }

    private void AddTooltipEvents(GameObject obj, LevelNode node)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => ShowTooltip(node));

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => HideTooltip());

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    private void ShowTooltip(LevelNode node)
    {
        tooltip.SetActive(true);

        tooltipText.text = GetTooltip(node);
    }

    private string GetTooltip(LevelNode node)
    {
        return $"<b>{node.data.nodeName.GetLocalizedString()}</b>";
        //\n{node.data.description.GetLocalizedString()}";
    }

    private void HideTooltip()
    {
        tooltip.SetActive(false);
    }

    private void DrawConnections()
    {
        drawnConnections.Clear();

        foreach (var node in levelTree.allNodes)
        {
            foreach (string connId in node.connections)
            {
                string key = node.data.id + "_" + connId;
                string reverseKey = connId + "_" + node.data.id;

                if (drawnConnections.Contains(key) || drawnConnections.Contains(reverseKey))
                    continue;

                var connected = levelTree.GetNode(connId);
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
        line.transform.SetParent(lineContainer, false);

        RectTransform rect = line.AddComponent<RectTransform>();
        Image image = line.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.2f);

        // 🔥 pivot по центру
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        Vector2 direction = end - start;
        float distance = direction.magnitude;

        // 🔥 позиция = середина
        rect.anchoredPosition = (start + end) / 2f;

        // 🔥 размер
        rect.sizeDelta = new Vector2(distance, 4f);

        // 🔥 поворот
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0, 0, angle);
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
        GameEvents.OnTriggerLevelTree -= TogglePanel;
    }
}
