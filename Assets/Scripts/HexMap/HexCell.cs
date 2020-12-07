using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public Color color;
    public bool isOccupied = false;
    public Canvas MoveCanvas; 
    public Canvas AttackCanvas;
    public Canvas DefenceCanvas;
    public Canvas CreateCanvas;

}
