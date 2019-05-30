using UnityEngine;
using UnityEditor;

// One of the handiest scripts I've ever written. Allows objects to snap to the grid with the press of a button.
[CustomEditor(typeof(SnapToGrid))]
class SnapToGridEditor : Editor {
    SnapToGrid ground;

    void Snap() {
        ground.Snap();

        // For some reason this line doesn't work and I never figured out why
        Undo.RecordObject(ground.gameObject, "Snap");
    }
    
    public override void OnInspectorGUI() {
        ground = target as SnapToGrid;
        if (GUILayout.Button("Snap")) {
            Snap();
        }
    }

    public void OnSceneGUI() {
        if (ground == null) return;

        Event e = Event.current;
        if (e.type != EventType.KeyDown) return;
        if (e.keyCode != KeyCode.S) return;
        
        Snap();
    }
}