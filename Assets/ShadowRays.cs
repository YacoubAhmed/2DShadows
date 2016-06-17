using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowRays : MonoBehaviour {

	public float viewRadius;
	public List<Vector2> allPoints;

	void Start () {
		allPoints = new List<Vector2> ();
	}

	void Update () {
		PolygonCollider2D[] polys = GetColliders ();
		allPoints.Clear ();
		allPoints.Add ((Vector2)transform.position + Vector2.up * viewRadius);
		allPoints.Add ((Vector2)transform.position + Vector2.right * viewRadius);
		allPoints.Add ((Vector2)transform.position + Vector2.down * viewRadius);
		allPoints.Add ((Vector2)transform.position + Vector2.left * viewRadius);

		foreach (PolygonCollider2D poly in polys) {
			foreach (Vector2 point in poly.points) {
				allPoints.Add (point + (Vector2)poly.transform.position + poly.offset);
			}
		}
		CastRays ();
	}

	void CastRays() {
		//first sort points based on their angle made with the x positive axis.
		allPoints.Sort(SortByAngle);

		//now raycast to each point and see what gwans
		foreach (Vector2 point in allPoints) {
			Debug.DrawRay (transform.position, (point - (Vector2)transform.position).normalized * viewRadius);
		}
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
			Gizmos.DrawCube (point, Vector3.one/4);
		}
	}
}
