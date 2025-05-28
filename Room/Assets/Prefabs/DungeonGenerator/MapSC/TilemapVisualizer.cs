using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTop;


    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        foreach (var position in floorPositions)
        {
            PaintSingleTile(position, true);
        }
    }

    public void PaintSingleBasicWall(Vector2Int position)
    {
        PaintSingleTile(position, false);
    }

    private void PaintSingleTile(Vector2Int position, bool isFloor)
    {
        var tilePosition = new Vector3Int(position.x, position.y, 0);
        var tilemap = isFloor ? floorTilemap : wallTilemap;
        var tile = isFloor ? floorTile : wallTop;
        tilemap.SetTile(tilePosition, tile);
    }


    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();


    }
}