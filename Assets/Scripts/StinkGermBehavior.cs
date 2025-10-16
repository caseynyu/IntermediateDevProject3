using NavMeshPlus.Extensions;
using UnityEngine;
using UnityEngine.AI;

public class StinkGermBehavior : EcosystemBaseBehavior
{
    enum StinkGermStates
    {
        moving,
        poweringUp,
        combining,
    }

    float potency = 1;

    [SerializeField]
    float searchRadius = 1.5f;

    GameObject target = null;

    float lerpTime;
    [SerializeField]
    float lerpTimeMax;
    [SerializeField]
    AnimationCurve idleWalkCurve;

    [SerializeField]
    Vector3 startPos = Vector3.zero;

    NavMeshAgent agent;

    

    //Will find the nearest stink germ to go towards it and combine, then that one will stay in that area
    //

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        Moving();
        //ChooseNewTarget();
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, .5f);
        Gizmos.DrawWireSphere(transform.position, searchRadius);


    }

    void Moving()
    {
        target = ChooseNewTarget();
        if (target == null)
        {
            agent.ResetPath();
            //startPos = transform.position;
            //lerpTime = 0;
        }
        else
        {
            agent.SetDestination(target.transform.position);
            //transform.position = MoveTowardsTarget();
        }

    }

    Vector3 MoveTowardsTarget()
    {
        lerpTime += Time.deltaTime;
        float percent = idleWalkCurve.Evaluate(lerpTime / lerpTimeMax);
        Vector3 newPos = Vector3.LerpUnclamped(startPos, target.transform.position, percent);
        Vector3 dir = (startPos - target.transform.position).normalized;
        transform.up = dir;
        return newPos;
    }

    GameObject ChooseNewTarget()
    {
        GetWideCollisions();
        if (GetObjectWithTag("stinkgerm") != null)
        {
            return GetObjectWithTag("stinkgerm");
        }
        else
        {
            return null;
        }
    }



    void StartWithPotency()
    {

    }
    
    

    void GetWideCollisions()
    {
        collidedObjects.Clear();
        Collider2D[] collidingObjectsCircle = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        foreach (Collider2D i in collidingObjectsCircle)
        {
            collidedObjects.Add(i.gameObject);
        }
    }
    
}
