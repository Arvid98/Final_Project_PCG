using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableObject), true), CanEditMultipleObjects]
public class ScriptableObjectEditor : Editor
{

    private MakeButtonManager buttonManager;

    void OnEnable()
    {
        buttonManager = new MakeButtonManager(target);
    }

    public override void OnInspectorGUI()
    {
        buttonManager.Draw(targets);
        base.OnInspectorGUI();
    }
}
