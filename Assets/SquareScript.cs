using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareScript : MonoBehaviour {

	private int num=0;
	private int col=0;
	public bool isFinish=false;
	public GameObject block;
	public GameObject text;
	private Material redM;
	private Material blueM;
	private Material greenM;
	private Material yellowM;

	// Use this for initialization
	void Start () {
		if(isFinish){
			num=100;
			col=0;
		}
	}

	// Update is called once per frame
	void Update () {

	}

	public void SetMaterials(Material r, Material b, Material g, Material y) {
		redM=r;
		blueM=b;
		greenM=g;
		yellowM=y;
	}

	public void SetNumber(int n) {
		num=n;
		text.GetComponent<TextMesh>().text=num.ToString();
        return;
	}

	public int GetNumber() {
		return num;
	}

	public void SetColour(int c) {
		col=c;
		if(isFinish)return;
        block.GetComponent<Renderer>().material = c == 0 ? redM : c == 1 ? blueM : c == 2 ? greenM : yellowM;
        return;
	}

	public int GetColour() {
		return col;
	}
}
