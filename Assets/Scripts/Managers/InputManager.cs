using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InputManager : MonoBehaviour
{

    public static InputManager Instance { get; private set; }

    

    public bool mouseActive = true;
    [System.Serializable]
    public struct CameraPreset
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public float autoplayCamSpeed = 2f;
    public float xAutoRotateTime = 5f;
    public float yAutoRotateTime = 5f;
    private Coroutine autoplayRoutine;
    

    public bool lockUserInput = false;

    private float _yaw = 0.0f;
    private float _pitch = 0.0f;

    private float reloadModifier;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    
  
    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            CameraController.Instance.OrbitCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            
        }
        else if (Input.GetMouseButton(1))
        {
            CameraController.Instance.RotateCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
    }


    private void HandleInput()
    {
        if (!lockUserInput)
        {
            HandleLockableInput();
        }
        HandleNonLockableInput();
    }

    void HandleScrollWheelInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            CameraController.Instance.ZoomCamera(Input.GetAxis("Mouse ScrollWheel") * 500);
        }
    }

    void HandleLockableInput()
    {

        HandleMouseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            reloadModifier = -.1f;
            _cameraSpeed = _cameraSpeedMax;
        }
        else
        {
            reloadModifier = .1f;
            _cameraSpeed = _cameraSpeedNormal;
        }
        
        // TRANSLATIONAL MOVEMENT
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            CameraController.Instance.TranslateCamera(CameraController.TranslationInput.Forward);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            CameraController.Instance.TranslateCamera(CameraController.TranslationInput.Left);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            CameraController.Instance.TranslateCamera(CameraController.TranslationInput.Back);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            CameraController.Instance.TranslateCamera(CameraController.TranslationInput.Right);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            CameraController.Instance.TranslateCamera(CameraController.TranslationInput.Up);
        }
        if (Input.GetKey(KeyCode.E))
        {
            CameraController.Instance.TranslateCamera(CameraController.TranslationInput.Down);
        }

    }

    void HandleNonLockableInput()
    {
        HandleScrollWheelInput();
        if (Input.GetKeyDown(KeyCode.I))


        if (Input.GetKeyDown(KeyCode.C))
        {

        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Pause the time
            if (SimManager.Instance.IsSimulationRunning()) {
                SimManager.Instance.PauseSimulation();
            } else {
                SimManager.Instance.ResumeSimulation();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            autoRotate = !autoRotate;
            if (autoRotate)
            {
                autoplayRoutine = StartCoroutine(AutoPlayRoutine()); 
            }
            else
            {
                StopCoroutine( autoplayRoutine );
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // ORIGIN
            // Set pre-set view 1 
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Set pre-set view 2

        }


        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Set pre-set view 4
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // Set pre-set view 5
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            // Set pre-set view 6
        }


    }

}
