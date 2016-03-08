//http://mathworld.wolfram.com/CellularAutomaton.html
//http://jeremykun.com/2012/07/29/the-cellular-automaton-method-for-cave-generation/

using UnityEngine;
using System.Collections;

public class NoiseGenerator : MonoBehaviour {

    public int width;
    public int height;
    [Range(0, 100)]
    public int randomNoisePercent;
    int[,] map;
    bool _randomSeed;
    public int seed;
    [Range(0,10)]
    public int smoothValue;
    [Range(1,20)]
    public int borderWidth;
	// Use this for initialization
	void Start () {
        map = new int[width,height];
        GenerateMap();
    }
	
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateMap();
        }   
    }
	// Update is called once per frame

    void GenerateMap()
    {
        RandomMapGenerator();
        for (int i = 0; i < smoothValue; i++)
        {
            SmoothenNoise();
        }

        int[,] mapWithBorder = new int[width+(2*borderWidth),height + (2 * borderWidth)];

        for (int i = 0; i < mapWithBorder.GetLength(0); i++)
        {
            for (int j = 0; j < mapWithBorder.GetLength(1); j++)
            {
                if (i>borderWidth&&i<width+ borderWidth && j>borderWidth&&j<height+borderWidth)
                {
                    mapWithBorder[i, j] = map[i-borderWidth,j-borderWidth];
                }
                else
                {
                    mapWithBorder[i, j] = 1;
                }
            }
        }
        MeshGenerator meshgenerator = GetComponent<MeshGenerator>();
        GetComponent<MeshFilter>().mesh = null;
        meshgenerator.GenerateMesh(mapWithBorder, 1);
    }

    void RandomMapGenerator()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i==0||j==0||i==width-1||j==height-1)
                {
                    map[i, j] = 1;
                }
                else
                { 
                    if ((int)Random.Range(0,100) < randomNoisePercent)
                    {
                        map[i, j] = 1; 
                    }
                    else
                    {
                        map[i, j] = 0;
                    }
                }
            }
        }
    }

    //Cellular Automata Start

    void SmoothenNoise()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int neigbourWallCount = NeighbouringWallCount(i, j);
                if (neigbourWallCount>4)
                {
                    map[i,j] = 1;
                }
                else if(neigbourWallCount < 4)
                {
                    map[i, j] = 0;
                }
            }
        }
    }

    int NeighbouringWallCount(int gridX,int gridY)
    {
        int wallCount = 0;

        for (int i = gridX-1; i <= gridX+1; i++)
        {
            for (int j = gridY -1 ; j <= gridY +1; j++)
            {
                if (i!=gridX||j!=gridY) {
                    if (i < width - 1 && j < height - 1 && j > 0 && i > 0)
                    {
                        wallCount += map[i, j];
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }
        }
        return wallCount;
    }

    //Cellular Automata End

    //void OnDrawGizmos()
    //{
    //    if (map != null)
    //    {
    //        for (int i = 0; i < width; i++)
    //        {
    //            for (int j = 0; j < height; j++)
    //            {
    //                if (map[i, j] == 1)
    //                {
    //                    Gizmos.color = Color.black;
    //                }
    //                else
    //                {
    //                    Gizmos.color = Color.white;
    //                }
    //                Vector3 tempPos = new Vector3(transform.position.x + i + 0.5f - (width / 2), transform.position.z - (height / 2) + j + 0.5f, 0);
    //                Gizmos.DrawCube(tempPos, Vector3.one * 1f);
    //            }
    //        }
    //    }
    //}
}
