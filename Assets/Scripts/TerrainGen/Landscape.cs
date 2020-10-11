using System.IO;
using UnityEngine;

public class Landscape
{

    public float[,] Generate(int xDim, int yDim, int perlinLayers, float startF, float startA)
    {

        int width = xDim;
        int height = yDim;

        float[,] heightData = new float[width, height];

        for (int i = 0; i < perlinLayers; i++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float data = Mathf.PerlinNoise(
                        (startF * x / width),
                        (startF * y / height));

                    heightData[x, y] += (data * startA) - 0.1f;
                }
            }
            startF *= 2;
            startA /= 2;
        }

        Texture2D t = new Texture2D(width, height, TextureFormat.RGB24, false);

        for (int x = 0; x < width; x++)
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

        return heightData;
    }
}