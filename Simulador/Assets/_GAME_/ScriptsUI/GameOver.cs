using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI maizText; // Texto que mostrará la cantidad de maíz recolectado
    public GameObject GameOverP;


    private void Start()
    {
        GameOverP.SetActive(false);
    }

    public void ShowGameOverMenu(int maizRecolectado)
    {
        GameOverP.SetActive(true);  // Mostrar el menú de Game Over
        maizText.text = "" + maizRecolectado.ToString();  // Actualizar el texto
        Time.timeScale = 0f;  // Detener el tiempo del juego
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeTown");
    }

    public void Quit()
    {
        SceneManager.LoadScene("Start Menu");
    }
}
