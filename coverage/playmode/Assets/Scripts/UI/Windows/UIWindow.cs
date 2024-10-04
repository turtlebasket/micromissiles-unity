﻿using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIElementMouseCapturer))]
[RequireComponent(typeof(UIElementDragger))]
[RequireComponent(typeof(Image))]
public class UIWindow : MonoBehaviour
{
    // Window title
    [SerializeField]
    private string windowTitle = "Window";

    // Close button
    private GameObject closeButton;
    [SerializeField]
    private CloseButtonCallback closeButtonCallback;
    [Serializable]
    private enum CloseButtonCallback
    {
        CLOSE_WINDOW,
        TOGGLE_WINDOW
    }

    // IsOpen property
    private bool isOpen;
    private void OnEnable() { isOpen = true; }
    private void OnDisable() { isOpen = false; }

    public void ToggleWindow()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void CloseWindow()
    {
        Destroy(gameObject);
        isOpen = false;
    }

    /// <summary>
    /// Called when the UIWindow component is created in the editor
    /// We will use it to configure the image component
    /// </summary>
    private void Reset()
    {
        // 18 16 28 125
        GetComponent<Image>().color = new Color32(18, 16, 28, 125);
    }

    public virtual void Start()
    {
        isOpen = gameObject.activeSelf;
        CreateCloseButton();
        CreateWindowTitle();
        if (closeButtonCallback == CloseButtonCallback.CLOSE_WINDOW)
            closeButton.AddComponent<Button>().onClick.AddListener(CloseWindow);
        else if (closeButtonCallback == CloseButtonCallback.TOGGLE_WINDOW)
            closeButton.AddComponent<Button>().onClick.AddListener(ToggleWindow);
    }

    private void CreateWindowTitle()
    {
        GameObject windowTitleObject = new GameObject("WindowTitle", typeof(RectTransform));
        windowTitleObject.transform.SetParent(transform);
        TextMeshProUGUI windowTitleHandle = windowTitleObject.AddComponent<TextMeshProUGUI>();
        windowTitleHandle.text = windowTitle;
        windowTitleHandle.font = UIManager.Instance.Font;
        windowTitleHandle.fontSize = 14;
        windowTitleHandle.color = Color.white;
        windowTitleHandle.alignment = TextAlignmentOptions.Left;
        windowTitleHandle.rectTransform.anchorMin = new Vector2(0, 1);
        windowTitleHandle.rectTransform.anchorMax = new Vector2(1, 1);
        windowTitleHandle.rectTransform.pivot = new Vector2(0, 1);
        windowTitleHandle.rectTransform.sizeDelta = new Vector2(0, 30);
        windowTitleHandle.rectTransform.anchoredPosition = new Vector2(5, 0);
        windowTitleHandle.rectTransform.SetRight(30); // Give spacing to the close button
    }


    /// <summary>
    /// Create the close [x] button in the top right corner of the window
    /// </summary>
    private void CreateCloseButton()
    {
        closeButton = new GameObject("CloseButton", typeof(RectTransform));
        RectTransform buttonTransform = closeButton.GetComponent<RectTransform>();
        buttonTransform.SetParent(transform);
        // anchor top right 
        buttonTransform.anchorMin = new Vector2(1, 1);
        buttonTransform.anchorMax = new Vector2(1, 1);
        buttonTransform.pivot = new Vector2(1, 1);
        // position top right
        buttonTransform.anchoredPosition = new Vector2(0, 0);
        // size
        buttonTransform.sizeDelta = new Vector2(30, 30);
        // add button component
        TextMeshProUGUI textbox = closeButton.AddComponent<TextMeshProUGUI>();
        textbox.text = "X";
        textbox.font = UIManager.Instance.Font;
        textbox.fontSize = 12;
        textbox.alignment = TextAlignmentOptions.Center;
        textbox.verticalAlignment = VerticalAlignmentOptions.Middle;
    }
}