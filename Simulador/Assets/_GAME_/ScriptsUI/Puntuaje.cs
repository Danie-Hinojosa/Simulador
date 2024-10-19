using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Referencias a los elementos de texto del Canvas
    [SerializeField] private TextMeshProUGUI maizText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI gearText;

    // Referencias a los controladores del jugador
    [SerializeField] private Player_Controller playerController;
    [SerializeField] private Player_Controller_Serial playerControllerSerial;

    private void Start()
    {
        // Comprueba que al menos una referencia esté asignada
        if (playerController == null && playerControllerSerial == null)
        {
            Debug.LogError("Ningún controlador del jugador está asignado en el UIManager.");
        }

        if (maizText == null || speedText == null || gearText == null)
        {
            Debug.LogError("Uno o más elementos de texto no están asignados en el UIManager.");
        }
    }

    private void Update()
    {
        // Verifica cuál controlador está activo y actualiza la UI
        if (playerController != null && playerController.isActiveAndEnabled)
        {
            UpdateUI(playerController); // Actualiza la UI usando Player_Controller
        }
        else if (playerControllerSerial != null && playerControllerSerial.isActiveAndEnabled)
        {
            UpdateUI(playerControllerSerial); // Actualiza la UI usando Player_Controller_Serial
        }
    }

    // Método genérico para actualizar los textos de la UI
    private void UpdateUI(dynamic controller)
    {
        // Actualiza la cantidad de maíz recolectado
        if (maizText != null)
        {
            maizText.text = "" + controller.CollectedMaizeCount;
        }

        // Actualiza la velocidad del tractor
        if (speedText != null)
        {
            speedText.text = "" + Mathf.Round(controller.CurrentSpeed); // Redondear para mostrar un valor más limpio
        }

        // Actualiza el estado de la marcha
        if (gearText != null)
        {
            switch (controller.CurrentGear)
            {
                case Player_Controller.Gear.FORWARD:
                    gearText.text = "Drive";
                    break;
                case Player_Controller.Gear.REVERSE:
                    gearText.text = "Reverse";
                    break;
                case Player_Controller.Gear.NEUTRAL:
                    gearText.text = "Neutral";
                    break;
                case Player_Controller_Serial.Gear.FORWARD:
                    gearText.text = "Drive";
                    break;
                case Player_Controller_Serial.Gear.REVERSE:
                    gearText.text = "Reverse";
                    break;
                case Player_Controller_Serial.Gear.NEUTRAL:
                    gearText.text = "Neutral";
                    break;
            }
        }
    }
}