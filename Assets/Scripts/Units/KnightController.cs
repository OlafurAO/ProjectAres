using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightController : MonoBehaviour {
    public int health = 200;
    public int armor = 5;
    public int armorModifier = 0;
    private int baseDamage = 10;
    public int damageModifier = 0;
    
    public string type = "knight";
    public string weaknessType = "wizard";
    public int goldCost = 50;
    public int movementRange = 5;
    public int attackRange = 1;
    public Vector3 location;

    private bool isSelected = false;
    private bool isAttacking = false;
    private bool isMoving = false;
    private bool isDefending = false;
    public bool isDead = false;

    public Animator animator;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetMouseButtonDown(0)) {
            
        } 

        if(Input.GetKey(KeyCode.Space)) {
            if(isMoving) isMoving = false;
            isAttacking = true;
        } else if(Input.GetKey(KeyCode.A)) {
            if(isAttacking) isAttacking = false; 
            isMoving = true;
        }

        if(isMoving) {
            animator.Play("run");
        } else if(isAttacking) {
            animator.Play("attack");
        } else {
            animator.Play("idle");
        }
    }

    void Move() {
        //transform.position = new Vector3(0, 0, 0);
    }

    void Attack() {
        //TODO: determine total damage based on unit's base damage
        //TODO: call victim's TakeDamage() function, pass total damage as parameter
    }

    void Defend() {
        //TODO: add some value to armorModifier
    }

    void TakeDamage() {
        if(armor != 0) {
            //TODO: health -= damage/2
            //TODO: if attacker type == weakness then armor--
        } else {
            //TODO: health -= damage
        }
    }

    // Deployment phase
    void PlaceUnit(Vector3 position) {
        //transform.position = position;
    }
}
