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


    // Referencia al controlador del jugador
    [SerializeField] private Player_Controller playerController;

    private void Start()
    {
        // Comprueba que las referencias estén asignadas
        if (playerController == null)
        {
            Debug.LogError("Player_Controller no está asignado en el UIManager.");
        }

        if (maizText == null || speedText == null || gearText == null)
        {
            Debug.LogError("Uno o más elementos de texto no están asignados en el UIManager.");
        }
    }

    private void Update()
    {
        if (playerController != null)
        {
            UpdateUI(); // Actualiza la información de la UI
        }
    }

    // Actualiza los textos de la UI
    private void UpdateUI()
    {
        // Actualiza la cantidad de maíz recolectado
        if (maizText != null)
        {
            maizText.text = "" + playerController.CollectedMaizeCount;
        }

        // Actualiza la velocidad del tractor
        if (speedText != null)
        {
            speedText.text = "" + Mathf.Round(playerController.CurrentSpeed); // Redondear para mostrar un valor más limpio
        }

        // Actualiza el estado de la marcha
        if (gearText != null)
        {
            switch (playerController.CurrentGear)
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
            }
        }
    }
}
