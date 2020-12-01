using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public List<GameObject> allUnits = new List<GameObject>();
    public GameObject currentUnit;
    public int currentUnitIndex;

    private string[] tags = {
        "Knight", "Archer", "Wizard"
    };

    void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        RollInitiative();
    }

    //TODO: Randomize
    void RollInitiative() {
        allUnits = new List<GameObject>();
        currentUnitIndex = 0;

        foreach(string tag in tags) {
            var tmp = new List<GameObject>(GameObject.FindGameObjectsWithTag(tag));
            allUnits = allUnits.Concat(tmp).ToList();
        }

        foreach(GameObject i in allUnits) {
            print(i.name);
        }

        currentUnit = allUnits.ElementAt(currentUnitIndex);
    }

    public void EndTurn() {
        if(currentUnitIndex != allUnits.Count - 1) {
            //TODO: Move camera
            currentUnitIndex++;
            currentUnit = allUnits[currentUnitIndex];
        } else {
            RollInitiative();
        }
    }

    // Update is called once per frame
    void Update() {
        //TODO: move tmpCurrentUnit  

        // Check for left mouse button click
        if(Input.GetMouseButtonDown(0)) {
            // Did player click on a unit?
            Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;
            bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);

            if(didHit){
                
                int damage = 0;
                string type = "";

                if(currentUnit.tag == "Knight") {
                    var attackerScript = currentUnit.GetComponent<KnightController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(); 
                } else if(currentUnit.tag == "Archer") {
                    var attackerScript = currentUnit.GetComponent<ArcherController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(); 
                } else {
                    var attackerScript = currentUnit.GetComponent<WizardController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(); 
                }

                if(rhInfo.collider.gameObject.tag == "Knight") {
                    var victimScript = rhInfo.collider.gameObject.GetComponent<KnightController>();
                    victimScript.TakeDamage(damage, type, 0.5f);
                } else if(rhInfo.collider.gameObject.tag == "Archer") {
                    var victimScript = rhInfo.collider.gameObject.GetComponent<ArcherController>();
                    victimScript.TakeDamage(damage, type, 0.5f);
                } else {
                    var victimScript = rhInfo.collider.gameObject.GetComponent<WizardController>();
                    victimScript.TakeDamage(damage, type, 0.5f);
                }            
            } else {
                print("clicked on empty space");
            } 
        };
    }
}
