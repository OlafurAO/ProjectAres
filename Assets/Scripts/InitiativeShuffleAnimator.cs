using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeShuffleAnimator : MonoBehaviour {
    private List<GameObject> currPortraits = new List<GameObject>();
    private List<GameObject> prevPortraits = new List<GameObject>();
    private List<Vector2> currPortraitLocations = new List<Vector2>();
    private List<Vector2> prevPortraitLocations = new List<Vector2>();
    private int currentPortraitIndex;

    public Image overlay;
    public Canvas canvas;

    bool isFirstShuffle = true;
    bool isShuffling = false;
    bool isGatheringOldCards = false;
    int oldCardsGathered = 0;

    
    // Start is called before the first frame update
    void Start() {
        overlay.color = new Vector4(overlay.color.r, overlay.color.g, overlay.color.b, 0.5f);
    }

    // Update is called once per frame
    void Update() {
        if(isShuffling) {
            if(isGatheringOldCards) {
                foreach(GameObject portrait in prevPortraits) {
                    Vector2 currLocation = portrait.transform.localPosition;
                    Vector2 targetLocation = new Vector2(0f, 0f);
                    if(currLocation == targetLocation) {
                        oldCardsGathered++;    
                    } else {
                        portrait.transform.localPosition = Vector3.MoveTowards(currLocation, targetLocation, 5.5f);
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
                    Vector2 targetLocation = new Vector2(loc.x - 60 + currentPortraitIndex * 10, loc.y + 60);// - new Vector2(0f, 1f);
                    
                    if(currLocation != targetLocation) {
                        currPortraits[currentPortraitIndex].transform.localPosition = Vector3.MoveTowards(currLocation, targetLocation, 5f);
                    } else {
                        currentPortraitIndex++;
                    }
                } else {
                    prevPortraits = currPortraits;
                    prevPortraitLocations = currPortraitLocations;
                    overlay.gameObject.SetActive(false);
                    isShuffling = false;

                    if(isFirstShuffle) {
                        isFirstShuffle = false;
                    }
                }
            }    
        } 
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
            if(isFirstShuffle) {
                image.gameObject.transform.localScale = new Vector3(1.52f, 1.52f, 1.52f);
            }   
        }

        // Reverse the list again
        currPortraits.Reverse();
    }

    // Remove portrait from the previous portraits list if a unit dies
    public void RemovePrevPortrait(int index) {
        prevPortraits.RemoveAt(index);
        prevPortraitLocations.RemoveAt(index);
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

    public List<Vector2> GetCurrPortraitLocations() {
        return currPortraitLocations;
    }

    public bool IsShuffling() {
        return isShuffling;
    }
}
