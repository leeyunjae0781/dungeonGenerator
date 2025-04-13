using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    public TilemapVisualizer tilemapVisualizer;
    public Vector2Int startPosition = Vector2Int.zero;

    public void GenerateDungeon(){
        tilemapVisualizer.Clear();
        RunProceduralGeneration();
    }

    public void Clear()
    {
        tilemapVisualizer.Clear();
    }

    protected abstract void RunProceduralGeneration();
}
