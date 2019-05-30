using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 Round(this Vector3 vector) => new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
    public static Vector2 Round(this Vector2 vector) => new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));

    public static Vector3 Abs(this Vector3 vector) => new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
	public static Vector2 Abs(this Vector2 vector) => new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));

	public static Vector3 Max(Vector3 vector1, Vector3 vector2) => new Vector3(Mathf.Max(vector1.x, vector2.x), Mathf.Max(vector1.y, vector2.y), Mathf.Max(vector1.z, vector2.z));
	public static Vector3 Min(Vector3 vector1, Vector3 vector2) => new Vector3(Mathf.Min(vector1.x, vector2.x), Mathf.Min(vector1.y, vector2.y), Mathf.Min(vector1.z, vector2.z));
    
    public static Vector3 ToVector3(this Vector2 vector) => new Vector3(vector.x, vector.y, 0);
	public static Vector2 ToVector2(this Vector3 vector) => new Vector2(vector.x, vector.y);

	public static Rect RectFromCenter(Vector2 center, Vector2 size) => new Rect(center - size / 2, size);
	public static Vector2 ClampInRect(Vector2 vector, Rect rect) => new Vector2(Mathf.Clamp(vector.x, rect.xMin, rect.xMax), Mathf.Clamp(vector.y, rect.yMin, rect.yMax));

	public static Vector2 ViewportSize(this Camera camera) => camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f)) - camera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));


}