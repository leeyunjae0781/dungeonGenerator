using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using static DelaunayTriangulation;
using Random = UnityEngine.Random;


public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    public int minRoomWidth = 4, minRoomHeight = 4;
    public int dungeonWidth = 20, dungeonHeight = 20;
    [Range(0, 10)]
    public int offset = 1;
    public bool randomWalkRooms = false;

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    // 실제로 실행되는 함수
    private void CreateRooms()
    {
        // 이진분할알고리즘으로 방이 생길 구역 나누기
        var roomList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int
                (dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight, offset);
        
        // 타일을 칠할 바닥 해시셋
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        // 방의 중심과 그 방에 속한 타일을 묶음 여기에서 선언 CreateRoomsRandomly(roomList, Center_Floor_Set) 에서 계산
        // 길찾기에서 방 안의 랜덤한 타일을 찾을 때, 타일이 속한 방을 구별하기 위해 사용 타일은 하나의 해시셋에 저장되기 때문에 필요
        Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        // 랜덤워크를 할지 말지
        if (randomWalkRooms)
        {
            floor = CreateRoomsRandomly(roomList, Center_Floor_Set);
        }
        else
        {
            floor = CreateSimpleRooms(roomList);
        }

        // 길찾기, 통로연결
        HashSet<Vector2Int> corridors = ConnectRooms(Center_Floor_Set);
        floor.UnionWith(corridors);

        // 타일 칠하기, 벽 칠하기
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);

    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomList, Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomList.Count; i++) 
        {
            // roomList[i]는 이진분할 알고리즘으로 나온 방의 영역
            var roomBounds = roomList[i];

            // 방 중심 계산
            var roomCenter =
                (Vector2Int)Vector3Int.RoundToInt(roomList[i].center);

            var roomFloor = RunRandomWalkWithBound(randomWalkParameters, roomCenter, roomBounds);
            floor.UnionWith(roomFloor);

            Center_Floor_Set[new Vector2Int((int)roomList[i].center.x, (int)roomList[i].center.y)] = roomFloor;

            //foreach (var position in roomFloor)
            //{
            //    if(position.x > (roomBounds.xMin) && position.x < (roomBounds.xMax) 
            //        && position.y > (roomBounds.yMin) && position.y < (roomBounds.yMax))
            //    {
            //        floor.Add(position);
            //    }
            //}


            // 이거하면 방 사이 칸이 offset * 2

            //foreach (var position in roomFloor)
            //{
            //    if (position.x > (roomBounds.xMin) && position.x <= (roomBounds.xMax)
            //        && position.y > (roomBounds.yMin) && position.y <= (roomBounds.yMax))
            //    {
            //        floor.Add(position);
            //    }
            //}
        }

        return floor;
    }

    // 랜덤워크 안 할때
    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomList)
        {
            for (int col = 0; col < room.size.x; col++)
            {
                for (int row = 0; row < room.size.y; row++)
                {
                    Vector2Int position = new Vector2Int(room.min.x + col, room.min.y + row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set)
    {
        List<Vector2Int> roomCenterList = new List<Vector2Int>(Center_Floor_Set.Keys);

        // 방 안 랜덤한 타일을 기준으로 연결
        List<Vector2> vector2Points = new List<Vector2>();
        foreach (var roomCenter in roomCenterList)
        {
            List<Vector2Int> room_Floor = new List<Vector2Int>(Center_Floor_Set[roomCenter]);

            int randomPosIdx = Random.Range(0, room_Floor.Count);
            // Vector2Int에서 Vector2로 변환
            vector2Points.Add(room_Floor[randomPosIdx]);
        }


        // 방 중심을 기준으로 연결
        //List<Vector2> vector2Points = new List<Vector2>();
        //foreach (var roomCenter in roomCenterList)
        //{
        //    // Vector2Int에서 Vector2로 변환
        //    vector2Points.Add(new Vector2(roomCenter.x, roomCenter.y));
        //}

        // 보이어 왓슨 알고리즘으로 들로네삼각형 그리기
        var triagles = DelaunayTriangulation.CreateDelaunay(vector2Points);
        
        List<DelaunayTriangulation.Edge> edges = new List<DelaunayTriangulation.Edge>();

        foreach (var tri in triagles) // 들로네 삼각형 결과로부터
        {
            edges.Add(new DelaunayTriangulation.Edge(tri.a, tri.b));
            edges.Add(new DelaunayTriangulation.Edge(tri.b, tri.c));
            edges.Add(new DelaunayTriangulation.Edge(tri.c, tri.a));
        }

        // 크루스칼 알고리즘으로 간선edge 간에 최단 거리 길찾기
        List<DelaunayTriangulation.Edge> mst = DelaunayTriangulation.GenerateMST(vector2Points, edges);
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        // 간선 잇기
        foreach (var item in mst)
        {
            Debug.Log(item.v0 + " , " + item.v1 + " , " + item.weight);
            Vector2Int Room1 = new Vector2Int((int)item.v0.x, (int)item.v0.y);
            Vector2Int Room2 = new Vector2Int((int)item.v1.x, (int)item.v1.y);
            List<Vector2Int> newCorrior = CreateCorridor(Room1, Room2);

            corridors.UnionWith(newCorrior);
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
                    newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y)); // 코너위치에 3x3칸의 브러쉬로 그리기
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

        int randomvar = 0; // 이동이 끝난 방향은 더 이상 이동하지 않도록 해주는 변수 

        while (position.y != destination.y || position.x != destination.x)
        {
            if (Random.Range(0,10) + randomvar >= 5) // 랜덤으로 좌우 / 상하 방향 정하기
            {
                if (destination.y > position.y)
                {
                    position += Vector2Int.up;
                }
                else if (destination.y < position.y)
                {
                    position += Vector2Int.down;
                }else { randomvar -= 5; } // 이동 끝나면 막기
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
