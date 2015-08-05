using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour {

	public SquareGrid squareGrid;
    List<Vector3> vertices;
    List<int> triangles;

	public void GenerateMesh(int[,] map, float squareSize)
	{
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
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
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
                MeshFromPoints(square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            // 0010
            case 2:
                MeshFromPoints(square.centreRight, square.bottomRight, square.centreBottom);
                break;
            // 0100
            case 4:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight);
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
    }

	/*void OnDrawGizmos()
	{
		if (squareGrid != null)
		{
			for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
			{
				for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
				{
					Gizmos.color = (squareGrid.squares[x,y].topLeft.active)?Color.black:Color.white;
					Gizmos.DrawCube(squareGrid.squares[x,y].topLeft.position, Vector3.one * .4f);

					Gizmos.color = (squareGrid.squares[x,y].topRight.active)?Color.black:Color.white;
					Gizmos.DrawCube(squareGrid.squares[x,y].topRight.position, Vector3.one * .4f);

					Gizmos.color = (squareGrid.squares[x,y].bottomLeft.active)?Color.black:Color.white;
					Gizmos.DrawCube(squareGrid.squares[x,y].bottomLeft.position, Vector3.one * .4f);

					Gizmos.color = (squareGrid.squares[x,y].bottomRight.active)?Color.black:Color.white;
					Gizmos.DrawCube(squareGrid.squares[x,y].bottomRight.position, Vector3.one * .4f);

					Gizmos.color = Color.grey;
					Gizmos.DrawCube(squareGrid.squares[x,y].centreTop.position, Vector3.one * .15f);
					Gizmos.DrawCube(squareGrid.squares[x,y].centreRight.position, Vector3.one * .15f);
					Gizmos.DrawCube(squareGrid.squares[x,y].centreBottom.position, Vector3.one * .15f);
					Gizmos.DrawCube(squareGrid.squares[x,y].centreLeft.position, Vector3.one * .15f);
				}
			}
		}
	}*/

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
