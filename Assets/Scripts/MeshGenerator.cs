using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour {
    
	public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshFilter cave;
    public bool is2D;
    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

	public void GenerateMesh(int[,] map, float squareSize)
	{
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

		squareGrid = new SquareGrid(map, squareSize);
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        // Textures
        int tileAmount = 10;
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;

        if (is2D)
        {
            Generate2DColliders();
        }
        else
        {
            CreateWallMesh();
        }

	}

    void CreateWallMesh()
    {
        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTrinagles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]);                                     // Left
                wallVertices.Add(vertices[outline[i+1]]);                                   // Right
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight);           // Bottom Left
                wallVertices.Add(vertices[outline[i+1]] - Vector3.up * wallHeight);         // Bottom Right

                wallTrinagles.Add(startIndex);
                wallTrinagles.Add(startIndex + 2);
                wallTrinagles.Add(startIndex + 3);

                wallTrinagles.Add(startIndex + 3);
                wallTrinagles.Add(startIndex + 1);
                wallTrinagles.Add(startIndex);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTrinagles.ToArray();
        walls.mesh = wallMesh;

        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    void Generate2DColliders()
    {
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        for (int i = 0; i < currentColliders.Length; i++)
        {
            Destroy(currentColliders[i]);
        }

        CalculateMeshOutlines();

        foreach (List<int> outline in outlines)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = new Vector2 (vertices[outline[i]].x, vertices[outline[i]].z);
            }
            edgeCollider.points = edgePoints;
        }
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            // 0 Points
            // 0000
            case 0:
                break;

            // 1 Point
            // 0001
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            // 0010
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            // 0100
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            // 1000
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 Points
            // 0011
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            // 0110
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            // 1001
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            // 1100
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            // 0101
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            // 1010
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 Points
            // 0111
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            // 1011
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            // 1101
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            // 1110
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 Points
            // 1111
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            // If the point hasn't been assigned
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangeList = new List<Triangle>();
            triangeList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangeList);
        }
    }

    void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutLineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutLineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutLineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];

                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }
        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> triangleContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < triangleContainingVertexA.Count; i++)
        {
            if (triangleContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        // Returns true if both vertices only has 1 common triangle
        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        // Constructor
        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

	// Holds 2D array of squares
	public class SquareGrid
	{
		public Square[,] squares;

		// Constructor
		public SquareGrid(int[,] map, float squareSize)
		{
			int nodeCountX = map.GetLength(0);
			int nodeCountY = map.GetLength(1);

			float mapWidth = nodeCountX * squareSize;
			float mapHeight = nodeCountY * squareSize;

			ControlNode[,] controlNodes = new ControlNode[nodeCountX,nodeCountY];

			// Grid of Control Nodes
			for (int x = 0; x < nodeCountX; x++)
			{
				for (int y = 0; y < nodeCountY; y++)
				{
					Vector3 pos = new Vector3(-mapWidth + x * squareSize + squareSize/2, 0, -mapHeight + y * squareSize + squareSize/2);
					controlNodes[x,y] = new ControlNode(pos, map[x,y] == 1, squareSize);
				}
			}

			// Grid of Squares out of the Control Nodes
			squares = new Square [nodeCountX - 1, nodeCountY - 1];
			for (int x = 0; x < nodeCountX - 1; x++)
			{
				for (int y = 0; y < nodeCountY - 1; y++)
				{
					squares[x,y] = new Square(controlNodes[x,y+1], controlNodes[x+1,y+1], controlNodes[x+1,y], controlNodes[x,y]);
				}
			}
		}
	}

	public class Square
	{
		public ControlNode topLeft, topRight, bottomRight, bottomLeft;		// Reference to all corners of the nodes
		public Node centreTop, centreRight, centreBottom, centreLeft;		// Reference to the mid-point nodes
        public int configuration;                                           // 16 Ways to turn the nodes on/off

		// Constructor
		public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
		{
			topLeft = _topLeft;
			topRight = _topRight;
			bottomLeft = _bottomLeft;
			bottomRight = _bottomRight;

			centreTop = topLeft.right;
			centreRight = bottomRight.above;
			centreBottom = bottomLeft.right;
			centreLeft = bottomLeft.above;

            // Check to see if the nodes are active
            if (topLeft.active)
            {
                configuration += 8;     // 1000 = 8
            }
            if (topRight.active)
            {
                configuration += 4;     // 0100 = 4
            }
            if (bottomRight.active)
            {
                configuration += 2;     // 0010 = 2
            }
            if (bottomLeft.active)
            {
                configuration += 1;     // 0001 = 1
            }
		}
	}

	public class Node
	{
		public Vector3 position;				// Keep track of its position in the world
		public int vertexIndex = -1;

		// Constructor
		public Node(Vector3 _pos)
		{
			position = _pos;
		}
	}
	
	public class ControlNode : Node
	{
		public bool active;						// Says whether or not it's active (active = wall, not active = not wall)
		public Node above, right;

		// Constructor
		public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
		{
			active = _active;
			above = new Node(position + Vector3.forward * squareSize/2f);
			right = new Node(position + Vector3.right * squareSize/2f);
		}
	}
}
