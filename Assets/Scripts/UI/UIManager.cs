using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance { get; private set; }

    public GameObject botPanel;
    public TextMeshProUGUI agentPanelText;

    public TMP_FontAsset Font;


    private UIMode curMode = UIMode.NONE;

    
    // Start is called before the first frame update
    void Awake()
    {
        // singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);    
    }

    void Start()
    {
        //inputManager = InputManager.Instance;
        //worldManager = WorldManager.Instance;
    }

    public void SetUIMode(UIMode mode){
        curMode = mode;
    }

    public UIMode GetUIMode(){
        return curMode;
    }

    public void SetAgentPanelText(string text)
    {
        agentPanelText.text = text;
    }

    public string GetAgentPanelText()
    {
        return agentPanelText.text;
    }


    private void UpdateAgentPanel()
    {
        string agentPanelText = "";
        foreach(Agent agent in SimManager.Instance.GetActiveAgents())
        {
            string jobText = agent.name + "| Phase: " + agent.GetFlightPhase().ToString();
            agentPanelText += jobText + "\n";
        }
        SetAgentPanelText(agentPanelText);
    }


    // Update is called once per frame
    void Update()
    {
        UpdateAgentPanel();

    }
}


public enum UIMode {
    NONE,
    BUILD,
    MINE
}