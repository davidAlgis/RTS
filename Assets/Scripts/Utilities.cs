using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

public class Utilities : MonoBehaviour
{
    public static Vector2 matrixTimesVector2(float[,] matrix, Vector2 vector)
    {
        if (matrix.Length < 4)
        {
            Debug.LogError("Size of matrix and vector not homogeneous return the vector2.zero.");
            return Vector2.zero;
        }
        else
            return new Vector2(matrix[0, 0] * vector.x + matrix[0, 1] * vector.y, matrix[1, 0] * vector.x + matrix[1, 1] * vector.y);

    }

    public static float[,] invMatrix2x2(float[,] matrix)
    {
        if (matrix.Length < 4)
        {
            Debug.LogError("Size of matrix not homogeneous. Return matrix 2x2 zero.");
            return new float[2, 2] { { 0.0f, 0.0f }, { 0.0f, 0.0f } };
        }
        else
        {
            float det = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

            if(det != 0)
            {
                det = 1 / det;
                return new float[2, 2] { { det * matrix[1, 1], -det * matrix[0, 1] }, { -det * matrix[1, 0], det * matrix[0, 0] } };
            }
            else
            {
                Debug.LogError("The matrix is not invertible. Return matrix 2x2 zero.");
                return new float[2, 2] { { 0.0f, 0.0f }, { 0.0f, 0.0f } };
            }
        }
    }

    public static Vector2 changeBasis(float[,] P, Vector2 vec)
    {
        if (P.Length < 4)
        {
            Debug.LogError("Size of matrix not homogeneous. Return vector 2x1 zero.");
            return Vector2.zero;
        }

        float[,] Pinv = invMatrix2x2(P);

        //return P*vec
        return matrixTimesVector2(P,vec);
    }

    public static Vector2 translateVector(Vector2 translation, Vector2 vec)
    {
        return vec + translation;
    }

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
        int nbrOfPointInCircle = 15;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.loop = true;
        lineRenderer.positionCount = nbrOfPointInCircle;
        float height;
        height = 0.75f - pos.y;
        Vector3[] vertexPositions = new Vector3[nbrOfPointInCircle];
        int index = 0;
        for(float coef = 0.0f; coef < 2 * Mathf.PI; coef += 2*Mathf.PI/ nbrOfPointInCircle)
        {
            vertexPositions[index] = new Vector3(radius * Mathf.Cos(coef), height, radius * Mathf.Sin(coef));
            index++;
        }
        lineRenderer.SetPositions(vertexPositions);
    }

    public static bool isPositionAvailable(Vector3 position, float radius)
    {
        int layerMask = ~(LayerMask.GetMask("Floor") | LayerMask.GetMask("Selectable"));

        var hitColliders = Physics.OverlapSphere(position, radius,layerMask);


        if (hitColliders.Length > 0)
        {
            print("hit :" + hitColliders[0].gameObject.name);
            return false;
        }
        else
            return true;
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