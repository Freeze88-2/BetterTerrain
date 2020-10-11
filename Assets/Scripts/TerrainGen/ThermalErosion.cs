public class ThermalErosion
{
    private (int dx, int dy)[] neighbors = new (int, int)[]
        { (1, 1), (-1,-1), (1, -1), (-1, 1), (1, 0), (-1, 0), (0, 1), (0, -1)};

    public float[,] Generator(float[,] curterrain, float threshold)
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
}