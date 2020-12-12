using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeShuffleAnimator : MonoBehaviour {
    private List<GameObject> currPortraits = new List<GameObject>();
    private List<GameObject> prevPortraits = new List<GameObject>();
    private List<Vector2> currPortraitLocations = new List<Vector2>();
    private List<Vector2> prevPortraitLocations = new List<Vector2>();
    private GameObject portraitToDestroy; // When a unit gets defeated, save it into this variable and play a litle animation for it
    private int currentPortraitIndex;

    public Image overlay;
    public TMPro.TextMeshProUGUI roundStartText;
    public Canvas canvas;

    bool isFirstShuffle = true;
    bool isShuffling = false;
    bool isGatheringOldCards = false;
    bool isDestroyingPortrait = false;
    bool isShiftingPortraits = false;
    bool isComplete = false;
    int oldCardsGathered = 0;
    int removeIndex = -1;
    int portraitsToShift = -1; // How many portraits need to be shifted if a unit is defeated

    
    // Start is called before the first frame update
    void Start() {
        overlay.color = new Vector4(overlay.color.r, overlay.color.g, overlay.color.b, 0.5f);
    }

    // Update is called once per frame
    void Update() {
        if(isDestroyingPortrait) {
            var color = portraitToDestroy.gameObject.GetComponent<Image>().color;
            if(color.a <= 0f) {
                Destroy(portraitToDestroy);
                isDestroyingPortrait = false;
                portraitToDestroy = null;
            } else {
                portraitToDestroy.gameObject.GetComponent<Image>().color = new Vector4(color.r, color.g, color.b, color.a - 0.02f);
            }
        } else if(isShiftingPortraits) {
            for(int i = removeIndex; i < prevPortraits.Count; i++) {
                Vector2 currLocation = prevPortraits[i].transform.localPosition;
                var loc = prevPortraitLocations[i];
                Vector2 targetLocation = new Vector2(loc.x - 60, loc.y + 60);

                if(currLocation != targetLocation) {
                    prevPortraits[i].transform.localPosition = Vector3.MoveTowards(currLocation, targetLocation, 1f);
                } else {
                    portraitsToShift--;
                }
            }

            if(portraitsToShift <= 0) {
                isShiftingPortraits = false;
                portraitsToShift = -1;
            }
        }
      
        if(isShuffling) {
            if(isGatheringOldCards) {
                foreach(GameObject portrait in prevPortraits) {
                    Vector2 currLocation = portrait.transform.localPosition;
                    Vector2 targetLocation = new Vector2(0f, 0f);
                    if(currLocation == targetLocation) {
                        oldCardsGathered++;    
                    } else {
                        portrait.transform.localPosition = Vector3.MoveTowards(currLocation, targetLocation, 8f);
                        currLocation = portrait.transform.localPosition;
                    }                    
                }

                if(oldCardsGathered >= prevPortraits.Count) {
                    DestroyOldPortraits();
                    isGatheringOldCards = false;
                    oldCardsGathered = 0;
                }
            } else {
                if(currentPortraitIndex < currPortraits.Count) {
                    Vector2 currLocation = currPortraits[currentPortraitIndex].transform.localPosition;

                    var loc = currPortraitLocations[currentPortraitIndex];
                    Vector2 targetLocation = new Vector2(loc.x - 60, loc.y + 60);// - new Vector2(0f, 1f);
                    
                    if(currLocation != targetLocation) {
                        currPortraits[currentPortraitIndex].transform.localPosition = Vector3.MoveTowards(currLocation, targetLocation, 7f);
                    } else {
                        currentPortraitIndex++;
                    }
                } else {
                    if(!isComplete) {
                        prevPortraits = currPortraits;
                        prevPortraitLocations = currPortraitLocations;
                        overlay.gameObject.SetActive(false);

                        roundStartText.gameObject.SetActive(true);
                        FindObjectOfType<AudioManager>().Play("round_start", 0.0f);
                        StartCoroutine(StartRoundWithDelay());

                        isComplete = true;

                        if(isFirstShuffle) {
                            isFirstShuffle = false;
                        }
                    }    
                }
            }    
        } 
    }

    IEnumerator StartRoundWithDelay() {
        yield return new WaitForSeconds(3f);
        roundStartText.gameObject.SetActive(false);
        isShuffling = false;
    }

    public void SetNewPortraits(List<GameObject> newPortraits, List<Vector2> newPortraitLocations) {
        currPortraits = newPortraits;
        currPortraitLocations = newPortraitLocations;
        currentPortraitIndex = 0;
    }

    public void StartShuffling() {
        overlay.gameObject.SetActive(true);
        //canvas.gameObject.SetActive(false);
        Shuffle();

        isShuffling = true;
        if(!isFirstShuffle) {
            isGatheringOldCards = true;
        } 
    }

    public void Shuffle() {
        // Reverse the list so that the first portrait will be the last gameobject to be added
        // and therefore be on top of the pile
        currPortraits.Reverse();

        // Set the animator game object as the portrait's parents and set their location in the middle of the screen
        foreach(GameObject image in currPortraits) {
            image.gameObject.transform.SetParent(this.gameObject.transform);
            image.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

            // During the first round, the portraits are small for some reason but not in later rounds.
            // This code fixes their size
            if(!isFirstShuffle) {
                image.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            }   
        }

        // Reverse the list again
        currPortraits.Reverse();
    }

    // Remove portrait from the previous portraits list if a unit dies
    public void RemovePrevPortrait(int index) {
        portraitToDestroy = prevPortraits[index];
        prevPortraits.RemoveAt(index);
        
        // Remove the last location, the portrait after the deleted portrait gets the deleted portrait's location etc.
        prevPortraitLocations.RemoveAt(prevPortraitLocations.Count - 1);
        
        removeIndex = index;
        portraitsToShift = prevPortraits.Count - index;
        isDestroyingPortrait = true;
        isShiftingPortraits = true;
    }

    // Destroy portraits from previous round
    void DestroyOldPortraits() {
        foreach(GameObject portrait in prevPortraits) {
            Destroy(portrait);
        }
        /*
        var portraits = GameObject.FindGameObjectsWithTag("Portrait");
        for(var i = 0; i < portraits.Length; i++) {
            Destroy(portraits[i]);
        }
        */
    }

    public void DestroyCurrentHighlighter() {
        // Destroy previous highlighters
        var highlighters = GameObject.FindGameObjectsWithTag("Highlighter");
        for(var i = 0; i < highlighters.Length; i++) {
            Destroy(highlighters[i]);
        }
    }

    public void HighlightCurrentUnitsPortrait(int index) {
        GameObject highlighter = new GameObject("Highlighter");
        highlighter.tag = "Highlighter";
        RectTransform trans = highlighter.AddComponent<RectTransform>();

        // Set the canvas as a parent
        trans.transform.SetParent(this.gameObject.transform);
        // Not sure what this does but I'm sure it does something
        trans.localScale = Vector3.one;
        
        // Sets the position of the highlighter
        trans.anchoredPosition = new Vector2(prevPortraitLocations[index].x - 60, prevPortraitLocations[index].y + 60); //new Vector2(200, Screen.height/2 - 50);
        // Sets the size of the highlighter
        trans.sizeDelta = new Vector2(50, 50);

        Image image = highlighter.AddComponent<Image>();        
        Texture2D tex = Resources.Load<Texture2D>("Images/Shapes/square");
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        highlighter.transform.SetParent(canvas.transform);
    }

    public List<Vector2> GetCurrPortraitLocations() {
        return currPortraitLocations;
    }

    public bool IsShuffling() {
        return isShuffling;
    }
}
