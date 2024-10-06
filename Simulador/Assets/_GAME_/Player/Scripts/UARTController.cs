using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports; // Para la comunicación serial
using System; // Asegúrate de que este 'using' esté presente para manejar excepciones

public class UARTController : MonoBehaviour
{
    // UART interface
    public SerialPort serialPort = new SerialPort("COM8", 115200); // Ajusta el puerto COM según tu configuración
    private Player_Controller playerController; // Referencia al script del jugador (tractor)

    void Start()
    {
        playerController = GetComponent<Player_Controller>(); // Obtener el componente del controlador del tractor

        if (!serialPort.IsOpen)
        {
            serialPort.Open(); // Abrir conexión de puerto serie
            serialPort.ReadTimeout = 1; // Establecer tiempo de espera para la lectura
        }

        // Enviar un byte inicial para indicar que la conexión ha comenzado
        if (serialPort.IsOpen)
        {
            var dataByte = new byte[] { 0x00 };
            serialPort.Write(dataByte, 0, 1); // Enviar un dato para inicializar la comunicación con la FPGA
        }
    }

    void Update()
    {
        if (!serialPort.IsOpen)
        {
            serialPort.Open(); // Reabrir conexión si está cerrada
            serialPort.ReadTimeout = 1;
        }

        if (serialPort.IsOpen)
        {
            try
            {
                int receivedData;
                if (serialPort.BytesToRead > 0)
                {
                    receivedData = serialPort.ReadByte(); // Leer el dato recibido de la FPGA
                    ProcessReceivedData(receivedData); // Procesar el dato recibido
                    serialPort.Close(); // Cerrar la conexión después de leer
                }
            }
            catch (Exception e) // Agrega este bloque para manejar las excepciones
            {
                Debug.LogWarning("Error al leer desde el puerto serial: " + e.Message);
            }
        }

        // Actualizamos el movimiento del tractor en función de los datos recibidos
        UpdateMovement();
    }

    private void ProcessReceivedData(int receivedData)
    {
        // Aquí asignamos las acciones según los switches activados desde la FPGA
        switch (receivedData)
        {
            case 0x01: // Switch activado para mover a la derecha
                playerController._moveDir.x = 1; // Mover el tractor a la derecha
                break;
            case 0x02: // Switch activado para mover a la izquierda
                playerController._moveDir.x = -1; // Mover el tractor a la izquierda
                break;
            case 0x03: // Switch activado para mover hacia arriba
                playerController._moveDir.y = 1; // Mover el tractor hacia arriba
                break;
            case 0x04: // Switch activado para mover hacia abajo
                playerController._moveDir.y = -1; // Mover el tractor hacia abajo
                break;
            case 0x05: // Switch activado para frenar
                SetGear(Player_Controller.Gear.BRAKE);
                break;
            case 0x06: // Switch activado para cambiar a marcha adelante (Drive)
                SetGear(Player_Controller.Gear.FORWARD);
                break;
            case 0x07: // Switch activado para cambiar a marcha reversa
                SetGear(Player_Controller.Gear.REVERSE);
                break;    
            case 0x08: // Switch activado para poner en neutral
                SetGear(Player_Controller.Gear.NEUTRAL);
                break;
            default: // Si no se reconoce el dato, neutral
                playerController._moveDir = Vector2.zero; // Detener el movimiento si no hay entradas válidas
                playerController.CurrentGear = Player_Controller.Gear.NEUTRAL;
                break;
        }
    }

    private void SetGear(Player_Controller.Gear gear)
    {
        playerController.CurrentGear = gear; // Cambiar la marcha del tractor
    }

    private void UpdateMovement()
    {
        if (playerController.CurrentGear == Player_Controller.Gear.BRAKE)
        {
            // Si el freno está activado, el movimiento se detiene progresivamente
            playerController._moveDir = Vector2.zero; // Detener el movimiento
            playerController._currentSpeed = Mathf.MoveTowards(playerController._currentSpeed, 0, playerController._brakeForce * Time.deltaTime);
        }
        else if (playerController.CurrentGear != Player_Controller.Gear.NEUTRAL)
        {
            // Aplicar el movimiento según la dirección y la velocidad del tractor
            playerController._currentSpeed = Mathf.MoveTowards(playerController._currentSpeed, playerController._maxSpeed, playerController._accelerationRate * Time.deltaTime);
            Vector2 movement = playerController._moveDir.normalized * playerController._currentSpeed * Time.deltaTime;
            playerController._rb.velocity = movement;
        }
        else
        {
            // Si está en neutral, detener el movimiento
            playerController._rb.velocity = Vector2.zero;
        }
    }
}
