using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimerController : MonoBehaviour
{
    public Image timer_fr_image;
    float time_remaining;
    public float max_time = 5.0f;
    private Player_Controller playerController;
    public GameOver gameOverManager;
    void Start()
    {
        ResetTimer();
        time_remaining = max_time;
        playerController = FindObjectOfType<Player_Controller>();
    }

    // Update is called once per frame
    void Update()
    {
        if (time_remaining > 0)
        {
            time_remaining -= Time.deltaTime;
            timer_fr_image.fillAmount = time_remaining / max_time;
        }else{
            EndGame();
        }
    }

    void EndGame()
    {
        // Mostrar el menú de Game Over y pasar la cantidad de maíz recolectado
        gameOverManager.ShowGameOverMenu(playerController.CollectedMaizeCount);
    }

    public void ResetTimer()
    {
        time_remaining = max_time;  // Restablece el tiempo al máximo
    }
}
