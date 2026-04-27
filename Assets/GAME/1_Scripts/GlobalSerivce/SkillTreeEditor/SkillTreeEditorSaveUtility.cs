using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class SkillTreeEditorSaveUtility
{
    public static void Save(SkillTreeGraph graph, SkillTreeGraphView view)
    {
        if (graph == null || view == null)
        {
            Debug.LogWarning("[SkillTreeEditor] Save skipped because graph or view is null.");
            return;
        }

        if (graph.nodes == null)
            graph.nodes = new List<TalentNodeData>();

        foreach (TalentNodeData data in graph.nodes.Where(node => node != null))
        {
            if (data.connections == null)
                data.connections = new List<string>();
            data.connections.Clear();
        }

        foreach (GraphElement element in view.graphElements)
        {
            Edge edge = element as Edge;
            if (edge == null)
                continue;

            SkillTreeNodeView fromNode = edge.output != null ? edge.output.node as SkillTreeNodeView : null;
            if (fromNode == null)
                continue;

            SkillTreeNodeView toNode = edge.input != null ? edge.input.node as SkillTreeNodeView : null;
            if (toNode == null)
                continue;

            if (fromNode.Data == null || toNode.Data == null)
                continue;

            fromNode.Data.position = EditorPositionToDataPosition(view, fromNode.GetPosition().position);
            toNode.Data.position = EditorPositionToDataPosition(view, toNode.GetPosition().position);
            if (fromNode.Data.connections == null)
                fromNode.Data.connections = new List<string>();

            if (!fromNode.Data.connections.Contains(toNode.Data.id))
                fromNode.Data.connections.Add(toNode.Data.id);
        }

        foreach (Node node in view.nodes)
        {
            SkillTreeNodeView nodeView = node as SkillTreeNodeView;
            if (nodeView == null || nodeView.Data == null)
                continue;

            nodeView.Data.position = EditorPositionToDataPosition(view, nodeView.GetPosition().position);
            nodeView.RefreshFromData();
            EditorUtility.SetDirty(nodeView.Data);
        }

        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void Load(SkillTreeGraph graph, SkillTreeGraphView view)
    {
        if (graph == null || view == null)
        {
            Debug.LogWarning("[SkillTreeEditor] Load skipped because graph or view is null.");
            return;
        }

        view.SetGraph(graph);
    }

    private static Vector2 EditorPositionToDataPosition(SkillTreeGraphView view, Vector2 editorPosition)
    {
        float gridSize = 25f;
        if (view != null && view.Graph != null && view.Graph.gridSnapSize > 0f)
            gridSize = view.Graph.gridSnapSize;

        Vector2 snapped = new Vector2(
            Mathf.Round(editorPosition.x / gridSize) * gridSize,
            Mathf.Round(editorPosition.y / gridSize) * gridSize);

        return new Vector2(snapped.x, -snapped.y);
    }
}
