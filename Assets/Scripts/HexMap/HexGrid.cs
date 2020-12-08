using System.Collections.Generic;
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

	public Canvas MoveCanvas1;
	public Canvas AttackCanvas1;
	public Canvas DefenceCanvas1;
	public Canvas CreateUnitCanvas1;

	public Camera camera;

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
		Canvas temp = Instantiate<Canvas>(MoveCanvas1);
		Canvas temp2 = Instantiate<Canvas>(AttackCanvas1);
		Canvas temp3 = Instantiate<Canvas>(DefenceCanvas1);
		Canvas temp4 = Instantiate<Canvas>(CreateUnitCanvas1);
		temp.transform.SetParent(cell.transform, false);
		temp2.transform.SetParent(cell.transform, false);
		temp3.transform.SetParent(cell.transform, false);
		temp4.transform.SetParent(cell.transform, false);
		
		cell.MoveCanvas = temp;
		cell.AttackCanvas = temp2;
		cell.DefenceCanvas = temp3;
		cell.CreateCanvas = temp4;
		
		cell.MoveCanvas.enabled = false;
		cell.AttackCanvas.enabled = false;
		cell.DefenceCanvas.enabled = false;
		cell.CreateCanvas.enabled = false;
		
		
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
		
		return cell; 
	}

	public HexCell MovementCell(Vector3 position){
		HexCell currCell = getCell(position);
		currCell.MoveCanvas.transform.LookAt(camera.transform.position);
		currCell.MoveCanvas.enabled = true;
		return currCell;
	}


	public HexCell CreateUnitCell(Vector3 position){
		HexCell currCell = getCell(position);
		currCell.CreateCanvas.transform.LookAt(camera.transform.position);
		currCell.CreateCanvas.enabled = true;
		return currCell;
	}

	public HexCell AttackCell(Vector3 position){
		HexCell currCell = getCell( position);
		currCell.AttackCanvas.transform.LookAt(camera.transform.position);
		currCell.AttackCanvas.enabled = true;
		return currCell;
	}



	public HexCell DefenceCell(Vector3 position){
		HexCell currCell = getCell( position);
		currCell.DefenceCanvas.transform.LookAt(camera.transform.position);
		currCell.DefenceCanvas.enabled = true;
		return currCell;
	}


	public HexCell getCell(Vector3 position){
		//the same as "touchCell" but different name 
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.color = touchedColor;
		hexMesh.Triangulate(cells);
		
		return cell; 
		
	}
//make button face camera 
	public void MoveButton(Canvas ButtonCanvas){
		ButtonCanvas.transform.LookAt(camera.transform.position);
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

	//button calls this to disable the button in the game
    public void DisableButton(Canvas canvas){
        canvas.enabled = false;
    }

/*	public void canvasthingy(HexCell cell){
		cell.canvas.RectTransform.PosX = cell.position.X; 

	}*/

}

