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
    bool isTakingDamage = false;
    public bool isDead = false;
    public bool deathConfirmed = false;
    public bool isIdle = true;

    //Where the unit should move next
    public Vector3 destination;
    //how fast the model should go from one space to the other 
    public int speed; 

    private Vector3 rotation; 
    public Animator animator;
    public string currentAnimation = "idle";

    // Start is called before the first frame update
    void Start() {
        destination = transform.position; 
        
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetMouseButtonDown(0)) {/*
            //commentað út svo dótið hans virki (bæði með "when mouce clicked)
            
            Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;
            if(Physics.Raycast(toMouse, out rhInfo, 500.0f)){
                destination = rhInfo.point;
            }*/
        } 

        if(Input.GetKey(KeyCode.Space)) {
            if(isMoving) isMoving = false;
            isAttacking = true;
        } else if(Input.GetKey(KeyCode.A)) {
            if(isAttacking) isAttacking = false; 
            isMoving = true;
        if(isAttacking) {
            animator.Play("attack");
            isAttacking = false;
        } else if(isMoving) {
            animator.Play("run");
            isMoving = false;
        } else if(isTakingDamage) {
            animator.Play("hurt");
            isTakingDamage = false;
        }
        }

        if(!isIdle) {
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("attack") 
            && !animator.GetCurrentAnimatorStateInfo(0).IsName("run") 
            && !animator.GetCurrentAnimatorStateInfo(0).IsName("hurt")) {
                animator.Play("idle");
                isIdle = true;
            }
        }
        if(!isDead) {
            if(isAttacking) {
                animator.Play("attack");
                isAttacking = false;
            } else if(isMoving) {
                animator.Play("run");
                isMoving = false;
            } else if(isTakingDamage) {
                animator.Play("hurt");
                isTakingDamage = false;
            }
        } else {
            if(!deathConfirmed) {
                // 50/50 chance which death animation is played
                if(Random.Range(0, 2) == 0) {
                    animator.Play("die2");
                } else {
                    animator.Play("die1");
                }
                deathConfirmed = true;
            }    
        }
        //if there is a new destination then move to it, else don't move
        if(destination != transform.position){
            isMoving = true;
            isIdle = false;
            rotation = Vector3.RotateTowards(transform.forward, destination, speed, 0.0f);
            Move();
        }else
        {
            isMoving = false;
            isIdle = true;
        }
    }

    void Move() {
        print("moving");
        transform.rotation = Quaternion.LookRotation(rotation);
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime* speed);
    }

    // Enable attack animation and disable idle animation
    public void Attack() {
        isAttacking = true;   
        isIdle = false;     
    }

    void Defend() {
        //TODO: add some value to armorModifier
    }

    public void TakeDamage(int damage, string attackerType, float animationDelay) {
        StartCoroutine(TakeDamageAfterDelay(damage, attackerType, animationDelay));
    }

    // Delay taking damage to line up with the attack animation
    IEnumerator TakeDamageAfterDelay(int damage, string attackerType, float time) {
        yield return new WaitForSeconds(time);
        if(armor != 0) {
            health -= Mathf.FloorToInt(damage / 2);
            if(attackerType == weaknessType) {
                armor--;
            }
        } else {
            health -= damage;
        }

        isIdle = false;
        if(health <= 0) {
            isDead = true;
        } else {
            isTakingDamage = true;
        }
    }

    // Deployment phase
    void PlaceUnit(Vector3 position) {
        //transform.position = position;
    }
}
