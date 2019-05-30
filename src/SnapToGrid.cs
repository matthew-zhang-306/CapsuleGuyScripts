using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    // Code copied from original work
    public void Snap() {
        // Get opposite corners of the wall and round them to the nearest lattice point
		Vector3 topLeft = transform.localPosition - transform.localScale / 2;
		Vector3 bottomRight = transform.localPosition + transform.localScale / 2;
        topLeft = (topLeft / 0.5f).Round() * 0.5f;
        bottomRight = (bottomRight / 0.5f).Round() * 0.5f;
		Vector3 tempScale = (topLeft - bottomRight).Abs();

		// Move the wall into the desired position
		transform.localPosition = (topLeft + bottomRight) / 2;
		transform.localScale = new Vector3(tempScale.x, tempScale.y, 1);
    }
}
