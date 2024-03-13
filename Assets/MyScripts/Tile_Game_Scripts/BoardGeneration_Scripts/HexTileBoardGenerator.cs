using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class HexTileBoardGenerator : MonoBehaviour
{
    //og IslandSize = 10;
    //og MapSize = 70;

    [SerializeField]
    private int MapSize = 70, PlayerNumber = 4, AverageIslandSize = 5;

    //private int AverageIslandSize => MapSize / 5;

    public GameObject _hexTileRefFromPath;
    public GameObject HexTileRefFromPath
    {
        get => _hexTileRefFromPath != null ? _hexTileRefFromPath : _hexTileRefFromPath = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Objects/Hex_Tile.prefab").GameObject();
    }

    public void ClearMap()
    {
        FindObjectsOfType<GameObject>()
         .Where(go => go != null && go.activeInHierarchy && go.name.Contains("(Clone)"))
         .ToList()
         .ForEach(DestroyImmediate);
    }

    public DateTime startTime;
    public void PrintTime(string message)
    {
        //return;
        var newTime = DateTime.Now;
        print(message + " : " + (newTime - startTime).TotalSeconds);
        startTime = newTime;
    }

    //public void GenerateMap()
    //{
    //    StartCoroutine(GenerateMapCoroutine());
    //}
    //List<int> coroutinesRunning = new();

    //public IEnumerator SetUpComponents(TileScript tile, int index)
    //{
    //    coroutinesRunning.Add(index);
    //    tile.SetUpComponents();
    //    yield return null;
    //    coroutinesRunning.Remove(index);
    //}


    public void GenerateMap()
    {
        startTime = DateTime.Now;
        ClearMap();

        List<Vector2Int> LandCoordinates = LandGeneratorAlgorithm.GenerateLandCoordinates(MapSize, AverageIslandSize).ToList();

        PrintTime("After LandAlogrithms"); //145

        int extraWater = 10;
        List<Vector2Int> WholeMapCoordinates = GenerateWholeMapPositions(LandCoordinates, extraWater);
        List<Vector2Int> WaterCoordinates = WholeMapCoordinates.Except(LandCoordinates).ToList();

        List<TileScript> WaterTiles = WaterCoordinates.Select(i => GenerateTileObject(i, TileScript.TileType.Water)).ToList();
        List<TileScript> LandTiles = LandCoordinates.Select(i => GenerateTileObject(i, TileScript.TileType.Land)).ToList();
        List<TileScript> AllTiles = WaterTiles.Union(LandTiles).ToList();


        var gameController = FindObjectsOfType<GameController>().FirstOrDefault();
        gameController.FullTileList = AllTiles;

        //AllTiles.ForEach(t => t.SetUpComponents());

        //gameController.TeamList = Enumerable.Range(1, PlayerNumber).ToList();

        int NUMBEROFCOSALCITIES = LandTiles.Count / 40; //50
        int NUMBEROFLANDCITES = LandTiles.Count / 40; //50


        PrintTime("After Tile Generation"); //754


        List<TileScript> landCities = ChooseSpacedCities(NUMBEROFLANDCITES, LandTiles, false);
        PrintTime("Citylandfind");
        List<TileScript> landAndCostalCities = ChooseSpacedCities(NUMBEROFCOSALCITIES, LandTiles, true, landCities.ToList());

        PrintTime("CityBothFind");
        TileScript furthestCity = FindFurthestCity(landCities, landCities);
        List<TileScript> teamCities = new() { furthestCity };

        PrintTime("City2");
        float averageDistance = landCities.SelectMany(city1 =>
                  landCities.Where(city2 => city1 != city2)
                  .Select(city2 => Vector3.Distance(city1.transform.position, city2.transform.position)))
                  .Average();

        for (int i = 0; i < PlayerNumber - 1; i++)
        {
            teamCities.Add(FindFurthestCityNotTooClose(landCities, teamCities, averageDistance));
        }
        PrintTime("City3");
        teamCities = teamCities.OrderBy(x => UnityEngine.Random.value).ToList(); //shuffle
        int teamNo = 1;
        teamCities.ForEach(c =>
        {
            c.Team = teamNo++;
            c.IsCapital = true;
        });

        PrintTime("City4"); //2230


        AllTiles.ForEach(x => x.CalculateSprite());


        PrintTime("END TIME"); ; //7491 --> 4734 -->
    }

    public List<Vector2Int> GenerateWholeMapPositions(List<Vector2Int> LandCoordinates, int extraWater)
    {
        int maxX = LandCoordinates.Max(p => p.x) + extraWater;
        int maxY = LandCoordinates.Max(p => p.y) + extraWater;
        int minX = LandCoordinates.Min(p => p.x) - extraWater;
        int minY = LandCoordinates.Min(p => p.y) - extraWater;

        return Enumerable.Range(minX, maxX - minX)
            .SelectMany(x => Enumerable.Range(minY, maxY - minY).Select(y => new Vector2Int(x, y)))
            .ToList();
    }

    private TileScript GenerateTileObject(Vector2Int tileCoords, TileScript.TileType? GivenType = null)
    {
        float x = (float)tileCoords.x;
        float y = x % 2 == 0 ? (float)tileCoords.y + 0.5f : (float)tileCoords.y;

        float offSet = 1; //old 0.28f
        x = (float)(x * 0.24 * offSet); //0.27
        y = (float)(y * 0.28 * offSet); //0.31

        GameObject HexTileGameObject = Instantiate(HexTileRefFromPath, new Vector3(x, y), new Quaternion());
        HexTileGameObject.GetComponent<TileScript>().GridLocation = tileCoords;

        TileScript tileScript = HexTileGameObject.GetComponent<TileScript>();
        if (GivenType != null)
        {
            tileScript.Type = (TileScript.TileType)GivenType;
        }
        return tileScript;
    }


    static TileScript FindFurthestCity(List<TileScript> cities1, List<TileScript> cities2, int fractionToChooseFrom = 10)
    {
        List<TileScript> citiesByDistance = cities1
           .OrderByDescending(city1 => cities2.Sum(city2 => Vector2.Distance(city1.transform.position, city2.transform.position))).ToList();

        //return citiesByDistance.First(); //this for no randomness

        int fraction = citiesByDistance.Count / fractionToChooseFrom;

        // Take the first quarter of the list
        List<TileScript> firstFractionCities = citiesByDistance.Take(fraction).ToList();

        // Access the randomly chosen item
        TileScript furthestCity = firstFractionCities[UnityEngine.Random.Range(0, firstFractionCities.Count)];

        return furthestCity;
    }

    static TileScript FindFurthestCityNotTooClose(List<TileScript> landCities, List<TileScript> teamCities, float averageDistance)
    {
        var tempLandCities = landCities.ToList();
        var tempTeamCities = teamCities.ToList();

        TileScript furthestCity = null;

        int whileSkipper = 0;
        while (true && whileSkipper < 8)
        {
            whileSkipper++;

            furthestCity = FindFurthestCity(tempLandCities, tempTeamCities);


            tempTeamCities.Add(furthestCity);
            bool notTooClose = teamCities.All(city => teamCities.Where(otherCity => otherCity != city)
                                       .All(otherCity => Vector2.Distance(city.transform.position, otherCity.transform.position) >= averageDistance));


            if (notTooClose)
            {
                break;
            }

            tempTeamCities.Remove(furthestCity);
            tempLandCities.Remove(furthestCity);
        }

        return furthestCity;
    }

    //public TileScript LeastCloseCity(List<TileScript> cities1, List<TileScript> cities2)
    //{
    //    var otherCities = cities1.Except(cities2).ToList();

    //    return otherCities
    //        .OrderBy(city1 => cities2.Max(city2 => Vector2.Distance(city1.transform.position, city2.transform.position)))
    //        .Last();
    //}

    private List<TileScript> ChooseSpacedCities(int numberOfCites, List<TileScript> grassTiles, bool shouldBeNextToSea, List<TileScript> startingCityList = null)
    {
        int randomEffort = 5; //50
        List<TileScript> completeCitiesList = startingCityList ?? new List<TileScript>();

        for (int i = 0; i < numberOfCites; i++)
        {
            var distanceWeightings = new List<KeyValuePair<TileScript, float>>();
            for (int z = 0; z < randomEffort; z++)
            {
                TileScript randomGrassTile;
                do
                {
                    randomGrassTile = grassTiles[UnityEngine.Random.Range(0, grassTiles.Count)];
                }
                while (randomGrassTile.IsNextToSea != shouldBeNextToSea && !completeCitiesList.Contains(randomGrassTile));

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



}