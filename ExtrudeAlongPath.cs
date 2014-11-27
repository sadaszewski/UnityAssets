/* Author: Stanislaw Adaszewski, 2014 *
 * Email: s.adaszewski@gmail.com      *
 * http://algoholic.eu                */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExtrudeAlongPath : MonoBehaviour {
	public Transform path;
	public Transform obj;
	public int steps = 1;
	public string stateName = "New Animation";

	Vector3 Clone(Vector3 v) {
		return new Vector3 (v.x, v.y, v.z);
	}

	struct Pair {
		public int a, b;
		public Pair(int a, int b) { this.a = a; this.b = b; }
		public Pair swapped() {
			return new Pair (this.b, this.a);
		}
	}

	Pair MakeEdge(int a, int b) {
		return new Pair (a > b ? a : b, a > b ? b : a);
	}



	// Use this for initialization
	void Start () {
		Animator anim = path.GetComponent<Animator>();
		//anim.StartPlayback();

		MeshFilter mf = obj.GetComponent<MeshFilter> ();
		Vector3[] verts = mf.mesh.vertices;
		int[] indices = mf.mesh.triangles;
		Vector2[] uv = mf.mesh.uv;

		Dictionary<Pair, int> edgeCnt = new Dictionary<Pair, int>();
		for (int i = 0; i < indices.Length; i += 3) {
			for (int k = 0; k < 3; k++) {
				int k1 = (k + 1) % 3;
				Pair edge = new Pair(indices[i + k], indices[i + k1]); // MakeEdge(indices[k], indices[k1]);
				Pair swapped = edge.swapped();
				if (edgeCnt.ContainsKey(edge)) {
					edgeCnt[edge]++;
				} else if (edgeCnt.ContainsKey(swapped)) {
					edgeCnt[swapped]++;
				} else {
					edgeCnt.Add(edge, 1);
				}
			}
		}
		Pair[] filtered = (from kvp in edgeCnt where kvp.Value == 1 select kvp.Key).ToArray<Pair>();

		int n_vert = verts.Length;
		int n_ind = indices.Length;
		int n_filt = filtered.Length;

		Vector3[] new_verts = new Vector3[n_vert * (steps + 1)];
		int[] new_indices = new int[n_filt * 6 * steps];
		Vector2[] new_uv = new Vector2[new_verts.Length];

		Debug.Log (string.Format ("n_vert: {0} n_ind: {1} new_verts.Length: {2} new_indices.Length: {3}", n_vert, n_ind, new_verts.Length, new_indices.Length));

		for (int i = 0; i < steps + 1; i++) {
			anim.Play (stateName, -1, (float) i / steps);
			anim.Update(0.0f);
			Vector3 p1 = Clone(path.position);
			anim.StopPlayback();
			anim.Play (stateName, -1, (float) (i + 1) / steps - 0.001f);
			anim.Update(0.0f);
			Vector3 p2 = Clone(path.position);
			anim.StopPlayback();
			//obj.forward = (p2 - p1).normalized;
			obj.position = p1;
			obj.LookAt(p2, obj.up);
			Debug.Log(p2 - p1);
			Matrix4x4 mat = obj.localToWorldMatrix;
			//Debug.Log(mat);

			for (int k = 0; k < n_vert; k++) {
				new_verts[i * n_vert + k] = mat.MultiplyPoint(verts[k]);
				new_uv[i * n_vert + k] = new Vector2(uv[k].x, (float) i / steps);
			}

			//Instantiate(obj);
		}

		/* for (int i = 0; i < new_indices.Length; i++) {
			new_indices[i] = i;
		}*/

		int ofs = 0;
		for (int i = 0; i < steps - 1; i++) {
			Debug.Log(string.Format("i: {0}", i));
			for (int k = 0; k < n_filt; k++) {
				int a = n_vert * i;
				int b = n_vert * (i + 1);

				new_indices[ofs++] = a + filtered[k].b;
				new_indices[ofs++] = a + filtered[k].a;
				new_indices[ofs++] = b + filtered[k].a;

				new_indices[ofs++] = a + filtered[k].b;
				new_indices[ofs++] = b + filtered[k].a;
				new_indices[ofs++] = b + filtered[k].b;
			}
		}

		Debug.Log (string.Format ("ofs: {0}", ofs));

		Mesh mesh = new Mesh ();
		mesh.vertices = new_verts;
		mesh.triangles = new_indices;
		//mesh.SetIndices (new_indices, MeshTopology.Points, 0);
		mesh.uv = new_uv;
		mesh.RecalculateNormals ();
		//mesh.SetTriangles (new_indices, 0);

		mf.mesh = mesh;

		//obj.GetComponent<MeshCollider> ().sharedMesh = mesh;
		//obj.position.Set (0, 0, 0);
		obj.position = new Vector3 (0, 0, 0);
		obj.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		obj.localRotation = Quaternion.identity;

		// Destroy (obj.gameObject);
		Destroy (path.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
