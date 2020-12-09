using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlacingUnits : MonoBehaviour {
    public Canvas PlayerOneCanvas;
    public Canvas PlayerTwoCanvas; 
    public Image BackgroundImage;
    
    //temporary list of all units and color ("knihtObject", "blue")
    public List<GameObject> tempUnits;
    //stores the location clicked where you want to create a unit
    private Vector3 CreatedLocation;
    private HexCell SelectedCell;

    //stores a canvas of the newest button visable (so we can disable it if needed (click on 2 tiles at onece and 1 one dissapears))
    private Canvas currButtonCanvas;
    public HexGrid grid;
    HexCell index; 

    //witch player is placing units? player one or two?
    private int player = 1;
    public List<GameObject> units;

    bool placing = true;

    //health, armor, gold and such connected to the knight, wizard, archer controller (atleast the ones in "tempUnit")
  /* //náði ekki að láta það virka svo ætla bara að hard kóða allavona núna 
    public TMPro.TextMeshProGUI KnightHealth;
    public TMPro.TextMeshProGUI KnightArmor;
    public TMPro.TextMeshProGUI KnightGold;
    public TMPro.TextMeshProGUI KnightAttack;
    public TMPro.TextMeshProGUI WizardHealth;
    public TMPro.TextMeshProGUI WizardArmor;
    public TMPro.TextMeshProGUI WizardGold;
    public TMPro.TextMeshProGUI WizardAttack;
    public TMPro.TextMeshProGUI ArcherHealth;
    public TMPro.TextMeshProGUI ArcherArmor;
    public TMPro.TextMeshProGUI ArcherGold;
    public TMPro.TextMeshProGUI ArcherAttack;
    */
    void Awaken(){
        PlayerTwoCanvas.enabled = false;
        //stendur fyrir ofan en hard kóða þetta bara 
        /* 
        KnightHealth.text = tempUnits[0].health;
        KnightArmor.text = tempUnits[0].armor;
        KnightAttack.text = tempUnits[0].baseDamage;
        KnightGold.text = tempUnits[0].goldCost;
        
        WizardHealth.text = tempUnits[2].health;
        WizardArmor.text = tempUnits[2].armor;
        WizardAttack.text = tempUnits[2].baseDamage;
        WizardGold.text = tempUnits[2].goldCost;
        
        ArcherHealth.text = tempUnits[1].health;
        ArcherArmor.text = tempUnits[1].armor;
        ArcherAttack.text = tempUnits[1].baseDamage;
        ArcherGold.text = tempUnits[1].goldCost;
        */
    }
    void Update() {
        if(!placing) {
            print("e");
            return;
        }

        if(currButtonCanvas != null){
            grid.MoveButton(currButtonCanvas);
        }
        // Is player hovering over an object?
        Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rhInfo;
        bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);

    

        // Check for left mouse button click
        if(Input.GetMouseButtonDown(0)) {    
            // Did player click on a UI button? if so, don't do anything else
            if(EventSystem.current.IsPointerOverGameObject()) {
                print("thingy");
                return;
            }


            // Did player click on a unit? should it be removed? or moved? 
            if(didHit && (rhInfo.collider.gameObject.tag.Contains("Knight") || rhInfo.collider.gameObject.tag.Contains("Archer") 
              || rhInfo.collider.gameObject.tag.Contains("Wizard") || rhInfo.collider.gameObject.tag.Contains("Defeated"))){
                  //if any button is visable, disable it (so there wont be millions of buttons everywhere)
                  if(SelectedCell != null){
                      grid.DisableButton(currButtonCanvas);
                  }
                  index = grid.DeleteCell(rhInfo.point);
                  currButtonCanvas = index.DeleteCanvas;
                  SelectedCell = index;
                  return;
            } 
            if(Physics.Raycast(toMouse, out rhInfo, 1000.0f)){
                if(SelectedCell != null){
                    grid.DisableButton(currButtonCanvas);
                }
                
                index = grid.CreateUnitCell(rhInfo.point);
                currButtonCanvas = index.CreateCanvas;
                SelectedCell = index;
            }
        };

    }
    public void CreateUnit(string type){
        if(player == 1){
            if(type == "Knight"){
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[0]);
                FinalUnit.tag = "KnightBlue";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }else if(type == "Archer"){
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[1]);
                FinalUnit.tag = "ArcherBlue";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }else if(type == "Wizard"){
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[2]);
                FinalUnit.tag = "WizardBlue";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }
        }else if(player == 2){
            if(type == "Knight"){
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[3]);
                FinalUnit.tag = "KnightRed";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }else if(type == "Archer"){
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[4]);
                FinalUnit.tag = "ArcherRed";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }else if (type == "Wizard"){
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[5]);
                FinalUnit.tag = "WizardRed";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }
        }

    }

    public void PlayerOneFinished(){
        PlayerOneCanvas.enabled = false; 
        player = 2; 
        PlayerTwoCanvas.enabled = true;
        units = new List<GameObject>();
        BackgroundImage.color = UnityEngine.Color.red;
    }
    public void PlayerTwoFinished(){
        print(placing);
        PlayerTwoCanvas.enabled = false;
        BackgroundImage.enabled = (false);
        placing = false;
        print(placing);
    }

    public void DeleteUnit(){
        foreach (GameObject unit in units)
        {
            if(unit.transform.position == index.transform.position){
                unit.SetActive(false);
                index.isOccupied = false;
            }
        }
    }
    
    
}



