using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruncatedOctahedron : MonoBehaviour {

    public Material meshMaterial;
    public Material wireMaterial;

    Vector3[] ComputeSurfaceNormals(Mesh m) {
        var t = m.triangles;
        var v = m.vertices;
        var n = t.Length;
        var res = new Vector3[n];
        for (int i = 0; i < n; i += 3) {
            var A = v[t[i]];
            var B = v[t[i + 1]];
            var C = v[t[i + 2]];
            var AB = B - A;
            var AC = C - A;
            var N = Vector3.Cross(AB, AC);
            res[i] = res[i + 1] = res[i + 2] = N;
        }
        return res;
    }

    void UnshareVertices(Mesh m) {
        var t = m.triangles;
        var v = m.vertices;
        var n = t.Length;
        var v1 = new Vector3[n];
        var t1 = new int[n];
        for (int i = 0; i < n; i += 3) {
            v1[i] = v[t[i]];
            v1[i + 1] = v[t[i + 1]];
            v1[i + 2] = v[t[i + 2]];
            t1[i] = i;
            t1[i + 1] = i + 1;
            t1[i + 2] = i + 2;
        }
        m.vertices = v1;
        m.triangles = t1;
    }

    void TrianglesToLines(Mesh m) {
        var t = m.triangles;
        var v = m.vertices;
        var n = t.Length;
        var t1 = new int[n * 2];
        for (int i = 0; i < n; i += 3) {
            t1[2 * i] = t[i];
            t1[2 * i + 1] = t[i + 1];

            t1[2 * i + 2] = t[i + 1];
            t1[2 * i + 3] = t[i + 2];

            t1[2 * i + 4] = t[i + 2];
            t1[2 * i + 5] = t[i];
        }
        m.SetIndices(t1, MeshTopology.Lines, 0);
    }

    Vector3[] CreateVertices() {
        var v = new Vector3[24];

        v[0] = new Vector3(0, -1, 2);
        v[1] = new Vector3(1, 0, 2);
        v[2] = new Vector3(0, 1, 2);
        v[3] = new Vector3(-1, 0, 2);

        v[4] = new Vector3(0, -2, 1);
        v[5] = new Vector3(-1, -2, 0);
        v[6] = new Vector3(0, -2, -1);
        v[7] = new Vector3(1, -2, 0);

        v[8] = new Vector3(2, 0, 1);
        v[9] = new Vector3(2, -1, 0);
        v[10] = new Vector3(2, 0, -1);
        v[11] = new Vector3(2, 1, 0);

        v[12] = new Vector3(0, 2, 1);
        v[13] = new Vector3(1, 2, 0);
        v[14] = new Vector3(0, 2, -1);
        v[15] = new Vector3(-1, 2, 0);

        v[16] = new Vector3(-2, 0, 1);
        v[17] = new Vector3(-2, 1, 0);
        v[18] = new Vector3(-2, 0, -1);
        v[19] = new Vector3(-2, -1, 0);

        v[20] = new Vector3(0, -1, -2);
        v[21] = new Vector3(1, 0, -2);
        v[22] = new Vector3(0, 1, -2);
        v[23] = new Vector3(-1, 0, -2);

        return v;
    }

    int[] CreateLineStrip() {
        var t = new int[] {
            2, 3, 0, 1, 2,
            12, 15, 17, 16, 3,
            16, 17, 18, 19,
            16, 19, 5, 4, 0,
            4, 5, 6, 7,
            4, 7, 9, 8, 1,
            8, 9, 10, 11,
            8, 11, 13, 12,
            13, 14, 15,
            14, 22, 23, 18, 17, 18,
            23, 20, 6, 5, 6,
            20, 21, 10, 9, 10,
            21, 22, 14, 13
        };
        return t;
    }

    Mesh CreateWireframe() {
        var m = new Mesh();
        var v = CreateVertices();
        var t = CreateLineStrip();
        m.vertices = v;
        m.SetIndices(t, MeshTopology.LineStrip, 0);
        return m;
    }

    Mesh CreateMesh() {
        var m = new Mesh();

        var v = CreateVertices();

        var t = new int[] {
            2, 3, 0,
            2, 0, 1,

            0, 4, 7,
            0, 7, 1,
            1, 7, 9,
            1, 9, 8,

            1, 8, 11,
            1, 11, 13,
            1, 13, 2,
            2, 13, 12,

            2, 12, 15,
            2, 15, 3,
            3, 15, 17,
            3, 17, 16,

            3, 16, 19,
            3, 19, 5,
            3, 5, 0,
            0, 5, 4,

            22, 21, 20,
            22, 20, 23,

            7, 6, 20,
            21, 7, 20,
            9, 7, 21,
            10, 9, 21,

            11, 10, 21,
            13, 11, 21,
            22, 13, 21,
            14, 13, 22,

            15, 14, 22,
            23, 15, 22,
            17, 15, 23,
            18, 17, 23,

            19, 18, 23,
            5, 19,23,
            20, 5, 23,
            6, 5, 20,

            // the forgotten 4
            4, 5, 6,
            4, 6, 7,

            8, 9, 10,
            8, 10, 11,

            12, 13, 14,
            12, 14, 15,

            16, 17, 18,
            16, 18, 19
        };

        m.vertices = v;
        m.triangles = t;
        // TrianglesToLines(m);
        UnshareVertices(m);
        m.normals = ComputeSurfaceNormals(m);
        // m.SetIndices(t, MeshTopology.Lines, 0);

        return m;
    }

    GameObject CreateMeshObject() {
        var go = new GameObject("truncOcthed");
        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = CreateMesh();
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = meshMaterial;
        return go;
    }

    GameObject CreateLineRendererObject() {
        var go = new GameObject("truncOcthed");
        var lr = go.AddComponent<LineRenderer>();
        var v = CreateVertices();
        var t = CreateLineStrip();
        var p = new Vector3[t.Length];
        for (int i = 0; i < t.Length; i++) {
            p[i] = v[t[i]];
        }
		lr.useWorldSpace = false;
		lr.numPositions = t.Length;
        lr.SetPositions(p);
        lr.material = wireMaterial;
        //lr.startWidth = lr.endWidth = 0.04f;
        lr.widthMultiplier = 0.1f;
        
        return go;
    }

    void CreateGrid(int nx, int ny, int nz) {
        var mesh = CreateMeshObject();
        var wire = CreateLineRendererObject();
        for (int z = 0; z < nz; z++) {
            for (int x = 0; x < nx; x++) {
                for (int y = 0; y < ny; y++) {
                    float scramble = 2 * (z % 2 == 1 ? 1 : 0);
                    var p = new Vector3(x * 4 + scramble, y * 4 + scramble, z * 2);
                    var meshInst = GameObject.Instantiate(mesh, p, Quaternion.identity);
					GameObject.Instantiate(wire).transform.SetParent(meshInst.transform, false);
                    //.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                }
            }
        }
        GameObject.Destroy(mesh);
        GameObject.Destroy(wire);
        
    }

	// Use this for initialization
	void Start () {
        CreateGrid(32,4,8);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
