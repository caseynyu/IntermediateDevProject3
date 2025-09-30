using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
public class SpiderBehavior : MonoBehaviour
{

    [SerializeField]
    Transform[] possibleTargets;
    Transform target = null;

    float lerpTime;

    [SerializeField]
    float lerpTimeMax;

    enum SpiderStates
    {
        eating,
        showering,
        dying,
        idling
    }
    SpiderStates state = SpiderStates.idling;

    [SerializeField]
    AnimationCurve idleWalkCurve;

    float hungerVal = 5;

    List<GameObject> allFood = new List<GameObject>();

    [SerializeField]
    float hungerStep;
    float hungerTime;

    GameObject touchingObj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindAllFood();
        hungerTime = hungerStep;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case SpiderStates.idling:
                RunIdle();
                break;
            case SpiderStates.eating:
                RunEat();
                break;
            case SpiderStates.showering:
                break;
            case SpiderStates.dying:
                break;
            default:
                break;
        }

    }


    void RunIdle()
    {
        if (target == null)
        {
            int newTarget = Random.Range(0, possibleTargets.Length);
            target = possibleTargets[newTarget];
            lerpTime = 0;
        }
        else
        {
            transform.position = Move();
        }
        StepNeeds();
        if (hungerVal <= 0)
        {
            target = null;
            state = SpiderStates.eating;
        }

    }
    void RunEat()
    {
        //write everything it needs to eat here
        if (target == null)
        {
            target = FindNearest(allFood);
            lerpTime = 0;
        }
        else
        {
            transform.position = Move();
            if (touchingObj != null)
            {
                if (touchingObj.tag == "food")
                {
                    allFood.Remove(touchingObj);
                    hungerVal = 5;
                    Destroy(touchingObj);
                    touchingObj = null;
                    target = null;
                    state = SpiderStates.idling;
                }
            }
        }
    }

    void StepNeeds()
    {
        hungerTime -= Time.deltaTime;
        if (hungerTime <= 0)
        {
            hungerVal--;
            hungerTime = hungerStep;
        }
    }

    void FindAllFood()
    {
        allFood.AddRange(GameObject.FindGameObjectsWithTag("food"));
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

    Vector3 Move()
    {
        lerpTime += Time.deltaTime;
        float percent = idleWalkCurve.Evaluate(lerpTime / lerpTimeMax);
        Vector3 newPos = Vector3.Lerp(transform.position, target.position, percent);
        return newPos;
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
