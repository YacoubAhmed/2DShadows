using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowRays : MonoBehaviour {

	public float viewRadius;
	public List<Vector2> allPoints, hitPoints;
	public float offset;
	private float accuracyDegree;
	[Range(0.5f, 100f)]
	public float accuracies;
	public MeshFilter mf;
	Mesh shadowMesh;
	SineCosineTable sct;

	void Start () {
		if (accuracies == 0f) {accuracies = 10f;}
		sct = this.gameObject.GetComponent<SineCosineTable>();
		allPoints = new List<Vector2> ();
		mf = GetComponent<MeshFilter> ();
		shadowMesh = new Mesh ();
		shadowMesh.name = "Shadow Mesh";
		mf.mesh = shadowMesh;
	}

	//TODO do not draw rays to every single object, only to ones within a radius, perhaps using a Physics.OverlapCircle method

	void Update () {
		accuracyDegree = 1/accuracies;
		GetPoints();

		OffsetPoints();

		DrawCirclePoints();

		CastRays ();

		DrawMesh ();
	}

	void CastRays() {
		//First clear the hitpoint list
		hitPoints.Clear ();

		//Second sort points based on their angle made with the x positive axis.
		allPoints.Sort(SortByAngle);

		//Now raycast to each point, and if we do not hit something then just add a point at the end of the ray
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

	void DrawCirclePoints () {
		//This just adds a bunch of points in a circle around the object based on its accuracy value, not needed for the shadows but just rounds out the edges
		for (float i = 0; i <= 360f; i += (accuracyDegree)*51) {
			float sin = sct.sinTable[Mathf.RoundToInt(i * 100)];
			float cos = sct.cosTable[Mathf.RoundToInt(i * 100)];
			allPoints.Add ((Vector2)transform.position + new Vector2 (sin, cos));
		}
	}

	void DrawMesh() {
		//Here we use the ordered list of points from their angles to create a mesh around the object.
		int vertexNumber = hitPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexNumber];
		int[] triangles = new int[(vertexNumber - 1) * 3];

		vertices [0] = Vector3.zero;
		for (int i = 0; i < vertexNumber - 1; i++) {
			vertices [i + 1] = transform.InverseTransformPoint (hitPoints [i]);

			if (i < vertexNumber - 2) {
				triangles [i * 3] = 0;
				triangles [i * 3 + 1] = i + 1;
				triangles [i * 3 + 2] = i + 2;
			}
		}
		//Closing off the mesh
		triangles [(vertexNumber - 2) * 3] = 0;
		triangles [(vertexNumber - 2) * 3 + 1] = vertexNumber-1;
		triangles [(vertexNumber - 2) * 3 + 2] = 1;

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

	void GetPoints() {
		allPoints.Clear ();
		//TODO make this a bit more efficient, perhaps use overlapsphere?

		foreach(Collider2D col in Physics2D.OverlapCircleAll(transform.position, viewRadius)) {
			if(col.
		}
		PolygonCollider2D[] polys = GameObject.FindObjectsOfType<PolygonCollider2D> ();
		BoxCollider2D[] boxes = GameObject.FindObjectsOfType<BoxCollider2D> ();

		foreach (PolygonCollider2D poly in polys) {
			foreach (Vector2 point in poly.points) {
				Vector2 addPoint = point + (Vector2)poly.transform.position + poly.offset;
				allPoints.Add (addPoint);
			}
		}

		foreach (BoxCollider2D box in boxes) {
			Vector2 boxPos = box.gameObject.transform.position;
			Vector2 boxScale = box.size;
			allPoints.Add( boxPos + box.offset + 0.5f * new Vector2 (boxScale.x, boxScale.y) );
			allPoints.Add( boxPos + box.offset + 0.5f * new Vector2 (boxScale.x, -boxScale.y) );
			allPoints.Add( boxPos + box.offset + 0.5f * new Vector2 (-boxScale.x, -boxScale.y) );
			allPoints.Add( boxPos + box.offset + 0.5f * new Vector2 (-boxScale.x, boxScale.y) );
		}

	}

	void OffsetPoints() {
		List<Vector2> offsetPoints = new List<Vector2>();
		foreach(Vector2 addPoint in allPoints) {
			float sin = sct.sinTable[Mathf.RoundToInt(0.5f * GetAngle(addPoint) * 100f)];
			float cos = sct.cosTable[Mathf.RoundToInt(0.5f * GetAngle(addPoint) * 100f)];
			offsetPoints.Add(addPoint + new Vector2(sin, cos)*offset);
			offsetPoints.Add(addPoint + new Vector2(-sin, -cos)*offset);
		}
		foreach(Vector2 offsetPoint in offsetPoints) {
			allPoints.Add(offsetPoint);
		}
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
