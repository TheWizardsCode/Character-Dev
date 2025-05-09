using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestAgentScript : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        GetComponent<NavMeshAgent>().SetDestination(target.position);
    }
}
