using UnityEngine;
using System.Collections;

public class SineCosineTable : MonoBehaviour {


	public float[] sinTable, cosTable;
	//note all in degrees

	void Start () {
		int l = 0;
		sinTable = new float[36000];
		cosTable = new float[36000];
		for (float i = 0; i <= 360f; i += 0.01f) {
			sinTable [l] = Mathf.Sin (i*Mathf.Deg2Rad);
			cosTable [l] = Mathf.Cos (i*Mathf.Deg2Rad);
			l++;
		}
	}
}
