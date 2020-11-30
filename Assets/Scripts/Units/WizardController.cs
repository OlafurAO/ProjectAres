using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardController : MonoBehaviour
{
    public int health = 150;
    public int armor = 1;
    public int armorModifier = 0;
    public int baseDamage = 15;
    public int damageModifier = 0;
    
    public string type = "wizard";
    public string weaknessType = "archer";
    public int goldCost = 150;
    public int movementRange = 8;
    public int attackRange = 2;
    public Vector3 location;

    private bool isAttacking = false;
    private bool isMoving = false;
    private bool isDefending = false;
    public bool isDead = false;
    public bool isIdle = true;
    public bool isTakingDamage = false;

    public Animator animator;

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
        } else if(isTakingDamage) {
            animator.Play("hurt");
            isTakingDamage = false;
        }
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

    public void TakeDamage(int damage, string attackerType, float animationDelay) {
        StartCoroutine(TakeDamageAfterDelay(damage, attackerType, animationDelay));
    }

    IEnumerator TakeDamageAfterDelay(int damage, string attackerType, float time) {
        yield return new WaitForSeconds(time);

        isTakingDamage = true;
        isIdle = false;
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
