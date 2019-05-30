using System.Collections.Generic;
using UnityEngine;

public class BackgroundLayer : MonoBehaviour
{
    GameObject cam;
    Vector3 cameraSize;
    Vector3 prevCamPos;

    public SpriteRenderer bgTile;
    GameObject[,] bgGrid;

    int numXTiles { get { return bgGrid.GetLength(1); }}
    int numYTiles { get { return bgGrid.GetLength(0); }}

    int leftIndex;
    int rightIndex;
    int bottomIndex;
    int topIndex;

    public Vector2 parallaxMultiplier;
    public bool tileVertically;

    private void Start() {
        cam = GameManager.Instance?.Cam ?? Camera.main.gameObject;
        if (cam == null) {
            Debug.LogWarning("No camera found for background layer " + this + "!");
            return;
        }
        
        prevCamPos = cam.transform.position;
        cameraSize = cam.GetComponent<Camera>().ViewportSize();

        // Create tiles
        bgGrid = new GameObject[tileVertically ? 3 : 1, 3];
        for (int y = 0; y < numYTiles; y++) {
            for (int x = 0; x < numXTiles; x++) {
                if (y == numYTiles / 2 && x == numXTiles / 2) {
                    bgGrid[y, x] = bgTile.gameObject;
                }
                else {
                    Vector2 offsetPos = new Vector2(bgTile.bounds.size.x * (x - numXTiles / 2), bgTile.bounds.size.y * (y - numYTiles / 2));
                    bgGrid[y, x] = GameObject.Instantiate(bgTile.gameObject, transform.position + offsetPos.ToVector3(), Quaternion.identity, transform);
                }
            }
        }

        leftIndex = 0;
        rightIndex = numXTiles - 1;
        bottomIndex = 0;
        topIndex = numYTiles - 1;
    }

    private void FixedUpdate() {
        if (cam == null) return;

        // Parallax motion
        Vector3 currCamPos = cam.transform.position;
        Vector3 deltaPos = currCamPos - prevCamPos;
        Vector3 parallaxDeltaPos = new Vector3(deltaPos.x * parallaxMultiplier.x, deltaPos.y * parallaxMultiplier.y, 0);
        transform.position += parallaxDeltaPos;

        prevCamPos = currCamPos;

        // Scrolling motion
        Rect cameraRect = VectorExtensions.RectFromCenter(currCamPos, cameraSize);
        while (cameraRect.xMax < GetCurrentCenter().transform.position.x - bgTile.bounds.extents.x) {
            // Tile to the left
            for (int y = 0; y < numYTiles; y++) {
                bgGrid[y, rightIndex].transform.position -= Vector3.right * (bgTile.bounds.size.x * numXTiles);
            }
            leftIndex = rightIndex;

            rightIndex--;
            if (rightIndex < 0)
                rightIndex += numXTiles;
        }
        while (cameraRect.xMin > GetCurrentCenter().transform.position.x + bgTile.bounds.extents.x) {
            // Tile to the right
            for (int y = 0; y < numYTiles; y++) {
                bgGrid[y, leftIndex].transform.position += Vector3.right * (bgTile.bounds.size.x * numXTiles);
            }
            rightIndex = leftIndex;

            leftIndex++;
            if (leftIndex >= numXTiles)
                leftIndex -= numXTiles;
        }
        if (tileVertically) {
            while (cameraRect.yMax < GetCurrentCenter().transform.position.y - bgTile.bounds.extents.y) {
                // Tile downard
                for (int x = 0; x < numXTiles; x++) {
                    bgGrid[topIndex, x].transform.position -= Vector3.up * (bgTile.bounds.size.y * numYTiles);
                }
                bottomIndex = topIndex;

                topIndex--;
                if (topIndex < 0)
                    topIndex += numYTiles;
            }
            while (cameraRect.yMin > GetCurrentCenter().transform.position.y + bgTile.bounds.extents.y) {
                // Tile upward
                for (int x = 0; x < numXTiles; x++) {
                    bgGrid[bottomIndex, x].transform.position += Vector3.up * (bgTile.bounds.size.y * numYTiles);
                }
                topIndex = bottomIndex;

                bottomIndex++;
                if (bottomIndex >= numYTiles)
                    bottomIndex -= numYTiles;
            }
        }
    }


    GameObject GetCurrentCenter() {
        int xIndex = (leftIndex + numXTiles / 2) % numXTiles;
        int yIndex = (bottomIndex + numYTiles / 2) % numYTiles;

        return bgGrid[yIndex, xIndex];
    }

    public List<SpriteRenderer> GetAllSprites() {
        List<SpriteRenderer> srs = new List<SpriteRenderer>();
        for (int y = 0; y < numYTiles; y++) {
            for (int x = 0; x < numXTiles; x++) {
                srs.Add(bgGrid[y, x].GetComponent<SpriteRenderer>());
            }
        }
        return srs;
    }


}
