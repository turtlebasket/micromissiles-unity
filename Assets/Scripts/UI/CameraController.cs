using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Singleton

    /// <summary>
    /// Singleton instance of the CameraController.
    /// </summary>
    public static CameraController Instance { get; private set; }

    #endregion

    #region Camera Settings

    /// <summary>
    /// Determines if mouse input is active for camera control.
    /// </summary>
    public bool mouseActive = true;

    /// <summary>
    /// Locks user input for camera control.
    /// </summary>
    public bool lockUserInput = false;

    /// <summary>
    /// Normal speed of camera movement.
    /// </summary>
    [SerializeField] private float _cameraSpeedNormal = 100.0f;

    /// <summary>
    /// Maximum speed of camera movement.
    /// </summary>
    [SerializeField] private float _cameraSpeedMax = 1000.0f;

    /// <summary>
    /// Current speed of camera movement.
    /// </summary>
    private float _cameraSpeed;

    /// <summary>
    /// Horizontal rotation speed.
    /// </summary>
    public float _speedH = 2.0f;

    /// <summary>
    /// Vertical rotation speed.
    /// </summary>
    public float _speedV = 2.0f;

    /// <summary>
    /// Current yaw angle of the camera.
    /// </summary>
    private float _yaw = 0.0f;

    /// <summary>
    /// Current pitch angle of the camera.
    /// </summary>
    private float _pitch = 0.0f;

    #endregion

    #region Orbit Settings

    /// <summary>
    /// Determines if the camera should auto-rotate.
    /// </summary>
    public bool autoRotate = false;

    /// <summary>
    /// Target transform for orbit rotation.
    /// </summary>
    public Transform target;

    /// <summary>
    /// Distance from the camera to the orbit target.
    /// </summary>
    [SerializeField] private float _orbitDistance = 5.0f;

    /// <summary>
    /// Horizontal orbit rotation speed.
    /// </summary>
    [SerializeField] private float _orbitXSpeed = 120.0f;

    /// <summary>
    /// Vertical orbit rotation speed.
    /// </summary>
    [SerializeField] private float _orbitYSpeed = 120.0f;

    /// <summary>
    /// Speed of camera zoom.
    /// </summary>
    [SerializeField] private float _zoomSpeed = 500.0f;

    /// <summary>
    /// Minimum vertical angle limit for orbit.
    /// </summary>
    public float orbitYMinLimit = -20f;

    /// <summary>
    /// Maximum vertical angle limit for orbit.
    /// </summary>
    public float orbitYMaxLimit = 80f;

    /// <summary>
    /// Minimum distance for orbit.
    /// </summary>
    private float _orbitDistanceMin = 10f;

    /// <summary>
    /// Maximum distance for orbit.
    /// </summary>
    private float _orbitDistanceMax = 10000f;

    /// <summary>
    /// Current horizontal orbit angle.
    /// </summary>
    private float _orbitX = 0.0f;

    /// <summary>
    /// Current vertical orbit angle.
    /// </summary>
    private float _orbitY = 0.0f;

    #endregion

    #region Rendering

    /// <summary>
    /// Renderer for the orbit target.
    /// </summary>
    public Renderer targetRenderer;

    /// <summary>
    /// Renderer for the floor.
    /// </summary>
    public Renderer floorRenderer;

    /// <summary>
    /// Alpha value for material transparency.
    /// </summary>
    public float matAlpha;

    #endregion

    #region Autoplay Settings

    /// <summary>
    /// Speed of camera movement during autoplay.
    /// </summary>
    public float autoplayCamSpeed = 2f;

    /// <summary>
    /// Duration of horizontal auto-rotation.
    /// </summary>
    public float xAutoRotateTime = 5f;

    /// <summary>
    /// Duration of vertical auto-rotation.
    /// </summary>
    public float yAutoRotateTime = 5f;

    /// <summary>
    /// Coroutine for autoplay functionality.
    /// </summary>
    private Coroutine autoplayRoutine;

    #endregion

    #region Camera Presets

    /// <summary>
    /// Represents a preset camera position and rotation.
    /// </summary>
    [System.Serializable]
    public struct CameraPreset
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    /// <summary>
    /// Preset camera position for key 4.
    /// </summary>
    CameraPreset fourPos = new CameraPreset();

    /// <summary>
    /// Preset camera position for key 5.
    /// </summary>
    CameraPreset fivePos = new CameraPreset();

    /// <summary>
    /// Preset camera position for key 6.
    /// </summary>
    CameraPreset sixPos = new CameraPreset();

    #endregion

    #region Movement

    /// <summary>
    /// Mapping of translation inputs to movement vectors.
    /// </summary>
    private Dictionary<TranslationInput, Vector3> _translationInputToVectorMap;

    /// <summary>
    /// Forward movement vector.
    /// </summary>
    Vector3 wVector = Vector3.forward;

    /// <summary>
    /// Left movement vector.
    /// </summary>
    Vector3 aVector = Vector3.left;

    /// <summary>
    /// Backward movement vector.
    /// </summary>
    Vector3 sVector = Vector3.back;

    /// <summary>
    /// Right movement vector.
    /// </summary>
    Vector3 dVector = Vector3.right;

    /// <summary>
    /// Angle between forward vector and camera direction.
    /// </summary>
    public float forwardToCameraAngle;

    #endregion

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

        _translationInputToVectorMap = new Dictionary<TranslationInput, Vector3> {
            {TranslationInput.Forward, wVector},
            {TranslationInput.Left, aVector},
            {TranslationInput.Back, sVector},
            {TranslationInput.Right, dVector},
            {TranslationInput.Up, Vector3.up},
            {TranslationInput.Down, Vector3.down}
        };
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
        _orbitX = angles.y;
        _orbitY = angles.x;

        UpdateTargetAlpha();
    }

    IEnumerator AutoPlayRoutine()
    {
        while (true)
        {
            float elapsedTime = 0f;
            while (elapsedTime <= xAutoRotateTime)
            {
                _orbitX += Time.unscaledDeltaTime * autoplayCamSpeed * _orbitDistance * 0.02f;
                UpdateCamPosition(_orbitX, _orbitY);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime <= yAutoRotateTime)
            {
                _orbitY -= Time.unscaledDeltaTime * autoplayCamSpeed * _orbitDistance * 0.02f;
                UpdateCamPosition(_orbitX, _orbitY);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime <= xAutoRotateTime)
            {
                _orbitX -= Time.unscaledDeltaTime * autoplayCamSpeed * _orbitDistance * 0.02f;
                UpdateCamPosition(_orbitX, _orbitY);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime <= yAutoRotateTime)
            {
                _orbitY += Time.unscaledDeltaTime * autoplayCamSpeed * _orbitDistance * 0.02f;
                UpdateCamPosition(_orbitX, _orbitY);
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
            _orbitDistance = hit.distance;
            Vector3 angles = transform.eulerAngles;
            _orbitX = angles.y;
            _orbitY = angles.x;
            UpdateCamPosition(_orbitX, _orbitY);
        }
        else
        {
            target.transform.position = transform.position + (transform.forward * 100);
            _orbitDistance = 100;
            Vector3 angles = transform.eulerAngles;
            _orbitX = angles.y;
            _orbitY = angles.x;
            //UpdateCamPosition();
        }
    }

    public void EnableTargetRenderer(bool enable) { targetRenderer.enabled = enable; }

    public void EnableFloorGridRenderer(bool enable) { floorRenderer.enabled = enable;  } 


    public void OrbitCamera(float xOrbit, float yOrbit) {
        if (target)
        {
            _orbitX += xOrbit * _orbitXSpeed * _orbitDistance * 0.02f;
            _orbitY -= yOrbit * _orbitYSpeed * _orbitDistance * 0.02f;

            _orbitY = ClampAngle(_orbitY, orbitYMinLimit, orbitYMaxLimit);
            UpdateCamPosition(_orbitX, _orbitY);
        }
    }

    public void RotateCamera(float xRotate, float yRotate) {
        _yaw += xRotate * _speedH;
        _pitch -= yRotate * _speedV;
        transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);
    }

    private void UpdateCamPosition(float x, float y)
    {
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        RaycastHit hit;
        //Debug.DrawLine(target.position, transform.position, Color.red);
        if (Physics.Linecast(target.position, transform.position, out hit, ~LayerMask.GetMask("Floor"), QueryTriggerInteraction.Ignore))
        {
            _orbitDistance -= hit.distance;
        }
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -_orbitDistance);
        Vector3 position = rotation * negDistance + target.position;
        UpdateTargetAlpha();

        SetCameraRotation(rotation);
        transform.position = position;
    }


    public void ZoomCamera(float zoom)
    {
        _orbitDistance = Mathf.Clamp(_orbitDistance - zoom * _zoomSpeed, _orbitDistanceMin, _orbitDistanceMax);
        UpdateCamPosition(_orbitX, _orbitY);
    }

    void UpdateTargetAlpha()
    {
        matAlpha = (_orbitDistance - _orbitDistanceMin) / (_orbitDistanceMax - _orbitDistanceMin);
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

    public enum TranslationInput {
        Forward,
        Left,
        Back,
        Right,
        Up,
        Down
    }



    public void TranslateCamera(TranslationInput input) {
        UpdateDirectionVectors();
        target.Translate(_translationInputToVectorMap[input] * Time.unscaledDeltaTime * _cameraSpeed);
        UpdateCamPosition(_orbitX, _orbitY);
    }

    void HandleLockableInput()
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _cameraSpeed = _cameraSpeedMax;
        }
        else
        {
            _cameraSpeed = _cameraSpeedNormal;
        }
        
        
    }

    void HandleNonLockableInput()
    {
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
