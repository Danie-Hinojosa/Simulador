using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public Image timer_fr_image;
    float time_remaining;
    public float max_time = 5.0f;
    private Player_Controller playerController;
    private Player_Controller_Serial playerControllerSerial;
    public GameOver gameOverManager;

    void Start()
    {
        ResetTimer();
        time_remaining = max_time;

        // Detectar cuál controlador está activo al iniciar
        playerController = FindObjectOfType<Player_Controller>();
        playerControllerSerial = FindObjectOfType<Player_Controller_Serial>();
    }

    void Update()
    {
        if (time_remaining > 0)
        {
            time_remaining -= Time.deltaTime;
            timer_fr_image.fillAmount = time_remaining / max_time;
        }
        else
        {
            EndGame();
        }
    }

    void EndGame()
    {
        // Verificar cuál controlador está activo y pasar el maíz recolectado al Game Over
        if (playerController != null && playerController.enabled)
        {
            gameOverManager.ShowGameOverMenu(playerController.CollectedMaizeCount);
        }
        else if (playerControllerSerial != null && playerControllerSerial.enabled)
        {
            gameOverManager.ShowGameOverMenu(playerControllerSerial.CollectedMaizeCount);
        }
    }

    public void ResetTimer()
    {
        time_remaining = max_time;  // Restablece el tiempo al máximo
    }
}
