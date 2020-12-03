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

    public List<AudioClip> audioClips = new List<AudioClip>();

    // To keep track of the portrait's location for the initiative highlighter
    public List<Vector2> portraitLocations = new List<Vector2>();

    // TODO: add different colored teams
    private string[] portraitTextureNames = {
        "KnightBlue", "ArcherBlue", "WizardBlue", 
        "KnightRed", "ArcherRed", "WizardRed"
    };

    private string[] sfxNames = {
        "archer_attack", "hit", "knight_attack", "wizard_attack"
    };

    private string[] tags = {
        "Knight", "Archer", "Wizard"
    };

    void Awake() {
        instance = this;
        canvas = GameObject.Find("Canvas");
        LoadPortraitTextures();
        LoadAudioClips();
    }

    // Start is called before the first frame update
    void Start() {
        //TODO: Create units via code
        // UNIT COMPONENTS: a tag with their unit type, box controller, model, boxcontroller, 
        // animator, Selector (set to inactive at first, Mesh Renderer: Cast shadows = off), 
        // layer = "Unit" (for both object and model)
        // Use GameObject.AddComponent function
        RollInitiative();
    }

    void LoadPortraitTextures() {
        foreach(string name in portraitTextureNames) {
            string filePath = "Images/UnitPortraits/" + name;
            print(filePath);
            Texture2D texture = Resources.Load<Texture2D>(filePath);
            portraitTextures.Add(texture);
        }
    }

    void LoadAudioClips() {
        // TODO: load audio clips
        // var audioClip = Resources.Load<AudioClip>("Audio/audioClip01");
        foreach(string name in sfxNames) {
            string filePath = "SFX/" + name;
            print(filePath);
            AudioClip audioClip = Resources.Load<AudioClip>(filePath);
            audioClips.Add(audioClip);
            //print(audioClip.name);
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
        EnableCurrentUnitCircle();
        SetInitiativePortraits();
        HighlightCurrentUnitsPortrait();
    }

    // Enables the circle around the current unit
    void EnableCurrentUnitCircle() {
        currentUnit.gameObject.transform.Find("Selector").gameObject.SetActive(true);
    }

    // Disables the circle around the current unit
    void DisableCurrentUnitCircle() {
        currentUnit.gameObject.transform.Find("Selector").gameObject.SetActive(false);
    }

    // For some reason after the first round, two units keep their circles. 
    // Call this function at the beginning of every round to mitigate that
    void DisableAllUnitCircles() {
        foreach(GameObject unit in allUnits) {
            unit.gameObject.transform.Find("Selector").gameObject.SetActive(false);
        }
    }

    //TODO: Border around current unit's portrait
    void HighlightCurrentUnitsPortrait() {
        // Destroy previous hughlighters
        var highlighters = GameObject.FindGameObjectsWithTag("Highlighter");
        for(var i = 0; i < highlighters.Length; i++) {
            Destroy(highlighters[i]);
        }

        GameObject highlighter = new GameObject("Highlighter");
        highlighter.tag = "Highlighter";
        RectTransform trans = highlighter.AddComponent<RectTransform>();

        // Set the canvas as a parent
        trans.transform.SetParent(canvas.transform);
        // Not sure what this does but I'm sure it does something
        trans.localScale = Vector3.one;
        
        // Sets the position of the highlighter
        trans.anchoredPosition = portraitLocations[currentUnitIndex]; //new Vector2(200, Screen.height/2 - 50);
        // Sets the size of the highlighter
        trans.sizeDelta = new Vector2(50, 50);

        Image image = highlighter.AddComponent<Image>();        
        Texture2D tex = Resources.Load<Texture2D>("Images/Shapes/square");
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        highlighter.transform.SetParent(canvas.transform);
    }

    void SetInitiativePortraits() {
        DestroyAllPortraits();
        portraitLocations = new List<Vector2>();

        int middleIndex = 0;
        if(allUnits.Count % 2 == 0) {
            print("even number");
            middleIndex = allUnits.Count / 2; 
        } else {
            print("odd number");
            middleIndex = Mathf.CeilToInt(allUnits.Count / 2) + 1;
        }

        int portraitCount = 1;
        foreach(GameObject unit in allUnits) {
            // Create a new gameobject to store the image
            GameObject imageObject = new GameObject("name_" + portraitCount.ToString());
            imageObject.tag = "Portrait";
            RectTransform trans = imageObject.AddComponent<RectTransform>();

            // Set the canvas as a parent
            trans.transform.SetParent(canvas.transform);
            // Not sure what this does but I'm sure it does something
            trans.localScale = Vector3.one;

            // TODO: find out offset for even list lengths

            // Determines the x position of the portrait
            int offset = portraitCount < middleIndex ? -(middleIndex - portraitCount)
                : portraitCount == middleIndex ? 0 
                : portraitCount - middleIndex;

            // Sets the position of the portrait
            trans.anchoredPosition = new Vector2(offset * 60, Screen.height/2 - 50);
            // Sets the size of the portrait
            trans.sizeDelta = new Vector2(50, 50);

            // Save this portrait's location for the highlighter
            portraitLocations.Add(trans.anchoredPosition);

            Texture2D tex = null;
            // Find the unit's texture 
            foreach(Texture2D texture in portraitTextures) {
                if(texture.name.ToLower().Contains(unit.tag.ToLower())) {
                    tex = texture;
                }
            }

            if(tex != null) {
                Image image = imageObject.AddComponent<Image>();
                // Set the unit's texture as the image's sprite
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
            //TODO: Move camera to current unit
            currentUnitIndex++;
            DisableCurrentUnitCircle();
            currentUnit = allUnits[currentUnitIndex];
            EnableCurrentUnitCircle();
            HighlightCurrentUnitsPortrait();
            //copy af koðanum til að láta unitið vera í "defend" ef hann clickar á sjálfan sig 
            if(currentUnit.tag == "Knight") {
                var attackerScript = currentUnit.GetComponent<KnightController>();
                attackerScript.UnDefend(); 
            } else if(currentUnit.tag == "Archer") {
                var attackerScript = currentUnit.GetComponent<ArcherController>();
                attackerScript.UnDefend(); 
            } else {
                var attackerScript = currentUnit.GetComponent<WizardController>();
                attackerScript.UnDefend(); 
            }
        } else {
            DisableAllUnitCircles();
            RollInitiative();
        }
    }

    // Update is called once per frame
    void Update() {
        // Check for left mouse button click
        if(Input.GetMouseButtonDown(0)) {            
            // Did player click on a unit?
            Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;
            bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);

            print(rhInfo.collider.gameObject.tag);
            if(rhInfo.collider.gameObject.tag == "UI") return;

            // Did player click on a unit?
            if(didHit && (rhInfo.collider.gameObject.tag == "Knight" 
              || rhInfo.collider.gameObject.tag == "Archer" || rhInfo.collider.gameObject.tag == "Wizard" )){
                
                // Don't allow units to attack themselves
                if(rhInfo.collider.gameObject == currentUnit){
                    //copy af koðanum til að láta unitið vera í "defend" ef hann clickar á sjálfan sig 
                    if(currentUnit.tag == "Knight") {
                        var attackerScript = currentUnit.GetComponent<KnightController>();
                        attackerScript.Defend(); 
                    } else if(currentUnit.tag == "Archer") {
                        var attackerScript = currentUnit.GetComponent<ArcherController>();
                        attackerScript.Defend(); 
                    } else {
                        var attackerScript = currentUnit.GetComponent<WizardController>();
                        attackerScript.Defend(); 
                    }
                };

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
        int index = 0;
        foreach(GameObject unit in allUnits) {
            if(unit.tag == "Knight") {
                var script = unit.GetComponent<KnightController>();
                if(script.IsUnitDead()) {
                    unitsToRemove.Add(unit);
                    if(index < currentUnitIndex) {
                        currentUnitIndex--;
                    }
                }
            } else if(unit.tag == "Archer") {
                var script = unit.GetComponent<ArcherController>();
                if(script.IsUnitDead()) {
                    unitsToRemove.Add(unit);
                    if(index < currentUnitIndex) {
                        currentUnitIndex--;
                    }
                }
            } else {
                var script = unit.GetComponent<WizardController>();
                if(script.IsUnitDead()) {
                    unitsToRemove.Add(unit);
                    if(index < currentUnitIndex) {
                        currentUnitIndex--;
                    }
                }
            }

            index++;
        }

        foreach(GameObject unit in unitsToRemove) {
            allUnits.Remove(unit);
        }

        if(unitsToRemove.Count != 0) {
            SetInitiativePortraits();
            HighlightCurrentUnitsPortrait();
        }    
    }
}