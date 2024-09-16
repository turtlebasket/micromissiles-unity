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

    private float _cameraSpeed;
    public float _cameraSpeedNormal = 5.0f;
    public float _cameraSpeedMax = 10.0f;

    public float _speedH = 2.0f;
    public float _speedV = 2.0f;

    private float _yaw = 0.0f;
    private float _pitch = 0.0f;

    private float reloadModifier;

    // Orbit controller
    public bool autoRotate = false;
    
    public Transform target;
    public Renderer targetRenderer;
    public Renderer floorRenderer;
    public float matAlpha;
    public float orbitDistance = 5.0f;
    public float orbitXSpeed = 120.0f;
    public float orbitYSpeed = 120.0f;

    public float orbitYMinLimit = -20f;
    public float orbitYMaxLimit = 80f;

    public float orbitDistanceMin = .5f;
    public float orbitDistanceMax = 15f;

    private Rigidbody _rigidbody;

    float x = 0.0f;
    float y = 0.0f;


    CameraPreset fourPos = new CameraPreset();
    CameraPreset fivePos = new CameraPreset();
    CameraPreset sixPos = new CameraPreset();

    Vector3 wVector = Vector3.forward;
    Vector3 aVector = Vector3.left;
    Vector3 sVector = Vector3.back;
    Vector3 dVector = Vector3.right;

    public float forwardToCameraAngle;

    void SetCameraRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
        _pitch = rotation.eulerAngles.x;
        _yaw = rotation.eulerAngles.y;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

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
        fourPos.position = new Vector3(0, 0, 0);
        fourPos.rotation = Quaternion.Euler(0,0,0);
        fivePos.position = new Vector3(0, 0, 0);
        fivePos.rotation = Quaternion.Euler(0, 0, 0);
        sixPos.position = new Vector3(0, 0, 0);
        sixPos.rotation = Quaternion.Euler(0, 0, 0);

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        UpdateTargetAlpha();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    IEnumerator AutoPlayRoutine()
    {
        while (true)
        {
            float elapsedTime = 0f;
            while (elapsedTime <= xAutoRotateTime)
            {
                x += Time.unscaledDeltaTime * autoplayCamSpeed * orbitDistance * 0.02f;
                UpdateCamPosition();
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime <= yAutoRotateTime)
            {
                y -= Time.unscaledDeltaTime * autoplayCamSpeed * orbitDistance * 0.02f;
                UpdateCamPosition();
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime <= xAutoRotateTime)
            {
                x -= Time.unscaledDeltaTime * autoplayCamSpeed * orbitDistance * 0.02f;
                UpdateCamPosition();
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime <= yAutoRotateTime)
            {
                y += Time.unscaledDeltaTime * autoplayCamSpeed * orbitDistance * 0.02f;
                UpdateCamPosition();
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            yield return null;
        }
        
    }
    void ResetCameraTarget()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, float.MaxValue, LayerMask.GetMask("Floor"), QueryTriggerInteraction.Ignore))
        {
            target.transform.position = hit.point;
            orbitDistance = hit.distance;
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
            UpdateCamPosition();
        }
        else
        {
            target.transform.position = transform.position + (transform.forward * 100);
            orbitDistance = 100;
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
            //UpdateCamPosition();
        }
    }

    public void EnableTargetRenderer(bool enable) { targetRenderer.enabled = enable; }

    public void EnableFloorGridRenderer(bool enable) { floorRenderer.enabled = enable;  } 

    private void HandleMouseInput()
    {
        // Orbit
        if (target)
        {
            if (Input.GetMouseButton(0))
            {
                x += Input.GetAxis("Mouse X") * orbitXSpeed * orbitDistance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * orbitYSpeed * orbitDistance * 0.02f;

                y = ClampAngle(y, orbitYMinLimit, orbitYMaxLimit);
                UpdateCamPosition();
                
            }
            // Rotate
            else if (Input.GetMouseButton(1))
            {
                _yaw += _speedH * Input.GetAxis("Mouse X");
                _pitch -= _speedV * Input.GetAxis("Mouse Y");

                transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);
            }

        }
        
        
    }

    private void UpdateCamPosition()
    {
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        RaycastHit hit;
        //Debug.DrawLine(target.position, transform.position, Color.red);
        if (Physics.Linecast(target.position, transform.position, out hit, ~LayerMask.GetMask("Floor"), QueryTriggerInteraction.Ignore))
        {
            orbitDistance -= hit.distance;
        }
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -orbitDistance);
        Vector3 position = rotation * negDistance + target.position;
        UpdateTargetAlpha();

        SetCameraRotation(rotation);
        transform.position = position;
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
            orbitDistance = Mathf.Clamp(orbitDistance - Input.GetAxis("Mouse ScrollWheel") * 500, orbitDistanceMin, orbitDistanceMax);
            UpdateCamPosition();
        }
    }

    void UpdateTargetAlpha()
    {
        matAlpha = (orbitDistance - orbitDistanceMin) / (orbitDistanceMax - orbitDistanceMin);
        matAlpha = Mathf.Max(Mathf.Sqrt(matAlpha) - 0.5f, 0);
        Color matColor = targetRenderer.material.color;
        matColor.a = matAlpha;
        targetRenderer.material.color = matColor;
    }

    void UpdateDirectionVectors()
    {
        Vector3 cameraToTarget = target.position - transform.position;
        cameraToTarget.y = 0;
        forwardToCameraAngle = Vector3.SignedAngle(Vector3.forward, cameraToTarget, Vector3.down);

        if(forwardToCameraAngle >-45f && forwardToCameraAngle <= 45f)
        {
            wVector = Vector3.forward;
            aVector = Vector3.left;
            sVector = Vector3.back;
            dVector = Vector3.right;
        }
        else if(forwardToCameraAngle > 45f && forwardToCameraAngle <= 135f)
        {
            wVector = Vector3.left;
            aVector = Vector3.back;
            sVector = Vector3.right;
            dVector = Vector3.forward;
        }
        else if(forwardToCameraAngle > 135f || forwardToCameraAngle <= -135f)
        {
            wVector = Vector3.back;
            aVector = Vector3.right;
            sVector = Vector3.forward;
            dVector = Vector3.left;
        }
        else if(forwardToCameraAngle > -135f && forwardToCameraAngle <= -45f)
        {
            wVector = Vector3.right;
            aVector = Vector3.forward;
            sVector = Vector3.left;
            dVector = Vector3.back;
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
            UpdateDirectionVectors();
            //transform.Translate(Vector3.forward * Time.deltaTime * _cameraSpeed);
            target.Translate(wVector * Time.unscaledDeltaTime * _cameraSpeed);
            UpdateCamPosition();
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            UpdateDirectionVectors();
            //transform.Translate(Vector3.left * Time.deltaTime * _cameraSpeed);
            target.Translate(aVector * Time.unscaledDeltaTime * _cameraSpeed);
            UpdateCamPosition();
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            UpdateDirectionVectors();
            //transform.Translate(Vector3.back * Time.deltaTime * _cameraSpeed);
            target.Translate(sVector * Time.unscaledDeltaTime * _cameraSpeed);
            UpdateCamPosition();
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            UpdateDirectionVectors();
            //transform.Translate(Vector3.right * Time.deltaTime * _cameraSpeed);
            target.Translate(dVector * Time.unscaledDeltaTime * _cameraSpeed);
            UpdateCamPosition();
        }
        if (Input.GetKey(KeyCode.Q))
        {
            //transform.Translate(Vector3.up * Time.deltaTime * _cameraSpeed);
            target.Translate(Vector3.up * Time.unscaledDeltaTime * _cameraSpeed);

            UpdateCamPosition();
        }
        if (Input.GetKey(KeyCode.E))
        {
            //transform.Translate(Vector3.down * Time.deltaTime * _cameraSpeed);
            target.Translate(Vector3.down * Time.unscaledDeltaTime * _cameraSpeed);

            UpdateCamPosition();
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
            transform.position = new Vector3(0, 20, -20);
            SetCameraRotation(Quaternion.Euler(24f, -0.5f, 0));
            Camera.main.fieldOfView = 45f;
            ResetCameraTarget();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            transform.position = new Vector3(0, 30, -20);
            SetCameraRotation(Quaternion.Euler(36.6f, -0.5f, 0));
            Camera.main.fieldOfView = 60f;
            ResetCameraTarget();

        }


        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                fourPos.position = transform.position;
                fourPos.rotation = transform.rotation;
            }
            else
            {
                transform.position = fourPos.position;
                SetCameraRotation(fourPos.rotation);
                ResetCameraTarget();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                fivePos.position = transform.position;
                fivePos.rotation = transform.rotation;
            }
            else
            {
                transform.position = fivePos.position;
                SetCameraRotation(fivePos.rotation);
                ResetCameraTarget();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                sixPos.position = transform.position;
                sixPos.rotation = transform.rotation;
            }
            else
            {
                transform.position = sixPos.position;
                SetCameraRotation(sixPos.rotation);
                ResetCameraTarget();
            }
        }


    }

}
