using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Goblin_Script;
using Random = UnityEngine.Random;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;
    [SerializeField]
    private TileBase floorTile, wallTop;
    [SerializeField]
    private Grid grid;

    List<Vector2Int> FreePositionsToBuildOn = new();




    public GameObject HexTileRef;


    public GameObject WallRef;
    public GameObject BarrelRef;
    public GameObject KnightRef;
    public GameObject GoblinRef;
    public GameObject ArcherRef;
    public GameObject ArmoredGoblinRef;

    public int NUMBEROFPLAYERS = 4;




    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        List<Vector2Int> grassPositions = floorPositions.ToList();

        int extraWater = 10;

        int maxX = grassPositions.Max(position => position.x) + extraWater;
        int maxY = grassPositions.Max(position => position.y) + extraWater;
        int minX = grassPositions.Min(position => position.x) - extraWater;
        int minY = grassPositions.Min(position => position.y) - extraWater;

        List<Vector2Int> wholeMapPositions = new List<Vector2Int>();

        for (int x = minX; x < maxX; x++)
        {
            int vecX = x;

            for (int y = minX; y < maxY; y++)
            {
                int vecY = y;
                wholeMapPositions.Add(new Vector2Int(vecX, vecY));
            }
        }

        List<Vector2Int> waterPositions = wholeMapPositions.Except(grassPositions).ToList();

        //(List<Vector2Int> cityPositions, List<Vector2Int> reducedPositions) = RandomlyChooseLocations(grassPositions, 20);
        //grassPositions = reducedPositions;

        //Dictionary<Vector2Int, TileScript.TileType> TileDictionary = new Dictionary<Vector2Int, TileScript.TileType>();


        CreateTileObjects(waterPositions, TileScript.TileType.Water);
        CreateTileObjects(grassPositions, TileScript.TileType.Land);
        //List<TileScript> cityTiles = CreateTileObjects(cityPositions, TileScript.TileType.City);

        GameControllerScript gameController = FindObjectsOfType<GameControllerScript>().FirstOrDefault();
        //gameController.PopulateTileList();


        List<TileScript> grasstiles = gameController.FullTileList.Where(w => w.Type == TileScript.TileType.Land).ToList();
        int NUMBEROFCOSALCITIES = grasstiles.Count / 20; //50
        int NUMBEROFLANDCITES = grasstiles.Count / 20; //50




        List<TileScript> landCities = ChooseSpacedCities(NUMBEROFLANDCITES, grasstiles, false);
        List<TileScript> landAndCostalCities = ChooseSpacedCities(NUMBEROFCOSALCITIES, grasstiles, true, landCities.ToList());


        //var distanceWeightings = new List<KeyValuePair<List<TileScript>, float>>();
        //int randomEffort = 5;
        //for (int i = 0; i < randomEffort; i++)
        //{
        //    List<TileScript> tempCities = new List<TileScript>();
        //    TileScript randomCity;

        //    for (int z = 0; z < NUMBEROFPLAYERS; i++)
        //    {

        //        do
        //        {
        //            randomCity = landCities[Random.Range(0, landCities.Count)];
        //        }
        //        while (tempCities.Contains(randomCity));
        //        tempCities.Add(randomCity);
        //    }

        //    float totaldistance = tempCities.Zip(tempCities.Skip(1),
        //         (a, b) => Vector2.Distance(a.transform.position, b.transform.position))
        //        .Sum();

        //    distanceWeightings.Add(new KeyValuePair<List<TileScript>, float>(tempCities, totaldistance));

        //}

        //List<TileScript> finalStartingCites = distanceWeightings.OrderByDescending(i => i.Value).Last().Key;
        //int teamNo = 1;
        //finalStartingCites.ForEach(c => c.Team = teamNo++);



        //var furthestCity = landCities
        //.Select(city => new
        //{
        //    City = city,
        //    TotalDistance = landCities
        //        .Where(otherCity => otherCity != city)
        //        .Sum(otherCity => Vector2.Distance(city.transform.position, otherCity.transform.position))
        //})
        //.OrderByDescending(cityData => cityData.TotalDistance)
        //.First().City;

        TileScript furthestCity = FindFurthestCity(landCities, landCities);
        List<TileScript> teamCities = new List<TileScript> { furthestCity };

        for (int i = 0; i < NUMBEROFPLAYERS - 1; i++)
        {
            teamCities.Add(FindFurthestCity(landCities, teamCities));
        }

        teamCities = teamCities.OrderBy(x => Random.value).ToList(); //shuffle
        int teamNo = 1;
        teamCities.ForEach(c => c.Team = teamNo++);








        //gameController.FullTileList.ForEach(tile => tile.GameConroller = gameController);
        gameController.FullTileList.ForEach(tile => tile.CalculateSprite());

        return;




        List<Vector2Int> floorPositionsAsList = floorPositions.ToList();
        FreePositionsToBuildOn = floorPositionsAsList;
        var floorIncludingUnderBarrels = BuildWallsAndBarrels(floorPositionsAsList, Direction2D.cardinalDirectionsList);

        grid.cellSize = new Vector3(1, 1);
        PaintTiles(floorIncludingUnderBarrels, floorTilemap, floorTile);
        grid.cellSize = new Vector3(0.16f, 0.16f);

        RandomlyPlaceObjects(KnightRef, 1);



        var GoblinLevelWeightings = new List<KeyValuePair<string, int>>() {
            new KeyValuePair<string, int>("NORMAL", 50),
            new KeyValuePair<string, int>(nameof(GoblinLevelTypes.Big), 20),
            new KeyValuePair<string, int>(nameof(GoblinLevelTypes.SparkleFast), 30)
        };

        RandomlyPlaceObjects(GoblinRef, 20, GoblinLevelWeightings);

        RandomlyPlaceObjects(ArmoredGoblinRef, 20);

        //RandomlyPlaceObjects(ArmoredGoblinRef, 6);

        // for (int i = 0; i < 7; i++)
        // {
        //     var positions = ReturnRandomGroupPositions(5);
        //     BuildObjectsFromPositions(GoblinRef, positions.GetRange(0, 4));
        //     BuildObjectsFromPositions(ArcherRef, positions.GetRange(4, 1));
        // }


    }

    static TileScript FindFurthestCity(List<TileScript> cities1, List<TileScript> cities2)
    {
        ////take top perctage of list
        TileScript furthestCity = cities1
           .OrderByDescending(city1 => cities2.Sum(city2 => Vector2.Distance(city1.transform.position, city2.transform.position)))
           .FirstOrDefault();

        return furthestCity;
    }

    private List<TileScript> ChooseSpacedCities(int numberOfCites, List<TileScript> grassTiles, bool shouldBeNextToSea, List<TileScript> startingCityList = null)
    {
        int randomEffort = 50;
        List<TileScript> completeCitiesList = startingCityList ?? new List<TileScript>();

        for (int i = 0; i < numberOfCites; i++)
        {
            var distanceWeightings = new List<KeyValuePair<TileScript, float>>();
            for (int z = 0; z < randomEffort; z++)
            {
                TileScript randomGrassTile;
                do
                {
                    randomGrassTile = grassTiles[Random.Range(0, grassTiles.Count)];
                }
                while (randomGrassTile.isNextToSea != shouldBeNextToSea);


                float closestDistance = 1;

                if (completeCitiesList.Count > 0)
                {
                    closestDistance = completeCitiesList.Select(s => Vector2.Distance(s.transform.position, randomGrassTile.transform.position)).ToList().Min();
                }

                //float totalDistanceToOtherCities = completeCitiesList.Sum(s => Vector2.Distance(s.transform.position, randomGrassTile.transform.position));

                distanceWeightings.Add(new KeyValuePair<TileScript, float>(randomGrassTile, closestDistance));
            }

            TileScript newCity = distanceWeightings.OrderByDescending(i => i.Value).First().Key;

            grassTiles.Remove(newCity);
            newCity.Type = TileScript.TileType.City;
            completeCitiesList.Add(newCity);
        }
        return completeCitiesList;
    }

    private List<TileScript> CreateTileObjects(IEnumerable<Vector2Int> HexPositionList, TileScript.TileType? GivenType = null)
    {
        List<TileScript> CreatedGameTileObjects = new List<TileScript>();
        foreach (var Position in HexPositionList)
        {
            //float x = (float)((Position.x * 0.16f) + 0.08);
            //float y = (float)((Position.y * 0.16f) + 0.08);

            float x = (float)Position.x;
            float y = x % 2 == 0 ? (float)Position.y + 0.5f : (float)Position.y;


            //float scale = 0.28f;
            float offSet = 1f;

            x = (float)(x * 0.24 * offSet);
            y = (float)(y * 0.28 * offSet);


            //x = (float)((x * 0.27) + offSet);
            //y = (float)((y * 0.31) + offSet);

            GameObject HexTileRefFromPath = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/Hex_Tile.prefab").GameObject();
            GameObject HexTileGameObject = Instantiate(HexTileRef, new Vector3(x, y), new Quaternion());

            HexTileGameObject.GetComponent<TileScript>().GridLocation = Position;

            if (HexTileGameObject.TryGetComponent<TileScript>(out var Script))
            {
                CreatedGameTileObjects.Add(Script);

                if (GivenType != null)
                {
                    Script.Type = (TileScript.TileType)GivenType;
                }
            }

        }
        return CreatedGameTileObjects;
    }
    (List<Vector2Int>, List<Vector2Int>) RandomlyChooseLocations(List<Vector2Int> Positions, int Amount) // tuple return type
    {
        List<Vector2Int> RandomPositions = new List<Vector2Int>();


        for (int i = 0; i < Amount; i++)
        {
            int randomI = Random.Range(0, Positions.Count);
            Vector2Int chosenPosition = Positions[randomI];
            RandomPositions.Add(chosenPosition);
            Positions.Remove(chosenPosition);
        }

        return (RandomPositions, Positions); // tuple literal
    }













    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    // internal void PaintSingleBasicWall(Vector2Int position)
    // {
    //     PaintSingleTile(wallTilemap, wallTop, position);
    // }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
            if (go != null && go.activeInHierarchy && go.name.Contains("(Clone)"))
            {
                DestroyImmediate(go);
            }
    }

    // public static void CreateWalls(List<Vector2Int> floorPositions)
    // {
    //     var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList);
    //     foreach (var position in basicWallPositions)
    //     {
    //         PaintSingleTile(position);
    //     }
    // }

    private List<Vector2Int> BuildWallsAndBarrels(List<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        var floorIncludingUnderBarrel = floorPositions;

        List<Vector2Int> WallAndBarrelPositions = new();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition) == false)
                {
                    WallAndBarrelPositions.Add(neighbourPosition);
                }
            }
        }

        List<Vector2Int> barrelPositions = new();
        List<Vector2Int> wallOnlyPositions = WallAndBarrelPositions;

        foreach (var WallPosition in WallAndBarrelPositions.ToList())
        {
            int SidesUnconvered = 4;
            foreach (var direction in directionList)
            {
                var neighbourPosition = WallPosition + direction;
                if (floorPositions.Contains(neighbourPosition) || WallAndBarrelPositions.Contains(neighbourPosition))
                {
                    SidesUnconvered--;
                }
            }
            if (SidesUnconvered == 0)
            {
                barrelPositions.Add(WallPosition);
            }
        }

        //CONNECT ALMOST BARRELS with barrel and wall
        foreach (var barrelPosition in barrelPositions.ToList())
        {

            foreach (var direction in directionList)
            {
                var neighbourPosition = barrelPosition + direction;
                if (!WallAndBarrelPositions.Contains(neighbourPosition))
                {
                    var oneAlongNeighbourPosition = neighbourPosition + direction;
                    if (WallAndBarrelPositions.Contains(oneAlongNeighbourPosition))
                    {
                        if (Random.Range(0, 3) == 1)
                        {
                            barrelPositions.Add(neighbourPosition);
                        }
                    }
                }
            }
        }

        //REMOVE INDIVIDUAL BARRELS




        wallOnlyPositions = wallOnlyPositions.Where(x => !barrelPositions.Contains(x))
                         .ToList();

        FreePositionsToBuildOn = FreePositionsToBuildOn.Where(x => !barrelPositions.Contains(x))
                      .ToList();

        floorIncludingUnderBarrel.AddRange(barrelPositions);

        barrelPositions = barrelPositions.Distinct().ToList();

        BuildObjectsFromPositions(BarrelRef, barrelPositions);
        BuildObjectsFromPositions(WallRef, wallOnlyPositions);

        return floorIncludingUnderBarrel;
    }

    private void BuildObjectsFromPositions(GameObject GameObjectRef, List<Vector2Int> ObjectsPositions, List<KeyValuePair<string, int>> variations = null)
    {

        foreach (var Position in ObjectsPositions)
        {
            float x = (float)((Position.x * 0.16f) + 0.08);
            float y = (float)((Position.y * 0.16f) + 0.08);
            GameObject go = Instantiate(GameObjectRef, new Vector3(x, y), new Quaternion());

            if (variations != null)
            {

                var totalChance = variations.Sum(x => x.Value);
                var chosenNumber = Random.Range(0, totalChance);
                var count = 0;

                var chosenString = "";

                foreach (var item in variations)
                {
                    count += item.Value;

                    if (count >= chosenNumber)
                    {
                        chosenString = item.Key;
                        break;
                    }

                }
                var com = go.AddComponent<Level_Holder>();
                com.StringLevel = chosenString;

                if (go.TryGetComponent<Goblin_Script>(out var parent))
                {
                    parent.SetLevel(com.StringLevel);
                }

            }
        }

    }

    private void RandomlyPlaceObjects(GameObject GameObjectRef, int numberOfObject, List<KeyValuePair<string, int>> variations = null)
    {
        List<Vector2Int> ObjectsPositions = new();

        for (int i = 0; i < numberOfObject; i++)
        {
            int randomI = Random.Range(0, FreePositionsToBuildOn.Count);
            Vector2Int chosenPosition = FreePositionsToBuildOn[randomI];

            ObjectsPositions.Add(chosenPosition);
            FreePositionsToBuildOn.Remove(chosenPosition);
        }

        BuildObjectsFromPositions(GameObjectRef, ObjectsPositions, variations);
    }

    private List<Vector2Int> ReturnRandomGroupPositions(int numberInGroup)
    {
        List<Vector2Int> ObjectsPositions = new();

        Vector2Int startingLocation = FreePositionsToBuildOn[Random.Range(0, FreePositionsToBuildOn.Count)];
        ObjectsPositions.Add(startingLocation);
        FreePositionsToBuildOn.Remove(startingLocation);

        for (int i = 0; i < numberInGroup; i++)
        {
            Vector2Int possibleLocation = FreePositionsToBuildOn[Random.Range(0, FreePositionsToBuildOn.Count)];
            int safteybreak = 0;

            while (Vector2.Distance(possibleLocation, startingLocation) > 3.5 || safteybreak > 15)
            {
                possibleLocation = FreePositionsToBuildOn[Random.Range(0, FreePositionsToBuildOn.Count)];
            }

            ObjectsPositions.Add(possibleLocation);
            FreePositionsToBuildOn.Remove(possibleLocation);

        }

        return ObjectsPositions;
    }
}
