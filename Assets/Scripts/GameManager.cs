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
        //TODO: Create units via code
        //UNIT COMPONENTS: a tag with their unit type, box controller, model, boxcontroller, animator
        RollInitiative();
    }

    void RollInitiative() {
        allUnits = new List<GameObject>();
        currentUnitIndex = 0;

        // Get all units, i.e. Game Objects with the tags "Knight", "Archer" and "Wizard"
        foreach(string tag in tags) {
            var tmp = new List<GameObject>(GameObject.FindGameObjectsWithTag(tag));
            allUnits = allUnits.Concat(tmp).ToList();
        }

        // Sort the list of units in a random order
        System.Random rnd = new System.Random();
        allUnits = allUnits.Select(x => new { value = x, order = rnd.Next()})
            .OrderBy(x => x.order).Select(x => x.value).ToList();
        
        // Set the current unit as the first unit in the list
        currentUnit = allUnits.ElementAt(currentUnitIndex);
    }

    public void EndTurn() {
        // If the list is over, then the round is over and initiative needs to be rolled again
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
        // Check for left mouse button click
        if(Input.GetMouseButtonDown(0)) {
            //print(currentUnit);
            // Did player click on a unit?
            Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;
            bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);

            // Don't allow units to attack themselves
            if(didHit && (rhInfo.collider.gameObject.tag == "Knight" 
              || rhInfo.collider.gameObject.tag == "Archer" || rhInfo.collider.gameObject.tag == "Wizard" )){
                  
                if(rhInfo.collider.gameObject == currentUnit) return;

                int damage = 0;
                string type = "";
                Vector3 victimLocation= rhInfo.collider.gameObject.GetComponent<Transform>().position;
                if(currentUnit.tag == "Knight") {
                    var attackerScript = currentUnit.GetComponent<KnightController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(victimLocation); 
                } else if(currentUnit.tag == "Archer") {
                    var attackerScript = currentUnit.GetComponent<ArcherController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(victimLocation); 
                } else {
                    var attackerScript = currentUnit.GetComponent<WizardController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(victimLocation); 
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
                if(Physics.Raycast(toMouse, out rhInfo, 500.0f)){
                    if(currentUnit.tag == "Knight") {
                        var script = currentUnit.GetComponent<KnightController>();
                        script.StartMoving(rhInfo.point);
                    } else if(currentUnit.tag == "Archer") {
                        var script = currentUnit.GetComponent<ArcherController>();  
                        script.StartMoving(rhInfo.point);
                    } else {
                        var script = currentUnit.GetComponent<WizardController>();
                        script.StartMoving(rhInfo.point);
                    }
                }
            } 
        };

        //TODO: Remove dead bois from initiative animation
        CheckForDeadUnits();
    }

    void CheckForDeadUnits() {
        // Check for any dead units and remove them from the list.
        // We need to store the units to remove in a list and remove them manually
        // because removing from a list while looping through it throws an error
        var unitsToRemove = new List<GameObject>();
        foreach(GameObject unit in allUnits) {
            if(unit.tag == "Knight") {
                var script = unit.GetComponent<KnightController>();
                if(script.IsUnitDead()) {
                    //allUnits.Remove(unit);
                    unitsToRemove.Add(unit);
                }
            } else if(unit.tag == "Archer") {
                var script = unit.GetComponent<ArcherController>();
                if(script.IsUnitDead()) {
                    //allUnits.Remove(unit);
                    unitsToRemove.Add(unit);
                }
            } else {
                var script = unit.GetComponent<WizardController>();
                if(script.IsUnitDead()) {
                    //allUnits.Remove(unit);
                    unitsToRemove.Add(unit);
                }
            }
        }

        foreach(GameObject unit in unitsToRemove) {
            allUnits.Remove(unit);
        }
    }
}