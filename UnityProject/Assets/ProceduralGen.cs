using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGen : MonoBehaviour
{
    public int[,] solution;
    public int gridSize = 3;                // ie. 3x3 grid
    public bool bFillEmptyRooms = false;    // Used for testing. Only draws the solution and not the filler rooms

    public Transform[] rowSpawners0;        // Top row
    public Transform[] rowSpawners1;        // Second row
    public Transform[] rowSpawners2;        // Third Row
    public GameObject[] roomTypes;          // Prefabs of rooms

    // Room type 0: Closed on all sides.
    // Room type 1: Openings on left and right sides.
    // Room type 2: Openings left, right, and downwards
    // Room type 3: Openings left, right, and upwards.
    // Room type 4: Openings on all sides.

    // Start is called before the first frame update
    void Start()
    {
        solution = GenerateLvlSolution();
        DrawRooms();
        FillEmptyRooms();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private int[,] GenerateLvlSolution()
    {
        int[,] solutionPath = new int[gridSize, gridSize];

        // Initialize multi-array to have values of -1 - "Empty Room" status
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
                solutionPath[i, j] = -1;

        // Counts how many times in a row the solution path goes downwards
        int downCounter = 0;

        // Generates a random starting room on the top row
        int initialRndPosition = Random.Range(0, gridSize);

        // Now that we have our first room on the top row - Chose a room of either type 1 or 2.
        solutionPath[0, initialRndPosition] = Random.Range(1, 3);

        // Save the indexes of the last row and column that we used
        int lastRow = 0;
        int lastCol = initialRndPosition;

        // Generate the rest of the solution.
        for (; ; )
        {
            // Generate a random direction.
            // 1 or 2 = Go left
            // 3 or 4 = Go right
            // 5 = Go down
            int rdmDirection = Random.Range(1, 6);

            if (rdmDirection == 1 || rdmDirection == 2)
            {
                // Try to go left only if the solution does not go beyond the grid (ie. -1)...
                if (IsPathOk(solutionPath, lastRow, lastCol - 1))
                {
                    // Set the room to type 1, decrease the last column value by one because we moved left by one,
                    // and reset the down counter to 0 because we moved sideways.
                    solutionPath[lastRow, lastCol - 1] = 1;
                    lastCol--;
                    downCounter = 0;
                }
                else
                {
                    // else, go down because we are already on the edge of the grid.
                    rdmDirection = 5;
                }
            }
            else if (rdmDirection == 3 || rdmDirection == 4)
            {
                // Try to go right only if the solution does not go beyond the grid.
                if (IsPathOk(solutionPath, lastRow, lastCol + 1))
                {
                    // Set the room to type 1, increase the last column value by one because we moved right by one,
                    // and reset the down counter to 0 because we moved sideways.
                    solutionPath[lastRow, lastCol + 1] = 1;
                    lastCol++;
                    downCounter = 0;
                }
                else
                {
                    // else, go down because we are already on the edge of the grid.
                    rdmDirection = 5;
                }
            }
            else if (rdmDirection == 5)
            {
                // Increase the down counter to determine if a room of type 4 (a top and bottom opening) is needed.
                downCounter++;

                // Update the previous room to have an appropiate opening on the top and bottom
                int updatedRoom = downCounter >= 2 ? 4 : 2;
                solutionPath[lastRow, lastCol] = updatedRoom;

                if (IsPathOk(solutionPath, lastRow + 1, lastCol))
                {
                    // Generate a room with an upward opening, 
                    // and increase the last row value by one because we moved down by one.
                    solutionPath[lastRow + 1, lastCol] = 3;
                    lastRow++;
                }
                else
                {
                    // The solution is complete
                    break;
                }
            }
        }

        return solutionPath;
    }

    // Will make sure that the solution will not go outside the grid
    private bool IsPathOk(int[,] solutionPath, int row, int col)
    {
        if (row < 0 || row >= gridSize || col < 0 || col >= gridSize) { return false; }

        return solutionPath[row, col] == -1;
    }

    private void AddRoom(int row, int col, int room)
    {
        Transform[] targetRow = null;

        switch (row)
        {
            case 0:
                targetRow = rowSpawners0;
                break;
            case 1:
                targetRow = rowSpawners1;
                break;
            case 2:
                targetRow = rowSpawners2;
                break;
            default:
                break;
        }

        GameObject newRoom = Instantiate(roomTypes[room], targetRow[col].position, transform.rotation);
        targetRow[col] = newRoom.transform;
    }

    private void DrawRooms()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // Only create a solution room if the solution path element has a value of -1  - "Empty room" status
                if (solution[i, j] != -1)
                {
                    AddRoom(i, j, solution[i, j]);
                }
            }
        }
    }

    private void FillEmptyRooms()
    {
        if (!bFillEmptyRooms) { return; }

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // Only create a filler room if the solution path element has a value of -1  - "Empty room" status
                if (solution[i, j] == -1)
                {
                    int rndEmptyRoom = Random.Range(0, 2);
                    AddRoom(i, j, rndEmptyRoom);
                }
            }
        }
    }
}
