using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class LevelEditor : MonoBehaviour
{
    public GameObject Circuit;
    private Vector3 _previewPosition;
    private bool _canPlace = false;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane groundPlane = new(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 mousePos = ray.GetPoint(enter);
            _previewPosition = SnapToGrid(mousePos);
            _previewPosition.y = 0.5f;

            Vector3 halfExtents = new(0.5f, 0.5f, 0.5f);
            Collider[] hits = Physics.OverlapBox(_previewPosition, halfExtents);
            _canPlace = true;
            foreach (Collider hit in hits)
            {
                if (hit.gameObject.name.Contains("Circuit"))
                {
                    _canPlace = false;
                    break;
                }
            }

            Handles.color = _canPlace ? Color.green : Color.red;
            Handles.DrawWireCube(_previewPosition, new Vector3(10, 2, 10));

            if (e.type == EventType.MouseDown && e.button == 0 && _canPlace)
            {
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(Circuit);
                Undo.RegisterCreatedObjectUndo(newObj, "Place Object");
                newObj.transform.position = _previewPosition;
                e.Use();
            }
        }

        sceneView.Repaint();
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        float gridSize = 5f;
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.z = Mathf.Round(position.z / gridSize) * gridSize;
        return position;
    }
}
