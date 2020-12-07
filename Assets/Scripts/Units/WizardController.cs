using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WizardController : MonoBehaviour
{
    public int maxHealth = 80;
    public int health = 80;
    public HexCoordinates IndexedLocation;
    public HexCell CurrCell; 
    public int armor = 1;
    public int armorModifier = 0;
    public int baseDamage = 20;
    public int damageModifier = 0;
    
    public string type = "wizard";
    public string weaknessType = "archer";
    public int goldCost = 150;
    public int movementRange = 8;
    public int attackRange = 2;
    public TMPro.TextMeshProUGUI damageTakenText;
    public TMPro.TextMeshProUGUI healthText;
    public Vector3 location;

    private bool isAttacking = false;
    private bool isMoving = false;
    private bool isDefending = false;
    public bool isDead = false;
    public bool deathConfirmed = false;
    public bool isIdle = true;
    public bool isTakingDamage = false;
    
    private bool startPlayingMoveAnimation = false;
    private bool startPlayingIdleAnimation = true;

    //Where the unit should move next
    public Vector3 destination;
    
    public GameObject DefenceImage;
    public Image healthBar;
    public Text armorDisplay;
    private Vector3 rotation; 
    //how fast the model should go from one space to the other 
    public int speed = 5; 
    public string team;

    public HexGrid grid; 

    public Animator animator;
    

    public Canvas HealthCanvas; 
    public Camera camera;
    // Start is called before the first frame update
    void Start() {
        destination = transform.position;  
        location = transform.position;

        armorDisplay.text = "Armor: " + armor;
        healthText.text = health + "/" + maxHealth;
        healthText.fontSize = 20;
        healthText.transform.localPosition = new Vector3(-25.0f, -10.0f, 0.0f);

        damageTakenText.fontWeight = TMPro.FontWeight.Bold;
        damageTakenText.transform.localPosition = new Vector3(-43.0f, 150.0f, 0.0f);
        damageTakenText.fontSize = 26;

        // Make healthbar face camera as soon as game starts
        MoveHealthBar();
    }

    // Update is called once per frame
    void Update() {
        if(!isDead) {
            if(damageTakenText.text != "") {
                UpdateDamageTakenText();
            }

            //print(damageTakenText.transform.position);

            //if there is a new destination then move to it, else don't move
            if(destination != transform.position){
                Move();
            } else {
                if(isDefending){
                    animator.Play("Defence");
                    isMoving = false;
                    isIdle = true;
                }else if(startPlayingIdleAnimation) {
                    startPlayingIdleAnimation = false;
                    animator.Play("idle");
                    isMoving = false;
                    isIdle = true;
                }
            }

            if(isAttacking) {
                animator.Play("attack");
                isAttacking = false;
            } else if(isMoving) {
                if(startPlayingMoveAnimation) {
                    startPlayingMoveAnimation = false;
                    animator.Play("run");      
                }
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

    void UpdateDamageTakenText() {
        damageTakenText.transform.Translate(new Vector3(0, 0.005f, 0));
    }

    IEnumerator ClearDamageTakenText() {
        yield return new WaitForSeconds(0.8f);
        ResetDamageTakenText();
    }

    void ResetDamageTakenText() {
        damageTakenText.text = "";
        damageTakenText.transform.localPosition = new Vector3(-39.0f, 35.0f, 0.0f);        
    }

    public bool StartMoving(Vector3 dest, HexCell hex) {
        float length = Vector3.Distance(transform.position, dest);
        if(length > 7){
            print("no way hosey");
            return false; 
        }else{
            if(hex.isOccupied){
                print("nowayer hoseyer");
                return false; 
            }
            destination = dest;
            startPlayingIdleAnimation = true;
            startPlayingMoveAnimation = true;
            isMoving = true;
            isIdle = false;
            IndexedLocation = hex.coordinates; 
            grid.OccupyCell(hex);
            if(CurrCell != null){
                grid.UnOccupyCell(CurrCell);
            } 
            CurrCell = hex; 
            return true; 
        }
    }

    void Move() {
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime* speed);
        transform.LookAt(destination);
    }

    public bool Attack(Vector3 victimPos) {
        float length = Vector3.Distance(transform.position, victimPos);
        if(length >7.5){
            print(length);
            print("no way hosey");
        }else{
            isAttacking = true;   
            isIdle = false;     
            transform.LookAt(victimPos);
            return true; 
        }
        return false; 
    }

    public void Defend() {
        //TODO: add some value to armorModifier
        isDefending = true; 
        DefenceImage.GetComponent<Renderer>().enabled = true;
    }

    public void UnDefend() {
        //TODO: add some value to armorModifier
        isDefending = false; 
        DefenceImage.GetComponent<Renderer>().enabled = false;
    }

    public void TakeDamage(int damage, string attackerType, float animationDelay) {
        if(isDefending == true){
            damage = (int)(damage/2); 
        }
        StartCoroutine(TakeDamageAfterDelay(damage, attackerType, animationDelay));
    }

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

        healthBar.fillAmount = ((float)health / (float)maxHealth);

        armorDisplay.text = "Armor: " + armor;
        healthText.text = health + "/" + maxHealth;

        damageTakenText.text = (armor != 0 ? Mathf.FloorToInt(damage / 2) : damage).ToString();
        damageTakenText.transform.localPosition = new Vector3(-43.0f, 55.0f, 0.0f);
        StartCoroutine(ClearDamageTakenText());

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
    
    //moving healthbar to face the camera
    public void MoveHealthBar(){
        HealthCanvas.transform.LookAt(camera.transform.position);
    }
}
