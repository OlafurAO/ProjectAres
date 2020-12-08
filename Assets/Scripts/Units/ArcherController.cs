using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArcherController : MonoBehaviour {
    public HexGrid grid; 
    public int health = 60;
    public int maxHealth = 60;
    private int maxArmor = 2;
    public int armor = 2;
    public int armorModifier = 0;
    public int baseDamage = 10;
    public int damageModifier = 0;
    
    public string type = "archer";
    public string weaknessType = "knight";
    public int goldCost = 20;
    public int movementRange = 10;
    public int attackRange = 2;
    public TMPro.TextMeshProUGUI damageTakenText;
    public TMPro.TextMeshProUGUI healthText;
    public TMPro.TextMeshProUGUI armorText;
    public TMPro.TextMeshProUGUI armorDamageText;
    public TMPro.TextMeshProUGUI armorAbsorbtionText; // Notifies the player that damage will get absorbed due to armor
    public Vector3 location;

    private bool isAttacking = false;
    private bool isMoving = false;
    private bool isDefending = false;
    public bool isDead = false;
    public bool deathConfirmed = false;
    public bool isIdle = true;
    public bool isTakingDamage = false;
    public HexCell CurrCell; 

    private bool startPlayingMoveAnimation = false;
    private bool startPlayingIdleAnimation = true;
    private bool showHealthBarDropOff = false;

    //Where the unit should move next
    public Vector3 destination;
    private Vector3 rotation; 
    //how fast the model should go from one space to the other 
    public int speed = 5; 
    public string team;

    public Animator animator;
    
    public GameObject DefenceImage;
    public Image healthBar;
    public Image healthBarPreview; // Shows a preview of the damage that will be done to the unit
    public Image healthBarFallOff; // Animation where a chunk falls off the health bar
    private int healthBarAlphaModifier = 0; // Makes the health damage preview slowly fade in and out to compare current health to future health
    public Image armorBar;
    public Image armorBarPreview;
    public HexCoordinates IndexedLocation;

    public Canvas HealthCanvas; 
    //main camera to make the health bar face
    public Camera camera; 
    // Start is called before the first frame update
    void Start() {
        destination = transform.position;  
        location = transform.position;

        healthText.text = health + "/" + maxHealth;
        healthText.fontSize = 20;
        healthText.transform.localPosition = new Vector3(-25.0f, -10.0f, 0.0f);

        armorText.text = armor + "/" + maxArmor;
        armorText.fontSize = 20;
        armorText.transform.localPosition = new Vector3(-36.0f, 35.0f, 0.0f);

        damageTakenText.fontWeight = TMPro.FontWeight.Bold;
        damageTakenText.transform.localPosition = new Vector3(-43.0f, 150.0f, 0.0f);
        damageTakenText.fontSize = 26;

        armorDamageText.fontWeight = TMPro.FontWeight.Bold;
        armorDamageText.transform.localPosition = new Vector3(-80.0f, 150.0f, 0.0f);
        armorDamageText.color = new Vector4(253f/255f, 231f/255f, 76f/255f, 1f);
        armorDamageText.fontSize = 26;
        
        armorBarPreview.color = new Vector4(253f/255f, 231f/255f, 76f/255f, 1f);
        armorBarPreview.fillAmount = 1f;

        armorAbsorbtionText.text = "";
        
        // Make healthbar face camera as soon as game starts
        MoveHealthBar();
    }

    // Update is called once per frame
    void Update() {
        if(!isDead) {
            if(damageTakenText.text != "") {
                UpdateDamageTakenText();
            }

            if(healthBarAlphaModifier != 0) {
                var newColor = healthBar.color;
                newColor.a += healthBarAlphaModifier * 0.012f;
                healthBar.color = newColor;
                
                if(armorBarPreview.gameObject.activeSelf) {
                    newColor = armorBar.color;
                    newColor.a += healthBarAlphaModifier * 0.012f;
                    armorBar.color = newColor;
                }

                if(newColor.a <= 0.1f) {
                    healthBarAlphaModifier = 1;
                } else if(newColor.a >= 1.0f) {
                    healthBarAlphaModifier = -1;
                }
            }

            if(showHealthBarDropOff) {
                Vector3 currPosition = healthBarFallOff.transform.localPosition;
                healthBarFallOff.transform.localPosition = new Vector3(currPosition.x, currPosition.y - 0.006f, currPosition.z);
            }

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
                StartCoroutine(DeactivateHealthBar());
            }    
        }
    }

    // Remove healthbar on unit death
    IEnumerator DeactivateHealthBar() {
        yield return new WaitForSeconds(1f);
        HealthCanvas.gameObject.SetActive(false);
    }

    void UpdateDamageTakenText() {
        damageTakenText.transform.Translate(new Vector3(0, 0.005f, 0));
        armorDamageText.transform.Translate(new Vector3(0, 0.005f, 0));
    }

    IEnumerator ClearDamageTakenText() {
        yield return new WaitForSeconds(0.8f);
        ResetDamageTakenText();
    }

    IEnumerator ResetHealthBarFallOff() {
        yield return new WaitForSeconds(0.5f);
        healthBarFallOff.gameObject.SetActive(false);
        healthBarFallOff.transform.localPosition = new Vector3(- (1 - healthBar.fillAmount), 0f, 0f);
        showHealthBarDropOff = false;
    }

    void ResetDamageTakenText() {
        damageTakenText.text = "";
        armorDamageText.text = "";

        damageTakenText.transform.localPosition = new Vector3(-39.0f, 35.0f, 0.0f);
        armorDamageText.transform.localPosition = new Vector3(-80.0f, 35.0f, 0.0f);
    }

    public bool StartMoving(Vector3 dest, HexCell hex) {
        float length = Vector3.Distance(transform.position, dest);
        if(length > 7){
            print("no way hosey");
            return false;
        }else{
            if(hex.isOccupied){
                print("no wayer hoseyer");
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
        MoveHealthBar();
    }

    public bool Attack(Vector3 victimPos) {
        float length = Vector3.Distance(transform.position, victimPos);
        if(length > 7.5){
            print(length);
            print("no way hosey");
        }else{
            isAttacking = true;   
            isIdle = false;     
            transform.LookAt(victimPos);
            MoveHealthBar();
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
        StartCoroutine(TakeDamageAfterDelay(damage, attackerType, animationDelay));
    }

    // Delay taking damage to line up with the attack animation
    IEnumerator TakeDamageAfterDelay(int damage, string attackerType, float time) {
        yield return new WaitForSeconds(time);
        damageTakenText.text = (armor != 0 ? Mathf.FloorToInt(damage / 2) : damage).ToString();
        if(armor != 0) {
            health -= Mathf.FloorToInt(damage / 2);
            if(attackerType == weaknessType) {
                armor--;
                armorBar.fillAmount = ((float)armor / (float)maxArmor);
                armorText.text = armor + "/" + maxArmor;
                armorDamageText.transform.localPosition = new Vector3(-80.0f, 55.0f, 0.0f);
                armorDamageText.text = "1";
            }
        } else {
            health -= damage;
        }
        
        healthBar.fillAmount = ((float)health / (float)maxHealth);
        healthText.text = (health < 0 ? 0.ToString() : health.ToString()) + "/" + maxHealth;
        damageTakenText.transform.localPosition = new Vector3(-43.0f, 55.0f, 0.0f);

        healthBarFallOff.gameObject.SetActive(true);
        healthBarFallOff.fillAmount = (armor != 0 ? ((float)Mathf.FloorToInt(damage / 2) / (float)maxHealth)
            : (float)damage / (float)maxHealth);
            
        showHealthBarDropOff = true;
        if(health == 0) {
            healthBarFallOff.fillAmount = 0f;
        }
        
        StartCoroutine(ClearDamageTakenText());
        StartCoroutine(ResetHealthBarFallOff());

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

    public void ShowPreviewHealthBar(float damage, string attackerType) {
        float totalDamage = (armor != 0 ? Mathf.FloorToInt(damage / 2) : damage);
        healthBarPreview.fillAmount = (((float)health - totalDamage) / (float)maxHealth);
        healthBarPreview.gameObject.SetActive(true);
        
        if(armor != 0) {
            if(attackerType == weaknessType) {
                armorBarPreview.fillAmount = (((float)armor - 1f) / (float)maxArmor); 
                armorBarPreview.gameObject.SetActive(true);
            }
            armorAbsorbtionText.text = "Armor absorbs\n50'/, damage";
        }
        
        healthBarAlphaModifier = -1;
    }

    public void DisablePreviewHealthBar() {
        healthBarPreview.gameObject.SetActive(false);
        armorBarPreview.gameObject.SetActive(false);
        armorAbsorbtionText.text = "";

        var newColor = healthBar.color;
        healthBarAlphaModifier = 0;
        
        newColor.a = 1.0f;
        healthBar.color = newColor;

        newColor = armorBar.color;
        newColor.a = 1.0f;
        armorBar.color = newColor;
    }
}
