using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


[SelectionBase]
public class Player_Controller : MonoBehaviour
{
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
    public Vector2 _moveDir = Vector2.zero;
    private Directions _facingDirections = Directions.RIGHT;
    
    public Gear _currentGear = Gear.NEUTRAL; // Gear inicial
    private Gear _previousGear = Gear.NEUTRAL; // Para almacenar la marcha anterior

    public float _currentSpeed = 0f;         // Velocidad actual del tractor
    private float _targetSpeed = 0f;          // Velocidad objetivo que se irá alcanzando progresivamente
    private readonly int _animMoveRight = Animator.StringToHash("Anim_Player_Move_Right");
    private readonly int _animIdleRight = Animator.StringToHash("Anim_Player_Idle_Right");
    private readonly int _animMoveUp = Animator.StringToHash("Anim_Player_Move_Up");
    private readonly int _animIdleUp = Animator.StringToHash("Anim_Player_Idle_Up");
    private readonly int _animMoveDown = Animator.StringToHash("Anim_Player_Move_Down"); // Nueva animación para Down
    private readonly int _animIdleDown = Animator.StringToHash("Anim_Player_Idle_Down"); // Nueva animación idle para Down
    private int _collectedMaizeCount = 0;     // Contador de maíz recolectado
    [SerializeField] Text collectedCountText; // Referencia al texto de la UI para mostrar la cantidad recolectada
    #endregion

    // Propiedades públicas para permitir que el UIManager acceda a la información
    public int CollectedMaizeCount => _collectedMaizeCount; // Proporciona acceso a la cantidad de maíz recolectado
    public float CurrentSpeed => _currentSpeed;             // Proporciona acceso a la velocidad actual
    public Gear CurrentGear { get { return _currentGear; } set { _currentGear = value; } } // Proporciona acceso al estado de la marcha

    #region Tick

    private void Update()
    {
        GatherInput();
        CalculateFacingDirection();
        UpdateAnimation();
        UpdateGear(); // Actualiza la marcha según las entradas
        UpdateUI(); // Actualiza la interfaz de usuario

    }

    private void FixedUpdate() 
    {
        MovementUpdate();
    }
    #endregion

    #region Input Logic
    private void GatherInput()
    {
        // Recoge entradas para movimiento
        _moveDir.x = Input.GetAxisRaw("Horizontal");
        _moveDir.y = Input.GetAxisRaw("Vertical");

        // Verifica si la tecla espacio está siendo presionada para activar el freno
        if (Input.GetKey(KeyCode.Space))
        {
            // Guardamos la marcha actual antes de frenar
            if (_currentGear != Gear.BRAKE)
            {
                _previousGear = _currentGear;
            }
            _currentGear = Gear.BRAKE; // Freno activo
        }
        else if (Input.GetKeyUp(KeyCode.Space)) // Cuando se suelta la tecla de freno
        {
            _currentGear = _previousGear; // Vuelve a la marcha anterior
        }
        else if (Input.GetKeyDown(KeyCode.E)) // Cambié de "W" a "E" para la aceleración
        {
            _currentGear = Gear.FORWARD; // Aceleración
        }
        else if (Input.GetKeyDown(KeyCode.R)) // Reversa con "R"
        {
            _currentGear = Gear.REVERSE; // Reversa
        }
        else if (Input.GetKeyDown(KeyCode.N)) 
        {
            _currentGear = Gear.NEUTRAL; // Neutral
        }
    }
    #endregion

    #region Movement Logic
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
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, _brakeForce * Time.fixedDeltaTime); // Disminuir la velocidad progresivamente
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
        if (_currentGear == Gear.REVERSE)
        {
            movement = -movement; // Invertir dirección en reversa
        }
        _rb.velocity = movement;
    }
    #endregion

    #region Gear Logic
    private void UpdateGear()
    {
        // Mostrar estado actual de la marcha en la consola (opcional, para depuración)
        Debug.Log($"Current Gear: {_currentGear}, Speed: {_currentSpeed}");
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
        switch (_facingDirections)
        {
            case Directions.LEFT:
                _spriteRenderer.flipX = true;
                _animator.CrossFade(_animMoveRight, 0);
                break;

            case Directions.RIGHT:
                _spriteRenderer.flipX = false;
                _animator.CrossFade(_animMoveRight, 0);
                break;

            case Directions.UP:
                _spriteRenderer.flipX = false; // Asumimos que la animación hacia arriba no necesita inversión
                if (_moveDir.SqrMagnitude() > 0)
                {
                    _animator.CrossFade(_animMoveUp, 0); // Activar animación de movimiento hacia arriba
                }
                else
                {
                    _animator.CrossFade(_animIdleUp, 0); // Activar animación de idle hacia arriba
                }
                break;

            case Directions.DOWN:
                _spriteRenderer.flipX = false; // Asumimos que la animación hacia abajo no necesita inversión
                if (_moveDir.SqrMagnitude() > 0)
                {
                    _animator.CrossFade(_animMoveDown, 0); // Activar animación de movimiento hacia abajo
                }
                else
                {
                    _animator.CrossFade(_animIdleDown, 0); // Activar animación de idle hacia abajo
                }
                break;
        }

        // Animación idle cuando no hay movimiento
        if (_moveDir.SqrMagnitude() == 0 && _facingDirections != Directions.UP && _facingDirections != Directions.DOWN)
        {
            _animator.CrossFade(_animIdleRight, 0);
        }
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

    #region UI Logic
    private void UpdateUI()
    {
        if (maizText != null)
        {
            maizText.text = "Maíz Recolectado: " + _collectedMaizeCount;
        }

        if (speedText != null)
        {
            speedText.text = "Velocidad: " + Mathf.Round(_currentSpeed); // Redondear la velocidad para mostrar un valor más limpio
        }

        if (gearText != null)
        {
            switch (_currentGear)
            {
                case Gear.FORWARD:
                    gearText.text = "Marcha: Drive";
                    break;
                case Gear.REVERSE:
                    gearText.text = "Marcha: Reverse";
                    break;
                case Gear.NEUTRAL:
                    gearText.text = "Marcha: Neutral";
                    break;
            }
        }
    }
    #endregion



}