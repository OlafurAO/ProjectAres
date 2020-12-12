using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
<<<<<<< HEAD
using UnityEngine.Networking;


=======
using System.Text;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
>>>>>>> 40a8d9651fa4c234ee8dec9b4f773a5b30b783c4

public class HexMapEditor : MonoBehaviour {

	int activeTerrainTypeIndex;
	public HexGrid hexGrid;

	int activeElevation;
	int activeWaterLevel;

	int brushSize;

	bool applyElevation = true;
	bool applyWaterLevel = true;

	enum OptionalToggle {
		Ignore, Yes, No
	}

	OptionalToggle riverMode, roadMode;

	bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;

	public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetApplyWaterLevel (bool toggle) {
		applyWaterLevel = toggle;
	}
	public void SetWaterLevel (float level) {
		activeWaterLevel = (int)level;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void SetRiverMode (int mode) {
		riverMode = (OptionalToggle)mode;
	}

	public void SetRoadMode (int mode) {
		roadMode = (OptionalToggle)mode;
	}

	public void ShowUI (bool visible) {
		hexGrid.ShowUI(visible);
	}

	void Update () {
		if (
			Input.GetMouseButton(0) &&
			!EventSystem.current.IsPointerOverGameObject()
		) {
			HandleInput();
		}
		else {
			previousCell = null;
		}
	}

	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			HexCell currentCell = hexGrid.GetCell(hit.point);
			if (previousCell && previousCell != currentCell) {
				ValidateDrag(currentCell);
			}
			else {
				isDrag = false;
			}
			EditCells(currentCell);
			previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
	}

	void ValidateDrag (HexCell currentCell) {
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

	void EditCells (HexCell center) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;

		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
	}

	void EditCell (HexCell cell) {
		if (cell) {
			if (activeTerrainTypeIndex >= 0) {
				cell.TerrainTypeIndex = activeTerrainTypeIndex;
			}
			if (applyElevation) {
				cell.Elevation = activeElevation;
			}
			if (applyWaterLevel) {
				cell.WaterLevel = activeWaterLevel;
			}
			if (riverMode == OptionalToggle.No) {
				cell.RemoveRiver();
			}
			if (roadMode == OptionalToggle.No) {
				cell.RemoveRoads();
			}
			if (isDrag) {
				HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
				if (otherCell) {
					if (riverMode == OptionalToggle.Yes) {
						otherCell.SetOutgoingRiver(dragDirection);
					}
					if (roadMode == OptionalToggle.Yes) {
						otherCell.AddRoad(dragDirection);
					}
				}
			}
		}
	}
	public void SetTerrainTypeIndex (int index) {
		activeTerrainTypeIndex = index;
	}
	public void Save () {
		string path = Path.Combine(Application.streamingAssetsPath, "test.map");

		using (
			BinaryWriter writer =
				new BinaryWriter(File.Open(path, FileMode.Create))
		) {
			hexGrid.Save(writer);
		}
	}

<<<<<<< HEAD
	public void Load () {
		string path = Path.Combine(Application.streamingAssetsPath, "test.map");

		var unityWebRequest = UnityWebRequest.Get(path);
		unityWebRequest.SendWebRequest();
		var stuff = unityWebRequest.downloadHandler.data;
		
		using (
			BinaryReader reader =
				new BinaryReader(File.OpenRead(path))) {
					hexGrid.Load(reader);
=======
	IEnumerator LoadMap() {
		var loadingRequest = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, "test.map"));
		Debug.Log("Sending request");
		yield return loadingRequest.SendWebRequest();
		Debug.Log("Done Sending request");

		if(loadingRequest.isNetworkError || loadingRequest.isHttpError) {
			Debug.Log("OH NOOOOOOOOOOO");
			Debug.Log(loadingRequest.error);
		} else {
			Debug.Log("OH YEEEEEEEEEEEEEEEAH");
			Debug.Log(loadingRequest.downloadHandler.text);

			byte[] byteArray = loadingRequest.downloadHandler.data;
			MemoryStream stream = new MemoryStream(byteArray);
			using (BinaryReader reader = new BinaryReader(stream)) {
				hexGrid.Load(reader);
			}
>>>>>>> 40a8d9651fa4c234ee8dec9b4f773a5b30b783c4
		}
	}

	public void Load () {
		StartCoroutine(LoadMap());
	}
}