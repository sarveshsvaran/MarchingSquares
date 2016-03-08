using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//https://upload.wikimedia.org/wikipedia/en/5/59/Marching-squares-isoline.png

public class MeshGenerator : MonoBehaviour {

    public CellGrid cells;
    public List <Vector3> vertices;
    public List<int> triangles;

    public GameObject innerOutlineObject;
    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();

    List<List<int>> innerOutline = new List<List<int>>();

    public Material groundMaterial;
    List<int> outerOutline = new List<int>();

    HashSet<int> checkedInnerVertices = new HashSet<int>();
    HashSet<int> checkedOuterVertices = new HashSet<int>();

    [Range(3,20)]
    public int depth;

    public List<Vector3> innerOutlineVertices = new List<Vector3>();
    public List<int> innerOutlineTriangle = new List<int>();
    GameObject ground;

    public void Start()
    {
        ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        //ground.AddComponent<MeshCollider>();
    }

    public void GenerateMesh(int[,] map,float gridSize)
    {

        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    vertices[i] = Vector3.one;
        //}
        //for (int i = 0; i < triangles.Count; i++)
        //{
        //    triangles[i] = 0;
        //}
        //cells = new Cell[map.GetLength(0), map.GetLength(1)];

        innerOutline.Clear();
        innerOutlineVertices.Clear();
        checkedInnerVertices.Clear();
        triangleDictionary.Clear();
        innerOutlineTriangle.Clear();

        vertices = new List<Vector3>();
        triangles = new List<int>();
        cells = new CellGrid(map,gridSize);


        for (int i = 0; i < cells.squares.GetLength(0); i++)
        {
            for (int j = 0; j < cells.squares.GetLength(1); j++)
            {

                TriangulateCell(cells.squares[i, j]);

            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh= mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateNormals();


        GenerateInnerOutlineMesh();
        GenerateBottomSurface((map.GetLength(0)) * gridSize, (map.GetLength(1)) * gridSize);
     }

    void GenerateBottomSurface(float width,float height)
    {
        //GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ground.name = "Ground";
        ground.transform.parent = transform;
        ground.GetComponent<MeshRenderer>().material = groundMaterial;
        ground.transform.localRotation = Quaternion.Euler(90,0,0);
        Vector3 scale = new Vector3((int)width,(int)height,1);
        ground.transform.localScale = scale;
        Vector3 pos = new Vector3(transform.position.x,transform.position.y-depth,transform.position.z);
        ground.transform.position = pos;
    }
    void GenerateInnerOutlineMesh()
    {
        TraverseMeshForAllOutlines();

        Mesh innerOutlineMesh = new Mesh();

        foreach (List<int>outline in innerOutline)
        {

            for (int i = 0; i < outline.Count-1; i++)
            {
                int x = innerOutlineVertices.Count;

                innerOutlineVertices.Add(vertices[outline[i]]);
                innerOutlineVertices.Add(vertices[outline[i+1]]);
                innerOutlineVertices.Add(vertices[outline[i]] - Vector3.up*depth);
                innerOutlineVertices.Add(vertices[outline[i+1]] - Vector3.up*depth);


                innerOutlineTriangle.Add(x + 3);
                innerOutlineTriangle.Add(x + 1);
                innerOutlineTriangle.Add(x + 0);


                innerOutlineTriangle.Add(x + 0);
                innerOutlineTriangle.Add(x + 2);
                innerOutlineTriangle.Add(x + 3);
                
            }
        }
        innerOutlineMesh.vertices = innerOutlineVertices.ToArray();
        innerOutlineMesh.triangles = innerOutlineTriangle.ToArray();
        innerOutlineObject.GetComponent<MeshFilter>().mesh = innerOutlineMesh;
        innerOutlineObject.GetComponent<MeshCollider>().sharedMesh = innerOutlineMesh;


    }

    void TriangulateCell(Cell cell)
    {
        switch (cell.contourConfig)
        {
            //0
            case 0:
                break;
            case 1:
                MeshFromNode(cell.centreLeft,cell.centreBottom, cell.bottomLeft);
                break;
            case 2:
                MeshFromNode(cell.centreBottom,cell.centreRight,cell.bottomRight);
                break;
            case 3:
                MeshFromNode(cell.centreRight, cell.bottomRight, cell.bottomLeft,cell.centreLeft);
                break;
            case 4:
                MeshFromNode(cell.topRight, cell.centreRight,cell.centreTop);
                break;
            case 5:
                MeshFromNode(cell.centreTop, cell.topRight, cell.centreRight, cell.centreBottom, cell.bottomLeft, cell.centreLeft);
                break;
            case 6:
                MeshFromNode(cell.centreTop, cell.topRight, cell.bottomRight, cell.centreBottom);
                break;
            case 7:
                MeshFromNode(cell.centreTop, cell.topRight, cell.bottomRight, cell.bottomLeft,cell.centreLeft);
                break;
            case 8:
                MeshFromNode(cell.topLeft,cell.centreTop,cell.centreLeft);
                break;
            case 9:
                MeshFromNode(cell.topLeft, cell.centreTop, cell.centreBottom, cell.bottomLeft);
                break;
            case 10:
                MeshFromNode(cell.topLeft, cell.centreTop, cell.centreRight, cell.bottomRight, cell.centreBottom,cell.centreLeft);
                break;
            case 11:
                MeshFromNode(cell.topLeft, cell.centreTop, cell.centreRight, cell.bottomRight, cell.bottomLeft);
                break;
            case 12:
                MeshFromNode(cell.topLeft, cell.topRight, cell.centreRight, cell.centreLeft);
                break;
            case 13:
                MeshFromNode(cell.topLeft,cell.topRight, cell.centreRight, cell.centreBottom,cell.bottomLeft);
                break;
            case 14:
                MeshFromNode(cell.topLeft,cell.topRight, cell.bottomRight,cell.centreBottom,cell.centreLeft);
                break;
            case 15:
                MeshFromNode(cell.topLeft, cell.topRight, cell.bottomRight, cell.bottomLeft);
                checkedInnerVertices.Add(cell.topLeft.vertexIndex);
                checkedInnerVertices.Add(cell.topRight.vertexIndex);
                checkedInnerVertices.Add(cell.bottomRight.vertexIndex);
                checkedInnerVertices.Add(cell.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromNode(params MinorNode[] nodes)
    {
        AddVertices(nodes);

        if (nodes.Length >= 3)
            AddTriangles(nodes[0],nodes[1],nodes[2]);
        if (nodes.Length >= 4)
            AddTriangles(nodes[0], nodes[2], nodes[3]);
        if (nodes.Length >= 5)
            AddTriangles(nodes[0], nodes[3], nodes[4]);
        if (nodes.Length >= 6)
            AddTriangles(nodes[0], nodes[4], nodes[5]);

    }

    void AddVertices(MinorNode[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].vertexIndex == -1)
            {
                nodes[i].vertexIndex = vertices.Count;
                vertices.Add(nodes[i].position);
            }
        }
    }
    void AddTriangles(MinorNode a,MinorNode b,MinorNode c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex,b.vertexIndex,c.vertexIndex);
        AddToDictionary(a.vertexIndex,triangle);
        AddToDictionary(b.vertexIndex,triangle);
        AddToDictionary(c.vertexIndex,triangle);
    }


    void AddToDictionary(int vertexKey,Triangle correspondingTriangle)
    {
        if(triangleDictionary.ContainsKey(vertexKey))
            triangleDictionary[vertexKey].Add(correspondingTriangle);
        else
        {
            List<Triangle> tempTriangles = new List<Triangle>();
            tempTriangles.Add(correspondingTriangle);
            triangleDictionary.Add(vertexKey,tempTriangles);
        }
    }


    void CalculateOuterOutlines()
    {
        if (!checkedOuterVertices.Contains(0))
        {
            int newVertexIndex = GetNeighbouringOutlineVertex(0);
            if (newVertexIndex!=-1)
            {

                outerOutline.Add(0);
                TraverseGivenOutline(0,outerOutline.Count-1);
                outerOutline.Add(0);
            }
        }
    }
    void TraverseMeshForAllOutlines()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (!checkedInnerVertices.Contains(i)) {
                int newVertexIndex = GetNeighbouringOutlineVertex(i);
                if (newVertexIndex != -1)
                {
                    checkedInnerVertices.Add(i);
                    //if (isOutline(i, newVertexIndex))
                    //{
                        //Debug.Log("Calling Outlines");
                        List<int> tempOutline = new List<int>();
                        tempOutline.Add(i);                                                 //new outline has been found

                        innerOutline.Add(tempOutline);
                        TraverseGivenOutline(i,innerOutline.Count-1);
                        innerOutline[innerOutline.Count-1].Add(i);
                    //}
                }
            }
        }
    }

    void TraverseGivenOutline(int outlineStartVertex,int outlineIndex)                  //passing the outlineindex and start of the vertex from the new outline found
    {
        innerOutline[outlineIndex].Add(outlineStartVertex);
        checkedInnerVertices.Add(outlineStartVertex);
        if (GetNeighbouringOutlineVertex(outlineStartVertex)!=-1)
        {
            //innerOutline[outlineIndex].Add(GetNeighbouringOutlineVertex(outlineStartVertex));
            TraverseGivenOutline(GetNeighbouringOutlineVertex(outlineStartVertex),outlineIndex);
        }
    }

    bool isOutline(int vertexA,int vertexB)
    {
        List<Triangle> vertexATriangles = triangleDictionary[vertexA];
        //List<Triangle> vertexBTriangles = triangleDictionary[vertexB];
        int commonTriangleCount = 0;
        //for (int i = 0; i < vertexBTriangles.Count; i++)
        //{
        //    if (vertexATriangles.Contains(vertexBTriangles[i]))
        //    {
        //        if (commonTriangleCount>1)
        //        {
        //            break;
        //        }
        //        else
        //            commonTriangleCount++;
        //    }
        //}

        for (int i = 0; i < vertexATriangles.Count; i++)
        {
            if (vertexATriangles[i].ContainsVertex(vertexB))
            {
                commonTriangleCount++;
                if (commonTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return commonTriangleCount==1;
    }

    int GetNeighbouringOutlineVertex(int vertexA)
    {
        List<Triangle> vertexATriangles = new List<Triangle>();

        vertexATriangles = triangleDictionary[vertexA];

        for (int i = 0; i < vertexATriangles.Count; i++)
        {
            Triangle triangle = vertexATriangles[i];
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexA!=triangle[j] && !checkedInnerVertices.Contains(triangle[j])) {
                    if (isOutline(vertexA, triangle[j])) {
                        return triangle[j];
                    }
                }
            }
         }

        return -1;
    }
    struct Triangle
    {
        public int vertexA;
        public int vertexB;
        public int vertexC;

        int[] vertices; 
        public Triangle(int vertexA,int vertexB,int vertexC)
        {
            this.vertexA = vertexA;
            this.vertexB = vertexB;
            this.vertexC = vertexC;

            vertices = new int[3];
            vertices[0] = vertexA;
            vertices[1] = vertexB;
            vertices[2] = vertexC;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }        
        }
        public bool ContainsVertex(int vertex)
        {
            return vertexA == vertex || vertexB == vertex || vertex == vertexC;
        }
    }
    public class CellGrid
    {
        public Cell[,] squares;
        public CellGrid(int[,] map,float gridSize)
        {

            int gridCountX = map.GetLength(0);
            int gridCountY = map.GetLength(1);
            float mapWidth = (gridCountX * gridSize);
            float mapHeight =(gridCountY * gridSize);

            MajorNode[,] majorNodes;
            majorNodes = new MajorNode[gridCountX,gridCountY];
            for (int i = 0; i < gridCountX ; i++)
            {
                for (int j = 0; j < gridCountY ; j++)
                {
                    Vector3 pos = new Vector3(-mapWidth/2 + i*gridSize + gridSize/2,0,-mapHeight/2 + j*gridSize + gridSize/2);
                    majorNodes[i,j] = new MajorNode(pos,map[i,j]==1,gridSize);
                }
            }

            for (int i = 0; i < gridCountX; i++)
            {
                //Debug.Log("Calling sssffff" + majorNodes[i, 20].position);
            }
            squares = new Cell[gridCountX-1,gridCountY-1];
            for (int i = 0; i < gridCountX-1; i++)
            {

                for (int j = 0; j < gridCountY-1; j++)
                {
                    //squares[i, j].topLeft = Nodes[i, j + 1];
                    //squares[i, j].topRight = Nodes[i + 1, j + 1];
                    //squares[i, j].bottomRight = Nodes[i + 1, j];
                    //squares[i, j].bottomLeft = Nodes[i, j];
                    squares[i, j] = new Cell(majorNodes[i, j + 1], majorNodes[i + 1, j + 1], majorNodes[i + 1, j], majorNodes[i, j]);
                }
                //Debug.Log("Calling sssffff" + squares[i, 0].topLeft.position);

            }
        }
    }


    //gets major node and forms minor nodes automatically

    public class Cell
    {
        public MajorNode topLeft, topRight, bottomRight, bottomLeft;
        public MinorNode centreTop, centreRight, centreBottom, centreLeft;
        public int contourConfig;

        public Cell(MajorNode topLeft, MajorNode topRight, MajorNode bottomRight, MajorNode bottomLeft)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (bottomLeft.status==true)
            {
                contourConfig += 1;
            }
            if (bottomRight.status == true)
            {
                contourConfig += 2;
            }
            if (topRight.status == true)
            {
                contourConfig += 4;
            }
            if (topLeft.status == true)
            {
                contourConfig += 8;
            }
        }
        
    }

    public class MinorNode
    {
        public Vector3 position;
        public int vertexIndex = -1;
        public MinorNode(Vector3 position)
        {
            this.position = position;
        }
    }

    public class MajorNode : MinorNode
    {
        public bool status;
        public MinorNode above, right;

        public MajorNode(Vector3 position,bool status,float squareSize) : base(position)
        {
            this.status = status;
            above = new MinorNode(position + Vector3.forward*(squareSize/2));
            right = new MinorNode(position + Vector3.right*(squareSize/2));
        }
    }
}
