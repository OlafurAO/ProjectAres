﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public static Camera mainCamera;
    public GameObject canvas;

    private Vector3 mousePos = Vector3.zero;

    public List<GameObject> allUnits = new List<GameObject>();
    public GameObject currentUnit;
    public int currentUnitIndex;

    public List<RawImage> initiativePortraits = new List<RawImage>();
    public List<Texture2D> portraitTextures = new List<Texture2D>();

    public List<AudioClip> audioClips = new List<AudioClip>();

    // To keep track of the portrait's location for the initiative highlighter
    public List<Vector2> portraitLocations = new List<Vector2>();

    public HexGrid grid; 

    public TMPro.TextMeshProUGUI winnerLabel;

    private HexCell SelectedCell; 
    //temporary list of all units and color ("knihtObject", "blue")
    public List<GameObject> tempUnits;

    // TODO: add different colored teams
    private string[] portraitTextureNames = {
        "KnightBlue", "ArcherBlue", "WizardBlue", 
        "KnightRed", "ArcherRed", "WizardRed"
    };

    private string[] sfxNames = {
        "archer_attack", "hit", "knight_attack", "wizard_attack"
    };

    private string[] tags = {
        "KnightBlue", "ArcherBlue", "WizardBlue",
        "KnightRed", "ArcherRed", "WizardRed",
    };

    // TODO: fix harcoding
    private int blueUnitsRemaining = 3;
    private int redUnitsRemaining = 3;
    bool gameOver = false;
    bool movement; 
    bool action; 

    public Vector3 victimLocation;
    //store conteroller of the victim  
    public WizardController wizVictim;
    public KnightController knightVictim; 
    public ArcherController archVictim; 
    //store what type the victim is (wiz, knight, arch)
    public string VictimUnit; 
    //stores a canvas of the newest button visable (so we can disable it if needed (click on 2 tiles at onece and 1 one dissapears))
    private Canvas currButtonCanvas;
    //stores the location clicked where you want to create a unit
    private Vector3 CreatedLocation;


    void Awake() {
        instance = this;
        mainCamera = Camera.main;
        mainCamera.enabled = true;
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

    public void Restart() {
        SceneManager.LoadScene("SampleScene");
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
        movement = true; 
        action = true; 
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
            if(currentUnit.tag.Contains("Knight")) {
                var attackerScript = currentUnit.GetComponent<KnightController>();
                attackerScript.UnDefend(); 
            } else if(currentUnit.tag.Contains("Archer")) {
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
        action = true; 
        movement = true; 
    }

    void MoveCamera() {
        // Speed of camera movement, if shift is held down, go faster
        float cameraSpeed = Input.GetKey(KeyCode.LeftShift) ? 18f : 10f;
        bool didCameraMove = false;

        // Left, right
        if(Input.GetKey(KeyCode.A)) {
            if(mainCamera.transform.position.x > -5) {
                mainCamera.transform.Translate(new Vector3(-cameraSpeed * Time.deltaTime, 0, 0));
                didCameraMove = true;
            }    
        } else if(Input.GetKey(KeyCode.D)) {
            if(mainCamera.transform.position.x < 25) {
                mainCamera.transform.Translate(new Vector3(cameraSpeed * Time.deltaTime, 0, 0));
                didCameraMove = true;
            }    
        } 
        
        // Back, forth
        if(Input.GetKey(KeyCode.W)) { 
            if(mainCamera.transform.position.z < 20) {
                var rotation = mainCamera.transform.rotation;
                mainCamera.transform.rotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
                mainCamera.transform.Translate(new Vector3(0, 0, cameraSpeed * Time.deltaTime));
                mainCamera.transform.rotation = rotation;
                didCameraMove = true;
            }    
        } else if(Input.GetKey(KeyCode.S)) {
            if(mainCamera.transform.position.z > -10) {
                var rotation = mainCamera.transform.rotation;
                mainCamera.transform.rotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
                mainCamera.transform.Translate(new Vector3(0, 0, -cameraSpeed * Time.deltaTime));
                mainCamera.transform.rotation = rotation;
                didCameraMove = true;
            }    
        } 
        
        // Up, down
        if(Input.GetKey(KeyCode.E)) { 
            if(mainCamera.transform.position.y < 10) {
                mainCamera.transform.Translate(new Vector3(0, cameraSpeed * Time.deltaTime, 0));
                didCameraMove = true;
            }    
        } else if(Input.GetKey(KeyCode.Q)) {
            if(mainCamera.transform.position.y > 2) {
                mainCamera.transform.Translate(new Vector3(0, -cameraSpeed * Time.deltaTime, 0));
                didCameraMove = true; 
            }
        }

        // Scroll wheel zoom
        if(Input.GetAxis("Mouse ScrollWheel") > 0f) {
            if(mainCamera.transform.position.y > 2) {
                mainCamera.transform.Translate(new Vector3(0, 0, 50f * Time.deltaTime));
            }
        } else if(Input.GetAxis("Mouse ScrollWheel") < 0f) {
            if(mainCamera.transform.position.y < 10) {
                mainCamera.transform.Translate(new Vector3(0, 0, -50f * Time.deltaTime));
            }
        }

        // Rotate
        if(Input.GetMouseButton(1)) {
            // The speed of the mouse movement
            float mouseDelta = Input.mousePosition.x - mousePos.x;

            // Some black magic fuckery going on right here, quaternions are fucking hard
            Quaternion rotation = mainCamera.transform.rotation;
            Quaternion newRotation = Quaternion.Euler(0, rotation.eulerAngles.y + 15 * mouseDelta * Time.deltaTime, 0);
            newRotation.eulerAngles = new Vector3(rotation.eulerAngles.x, newRotation.eulerAngles.y, rotation.eulerAngles.z);

            // I'm truly amazed that this works
            mainCamera.transform.rotation = newRotation;
        }    

        // Don't waste processing power unless camera moves
        if(didCameraMove) {
            foreach (var unit in allUnits) {
                if(unit.tag.Contains("Knight")){
                    unit.GetComponent<KnightController>().MoveHealthBar();
                } else if(unit.tag.Contains("Archer")){
                    unit.GetComponent<ArcherController>().MoveHealthBar();
                } else if(unit.tag.Contains("Wizard")){
                    unit.GetComponent<WizardController>().MoveHealthBar();
                }   
            }
        }    
    }

    // Update is called once per frame
    void Update() {
        MoveCamera();
        mousePos = Input.mousePosition;
        if(currButtonCanvas != null){
            grid.MoveButton(currButtonCanvas);
        }

        if(gameOver) {
            return;
        }

        if(blueUnitsRemaining == 0) {
            winnerLabel.text = "Red wins!";
            gameOver = true;
        } else if(redUnitsRemaining == 0) {
            winnerLabel.text = "Blue wins!";
            gameOver = true;
        }

        // Check for left mouse button click
        if(Input.GetMouseButtonDown(0)) {    
            // Did player click on a UI button? if so, don't do anything else
            if(EventSystem.current.IsPointerOverGameObject()) {
                print("thingy");
                return;
            }

            // Did player click on a unit?
            Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;
            bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);
            HexCell index; 

            // Did player click on a unit?
            if(didHit && (rhInfo.collider.gameObject.tag.Contains("Knight") || rhInfo.collider.gameObject.tag.Contains("Archer") 
              || rhInfo.collider.gameObject.tag.Contains("Wizard") || rhInfo.collider.gameObject.tag.Contains("Defeated"))){
                if(rhInfo.collider.gameObject.tag.Contains("Defeated")) return;
                if(!action) return; 
                if(rhInfo.collider.gameObject == currentUnit){
                    //if "defence" then display button and if pressed run "defend" command/ function below
                    if(SelectedCell != null){
                        grid.DisableButton(currButtonCanvas);
                    }
                    index = grid.DefenceCell(rhInfo.point);
                    currButtonCanvas = index.DefenceCanvas;
                    SelectedCell = index;
                    
                    return; 
                // No friendly fire!
                } else if((currentUnit.tag.Contains("Red") && rhInfo.collider.gameObject.tag.Contains("Red"))
                  || (currentUnit.tag.Contains("Blue") && rhInfo.collider.gameObject.tag.Contains("Blue"))) {
                    return;  
                }

                print("attack");

                //attack 
                //victim stuff put in global so that the other method doesn't need to get it (þarf script og þannig frá rhInfo.Colider stuff)
                victimLocation= rhInfo.collider.gameObject.GetComponent<Transform>().position;
                if(rhInfo.collider.gameObject.tag.Contains("Knight")) {
                    knightVictim = rhInfo.collider.gameObject.GetComponent<KnightController>();
                    VictimUnit = "Knight";
                } else if(rhInfo.collider.gameObject.tag.Contains("Archer")) {
                    archVictim = rhInfo.collider.gameObject.GetComponent<ArcherController>();
                    VictimUnit = "Archer";
                } else {
                    wizVictim = rhInfo.collider.gameObject.GetComponent<WizardController>();
                    VictimUnit = "Wizard";
                }
                //not sure this workds (clicks on unit at get's  unit's cell)
                if(SelectedCell != null){
                    grid.DisableButton(currButtonCanvas);
                }
                index = grid.AttackCell(rhInfo.point);
                currButtonCanvas = index.AttackCanvas;
                SelectedCell = index; 
                
            } else {
                if(!movement) return; 
                //if move unit, display button that if pressed runs "move unit"
                if(Physics.Raycast(toMouse, out rhInfo, 1000.0f)){
                    if(SelectedCell != null){
                        grid.DisableButton(currButtonCanvas);
                    }
                    index = grid.MovementCell(rhInfo.point);
                    currButtonCanvas = index.MoveCanvas;
                    SelectedCell = index; 
                }
            } 
        };

        //TODO: Remove dead bois from initiative animation
        CheckForDeadUnits();

        
        if(Input.GetMouseButtonDown(1)) {
            Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;
            bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);
            HexCell index; 
            if(Physics.Raycast(toMouse, out rhInfo, 1000.0f)){
                if(SelectedCell != null){
                    grid.DisableButton(currButtonCanvas);
                }
                index = grid.CreateUnitCell(rhInfo.point);
                currButtonCanvas = index.CreateCanvas;
                SelectedCell = index; 
                CreatedLocation = index.transform.position;
            }
        }
    }


    List<(double, double)> NullLocation = new List<(double, double)>(){ ( 0, 0),(1.5,3),(3.5,6),(5.5,9),(7,12),(8.5,15)};

    Vector3 GetMoveLocation(int x, int z) {
        var up = NullLocation[z];
        var left = (float)up.Item1 + (3.5 * x);
        return new Vector3 ((float)left, 0, (float)up.Item2);

    }

    public void MoveUnit(){
        if(SelectedCell == null) return; 
        var index = SelectedCell;
        Vector3 destination = GetMoveLocation(index.coordinates.X, index.coordinates.Z);
        if(currentUnit.tag.Contains("Knight")) {
            var script = currentUnit.GetComponent<KnightController>();
            bool move = script.StartMoving(destination, index);
            if(move) movement = false; 
        } else if(currentUnit.tag.Contains("Archer")) {
            var script = currentUnit.GetComponent<ArcherController>();  
            bool move = script.StartMoving(destination, index );
            if(move) movement = false; 
        } else {
            var script = currentUnit.GetComponent<WizardController>();
            bool move = script.StartMoving(destination, index );
            if(move) movement = false; 
        }
    }
    public void CreateUnit(string type){
        //check each unit in "tempunit" if their tag is equal to "type" (ex. redKnight)
        foreach(GameObject units in tempUnits) {
            if(units.tag.Contains(type)){ 
                GameObject FinalUnit = Instantiate<GameObject>(units);
                FinalUnit.transform.position = CreatedLocation; 
                FinalUnit.SetActive(true); 
                if(type == "BlueKnightCopy"){
                    FinalUnit.tag = "KnightBlue";
                    return;
                }else if(type == "BlueArcherCopy"){
                    FinalUnit.tag = "ArcherBlue";
                    return;
                }else if(type == "BlueWizardCopy"){
                    FinalUnit.tag = "WizardBlue";
                    return;
                }else if(type == "RedKnightCopy"){
                    FinalUnit.tag = "KnightRed";
                    return;
                }else if(type == "RedArcherCopy"){
                    FinalUnit.tag = "ArcherRed";
                    return;
                }else if (type == "RedWizardCopy"){
                    FinalUnit.tag = "WizardRed";
                    return;
                }
                return; 
            }
        }

    }
    public void UnitAttack(){
        int damage = 0;
        string type = "";
        bool InRange = false; 
        if(currentUnit.tag.Contains("Knight")) {
            var attackerScript = currentUnit.GetComponent<KnightController>();
            damage = attackerScript.baseDamage;
            type = attackerScript.type;
            InRange = attackerScript.Attack(victimLocation);
        } else if(currentUnit.tag.Contains("Archer")) {
            var attackerScript = currentUnit.GetComponent<ArcherController>();
            damage = attackerScript.baseDamage;
            type = attackerScript.type;
            InRange = attackerScript.Attack(victimLocation); 
            
        } else {
            var attackerScript = currentUnit.GetComponent<WizardController>();
            damage = attackerScript.baseDamage;
            type = attackerScript.type;
            InRange = attackerScript.Attack(victimLocation); 
            
        }
        
        if(InRange) {
                action = false; 
                movement = false;
            } else{
                action = true; 
                return;
            } 
        if(VictimUnit == "Knight"){
            knightVictim.TakeDamage(damage, type, 0.5f);
        }else if (VictimUnit == "Archer"){
            archVictim.TakeDamage(damage, type, 0.5f);
        }else if(VictimUnit == "Wizard"){
            wizVictim.TakeDamage(damage, type, 0.5f);
        }  
        
    }

    public void Defend(){
        //copy af koðanum til að láta unitið vera í "defend" ef hann clickar á sjálfan sig 
        if(currentUnit.tag.Contains("Knight")) {
            var attackerScript = currentUnit.GetComponent<KnightController>();
            attackerScript.Defend(); 
            action = false;
            movement = false; 
        } else if(currentUnit.tag.Contains("Archer")) {
            var attackerScript = currentUnit.GetComponent<ArcherController>();
            attackerScript.Defend();  
            action = false;
            movement = false; 
        } else {
            var attackerScript = currentUnit.GetComponent<WizardController>();
            attackerScript.Defend();  
            action = false;
            movement = false; 
        }

    }



    void CheckForDeadUnits() {
        // Check for any dead units and remove them from the list.
        // We need to store the units to remove in a list and remove them manually
        // because removing from a list while looping through it throws an error
        var unitsToRemove = new List<GameObject>();
        int index = 0;
        foreach(GameObject unit in allUnits) {
            if(unit.tag.Contains("Knight")) {
                var script = unit.GetComponent<KnightController>();
                if(script.IsUnitDead()) {
                    unitsToRemove.Add(unit);
                    if(index < currentUnitIndex) {
                        currentUnitIndex--;
                    }
                }
            } else if(unit.tag.Contains("Archer")) {
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
            if(unit.tag.Contains("Blue")) {
                blueUnitsRemaining--;
            } else {
                redUnitsRemaining--;                
            }
            unit.tag = "Defeated";
        }

        if(unitsToRemove.Count != 0) {
            SetInitiativePortraits();
            HighlightCurrentUnitsPortrait();
        }    
    }
}