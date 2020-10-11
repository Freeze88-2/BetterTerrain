using UnityEngine;

public class FaultErosion
{
    public float[,] Generator(float[,] curterrain, float depth, float decreaseDistance = 0)
    {
        float[,] elevation = new float[curterrain.GetLength(0), curterrain.GetLength(1)];

        System.Array.Copy(curterrain, elevation, curterrain.GetLength(0) * curterrain.GetLength(1));

        for (int i = 0; i < 10; i++)
        {
            // Create random fault epicentre and direction vector
            float cx = Random.value * curterrain.GetLength(0);
            float cy = Random.value * curterrain.GetLength(1);
            float direction = Random.value * 2 * Mathf.PI;
            float dx = Mathf.Cos(direction);
            float dy = Mathf.Sin(direction);

            // Apply the fault
            for (int x = 0; x < curterrain.GetLength(0); x++)
            {
                for (int y = 0; y < curterrain.GetLength(1); y++)
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
                    elevation[x, y] += change;
                }

            }
        }
        return elevation;
    }
}