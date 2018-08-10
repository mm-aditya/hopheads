using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class buttonclicks : MonoBehaviour
{
    //Make sure to attach these Buttons in the Inspector
    public Button m_YourFirstButton, m_YourSecondButton;
    public GameObject mainMenu;
    public GameObject controls;
    public GameObject chars;

    void Start()
    {
        Button btn1 = m_YourFirstButton.GetComponent<Button>();
        Button btn2 = m_YourSecondButton.GetComponent<Button>();

        //Calls the TaskOnClick/TaskWithParameters method when you click the Button
        btn1.onClick.AddListener(TaskOnClick);
        btn2.onClick.AddListener(TaskOnClick2);
    }

    void TaskOnClick()
    {
        //Output this to console when the Button is clicked
        Debug.Log("You have clicked the play button!");
        mainMenu.SetActive(false);
        controls.SetActive(false);
        chars.SetActive(true);
    }

    void TaskOnClick2()
    {
        //Output this to console when the Button is clicked
        Debug.Log("You have clicked the controls button!");
        mainMenu.SetActive(false);
        controls.SetActive(true);
        
    }


}
