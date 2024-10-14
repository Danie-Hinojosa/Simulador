using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsInst : MonoBehaviour
{
    public void Quit(){
        SceneManager.LoadScene("Start Menu");
    }

    public void Keyboard(){
        SceneManager.LoadScene("Instructions");
    }

    public void fpga(){
        SceneManager.LoadScene("Instructionsfpga");
    }
}
