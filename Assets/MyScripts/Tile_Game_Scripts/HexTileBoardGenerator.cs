using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexTileBoardGenerator : MonoBehaviour
{
    [SerializeField]
    private int AverageIslandSize = 10, MapSize = 70, PlayerNumber = 4;


    public void GenerateMap()
    {
        ClearMap();
        RunMapGeneration();
    }
    public void ClearMap()
    {
        FindObjectsOfType<GameObject>()
         .Where(go => go != null && go.activeInHierarchy && go.name.Contains("(Clone)"))
         .ToList()
         .ForEach(DestroyImmediate);
    }

    private void RunMapGeneration()
    {
        HashSet<Vector2Int> LandCoordinates = LandGeneratorAlgorithm.GenerateLandCoordinates(MapSize, AverageIslandSize);


        Debug.Log("WOOOOOO");
    }




}
