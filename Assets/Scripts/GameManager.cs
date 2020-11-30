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
        //TODO: make tmpCurrentUnit attack another unit  

        // Left mouse click
        if(Input.GetMouseButtonDown(0)) {
            // Did player click on unit?
            Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;
            bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);

            if(didHit) {
                // TODO: change hardcoding
                
                //TODO: determine total damage based on unit's base damage
                //TODO: call victim's TakeDamage() function, pass total damage as parameter

                var attackerScript = tmpCurrentUnit.GetComponent<KnightController>();
                var victimScript = rhInfo.collider.gameObject.GetComponent<KnightController>();

                int damage = attackerScript.baseDamage;
                string type = attackerScript.type;

                attackerScript.Attack();                
                victimScript.TakeDamage(damage, type);
                

                
                //print(rhInfo.collider.gameObject.);
            } else {
                print("clicked on empty space");
            }
        }       
    }
}
