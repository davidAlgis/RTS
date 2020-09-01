using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

public class Utilities : MonoBehaviour
{
    public static bool isCloseEpsilonVec3(Vector3 x, Vector3 y, float epsilon = 0.5f)
    {
        if (Vector3.Distance(x, y) < epsilon)
            return true;
        else
            return false;
    }

    public static bool isCloseEpsilonf(float x, float y, float epsilon = 0.5f)
    {
        if (Mathf.Abs(x-y) < epsilon)
            return true;
        else
            return false;
    }



    public static float crossProductVector2(Vector2 v, Vector2 w)
    {
        return v.x*w.y-v.y*w.x; 
    }

    public static bool haveSameSign(float x, float y)
    {
        return ((x >= 0 && y >= 0) || (x <= 0 && y <= 0));
    }

    public static Vector2 rotate(Vector2 pointA, Vector2 pointB ,float theta, float scale=1)
    {
        
        Vector2 AB = scale *(pointB - pointA);
        //convert degree to radian
        theta *= (Mathf.PI / 180);
        Vector2 afterRotation = new Vector2(Mathf.Cos(theta)*AB.x - Mathf.Sin(theta) * AB.y, Mathf.Sin(theta) * AB.x + Mathf.Cos(theta) * AB.y);

        return afterRotation + pointA;
    }

    public static void instantiateSphereAtPosition(Vector3 position, string name = "Sphere")
    {
        GameObject Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Sphere.name = name;
        Sphere.transform.position = position;
        Sphere.GetComponent<SphereCollider>().enabled = false;
        //Sphere.transform.localScale /= 10;
    }

    //pass the extremities of the box and return the size and center of the box
    public static void convertPropertiesBoxCollider(Vector3 extremity1, Vector3 extremity2, out Vector3 center, out Vector3 size)
    {
        
        center = 0.5f * (extremity1 + extremity2);
        if(extremity2.x > extremity1.x || extremity2.y > extremity1.y || extremity2.z > extremity1.z)
            size = new Vector3(2.0f * Mathf.Abs((extremity2 - center).x), 2.0f * Mathf.Abs((extremity2 - center).y), 2.0f * Mathf.Abs((extremity2 - center).z));
        else
            size = new Vector3(2.0f * Mathf.Abs((extremity1 - center).x), 2.0f * Mathf.Abs((extremity1 - center).y), 2.0f * Mathf.Abs((extremity1 - center).z));
    }

    public static bool navMeshHaveReachDestination(NavMeshAgent agent)
    {
        //bool reachDestination = false;
        if (!agent.pathPending)
            if (agent.remainingDistance <= agent.stoppingDistance)
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    return true;

        return false;
    }

    public static void drawSquareContour(LineRenderer lineRenderer, Vector3 pos, float radius, float width = 0.1f)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        float left, up, right, down, height;
        left = - radius / Mathf.Sqrt(2);
        right = radius / Mathf.Sqrt(2);
        up =  radius / Mathf.Sqrt(2);
        down = - radius / Mathf.Sqrt(2);
        height =  0.75f - pos.y;
        Vector3[] vertexPositions = new Vector3[4] { new Vector3(left, height, up), new Vector3(left, height, down), new Vector3(right, height, down), new Vector3(right, height, up) };
        lineRenderer.SetPositions(vertexPositions);
    }

    public static void drawCircleContour(LineRenderer lineRenderer, Vector3 pos, float radius, float width = 0.1f)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        float height;
        height = 0.75f - pos.y;
        int nbrOfPointInCircle = 10;
        Vector3[] vertexPositions = new Vector3[nbrOfPointInCircle];
        //{ new Vector3(left, height, up), new Vector3(left, height, down), new Vector3(right, height, down), new Vector3(right, height, up) };
        int index = 0;
        for(float coef = 0.0f; index < 2 * Mathf.PI; coef += Mathf.PI/ nbrOfPointInCircle)
        {
            vertexPositions[index] = new Vector3(radius * Mathf.Cos(coef), height, radius * Mathf.Sin(coef));
        }
        lineRenderer.SetPositions(vertexPositions);
    }
}


[System.Serializable]
public class Pair<T, U>
{
    public Pair(T first, U second)
    {
        this.first = first;
        this.second = second;
    }

    public T first { get; set; }
    public U second { get; set; }
};