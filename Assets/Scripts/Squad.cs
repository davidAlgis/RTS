using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Squad
{
    private List<SMovable> m_squadSMovable = new List<SMovable>();
    private List<Vector2> m_grid = new List<Vector2>();
    private Vector3 m_goal;
    private Vector2 m_direction;
    private uint m_nearerSquare;
    private float m_radiusMax;
    private int m_currentIndexGrid = 0;
    private bool m_directionIsSet = false;
    public List<SMovable> SquadSMovable { get => m_squadSMovable; set => m_squadSMovable = value; }
    public bool DirectionIsSet { get => m_directionIsSet; set => m_directionIsSet = value; }
    public Vector3 Goal { get => m_goal; set => m_goal = value; }

    public Squad(){}

    public void resetSquad()
    {
        m_grid.Clear();
        m_currentIndexGrid = 0;
        m_directionIsSet = false;
    }

    public void defineGoalAndSquadLeader(Vector3 goal)
    {
        resetSquad();

        if (m_squadSMovable == null)
        {
            Debug.LogWarning("m_squadSMovable is empty");
            return;
        }

        float lengthMax = 0.0f;
        SMovable squadLeader = m_squadSMovable[0];
        Vector3 pointForDirection = m_squadSMovable[0].transform.position;

        foreach(SMovable smovable in m_squadSMovable)
        {
            Pair<float,Vector3> lengthAndFinalCorner = smovable.calculatePathLength(goal);
            if (lengthMax < lengthAndFinalCorner.first)
            {
                squadLeader = smovable;
                lengthMax = lengthAndFinalCorner.first;
                pointForDirection = lengthAndFinalCorner.second;
            }
        }
        m_goal = goal;

        setDirection(pointForDirection);
    }

    /*when the first smovable reach the zone around the goal, he defined the goal 
     * and the other smovable stop their coroutine. */
    public void setDirection(Vector3 point)
    {
        if (m_goal != null)
        {
            m_direction = new Vector2(m_goal.x - point.x, m_goal.z - point.z);
            m_direction = m_direction.normalized;
        } 
        else
        {
            Debug.LogError("The goal has not been defined");
            return;
        }

        if(m_squadSMovable == null)
        {
            Debug.LogError("The squad has not been well defined");
            return;
        }

        float squareRoot = Mathf.Sqrt(m_squadSMovable.Count);

        if (squareRoot - (int)squareRoot == 0.0f)
            m_nearerSquare = (uint)(squareRoot);
        else
            m_nearerSquare = (uint)(squareRoot) + 1;

        m_radiusMax = 1.0f;
        
        foreach (SMovable smovable in m_squadSMovable)
        {
            smovable.StopAllCoroutines();
            if (smovable.Radius > m_radiusMax)
                m_radiusMax = smovable.Radius;
        }
        setGrid();

        m_directionIsSet = true;
    }

    public void getDestination(SMovable smovable)
    {
        while(m_currentIndexGrid < m_grid.Count)
        {
            Vector2 currentPositionOnGrid = getNthPositionOnGrid(m_currentIndexGrid);
            Vector3 newDestination = new Vector3(currentPositionOnGrid.x, 0.1f, currentPositionOnGrid.y);

            if ( Utilities.isPositionAvailable(newDestination, smovable.Radius))
            {
                m_currentIndexGrid++;
                smovable.Agent.destination = newDestination;
                return;
            }

            m_currentIndexGrid++;
        }

        //when we have tried every possible destination on grid
        Debug.LogWarning("Unable to find any available destination for " + smovable.gameObject.name);
        smovable.Agent.destination = smovable.transform.position;
    }

    public void setGrid()
    {

        float sizeOfSquare = m_radiusMax * ( m_nearerSquare - 1);
        Vector2 currentPosition = new Vector2(-sizeOfSquare, sizeOfSquare);


        float[,] newBasis = new float[2, 2] {{ m_direction.y, -m_direction.x }, { m_direction.x, m_direction.y } };
        newBasis = Utilities.invMatrix2x2(newBasis);

        Vector2 translation = new Vector2(m_goal.x, m_goal.z);
        m_grid.Add(Utilities.translateVector(translation, Utilities.changeBasis(newBasis, currentPosition)));
        //print("0 = " + m_grid[0] + " in new basis =" + Utilities.translateVector(translation, Utilities.changeBasis(newBasis, m_grid[0])));
        for (int i = 1; i < m_nearerSquare * m_nearerSquare * 2;i++)
        {
            currentPosition.x += m_radiusMax * 2;

            if (i % m_nearerSquare == 0)
            {
                currentPosition.x = -sizeOfSquare;
                currentPosition.y -= m_radiusMax * 2;
            }
            m_grid.Add(Utilities.translateVector(translation, Utilities.changeBasis(newBasis, currentPosition)));
            //Utilities.instantiateSphereAtPosition(new Vector3(m_grid[i].x, 0.1f, m_grid[i].y), "grid" + i);
        }
    }

    public Vector2 getNthPositionOnGrid(int n)
    {
        Vector2 position = new Vector2(m_goal.x, m_goal.z);
        if (m_grid == null)
            return position;

        if (m_grid.Count < n)
            return position;

        return m_grid[n];
    }

    

}
