using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.AI;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;

public class HandGermBehavior : MonoBehaviour
{

    enum HandGermStates
    {
        moving,
        combining,
        infecting,
        poweringUp,
        idling,
    }

    HandGermStates state = HandGermStates.idling;

    public float potency = 0;

    float moveTimeMax;
    float moveTimeStep;

    [SerializeField]
    float moveVal;
    [SerializeField]
    Transform sprite;

    [SerializeField]
    float radius = 5f, speed = 1f;
    Vector3 baseStartPoint;
    Vector3 destination;
    Vector3 start;
    float progress = 0f;

    [SerializeField]
    float moveTimeRangeMin, moveTimeRangeMax;

    List<GameObject> collidedObjects = new List<GameObject>();

    void Start()
    {
        start = sprite.transform.localPosition;
        baseStartPoint = sprite.transform.localPosition;
        progress = 0f;

        
        float moveTimeMax = UnityEngine.Random.Range(3f, 8f);
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
        }
        StepNeeds();
        RandomMoveAnimation();        

    }

    void Idling()
    {
        RandomMoveAnimation();
        NewNeedsAction();
    }

    void NewNeedsAction()
    {
        if (moveVal <= 0) {
            state = HandGermStates.moving;
        }
    }

    void RandomMoveAnimation()
    {
        bool reached = false;
        progress += speed * Time.deltaTime;

        if (progress >= 1f)
        {
            progress = 1f;
            reached = true;
        }

        sprite.transform.localPosition = (destination * progress) + start * (1 - progress);

        if (reached)
        {
            start = destination;
            PickNewRandomDestination();
            progress = 0f;
        }
    }

    void Moving()
    {
        moveTimeMax = UnityEngine.Random.Range(moveTimeRangeMin, moveTimeRangeMax);
        moveTimeStep = 0;
        GameObject[] tempCollidedObjects = collidedObjects.ToArray();
        tempCollidedObjects = ShuffleArray(tempCollidedObjects);
        foreach (GameObject i in collidedObjects)
        {
            if (i.CompareTag("handgermspot") && i.transform != transform.parent)
            {
                MoveToAnother(i.transform);
                break;
            }
        }
    }

    void StepNeeds()
    {
        moveTimeStep -= Time.deltaTime;
    }


    void PickNewRandomDestination()
    {
        Vector3 vector3 = (UnityEngine.Random.insideUnitCircle * radius);
        destination = vector3 + baseStartPoint;
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
