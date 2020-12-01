using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public GameObject tmpCurrentUnit;
    // Start is called before the first frame update

    void Awake() {
        instance = this;
    }

    void Start() {
        
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

            if(didHit) {
                // TODO: change hardcoding
                int damage = 0;
                string type = "";
                if(tmpCurrentUnit.tag == "Knight") {
                    var attackerScript = tmpCurrentUnit.GetComponent<KnightController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(); 
                } else if(tmpCurrentUnit.tag == "Archer") {
                    var attackerScript = tmpCurrentUnit.GetComponent<ArcherController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(); 
                } else {
                    var attackerScript = tmpCurrentUnit.GetComponent<WizardController>();
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
        }       
    }
}
