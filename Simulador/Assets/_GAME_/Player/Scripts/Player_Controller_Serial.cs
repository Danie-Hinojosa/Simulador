using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports; // Para la comunicación serial
using System; // Para manejar excepciones

[SelectionBase]
public class Player_Controller_Serial : MonoBehaviour
{
    private float _serialReadTimer = 0f; // Temporizador para leer del puerto serial
    private float _serialReadInterval = 0f; // Intervalo de lectura en segundos
    #region Enums
    private enum Directions { UP, DOWN, LEFT, RIGHT }
    public enum Gear { NEUTRAL, FORWARD, REVERSE, BRAKE }
    #endregion

    #region Editor Data
    [Header("Movement Attributes")]
    public float _maxSpeed = 50f;        // Velocidad máxima hacia adelante
    public float _reverseSpeed = 25f;    // Velocidad máxima en reversa
    public float _accelerationRate = 5f; // Tasa de aceleración progresiva
    public float _brakeForce = 10f;      // Fuerza de frenado

    [Header("Dependencies")]
    public Rigidbody2D _rb;
    [SerializeField] public Animator _animator;
    [SerializeField] public SpriteRenderer _spriteRenderer;
    [SerializeField] private Text maizText;    // Texto para la cantidad de maíz recolectado
    [SerializeField] private Text speedText;   // Texto para la velocidad del tractor
    [SerializeField] private Text gearText;    // Texto para el estado de la marcha
    #endregion

    #region Internal Data
    private Vector2 _moveDir = Vector2.zero;
    private Directions _facingDirections = Directions.RIGHT;

    private Gear _currentGear = Gear.NEUTRAL; // Gear inicial
    private float _currentSpeed = 0f;          // Velocidad actual del tractor
    private float _targetSpeed = 0f;           // Velocidad objetivo que se irá alcanzando progresivamente
     private readonly int _animMoveRight = Animator.StringToHash("Anim_Player_Move_Right");
    private readonly int _animIdleRight = Animator.StringToHash("Anim_Player_Idle_Right");
    private readonly int _animMoveUp = Animator.StringToHash("Anim_Player_Move_Up");
    private readonly int _animIdleUp = Animator.StringToHash("Anim_Player_Idle_Up");
    private readonly int _animMoveDown = Animator.StringToHash("Anim_Player_Move_Down"); // Nueva animación para Down
    private readonly int _animIdleDown = Animator.StringToHash("Anim_Player_Idle_Down"); // Nueva animación idle para Down
    private int _collectedMaizeCount = 0;      // Contador de maíz recolectado
    [SerializeField] Text collectedCountText;   // Referencia al texto de la UI para mostrar la cantidad recolectada

    // UART interface
    public SerialPort serialPort = new SerialPort("COM7", 115200); // Ajusta el puerto COM según tu configuración
    
    
    
    #endregion

    public int CollectedMaizeCount => _collectedMaizeCount; // Proporciona acceso a la cantidad de maíz recolectado
    public float CurrentSpeed => _currentSpeed;             // Proporciona acceso a la velocidad actual
    public Gear CurrentGear { get { return _currentGear; } set { _currentGear = value; } } // Proporciona acceso al estado de la marcha

    #region Tick

    private void Start()
    {
        if (!serialPort.IsOpen)
        {
            serialPort.Open(); // Abrir conexión de puerto serie
            serialPort.ReadTimeout = 1; // Establecer tiempo de espera para la lectura
            Debug.Log("Puerto abierto");
            Update();
        }
    }

    private void Update()
{
    _serialReadTimer += Time.deltaTime;

    // Intenta leer del puerto serial lo más frecuentemente posible (cada frame)
    if (serialPort.IsOpen)
    {
        try
        {
            int receivedData;
            while (serialPort.BytesToRead > 0)
            {
                receivedData = serialPort.ReadByte(); // Leer el dato recibido de la FPGA
                Debug.Log("Dato recibido: " + receivedData);
                ProcessReceivedData(receivedData); // Procesar el dato recibido
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error al leer desde el puerto serial: " + e.Message);
        }
    }

    // Enviar la cantidad de maíz recolectado a la FPGA
    SendMaizeCollected();
    // Actualiza la UI
    UpdateUI();
    UpdateAnimation();
}

    private void FixedUpdate() 
    {
        MovementUpdate();
    }
    #endregion

    #region Movement Logic
    public void SetMovementDirection(Vector2 moveDir)
    {
        _moveDir = moveDir; // Establece la dirección de movimiento basada en los datos recibidos
    }

    private void MovementUpdate()
    {
        // Ajusta la velocidad objetivo según la marcha seleccionada
        switch (_currentGear)
        {
            case Gear.FORWARD:
                _targetSpeed = _maxSpeed; // Establece la velocidad objetivo máxima hacia adelante
                break;
            case Gear.REVERSE:
                _targetSpeed = _reverseSpeed; // Establece la velocidad objetivo máxima en reversa
                break;
            case Gear.BRAKE:
                _targetSpeed = 0f; // Velocidad objetivo es 0 al frenar
                break;
            case Gear.NEUTRAL:
                _targetSpeed = 0f; // En neutral, la velocidad objetivo es 0
                break;
        }

        // Incrementa o disminuye la velocidad progresivamente hacia la velocidad objetivo
        if (_currentGear != Gear.BRAKE)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, _accelerationRate * Time.fixedDeltaTime);
        }

        // Movimiento en dirección según la velocidad actual
        Vector2 movement = _moveDir.normalized * _currentSpeed * Time.fixedDeltaTime;
        _rb.velocity = movement;
    }
    #endregion

    #region Collision Logic
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Maiz"))
        {
            _collectedMaizeCount++; // Incrementa el contador de maíz
            Destroy(other.gameObject); // Elimina el objeto de maíz de la escena
            UpdateUI(); // Actualiza la UI inmediatamente
        }
    }
    #endregion

    #region UART Logic
private void ProcessReceivedData(int receivedData)
    {
        // Aquí asignamos las acciones según los bits del switch activados desde la FPGA
        switch (receivedData)
        {
            case 1: // 1: Primer switch activado
                if (_currentGear == Gear.FORWARD || _currentGear == Gear.REVERSE) // Verifica el estado de la marcha
                {
                    _facingDirections = Directions.RIGHT;
                    SetMovementDirection(new Vector2(1, 0)); // Mover a la derecha
                }
                break;
            case 2: // 2: Segundo switch activado
                if (_currentGear == Gear.FORWARD || _currentGear == Gear.REVERSE)
                {
                    _facingDirections = Directions.LEFT;
                    SetMovementDirection(new Vector2(-1, 0)); // Mover a la izquierda
                }
                break;
            case 4: // 4: Tercer switch activado
                if (_currentGear == Gear.FORWARD || _currentGear == Gear.REVERSE)
                {
                _facingDirections = Directions.UP;
                    SetMovementDirection(new Vector2(0, 1)); // Mover hacia arriba
                }
                break;
            case 8: // 8: Cuarto switch activado
                if (_currentGear == Gear.FORWARD || _currentGear == Gear.REVERSE)
                {
                _facingDirections = Directions.DOWN;                    
                    SetMovementDirection(new Vector2(0, -1)); // Mover hacia abajo
                }
                break;
            case 16: // 16: Quinto switch activado
                
                SetGear(Gear.BRAKE); // Frenar
                break;
            case 32: // 32: Sexto switch activado
                
                SetGear(Gear.FORWARD); // Marcha hacia adelante
                break;
            case 64: // 64: Séptimo switch activado
                
                    SetGear(Gear.REVERSE); // Marcha en reversa
                break;
            case 128: // 128: Octavo switch activado
                
                    SetGear(Gear.NEUTRAL); // Cambiar a neutral
                break;
            default:
                break; // Ningún switch activado
        }
    }

    private void SetGear(Gear gear)
    {
        CurrentGear = gear; // Cambiar la marcha del tractor
    }

    private void SendMaizeCollected()
    {
        if (serialPort.IsOpen)
        {
            var maizeCount = (byte)CollectedMaizeCount; // Obtener la cantidad de maíz recolectado
            serialPort.Write(new byte[] { maizeCount }, 0, 1); // Enviar la cantidad recolectada a la FPGA
        }
    }

    private void OnApplicationQuit()
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close(); // Cerrar el puerto serie al salir de la aplicación
        }
    }
    #endregion
#region Animation Logic
    private void CalculateFacingDirection()
    {
        if (_moveDir.x != 0)
        {
            if (_moveDir.x > 0)
            {
                _facingDirections = Directions.RIGHT;
            }
            else if (_moveDir.x < 0)
            {
                _facingDirections = Directions.LEFT;
            }
        }
        else if (_moveDir.y > 0)
        {
            _facingDirections = Directions.UP; // Dirección hacia arriba
        }
        else if (_moveDir.y < 0)
        {
            _facingDirections = Directions.DOWN; // Nuevo: Dirección hacia abajo
        }
    }

    private void UpdateAnimation()
    {
        // Verifica si hay movimiento
        if (_moveDir.SqrMagnitude() == 0)
        {
            // Si no hay movimiento, activa la animación idle correspondiente
            switch (_facingDirections)
            {
                case Directions.LEFT:
                    _animator.CrossFade(_animIdleRight, 0); // Usamos la misma animación idle para ambos lados
                    break;

                case Directions.RIGHT:
                    _animator.CrossFade(_animIdleRight, 0);
                    break;
                // No hacemos nada para UP y DOWN
            }
        }
        else
        {
            // Si hay movimiento, activa la animación de movimiento solo en la dirección actual
            switch (_facingDirections)
            {
                case Directions.LEFT:
                    _spriteRenderer.flipX = true; // Activar flip solo para el lado izquierdo
                    _animator.CrossFade(_animMoveRight, 0); // Animación de movimiento hacia la izquierda
                    break;

                case Directions.RIGHT:
                    _spriteRenderer.flipX = false; // Desactivar flip para el lado derecho
                    _animator.CrossFade(_animMoveRight, 0); // Animación de movimiento hacia la derecha
                    break;
                // No hacemos nada para UP y DOWN
            }
        }
    }
    #endregion

    #region UI Logic
    private void UpdateUI()
    {
        if (maizText != null)
        {
            maizText.text = "Maíz Recolectado: " + _collectedMaizeCount;
        }

        if (speedText != null)
        {
            speedText.text = "Velocidad: " + Mathf.Round(_currentSpeed);
        }

        if (gearText != null)
        {
            gearText.text = "Marcha: " + _currentGear.ToString();
        }
    }
    #endregion
}