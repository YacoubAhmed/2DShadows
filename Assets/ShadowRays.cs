using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowRays : MonoBehaviour {

	public float viewRadius;
	public List<Vector2> allPoints, hitPoints;
	public float accuracyDegree, offset;
	public MeshFilter mf;
	Mesh shadowMesh;

	void Start () {
		allPoints = new List<Vector2> ();
		mf = GetComponent<MeshFilter> ();
		shadowMesh = new Mesh ();
		shadowMesh.name = "Shadow Mesh";
		mf.mesh = shadowMesh;
	}

	void Update () {
		
		PolygonCollider2D[] polys = GetColliders ();
		allPoints.Clear ();
		hitPoints.Clear ();
		for (float i = 0; i <= (2) * Mathf.PI; i += accuracyDegree) {
			allPoints.Add ((Vector2)transform.position + new Vector2 (Mathf.Sin (i), Mathf.Cos (i)));
		}

		foreach (PolygonCollider2D poly in polys) {
			foreach (Vector2 point in poly.points) {
				Vector2 addPoint = point + (Vector2)poly.transform.position + poly.offset;
				allPoints.Add(addPoint + new Vector2(Mathf.Sin(0.5f* GetAngle(addPoint)*Mathf.Deg2Rad), Mathf.Cos(0.5f* GetAngle(addPoint)*Mathf.Deg2Rad))*accuracyDegree);
				allPoints.Add(addPoint + new Vector2(-Mathf.Sin(0.5f* GetAngle(addPoint)*Mathf.Deg2Rad), -Mathf.Cos(0.5f* GetAngle(addPoint)*Mathf.Deg2Rad))*accuracyDegree);
				allPoints.Add (addPoint);
			}
		}
		CastRays ();

		DrawMesh ();
	}

	void CastRays() {

		//second sort points based on their angle made with the x positive axis.
		allPoints.Sort(SortByAngle);

		//now raycast to each point and see what gwans
		foreach (Vector2 point in allPoints) {
			Ray2D ray = new Ray2D (transform.position, (point - (Vector2)transform.position).normalized);
			Debug.DrawRay (transform.position, ray.direction * viewRadius);
			if(Physics2D.Raycast(ray.origin, ray.direction, viewRadius)) {
				hitPoints.Add (Physics2D.Raycast (ray.origin, ray.direction, viewRadius).point);
			} else {
				hitPoints.Add ((Vector2)transform.position + ray.direction * viewRadius);
			}
		}
	}

	void DrawMesh() {
		int vertexCount = hitPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount - 1) * 3];

		vertices [0] = Vector3.zero;
		for (int i = 0; i < vertexCount - 1; i++) {
			vertices [i + 1] = transform.InverseTransformPoint (hitPoints [i]);
			//vertices[i + 1] = hitPoints[i];

			if (i < vertexCount - 2) {
				triangles [i * 3] = 0;
				triangles [i * 3 + 1] = i + 1;
				triangles [i * 3 + 2] = i + 2;
			}
		}

		triangles [(vertexCount - 1) * 3 - 3] = 0;
		triangles [(vertexCount - 1) * 3 - 2] = vertexCount-1;
		triangles [(vertexCount - 1) * 3 - 1] = 1;

		shadowMesh.Clear ();
		shadowMesh.vertices = vertices;
		shadowMesh.triangles = triangles;
		shadowMesh.RecalculateNormals ();
	}

	int SortByAngle(Vector2 vectA, Vector2 vectB) {
		if (GetAngle(vectA) > GetAngle(vectB)) {
			return 1;
		} else if (GetAngle(vectA) < GetAngle(vectB)) {
			return -1;
		}
		return 0;
	}

	float GetAngle(Vector2 vect) {
		float returnAngle = 0f;
		Vector2 relativeVect = vect - (Vector2)this.transform.position;
		//print (relativeVect);
		if (relativeVect.x == 0) {
			if (relativeVect.y > 0f) {
				returnAngle = 90f;
				return returnAngle;
			} else {
				returnAngle = 270f;
				return returnAngle;
			}
		} if (relativeVect.y == 0) {
			if (relativeVect.x > 0f) {
				returnAngle = 0f;
				return returnAngle;
			} else {
				returnAngle = 180f;
				return returnAngle;
			}
		}
		float tan = Mathf.Atan2 (Mathf.Abs(relativeVect.y), Mathf.Abs(relativeVect.x))*Mathf.Rad2Deg;
		if (relativeVect.x < 0f) {
			if (relativeVect.y < 0f) {
				// x -, y-
				returnAngle = 180 + tan;
			} else if (relativeVect.y > 0f) {
				// x -, y+
				returnAngle = 180 - tan;
			}
		} else if (relativeVect.x > 0f) {
			if (relativeVect.y < 0f) {
				// x +, y-
				returnAngle = 360 - tan;
			} else if (relativeVect.y > 0f) {
				// x +, y+
				returnAngle = tan;
			}
		}
		return returnAngle;
	}

	Vector2 GetDirFromAngle(float angle) {
		return new Vector2 (Mathf.Cos (angle * Mathf.Deg2Rad), Mathf.Sin (angle * Mathf.Deg2Rad));
	}

	PolygonCollider2D[] GetColliders() {
		//TODO make this a bit more efficient, perhaps use overlapsphere?
		PolygonCollider2D[] polys = GameObject.FindObjectsOfType<PolygonCollider2D> ();
		BoxCollider2D[] boxes = GameObject.FindObjectsOfType<BoxCollider2D> ();
		List<PolygonCollider2D> polyList = new List<PolygonCollider2D>();
		foreach(PolygonCollider2D poly in polys) {
			polyList.Add (poly);
		}

		/*foreach (BoxCollider2D box in boxes) {
			PolygonCollider2D polyTemp = new PolygonCollider2D ();
			Vector2 boxPos = box.gameObject.transform.position;
			Vector2 boxScale = box.gameObject.transform.lossyScale;

			polyTemp.points = new Vector2[4] {
				boxPos + new Vector2 (boxScale.x, boxScale.y),
				boxPos + new Vector2 (boxScale.x, -boxScale.y),
				boxPos + new Vector2 (-boxScale.x, -boxScale.y),
				boxPos + new Vector2 (-boxScale.x, boxScale.y)
			};

			polyList.Add (polyTemp);
		}
		*/

		return polyList.ToArray();
	}

	void OnDrawGizmos() {
		float i = 0;
		foreach (Vector2 point in allPoints) {
			//i = GetAngle (point - (Vector2)transform.position);
			//Gizmos.DrawCube (point, Vector3.one * i * Mathf.Deg2Rad);
			Gizmos.DrawCube (point, Vector3.one/15);
		}

		foreach (Vector2 point in hitPoints) {
			//i = GetAngle (point - (Vector2)transform.position);
			//Gizmos.DrawCube (point, Vector3.one * i * Mathf.Deg2Rad);
			Gizmos.color = Color.red;
			Gizmos.DrawCube (point, Vector3.one/10);
		}
	}
}
