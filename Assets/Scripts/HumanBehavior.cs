using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.AI;
using System;
using Unity.Mathematics;

public class HumanBehavior : EcosystemBaseBehavior
{



    [SerializeField]
    Transform desk, foodStore, bed, table, sink, toilet, shower;


    Transform[] possibleTargets;
    Transform target = null;

    float lerpTime;

    [SerializeField]
    float lerpTimeMax;

    enum HumanStates
    {
        eating,
        sleeping,
        showering,
        bathrooming,
        desking,
        sinking,
        idling,
        buyingfood,
    }
    HumanStates state = HumanStates.desking;

    [SerializeField]
    AnimationCurve idleWalkCurve;


     //At This point I don't even know what the difference is between something's Val, ValMax, TimeMax, and MaxTime, oh well. I came back to add some more timers to this and made them completely differently from how I was doing the first few so it's inconsistent

    float hungerVal, bathroomVal, showerVal, buyFoodVal;

    [SerializeField]
    float hungerValMax, bathroomValMax, showerValMax, sleepValMax;

    List<GameObject> allFood = new List<GameObject>();

    List<GameObject> carriedFood = new List<GameObject>();

    //List<GameObject> collidedObjects = new List<GameObject>();

    float hungerMaxTime = 1, bathroomMaxTime = 1, showerMaxTime = 1, buyFoodMaxTime = 1, sleepMaxTime = 1;


    float hungerStep, bathroomStep, showerStep, buyfoodStep, sleepStep;


    [SerializeField]
    GameObject foodPrefab, handGermPrefab, stinkGermPrefab;

    bool alreadyBoughtFood = false, alreadyAteFood = false, alreadyBathroomed = false;


    public NavMeshAgent agent;

    float eatingTimeStep, eatingTimeMax, bathroomingTimeStep, bathroomingTimeMax, showeringTimeStep, showeringTimeMax, stinkTimeStep, sleepTimeStep, sinkTimeStep;

    [SerializeField]
    float stinkTimeMax, sleepTimeMax, sinkTimeMax = 3;

    [SerializeField]
    float eatingTimeRangeMin = 4f, eatingTimeRangeMax = 6f, bathroomingTimeRangeMin, bathroomingTimeRangeMax, showeringTimeRangeMin, showeringTimeRangeMax, sleepTimeRangeMin, sleepTimeRangeMax;


    float poisonedAmount = 0;


    void Start()
    {
        //Sets first random timer maximums
        eatingTimeMax = UnityEngine.Random.Range(eatingTimeRangeMin, eatingTimeRangeMax);
        bathroomingTimeMax = UnityEngine.Random.Range(bathroomingTimeRangeMin, bathroomingTimeRangeMax);
        showeringTimeMax = UnityEngine.Random.Range(showeringTimeRangeMin, showeringTimeRangeMax);
        sleepTimeMax = UnityEngine.Random.Range(sleepTimeRangeMin, sleepTimeRangeMax);

        //Set all values to max
        hungerVal = 5;
        bathroomVal = 13;
        showerVal = showerValMax;
        stinkTimeStep = stinkTimeMax;
        sleepTimeStep = sleepTimeMax;
        sinkTimeStep = sinkTimeMax;
        

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        FindAllFood();

        

    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case HumanStates.buyingfood:
                BuyingFood();
                break;
            case HumanStates.eating:
                Eating();
                break;
            case HumanStates.desking:
                Desking();
                break;
            case HumanStates.bathrooming:
                Bathrooming();
                break;
            case HumanStates.showering:
                Showering();
                break;
            case HumanStates.sinking:
                Sinking();
                break;
            case HumanStates.sleeping:
                Sleeping();
                break;
        }
        FindAllFood();
        StepNeeds();
        GetCollisions();
    }

    void BuyingFood()
    {
        if (target == null)
        {
            target = foodStore;
            lerpTime = 0;
        }
        else
        {
            agent.SetDestination(target.position);
            //transform.position = MoveTowardsTarget();
                if (IsNameColliding("Food Store") && !alreadyBoughtFood)
                {
                    alreadyBoughtFood = true;
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject newFood = GameObject.Instantiate(foodPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
                        newFood.transform.parent = gameObject.transform;
                        carriedFood.Add(newFood);
                    }
                    for (int i = 0; i<3; i++)
                    {
                        GameObject newHandGerm = GameObject.Instantiate(handGermPrefab, transform.position, transform.rotation);
                        newHandGerm.transform.parent = gameObject.transform;
                    }
                    target = table;
                }
                if (IsNameColliding("Table") && allFood.Count != 0)
                {
                    alreadyBoughtFood = false;
                    //Debug.Log(carriedFood.Count);
                    int count = carriedFood.Count;
                    for (int i = 0; i < count; i++)
                    {
                        //Debug.Log(i);
                        carriedFood[0].transform.parent = table.transform;
                        carriedFood[0].transform.position = new Vector3(table.transform.position.x, table.transform.position.y - (.06f * i), table.transform.position.z);
                        carriedFood.RemoveAt(0);
                    }
                    carriedFood.Clear();
                    target = null;
                    state = HumanStates.desking;
                    FindAllFood();
                }
        }
    }

    void Eating()
    {

        if (allFood.Count == 0 && alreadyAteFood == false)
        {
            state = HumanStates.buyingfood;
            //Debug.Log("test");
        }
        else
        {
            target = table;
            agent.SetDestination(target.position);
            
            if (IsNameColliding("Table") && alreadyAteFood == false)
            {
                GameObject chosenFood = FindNearest(allFood).gameObject;
                if (CheckSpecificColliding(chosenFood))
                {
                    allFood.Remove(chosenFood);
                    Destroy(chosenFood);
                    //touchingObj = null;
                    //target = null;
                    alreadyAteFood = true;
                    eatingTimeStep = 0;
                    //state = HumanStates.desking;
                    FindAllFood();
                }
            }
            if (alreadyAteFood == true)
            {
                eatingTimeStep += Time.deltaTime;

                if (eatingTimeStep >= eatingTimeMax)
                {
                    eatingTimeStep = 0;
                    hungerVal = hungerValMax;
                    eatingTimeMax = UnityEngine.Random.Range(4f, 6f);
                    alreadyAteFood = false;
                    target = null;
                    state = HumanStates.desking;
                }

            }

        }
    }

    void Bathrooming()
    {
        GetCollisions();
        //Walks to the bathroom and stays there, creates a stink germ and goes back
        target = toilet;
        agent.SetDestination(target.position);
        if (IsNameColliding("Toilet"))
        {
            bathroomingTimeStep += Time.deltaTime;
            
            if (bathroomingTimeStep >= bathroomingTimeMax)
            {
                Instantiate(stinkGermPrefab, transform.position, quaternion.identity);
                bathroomingTimeStep = 0;
                bathroomingTimeMax = UnityEngine.Random.Range(bathroomingTimeRangeMin, bathroomingTimeRangeMax);

                target = null;
                bathroomVal = bathroomValMax;
                // Flip a coin to see if the person washes their hands or not
                if (UnityEngine.Random.Range(0, 101) >= 50)
                {
                    state = HumanStates.sinking;
                }
                else
                {
                    state = HumanStates.desking;
                }
                
            }
        }
    }

    void Sinking()
    {
        target = sink;
        agent.SetDestination(target.position);
        if (IsNameColliding("Sink"))
        {
            sinkTimeStep -= Time.deltaTime;
            if(sinkTimeStep <= 0)
            {
                sinkTimeStep = sinkTimeMax;
                foreach(GameObject i in collidedObjects)
                {
                    if(i.tag == "handgerm")
                    {
                        Destroy(i.gameObject);
                    }
                }
                state = HumanStates.desking;
            }
        }
    }

    void Sleeping()
    {
        target = bed;
        agent.SetDestination(target.position);
        if (IsNameColliding("Bed"))
        {
            sleepTimeStep -= (Time.deltaTime / 5);
            if(sleepTimeStep <= 0)
            {
                sleepTimeStep = sleepTimeMax;
                poisonedAmount = 0;
                state = HumanStates.desking;
            }
        }
    }
    
    public void VirusInfect(float virusPotency)
    {
        poisonedAmount += virusPotency;
        sleepTimeMax = poisonedAmount;
        sleepTimeStep = sleepTimeMax;
    }

    void Showering()
    {
        target = shower;
        agent.SetDestination(target.position);
        if (IsNameColliding("Shower"))
        {
            showeringTimeStep += Time.deltaTime;

            if (showeringTimeStep >= showeringTimeMax)
            {

                showeringTimeStep = 0;
                GetWideCollisions();
                foreach (GameObject i in collidedObjects)
                {
                    if (i.tag == "handgerm" || i.tag == "stinkgerm")
                    {
                        Destroy(i.gameObject);
                    }
                }
                GetCollisions();
                showeringTimeMax = UnityEngine.Random.Range(showeringTimeRangeMin, showeringTimeRangeMax);
                target = null;
                showerVal = showerValMax;
                state = HumanStates.desking;
                
            }
        }
    }
    void Desking()
    {
        if (transform.position != desk.position)
        {

            target = desk;
            //transform.position = MoveTowardsTarget();
            agent.SetDestination(target.position);
        }
        else
        {

        }
        NewNeedsAction();
    }

    Vector3 MoveTowardsTarget()
    {
        lerpTime += Time.deltaTime;
        float percent = idleWalkCurve.Evaluate(lerpTime / lerpTimeMax);
        Vector3 newPos = Vector3.Lerp(transform.position, target.position, percent);
        return newPos;
    }

    void NewNeedsAction()
    {
        //Hunger first, then toilet, then shower, then buy food

        if(poisonedAmount > 0)
        {
            state = HumanStates.sleeping;
        }

        if (hungerVal <= 0)
        {
            target = null;
            state = HumanStates.eating;
            FindAllFood();
        }
        if (bathroomVal <= 0)
        {
            target = null;
            state = HumanStates.bathrooming;
        }
        if (showerVal <= 0)
        {
            target = null;
            state = HumanStates.showering;
        }
    }
    void StepNeeds()
    {
        if(state == HumanStates.desking || state == HumanStates.eating || state == HumanStates.sinking)
        {
            stinkTimeStep -= Time.deltaTime;
        }

        if (state != HumanStates.eating)
        {
            hungerStep -= Time.deltaTime;
        }
        if (state != HumanStates.bathrooming)
        {
            bathroomStep -= Time.deltaTime;
        }
        if (state != HumanStates.showering)
        {
            showerStep -= Time.deltaTime;
        }
        buyfoodStep -= Time.deltaTime;

        if (hungerStep <= 0)
        {
            hungerVal--;
            hungerStep = hungerMaxTime;
        }
        if (bathroomStep <= 0)
        {
            bathroomVal--;
            bathroomStep = bathroomMaxTime;
        }
        if (showerStep <= 0)
        {
            showerVal--;
            showerStep = showerMaxTime;
        }
        if (stinkTimeStep <= 0)
        {
            Instantiate(stinkGermPrefab, transform.position, quaternion.identity);
            stinkTimeStep = stinkTimeMax;
        }
    }

    void FindAllFood()
    {
        allFood.Clear();
        allFood.AddRange(GameObject.FindGameObjectsWithTag("food"));
    }
}

