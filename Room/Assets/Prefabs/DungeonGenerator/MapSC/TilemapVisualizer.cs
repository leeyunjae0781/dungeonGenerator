using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTop;
    [SerializeField] private TileBase emptyTile;


    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        foreach (var position in floorPositions)
        {
            PaintSingleTile(position, true, false);
        }   
        foreach (var position in floorPositions)
        {
            PaintSingleTile(position, false, true);
        } 
    }

    public void PaintSingleBasicWall(Vector2Int position)
    {
        PaintSingleTile(position, false, false);
    }

    private void PaintSingleTile(Vector2Int position, bool isFloor, bool isemptyTile)
    {
        var tilePosition = new Vector3Int(position.x, position.y, 0);
        var tilemap = isFloor ? floorTilemap : wallTilemap;

        TileBase tile ;
        if (isemptyTile == true) {tile = emptyTile;}
        else {tile = isFloor ? floorTile : wallTop;}
        tilemap.SetTile(tilePosition, tile);
    }


    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();


    }
}