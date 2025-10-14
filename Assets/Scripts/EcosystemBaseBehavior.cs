using UnityEngine;
using System.Collections.Generic;

public class EcosystemBaseBehavior : MonoBehaviour
{

    public List<GameObject> collidedObjects = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

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
}
