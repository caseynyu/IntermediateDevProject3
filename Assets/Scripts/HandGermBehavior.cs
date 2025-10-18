using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.AI;

public class HandGermBehavior : EcosystemBaseBehavior
{

    enum HandGermStates
    {
        moving,
        poweringUp,
        idling,
    }

    HandGermStates state = HandGermStates.idling;

    public float potency = 1;
    float moveTimeMax, moveTimeStep, powerUpTimeMax, powerUpTimeStep;
    

    [SerializeField]
    float moveTimeRangeMin, moveTimeRangeMax, powerUpTimeRangeMin, powerUpTimeRangeMax;


    void Start()
    {
        sprite = gameObject.GetComponentInChildren<SpriteRenderer>().gameObject.transform;
        start = sprite.transform.localPosition;
        baseStartPoint = sprite.transform.localPosition;
        progress = 0f;


        moveTimeMax = UnityEngine.Random.Range(moveTimeRangeMin, moveTimeRangeMax);
        moveTimeStep = moveTimeMax;
        powerUpTimeMax = UnityEngine.Random.Range(powerUpTimeRangeMin, powerUpTimeRangeMax);
        powerUpTimeStep = powerUpTimeMax;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case HandGermStates.idling:
                Idling();
                break;
            case HandGermStates.moving:
                Moving();
                break;
            case HandGermStates.poweringUp:
                PoweringUp();
                break;
        }
        StepNeeds();
        GetCollisions();
    }

    void Idling()
    {
        RandomMoveAnimation();
        NewNeedsAction();
    }

    void NewNeedsAction()
    {
        if (moveTimeStep <= 0)
        {
            state = HandGermStates.moving;
        }
        if(powerUpTimeStep <= 0 && potency < 3)
        {
            state = HandGermStates.poweringUp;
        }
    }

    void Moving()
    {
        moveTimeMax = UnityEngine.Random.Range(moveTimeRangeMin, moveTimeRangeMax);
        moveTimeStep = moveTimeMax;
        GameObject[] tempCollidedObjects = collidedObjects.ToArray();
        tempCollidedObjects = ShuffleArray(tempCollidedObjects);
        foreach (GameObject i in tempCollidedObjects)
        {
            if (i.CompareTag("handgermspot") && i.transform != transform.parent)
            {
                MoveToAnother(i.transform);
                state = HandGermStates.idling;
                return;
            }
        }
        state = HandGermStates.idling;
    }

    void PoweringUp()
    {
        powerUpTimeMax = UnityEngine.Random.Range(powerUpTimeRangeMin, powerUpTimeRangeMax);
        powerUpTimeStep = powerUpTimeMax;
        potency++;
        sprite.localScale = new Vector3(1 + (potency/4), 1 + (potency/4), sprite.localScale.z);
        state = HandGermStates.idling;
    }

    void StepNeeds()
    {
        moveTimeStep -= Time.deltaTime;
        powerUpTimeStep -= Time.deltaTime;
    }

    void MoveToAnother(Transform parentToTransformTo)
    {
        transform.parent = parentToTransformTo;
        transform.localPosition = new Vector3(0, 0, 0);
    }

    static T[] ShuffleArray<T>(T[] array)
    {
        System.Random random = new System.Random();
        return array.OrderBy(x => random.Next()).ToArray();
    }
}
