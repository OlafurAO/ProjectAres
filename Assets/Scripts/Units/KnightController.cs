using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightController : MonoBehaviour {
    public int health = 200;
    public int armor = 5;
    public int armorModifier = 0;
    public int baseDamage = 10;
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
    public bool isIdle = true;

    public Animator animator;
    public string currentAnimation = "idle";

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if(isAttacking) {
            animator.Play("attack");
            isAttacking = false;
        } else if(isMoving) {
            animator.Play("run");
            isMoving = false;
        }

        if(!isIdle) {
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("attack") 
            && !animator.GetCurrentAnimatorStateInfo(0).IsName("run") 
            && !animator.GetCurrentAnimatorStateInfo(0).IsName("take_damage")) {
                animator.Play("idle");
                isIdle = true;
                print("IDLE IS PLAYING");
            }
        }
        /*
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
        */
    }

    void Move() {
        //transform.position = new Vector3(0, 0, 0);
    }

    public void Attack() {
        isAttacking = true;   
        isIdle = false;     
    }

    void Defend() {
        //TODO: add some value to armorModifier
    }

    public void TakeDamage(int damage, string attackerType) {
        animator.Play("take_damage");
        if(armor != 0) {
            health -= Mathf.FloorToInt(damage / 2);
            if(attackerType == weaknessType) {
                armor--;
            }
        } else {
            health -= damage;
        }
    }

    // Deployment phase
    void PlaceUnit(Vector3 position) {
        //transform.position = position;
    }
}
