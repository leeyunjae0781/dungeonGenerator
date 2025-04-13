using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGenerationAlgorithms 
{
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startposition, int walkLength) {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startposition);
        var previousPosition = startposition;

        for (int i = 0 ; i < walkLength ; i++) {
            var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection();
            path.Add(newPosition);
            previousPosition = newPosition;
        }

        return path;

    }

    public static List<Vector2Int> RandomWalkcorridor(Vector2Int startposition, int corridorLength){
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection();
        var currentPosition = startposition;
        corridor.Add(currentPosition);

        for (int i = 0 ; i < corridorLength; i++) {
            currentPosition += direction;
            corridor.Add(currentPosition);
        }

        return corridor;

    }

    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight, int offset)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);

        while (roomsQueue.Count > 0) 
        {
            var room = roomsQueue.Dequeue();
            if (room.size.y >= minHeight && room.size.x >= minWidth) 
            {
                if(Random.value < 0.5f)
                {
                    // 수평으로 나누기
                    if(room.size.y >= minHeight * 2f)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room, offset);
                    }
                    else if (room.size.x >= minWidth * 2f)
                    {
                        SplitVertically(minWidth, roomsQueue, room, offset);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomList.Add(room);
                    }

                }
                else
                {
                    // 수직으로 나누기
                    if (room.size.x >= minWidth * 2f)
                    {
                        SplitVertically(minWidth, roomsQueue, room, offset);
                    }
                    else if (room.size.y >= minHeight * 2f)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room, offset);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomList.Add(room);
                    }
                }
            }
        }
        return roomList;
    }

    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room, int offset)
    {
        var xSplit = Random.Range(minWidth, room.size.x- minWidth);

        BoundsInt room1 = new BoundsInt(
            new Vector3Int(room.min.x + offset , room.min.y, room.min.z), 
            new Vector3Int(xSplit - offset * 2, room.size.y, room.size.z)
            );

        BoundsInt room2 = new BoundsInt(
            new Vector3Int(room.min.x + xSplit + offset, room.min.y, room.min.z), 
            new Vector3Int(room.size.x - xSplit - offset * 2, room.size.y, room.size.z)
            );

        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room, int offset)
    {
        var ySplit = Random.Range(minHeight, room.size.y- minHeight);

        BoundsInt room1 = new BoundsInt(
            new Vector3Int(room.min.x, room.min.y + offset, room.min.z), 
            new Vector3Int(room.size.x, ySplit - offset * 2, room.size.z)
            );

        BoundsInt room2 = new BoundsInt(
            new Vector3Int(room.min.x, room.min.y + ySplit + offset, room.min.z),
            new Vector3Int(room.size.x, room.size.y - ySplit - offset * 2, room.size.z)
            );

        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

}


public static class Direction2D{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int> {
        new Vector2Int(0,1), // Up
        new Vector2Int(1,0), // Right
        new Vector2Int(0,-1), // Down
        new Vector2Int(-1,0) // Left
    };

    public static Vector2Int GetRandomCardinalDirection(){
        return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)];
    }
}
