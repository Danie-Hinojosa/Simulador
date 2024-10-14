using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainStartMenu : MonoBehaviour // Renombrado de StartMenu a MainStartMenu
{
    public void Jugar()
    {
        SceneManager.LoadScene("HomeTown");
    }

    public void Instrucciones()
    {
        SceneManager.LoadScene("OptionsInst");
    }

    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }	
}

