using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public GameObject canvas;

    public List<GameObject> allUnits = new List<GameObject>();
    public GameObject currentUnit;
    public int currentUnitIndex;

    public List<RawImage> initiativePortraits = new List<RawImage>();

    public List<Texture2D> portraitTextures = new List<Texture2D>();

    // TODO: add different colored teams
    private string[] portraitTextureNames = {
        "Knight", "Archer", "Wizard"
    };

    private string[] tags = {
        "Knight", "Archer", "Wizard"
    };

    void Awake() {
        instance = this;
        canvas = GameObject.Find("Canvas");
        LoadPortraitTextures();
    }

    // Start is called before the first frame update
    void Start() {
        //TODO: Create units via code
        //UNIT COMPONENTS: a tag with their unit type, box controller, model, boxcontroller, animator
        // Use GameObject.AddComponent function

        // TODO: load audio clips
        // var audioClip = Resources.Load<AudioClip>("Audio/audioClip01");
        RollInitiative();
    }

    void LoadPortraitTextures() {
        foreach(string name in portraitTextureNames) {
            string filePath = "Images/UnitPortraits/" + name;
            Texture2D texture = Resources.Load<Texture2D>(filePath);
            portraitTextures.Add(texture);
        }
    }

    void RollInitiative() {
        allUnits = new List<GameObject>();
        currentUnitIndex = 0;

        // Get all units, i.e. Game Objects with the tags "Knight", "Archer" and "Wizard"
        foreach(string tag in tags) {
            var tmp = new List<GameObject>(GameObject.FindGameObjectsWithTag(tag));
            allUnits = allUnits.Concat(tmp).ToList();
        }

        // Sort the list of units in a random order
        System.Random rnd = new System.Random();
        allUnits = allUnits.Select(x => new { value = x, order = rnd.Next()})
            .OrderBy(x => x.order).Select(x => x.value).ToList();
        
        // Set the current unit as the first unit in the list
        currentUnit = allUnits.ElementAt(currentUnitIndex);
        SetInitiativePortraits();
    }

    void SetInitiativePortraits() {
        DestroyAllPortraits();

        int middleIndex = 0;
        if(allUnits.Count % 2 == 0) {
            print("even number");
            middleIndex = allUnits.Count / 2; 
        } else {
            print("odd number");
            middleIndex = Mathf.CeilToInt(allUnits.Count / 2) + 1;
        }
        print(middleIndex);

        // TODO: Remove all protraits from the previous round
        int portraitCount = 1;
        foreach(GameObject unit in allUnits) {
            GameObject imageObject = new GameObject("name_" + portraitCount.ToString());
            imageObject.tag = "Portrait";
            RectTransform trans = imageObject.AddComponent<RectTransform>();
            trans.transform.SetParent(canvas.transform);
            trans.localScale = Vector3.one;

            // TODO: find out offset for even list lengths

            // Determines the x position of the portrait
            int offset = portraitCount < middleIndex ? -(middleIndex - portraitCount)
                : portraitCount == middleIndex ? 0 
                : portraitCount - middleIndex;

            trans.anchoredPosition = new Vector2(offset * 60, Screen.height/2 - 50);
            trans.sizeDelta = new Vector2(50, 50);

            Texture2D tex = null;
            foreach(Texture2D texture in portraitTextures) {
                if(texture.name.ToLower().Contains(unit.tag.ToLower())) {
                    tex = texture;
                }
            }

            if(tex != null) {
                Image image = imageObject.AddComponent<Image>();
                //Texture2D tex = Resources.Load<Texture2D>(filePath);
                image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                imageObject.transform.SetParent(canvas.transform);
            }

            portraitCount++;
        }
    }

    void DestroyAllPortraits() {
        var portraits = GameObject.FindGameObjectsWithTag("Portrait");
        for(var i = 0; i < portraits.Length; i++) {
            Destroy(portraits[i]);
        }
    }

    public void EndTurn() {
        // If the list is over, then the round is over and initiative needs to be rolled again
        if(currentUnitIndex != allUnits.Count - 1) {
            //TODO: Move camera
            currentUnitIndex++;
            currentUnit = allUnits[currentUnitIndex];
        } else {
            RollInitiative();
        }
    }

    // Update is called once per frame
    void Update() {
        // Check for left mouse button click
        if(Input.GetMouseButtonDown(0)) {
            Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;
            bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);

            // Did player click on a unit?
            if(didHit && (rhInfo.collider.gameObject.tag == "Knight" 
              || rhInfo.collider.gameObject.tag == "Archer" || rhInfo.collider.gameObject.tag == "Wizard" )){
                
                // Don't allow units to attack themselves
                if(rhInfo.collider.gameObject == currentUnit) return;

                int damage = 0;
                string type = "";
                Vector3 victimLocation= rhInfo.collider.gameObject.GetComponent<Transform>().position;
                if(currentUnit.tag == "Knight") {
                    var attackerScript = currentUnit.GetComponent<KnightController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(victimLocation); 
                } else if(currentUnit.tag == "Archer") {
                    var attackerScript = currentUnit.GetComponent<ArcherController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(victimLocation); 
                } else {
                    var attackerScript = currentUnit.GetComponent<WizardController>();
                    damage = attackerScript.baseDamage;
                    type = attackerScript.type;
                    attackerScript.Attack(victimLocation); 
                }

                if(rhInfo.collider.gameObject.tag == "Knight") {
                    var victimScript = rhInfo.collider.gameObject.GetComponent<KnightController>();
                    victimScript.TakeDamage(damage, type, 0.5f);
                } else if(rhInfo.collider.gameObject.tag == "Archer") {
                    var victimScript = rhInfo.collider.gameObject.GetComponent<ArcherController>();
                    victimScript.TakeDamage(damage, type, 0.5f);
                } else {
                    var victimScript = rhInfo.collider.gameObject.GetComponent<WizardController>();
                    victimScript.TakeDamage(damage, type, 0.5f);
                }
            } else {
                if(Physics.Raycast(toMouse, out rhInfo, 500.0f)){
                    if(currentUnit.tag == "Knight") {
                        var script = currentUnit.GetComponent<KnightController>();
                        script.StartMoving(rhInfo.point);
                    } else if(currentUnit.tag == "Archer") {
                        var script = currentUnit.GetComponent<ArcherController>();  
                        script.StartMoving(rhInfo.point);
                    } else {
                        var script = currentUnit.GetComponent<WizardController>();
                        script.StartMoving(rhInfo.point);
                    }
                }
            } 
        };

        //TODO: Remove dead bois from initiative animation
        CheckForDeadUnits();
    }

    void CheckForDeadUnits() {
        // Check for any dead units and remove them from the list.
        // We need to store the units to remove in a list and remove them manually
        // because removing from a list while looping through it throws an error
        var unitsToRemove = new List<GameObject>();
        foreach(GameObject unit in allUnits) {
            if(unit.tag == "Knight") {
                var script = unit.GetComponent<KnightController>();
                if(script.IsUnitDead()) {
                    //allUnits.Remove(unit);
                    unitsToRemove.Add(unit);
                }
            } else if(unit.tag == "Archer") {
                var script = unit.GetComponent<ArcherController>();
                if(script.IsUnitDead()) {
                    //allUnits.Remove(unit);
                    unitsToRemove.Add(unit);
                }
            } else {
                var script = unit.GetComponent<WizardController>();
                if(script.IsUnitDead()) {
                    //allUnits.Remove(unit);
                    unitsToRemove.Add(unit);
                }
            }
        }

        foreach(GameObject unit in unitsToRemove) {
            allUnits.Remove(unit);
        }
    }
}