using UnityEngine;
using System.Collections;

public class SineCosineTable : MonoBehaviour {

	public float[,] sinTable;
	public float[,] cosTable;

	// Use this for initialization
	void Start () {
		int l = 0;
		sinTable = new float[2, 36000];
		cosTable = new float[2, 36000];
		for (float i = 0; i <= 360f; i += 0.01f) {
			sinTable [0, l] = i;
			cosTable [0, l] = i;
			sinTable [1, l] = Mathf.Sin (i);
			cosTable [1, l] = Mathf.Cos (i);
			l++;
			print (i);
		}
	}

}
