using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : AbstractDungeonGenerator
{
    public int minRoomWidth = 4, minRoomHeight = 4;
    public int dungeonWidth = 20, dungeonHeight = 20;
    [Range(0, 10)]
    public int offset = 1;
    public bool randomWalkRooms = false;

    [SerializeField]
    protected SimpleRandomWalkSO randomWalkParameters;
    



    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }



    // 방을 생성하는 함수
    private void CreateRooms()
    {
        // 바이너리 스페이스 파티셔닝으로 방의 위치를 생성
        var roomList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int
                (dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight, offset);
        
        // 타일맵에 그릴 타일 위치
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        // 각 방의 중심과 그 방에 속한 타일들을 저장할 딕셔너리
        // 나중에 CreateRoomsRandomly(roomList, Center_Floor_Set) 함수에서 사용
        // 중심점에서 가장 가까운 타일을 찾고, 타일을 저장하기 위한 용도로 사용
        Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        // 방 생성 방식 선택
        if (randomWalkRooms)
        {
            floor = CreateRoomsRandomly(roomList, Center_Floor_Set);
        }
        else
        {
            floor = CreateSimpleRooms(roomList, Center_Floor_Set);
        }
        // 복도 생성
        HashSet<Vector2Int> corridors = ConnectRooms(Center_Floor_Set);
        floor.UnionWith(corridors);

        // 타일 그리기, 벽 그리기
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomList, Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomList.Count; i++) 
        {
            // roomList[i]는 바이너리 스페이스 파티셔닝으로 생성된 방의 위치
            var roomBounds = roomList[i];

            // 방의 중심점
            var roomCenter = (Vector2Int)Vector3Int.RoundToInt(roomList[i].center);

            var roomFloor = RunRandomWalkWithBound(randomWalkParameters, roomCenter, roomBounds);
            floor.UnionWith(roomFloor);

            Center_Floor_Set[new Vector2Int((int)roomList[i].center.x, (int)roomList[i].center.y)] = roomFloor;
        }
        return floor;
    }

    protected HashSet<Vector2Int> RunRandomWalkWithBound(SimpleRandomWalkSO parameters, Vector2Int position, BoundsInt roomBounds)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < parameters.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalkWithBounds(currentPosition, parameters.walkLength, roomBounds);
            floorPositions.UnionWith(path);

            if (parameters.startRandomlyEachIteration)
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
        }
        return floorPositions;
    }

    // 간단한 방 생성
    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomList, Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomList)
        {
            HashSet<Vector2Int> roomfloor = new HashSet<Vector2Int>();
            for (int col = 0; col < room.size.x; col++)
            {
                for (int row = 0; row < room.size.y; row++)
                {
                    Vector2Int position = new Vector2Int(room.min.x + col, room.min.y + row);
                    floor.Add(position);
                    roomfloor.Add(position);
                }
            }
            Center_Floor_Set[new Vector2Int((int)room.center.x, (int)room.center.y)] = roomfloor;
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set)
    {
        List<Vector2Int> roomCenterList = new List<Vector2Int>(Center_Floor_Set.Keys);

        // 각 방의 중심 타일을 랜덤하게 선택
        List<Vector2> vector2Points = new List<Vector2>();
        foreach (var roomCenter in roomCenterList)
        {
            List<Vector2Int> room_Floor = new List<Vector2Int>(Center_Floor_Set[roomCenter]);
            int randomPosIdx = Random.Range(0, room_Floor.Count);
            // Vector2Int에서 Vector2로 변환
            vector2Points.Add(room_Floor[randomPosIdx]);
        }

        // 들로네 삼각분할로 연결
        var triagles = DelaunayTriangulation.CreateDelaunay(vector2Points);
        
        List<DelaunayTriangulation.Edge> edges = new List<DelaunayTriangulation.Edge>();

        foreach (var tri in triagles) // 삼각형에서 변들을 추출
        {
            edges.Add(new DelaunayTriangulation.Edge(tri.a, tri.b));
            edges.Add(new DelaunayTriangulation.Edge(tri.b, tri.c));
            edges.Add(new DelaunayTriangulation.Edge(tri.c, tri.a));
        }

        // 최소신장트리 알고리즘으로 최적의 edge 선택
        List<DelaunayTriangulation.Edge> mst = DelaunayTriangulation.GenerateMST(vector2Points, edges);
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        // 복도 생성
        foreach (var item in mst)
        {
            Vector2Int Room1 = new Vector2Int((int)item.v0.x, (int)item.v0.y);
            Vector2Int Room2 = new Vector2Int((int)item.v1.x, (int)item.v1.y);
            List<Vector2Int> newCorrior = CreateCorridor(Room1, Room2);
            List<Vector2Int> new3x3Corrior = IncreaseCorridorBrush3by3(newCorrior);
            corridors.UnionWith(new3x3Corrior);
        }
        return corridors;
    }

    private List<Vector2Int> IncreaseCorridorBrush3by3(List<Vector2Int> corridor)
    {
        List<Vector2Int> newCorridor = new List<Vector2Int>();

        for (int i = 1; i < corridor.Count; i++)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y)); // 현재위치에서 3x3칸을 채우도록 설정
                }
            }
        }
        return newCorridor;
    }

    private List<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);

        int randomvar = 0; // 이동할 수 없는 방향이 있을 때 더 이상 이동하지 못하게 하는 변수

        while (position.y != destination.y || position.x != destination.x)
        {
            if (Random.Range(0,10) + randomvar >= 5) // 수직/수평 이동 결정
            {
                if (destination.y > position.y)
                {
                    position += Vector2Int.up;
                }
                else if (destination.y < position.y)
                {
                    position += Vector2Int.down;
                }
                else { randomvar -= 5; } // 이동 불가능한 방향
            }
            else
            {
                if (destination.x > position.x)
                {
                    position += Vector2Int.right;
                }
                else if (destination.x < position.x)
                {
                    position += Vector2Int.left;
                }
                else { randomvar += 5; }
            }
            corridor.Add(position);
        }
        return corridor;
    }
}
