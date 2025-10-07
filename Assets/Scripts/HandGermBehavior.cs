using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class HandGermBehavior : MonoBehaviour
{
    float moveTimeMax;
    float moveTimeStep;

    Transform sprite;
    void Start()
    {
        float moveTimeMax = UnityEngine.Random.Range(3f, 8f);
    }

    // Update is called once per frame
    void Update()
    {
        MoveTimer();

        Vector3 nextPos = NoiseyMove(Time.time);
        sprite.transform.Translate(nextPos * Time.deltaTime);
    }

    void MoveTimer()
    {
        moveTimeStep -= Time.deltaTime;

        if (moveTimeStep <= 0)
        {
            moveTimeMax = UnityEngine.Random.Range(3f, 8f);
            moveTimeStep = moveTimeMax;
            Collider2D[] collidedObjects = Physics2D.OverlapCircleAll(transform.position, .7f);
            collidedObjects = ShuffleArray(collidedObjects);
            foreach (Collider2D i in collidedObjects)
            {
                if (i.CompareTag("handgermspot") && i.transform != transform.parent)
                {
                    MoveToAnother(i.transform);
                }
            }
        }
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
    
    Vector3 NoiseyMove(float time)
    {
        float x = Mathf.PerlinNoise1D(time * UnityEngine.Random.Range(-2,2));
        x = x * UnityEngine.Random.Range(-1, 2);
        float y = Mathf.PerlinNoise1D(time * UnityEngine.Random.Range(-1,2));
        y = y * UnityEngine.Random.Range(-1, 2);
        Vector3 nextPos = new Vector3(x, y);
        return Vector3.Normalize(nextPos);
    }
}
