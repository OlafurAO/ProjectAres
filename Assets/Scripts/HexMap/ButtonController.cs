using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



    public class ButtonController : MonoBehaviour
    {
        public Button butt; 
        public void DisableButton(){
            butt.gameObject.SetActive(false);
        }
        public void EnableButton(){
            butt.gameObject.SetActive(true);
        }
    }
