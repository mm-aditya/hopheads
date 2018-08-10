using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class controlsbackbut : MonoBehaviour {

    public Button m_button;
    public GameObject mainMenu;
    public GameObject controls;

    // Use this for initialization
    void Start () {
        Button btn1 = m_button.GetComponent<Button>();
        btn1.onClick.AddListener(TaskOnClick);


    }

    void TaskOnClick()
    {
        //Output this to console when the Button is clicked
        Debug.Log("You have clicked the button!");
        controls.SetActive(false);
        mainMenu.SetActive(true);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
