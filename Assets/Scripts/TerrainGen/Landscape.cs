using System.IO;
using UnityEngine;


public class Landscape : MonoBehaviour
{

    [SerializeField]
    private int perlinLayers;

    [SerializeField]
    private float[] frequency;

    [SerializeField]
    private float[] amplitude;

    [SerializeField]
    private float maxAltitude = 0.1f;

    [SerializeField]
    private float threshold = 10f;

    [SerializeField]
    private float depth = 10f;

    [SerializeField]
    [Range(0, 1000)]
    private uint iterations = 10;

    private Terrain terrain;


    (int dx, int dy)[] neighbors = new (int, int)[]
        { (1, 1), (-1,-1), (1, -1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)};

    private void Start()
    {
        terrain = FindObjectOfType<Terrain>();

        int width = terrain.terrainData.heightmapResolution;
        int height = terrain.terrainData.heightmapResolution;

        float[,] heightData = new float[width, height];




        float min = 0;
        float max = float.NegativeInfinity;

        for (int i = 0; i < perlinLayers; i++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float data = Mathf.PerlinNoise(
                        (frequency[i] * (float)x / width) ,
                        (frequency[i] * (float)y / height));

                    heightData[x, y] += (data * amplitude[i]) - 0.1f;
                }
            }
        }

        Texture2D t = new Texture2D(width, height, TextureFormat.RGB24, false);

        for (int x = 0; x < width ; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float color = heightData[x, y];
                t.SetPixel(x, y, new Color(color, color, color));
            }
        }
        t.Apply(false, false);

        byte[] bytes = t.EncodeToPNG();
        File.WriteAllBytes(@"C:\Users\andre\Pictures\" + "noise" + ".png", bytes);

        //// Post-processing / normalizing
        //for (int i = 0; i < width; i++)
        //{
        //    for (int j = 0; j < height; j++)
        //    {
        //        if (heightData[i, j] < min) min = heightData[i, j];
        //        else if (heightData[i, j] > max) max = heightData[i, j];
        //    }
        //}

        //if (min < 0 || max > maxAltitude)
        //{
        //    for (int i = 0; i < width; i++)
        //    {
        //        for (int j = 0; j < height; j++)
        //        {
        //            heightData[i, j] =
        //                maxAltitude * (heightData[i, j] - min) / (max - min);
        //        }
        //    }
        //}

        // The loop should go into method ---- Make number a variable
        //for (int b = 0; b < 100; b++)
        //{
        //    FaultModifier(heightData, depth);
        //}
        heightData = HydraulicErosion(heightData);

        //heightData = ThermalElevation(heightData);

        terrain.terrainData.SetHeights(0, 0, heightData);
    }
    public void FaultModifier(float[,] landscape, float depth, float decreaseDistance = 0)
    {
        // Create random fault epicentre and direction vector
        float cx = Random.value * landscape.GetLength(0);
        float cy = Random.value * landscape.GetLength(1);
        float direction = Random.value * 2 * (float)Mathf.PI;
        float dx = (float)Mathf.Cos(direction);
        float dy = (float)Mathf.Sin(direction);

        // Apply the fault
        for (int x = 0; x < landscape.GetLength(0); x++)
        {
            for (int y = 0; y < landscape.GetLength(1); y++)
            {
                // Get the dot product of the location with the fault
                float ox = cx - x;
                float oy = cy - y;
                float dp = ox * dx + oy * dy;
                float change;

                // Positive dot product goes up, negative goes down
                if (dp > 0)
                {
                    // Fault size will decrease with distance if
                    // decreaseDistance > 0
                    float decrease = decreaseDistance != 0
                        ? decreaseDistance / (decreaseDistance + dp)
                        : 1;
                    // Positive dot product goes up
                    change = depth * decrease;
                }
                else
                {
                    // Fault size will decrease with distance if
                    // decreaseDistance > 0
                    float decrease = decreaseDistance > 0
                        ? decreaseDistance / (decreaseDistance - dp)
                        : 1;
                    // Negative dot product goes down
                    change = -depth * decrease;
                }

                // Apply fault modification
                landscape[x, y] += change;
            }
        }
    }

    private float[,] ThermalElevation(float[,] curterrain)
    {
        float[,] elevation = new float[curterrain.GetLength(0), curterrain.GetLength(1)];

        System.Array.Copy(curterrain, elevation, curterrain.GetLength(0) * curterrain.GetLength(1));

        for (int x = 1; x < curterrain.GetLength(0) - 1; x++)
        {
            for (int y = 1; y < curterrain.GetLength(1) - 1; y++)
            {
                float tileHeight = curterrain[x, y];

                float limit = tileHeight - threshold;

                for (int i = 0; i < neighbors.Length; i++)
                {
                    int nx = x + neighbors[i].dx;
                    int ny = y + neighbors[i].dy;

                    float newHeight = elevation[nx, ny];

                    if (newHeight < limit)
                    {
                        float delta = (limit - newHeight) / threshold;

                        float change = delta * threshold / 8;

                        elevation[x, y] -= change;
                        elevation[nx, ny] += change;
                    }
                }
            }
        }
        return elevation;
    }

    // Optimize this crap!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    private float[,] HydraulicErosion(float[,] curterrain)
    {

        float[,] elevation = new float[curterrain.GetLength(0), curterrain.GetLength(1)];

        System.Array.Copy(curterrain, elevation, curterrain.GetLength(0) * curterrain.GetLength(1));

        for (int m = 0; m < 100000; m++)
        {

            float varSpeed = 1;
            float waterAmount = 1;
            float sediment = 0f;

            int lowerX = 0;
            int lowerY = 0;

            Vector2 dir = Vector2.zero;

            int x = Random.Range(0, curterrain.GetLength(0));
            int y = Random.Range(0, curterrain.GetLength(1));


            for (int it = 0; it < iterations; it++)
            {
                float currLowerHeight = float.PositiveInfinity;

                // calculate new Pos
                for (int i = 0; i < neighbors.Length; i++)
                {
                    int nx = x + neighbors[i].dx;
                    int ny = y + neighbors[i].dy;

                    Vector2 curDir = new Vector2(x - nx, y - ny);

                    if (curDir == -dir)
                    {
                        continue;
                    }

                    if (nx > 0 && nx < curterrain.GetLength(0) - 1 &&
                        ny > 0 && ny < curterrain.GetLength(1) - 1)
                    {
                        if (elevation[nx, ny] < currLowerHeight)
                        {
                            currLowerHeight = elevation[nx, ny];

                            lowerX = nx;
                            lowerY = ny;
                        }
                    }
                }

                if (currLowerHeight == float.PositiveInfinity || waterAmount <= 0)
                {
                    break;
                }



                float deltaHeight = elevation[lowerX, lowerY] - elevation[x, y];


                float maxSediment = Mathf.Max(-deltaHeight * varSpeed * waterAmount * 0.02f, 0.02f);



                if (sediment > maxSediment || deltaHeight > 0)
                {
                    float amountToDeposit = (deltaHeight > 0) ? Mathf.Min(deltaHeight, sediment) : (sediment - maxSediment) * 0.3f;

                    sediment -= amountToDeposit;

                    elevation[x, y] += amountToDeposit;


                    // Debug.Log(amountToDeposit);

                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        int nx = x + neighbors[i].dx;
                        int ny = y + neighbors[i].dy;


                        if (nx > 0 && nx < curterrain.GetLength(0) - 1 &&
                            ny > 0 && ny < curterrain.GetLength(1) - 1)
                        {
                            if (elevation[nx, ny] + (amountToDeposit * 0.9f) < elevation[lowerX, lowerY])
                            {
                                elevation[nx, ny] += amountToDeposit * 0.9f;
                            }
                        }
                    }
                }
                else
                {
                    float amountToErode = Mathf.Min((maxSediment - sediment) * 0.005f, -deltaHeight);

                    elevation[x, y] -= amountToErode;


                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        int nx = x + neighbors[i].dx;
                        int ny = y + neighbors[i].dy;

                        Vector2 curDir = new Vector2(x - nx, y - ny);

                        if (nx > 0 && nx < curterrain.GetLength(0) - 1 &&
                            ny > 0 && ny < curterrain.GetLength(1) - 1)
                        {
                            if (curDir != dir || curDir != -dir)
                            {
                                elevation[nx, ny] -= amountToErode / 4;
                            }
                        }
                    }
                    sediment += amountToErode;
                }

                varSpeed = Mathf.Sqrt(varSpeed * varSpeed + deltaHeight * 4);
                waterAmount *= (1 - 0.01f);

                dir = new Vector2(x - lowerX, y - lowerY);

                x = lowerX;
                y = lowerY;
            }
        }
        return elevation;
    }
}