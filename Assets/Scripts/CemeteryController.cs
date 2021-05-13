using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CemeteryController : MonoBehaviour
{
    public GameObject grid;
    public float gridSizeX;
    public float gridSizeY;
    public int sizeX;
    public int sizeY;

    private void Awake()
    {
        for (int x = -sizeX / 2 - 1; x <= sizeX / 2; x += 2)
        {
            for (int y = 0; y > -sizeY; y -= 2)
            {
                Instantiate(grid, transform.position + new Vector3(x * gridSizeX + 0.5f, y * gridSizeY - 0.5f, 0), Quaternion.identity);
            }
        }
    }
}
