using UnityEngine;

public class HydraulicErosion
{
    private (int dx, int dy)[] neighbors = new (int, int)[]
        { (1, 1), (-1,-1), (1, -1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)};

    
    public float[,] Generator(float[,] curterrain, float iterations)
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

                    //if (curDir == -dir)
                    //{
                    //    continue;
                    //}

                    if (nx > 0 && nx < curterrain.GetLength(0) - 1 &&
                        ny > 0 && ny < curterrain.GetLength(1) - 1)
                    {
                        float el = elevation[nx, ny];

                        if (el < currLowerHeight)
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

                        Vector2 curDir = new Vector2(x - nx, y - ny);

                        if (nx > 0 && nx < curterrain.GetLength(0) - 1 &&
                            ny > 0 && ny < curterrain.GetLength(1) - 1)
                        {
                            if (elevation[nx, ny] + (amountToDeposit * 0.9f) < elevation[lowerX, lowerY] && curDir.x != dir.x && curDir.y != dir.y)
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