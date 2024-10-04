using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance { get; private set; }


    [SerializeField]
    private GameObject _agentStatusPanel;
    [SerializeField]
    private GameObject _configSelectorPanel;
    private TMP_Dropdown _configDropdown;
    public TextMeshProUGUI agentPanelText;
    public TextMeshProUGUI simTimeText;

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
        _configSelectorPanel.SetActive(false);
        SetupConfigSelectorPanel();
        //inputManager = InputManager.Instance;
        //worldManager = WorldManager.Instance;
    }

    public void ToggleConfigSelectorPanel(){
        _configSelectorPanel.SetActive(!_configSelectorPanel.activeSelf);
    }

    private void SetupConfigSelectorPanel(){
        _configSelectorPanel.GetComponentInChildren<Button>().onClick.AddListener(delegate {
            LoadSelectedConfig();
        });
        _configDropdown = _configSelectorPanel.GetComponentInChildren<TMP_Dropdown>();
        PopulateConfigDropdown();
    }

    private void PopulateConfigDropdown(){
        _configDropdown.ClearOptions();
        string configPath = Path.Combine(Application.streamingAssetsPath, "Configs");
        string[] configFiles = Directory.GetFiles(configPath, "*.json");
        
        List<string> configFileNames = new List<string>();
        foreach (string configFile in configFiles)
        {
            configFileNames.Add(Path.GetFileName(configFile));
        }
        _configDropdown.AddOptions(configFileNames);
    }
    private void LoadSelectedConfig(){
        string selectedConfig = _configDropdown.options[_configDropdown.value].text;
        SimManager.Instance.LoadNewConfig(selectedConfig);
        _configSelectorPanel.SetActive(false);
        //if(!InputManager.Instance.mouseActive){
        //    InputManager.Instance.mouseActive = true;
        //}
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

    private void UpdateSimTimeText()
    {
        simTimeText.text = "Elapsed Sim Time: " + SimManager.Instance.GetElapsedSimulationTime().ToString("F2");
    }
    // Update is called once per frame
    void Update()
    {
        //UpdateAgentPanel();
        UpdateSimTimeText();
    }
}


public enum UIMode {
    NONE,
    BUILD,
    MINE
}