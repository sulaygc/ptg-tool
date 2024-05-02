using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    private Vector3 position;
    private float density;

    public Point(Vector3 position)
    {
        this.position = position; // position should be set from here on
    }

    public Vector3 Position
    {
        // these are C# 'properties' i.e. a shorthand for getters and setters 
        get { return position; }
        // no setter as it'd be private anyway, so position can be modified rather than Position
    }

    public float Density
    {
        // density will need to be changed by the Cube class holding these Points
        get { return density; }
        set { density = value; }
    }
}

public class Cube
{
    private Point[] points;
    private Region parent_region;

    private static int[,] edge_to_vertices = new int[12, 2]
    {
        // this tells you what corners connect a given edge - this is important for interpolation
        // based off the diagram seen in documentation
        {0, 1},
        {1, 2},
        {2, 3},
        {3, 0},
        {4, 5},
        {5, 6},
        {6, 7},
        {7, 4},
        {4, 0},
        {5, 1},
        {6, 2},
        {7, 3}
    };

    public Cube(Region parent_region, Point point0, Point point1, Point point2, Point point3, Point point4, Point point5, Point point6, Point point7)
    {
        points = new Point[8] { point0, point1, point2, point3, point4, point5, point6, point7 };
        this.parent_region = parent_region; // we're holding a reference to the parent region so that we can send it triangles to add to its mesh
    }

    public void March(float isovalue)
    {
        int vertex_configuration = FindVertexConfiguration(isovalue);
        string edge_configuration_12bit = MCEdgeTable.GetEdgeConfiguration(vertex_configuration);

        // the edge configuration tells us which edges to get ready to need interpolated positions for
        // we make a dictionary to allow for mappings between needed edges and their interpolated positions
        Dictionary<int, Vector3> edge_to_interpolated_position = new Dictionary<int, Vector3>();
        for (int i = 0; i < 12; i++)
        {
            if (edge_configuration_12bit[i] == '1')
            {
                // we use 11 - i as the edge table's values are also right to left
                edge_to_interpolated_position.Add(11 - i, InterpolateEdge(11 - i, isovalue));
            }
        }

        // now we get the triangle configuration
        int[] triangle_configuration = MCTriTable.GetTriangleConfiguration(vertex_configuration);

        // interpret the result: e.g. {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
        // we can loop through groups of three
        // && is a "short circuit and" so that if the length is exceeded no error is thrown for trying to check for -1s out of range
        for (int i = 0; i < 16 && triangle_configuration[i] != -1; i += 3)
        {
            // use the interpolation dictionary from earlier, then send for rendering
            parent_region.ReceiveTri(
                // the order is reversed in order to fit the clockwise winding order
                edge_to_interpolated_position[triangle_configuration[i + 2]],
                edge_to_interpolated_position[triangle_configuration[i + 1]],
                edge_to_interpolated_position[triangle_configuration[i]]
                );
        }
    }

    private int FindVertexConfiguration(float isovalue)
    {
        // determine 8-bit configuration of this cube via comparing densities to the isovalue
        string vertex_configuration_8bit = "";
        for (int i = 0; i < 8; i++)
        {
            // Paul Bourke's lookup table (that we use) assigns 1s to points under the isovalue, so we will follow this convention
            if (points[i].Density < isovalue)
            {
                // the lookup table works with right-to-left binary as well - e.g. 2nd position from right indicates vertex 1
                vertex_configuration_8bit = "1" + vertex_configuration_8bit;
            }
            else
            {
                vertex_configuration_8bit = "0" + vertex_configuration_8bit;
            }
        }
        int vertex_configuration = Convert.ToInt32(vertex_configuration_8bit, 2);
        return vertex_configuration;
    }

    private Vector3 InterpolateEdge(int edge_number, float isovalue)
    {
        // first, we get the vertices for this edge
        Point vertex1 = points[edge_to_vertices[edge_number, 0]];
        Point vertex2 = points[edge_to_vertices[edge_number, 1]];

        // now we find the difference between their densities and get the isovalue as a percentage of this difference
        float density_difference = vertex2.Density - vertex1.Density;
        float isovalue_difference_from_v1 = isovalue - vertex1.Density;
        float proportion = isovalue_difference_from_v1 / density_difference;

        // the interpolation process finishes by moving that proportion of the distance vector between the two vertices
        return vertex1.Position + proportion * (vertex2.Position - vertex1.Position);
    }
}

public class Region
{
    private float length, width;

    // this needs to be readable outside of this class for the colouring system
    public float height { get; private set; }

    private float resolution;

    public Mesh mesh; // needs to be public so that the MeshFilter and MeshRenderer can use it
    private List<Vector3> mesh_vertices;
    private List<int> mesh_tris;

    // technically, if regions are treated as an OOP singleton then the origin can just be "set" as (0,0,0)
    // however, it's likely better practice to allow for this class to be extended to more than one region instance for the sake of extensibility
    private Vector3 origin;
    private Point[,,] point_array_3d;
    private Cube[,,] cube_array_3d;

    public Region(float length, float height, float width, float resolution, Vector3 origin)
    {
        this.length = length; this.height = height; this.width = width;
        this.resolution = resolution; this.origin = origin;
        DivideSpace();

        // set up everything needed for mesh rendering
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // use 32-bit cap on vertices, rather than default 16
        mesh_vertices = new List<Vector3>();
        mesh_tris = new List<int>();
    }

    public void MarchWithNoise(OctaveNoise noise, float isovalue, bool sub_y_flag)
    {
        // OctaveNoise object is being taken in so that less weight is put on the Region class to deal with everything
        // also because in an OOP sense it's not really a singular region's job to deal with setting up noise objects, since you could technically have several regions
        foreach (Point point in point_array_3d)
        {
            // subtracting the y position provides a "base" which produces more realistic terrain
            // without this you have cave-style noise everywhere, which is cool but unrealistic for surface terrain
            // see https://developer.nvidia.com/gpugems/gpugems3/part-i-geometry/chapter-1-generating-complex-procedural-terrains-using-gpu
            if (sub_y_flag)
            {
                point.Density = -point.Position.y;
            }
            else
            {
                // it needs to be 0 so that when the density is added it's to a "blank slate" rather than last time's value
                point.Density = 0f;
            }

            point.Density += noise.GetNoise(point.Position);
        }

        // if densities are updated, then the mesh will need to be remarched
        MarchCubes(isovalue);
    }

    private void MarchCubes(float isovalue)
    {
        // clear existing mesh data
        mesh.Clear();
        mesh_vertices.Clear();
        mesh_tris.Clear();

        foreach (Cube cube in cube_array_3d)
        {
            cube.March(isovalue);
        }

        // now that all triangles have been received the mesh can be produced
        mesh.vertices = mesh_vertices.ToArray();
        mesh.triangles = mesh_tris.ToArray();
        mesh.RecalculateNormals(); // without this, shading won't work properly
    }

    public void ReceiveTri(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        // mesh vertices must work in terms of Vector3 positions, mesh tris holds references to positions in the mesh vertices array
        int mesh_vertices_new_pos = mesh_vertices.Count;
        mesh_vertices.Add(vertex1); mesh_vertices.Add(vertex2); mesh_vertices.Add(vertex3);
        mesh_tris.Add(mesh_vertices_new_pos); mesh_tris.Add(mesh_vertices_new_pos + 1); mesh_tris.Add(mesh_vertices_new_pos + 2);
    }

    private void DivideSpace()
    {
        Vector3 current_position; // current position of the "point placer"
        InitialisePointArray();

        // algorithm for moving the point placer through the grid and adding points to the 3d array
        for (int i = 0; i < point_array_3d.GetLength(0); i++) 
        { 
            for (int j = 0; j < point_array_3d.GetLength(1); j++)
            {
                for (int k = 0; k < point_array_3d.GetLength(2); k++)
                {
                    // this section ensures that each point is assigned the correct position relative to region resolution
                    // the reason why it's (x / resolution) is because movement along a dimension takes you forward (1/resolution) per Point
                    Vector3 distance_from_origin = new Vector3(i / resolution, j / resolution, k / resolution);
                    current_position = origin + distance_from_origin;
                    point_array_3d[i, j, k] = new Point(current_position);
                }
            }
        }
        InitialiseCubeArray();
    }

    private void InitialiseCubeArray()
    {
        // this method produces the massive array of cubes - i need this array so that I can later tell every cube in this array to march
        cube_array_3d = new Cube[point_array_3d.GetLength(0) - 1, point_array_3d.GetLength(1) - 1, point_array_3d.GetLength(2) - 1];

        // it's the point array length - 1 because you can fit n-1 connectors for n nodes
        for (int i = 0; i < point_array_3d.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < point_array_3d.GetLength(1) - 1; j++)
            {
                for (int k = 0; k < point_array_3d.GetLength(2) - 1; k++)
                {
                    cube_array_3d[i,j,k] = new Cube
                    (
                        // following the cube diagram I use for edges (it needs to be consistent)
                        this,
                        point_array_3d[i, j, k],
                        point_array_3d[i+1, j, k],
                        point_array_3d[i+1, j, k+1],
                        point_array_3d[i, j, k+1],
                        point_array_3d[i, j+1, k],
                        point_array_3d[i+1, j+1, k],
                        point_array_3d[i+1, j+1, k+1],
                        point_array_3d[i, j+1, k+1]
                    );  
                }
            }
        }
    }

    private void InitialisePointArray()
    {
        // calculate point numbers for each dimension
        int point_numbers_x = Mathf.FloorToInt(length * resolution);
        int point_numbers_y = Mathf.FloorToInt(height * resolution);
        int point_numbers_z = Mathf.FloorToInt(width * resolution);

        point_array_3d = new Point[point_numbers_x, point_numbers_y, point_numbers_z];
    }
}

public class MeshGenerator : MonoBehaviour
{
    // these are public as the getters and setters would be blank anyway - i.e. redundant code
    public Region region;
    public OctaveNoise octave_noise;
    public float isovalue;
    public bool sub_y_flag;

    public void Regenerate()
    {
        // attach new region mesh to meshfilter
        GetComponent<MeshFilter>().mesh = region.mesh;

        // send data and march
        region.MarchWithNoise(octave_noise, isovalue, sub_y_flag);
    }
}