using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawMapUltis : MonoBehaviour
{
    public PolygonCollider2D polygon
    {
        get
        {
            if (this._polygon == null) this._polygon = GetComponent<PolygonCollider2D>();
            return this._polygon;
        }
    }
    PolygonCollider2D _polygon;

    public List<GameObject> myObjects = new List<GameObject>();
    public Material myMaterial;

    private void OnEnable()
    {
        UpdatePolygon();
    }

    public Mesh CreateMesh(int num, string meshName)
    {
        Mesh mesh = new Mesh();
        List<Vector2> nodePositions = polygon.points.ToList();

        var vertex = new Vector3[nodePositions.Count];
        for (int i = 0; i < nodePositions.Count; i++)
            vertex[i] = new Vector3(nodePositions[i].x, nodePositions[i].y, 0);


        //UVs
        var uvs = new Vector2[vertex.Length];
        for (int i = 0; i < vertex.Length; i++)
        {
            if ((i % 2) == 0) uvs[i] = new Vector2(0, 0);
            else uvs[i] = new Vector2(1, 1);
        }

        //Triangles
        var tris = new int[3 * (vertex.Length - 2)];    //3 verts per triangle * num triangles
        int C1; int C2; int C3;

        if (num == 0)
        {
            C1 = 0;
            C2 = 1;
            C3 = 2;

            for (int x = 0; x < tris.Length; x += 3)
            {
                tris[x] = C1;
                tris[x + 1] = C2;
                tris[x + 2] = C3;

                C2++;
                C3++;
            }
        }
        else
        {
            C1 = 0;
            C2 = vertex.Length - 1;
            C3 = vertex.Length - 2;

            for (int i = 0; i < tris.Length; i += 3)
            {
                tris[i] = C1;
                tris[i + 1] = C2;
                tris[i + 2] = C3;

                C2--;
                C3--;
            }
        }

        //Assign data to mesh
        mesh.vertices = vertex;
        mesh.uv = uvs;
        mesh.triangles = tris;

        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        //Name the mesh
        mesh.name = meshName;

        //Return the mesh
        return mesh;
    }

    [ContextMenu("UpdatePolygon")]
    public void UpdatePolygon()
    {
        for (int i = 0; i < 2; i++)
        {
            MeshFilter MF = myObjects[i].GetComponent<MeshFilter>();
            MeshRenderer MR = myObjects[i].GetComponent<MeshRenderer>();

            //Destroy old game object
            if (MF.sharedMesh != null) MF.sharedMesh.Clear();

            //New mesh and game object            
            myObjects[i].name = i.ToString();
            Mesh mesh = new Mesh();

            //Create mesh
            mesh = CreateMesh(i, i.ToString());

            //Assign materials
            MR.material = myMaterial;

            //Assign mesh to game object
            MF.mesh = mesh;
        }
    }
}
