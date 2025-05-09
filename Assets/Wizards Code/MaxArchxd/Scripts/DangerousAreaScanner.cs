using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.BackgroundAI;

[RequireComponent(typeof(NavMeshAgent))]
public class DangerousAreaScanner : AbstractSingleton<DangerousAreaScanner>
{
    [Tooltip("The agent will scan for danger before entering an area of the NavMesh on this list")]
    public List<string> dangerousAreas;
    [Tooltip("The agent will scan for obstacles with these tags. Leave the list empty to scan for all obstacles")]
    public List<string> dangerousObstacles;
    [Tooltip("Amount of time in seconds the agent will wait in place before scanning again")]
    public float waitTime = 3;
    [Tooltip("Amount of time in seconds between scans while the agent is moving")]
    public float scanInterval = 1;
    [Tooltip("Radius in which the agent will scan for danger")]
    public int detectionRadius = 10;

    private NavMeshAgent agent;
    private HashSet<int> dangerousAreasLayers;
    private int currentArea;
    private bool waiting;
    private float timeSinceLastScan;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        dangerousAreasLayers = new HashSet<int>();
        timeSinceLastScan = scanInterval;
    }

    void Start()
    {
        updateDangerousAreas();
        currentArea = getCurrentArea();
    }

    public void updateDangerousAreas()
    {
        foreach (string layerName in dangerousAreas)
        {
            int areaIndex = NavMesh.GetAreaFromName(layerName);
            if(areaIndex != -1) dangerousAreasLayers.Add(1 << areaIndex);
        }
    }

    void Update()
    {
        if (waiting) return;
        timeSinceLastScan += Time.deltaTime;
        tryScan();
    }

    private void tryScan()
    {
        float scanDistance = scanInterval * agent.speed;
        if (scanConditionsMet(scanDistance)) scanAreaAhead(scanDistance);
    }

    private bool scanConditionsMet(float scanDistance)
    {
        return timeSinceLastScan >= scanInterval && scanDistance <= agent.remainingDistance;
    }

    private void scanAreaAhead(float scanDistance)
    {
        NavMeshHit hit;
        agent.SamplePathPosition(NavMesh.AllAreas, scanDistance, out hit);
        if (hit.mask != currentArea) processAreaAhead(hit.mask);
        timeSinceLastScan = 0;
    }

    private void processAreaAhead(int areaMaskAhead)
    {
        if (isAreaDangerous(areaMaskAhead)) processDangerousArea();
        else processSafeArea(areaMaskAhead);
    }

    private bool isAreaDangerous(int areaMaskAhead)
    {
        return dangerousAreasLayers.Contains(areaMaskAhead) && checkForDanger();
    }

    private void processDangerousArea()
    {
        waiting = true;
        agent.isStopped = true;
        StartCoroutine("waitUntilDangerPassed");
    }

    private void processSafeArea(int areaMaskAhead)
    {
        currentArea = areaMaskAhead;
        agent.isStopped = false;
    }

    private IEnumerator waitUntilDangerPassed()
    {
        float timeElapsed = 0;
        while(timeElapsed < waitTime)
        {
            yield return null;
            timeElapsed += Time.deltaTime;
        }
        waiting = false;
    }

    private int getCurrentArea()
    {
        NavMeshHit hit;

        if(!NavMesh.SamplePosition(transform.position, out hit, 2, NavMesh.AllAreas))
        {
            return 1 << -1;
        }
        return hit.mask;
    }

    private bool checkForDanger()
    {
        List<NavMeshObstacle> obstacles = getRelevantObstacles();
        foreach(NavMeshObstacle obstacle in obstacles)
        {
            if(Vector3.Distance(obstacle.transform.position, transform.position) <= detectionRadius) return true;
        }
        return false;
    }

    private List<NavMeshObstacle> getRelevantObstacles()
    {
        if (dangerousObstacles.Count == 0) return new List<NavMeshObstacle>(ObstacleListSingleton.Instance.ObstaclesInScene);
        else
        {
            List<NavMeshObstacle> returner = new List<NavMeshObstacle>();
            foreach (string obstacleTag in dangerousObstacles) returner.AddRange(ObstacleListSingleton.Instance.getObstaclesWithTag(obstacleTag));
            return returner;
        }
    }
}
