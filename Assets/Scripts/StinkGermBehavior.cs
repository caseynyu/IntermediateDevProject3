using NavMeshPlus.Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class StinkGermBehavior : EcosystemBaseBehavior
{
    enum StinkGermStates
    {
        moving,
        poweringUp,
        combining,
        idling,
        startCooldowning,
    }

    StinkGermStates state = StinkGermStates.startCooldowning;

    float potency = 1;


    GameObject target = null;

    float lerpTime;
    [SerializeField]
    float lerpTimeMax;
    [SerializeField]
    AnimationCurve idleWalkCurve;

    [SerializeField]
    Vector3 startPos = Vector3.zero;

    NavMeshAgent agent;

    float powerUpTimeMax, powerUpTimeStep, powerUpAnimTimeMax, powerUpAnimTimeStep, lifetimeTimeMax, lifetimeTimeStep, startCooldowningTimeMax, startCooldowningTimeStep;
    [SerializeField]
    float powerUpTimeRangeMin, powerUpTimeRangeMax, lifetimeTimeRangeMin, lifetimeTimeRangeMax,startCooldowningTimeRangeMin,startCooldowningTimeRangeMax;



    [SerializeField]
    GameObject stinkGermPrefab, virusPrefab;

    

    //Will find the nearest stink germ to go towards it and combine, then that one will stay in that area
    //

    void Start()
    {
        sprite = gameObject.GetComponentInChildren<SpriteRenderer>().gameObject.transform;
        start = sprite.transform.localPosition;
        baseStartPoint = sprite.transform.localPosition;
        progress = 0f;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        powerUpTimeMax = UnityEngine.Random.Range(powerUpTimeRangeMin, powerUpTimeRangeMax);
        powerUpTimeStep = powerUpTimeMax;
        lifetimeTimeMax = UnityEngine.Random.Range(lifetimeTimeRangeMin, lifetimeTimeRangeMax);
        lifetimeTimeStep = lifetimeTimeMax;
        startCooldowningTimeMax = UnityEngine.Random.Range(startCooldowningTimeRangeMin, startCooldowningTimeRangeMax);
        startCooldowningTimeStep = startCooldowningTimeMax;
    }

    void Update()
    {
        switch (state)
        {
            case StinkGermStates.startCooldowning:
                StartCooldowning();
                break;
            case StinkGermStates.poweringUp:
                PoweringUp();
                break;
            case StinkGermStates.moving:
                Moving();
                break;
            case StinkGermStates.combining:
                Combining();
                break;
            case StinkGermStates.idling:
                break;

        }
        if(state != StinkGermStates.startCooldowning && state != StinkGermStates.combining)
        {
            StepNeeds();
        }
    }
    
    void StartCooldowning()
    {
        //Happens when the stink germ is created, to stop it from immediately going towards a hand germ for a bit
        startCooldowningTimeStep -= Time.deltaTime;
        sprite.gameObject.transform.localScale = new Vector3(((startCooldowningTimeMax - startCooldowningTimeStep)/startCooldowningTimeMax),((startCooldowningTimeMax - startCooldowningTimeStep)/startCooldowningTimeMax),sprite.gameObject.transform.localScale.z);
        if(startCooldowningTimeStep <= 0)
        {
            state = StinkGermStates.idling;
            sprite.gameObject.transform.localScale = Vector3.one;
        }
    }



    void PoweringUp()
    {
        sprite.transform.localPosition = Vector3.zero;
        powerUpTimeMax = UnityEngine.Random.Range(powerUpTimeRangeMin, powerUpTimeRangeMax);
        powerUpTimeStep = powerUpTimeMax;
        Instantiate(stinkGermPrefab,transform.position,quaternion.identity);
        state = StinkGermStates.idling;
    }
    
    
    void StepNeeds()
    {
        randomMoveSpeed = (powerUpTimeMax - powerUpTimeStep) / 3;
        sprite.gameObject.transform.localScale = new Vector3(1.5f - ((lifetimeTimeMax*1.5f - lifetimeTimeStep)/(lifetimeTimeMax*1.5f)), 1.5f - ((lifetimeTimeMax*1.5f - lifetimeTimeStep)/(lifetimeTimeMax*1.5f)), sprite.gameObject.transform.localScale.z);
        RandomMoveAnimation();
        target = ChooseNewTarget();
        if (target != null)
        {
            state = StinkGermStates.moving;
        }
        if(state == StinkGermStates.idling)
        {
            powerUpTimeStep -= Time.deltaTime;
            if (powerUpTimeStep <= 0)
            {
                state = StinkGermStates.poweringUp;
            }
            if (lifetimeTimeStep <= 0)
            {
                Destroy(gameObject);
            }
        }
        lifetimeTimeStep -= Time.deltaTime;
    }

    void Moving()
    {
        if (target == null)
        {
            agent.ResetPath();
        }
        else
        {
            agent.SetDestination(target.transform.position);
        }
        GetCollisions();
        if (IsTagColliding("handgerm") == true)
        {
            state = StinkGermStates.combining;
        }
        GetWideCollisions();
    }
    
    void Combining()
    {
        GetCollisions();
        GameObject other = GetObjectWithTag("handgerm");
        float newPotency = (lifetimeTimeStep /12);
        Debug.Log(newPotency);
        newPotency += other.GetComponent<HandGermBehavior>().potency;
        GameObject newVirus = Instantiate(virusPrefab, transform.position, quaternion.identity);
        newVirus.GetComponent<VirusBehavior>().potency = newPotency;
        Destroy(other);
        Destroy(gameObject);
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
        if(GetObjectWithTag("handgerm") != null)
        {
            if (GetObjectWithTag("handgerm").transform.parent != null && GetObjectWithTag("handgerm").transform.parent.name != "Hand Spot")
            {
                return GetObjectWithTag("handgerm");
            }
            if(GetObjectWithTag("handgerm").transform.parent == null){
                return GetObjectWithTag("handgerm");
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, .5f);
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }


    
}
