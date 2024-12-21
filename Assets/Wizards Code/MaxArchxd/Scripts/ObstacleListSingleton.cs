using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObstacleListSingleton : WizardsCode.BackgroundAI.AbstractSingleton<ObstacleListSingleton>
{
    private List<NavMeshObstacle> m_obstaclesInScene;
    private Dictionary<string, NavMeshObstacle[]> obstaclesByTag;

    public NavMeshObstacle[] ObstaclesInScene
    {
        get
        {
            return m_obstaclesInScene.ToArray();
        }
    }

    private void Awake()
    {
        obstaclesByTag = new Dictionary<string, NavMeshObstacle[]>();
    }

    void Start()
    {
        compileObstacleList();
    }

    public NavMeshObstacle[] getObstaclesWithTag(string tag)
    {
        try
        {
            return obstaclesByTag[tag];
        } catch
        {
            return new NavMeshObstacle[] { };
        }
    }

    public void compileObstacleList(string tag = "")
    {
        if (tag == "") compileListOfAllObstacles();
        else compileListOfObstaclesWithTag(tag);
    }

    private void compileListOfAllObstacles()
    {
        m_obstaclesInScene = new List<NavMeshObstacle>(FindObjectsOfType<NavMeshObstacle>());
    }

    private void compileListOfObstaclesWithTag(string tag)
    {
        List<NavMeshObstacle> candidates = new List<NavMeshObstacle>();
        NavMeshObstacle[] obstacles = FindObjectsOfType<NavMeshObstacle>();
        foreach (NavMeshObstacle obs in obstacles) if (obs.gameObject.tag == tag) candidates.Add(obs);
        obstaclesByTag[tag] = candidates.ToArray();
    }
}
