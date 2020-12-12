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
    private int gold = 1000; 
    
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
   //náði ekki að láta það virka svo ætla bara að hard kóða allavona núna 
    public TMPro.TextMeshProUGUI KnightHealth;
    public TMPro.TextMeshProUGUI KnightArmor;
    public TMPro.TextMeshProUGUI KnightGold;
    public TMPro.TextMeshProUGUI KnightAttack;
    public TMPro.TextMeshProUGUI WizardHealth;
    public TMPro.TextMeshProUGUI WizardArmor;
    public TMPro.TextMeshProUGUI WizardGold;
    public TMPro.TextMeshProUGUI WizardAttack;
    public TMPro.TextMeshProUGUI ArcherHealth;
    public TMPro.TextMeshProUGUI ArcherArmor;
    public TMPro.TextMeshProUGUI ArcherGold;
    public TMPro.TextMeshProUGUI ArcherAttack;
    private TMPro.TextMeshProUGUI goldText;
    public TMPro.TextMeshProUGUI goldText1;
    public TMPro.TextMeshProUGUI goldText2;

    //allt shittið fyrir ofan fyrir utan fyrir player tvö
    public TMPro.TextMeshProUGUI KnightHealth2;
    public TMPro.TextMeshProUGUI KnightArmor2;
    public TMPro.TextMeshProUGUI KnightGold2;
    public TMPro.TextMeshProUGUI KnightAttack2;
    public TMPro.TextMeshProUGUI WizardHealth2;
    public TMPro.TextMeshProUGUI WizardArmor2;
    public TMPro.TextMeshProUGUI WizardGold2;
    public TMPro.TextMeshProUGUI WizardAttack2;
    public TMPro.TextMeshProUGUI ArcherHealth2;
    public TMPro.TextMeshProUGUI ArcherArmor2;
    public TMPro.TextMeshProUGUI ArcherGold2;
    public TMPro.TextMeshProUGUI ArcherAttack2;

    
    void Start(){
        PlayerTwoCanvas.enabled = false;
        goldText = goldText1;
        //stendur fyrir ofan en hard kóða þetta bara 
        //blue
        KnightHealth.text = tempUnits[0].GetComponent<KnightController>().health.ToString();
        KnightArmor.text = tempUnits[0].GetComponent<KnightController>().armor.ToString();
        KnightAttack.text = tempUnits[0].GetComponent<KnightController>().baseDamage.ToString();
        KnightGold.text = tempUnits[0].GetComponent<KnightController>().goldCost.ToString();
        
        WizardHealth.text = tempUnits[2].GetComponent<WizardController>().health.ToString();
        WizardArmor.text = tempUnits[2].GetComponent<WizardController>().armor.ToString();
        WizardAttack.text = tempUnits[2].GetComponent<WizardController>().baseDamage.ToString();
        WizardGold.text = tempUnits[2].GetComponent<WizardController>().goldCost.ToString();
        
        ArcherHealth.text = tempUnits[1].GetComponent<ArcherController>().health.ToString();
        ArcherArmor.text = tempUnits[1].GetComponent<ArcherController>().armor.ToString();
        ArcherAttack.text = tempUnits[1].GetComponent<ArcherController>().baseDamage.ToString();
        ArcherGold.text = tempUnits[1].GetComponent<ArcherController>().goldCost.ToString();


        
        //red
        KnightHealth2.text = tempUnits[3].GetComponent<KnightController>().health.ToString();
        KnightArmor2.text = tempUnits[3].GetComponent<KnightController>().armor.ToString();
        KnightAttack2.text = tempUnits[3].GetComponent<KnightController>().baseDamage.ToString();
        KnightGold2.text = tempUnits[3].GetComponent<KnightController>().goldCost.ToString();
        
        WizardHealth2.text = tempUnits[5].GetComponent<WizardController>().health.ToString();
        WizardArmor2.text = tempUnits[5].GetComponent<WizardController>().armor.ToString();
        WizardAttack2.text = tempUnits[5].GetComponent<WizardController>().baseDamage.ToString();
        WizardGold2.text = tempUnits[5].GetComponent<WizardController>().goldCost.ToString();
        
        ArcherHealth2.text = tempUnits[4].GetComponent<ArcherController>().health.ToString();
        ArcherArmor2.text = tempUnits[4].GetComponent<ArcherController>().armor.ToString();
        ArcherAttack2.text = tempUnits[4].GetComponent<ArcherController>().baseDamage.ToString();
        ArcherGold2.text = tempUnits[4].GetComponent<ArcherController>().goldCost.ToString();
        
    }       

    void Update(){
        goldText.text = gold.ToString();
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
        
        // Check for right mouse button click
        if(Input.GetMouseButtonDown(1)) {    
            //if move unit, display button that if pressed runs "move unit"
            if(Physics.Raycast(toMouse, out rhInfo, 1000.0f)){
                if(BackgroundImage.color == GameManager.instance.blueColor){
                    grid.PlacementBlueTeam(rhInfo.point);
                }else{
                    grid.PlacementRedTeam(rhInfo.point);
                }
            }
        }

    }
    public void CreateUnit(string type){
        FindObjectOfType<AudioManager>().Play("menu_button_click", 0.0f);
        if(player == 1){
            if(type == "Knight"){
                if(gold < tempUnits[0].GetComponent<KnightController>().goldCost){
                    print("to expensive");
                    return;
                }else{
                    gold -= tempUnits[0].GetComponent<KnightController>().goldCost;
                }
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[0]);
                FinalUnit.tag = "KnightBlue";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }else if(type == "Archer"){
                if(gold < tempUnits[1].GetComponent<ArcherController>().goldCost){
                    print("to expensive");
                    return;
                }else{
                    gold -= tempUnits[1].GetComponent<ArcherController>().goldCost;
                }
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[1]);
                FinalUnit.tag = "ArcherBlue";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }else if(type == "Wizard"){
                if(gold < tempUnits[2].GetComponent<WizardController>().goldCost){
                    print("to expensive");
                    return;
                }else{
                    gold -= tempUnits[2].GetComponent<WizardController>().goldCost;
                }
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
                if(gold < tempUnits[0].GetComponent<KnightController>().goldCost){
                    print("to expensive");
                    return;
                }else{
                    gold -= tempUnits[0].GetComponent<KnightController>().goldCost;
                }
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[3]);
                FinalUnit.tag = "KnightRed";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }else if(type == "Archer"){
                if(gold < tempUnits[1].GetComponent<ArcherController>().goldCost){
                    print("to expensive");
                    return;
                }else{
                    gold -= tempUnits[1].GetComponent<ArcherController>().goldCost;
                }
                GameObject FinalUnit = Instantiate<GameObject>(tempUnits[4]);
                FinalUnit.tag = "ArcherRed";
                FinalUnit.transform.position = index.transform.position; 
                FinalUnit.SetActive(true); 
                index.isOccupied = true;
                units.Add(FinalUnit);
                return; 
            }else if (type == "Wizard"){
                if(gold < tempUnits[2].GetComponent<WizardController>().goldCost){
                    print("to expensive");
                    return;
                }else{
                    gold -= tempUnits[2].GetComponent<WizardController>().goldCost;
                }
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
        Color red = UnityEngine.Color.red;
        red.a = 0.5f;
        BackgroundImage.color = red;
        gold = 1000;
        goldText = goldText2;
        FindObjectOfType<AudioManager>().Play("menu_button_click", 0.0f);
        if(SelectedCell != null){
            grid.DisableButton(currButtonCanvas);
        }
        grid.NoCellsForBlue();
        grid.CellsForRed();
    }
    public void PlayerTwoFinished(){
        print(placing);
        PlayerTwoCanvas.enabled = false;
        placing = false;
        print(placing);
        if(SelectedCell != null){
            grid.DisableButton(currButtonCanvas);
        }
        grid.NoCellsForRed();
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



