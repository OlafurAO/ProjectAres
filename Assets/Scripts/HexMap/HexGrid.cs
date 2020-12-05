﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;

	public int width = 6;
	public int height = 6;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;
	HexMesh hexMesh;

	public Canvas canvas1;

	//Canvas gridCanvas;

	HexCell[] cells;

	void Awake () {
		//gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();
		cells = new HexCell[height * width];

		for (int z = 0, i = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z, i++);
			}
		}
	}



	void Start () {
	hexMesh.Triangulate(cells);
	}
	
	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z/2) * (HexMeetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMeetrics.outerRadius * 1.5f);
		cellPrefab.isOccupied = false;
		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.color = defaultColor;
		Canvas temp = Instantiate<Canvas>(canvas1);
		temp.transform.SetParent(cell.transform, false);
		cell.canvas = temp;
		//Button butt = cell.canvas.transform.GetComponent<Button>();
		//cell.buttControl = butt.GetComponent<ButtonController>();
		//print(butt.gameObject);
		cell.canvas.enabled = false;
		
		//code Stefan might re-use later on 
		/*MeshRenderer meshRenderer = temp.GetComponent<MeshRenderer>();
		meshRenderer.enabled = false;
		*/
		
		/*
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeperateLines();
		*/
	}

	public HexCell TouchCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.color = touchedColor;
		hexMesh.Triangulate(cells);
		cell.canvas.enabled = true;
		
		//testing canvas so stefan can place button on top of canvas
		//cell.buttControl.EnableButton();
		
		//code Stefan might re-use later on 
		/*MeshRenderer meshRenderer = temp.GetComponent<MeshRenderer>();
		meshRenderer.enabled = false;
		*/
		return cell; 
	}

	
	void Update () {
	}
	
	public void OccupyCell(HexCell cell){
		int index = cell.coordinates.X + cell.coordinates.Z * width + cell.coordinates.Z / 2;
		cells[index].isOccupied = true; 
	}
	public void UnOccupyCell(HexCell cell){
		int index = cell.coordinates.X + cell.coordinates.Z * width + cell.coordinates.Z / 2;
		cells[index].isOccupied = false;

	}

/*	public void canvasthingy(HexCell cell){
		cell.canvas.RectTransform.PosX = cell.position.X; 

	}*/

}

