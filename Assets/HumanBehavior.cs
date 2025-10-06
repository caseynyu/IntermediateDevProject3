using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class HumanBehavior : MonoBehaviour
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
        toileting,
        desking,
        sinking,
        idling,
        buyingfood,
    }
    HumanStates state = HumanStates.desking;

    [SerializeField]
    AnimationCurve idleWalkCurve;

    float hungerVal = 3, toiletVal = 5, showerVal = 30, buyFoodVal = 40;

    List<GameObject> allFood = new List<GameObject>();

    List<GameObject> carriedFood = new List<GameObject>();

    [SerializeField]
    float hungerMaxTime, toiletMaxTime, showerMaxTime, buyFoodMaxTime;

    
    float hungerStep, toiletStep, showerStep, buyfoodStep;


    GameObject touchingObj;

    [SerializeField]
    GameObject foodPrefab;

    bool already = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        }
        StepNeeds();
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
            transform.position = MoveTowardsTarget();
            if (touchingObj != null)
            {
                if (touchingObj.tag == "foodstore" && !already)
                {
                    already = true;
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject newFood = GameObject.Instantiate(foodPrefab, new Vector3(transform.position.x, transform.position.y + (i * 5), transform.position.z), transform.rotation);
                        newFood.transform.parent = gameObject.transform;
                        carriedFood.Add(newFood);
                    }
                    target = table;
                }
                if (touchingObj.tag == "table")
                {
                    already = false;
                    for (int i = 0; i < carriedFood.Count; i++)
                    {
                        carriedFood[0].transform.parent = table.transform;
                        carriedFood.Clear();
                    }
                    target = null;
                    state = HumanStates.desking;
                }
            }
        }
    }

    void Eating()
    {

        if (allFood.Count == 0)
        {
            state = HumanStates.buyingfood;
        }
        else
        {
            if (target == null)
            {
                target = FindNearest(allFood);
                lerpTime = 0;
            }
            else
            {
                transform.position = MoveTowardsTarget();
                if (touchingObj != null)
                {
                    if (touchingObj.tag == "food")
                    {
                        allFood.Remove(touchingObj);
                        hungerVal = 5;
                        Destroy(touchingObj);
                        touchingObj = null;
                        target = null;
                        state = HumanStates.desking;
                    }
                }
            }
            
        }
    }

    void Desking()
    {
        if (transform.position != desk.position)
        {
            target = desk;
            transform.position = MoveTowardsTarget();
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

    void NewNeedsAction() {
        //Hunger first, then toilet, then shower, then buy food
        if (hungerVal <= 0)
        {
            target = null;
            state = HumanStates.eating;
            FindAllFood();
        }
    }
    Transform FindNearest(List<GameObject> objsToFind)
    {
        float minDist = Mathf.Infinity;
        Transform nearest = null;
        for (int i = 0; i < objsToFind.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, objsToFind[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = objsToFind[i].transform;
            }
        }
        return nearest;
    }
    void StepNeeds()
    {
        hungerStep -= Time.deltaTime;
        toiletStep -= Time.deltaTime;
        showerStep -= Time.deltaTime;
        buyfoodStep -= Time.deltaTime;

        if (hungerStep <= 0)
        {
            hungerVal--;
            hungerStep = hungerMaxTime;
        }
    }

    void FindAllFood()
    {
        allFood.AddRange(GameObject.FindGameObjectsWithTag("food"));
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null)
        {
            touchingObj = col.gameObject;
        }

    }
    void OnTriggerExit2D(Collider2D col)
    {
        if (col != null)
        {
            if (col.gameObject == touchingObj) touchingObj = null;
        }
    }
}
