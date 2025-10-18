using UnityEngine;
using UnityEngine.AI;

public class VirusBehavior : EcosystemBaseBehavior
{

    public float potency;

    NavMeshAgent agent;

    Transform target = null;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        GetWideCollisions();
        if (IsTagColliding("person"))
        {
            target = GetObjectWithTag("person").transform;
            agent.SetDestination(target.position);
        }
        else
        {
            target = null;
            agent.ResetPath();
        }
        if(target != null)
        {
            GetCollisions();
            if (IsTagColliding("person"))
            {
                GetObjectWithTag("person").GetComponent<HumanBehavior>().VirusInfect(potency);
                Destroy(this.gameObject);
                
            }
        }
    }
}
