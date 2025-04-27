using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    public Tilemap floorTilemap, wallTilemap;
    public TileBase floorTile, wallTop;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions){
        PaintTiles(floorPositions, floorTilemap, floorTile) ;
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap Tilemap, TileBase tile) {
        foreach (var position in positions){
            PaintSingleTiles(Tilemap, tile, position) ;
        }
    }

    private void PaintSingleTiles(Tilemap tilemap, TileBase tile, Vector2Int position) {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition,tile);
    }

    public void Clear(){
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    internal void PaintSingleBasicWall(Vector2Int position)
    {
        PaintSingleTiles(wallTilemap,wallTop,position);
    }
}
