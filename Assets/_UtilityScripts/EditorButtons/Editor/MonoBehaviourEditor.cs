using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
public class MonoBehaviourEditor : Editor
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