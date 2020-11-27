using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightController : MonoBehaviour {
    public int health = 200;
    public int armor = 5;
    private int baseDamage = 10;
    
    public string type = "knight";
    public string weaknessType = "wizard";

    private bool isAttacking = false;
    private bool isMoving = false;
    private bool isDefending = false;

    public Animator animator;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
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

    }

    void Attack() {
        //TODO: determine total damage based on unit's base damage
        //TODO: call victim's TakeDamage() function, pass total damage as parameter
    }

    void TakeDamage() {
        if(armor != 0) {
            //TODO: health -= damage/2
            //TODO: if attacker type == weakness then armor--
        } else {
            //TODO: health -= damage
        }
    }
}
