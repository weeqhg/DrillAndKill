using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillTreeGraphView : GraphView
{
    private SkillTreeGraph graph;
    private readonly Dictionary<string, SkillTreeNodeView> nodeViewsById = new Dictionary<string, SkillTreeNodeView>();
    private readonly SkillTreeGridBackground gridBackground;

    public SkillTreeGraph Graph
    {
        get { return graph; }
    }

    public SkillTreeGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        gridBackground = new SkillTreeGridBackground(
            () => graph != null && graph.gridSnapSize > 0f ? graph.gridSnapSize : 25f,
            GetViewTranslation,
            GetViewScale);
        Insert(0, gridBackground);

        graphViewChanged += OnGraphViewChanged;
        style.flexGrow = 1f;
        schedule.Execute(() => gridBackground.MarkDirtyRepaint()).Every(16);

        RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        RegisterCallback<DragPerformEvent>(OnDragPerform);
    }

    public void SetGraph(SkillTreeGraph newGraph)
    {
        graph = newGraph;
        gridBackground.MarkDirtyRepaint();
        ReloadGraph();
    }

    public void ReloadGraph()
    {
        ClearGraph();

        if (graph == null)
            return;

        EnsureGraphCollections();

        foreach (TalentNodeData data in graph.nodes.Where(node => node != null))
            CreateNodeView(data);

        foreach (TalentNodeData data in graph.nodes.Where(node => node != null))
        {
            if (data.connections == null)
                continue;

            foreach (string childId in data.connections.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                if (!nodeViewsById.TryGetValue(data.id, out SkillTreeNodeView fromNode))
                    continue;

                if (!nodeViewsById.TryGetValue(childId, out SkillTreeNodeView toNode))
                    continue;

                Edge edge = fromNode.OutputPort.ConnectTo(toNode.InputPort);
                AddElement(edge);
            }
        }

        FrameAll();
    }

    public TalentNodeData CreateNewNodeAsset(Vector2 position)
    {
        if (graph == null)
        {
            EditorUtility.DisplayDialog("Skill Tree Editor", "Select a SkillTreeGraph first.", "OK");
            return null;
        }

        EnsureGraphCollections();

        string folderPath = string.IsNullOrWhiteSpace(graph.newNodeFolder) ? "Assets" : graph.newNodeFolder;
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Skill Tree Editor", $"Folder does not exist:\n{folderPath}", "OK");
            return null;
        }

        TalentNodeData data = ScriptableObject.CreateInstance<TalentNodeData>();
        data.id = System.Guid.NewGuid().ToString("N");
        data.connections = new List<string>();
        data.position = EditorPositionToDataPosition(position);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/TalentNodeData.asset");
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();

        graph.nodes.Add(data);
        CreateNodeView(data);

        EditorUtility.SetDirty(graph);
        EditorUtility.SetDirty(data);
        Selection.activeObject = data;

        return data;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
            endPort != startPort &&
            endPort.node != startPort.node &&
            endPort.direction != startPort.direction).ToList();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        Vector2 mousePosition = evt.localMousePosition;
        evt.menu.AppendAction("Create Talent Node Asset", _ => CreateNewNodeAsset(mousePosition));
        evt.menu.AppendSeparator();
    }

    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        if (graph == null)
            return;

        if (HasSupportedDraggedAssets())
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        if (graph == null || !HasSupportedDraggedAssets())
            return;

        DragAndDrop.AcceptDrag();
        AddTalentNodes(ExtractTalentNodesFromDragAndDrop(), evt.localMousePosition);
    }

    private void CreateNodeView(TalentNodeData data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.id))
            return;

        SkillTreeNodeView nodeView = new SkillTreeNodeView(data);
        Vector2 nodeSize = graph != null ? graph.defaultNodeSize : new Vector2(240f, 180f);
        Vector2 editorPosition = DataPositionToEditorPosition(data.position);
        ApplyNodeViewLayout(nodeView, editorPosition, nodeSize);
        AddElement(nodeView);
        nodeViewsById[data.id] = nodeView;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (graph == null)
            return change;

        if (change.movedElements != null)
        {
            foreach (GraphElement movedElement in change.movedElements)
            {
                SkillTreeNodeView nodeView = movedElement as SkillTreeNodeView;
                if (nodeView == null || nodeView.Data == null)
                    continue;

                Rect currentRect = nodeView.GetPosition();
                Vector2 snappedPosition = SnapToGrid(currentRect.position);
                ApplyNodeViewLayout(nodeView, snappedPosition, currentRect.size);
                nodeView.Data.position = EditorPositionToDataPosition(snappedPosition);
                EditorUtility.SetDirty(nodeView.Data);
            }
        }

        gridBackground.MarkDirtyRepaint();

        if (change.elementsToRemove != null)
        {
            foreach (GraphElement element in change.elementsToRemove)
            {
                if (element is Edge edge)
                    RemoveConnection(edge);

                if (element is SkillTreeNodeView nodeView)
                    RemoveNodeReference(nodeView);
            }
        }

        return change;
    }

    private void RemoveConnection(Edge edge)
    {
        SkillTreeNodeView fromNode = edge.output != null ? edge.output.node as SkillTreeNodeView : null;
        if (fromNode == null)
            return;

        SkillTreeNodeView toNode = edge.input != null ? edge.input.node as SkillTreeNodeView : null;
        if (toNode == null)
            return;

        if (fromNode.Data?.connections == null)
            return;

        fromNode.Data.connections.Remove(toNode.Data.id);
        EditorUtility.SetDirty(fromNode.Data);
    }

    private void RemoveNodeReference(SkillTreeNodeView nodeView)
    {
        if (nodeView.Data == null || graph == null)
            return;

        graph.nodes.Remove(nodeView.Data);
        nodeViewsById.Remove(nodeView.Data.id);

        foreach (TalentNodeData data in graph.nodes.Where(node => node != null && node.connections != null))
        {
            if (data.connections.Remove(nodeView.Data.id))
                EditorUtility.SetDirty(data);
        }

        EditorUtility.SetDirty(graph);
    }

    private void AddTalentNodes(List<TalentNodeData> nodesToAdd, Vector2 startEditorPosition)
    {
        if (graph == null || nodesToAdd == null || nodesToAdd.Count == 0)
            return;

        int addedCount = 0;
        Vector2 baseDataPosition = EditorPositionToDataPosition(startEditorPosition);

        foreach (TalentNodeData nodeData in nodesToAdd)
        {
            if (nodeData == null || graph.nodes.Contains(nodeData))
                continue;

            if (nodeData.connections == null)
                nodeData.connections = new List<string>();

            if (nodeData.position == Vector2.zero)
            {
                Vector2 offset = new Vector2(addedCount * 40f, -addedCount * 40f);
                nodeData.position = SnapToGrid(baseDataPosition + offset);
            }

            graph.nodes.Add(nodeData);
            CreateNodeView(nodeData);
            EditorUtility.SetDirty(nodeData);
            addedCount++;
        }

        if (addedCount > 0)
            EditorUtility.SetDirty(graph);
    }

    private bool HasSupportedDraggedAssets()
    {
        return ExtractTalentNodesFromDragAndDrop().Count > 0;
    }

    private List<TalentNodeData> ExtractTalentNodesFromDragAndDrop()
    {
        List<TalentNodeData> result = new List<TalentNodeData>();
        HashSet<TalentNodeData> unique = new HashSet<TalentNodeData>();

        foreach (Object draggedObject in DragAndDrop.objectReferences)
        {
            AddObjectAsTalentNode(draggedObject, unique, result);
        }

        foreach (string assetPath in DragAndDrop.paths)
        {
            AddPathAsTalentNodes(assetPath, unique, result);
        }

        return result;
    }

    private List<TalentNodeData> ExtractTalentNodesFromObjects(Object[] objects)
    {
        List<TalentNodeData> result = new List<TalentNodeData>();
        HashSet<TalentNodeData> unique = new HashSet<TalentNodeData>();

        foreach (Object obj in objects)
            AddObjectAsTalentNode(obj, unique, result);

        return result;
    }

    private void AddObjectAsTalentNode(Object obj, HashSet<TalentNodeData> unique, List<TalentNodeData> result)
    {
        TalentNodeData nodeData = obj as TalentNodeData;
        if (nodeData != null)
        {
            if (unique.Add(nodeData))
                result.Add(nodeData);
            return;
        }

        string path = AssetDatabase.GetAssetPath(obj);
        if (!string.IsNullOrWhiteSpace(path))
            AddPathAsTalentNodes(path, unique, result);
    }

    private void AddPathAsTalentNodes(string assetPath, HashSet<TalentNodeData> unique, List<TalentNodeData> result)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
            return;

        if (AssetDatabase.IsValidFolder(assetPath))
        {
            string[] guids = AssetDatabase.FindAssets("t:TalentNodeData", new[] { assetPath });
            foreach (string guid in guids)
            {
                string nodePath = AssetDatabase.GUIDToAssetPath(guid);
                TalentNodeData folderNode = AssetDatabase.LoadAssetAtPath<TalentNodeData>(nodePath);
                if (folderNode != null && unique.Add(folderNode))
                    result.Add(folderNode);
            }

            return;
        }

        TalentNodeData assetNode = AssetDatabase.LoadAssetAtPath<TalentNodeData>(assetPath);
        if (assetNode != null && unique.Add(assetNode))
            result.Add(assetNode);
    }
    private void ClearGraph()
    {
        DeleteElements(graphElements.ToList());
        nodeViewsById.Clear();
    }

    private void EnsureGraphCollections()
    {
        if (graph.nodes == null)
            graph.nodes = new List<TalentNodeData>();

        for (int i = graph.nodes.Count - 1; i >= 0; i--)
        {
            if (graph.nodes[i] == null)
                graph.nodes.RemoveAt(i);
        }
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        float gridSize = 25f;
        if (graph != null && graph.gridSnapSize > 0f)
            gridSize = graph.gridSnapSize;

        return new Vector2(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize);
    }

    private Vector2 DataPositionToEditorPosition(Vector2 dataPosition)
    {
        return SnapToGrid(new Vector2(dataPosition.x, -dataPosition.y));
    }

    private Vector2 EditorPositionToDataPosition(Vector2 editorPosition)
    {
        Vector2 snapped = SnapToGrid(editorPosition);
        return new Vector2(snapped.x, -snapped.y);
    }

    private void ApplyNodeViewLayout(SkillTreeNodeView nodeView, Vector2 editorPosition, Vector2 nodeSize)
    {
        if (nodeView == null)
            return;

        nodeView.ApplySize(nodeSize);
        nodeView.SetPortMirror(editorPosition.x < 0f);
        nodeView.SetPosition(new Rect(editorPosition, nodeSize));
    }

    private Vector3 GetViewTranslation()
    {
        Matrix4x4 matrix = viewTransform.matrix;
        return new Vector3(matrix.m03, matrix.m13, matrix.m23);
    }

    private Vector3 GetViewScale()
    {
        Matrix4x4 matrix = viewTransform.matrix;
        return new Vector3(matrix.m00, matrix.m11, matrix.m22);
    }
}
