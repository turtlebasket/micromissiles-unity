using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class CameraPanel : MonoBehaviour
{
    /*
    private RenderTexture _renderTexture;
    private Camera _camera;
    private RawImage _rawImage;
    private Vector2Int _lastScreenSize;

    void Start()
    {
        _rawImage = GetComponentInChildren<RawImage>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        // Create initial RenderTexture
        _renderTexture = _rawImage.texture as RenderTexture;
        _renderTexture.useDynamicScale = true;
        _rawImage.texture = _renderTexture;
        if (_camera != null)
        {
            _camera.targetTexture = _renderTexture;
        }

        UpdateRenderTextureSize();
    }

    void Update()
    {
        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
        if (currentScreenSize != _lastScreenSize)
        {
            UpdateRenderTextureSize();
            _lastScreenSize = currentScreenSize;
        }
    }

    void UpdateRenderTextureSize()
    {
        Vector2 panelSize = GetScaledPanelSize();
        
        float widthScale = Mathf.Clamp01(panelSize.x / Screen.width);
        float heightScale = Mathf.Clamp01(panelSize.y / Screen.height);
        
        // Use the smaller scale to maintain aspect ratio
        float scale = Mathf.Min(widthScale, heightScale);
        
        // Update ScalableBufferManager
        ScalableBufferManager.ResizeBuffers(scale, scale);
    }

    Vector2 GetScaledPanelSize()
    {
        RectTransform panelRectTransform = GetComponent<RectTransform>();
        Vector2 panelSize = panelRectTransform.rect.size;
        return new Vector2(
            panelSize.x * panelRectTransform.lossyScale.x,
            panelSize.y * panelRectTransform.lossyScale.y
        );
    }
    */
}