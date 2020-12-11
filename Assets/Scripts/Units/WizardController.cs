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
    private int maxArmor = 1;
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
    public TMPro.TextMeshProUGUI armorText;
    public TMPro.TextMeshProUGUI armorDamageText;
    public TMPro.TextMeshProUGUI armorAbsorbtionText; // Notifies the player that damage will get absorbed due to armor
    public TMPro.TextMeshProUGUI healthDamageTextPreview; // Text preview of the damage the player will deal
    public TMPro.TextMeshProUGUI armorDamageTextPreview; // Text preview of the damage the player will deal to armor
    public Vector3 location;

    public bool isAttacking = false;
    public bool isMoving = false;
    private bool isDefending = false;
    public bool isDead = false;
    public bool deathConfirmed = false;
    public bool isIdle = true;
    public bool isTakingDamage = false;
    
    private bool startPlayingMoveAnimation = false;
    private bool startPlayingIdleAnimation = true;
    private bool showHealthBarDropOff = false;

    //Where the unit should move next
    public Vector3 destination;
    
    public GameObject DefenceImage;
    public Image healthBar;
    public Image healthBarPreview; // Shows a preview of the damage that will be done to the unit
    public Image healthBarFallOff; // Animation where a chunk falls off the health bar
    private int healthBarAlphaModifier = 0; // Makes the health damage preview slowly fade in and out to compare current health to future health
    public Image armorBar;
    public Image armorBarPreview;

    private Vector3 rotation; 
    //how fast the model should go from one space to the other 
    public int speed = 5; 
    public string team;

    public HexGrid grid; 

    public Animator animator;

    public Canvas HealthCanvas; 
    public Camera camera;

    // Stores the location of the current victim for the arrow trail effect to travel towards
    private Vector3 currentVictimPos;
    private GameObject projectile;
    private bool projectileGoUp = true; 

    //portrait of the unit 
    public Image portrait;

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

        armorDamageTextPreview.transform.localPosition = new Vector3(-36.0f, 35.0f, 0.0f);
        healthDamageTextPreview.transform.localPosition = new Vector3(-25.0f, -10.0f, 0.0f);
        armorDamageTextPreview.fontSize = 20;
        healthDamageTextPreview.fontSize = 20;
        armorDamageTextPreview.text = "";
        healthDamageTextPreview.text = "";

        armorAbsorbtionText.text = "";
        armorAbsorbtionText.transform.localPosition = new Vector3(-115f, 19f, 0f);
        armorAbsorbtionText.color = Color.red;
        armorAbsorbtionText.fontSize = 30;

        // Make healthbar face camera as soon as game starts
        MoveHealthBar();
    }

    // Update is called once per frame
    void Update() {
        if(!isDead) {
            if(damageTakenText.text != "") {
                UpdateDamageTakenText();
            }

            if(projectile != null) {
                // If the "arrow" has reached the enemy, destroy the object
                if(projectile.transform.position.x >= currentVictimPos.x - 0.5f && projectile.transform.position.x <= currentVictimPos.x + 0.5f 
                  && projectile.transform.position.z >= currentVictimPos.z - 0.5f && projectile.transform.position.z <= currentVictimPos.z + 0.5f) {
                    StartCoroutine(DestroyProjectile());
                } else {
                    // Move the "arrow" towards its victim
                    projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, currentVictimPos, 0.3f);
                    if(projectileGoUp) {
                        projectile.transform.Translate(0f, 0.1f, 0f);
                    } else {
                        projectile.transform.Translate(0f, -0.1f, 0f);
                    }

                    if(projectile.transform.position.y == 3f) {
                        projectileGoUp = false;
                    }
                }
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
                /*
                if(currPosition.x >= 0.05) {
                    healthBarFallOff.transform.localPosition = new Vector3(currPosition.x + 0.004f, currPosition.y - 0.01f, currPosition.z);
                } else {
                    healthBarFallOff.transform.localPosition = new Vector3(currPosition.x + 0.01f, currPosition.y + 0.005f, currPosition.z);
                }*/
            }

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

    IEnumerator DestroyProjectile() {
        yield return new WaitForSeconds(2f);
        currentVictimPos = new Vector3(0f, 0f, 0f);
        Destroy(projectile);
        projectile = null;
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

            FindObjectOfType<AudioManager>().Play("wizard_attack", 0.2f);

            currentVictimPos = victimPos + new Vector3(0f, 1f, 0f);
            projectile = new GameObject("Projectile");

            projectile.transform.parent = this.gameObject.transform;
            projectile.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            projectile.transform.LookAt(currentVictimPos);

            StartCoroutine(CreateArrowParticleSystem());
            return true; 
        }
        return false; 
    }

    IEnumerator CreateArrowParticleSystem() {
        yield return new WaitForSeconds(0.5f);

        projectile.transform.position = this.gameObject.transform.position;
        Vector3 local = projectile.transform.localPosition;
        projectile.transform.localPosition = new Vector3(local.x - 0.3f, local.y + 1.2f, local.z + 1.4f);
        
        //arrow.transform.Translate(-1f, 1f, 1f);
        projectile.AddComponent<ParticleSystem>();

        ParticleSystem ps = projectile.GetComponent<ParticleSystem>();
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        var lifetimeSize = ps.sizeOverLifetime;
        var lifetimeColor = ps.colorOverLifetime;
        var emission = ps.emission;
        var noise = ps.noise;
        var shape = ps.shape;

        renderer.material = Resources.Load<Material>("Materials/Glow_A");
        ps.simulationSpace = ParticleSystemSimulationSpace.World;
        ps.startColor = new Color(25, 200, 100, 100);
        ps.startSpeed = 0;
        ps.startLifetime = 1;
        ps.maxParticles = 5000;

        emission.rateOverTime = 0;
        emission.rateOverDistance = 10;

        noise.enabled = true;
        noise.strength = 1f;
        noise.frequency = 0.5f;
        noise.damping = true;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(5f, 1f);
        curve.AddKey(0.5f, 0.5f);
        lifetimeSize.enabled = true;
        lifetimeSize.size = new ParticleSystem.MinMaxCurve(2.5f, curve);

        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] {
            new GradientColorKey(Color.white, 0.9f),
            new GradientColorKey(team == "blue" ? new Color(0f, 184f, 255f) : new Color(184f, 0f, 255f), 0.0f)
        }, new GradientAlphaKey[] {
            new GradientAlphaKey(1.0f, 1.0f)
        });
        lifetimeColor.enabled = true;
        lifetimeColor.color = grad;

        
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.01f;
        shape.radiusThickness = 0.01f;

        projectileGoUp = true;
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
        animator.Play("idle");
    }

    public void TakeDamage(int damage, string attackerType, float animationDelay) {
        if(isDefending == true){
            damage = (int)(damage/2); 
        }
        StartCoroutine(TakeDamageAfterDelay(damage, attackerType, animationDelay));
    }

    IEnumerator TakeDamageAfterDelay(int damage, string attackerType, float time) {
        yield return new WaitForSeconds(time);
        damageTakenText.text = (armor != 0 ? Mathf.FloorToInt(damage / 2) : damage).ToString();
        if(armor != 0) {
            health -= Mathf.FloorToInt(damage / 2);
            if(attackerType == weaknessType) {
                armor--;
                armorBar.fillAmount = ((float)armor / (float)maxArmor);
                armorText.text = armor + "/" + maxArmor;
                if(armor < 10) {
                    armorText.text = "0" + armorText.text;
                }

                armorDamageText.transform.localPosition = new Vector3(-80.0f, 55.0f, 0.0f);
                armorDamageText.text = "1";
            }
        } else {
            health -= damage;
        }

        healthBar.fillAmount = ((float)health / (float)maxHealth);
        healthText.text = (health < 0 ? 0.ToString() : health.ToString()) + "/" + maxHealth;
        if(health < 10) {
            healthText.text = "0" + healthText.text;
        }

        damageTakenText.transform.localPosition = new Vector3(-43.0f, 55.0f, 0.0f);

        healthBarFallOff.gameObject.SetActive(true);
        healthBarFallOff.fillAmount = (armor != 0 ? ((float)Mathf.FloorToInt(damage / 2) / (float)maxHealth)
            : (float)damage / (float)maxHealth);
        
        showHealthBarDropOff = true;

        StartCoroutine(ClearDamageTakenText());
        StartCoroutine(ResetHealthBarFallOff());

        isIdle = false;
        if(health <= 0) {
            healthBarFallOff.fillAmount = 0f;
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
        healthDamageTextPreview.gameObject.SetActive(true);
        
        if(armor != 0) {
            if(attackerType == weaknessType) {
                armorBarPreview.fillAmount = (((float)armor - 1f) / (float)maxArmor); 
                armorAbsorbtionText.text = "WEAK";
                
                int newArmor = armor - 1;
                armorDamageTextPreview.text = newArmor.ToString();

                armorDamageTextPreview.gameObject.SetActive(true);
                armorBarPreview.gameObject.SetActive(true);

                if(newArmor < 20) {
                    if(newArmor < 0) {
                        armorDamageTextPreview.text = "00";
                        armorText.text = "    /" + maxArmor.ToString();
                    } else if(newArmor == 1) {
                        armorDamageTextPreview.text = "0" + armorDamageTextPreview.text;
                        armorText.text = "    /" + maxArmor.ToString();
                    }else if(newArmor < 10) {
                        armorDamageTextPreview.text = "0" + armorDamageTextPreview.text;
                        armorText.text = "     /" + maxArmor.ToString();
                    } else {
                        armorText.text = "    /" + maxArmor.ToString();
                    }
                } else {
                    armorText.text = "     /" + maxArmor.ToString();
                }
            }
        }

        int newHealth = health - (int)(armor != 0 ? Mathf.FloorToInt(damage / 2) : damage);
        healthDamageTextPreview.text = newHealth.ToString();
        if(newHealth < 20) {
           if(newHealth < 0) {
                healthDamageTextPreview.text = "00";
                healthText.text = "    /" + maxHealth.ToString();
            } else if(newHealth < 10) {
                healthDamageTextPreview.text = "0" + healthDamageTextPreview.text;
                healthText.text = "     /" + maxHealth.ToString();
            } else {
                healthText.text = "    /" + maxHealth.ToString();
            }
        } else {
            healthText.text = "     /" + maxHealth.ToString();
        }
        
        healthBarAlphaModifier = -1;
    }

    public void DisablePreviewHealthBar() {
        healthBarPreview.gameObject.SetActive(false);
        armorBarPreview.gameObject.SetActive(false);
        healthDamageTextPreview.gameObject.SetActive(false);
        armorDamageTextPreview.gameObject.SetActive(false);
        armorAbsorbtionText.text = "";

        var newColor = healthBar.color;
        healthBarAlphaModifier = 0;
        
        newColor.a = 1.0f;
        healthBar.color = newColor;

        newColor = armorBar.color;
        newColor.a = 1.0f;
        armorBar.color = newColor;

        armorText.text = armor + "/" + maxArmor;
        if(armor < 10) {
            armorText.text = "0" + armorText.text;
        }

        healthText.text = (health < 0 ? 0.ToString() : health.ToString()) + "/" + maxHealth;
        if(health < 10) {
            healthText.text = "0" + healthText.text;
        }
    }
    
    //get's remaning health and armor of the current 
    public List<(float, int, int)> getDamage(float damage, string type){
        int returnhealth; 
        int returnarmor;
        if(armor <= 0){
            returnarmor = 0;
            returnhealth =(int)(health - damage);
        }else if(weaknessType == type){
            returnarmor = armor-1;
            returnhealth = (int)(health - Mathf.FloorToInt(damage / 2));
        }else{
            returnarmor = armor;
            returnhealth = (int)(health - Mathf.FloorToInt(damage / 2));
        }
        return new List<(float,int,int)>(){(returnarmor,returnhealth,baseDamage)};
    }
}
