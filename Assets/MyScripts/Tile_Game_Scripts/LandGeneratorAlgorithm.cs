using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class LandGeneratorAlgorithm
{
    public class RandomWalkParams
    {
        public int iterations = 10, walkLength = 10;
        public bool startRandomlyEachIteration = true;
    }

    public static HashSet<Vector2Int> GenerateLandCoordinates(int mapSize, int islandSize)
    {
        bool randomWalkRooms = true;
        Vector2Int startPosition = new(0, 0);

        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int(mapSize, mapSize, 0)), islandSize, islandSize);

        HashSet<Vector2Int> LandCoordinates = new HashSet<Vector2Int>();

        if (randomWalkRooms)
        {
            LandCoordinates = CreateRoomsRandomly(roomsList);
        }
        else
        {
            LandCoordinates = CreateSimpleRooms(roomsList);
        }


        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        //HIDING CORRIDORSSS 

        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        List<List<Vector2Int>> listOfcorridors = ConnectRooms(roomCenters);

        listOfcorridors.ForEach(corridor =>
        {
            if (corridor.Count < 20)
            {
                ////////////////////////////
                var walkParams = new RandomWalkParams();
                walkParams.walkLength = corridor.Count;
                corridors.UnionWith(RunRandomWalk(walkParams, corridor[0]));
                corridors.UnionWith(RunRandomWalk(walkParams, corridor[corridor.Count / 2]));
                corridors.UnionWith(RunRandomWalk(walkParams, corridor[corridor.Count - 1]));
                //////////////////////////////
            }
            //corridors.UnionWith(corridor);
        });


        LandCoordinates.UnionWith(corridors);

        return LandCoordinates;
        //tilemapVisualizer.PaintFloorTiles(floor);
        // WallGenerator.CreateWalls(floor, tilemapVisualizer);
    }

    private static HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        int offset = 1;

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var randomWalkParameters = new RandomWalkParams
            {
                iterations = 50,
                walkLength = 15
            };
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);

            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) && position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private static List<List<Vector2Int>> ConnectRooms(List<Vector2Int> roomCenters)
    {
        List<List<Vector2Int>> listOfcorridors = new List<List<Vector2Int>>();

        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            List<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            listOfcorridors.Add(newCorridor);
        }
        return listOfcorridors;
    }

    //private List<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    //{
    //    List<Vector2Int> corridor = new List<Vector2Int>();
    //    var position = currentRoomCenter;
    //    corridor.Add(position);
    //    while (position.y != destination.y)
    //    {
    //        if (destination.y > position.y)
    //        {
    //            position += Vector2Int.up;
    //        }
    //        else if (destination.y < position.y)
    //        {
    //            position += Vector2Int.down;
    //        }
    //        corridor.Add(position);
    //    }
    //    while (position.x != destination.x)
    //    {
    //        if (destination.x > position.x)
    //        {
    //            position += Vector2Int.right;
    //        }
    //        else if (destination.x < position.x)
    //        {
    //            position += Vector2Int.left;
    //        }
    //        corridor.Add(position);
    //    }
    //    return corridor;
    //}

    private static List<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        List<Vector2Int> corridor = new List<Vector2Int> { currentRoomCenter };
        var position = currentRoomCenter;

        while (position.y != destination.y)
        {
            position += (destination.y > position.y) ? Vector2Int.up : Vector2Int.down;
            corridor.Add(position);
        }

        while (position.x != destination.x)
        {
            position += (destination.x > position.x) ? Vector2Int.right : Vector2Int.left;
            corridor.Add(position);
        }

        return corridor;
    }

    private static Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private static HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        int offset = 1;

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private static HashSet<Vector2Int> RunRandomWalk(RandomWalkParams parameters, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < parameters.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength);
            floorPositions.UnionWith(path);
            if (parameters.startRandomlyEachIteration)
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
        }
        return floorPositions;
    }

}
