using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;

	public RectTransform uiRect;

	public HexGridChunk chunk;
	public bool isOccupied = false;
	public Canvas MoveCanvas; 
	public Canvas AttackCanvas;
	public Canvas DefenceCanvas;
	public Canvas CreateCanvas;
	public Canvas DeleteCanvas;
	public Canvas HexRangeCanvas;
	public Canvas HexHoverImageCanvas;
	public Vector3 ActualPosition;
	public bool BlueCanPlace;
	public bool RedCanPlace;
	public List<HexCell> HexRange = new List<HexCell>();
	public string team;
	public bool isKnight; 

	public Color Color {
		get {
			return HexMetrics.colors[terrainTypeIndex];
		}
	}
	public int TerrainTypeIndex {
		get {
			return terrainTypeIndex;
		}
		set {
			if (terrainTypeIndex != value) {
				terrainTypeIndex = value;
				Refresh();
			}
		}
	}

	public int Elevation {
		get {
			return elevation;
		}
		set {
			if (elevation == value) {
				return;
			}
			elevation = value;
			RefreshPosition();

			if (
				hasOutgoingRiver &&
				elevation < GetNeighbor(outgoingRiver).elevation
			) {
				RemoveOutgoingRiver();
			}
			if (
				hasIncomingRiver &&
				elevation > GetNeighbor(incomingRiver).elevation
			) {
				RemoveIncomingRiver();
			}

			for (int i = 0; i < roads.Length; i++) {
				if (roads[i] && GetElevationDifference((HexDirection)i) > 1) {
					SetRoad(i, false);
				}
			}

			Refresh();
		}
	}

	void RefreshPosition(){
			Vector3 position = transform.localPosition;
			position.y = elevation * HexMetrics.elevationStep;
			position.y +=
				(HexMetrics.SampleNoise(position).y * 2f - 1f) *
				HexMetrics.elevationPerturbStrength;
			transform.localPosition = position;

			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;
			ActualPosition = transform.position;
	}

	public int WaterLevel {
		get {
			return waterLevel;
		}
		set {
			if (waterLevel == value) {
				return;
			}
			waterLevel = value;
			Refresh();
		}
	}

	public bool IsUnderwater {
		get {
			return waterLevel > elevation;
		}
	}

	public bool HasIncomingRiver {
		get {
			return hasIncomingRiver;
		}
	}

	public bool HasOutgoingRiver {
		get {
			return hasOutgoingRiver;
		}
	}

	public bool HasRiver {
		get {
			return hasIncomingRiver || hasOutgoingRiver;
		}
	}

	public bool HasRiverBeginOrEnd {
		get {
			return hasIncomingRiver != hasOutgoingRiver;
		}
	}

	public HexDirection RiverBeginOrEndDirection {
		get {
			return hasIncomingRiver ? incomingRiver : outgoingRiver;
		}
	}

	public bool HasRoads {
		get {
			for (int i = 0; i < roads.Length; i++) {
				if (roads[i]) {
					return true;
				}
			}
			return false;
		}
	}

	public HexDirection IncomingRiver {
		get {
			return incomingRiver;
		}
	}

	public HexDirection OutgoingRiver {
		get {
			return outgoingRiver;
		}
	}

	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}


	public float StreamBedY {
		get {
			return
				(elevation + HexMetrics.streamBedElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public float RiverSurfaceY {
		get {
			return
				(elevation + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public float WaterSurfaceY {
		get {
			return
				(waterLevel + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public HexCell[] GetNeighbors(){
		return neighbors;
	}

	public void SetRange(){
		foreach (HexCell neigh in neighbors)
		{
			if(neigh != null){
				foreach (HexCell item in neigh.neighbors)
				{
						if (!HexRange.Contains(item)){
							HexRange.Add(item);
						}
				}
			}
		}
	}

	//Color color;
	int terrainTypeIndex;

	int elevation = int.MinValue;
	int waterLevel;

	bool hasIncomingRiver, hasOutgoingRiver;
	HexDirection incomingRiver, outgoingRiver;

	[SerializeField]
	public HexCell[] neighbors;

	[SerializeField]
	bool[] roads;

	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}

	public HexEdgeType GetEdgeType (HexCell otherCell) {
		return HexMetrics.GetEdgeType(
			elevation, otherCell.elevation
		);
	}

	public bool HasRiverThroughEdge (HexDirection direction) {
		return
			hasIncomingRiver && incomingRiver == direction ||
			hasOutgoingRiver && outgoingRiver == direction;
	}

	public void RemoveIncomingRiver () {
		if (!hasIncomingRiver) {
			return;
		}
		hasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(incomingRiver);
		neighbor.hasOutgoingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveOutgoingRiver () {
		if (!hasOutgoingRiver) {
			return;
		}
		hasOutgoingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(outgoingRiver);
		neighbor.hasIncomingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveRiver () {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}

	public void SetOutgoingRiver (HexDirection direction) {
		if (hasOutgoingRiver && outgoingRiver == direction) {
			return;
		}

		HexCell neighbor = GetNeighbor(direction);
		if (!neighbor || elevation < neighbor.elevation) {
			return;
		}

		RemoveOutgoingRiver();
		if (hasIncomingRiver && incomingRiver == direction) {
			RemoveIncomingRiver();
		}
		hasOutgoingRiver = true;
		outgoingRiver = direction;

		neighbor.RemoveIncomingRiver();
		neighbor.hasIncomingRiver = true;
		neighbor.incomingRiver = direction.Opposite();

		SetRoad((int)direction, false);
	}

	public bool HasRoadThroughEdge (HexDirection direction) {
		return roads[(int)direction];
	}

	public void AddRoad (HexDirection direction) {
		if (
			!roads[(int)direction] && !HasRiverThroughEdge(direction) &&
			GetElevationDifference(direction) <= 1
		) {
			SetRoad((int)direction, true);
		}
	}

	public void RemoveRoads () {
		for (int i = 0; i < neighbors.Length; i++) {
			if (roads[i]) {
				SetRoad(i, false);
			}
		}
	}

	public int GetElevationDifference (HexDirection direction) {
		int difference = elevation - GetNeighbor(direction).elevation;
		return difference >= 0 ? difference : -difference;
	}

	void SetRoad (int index, bool state) {
		roads[index] = state;
		neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
		neighbors[index].RefreshSelfOnly();
		RefreshSelfOnly();
	}

	void Refresh () {
		if (chunk) {
			chunk.Refresh();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) {
					neighbor.chunk.Refresh();
				}
			}
		}
	}

	void RefreshSelfOnly () {
		chunk.Refresh();
	}
	//TODO save bool value of stuff 
	public void Save (BinaryWriter writer) {
		writer.Write(terrainTypeIndex);
		writer.Write(elevation);
		writer.Write(waterLevel);
		writer.Write(BlueCanPlace);
		writer.Write(RedCanPlace);
		/*writer.Write(urbanLevel);
		writer.Write(farmLevel);
		writer.Write(plantLevel);
		writer.Write(specialIndex);
		writer.Write(walled);

		writer.Write(hasIncomingRiver);
		writer.Write((int)incomingRiver);

		writer.Write(hasOutgoingRiver);
		writer.Write((int)outgoingRiver);

		for (int i = 0; i < roads.Length; i++) {
			writer.Write(roads[i]);
		}*/
	}

	public void Load (BinaryReader reader) {
		terrainTypeIndex = reader.ReadInt32();
		elevation = reader.ReadInt32();
		RefreshPosition();
		waterLevel = reader.ReadInt32();

		
		BlueCanPlace = reader.ReadBoolean();
		RedCanPlace = reader.ReadBoolean();
		
		/*urbanLevel = reader.ReadInt32();
		farmLevel = reader.ReadInt32();
		plantLevel = reader.ReadInt32();
		specialIndex = reader.ReadInt32();
		walled = reader.ReadBoolean();

		hasIncomingRiver = reader.ReadBoolean();
		incomingRiver = (HexDirection)reader.ReadInt32();

		hasOutgoingRiver = reader.ReadBoolean();
		outgoingRiver = (HexDirection)reader.ReadInt32();

		for (int i = 0; i < roads.Length; i++) {
			roads[i] = reader.ReadBoolean();
		}
		*/
	}

	public void HoverHighlight() {

	}
	
	//show range on map (walking range)
	public void ShowWalkRange(){
		foreach (HexCell neigh in HexRange)
		{
				neigh.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
				neigh.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow);
				if(neigh.isOccupied){
					if(team == neigh.team){
						neigh.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
					}else if (neigh.team == ""){
						return;
					}else {
						if (!isKnight){
							neigh.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
						}else{
							neigh.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
							
						}
					}
				}
		}
		if(isKnight){
			foreach (HexCell unit in neighbors)
			{
					if(unit.isOccupied && unit.team != team && unit.team != ""){
						unit.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
						unit.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
					}
			}
		}
	}
	
	//no show range on map (walking range)
	public void NoShowWalkRange(){
		foreach (HexCell neigh in HexRange)
		{
				neigh.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
		}
	}
	//makes a red hexagon appear beneath a enemy that you can attack 
	public void ShowAttackRange(string type){
		if(type == "knight"){
			foreach (HexCell unit in neighbors)
			{
				if(unit.isOccupied && unit.team != team){
					unit.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
					unit.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
				}
			}
		}else{
			foreach (HexCell unit in HexRange)
			{
				if(unit.isOccupied && unit.team != team && unit.team != ""){
					unit.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
					unit.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
				}
			}

		}
	}
	//make attack hexagon dissapear
	public void NoShowAttackRange(string type){
		foreach (HexCell unit in HexRange)
		{
			if(unit.isOccupied && unit.team != team){
				unit.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
				unit.HexRangeCanvas.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow);
			}
		}
	}

	
	public bool CanAttack(HexCell victimLocation){
		if(isKnight){
			foreach (var units in neighbors)
			{
					if(units == victimLocation){return true;}
			}
			return false;
		}else{
			if(HexRange.Contains(victimLocation)){return true;}
			return false;
		}
	}
}