using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Linq;

public class SimMonitor : MonoBehaviour
{
    private const float UpdateRate = 0.01f; // 100 Hz
    private StreamWriter writer;
    private Coroutine monitorRoutine;

    private void Start()
    {
        SimManager.Instance.OnSimulationEnded += RegisterSimulationEnded;
        InitializeFile();
        monitorRoutine = StartCoroutine(MonitorRoutine());
    }

    private void InitializeFile()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"sim_telemetry_{timestamp}.csv";
        string directory = Application.persistentDataPath + "/Telemetry/Logs/";
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, fileName);
        writer = new StreamWriter(path, false);
        writer.WriteLine("Time,AgentID,AgentX,AgentY,AgentZ,AgentVX,AgentVY,AgentVZ,AgentState,AgentType");
        Debug.Log($"Monitoring simulation data to {path}");
    }

    private IEnumerator MonitorRoutine()
    {
        while (true)
        {
            ExportTelemetry();
            yield return new WaitForSeconds(UpdateRate);
        }
    }

    private void ExportTelemetry()
    {
        float time = (float)SimManager.Instance.GetElapsedSimulationTime();
        foreach (var agent in SimManager.Instance.GetActiveAgents())
        {
            Vector3 pos = agent.transform.position;
            if(pos == Vector3.zero) {
                continue;
            }
            Vector3 vel = agent.GetComponent<Rigidbody>().linearVelocity;
            string type = agent is Threat ? "T" : "M";
            writer.WriteLine($"{time:F2},{agent.name},{pos.x:F2},{pos.y:F2},{pos.z:F2},{vel.x:F2},{vel.y:F2},{vel.z:F2},{(int)agent.GetFlightPhase()},{type}");
        }

        writer.Flush();
    }

    private void RegisterSimulationEnded()
    {
        writer.Close();
        StopCoroutine(monitorRoutine);
    }


    private void OnDestroy()
    {
        if (writer != null)
        {
            writer.Close();
        }
    }
}