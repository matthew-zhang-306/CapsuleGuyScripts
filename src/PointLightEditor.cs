using UnityEngine;
using UnityEditor;

// Thanks Sebastian Lague!
[CustomEditor(typeof(PointLight))]
public class PointLightEditor : Editor
{
    private void OnSceneGUI() {
        PointLight pointLight = target as PointLight;

        Handles.color = Color.white;
        Handles.DrawWireArc(pointLight.transform.position, Vector3.forward, Vector3.right, 360, pointLight.maxRadius);

        Vector3 viewAngleA = pointLight.DirFromAngle(-pointLight.halfViewAngle, false);
        Vector3 viewAngleB = pointLight.DirFromAngle(+pointLight.halfViewAngle, false);

        Handles.DrawLine(pointLight.transform.position, pointLight.transform.position + viewAngleA * pointLight.maxRadius);
        Handles.DrawLine(pointLight.transform.position, pointLight.transform.position + viewAngleB * pointLight.maxRadius);
    }    
}
