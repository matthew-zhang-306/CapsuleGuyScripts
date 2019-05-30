using UnityEngine;

// Thanks Sebastian Lague
public class PointLight : MonoBehaviour
{
    public float viewAngle;
    public float halfViewAngle { get { return viewAngle / 2; }}
    public float maxRadius;

    public LayerMask wallMask;

    public float meshResolution;
    MeshFilter meshFilter;
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    private void Start() {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        mesh.name = "Light Mesh";
        meshFilter.mesh = mesh;

        MeshRenderer rend = GetComponent<MeshRenderer>();
        rend.sortingLayerName = "Lighting";
    }

    private void LateUpdate() {
        DrawLightMesh();
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.z;
        return Quaternion.Euler(0, 0, angleInDegrees) * Vector3.right;
    }

    private void DrawLightMesh() {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        if (vertices == null) {
            vertices = new Vector3[stepCount + 2];
            vertices[0] = Vector3.zero;
        }

        for (int i = 0; i <= stepCount; i++) {
            float angle = transform.eulerAngles.z - halfViewAngle + stepAngleSize * i;
            LightCastInfo lightcast = LightCast(angle);
            vertices[i+1] = transform.InverseTransformPoint(lightcast.point);
        }

        if (triangles == null) {
            triangles = new int[(vertices.Length - 2) * 3];
            for (int i = 0; i < vertices.Length - 1; i++) {
                if (i < vertices.Length - 2) {
                    triangles[i*3 + 0] = 0;
                    triangles[i*3 + 1] = i + 1;
                    triangles[i*3 + 2] = i + 2;
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    LightCastInfo LightCast(float globalAngle) {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, maxRadius, wallMask);

        if (hit.collider != null) {
            return new LightCastInfo(true, hit.point, hit.distance, globalAngle);
        } else {
            return new LightCastInfo(false, transform.position + dir * maxRadius, maxRadius, globalAngle);
        }
    }

    public struct LightCastInfo {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public LightCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }

        public override string ToString() {
            return "LightCastInfo[" + hit + ", " + point + ", " + dst + ", " + angle + "]";
        }
    }    
}
