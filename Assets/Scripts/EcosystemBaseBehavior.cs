using UnityEngine;
using System.Collections.Generic;

public class EcosystemBaseBehavior : MonoBehaviour
{
    public List<GameObject> collidedObjects = new List<GameObject>();
    
    
    [HideInInspector]
    public Transform sprite;

    [SerializeField]
    public float randomMoveRadius, randomMoveSpeed;
    [HideInInspector]
    public Vector3 baseStartPoint;
    [HideInInspector]
    public Vector3 destination;
    [HideInInspector]
    public Vector3 start;
    [HideInInspector]

    public float progress = 0f;

    public float searchRadius = 2.5f;

   public Transform FindNearest(List<GameObject> objsToFind)
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
    public void GetCollisions()
    {
        collidedObjects.Clear();
        Collider2D[] collidingObjectsCircle = Physics2D.OverlapCircleAll(transform.position, .4f);
        foreach (Collider2D i in collidingObjectsCircle)
        {
            collidedObjects.Add(i.gameObject);
        }
    }

    public bool IsNameColliding(string nameString)
    {
        foreach (GameObject i in collidedObjects)
        {
            if (i.name == nameString)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsTagColliding(string tagString)
    {
        foreach (GameObject i in collidedObjects)
        {
            if (i.CompareTag(tagString))
            {
                return true;
            }
        }
        return false;
    }
    public GameObject GetObjectWithName(string nameString)
    {
        foreach (GameObject i in collidedObjects)
        {
            if (i.name == nameString)
            {
                return i;
            }
        }
        return null;
    }

    
    public GameObject GetObjectWithTag(string tagString)
    {
        foreach (GameObject i in collidedObjects)
        {
            if (i.CompareTag(tagString)&& i != this.gameObject)
            {
                return i;
            }
        }
        return null;
    }

    public bool CheckSpecificColliding(GameObject specificObject)
    {
        foreach (GameObject i in collidedObjects)
        {
            if (i == specificObject)
            {
                return true;
            }
        }
        return false;
    }

    public void GetWideCollisions()
    {
        collidedObjects.Clear();
        Collider2D[] collidingObjectsCircle = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        foreach (Collider2D i in collidingObjectsCircle)
        {
            collidedObjects.Add(i.gameObject);
        }
    }


    public void RandomMoveAnimation()
    {
        bool reached = false;
        progress += randomMoveSpeed * Time.deltaTime;

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

    public void PickNewRandomDestination()
    {
        Vector3 vector3 = (UnityEngine.Random.insideUnitCircle * randomMoveRadius);
        destination = vector3 + baseStartPoint;
    }
}
