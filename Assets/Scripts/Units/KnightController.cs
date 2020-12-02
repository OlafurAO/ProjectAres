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

    private bool startPlayingMoveAnimation = false;


    //Where the unit should move next
    public Vector3 destination;
    private Vector3 rotation; 
    //how fast the model should go from one space to the other 
    public int speed = 5; 

    public Animator animator;

    // Start is called before the first frame update
    void Start() {
        destination = transform.position;    
        location = transform.position;
    }

    // Update is called once per frame
    void Update() {
        if(!isDead) {
            //if there is a new destination then move to it, else don't move
            if(destination != transform.position){
                Move();
            } else {
                isMoving = false;
                isIdle = true;
            }

            if(isAttacking) {
                animator.Play("attack");
                isAttacking = false;
            } else if(isMoving) {
                if(startPlayingMoveAnimation) {
                    startPlayingMoveAnimation = false;
                    
                }
                animator.Play("run");
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
    }

    public void StartMoving(Vector3 dest) {
        destination = dest;
        startPlayingMoveAnimation = true;
        isMoving = true;
        isIdle = false;
    }

    void Move() {
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime* speed);
        transform.LookAt(destination);
    }

    // Enable attack animation and disable idle animation
    public void Attack(Vector3 victimPos) {
        isAttacking = true;   
        isIdle = false;     
        transform.LookAt(victimPos);
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

    public bool IsUnitDead() {
        return isDead;
    }
}
