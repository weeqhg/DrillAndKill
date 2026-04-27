using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class SkillTreeNodeView : Node
{
    public TalentNodeData Data { get; }

    public Port InputPort { get; private set; }
    public Port OutputPort { get; private set; }

    private readonly Image iconImage;
    private readonly Label idLabel;
    private readonly VisualElement hoverTooltip;
    private readonly Label hoverTooltipLabel;

    public SkillTreeNodeView(TalentNodeData data)
    {
        Data = data;
        viewDataKey = data != null ? data.id : System.Guid.NewGuid().ToString();

        title = string.Empty;

        InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        InputPort.portName = "In";
        inputContainer.Add(InputPort);

        OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        OutputPort.portName = "Out";
        outputContainer.Add(OutputPort);

        titleContainer.style.display = DisplayStyle.None;

        iconImage = new Image();
        iconImage.scaleMode = ScaleMode.ScaleToFit;
        iconImage.image = Data != null ? Data.icon != null ? Data.icon.texture : null : null;
        iconImage.style.flexGrow = 1f;
        iconImage.style.unityBackgroundImageTintColor = Color.white;
        iconImage.style.marginLeft = 6f;
        iconImage.style.marginRight = 6f;
        iconImage.style.marginTop = 6f;
        iconImage.style.marginBottom = 2f;
        mainContainer.Add(iconImage);

        idLabel = new Label(Data != null ? Data.id : "Node");
        idLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        idLabel.style.fontSize = 10;
        idLabel.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        idLabel.style.marginBottom = 4f;
        mainContainer.Add(idLabel);

        hoverTooltip = new VisualElement();
        hoverTooltip.style.position = Position.Absolute;
        hoverTooltip.style.left = 100f;
        hoverTooltip.style.top = 0f;
        hoverTooltip.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.97f);
        hoverTooltip.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        hoverTooltip.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        hoverTooltip.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        hoverTooltip.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        hoverTooltip.style.borderBottomWidth = 1f;
        hoverTooltip.style.borderTopWidth = 1f;
        hoverTooltip.style.borderLeftWidth = 1f;
        hoverTooltip.style.borderRightWidth = 1f;
        hoverTooltip.style.paddingLeft = 8f;
        hoverTooltip.style.paddingRight = 8f;
        hoverTooltip.style.paddingTop = 6f;
        hoverTooltip.style.paddingBottom = 6f;
        hoverTooltip.style.maxWidth = 280f;
        hoverTooltip.style.display = DisplayStyle.None;
        hoverTooltip.pickingMode = PickingMode.Ignore;

        hoverTooltipLabel = new Label();
        hoverTooltipLabel.style.whiteSpace = WhiteSpace.Normal;
        hoverTooltipLabel.style.fontSize = 11;
        hoverTooltipLabel.style.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        hoverTooltip.Add(hoverTooltipLabel);
        Add(hoverTooltip);

        outputContainer.style.marginTop = 2f;
        inputContainer.style.marginBottom = 2f;

        style.minWidth = 0f;
        style.minHeight = 0f;

        UpdateTooltip();
        RegisterCallback<MouseEnterEvent>(_ => hoverTooltip.style.display = DisplayStyle.Flex);
        RegisterCallback<MouseLeaveEvent>(_ => hoverTooltip.style.display = DisplayStyle.None);
        RefreshExpandedState();
        RefreshPorts();
    }

    public void RefreshFromData()
    {
        iconImage.image = Data != null ? Data.icon != null ? Data.icon.texture : null : null;
        idLabel.text = Data != null ? Data.id : "Node";
        UpdateTooltip();
    }

    public void ApplySize(Vector2 size)
    {
        style.width = size.x;
        style.height = size.y;
        style.minWidth = size.x;
        style.minHeight = size.y;
        style.maxWidth = size.x;
        style.maxHeight = size.y;
        iconImage.style.width = size.x - 16f;
        iconImage.style.height = Mathf.Max(24f, size.y - 44f);
        hoverTooltip.style.left = size.x + 8f;
    }

    public void SetPortMirror(bool mirrored)
    {
        inputContainer.Clear();
        outputContainer.Clear();

        if (mirrored)
        {
            outputContainer.Add(InputPort);
            inputContainer.Add(OutputPort);
        }
        else
        {
            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);
        }

        RefreshPorts();
        RefreshExpandedState();
    }

    private void UpdateTooltip()
    {
        if (Data == null)
        {
            hoverTooltipLabel.text = "Talent Node";
            return;
        }

        string nodeNameText = string.Empty;
        string descriptionText = string.Empty;

        nodeNameText = GetLocalizedPreview(Data.nodeName, Data.id);
        descriptionText = GetLocalizedPreview(Data.description, string.Empty);

        string displayName = string.IsNullOrWhiteSpace(nodeNameText) ? Data.id : nodeNameText;
        string effectText = Data.itemEffect != null ? Data.itemEffect.name : "None";
        int connectionCount = Data.connections != null ? Data.connections.Count : 0;

        hoverTooltipLabel.text =
            "ID: " + Data.id +
            "\nName: " + displayName +
            "\nStat: " + Data.statType +
            "\nModifier: " + Data.modifierType +
            "\nValue: " + Data.statValue +
            "\nEffect: " + effectText +
            "\nConnections: " + connectionCount +
            (string.IsNullOrWhiteSpace(descriptionText) ? string.Empty : "\n\n" + descriptionText);
    }

    private string GetLocalizedPreview(LocalizedString localizedString, string fallback)
    {
        if (localizedString == null)
            return fallback;

        try
        {
            if (!string.IsNullOrWhiteSpace(localizedString.TableEntryReference.Key))
                return localizedString.TableEntryReference.Key;
        }
        catch
        {
        }

        try
        {
            long keyId = localizedString.TableEntryReference.KeyId;
            if (keyId != 0)
                return keyId.ToString();
        }
        catch
        {
        }

        return fallback;
    }
}
