using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillTreeGridBackground : IMGUIContainer
{
    private readonly System.Func<float> getGridSize;
    private readonly System.Func<Vector3> getViewPosition;
    private readonly System.Func<Vector3> getViewScale;

    private readonly Color backgroundColor = new Color(0.11f, 0.11f, 0.11f, 1f);
    private readonly Color minorLineColor = new Color(1f, 1f, 1f, 0.08f);
    private readonly Color majorLineColor = new Color(1f, 1f, 1f, 0.16f);

    public SkillTreeGridBackground(
        System.Func<float> getGridSize,
        System.Func<Vector3> getViewPosition,
        System.Func<Vector3> getViewScale)
    {
        this.getGridSize = getGridSize;
        this.getViewPosition = getViewPosition;
        this.getViewScale = getViewScale;

        style.position = Position.Absolute;
        style.left = 0f;
        style.top = 0f;
        style.right = 0f;
        style.bottom = 0f;
        pickingMode = PickingMode.Ignore;

        onGUIHandler = DrawGrid;
    }

    private void DrawGrid()
    {
        Rect rect = new Rect(0f, 0f, layout.width, layout.height);
        if (rect.width <= 0f || rect.height <= 0f)
            return;

        EditorGUI.DrawRect(rect, backgroundColor);

        float gridSize = Mathf.Max(1f, getGridSize != null ? getGridSize() : 25f);
        Vector3 viewPosition = getViewPosition != null ? getViewPosition() : Vector3.zero;
        Vector3 viewScale = getViewScale != null ? getViewScale() : Vector3.one;
        float zoom = Mathf.Max(0.01f, viewScale.x);

        float scaledGridSize = gridSize * zoom;
        float majorGridSize = scaledGridSize * 4f;

        float minorOffsetX = Mathf.Repeat(viewPosition.x, scaledGridSize);
        float minorOffsetY = Mathf.Repeat(viewPosition.y, scaledGridSize);
        float majorOffsetX = Mathf.Repeat(viewPosition.x, majorGridSize);
        float majorOffsetY = Mathf.Repeat(viewPosition.y, majorGridSize);

        Handles.BeginGUI();
        DrawGridLines(rect, scaledGridSize, minorOffsetX, minorOffsetY, minorLineColor);
        DrawGridLines(rect, majorGridSize, majorOffsetX, majorOffsetY, majorLineColor);
        Handles.EndGUI();
    }

    private void DrawGridLines(Rect rect, float spacing, float offsetX, float offsetY, Color color)
    {
        if (spacing < 4f)
            return;

        Handles.color = color;

        for (float x = offsetX; x < rect.width; x += spacing)
            Handles.DrawLine(new Vector3(x, 0f), new Vector3(x, rect.height));

        for (float y = offsetY; y < rect.height; y += spacing)
            Handles.DrawLine(new Vector3(0f, y), new Vector3(rect.width, y));
    }
}
