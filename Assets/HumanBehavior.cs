using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.AI;
using System;
using Unity.VisualScripting;

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

    float hungerVal, bathroomVal, showerVal, buyFoodVal;

    [SerializeField]
    float hungerValMax, bathroomValMax, showerValMax;

    List<GameObject> allFood = new List<GameObject>();

    List<GameObject> carriedFood = new List<GameObject>();

    //List<GameObject> collidedObjects = new List<GameObject>();

    float hungerMaxTime = 1, bathroomMaxTime = 1, showerMaxTime = 1, buyFoodMaxTime = 1;


    float hungerStep, bathroomStep, showerStep, buyfoodStep;


    GameObject touchingObj;

    [SerializeField]
    GameObject foodPrefab;

    bool alreadyBoughtFood = false, alreadyAteFood = false, alreadyBathroomed = false;


    public NavMeshAgent agent;

    float eatingTimeStep, eatingTimeMax, bathroomingTimeStep, bathroomingTimeMax, showeringTimeStep, showeringTimeMax;

    [SerializeField]
    float eatingTimeRangeMin = 4f, eatingTimeRangeMax = 6f, bathroomingTimeRangeMin, bathroomingTimeRangeMax, showeringTimeRangeMin, showeringTimeRangeMax;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Sets first random timer maximums
        eatingTimeMax = UnityEngine.Random.Range(eatingTimeRangeMin, eatingTimeRangeMax);
        bathroomingTimeMax = UnityEngine.Random.Range(bathroomingTimeRangeMin, bathroomingTimeRangeMax);
        showeringTimeMax = UnityEngine.Random.Range(showeringTimeRangeMin, showeringTimeRangeMax);

        //Set all values to max
        hungerVal = hungerValMax;
        bathroomVal = bathroomValMax;
        showerVal = showerValMax;

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
            if (touchingObj != null)
            {
                if (touchingObj.name == "Food Store" && !alreadyBoughtFood)
                {
                    alreadyBoughtFood = true;
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject newFood = GameObject.Instantiate(foodPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
                        newFood.transform.parent = gameObject.transform;
                        carriedFood.Add(newFood);
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
        target = toilet;
        agent.SetDestination(target.position);
        if (IsNameColliding("Toilet"))
        {
            bathroomingTimeStep += Time.deltaTime;

            if (bathroomingTimeStep >= bathroomingTimeMax)
            {
                bathroomingTimeStep = 0;
                bathroomingTimeMax = UnityEngine.Random.Range(bathroomingTimeRangeMin, bathroomingTimeRangeMax);
                target = null;
                bathroomVal = bathroomValMax;
                state = HumanStates.desking;
            }
        }
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
    }

    void FindAllFood()
    {
        allFood.Clear();
        allFood.AddRange(GameObject.FindGameObjectsWithTag("food"));
    }
}

