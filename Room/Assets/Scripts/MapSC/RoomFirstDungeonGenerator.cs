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

    // ������ ����Ǵ� �Լ�
    private void CreateRooms()
    {
        // �������Ҿ˰������� ���� ���� ���� ������
        var roomList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int
                (dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight, offset);
        
        // Ÿ���� ĥ�� �ٴ� �ؽü�
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        // ���� �߽ɰ� �� �濡 ���� Ÿ���� ���� ���⿡�� ���� CreateRoomsRandomly(roomList, Center_Floor_Set) ���� ���
        // ��ã�⿡�� �� ���� ������ Ÿ���� ã�� ��, Ÿ���� ���� ���� �����ϱ� ���� ��� Ÿ���� �ϳ��� �ؽü¿� ����Ǳ� ������ �ʿ�
        Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        // ������ũ�� ���� ����
        if (randomWalkRooms)
        {
            floor = CreateRoomsRandomly(roomList, Center_Floor_Set);
        }
        else
        {
            floor = CreateSimpleRooms(roomList);
        }

        // ��ã��, ��ο���
        HashSet<Vector2Int> corridors = ConnectRooms(Center_Floor_Set);
        floor.UnionWith(corridors);

        // Ÿ�� ĥ�ϱ�, �� ĥ�ϱ�
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);

    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomList, Dictionary<Vector2Int, HashSet<Vector2Int>> Center_Floor_Set)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomList.Count; i++) 
        {
            // roomList[i]�� �������� �˰������� ���� ���� ����
            var roomBounds = roomList[i];

            // �� �߽� ���
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


            // �̰��ϸ� �� ���� ĭ�� offset * 2

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

    // ������ũ �� �Ҷ�
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

        // �� �� ������ Ÿ���� �������� ����
        List<Vector2> vector2Points = new List<Vector2>();
        foreach (var roomCenter in roomCenterList)
        {
            List<Vector2Int> room_Floor = new List<Vector2Int>(Center_Floor_Set[roomCenter]);

            int randomPosIdx = Random.Range(0, room_Floor.Count);
            // Vector2Int���� Vector2�� ��ȯ
            vector2Points.Add(room_Floor[randomPosIdx]);
        }


        // �� �߽��� �������� ����
        //List<Vector2> vector2Points = new List<Vector2>();
        //foreach (var roomCenter in roomCenterList)
        //{
        //    // Vector2Int���� Vector2�� ��ȯ
        //    vector2Points.Add(new Vector2(roomCenter.x, roomCenter.y));
        //}

        // ���̾� �ӽ� �˰������� ��γ׻ﰢ�� �׸���
        var triagles = DelaunayTriangulation.CreateDelaunay(vector2Points);
        
        List<DelaunayTriangulation.Edge> edges = new List<DelaunayTriangulation.Edge>();

        foreach (var tri in triagles) // ��γ� �ﰢ�� ����κ���
        {
            edges.Add(new DelaunayTriangulation.Edge(tri.a, tri.b));
            edges.Add(new DelaunayTriangulation.Edge(tri.b, tri.c));
            edges.Add(new DelaunayTriangulation.Edge(tri.c, tri.a));
        }

        // ũ�罺Į �˰������� ����edge ���� �ִ� �Ÿ� ��ã��
        List<DelaunayTriangulation.Edge> mst = DelaunayTriangulation.GenerateMST(vector2Points, edges);
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        // ���� �ձ�
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
                    newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y)); // �ڳ���ġ�� 3x3ĭ�� �귯���� �׸���
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

        int randomvar = 0; // �̵��� ���� ������ �� �̻� �̵����� �ʵ��� ���ִ� ���� 

        while (position.y != destination.y || position.x != destination.x)
        {
            if (Random.Range(0,10) + randomvar >= 5) // �������� �¿� / ���� ���� ���ϱ�
            {
                if (destination.y > position.y)
                {
                    position += Vector2Int.up;
                }
                else if (destination.y < position.y)
                {
                    position += Vector2Int.down;
                }else { randomvar -= 5; } // �̵� ������ ����
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
