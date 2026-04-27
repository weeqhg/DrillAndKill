using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillTreeEditorWindow : EditorWindow
{
    [SerializeField] private SkillTreeGraph graph;

    private SkillTreeGraphView graphView;
    private ObjectField graphField;

    [MenuItem("Tools/Skill Tree Editor")]
    public static void Open()
    {
        SkillTreeEditorWindow window = GetWindow<SkillTreeEditorWindow>();
        window.titleContent = new GUIContent("Skill Tree");
        window.minSize = new Vector2(900f, 600f);
    }

    private void OnEnable()
    {
        CreateLayout();
        LoadCurrentGraph();
    }

    private void CreateLayout()
    {
        rootVisualElement.Clear();

        Toolbar toolbar = new Toolbar();

        graphField = new ObjectField("Graph")
        {
            objectType = typeof(SkillTreeGraph),
            allowSceneObjects = false,
            value = graph
        };
        graphField.style.minWidth = 260f;
        graphField.RegisterValueChangedCallback(evt =>
        {
            graph = evt.newValue as SkillTreeGraph;
            LoadCurrentGraph();
        });

        Button saveButton = new Button(SaveGraph) { text = "Save" };
        Label dropHint = new Label("Drag TalentNodeData or a folder from Project into the graph");
        dropHint.style.unityFontStyleAndWeight = FontStyle.Italic;
        dropHint.style.marginLeft = 8f;
        dropHint.style.color = new Color(0.75f, 0.75f, 0.75f, 1f);

        toolbar.Add(graphField);
        toolbar.Add(saveButton);
        toolbar.Add(dropHint);

        graphView = new SkillTreeGraphView();
        graphView.style.flexGrow = 1f;

        rootVisualElement.Add(toolbar);
        rootVisualElement.Add(graphView);
    }

    private void LoadCurrentGraph()
    {
        if (graphView == null)
            return;

        graphView.SetGraph(graph);
        Repaint();
    }

    private void SaveGraph()
    {
        SkillTreeEditorSaveUtility.Save(graph, graphView);
    }

    private void OnDisable()
    {
        if (graphView != null)
            rootVisualElement.Remove(graphView);
    }
}
