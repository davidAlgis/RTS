using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squad
{
    private List<SMovable> m_squadSMovable = new List<SMovable>();
    private List<Vector2> m_grid = new List<Vector2>();
    private Vector3 m_goal;
    private Vector2 m_direction;
    private uint m_nearerSquare;
    private float m_radiusMax;
    private int m_currentIndexGrid = 0;
    public List<SMovable> SquadSMovable { get => m_squadSMovable; set => m_squadSMovable = value; }
    public Vector3 Goal { get => m_goal; set => m_goal = value; }
    public Squad(){}

    /*when the first smovable reach the zone around the goal, he defined the goal 
     * and the other smovable stop their coroutine. */
    public void setDirection(Vector3 point)
    {
        
        if (m_goal != null)
        {
            m_direction = new Vector2(m_goal.x - point.x, m_goal.z - point.z);
            m_direction = m_direction.normalized;
        } 


        if(m_squadSMovable == null)
        {
            Debug.LogError("The squad as not been well defined");
            return;
        }

        m_nearerSquare = (uint)(Mathf.Sqrt(m_squadSMovable.Count)) + 1;

        m_radiusMax = 1.0f;
        
        foreach (SMovable smovable in m_squadSMovable)
        {
            smovable.StopAllCoroutines();
            if (smovable.Radius > m_radiusMax)
                m_radiusMax = smovable.Radius;

            smovable.lookForANewDestination(m_goal);
        }

        setGrid();

        foreach (SMovable smovable in m_squadSMovable)
            smovable.lookForANewDestination(m_goal);



    }

    public void getDestination(SMovable smovable)
    {
        Debug.LogWarning("m_grid.Count" + m_grid.Count + " m_currentIndexGrid = " + m_currentIndexGrid);
        while(m_currentIndexGrid < m_grid.Count)
        {
            Vector2 currentPositionOnGrid = getNthPositionOnGrid(m_currentIndexGrid);
            Vector3 newDestination = new Vector3(currentPositionOnGrid.x, 0.1f, currentPositionOnGrid.y);

            if ( Utilities.isPositionAvailable(newDestination, smovable.Radius))
            {
                m_currentIndexGrid++;
                Debug.LogWarning(smovable.gameObject.name +"reach destination " + m_currentIndexGrid);
                smovable.Agent.destination = newDestination;
                return;
            }

            Debug.LogWarning("doesnt suceed to go on the " + m_currentIndexGrid + " name = " + smovable.gameObject.name);
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
            //print("i = "+i+ " "  + m_grid[i] + " in new basis =" + Utilities.translateVector(translation, Utilities.changeBasis(newBasis, m_grid[i])));
            Utilities.instantiateSphereAtPosition(new Vector3(m_grid[i].x, 0.1f, m_grid[i].y), "grid" + i);
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
