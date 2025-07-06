using UdonSharp;
using VRC.SDKBase;
using UnityEngine;

public enum TileType
{
    Walkable,
    FullBlock,   // All sides visible (0000)
    ThreeSided,  // U-shaped (0001, 0010, 0100, 1000)
    Corner,      // L-shaped (0011, 0110, 1100, 1001)
    OneSided,    // One wall face (1110, 1101, 1011, 0111)
    DoubleSided, // Two adjacent wall faces (1010, 0101, 1001, 0011)
    Empty,        // No walls visible (1111)
    Exit
}
public class LevelGenerator : UdonSharpBehaviour
{
    public int m_MazeWidth = 80;
    public int m_MazeHeight = 80;

    public Game m_Game;

    void Start()
    {
    }

    // --------------------------------------------------------------------------------------------------- //
    // ----------------- Maze Generation ----------------------------------------------------------------- //
    // --------------------------------------------------------------------------------------------------- //

    //public void DebugPrintMaze(int[][] maze)
    //{
    //    for (int i = 0; i < maze.Length; i++)
    //    {
    //        string row = "";
    //        for (int j = 0; j < maze[0].Length; j++)
    //        {
    //            row += maze[i][j] + " ";
    //        }
    //        Debug.Log(row);
    //    }

    //    Debug.Log($"Last visited cell: {this.m_LastVisitedCell}");
    //}

    private int[][] m_CopiedMaze = new int[80][];

    private int GetCopedMazeValue(int i, int j)
    {
        return this.m_CopiedMaze[i][j];
    }

    private void SetCopiedMazeValue(int i, int j, int value)
    {
        this.m_CopiedMaze[i][j] = value;
    }
    // Variables to keep track of the grid traversal
    private int m_CurrentI = 0;
    private int m_CurrentJ = 0;
    private int m_CurrentIsland = 0;

    // Constants
    private int m_MaxIslands = 30; // Maximum number of islands to detect

    // Variables for maze dimensions
    private int m_MaxX;
    private int m_MaxY;
    private int m_MaxIslandSize;

    // Stack pointer for DFS
    private int m_StackPointer = 0;

    // Flags
    private bool m_IsFindingIslands = false;
    private void FindIslands()
    {
        InitializeIslandVariables();
        InitializeIslandsArray();
    }

    private void InitializeIslandVariables()
    {
        // Initialize variables
        m_MaxIslands = 30; // Maximum number of islands to detect
        m_MaxX = this.m_MazeWidth;
        m_MaxY = this.m_MazeHeight;
        m_MaxIslandSize = this.m_MazeWidth * this.m_MazeHeight; // Maximum number of cells per island
    }

    private void InitializeIslandsArray(bool testOnly = false)
    {
        // Create a 3D array to store island data
        m_DebugInstation_Sliced_Islands = new int[m_MaxIslands][][];

        // Initialize the islands array
        for (int i = 0; i < m_MaxIslands; i++)
        {
            m_DebugInstation_Sliced_Islands[i] = new int[m_MaxIslandSize][];
            for (int j = 0; j < m_MaxIslandSize; j++)
            {
                m_DebugInstation_Sliced_Islands[i][j] = new int[2]; // Each cell stores an (x, y) coordinate
            }
        }

        if (testOnly) return;

        SendCustomEventDelayedSeconds(nameof(this.InitializeIslandSizesArray), 0.1f);
    }

    public void InitializeIslandSizesArray()
    {
        // Initialize the islandSizes array to store the size of each island
        m_DebugInstantiation_IslandSizes = new int[m_MaxIslands];
        SendCustomEventDelayedSeconds(nameof(this.CopyMaze), 0.1f);
    }

    public void CopyMaze()
    {
        // Create a new maze variable to copy the original maze
        m_CopiedMaze = new int[m_MaxX][];
        for (int i = 0; i < m_MaxX; i++)
        {
            m_CopiedMaze[i] = new int[m_MaxY];
            for (int j = 0; j < m_MaxY; j++)
            {
                m_CopiedMaze[i][j] = m_DebugMazeInstantiation_Sliced[i][j];
            }
        }

        SendCustomEventDelayedSeconds(nameof(this.PreallocateStack), 0.1f);
    }

    public void PreallocateStack()
    {
        // Preallocate the stack
        for (int i = 0; i < m_DFS_MaxSize; i++)
        {
            stack[i] = new int[2]; // Each stack element stores (x, y)
        }

        SendCustomEventDelayedSeconds(nameof(this.InitializeTraversalIndices), 0.1f);
    }

    public void InitializeTraversalIndices()
    {
        // Initialize traversal indices
        m_CurrentI = 0;
        m_CurrentJ = 0;
        m_CurrentIsland = 0;
        m_IsFindingIslands = true;
        m_StackPointer = 0;

        SendCustomEventDelayedSeconds(nameof(ProcessFindIslands), this.m_OptimisationDelayKek);
    }
    //private void FindIslands()
    //{
    //    // Initialize variables
    //    m_MaxIslands = 30; // Maximum number of islands to detect
    //    m_MaxX = this.m_MazeWidth;
    //    m_MaxY = this.m_MazeHeight;
    //    m_MaxIslandSize = this.m_MazeWidth * this.m_MazeHeight; // Maximum number of cells per island

    //    // Create a 3D array to store island data
    //    this.m_DebugInstation_Sliced_Islands = new int[m_MaxIslands][][];

    //    // Initialize the islands array
    //    for (int i = 0; i < m_MaxIslands; i++)
    //    {
    //        this.m_DebugInstation_Sliced_Islands[i] = new int[m_MaxIslandSize][];
    //        for (int j = 0; j < m_MaxIslandSize; j++)
    //        {
    //            this.m_DebugInstation_Sliced_Islands[i][j] = new int[2]; // Each cell stores an (x, y) coordinate
    //        }
    //    }

    //    // Initialize the islandSizes array to store the size of each island
    //    this.m_DebugInstantiation_IslandSizes = new int[m_MaxIslands];

    //    // Create a new maze variable to copy the original maze
    //    this.m_CopiedMaze = new int[m_MaxX][];
    //    for (int i = 0; i < m_MaxX; i++)
    //    {
    //        this.m_CopiedMaze[i] = new int[m_MaxY];
    //        for (int j = 0; j < m_MaxY; j++)
    //        {
    //            this.m_CopiedMaze[i][j] = this.m_DebugMazeInstantiation_Sliced[i][j];
    //        }
    //    }

    //    // Preallocate the stack
    //    for (int i = 0; i < m_DFS_MaxSize; i++)
    //    {
    //        stack[i] = new int[2]; // Each stack element stores (x, y)
    //    }

    //    // Initialize traversal indices
    //    m_CurrentI = 0;
    //    m_CurrentJ = 0;
    //    m_CurrentIsland = 0;
    //    m_IsFindingIslands = true;
    //    m_StackPointer = 0;

    //    // Start processing the islands
    //    SendCustomEventDelayedSeconds(nameof(ProcessFindIslands), this.m_OptimisationDelayKek);
    //}

    public void ProcessFindIslands()
    {
        if (!m_IsFindingIslands)
            return;

        int stepsPerFrame = 250; // Adjust this value based on performance requirements
        int stepsProcessed = 0;

        while (stepsProcessed < stepsPerFrame && m_CurrentI < m_MaxX)
        {
            // Get the current position
            int i = m_CurrentI;
            int j = m_CurrentJ;

            // If grid value is '0' (unvisited land), start a new island
            if (this.m_CopiedMaze[i][j] <= 0)
            {
                int cellCount = 0; // Track the number of cells in the current island
                m_StackPointer = 0;
                IterativeDFS(i, j, m_CurrentIsland, ref cellCount);

                this.m_DebugInstantiation_IslandSizes[m_CurrentIsland] = cellCount;
                m_CurrentIsland++;

                // Prevent exceeding the maximum allowed islands
                if (m_CurrentIsland >= m_MaxIslands)
                {
                    m_IsFindingIslands = false;
                    break;
                }
            }

            // Move to the next cell
            m_CurrentJ++;
            if (m_CurrentJ >= m_MaxY)
            {
                m_CurrentJ = 0;
                m_CurrentI++;
            }

            stepsProcessed++;
        }

        if (m_CurrentI < m_MaxX && m_CurrentIsland < m_MaxIslands)
        {
            // Schedule next processing step
            SendCustomEventDelayedSeconds(nameof(ProcessFindIslands), 0.1f);
        }
        else
        {
            // Finished processing
            m_IsFindingIslands = false;
            this.m_DebugInstantiation_Size = m_CurrentIsland; // Total number of islands found
            // Proceed to the next step: Instantiate the maze

            this.m_DebugInstantiation_Counter = 0;
            this.m_PiecesSinceLastTile = 0;
            this.m_IsProcessingIslandsAlready = false;

            if (this.m_TestIslandFindsOnly)
            {
                SendCustomEventDelayedFrames(
                    nameof(this.OnFindIslandsTestComplete),
                    1
                );

                return;
            }
            this.m_IsInstantiatingDebugMaze_Sliced = true;
            SendCustomEventDelayedFrames(
                nameof(this.DebugInstantiateMaze_Slice_Iteration),
                1
            );
        }
    }

    int m_DFS_MaxSize = 6400; // Maximum stack size (40x40 grid)
    int[][] stack = new int[6400][];
    private void IterativeDFS(int startX, int startY, int currentIsland, ref int cellCount)
    {
        // Initialize the stack if starting anew
        if (m_StackPointer == 0)
        {
            stack[m_StackPointer][0] = startX;
            stack[m_StackPointer][1] = startY;
            m_StackPointer++;
        }

        int dfsStepsPerFrame = 1000; // Adjust this value based on performance requirements
        int dfsStepsProcessed = 0;

        while (m_StackPointer > 0 && dfsStepsProcessed < dfsStepsPerFrame)
        {
            // Pop the top element
            m_StackPointer--;
            int x = stack[m_StackPointer][0];
            int y = stack[m_StackPointer][1];

            // Skip invalid or already visited cells
            if (x < 0 || x >= this.m_MazeWidth || y < 0 || y >= this.m_MazeHeight || this.m_CopiedMaze[x][y] >= 1)
            {
                continue;
            }

            // Mark as visited (so we don't visit again)
            this.m_CopiedMaze[x][y] = 1;

            // Record the cell in the current island if within bounds
            if (cellCount < this.m_DebugInstation_Sliced_Islands[currentIsland].Length)
            {
                this.m_DebugInstation_Sliced_Islands[currentIsland][cellCount][0] = x;
                this.m_DebugInstation_Sliced_Islands[currentIsland][cellCount][1] = y;
                cellCount++;
            }
            else
            {
                // If cellCount exceeds allocated size, stop recording for this island
                Debug.LogWarning("Island size exceeded. Consider increasing the maxIslandSize.");
                break;
            }

            // Push neighbors onto the stack (including diagonal neighbors)
            PushToStack(x - 1, y);     // Up
            PushToStack(x + 1, y);     // Down
            PushToStack(x, y - 1);     // Left
            PushToStack(x, y + 1);     // Right

            // Diagonal neighbors
            PushToStack(x - 1, y - 1); // Top-left
            PushToStack(x - 1, y + 1); // Top-right
            PushToStack(x + 1, y - 1); // Bottom-left
            PushToStack(x + 1, y + 1); // Bottom-right

            dfsStepsProcessed++;
        }

        if (m_StackPointer > 0)
        {
            // Continue DFS in the next frame
            SendCustomEventDelayedSeconds(nameof(ProcessIterativeDFS), this.m_OptimisationDelayKek);
        }
    }

    public void ProcessIterativeDFS()
    {
        int currentIsland = m_CurrentIsland - 1; // Since m_CurrentIsland was incremented
        int cellCount = this.m_DebugInstantiation_IslandSizes[currentIsland];

        IterativeDFS(0, 0, currentIsland, ref cellCount);

        // Update the cell count after processing
        this.m_DebugInstantiation_IslandSizes[currentIsland] = cellCount;
    }
    private void PushToStack(int x, int y)
    {
        if (m_StackPointer < m_DFS_MaxSize - 1)
        {
            stack[m_StackPointer][0] = x;
            stack[m_StackPointer][1] = y;
            m_StackPointer++;
        }
        else
        {
            Debug.LogWarning("Stack overflow in DFS. Consider increasing m_DFS_MaxSize.");
        }
    }

    //private void FindIslands()
    //{
    //    int maxIslands = 30; // Maximum number of islands to detect
    //    int maxX = this.m_MazeWidth;
    //    int maxY = this.m_MazeHeight;
    //    int maxIslandSize = this.m_MazeWidth * this.m_MazeHeight; // Maximum number of cells per island

    //    // Create a 3D array to store island data
    //    this.m_DebugInstation_Sliced_Islands = new int[maxIslands][][];

    //    // Initialize the islands array
    //    for (int i = 0; i < maxIslands; i++)
    //    {
    //        this.m_DebugInstation_Sliced_Islands[i] = new int[maxIslandSize][];
    //        for (int j = 0; j < maxIslandSize; j++)
    //        {
    //            this.m_DebugInstation_Sliced_Islands[i][j] = new int[2]; // Each cell stores an (x, y) coordinate
    //        }
    //    }

    //    // Initialize the islandSizes array to store the size of each island
    //    this.m_DebugInstantiation_IslandSizes = new int[maxIslands];

    //    // Create a new maze variable to copy the original maze
    //    this.m_CopiedMaze = new int[maxX][];
    //    for (int i = 0; i < maxX; i++)
    //    {
    //        this.m_CopiedMaze[i] = new int[maxY];
    //        for (int j = 0; j < maxY; j++)
    //        {
    //            this.m_CopiedMaze[i][j] = this.m_DebugMazeInstantiation_Sliced[i][j];
    //        }
    //    }

    //    // preallocate the stack
    //    for (int i = 0; i < m_DFS_MaxSize; i++)
    //    {
    //        stack[i] = new int[2]; // Each stack element stores (x, y)
    //    }

    //    int currentIsland = 0;
    //    // Traverse the grid one by one
    //    for (int i = 0; i < maxX; i++)
    //    {
    //        for (int j = 0; j < maxY; j++)
    //        {
    //            // If grid value is '0' (unvisited land), start a new island
    //            if (this.m_CopiedMaze[i][j] == 0)
    //            {
    //                int cellCount = 0; // Track the number of cells in the current island
    //                IterativeDFS(i, j, currentIsland, ref cellCount);
    //                this.m_DebugInstantiation_IslandSizes[currentIsland] = cellCount;
    //                currentIsland++;
    //            }

    //            // Prevent exceeding the maximum allowed islands
    //            if (currentIsland >= maxIslands) break;
    //        }

    //        // Prevent exceeding the maximum allowed islands
    //        if (currentIsland >= maxIslands) break;
    //    }

    //    this.m_DebugInstantiation_Size = currentIsland; // Total number of islands found
    //}

    //int m_DFS_MaxSize = 6400; // Maximum stack size (40x40 grid)
    //int[][] stack = new int[6400][];
    //private void IterativeDFS(int startX, int startY, int currentIsland, ref int cellCount)
    //{
    //    //int maxSize = this.m_MazeWidth * this.m_MazeHeight;
    //    //int maxSize = 5000; // Maximum stack size (40x40 grid)
    //    //int[][] stack = new int[maxSize][];
    //    int stackPointer = 0;

    //    // Push the starting cell onto the stack
    //    stack[stackPointer][0] = startX;
    //    stack[stackPointer][1] = startY;
    //    stackPointer++;

    //    // Iterate until all neighbors have been visited
    //    while (stackPointer > 0)
    //    {
    //        // Pop the top element
    //        stackPointer--;
    //        int x = stack[stackPointer][0];
    //        int y = stack[stackPointer][1];

    //        // Skip invalid or already visited cells
    //        if (x < 0 || x >= this.m_MazeWidth || y < 0 || y >= this.m_MazeHeight || this.m_CopiedMaze[x][y] == 1)
    //        {
    //            continue;
    //        }

    //        // Mark as visited (so we don't visit again)
    //        this.m_CopiedMaze[x][y] = 1;

    //        // Record the cell in the current island if within bounds
    //        if (cellCount < this.m_DebugInstation_Sliced_Islands[currentIsland].Length)
    //        {
    //            this.m_DebugInstation_Sliced_Islands[currentIsland][cellCount][0] = x;
    //            this.m_DebugInstation_Sliced_Islands[currentIsland][cellCount][1] = y;
    //            cellCount++;
    //        }
    //        else
    //        {
    //            // If cellCount exceeds allocated size, stop recording for this island
    //            Debug.LogWarning("Island size exceeded. Consider increasing the maxIslandSize.");
    //            break;
    //        }

    //        // Push neighbors onto the stack (including diagonal neighbors)
    //        if (stackPointer < m_DFS_MaxSize - 1) { stack[stackPointer][0] = x - 1; stack[stackPointer++][1] = y; }     // Up
    //        if (stackPointer < m_DFS_MaxSize - 1) { stack[stackPointer][0] = x + 1; stack[stackPointer++][1] = y; }     // Down
    //        if (stackPointer < m_DFS_MaxSize - 1) { stack[stackPointer][0] = x; stack[stackPointer++][1] = y - 1; }    // Left
    //        if (stackPointer < m_DFS_MaxSize - 1) { stack[stackPointer][0] = x; stack[stackPointer++][1] = y + 1; }    // Right

    //        // Diagonal neighbors
    //        if (stackPointer < m_DFS_MaxSize - 1) { stack[stackPointer][0] = x - 1; stack[stackPointer++][1] = y - 1; } // Top-left
    //        if (stackPointer < m_DFS_MaxSize - 1) { stack[stackPointer][0] = x - 1; stack[stackPointer++][1] = y + 1; } // Top-right
    //        if (stackPointer < m_DFS_MaxSize - 1) { stack[stackPointer][0] = x + 1; stack[stackPointer++][1] = y - 1; } // Bottom-left
    //        if (stackPointer < m_DFS_MaxSize - 1) { stack[stackPointer][0] = x + 1; stack[stackPointer++][1] = y + 1; } // Bottom-right
    //    }
    //}

    private int state;

    public int Next()
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        return state;
    }

    public float NextFloat(float min, float max)
    {
        return min + (Next() & int.MaxValue) / (float)int.MaxValue * (max - min);
    }

    public int NextInt(int min, int max)
    {
        int range = max - min;
        int next = (int)(Next() & int.MaxValue); // Ensure non-negative
        return min + (next % range);
    }
    
    public bool m_TestIslandFindsOnly = false;
    public int GenerateMaze(int width, int height)
    {
        if (this.m_Game == null) return -1;
        this.ResetTwistyPathVariables();
        UnityEngine.Random.InitState(Networking.GetServerTimeInMilliseconds());
        this.state = (int)Networking.GetServerTimeInMilliseconds();

        for (int i = 0; i < height; i++)
        {
            this.m_Game.m_InitialMazeData[i] = new int[width];
            for (int j = 0; j < width; j++)
            {
                this.m_Game.m_InitialMazeData[i][j] = 0;
            }
        }

        // Get exit
        this.m_Game.m_InitialMazeDataExit = PickExit(width, height);
        m_LastVisitedCell = this.m_Game.m_InitialMazeDataExit;
        this.m_Game.m_InitialMazeData[this.m_Game.m_InitialMazeDataExit.x][this.m_Game.m_InitialMazeDataExit.y] = 1;

        Vector2Int[] path = new Vector2Int[width * height];

        int twistiness = this.m_Game.m_EasyModeOn ? 150 : 10;
        int size;
        path = GenerateTwistyPath(width, height, new Vector2Int(1, 1), this.m_Game.m_InitialMazeDataExit, out size, twistiness);

        bool foundExit = false;
        for (int i = size - 1; i >= 0; i--)
        {
            if (path[i].x == this.m_Game.m_InitialMazeDataExit.x && path[i].y == this.m_Game.m_InitialMazeDataExit.y)
            {
                foundExit = true;
                break;
            }
            //else if ((path[i] - this.m_Game.m_InitialMazeDataExit).magnitude < 2)
            //{
            //    foundExit = true;
            //    break;
            //}
        }

        if (foundExit)
        {
            //carve the path
            for (int i = 0; i < path.Length; i++)
            {
                this.m_Game.m_InitialMazeData[path[i].x][path[i].y] = 1;
            }

            this.CacheWalkableSpaces(ref width, ref height);
            
            this.PlaceRandomLogJumpTile();
            this.PlaceRandomLogJumpTile();

            this.RandomClutter(2, 2f, 5); // little rocks
            this.RandomClutter(5, 1.5f, 15); // yellow flower patch
            this.RandomClutter(6, 1.5f, 15); // white flower patch
            this.RandomClutter(7, 1.5f, 15); // pink flower patch
            this.RandomClutter(8, 1.5f, 15); // red mushroom patch
            this.RandomClutter(9, 1.5f, 15); // cup mushroom patch
            this.RandomClutter(10, 1.5f, 15); // yellow mushroom patch
            this.RandomClutter(11, 1.5f, 15); // stump and puff mushroom

            this.RandomRocks(ref width, ref height);

            // // Entrance
            this.m_Game.m_InitialMazeData[0][0] = 1;
            this.m_Game.m_InitialMazeData[0][1] = 1;
            this.m_Game.m_InitialMazeData[0][2] = 1;
            this.m_Game.m_InitialMazeData[1][0] = 1;
            this.m_Game.m_InitialMazeData[1][1] = 1;
            this.m_Game.m_InitialMazeData[1][2] = 1;
            this.m_Game.m_InitialMazeData[2][0] = 1;
            this.m_Game.m_InitialMazeData[2][1] = 1;
            this.m_Game.m_InitialMazeData[2][2] = 1;

            // Copy the generated maze into m_CopiedMaze for island processing
            this.m_CopiedMaze = new int[width][];
            for (int i = 0; i < width; i++)
            {
                this.m_CopiedMaze[i] = new int[height];
                for (int j = 0; j < height; j++)
                {
                    this.m_CopiedMaze[i][j] = this.m_Game.m_InitialMazeData[i][j];
                }
            }

            this.m_DebugMazeInstantiation_Sliced = new int[width][];
            for (int i = 0; i < width; i++)
            {
                this.m_DebugMazeInstantiation_Sliced[i] = new int[height];
                for (int j = 0; j < height; j++)
                {
                    this.m_DebugMazeInstantiation_Sliced[i][j] = this.m_Game.m_InitialMazeData[i][j];
                }
            }

            // Initialize island-finding variables
            this.m_CurrentI = 0;
            this.m_CurrentJ = 0;
            this.m_CurrentIsland = 0;
            this.m_MaxX = width;
            this.m_MaxY = height;
            this.m_IsFindingIslands = true;

            this.m_TestIslandFindsOnly = true;

            InitializeIslandVariables();
            InitializeIslandsArray(true);
            this.m_DebugInstantiation_IslandSizes = new int[m_MaxIslands];
            
            // Start the island-finding process
            SendCustomEventDelayedSeconds(nameof(this.PreallocateStack), 0.1f);

            return 1; // Maze generation succeeded
        }
        else
        {
            this.ResetTwistyPathVariables();
            return 0;
        }
    }

    public void OnFindIslandsTestComplete()
    {
        SendCustomEventDelayedSeconds(
            nameof(this.DebugInstantiateMaze_Slice_Loops),
            this.m_OptimisationDelayKek
        );
    }

    public void OnDoLoopsTestComplete()
    {
        this.m_Game.On_MazeGenerationComplete(this.m_TotalEnemies);
        this.m_TestIslandFindsOnly = false;
    }

    private Vector2Int[] m_WalkableSpaces; // Array to store all walkable spaces
    private int m_WalkableSpacesCount = 0; // Track the number of walkable spaces

    private void CacheWalkableSpaces(ref int width, ref int height)
    {
        m_WalkableSpaces = new Vector2Int[width * height]; // Allocate maximum possible size
        m_WalkableSpacesCount = 0; // Reset the count

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (this.m_Game.m_InitialMazeData[x][y] == 1) // Check if the tile is walkable
                {
                    m_WalkableSpaces[m_WalkableSpacesCount++] = new Vector2Int(x, y); // Add to walkable spaces
                }
            }
        }
    }

    private Vector2Int[] m_ForestEdges;
    private int m_ForestEdgesCount = 0; // Track the number of forest edges
    private void CacheForestEdges(ref int width, ref int height)
    {
        m_ForestEdges = new Vector2Int[width * height]; // Allocate maximum possible size
        m_ForestEdgesCount = 0; // Reset the count

        // Iterate through cached walkable spaces
        for (int i = 0; i < m_WalkableSpacesCount; i++)
        {
            Vector2Int space = m_WalkableSpaces[i];

            // Check the 4 cardinal directions for potential forest edges
            for (int dir = 0; dir < 4; dir++)
            {
                int neighborX = space.x + dirX[dir];
                int neighborY = space.y + dirY[dir];

                // Ensure the neighbor is within bounds
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    // Check if the neighbor is a potential forest edge (value <= 0)
                    if (this.m_Game.m_InitialMazeData[neighborX][neighborY] <= 0)
                    {
                        m_ForestEdges[m_ForestEdgesCount++] = new Vector2Int(neighborX, neighborY);
                    }
                }
            }
        }
    }

    private int[] FindNarrowWalkablePassageways(int passagewayLength)
    {
        // Create a temporary array to store the indexes of narrow passageways
        int[] narrowPassagewayIndexes = new int[m_WalkableSpacesCount];
        int narrowPassagewayCount = 0;

        // Iterate through all walkable spaces to find narrow passageways
        for (int i = 0; i < m_WalkableSpacesCount; i++)
        {
            Vector2Int space = m_WalkableSpaces[i];
            if (this.m_Game.m_InitialMazeData[space.x][space.y] != 1) continue;

            // Check for horizontal passageways
            bool isHorizontalPassageway = true;
            for (int offset = 0; offset < passagewayLength; offset++)
            {
                int checkX = space.x + offset;
                if (checkX >= m_MazeWidth || this.m_Game.m_InitialMazeData[checkX][space.y] != 1 ||
                    (space.y > 0 && this.m_Game.m_InitialMazeData[checkX][space.y - 1] > 0) || // Check west
                    (space.y < m_MazeHeight - 1 && this.m_Game.m_InitialMazeData[checkX][space.y + 1] > 0)) // Check east
                {
                    isHorizontalPassageway = false;
                    break;
                }
            }

            // Check for vertical passageways
            bool isVerticalPassageway = true;
            for (int offset = 0; offset < passagewayLength; offset++)
            {
                int checkY = space.y + offset;
                if (checkY >= m_MazeHeight || this.m_Game.m_InitialMazeData[space.x][checkY] != 1 ||
                    (space.x > 0 && this.m_Game.m_InitialMazeData[space.x - 1][checkY] > 0) || // Check north
                    (space.x < m_MazeWidth - 1 && this.m_Game.m_InitialMazeData[space.x + 1][checkY] > 0)) // Check south
                {
                    isVerticalPassageway = false;
                    break;
                }
            }

            // If either horizontal or vertical passageway is valid, store the index
            if (isHorizontalPassageway || isVerticalPassageway)
            {
                narrowPassagewayIndexes[narrowPassagewayCount++] = i;
            }
        }

        // Create a new array with the exact size of the found passageways
        int[] result = new int[narrowPassagewayCount];
        for (int i = 0; i < narrowPassagewayCount; i++)
        {
            result[i] = narrowPassagewayIndexes[i];
        }

        return result; // Return the array of indexes
    }

    private Vector2Int[] m_CachedLogTiles = new Vector2Int[100]; // Cache for log tile positions
    private int m_CachedLogTileCount = 0; // Count of cached log tiles

    private void PlaceRandomLogJumpTile()
    {
        int[] narrowPassagewayIndexes = FindNarrowWalkablePassageways(3);
        if (narrowPassagewayIndexes.Length == 0) return; // No narrow passageways found

        // Try to find a valid position for the log
        for (int attempt = 0; attempt < 10; attempt++) // Limit attempts to avoid infinite loops
        {
            // Randomly select one of the narrow passageways
            int randomIndex = NextInt(0, narrowPassagewayIndexes.Length);
            Vector2Int selectedSpace = m_WalkableSpaces[narrowPassagewayIndexes[randomIndex]];

            // Determine log type based on east and west walkability
            bool eastWalkable = (selectedSpace.x < m_MazeWidth - 1 && this.m_Game.m_InitialMazeData[selectedSpace.x + 1][selectedSpace.y] == 1);
            bool westWalkable = (selectedSpace.x > 0 && this.m_Game.m_InitialMazeData[selectedSpace.x - 1][selectedSpace.y] == 1);

            int logType = (!eastWalkable && !westWalkable) ? 3 : 4;


            // Check if the selected space is too close to an existing log tile
            bool isTooClose = false;
            bool isLogTypeUsed = false;
            for (int i = 0; i < m_CachedLogTileCount; i++)
            {
                Vector2Int cachedLog = m_CachedLogTiles[i];

                // check if this log type has been used already
                if (this.m_Game.m_InitialMazeData[cachedLog.x][cachedLog.y] == logType)
                {
                    isLogTypeUsed = true;
                    break;
                }

                int manhattanDistance = Mathf.Abs(cachedLog.x - selectedSpace.x) + Mathf.Abs(cachedLog.y - selectedSpace.y);
                if (manhattanDistance < 8)
                {
                    isTooClose = true;
                    break;
                }
            }

            if (isTooClose) continue; // Skip this space if it's too close to another log
            if (isLogTypeUsed) continue; // Skip if this log type has already been used

            // Place the log jump tile at the selected space
            this.m_Game.m_InitialMazeData[selectedSpace.x][selectedSpace.y] = logType;

            // Cache the log tile position
            if (m_CachedLogTileCount < m_CachedLogTiles.Length)
            {
                m_CachedLogTiles[m_CachedLogTileCount++] = selectedSpace;
            }

            return; // Successfully placed a log, exit the function
        }
    }

    private Vector2Int m_CurrentWalkableSpace = new Vector2Int();
    private Vector2Int[] m_CachedClutters = new Vector2Int[80 * 80]; // Store clutter positions
    private int m_CachedCluttersCount = 0; // Track the number of cached clutters
    private void RandomClutter(int type, float chance, int minDist)
    {
        for (int i = 0; i < this.m_WalkableSpacesCount; i++)
        {
            this.m_CurrentWalkableSpace = this.m_WalkableSpaces[i];
            // Skip if the current walkable space is already modified
            if (this.m_Game.m_InitialMazeData[this.m_CurrentWalkableSpace.x][this.m_CurrentWalkableSpace.y] != 1)
                continue;

            if (NextFloat(0f, 100f) < chance)
            {
                bool isTooClose = false;
                for (int j = 0; j < this.m_CachedCluttersCount; j++)
                {
                    Vector2Int cachedClutter = this.m_CachedClutters[j];
                    // Check if the cached clutter is of the same type

                    if ((Mathf.Abs(cachedClutter.x - this.m_CurrentWalkableSpace.x) + Mathf.Abs(cachedClutter.y - this.m_CurrentWalkableSpace.y) < minDist)
                    && (this.m_Game.m_InitialMazeData[cachedClutter.x][cachedClutter.y] == type))
                    {
                        isTooClose = true;
                        break;
                    }
                }

                if (!isTooClose)
                {
                    this.m_Game.m_InitialMazeData[this.m_CurrentWalkableSpace.x][this.m_CurrentWalkableSpace.y] = type;
                    m_CachedClutters[this.m_CachedCluttersCount++] = new Vector2Int(this.m_CurrentWalkableSpace.x, this.m_CurrentWalkableSpace.y);
                }
            }
        }
    }

    private void RandomRocks(ref int width, ref int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (this.m_Game.m_InitialMazeData[x][y] == 0 && NextInt(0, 100) < 10) // 10% chance to be rock variant
                    {
                        this.m_Game.m_InitialMazeData[x][y] = -1;
                    }
                }
            }
        }
    }

    private bool CheckStraightLine(int startX, int startY, Vector2Int direction)
    {
        int straightLineCount = 0;
        Vector2Int perpDirection1 = new Vector2Int(-direction.y, direction.x); // 90 degrees clockwise
        Vector2Int perpDirection2 = new Vector2Int(direction.y, -direction.x); // 90 degrees counter-clockwise

        // Check how many consecutive "0" tiles we have in this direction
        for (int i = 0; i < this.m_MazeWidth && i < this.m_MazeHeight; i++)
        {
            int currentX = startX + direction.x * i;
            int currentY = startY + direction.y * i;

            // Check bounds
            if (currentX < 0 || currentX >= this.m_MazeWidth || currentY < 0 || currentY >= this.m_MazeHeight)
                break;

            // Check if current tile is "0"
            if (this.m_Game.m_InitialMazeData[currentX][currentY] != 0)
                break;

            // Check if there's a "1" tile adjacent to this "0" tile (on the same cardinal direction)
            bool hasAdjacentOne = false;
            
            // Check perpendicular direction 1
            int adj1X = currentX + perpDirection1.x;
            int adj1Y = currentY + perpDirection1.y;
            if (adj1X >= 0 && adj1X < this.m_MazeWidth && adj1Y >= 0 && adj1Y < this.m_MazeHeight)
            {
                if (this.m_Game.m_InitialMazeData[adj1X][adj1Y] >= 1)
                    hasAdjacentOne = true;
            }

            // Check perpendicular direction 2
            int adj2X = currentX + perpDirection2.x;
            int adj2Y = currentY + perpDirection2.y;
            if (adj2X >= 0 && adj2X < this.m_MazeWidth && adj2Y >= 0 && adj2Y < this.m_MazeHeight)
            {
                if (this.m_Game.m_InitialMazeData[adj2X][adj2Y] >= 1)
                    hasAdjacentOne = true;
            }

            // If this "0" tile has an adjacent "1" tile, count it
            if (hasAdjacentOne)
            {
                straightLineCount++;
            }
            else
            {
                // Reset count if we don't find adjacent "1" tile
                straightLineCount = 0;
            }

            // If we found 4 or more consecutive "0" tiles with adjacent "1" tiles, return true
            if (straightLineCount >= 5)
            {
                return true;
            }
        }

        return false;
    }
    
    
    private Vector2Int[] m_Path;
    private bool[] m_Visited;
    private Vector2Int m_TP_Current = new Vector2Int();
    private Vector2Int m_Next = new Vector2Int();
    private Vector2Int[] m_TP_Directions;
    private int m_PathIndex = 0;
    private bool m_TP_Moved = false;

    private void ResetDirections()
    {
        this.m_TP_Directions = new Vector2Int[4] {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };
    }
    
    private void ResetTwistyPathVariables()
    {
        // Reset Vector2Int objects
        this.m_TP_Current.x = 0;
        this.m_TP_Current.y = 0;
        this.m_Next.x = 0;
        this.m_Next.y = 0;

        // Clear arrays
        this.m_Path = null;
        this.m_Visited = null;
        
        // Reset directions array
        this.m_TP_Directions = new Vector2Int[4] {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        // Reset primitives
        this.m_PathIndex = 0;
        this.m_TP_Moved = false;
    }

    public Vector2Int[] GenerateTwistyPath(int width, int height, Vector2Int start, Vector2Int exit, out int size, int twistiness)
    {
        // Initialize or resize arrays if needed
        if (m_Path == null || m_Path.Length != width * height)
        {
            m_Path = new Vector2Int[width * height];
        }
        else
        {
            System.Array.Clear(m_Path, 0, m_Path.Length);
        }

        if (m_Visited == null || m_Visited.Length != width * height)
        {
            m_Visited = new bool[width * height];
        }
        else
        {
            System.Array.Clear(m_Visited, 0, m_Visited.Length);
        }

        this.m_TP_Current.x = start.x;
        this.m_TP_Current.y = start.y;
        m_PathIndex = 0;
        m_Path[m_PathIndex++] = this.m_TP_Current;
        m_Visited[Get1DIndex(this.m_TP_Current.x, this.m_TP_Current.y, width)] = true;
        this.m_TP_Moved = false;

        while (!(this.m_TP_Current.x == exit.x && this.m_TP_Current.y == exit.y) && m_PathIndex < m_Path.Length)
        {
            this.ResetDirections();
            this.ShuffleDirections(this.m_TP_Directions);

            if (!this.m_TP_Moved)
            {
                for (int i = 0; i < this.m_TP_Directions.Length; i++)
                {
                    m_Next.x = this.m_TP_Current.x + this.m_TP_Directions[i].x;
                    m_Next.y = this.m_TP_Current.y + this.m_TP_Directions[i].y;
                    
                    if (IsCloserToExit(this.m_TP_Current, m_Next, exit))
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            m_Next.x += this.m_TP_Directions[i].x;
                            m_Next.y += this.m_TP_Directions[i].y;
                            
                            if (IsWithinBounds(m_Next, width, height))
                            {
                                this.m_TP_Current.x = m_Next.x;
                                this.m_TP_Current.y = m_Next.y;
                                if (!m_Visited[Get1DIndex(m_Next.x, m_Next.y, width)])
                                {
                                    m_Path[m_PathIndex++] = this.m_TP_Current;
                                    m_Visited[Get1DIndex(this.m_TP_Current.x, this.m_TP_Current.y, width)] = true;
                                    this.m_TP_Moved = true;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (!this.m_TP_Moved) { break; }

            this.m_TP_Moved = false;

            for (int i = 0; i < this.m_TP_Directions.Length; i++)
            {
                m_Next.x = this.m_TP_Current.x + this.m_TP_Directions[i].x;
                m_Next.y = this.m_TP_Current.y + this.m_TP_Directions[i].y;
                
                if (IsWithinBounds(m_Next, width, height) && !m_Visited[Get1DIndex(m_Next.x, m_Next.y, width)])
                {
                    if ((NextInt(0, 1000) > twistiness && (m_Next.x - exit.x) * (m_Next.x - exit.x) + (m_Next.y - exit.y) * (m_Next.y - exit.y) > 25) || 
                        IsCloserToExit(this.m_TP_Current, m_Next, exit))
                    {
                        this.m_TP_Current.x = m_Next.x;
                        this.m_TP_Current.y = m_Next.y;
                        m_Path[m_PathIndex++] = this.m_TP_Current;
                        m_Visited[Get1DIndex(this.m_TP_Current.x, this.m_TP_Current.y, width)] = true;
                        this.m_TP_Moved = true;
                        break;
                    }
                }
            }

            if (!this.m_TP_Moved)
            {
                for (int i = 0; i < m_Directions.Length; i++)
                {
                    m_Next.x = this.m_TP_Current.x + this.m_TP_Directions[i].x;
                    m_Next.y = this.m_TP_Current.y + this.m_TP_Directions[i].y;
                    if (IsWithinBounds(m_Next, width, height) && !m_Visited[Get1DIndex(m_Next.x, m_Next.y, width)])
                    {
                        this.m_TP_Current.x = m_Next.x;
                        this.m_TP_Current.y = m_Next.y;
                        m_Path[m_PathIndex++] = this.m_TP_Current;
                        m_Visited[Get1DIndex(this.m_TP_Current.x, this.m_TP_Current.y, width)] = true;
                        this.m_TP_Moved = true;
                    }
                }
            }
        }

        Vector2Int[] trimmedPath = new Vector2Int[m_PathIndex];
        for (int i = 0; i < m_PathIndex; i++)
        {
            trimmedPath[i] = m_Path[i];
        }
        size = m_PathIndex;
        return trimmedPath;
    }

    private void ShuffleDirections(Vector2Int[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            int randomIndex = NextInt(i, directions.Length);
            Vector2Int temp = directions[i];
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }
    }

    private bool IsWithinBounds(Vector2Int cell, int width, int height)
    {
        return cell.x > 0 && cell.x < width && cell.y > 0 && cell.y < height;
    }

    private bool IsCloserToExit(Vector2Int current, Vector2Int next, Vector2Int exit)
    {
        return (next - exit).sqrMagnitude < (current - exit).sqrMagnitude;
    }

    private int Get1DIndex(int x, int y, int width)
    {
        return y * width + x;
    }

    private Vector2Int PickExit(int width, int height)
    {
        Vector2Int result = new Vector2Int(0, 0);

        // Randomly choose between four edges (0-3): 0 = top, 1 = bottom, 2 = left, 3 = right NextInt
        int randEdge = NextInt(0, 2);

        if (randEdge == 0) // Bottom edge
        {
            result.x = NextInt(0, width);
            result.y = height - 1;
        }
        else // Right edge
        {
            result.x = width - 1;
            result.y = NextInt(0, height);
        }

        return result;
    }

    private Vector2Int m_LastVisitedCell = new Vector2Int(-1, -1);

    // --------------------------------------------------------------------------------------------------- //
    // ----------------- Debug Maze Instantiation -------------------------------------------------------- //
    // --------------------------------------------------------------------------------------------------- //

    public GameObject m_Conf_Tree_1_Prefab;
    public GameObject m_DebugWallPrefab1x1;
    public GameObject m_DebugTorusMarkerPrefab1x1;
    public GameObject m_DebugAIPathNodeSpherePrefab0x25;
    private readonly Color[] colors = new Color[20]
    {
        Color.red,          // 0
        Color.green,        // 1
        Color.blue,         // 2
        Color.yellow,       // 3
        Color.cyan,         // 4
        Color.magenta,      // 5
        Color.gray,         // 6
        Color.white,        // 7
        Color.black,        // 8
        new Color(1.0f, 0.5f, 0.0f), // 9 (orange)
        new Color(0.5f, 0.0f, 0.5f), // 10 (purple)
        new Color(0.0f, 0.5f, 0.0f), // 11 (dark green)
        new Color(0.5f, 0.5f, 0.0f), // 12 (olive)
        new Color(0.0f, 0.0f, 1.0f), // 13 (dark blue)
        new Color(1.0f, 0.0f, 1.0f), // 14 (fuchsia)
        new Color(1.0f, 0.0f, 0.0f), // 15 (dark red)
        new Color(0.0f, 1.0f, 1.0f), // 16 (light cyan)
        new Color(1.0f, 1.0f, 0.0f), // 17 (light yellow)
        new Color(0.0f, 1.0f, 0.0f), // 18 (lime)
        new Color(0.5f, 0.5f, 0.5f)  // 19 (silver)
    };

    // Directions: Up, Down, Left, Right
    private readonly int[] dirX = new int[] { -1, 1, 0, 0 }; // X directions (Up, Down, Left, Right)
    private readonly int[] dirY = new int[] { 0, 0, -1, 1 }; // Y directions (Up, Down, Left, Right)
    private readonly int[] dirX_D = new int[] { -1, 1, 0, 0, -1, -1, 1, 1 }; // X directions (Up, Down, Left, Right, and Diagonals)
    private readonly int[] dirY_D = new int[] { 0, 0, -1, 1, -1, 1, -1, 1 }; // Y directions (Up, Down, Left, Right, and Diagonals)
    
    private Vector2Int[] FindLoopAroundIsland(ref int[][] island, int islandSize, ref int[][] maze, out bool foundLoop)
    {
        foundLoop = false;

        // Array to store the loop coordinates
        int maxIslandPerimeterSize = (int)Mathf.Ceil(9 * islandSize);
        Vector2Int[] loop = new Vector2Int[maxIslandPerimeterSize];
        for (int j = 0; j < maxIslandPerimeterSize; j++)
        {
            loop[j] = new Vector2Int(-999, -999); // Initialize each element with (-999, -999)
        }
        int loopIndex = 0;

        // Find the first perimeter cell (a cell in the island next to a '1' in the maze)
        Vector2Int start = new Vector2Int(-999, -999);
        for (int i = 0; i < islandSize; i++)
        {
            int x = island[i][0];
            int y = island[i][1];

            // Check if this island cell is adjacent to a '1' cell in the maze (walkable)
            for (int dir = 0; dir < 4; dir++) // Check all 4 directions
            {
                int newX = x + dirX[dir];
                int newY = y + dirY[dir];

                // Ensure the new position is within bounds
                if (newX >= 0 && newX < this.m_MazeWidth && newY >= 0 && newY < this.m_MazeHeight)
                {
                    // If the neighbor is walkable ('1'), we've found the perimeter
                    if (maze[newX][newY] >= 1)
                    {
                        start = new Vector2Int(newX, newY);
                        loop[loopIndex++] = start;
                        break;
                    }
                }
            }

            if (start.x != -999 && start.y != -999) // Exit loop if we found the start
            {
                break;
            }
        }

        // If no start was found, return an empty loop
        if (start.x == -999 && start.y == -999)
        {
            return new Vector2Int[0];
        }

        // Now trace the perimeter
        Vector2Int current = start;
        Vector2Int previous = new Vector2Int(-1, -1); // To avoid revisiting the previous cell

        while (!foundLoop && loopIndex < maxIslandPerimeterSize)
        {
            bool moved = false;

            // Try all 4 directions to trace the perimeter
            for (int i = 0; i < 4; i++)
            {
                int nextX = current.x + dirX[i];
                int nextY = current.y + dirY[i];

                // Check if the new position is part of the perimeter (maze cell == 1) and not the previous cell
                if (nextX >= 0 && nextX < this.m_MazeWidth && nextY >= 0 && nextY < this.m_MazeHeight &&
                    maze[nextX][nextY] >= 1 && (nextX != previous.x || nextY != previous.y))
                {
                    // Ensure that the new position is adjacent to an island cell
                    bool adjacentToIsland = false;
                    for (int dir = 0; dir < 8; dir++)
                    {
                        int checkX = nextX + dirX_D[dir];
                        int checkY = nextY + dirY_D[dir];

                        // Check if this new position is adjacent to an island cell
                        if (checkX >= 0 && checkX < this.m_MazeWidth && checkY >= 0 && checkY < this.m_MazeHeight &&
                            maze[checkX][checkY] <= 0) // Adjacent to island cell (water cell)
                        {
                            for (int k = 0; k < islandSize; k++)
                            {
                                int x = island[k][0];
                                int y = island[k][1];

                                if (x == checkX && y == checkY)
                                {
                                    adjacentToIsland = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (adjacentToIsland)
                    {
                        // Add this position to the loop
                        loop[loopIndex++] = current;
                        previous = current;
                        current = new Vector2Int(nextX, nextY);
                        moved = true;
                        break;
                    }
                }
            }

            // If we've looped back to the start, stop
            if (moved && current.x == start.x && current.y == start.y)
            {
                foundLoop = true;
                break;
            }

            if (!moved)
            {
                break; // Break if no movement is possible (shouldn't happen with correct input)
            }
        }

        return loop; // Return the loop if found, otherwise an empty array
    }

    private int IsPartOfAnyIsland(int i, int j, int size)
    {
        for (int islandIndex = 0; islandIndex < size; islandIndex++)
        {
            // Check each coordinate in the island
            for (int cellIndex = 0; cellIndex < this.m_DebugInstation_Sliced_Islands[islandIndex].Length; cellIndex++)
            {
                // Check if the current cell's coordinates match the (i, j) we're testing
                if (this.m_DebugInstation_Sliced_Islands[islandIndex][cellIndex][0] == i && this.m_DebugInstation_Sliced_Islands[islandIndex][cellIndex][1] == j)
                {
                    return islandIndex; // Found the coordinate in the island
                }
            }
        }

        return -1; // Not part of any island
    }

    // Function to debug print the coordinates of each island
    //private void DebugPrintIslands(int[][][] islands, int size)
    //{
    //    for (int islandIndex = 0; islandIndex < size; islandIndex++)
    //    {
    //        Debug.Log($"Island {islandIndex + 1}:");

    //        int[][] island = islands[islandIndex];

    //        // Check if the island has any cells
    //        if (island != null)
    //        {
    //            // Iterate through each cell in the island and print its coordinates
    //            for (int cellIndex = 0; cellIndex < island.Length; cellIndex++)
    //            {
    //                int x = island[cellIndex][0];
    //                int y = island[cellIndex][1];
    //                Debug.Log($"  Cell {cellIndex + 1}: ({x}, {y})");
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"Island {islandIndex + 1} is empty or null.");
    //        }
    //    }
    //}

    public void DeleteAllDebugWalls()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }


    public bool m_IsInstantiatingDebugMaze_Sliced = false;
    private int[][] m_DebugMazeInstantiation_Sliced;
    private int[][][] m_DebugInstation_Sliced_Islands;
    private int m_DebugInstantiation_Size = 0;
    private int[] m_DebugInstantiation_IslandSizes;
    private int m_MazeTheme = -1;
    private Vector2Int m_ExitPos = new Vector2Int(-1, -1);
    //public void DebugInstantiateMaze_Sliced(int theme)
    //{
    //    if (this.m_IsInstantiatingDebugMaze_Sliced)
    //    {
    //        Debug.LogError("DebugInstantiateMaze_Sliced: Already instantiating - ignored");
    //        return;
    //    }

    //    if (m_Game != null)
    //    {
    //        Game g = m_Game.GetComponent<Game>();
    //        if (g != null)
    //        {
    //            this.m_DebugMazeInstantiation_Sliced = g.m_Maze;
    //            this.m_TotalRows = -999;
    //            this.m_TotalCols = -999;
    //            this.m_ExitPos.x = g.m_MazeExit.x;
    //            this.m_ExitPos.y = g.m_MazeExit.y;

    //            this.FindIslands();

    //            this.m_MazeTheme = theme;
    //            this.m_DebugInstantiation_Counter = 0;
    //            this.m_IsInstantiatingDebugMaze_Sliced = true;
    //        }
    //    }
    //}
    public void DebugInstantiateMaze_Sliced(int theme)
    {
        if (this.m_IsInstantiatingDebugMaze_Sliced)
        {
            Debug.LogError("DebugInstantiateMaze_Sliced: Already instantiating - ignored");
            return;
        }

        if (this.m_Game != null)
        {
            this.m_TestIslandFindsOnly = false;
            this.m_DebugMazeInstantiation_Sliced = this.m_Game.m_Maze;
            this.m_TotalRows = -999;
            this.m_TotalCols = -999;
            this.m_ExitPos.x = this.m_Game.m_MazeExit.x;
            this.m_ExitPos.y = this.m_Game.m_MazeExit.y;
            this.m_MazeTheme = theme;
            int serverTime = (int)Networking.GetServerTimeInMilliseconds();
            if (serverTime == 0)
            {
                serverTime = 1; // Fallback value to avoid issues
            }
            this.state = (int)(Mathf.Round(serverTime / 5000.0f) * 5000);

            // Start finding islands
            this.FindIslands();
        }
    }

    int m_MaxDistance = 6;
    private int[][] m_Directions = new int[][] {
        new int[] { 0, 1 },  // Right
        new int[] { 1, 0 },  // Down
        new int[] { 0, -1 }, // Left
        new int[] { -1, 0 }  // Up
    };

    private int m_newI = 0;
    private int m_newJ = 0;
    private int m_curJ = 0;
    private int m_CurDistance = 1;
    private int m_CurDInner = 0;
    private int DistanceToWalkableSpace()
    {
        // Check in expanding diamond pattern (manhattan distance)
        for (this.m_CurDistance = 1; this.m_CurDistance <= this.m_MaxDistance; this.m_CurDistance++)
        {
            // Check each cell at current manhattan distance
            for (this.m_CurDInner = 0; this.m_CurDInner <= this.m_CurDistance; this.m_CurDInner++)
            {
                this.m_curJ = this.m_CurDistance - this.m_CurDInner;

                this.m_Directions[0][0] = this.m_CurDInner;
                this.m_Directions[0][1] = this.m_curJ;
                this.m_Directions[1][0] = this.m_CurDInner;
                this.m_Directions[1][1] = -this.m_curJ;
                this.m_Directions[2][0] = -this.m_CurDInner;
                this.m_Directions[2][1] = this.m_curJ;
                this.m_Directions[3][0] = -this.m_CurDInner;
                this.m_Directions[3][1] = -this.m_curJ;

                // Right direction
                this.m_newI = this.m_DISI_CurI + this.m_Directions[0][0];
                this.m_newJ = this.m_DISI_CurJ + this.m_Directions[0][1];
                if (this.m_newI >= 0 && this.m_newI < this.m_TotalRows && this.m_newJ >= 0 && this.m_newJ < this.m_TotalCols)
                {
                    if (this.m_DebugMazeInstantiation_Sliced[this.m_newI][this.m_newJ] >= 1)
                        return this.m_CurDistance - 1;
                }

                // Down direction
                this.m_newI = this.m_DISI_CurI + this.m_Directions[1][0];
                this.m_newJ = this.m_DISI_CurJ + this.m_Directions[1][1];
                if (this.m_newI >= 0 && this.m_newI < this.m_TotalRows && this.m_newJ >= 0 && this.m_newJ < this.m_TotalCols)
                {
                    if (this.m_DebugMazeInstantiation_Sliced[this.m_newI][this.m_newJ] >= 1)
                        return this.m_CurDistance - 1;
                }

                // Left direction
                this.m_newI = this.m_DISI_CurI + this.m_Directions[2][0];
                this.m_newJ = this.m_DISI_CurJ + this.m_Directions[2][1];
                if (this.m_newI >= 0 && this.m_newI < this.m_TotalRows && this.m_newJ >= 0 && this.m_newJ < this.m_TotalCols)
                {
                    if (this.m_DebugMazeInstantiation_Sliced[this.m_newI][this.m_newJ] >= 1)
                        return this.m_CurDistance - 1;
                }

                // Up direction
                this.m_newI = this.m_DISI_CurI + this.m_Directions[3][0];
                this.m_newJ = this.m_DISI_CurJ + this.m_Directions[3][1];
                if (this.m_newI >= 0 && this.m_newI < this.m_TotalRows && this.m_newJ >= 0 && this.m_newJ < this.m_TotalCols)
                {
                    if (this.m_DebugMazeInstantiation_Sliced[this.m_newI][this.m_newJ] >= 1)
                        return this.m_CurDistance - 1;
                }
            }
        }

        return -1;
    }

    public int m_DebugInstantiation_Counter = 0;
    private int m_TotalRows;
    private int m_TotalCols;

    public GameObject m_EmptyDebugPrefab;
    public GameObject m_FullBlockDebugPrefab;
    public GameObject m_ThreeSidedDebugPrefab;
    public GameObject m_CornerDebugPrefab;
    public GameObject m_OneSidedDebugPrefab;
    public GameObject m_DoubleSidedDebugPrefab;

    public GameObject m_Exit;
    public GameObject m_EmptyForestRockVariantPrefab;
    public GameObject[] m_EmptyForestPrefabs;
    public GameObject m_FullBlockForestPrefab;
    public GameObject m_FullBlockForestRockVariantPrefab;
    public GameObject m_ThreeSidedForestPrefab;
    public GameObject m_ThreeSidedForestRockVariantPrefab;
    public GameObject m_CornerForestPrefab;
    public GameObject m_CornerForestRockVariantPrefab;
    public GameObject m_OneSidedForestPrefab;
    public GameObject m_OneSidedForestRockVariantPrefab;
    public GameObject m_OneSidedForestRockVariantPrefab_B;
    public GameObject m_DoubleSidedForestPrefab;
    public GameObject m_DoubleSidedForestRockVariantPrefab;
    public GameObject m_WalkableTileForestPrefab;
    public GameObject m_WalkableTileForestRockVariantPrefab;
    public GameObject[] m_WalkableTileRockVariantRandomClutterPrefabs;

    public GameObject[] m_ForestFlowerPrefabs;

    public GameObject m_WalkableTileLogVariantPrefab;
    public GameObject m_WalkableTileLogVariantPrefab2;
    public GameObject m_WalkableTileYellowFlowerVariantPrefab;
    public GameObject m_WalkableTileWhiteFlowerVariantPrefab;
    public GameObject m_WalkableTilePinkFlowerVariantPrefab;
    public GameObject m_WalkableTileRedMushroomVariantPrefab;
    public GameObject m_WalkableTileCupMushroomVariantPrefab;
    public GameObject m_WalkableTileYellowMushroomVariantPrefab;
    public GameObject m_WalkableTileStumpVariantPrefab;

    public GameObject GetRandomFlower()
    {
        return this.m_ForestFlowerPrefabs[
            NextInt(
                0,
                this.m_ForestFlowerPrefabs.Length)
            ];
    }

    private GameObject GetTilePrefabForType(int lod)
    {
        if (this.m_DISI_curTileType == TileType.Exit)
        {
            return this.m_Exit;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 2))
        {
            return this.m_WalkableTileRockVariantRandomClutterPrefabs[
                Mathf.Clamp(NextInt(0, this.m_WalkableTileRockVariantRandomClutterPrefabs.Length), 0, this.m_WalkableTileRockVariantRandomClutterPrefabs.Length - 1)
            ];
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 3))
        {
            return this.m_WalkableTileLogVariantPrefab;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 5))
        {
            return this.m_WalkableTileYellowFlowerVariantPrefab;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 6))
        {
            return this.m_WalkableTileWhiteFlowerVariantPrefab;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 7))
        {
            return this.m_WalkableTilePinkFlowerVariantPrefab;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 8))
        {
            return this.m_WalkableTileRedMushroomVariantPrefab;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 9))
        {
            return this.m_WalkableTileCupMushroomVariantPrefab;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 10))
        {
            return this.m_WalkableTileYellowMushroomVariantPrefab;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 11))
        {
            return this.m_WalkableTileStumpVariantPrefab;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 4))
        {
            return this.m_WalkableTileLogVariantPrefab2;
        }
        else if (this.m_DISI_curIsWithinMazeBounds && (this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == -1))
        {
            switch (this.m_DISI_curTileType)
            {
                case TileType.Walkable: return this.m_WalkableTileForestPrefab;
                case TileType.FullBlock: return this.m_FullBlockForestRockVariantPrefab;
                case TileType.ThreeSided: return this.m_ThreeSidedForestRockVariantPrefab;
                case TileType.Corner: return this.m_CornerForestPrefab;
                case TileType.OneSided: return this.m_OneSidedForestPrefab;
                case TileType.DoubleSided: return this.m_DoubleSidedForestPrefab;
                case TileType.Exit: return this.m_Exit;
                default:
                    return this.m_EmptyForestPrefabs[
                        Mathf.Clamp(lod, 0, this.m_EmptyForestPrefabs.Length - 1)
                    ];
            }
        }
        else
        {
            switch (this.m_DISI_curTileType)
            {
                case TileType.Walkable: return this.m_WalkableTileForestPrefab;
                case TileType.FullBlock: return this.m_FullBlockForestPrefab;
                case TileType.ThreeSided: return this.m_ThreeSidedForestPrefab;
                case TileType.Corner: return this.m_CornerForestPrefab;
                case TileType.OneSided: return this.m_OneSidedForestPrefab;
                case TileType.DoubleSided: return this.m_DoubleSidedForestPrefab;
                case TileType.Exit: return this.m_Exit;
                default:
                    return this.m_EmptyForestPrefabs[
                        Mathf.Clamp(lod, 0, this.m_EmptyForestPrefabs.Length - 1)
                    ];
            }
        }
    }

    bool isWithinMazeBounds = false;
    bool north = false;
    bool south = false;
    bool west = false;
    bool east = false;

    private TileType GetTileType(out float rotation)//, out string cardinal)
    {
        isWithinMazeBounds = (this.m_DISI_CurI-1 >= 0 && this.m_DISI_CurI-1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
        north = isWithinMazeBounds && (this.m_DISI_CurI > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI-1][this.m_DISI_CurJ] >= 1);
        
        isWithinMazeBounds = (this.m_DISI_CurI+1 >= 0 && this.m_DISI_CurI+1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
        south = isWithinMazeBounds && (this.m_DISI_CurI < this.m_DebugMazeInstantiation_Sliced.Length-1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI+1][this.m_DISI_CurJ] >= 1);

        isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ-1 >= 0 && this.m_DISI_CurJ-1 < this.m_MazeHeight);
        west = isWithinMazeBounds && (this.m_DISI_CurJ > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ-1] >= 1);

        isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ+1 >= 0 && this.m_DISI_CurJ+1 < this.m_MazeHeight);
        east = isWithinMazeBounds && (this.m_DISI_CurJ < this.m_DebugMazeInstantiation_Sliced[0].Length-1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ+1] >= 1);

        // Hardcode the tile types and rotations based on adjacent cells
        if (north && east && south && west)
        {
            //cardinal = "NESW";
            rotation = 0f;
            return TileType.FullBlock;
        }
        if (north && east && south)
        {
           // cardinal = "NES";
            rotation = 180.0f;
            return TileType.ThreeSided;
        }
        if (north && east && west)
        {
           // cardinal = "NEW";
            rotation = 270f;
            return TileType.ThreeSided;
        }
        if (north && south && west)
        {
           // cardinal = "NSW";
            rotation = 0.0f;
            return TileType.ThreeSided;
        }
        if (east && south && west)
        {
           // cardinal = "ESW";
            rotation = 90f;
            return TileType.ThreeSided;
        }
        if (north && east)
        {
            //cardinal = "NE";
            rotation = 270.0f;
            return TileType.Corner;
        }
        if (north && south)
        {
            //cardinal = "NS";
            rotation = 0f;
            return TileType.DoubleSided;
        }
        if (north && west)
        {
           // cardinal = "NW";
            rotation = 0.0f;
            return TileType.Corner;
        }
        if (east && south)
        {
           // cardinal = "ES";
            rotation = 180.0f;
            return TileType.Corner;
        }
        if (east && west)
        {
           // cardinal = "EW";
            rotation = 90f;
            return TileType.DoubleSided;
        }
        if (south && west)
        {
          //  cardinal = "SW";
            rotation = 90.0f;
            return TileType.Corner;
        }
        if (north)
        {
          //  cardinal = "N";
            rotation = 180.0f;
            return TileType.OneSided;
        }
        if (east)
        {
          //  cardinal = "E";
            rotation = 90f;
            return TileType.OneSided;
        }
        if (south)
        {
          //  cardinal = "S";
            rotation = 0.0f;
            return TileType.OneSided;
        }
        if (west)
        {
           // cardinal = "W";
            rotation = 270f;
            return TileType.OneSided;
        }

        //cardinal = "";
        rotation = 0f;
        return TileType.Empty;
    }

    private GameObject m_TileRef;
    private GameObject m_ExitTileRef;
    Vector3 m_ColliderSizeRef =  new Vector3(1, 1, 1);
    BoxCollider m_BoxColliderRef;
    CapsuleCollider m_CapsuleColliderRef;
    public float m_OptimisationDelayKek = 0.75f;

    private int m_DISI_CurI = 0;
    private int m_DISI_CurJ = 0;
    private int m_DISI_totalWidthExtended = 0;
    private int m_DISI_totalHeightExtended = 0;
    private bool m_DISI_curIsWithinMazeBounds = false;
    private bool m_DISI_curIsOnEntranceArea = false;
    private string m_DISI_curCardinal = "";
    private float m_DISI_curRotation = 0.0f;
    private int m_DISI_curDistance = 0;
    private GameObject m_DISI_TreeChild;
    private float m_DISI_randomTreeScale;
    private Vector3 m_DISI_newPosition;
    private float m_DISI_offsetX;
    private int m_PiecesSinceLastTile = 0;
    private float m_RotationOffset = 0.0f;
    private TileType m_DISI_curTileType = TileType.Empty;
    public void DebugInstantiateMaze_Slice_Iteration()
    {
        if (m_TotalRows == -999) m_TotalRows = m_DebugMazeInstantiation_Sliced.Length;
        if (m_TotalCols == -999) m_TotalCols = m_DebugMazeInstantiation_Sliced[0].Length;

        this.m_DISI_totalWidthExtended = m_TotalRows + 12;  // Total width including borders
        this.m_DISI_totalHeightExtended = m_TotalCols + 12; // Total height including borders


        // Calculate i and j using the full range width/height
        this.m_DISI_CurI = (m_DebugInstantiation_Counter / this.m_DISI_totalWidthExtended) - 6;
        this.m_DISI_CurJ = (m_DebugInstantiation_Counter % this.m_DISI_totalHeightExtended) - 6;

        // Check if we've reached the end of the extended area
        if (this.m_DISI_CurI >= m_TotalRows + 6 || this.m_PiecesSinceLastTile > this.m_DISI_totalWidthExtended * 2)
        {
            this.m_IsInstantiatingDebugMaze_Sliced = false;
            //Debug.Log("DebugInstantiateMaze_Slice_Iteration: done");
            SendCustomEventDelayedSeconds(
                nameof(this.DebugInstantiateMaze_Slice_Loops),
                this.m_OptimisationDelayKek
            );

            return;
        }

        SendCustomEventDelayedSeconds(
            nameof(this.DebugInstantiateMaze_Slice_Iteration),
            this.m_Game.m_FPS == 0 ? 0.075f : Mathf.Clamp((1.0f / this.m_Game.m_FPS) / 10.0f, 0.0001f, 0.075f)
        );

        this.m_DISI_curIsWithinMazeBounds = (
            this.m_DISI_CurI >= 0 &&
            this.m_DISI_CurI < this.m_MazeWidth &&
            this.m_DISI_CurJ >= 0 &&
            this.m_DISI_CurJ < this.m_MazeHeight
        );


        this.m_DISI_curIsOnEntranceArea = (
            (this.m_DISI_CurI < 1 && this.m_DISI_CurJ < 6) ||
            (this.m_DISI_CurI == -1 && this.m_DISI_CurJ == 0) ||
            (this.m_DISI_CurI == 0 && this.m_DISI_CurJ == -1) ||
            (this.m_DISI_CurI == 1 && this.m_DISI_CurJ == -1)
        );

        if ((!this.m_DISI_curIsWithinMazeBounds || this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] <= 0) && !this.m_DISI_curIsOnEntranceArea)
        {
            this.m_DISI_curDistance = DistanceToWalkableSpace();
            if (this.m_DISI_curDistance < 7 && this.m_DISI_curDistance != -1)
            {
                this.m_DISI_offsetX = this.m_ColliderSizeRef.x / 2;
                this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
                this.m_DISI_newPosition.y = 0;
                this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
                // Create tile at origin first
                m_DISI_curTileType = this.GetTileType(
                    out this.m_DISI_curRotation
                    //out this.m_DISI_curCardinal
                );
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(this.m_DISI_curDistance),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_DISI_curRotation, 0),
                    this.transform
                );

                // get the first child of the tree
                if (this.m_TileRef.transform.childCount > 0)
                {
                    this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(0).gameObject;
                    // add a random scale to the tree between 0.75 and 1
                    this.m_DISI_randomTreeScale = NextFloat(0.2f, 0.25f);
                    this.m_DISI_TreeChild.transform.localScale.Set(
                        this.m_DISI_randomTreeScale,
                        this.m_DISI_randomTreeScale,
                        this.m_DISI_randomTreeScale
                    );
                    // add a random rotation in the y axis
                    this.m_DISI_TreeChild.transform.Rotate(0, NextInt(0, 360), 0);
                    // add a random offset
                    if (this.m_DISI_curDistance > 0)
                    {
                        this.m_DISI_TreeChild.transform.position.Set(
                            this.m_DISI_TreeChild.transform.position.x + NextFloat(-0.5f, 0.5f),
                            this.m_DISI_TreeChild.transform.position.y,
                            this.m_DISI_TreeChild.transform.position.z + NextFloat(-0.5f, 0.5f)
                        );
                    }
                    // add a tiny random y offset
                    this.m_DISI_TreeChild.transform.position.Set(
                        this.m_DISI_TreeChild.transform.position.x,
                        this.m_DISI_TreeChild.transform.position.y + NextFloat(-0.3f, 0.0f),
                        this.m_DISI_TreeChild.transform.position.z
                    );
                }
                //get the third child
                if (this.m_TileRef.transform.childCount > 2)
                {
                    this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(2).gameObject;
                    // add a random rotation in the y axis
                    this.m_DISI_TreeChild.transform.Rotate(0, NextInt(0, 360), 0);
                }

                this.m_PiecesSinceLastTile = 0;
            }
            else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
            {
                // Not instantiating a piece is worth 10% cpu time
                this.m_PiecesSinceLastTile++;
            }
        }
        else if (this.m_DISI_curIsWithinMazeBounds && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] >= 1)
        {
            //Vector3 position = new Vector3(j, 0, i);

            // first check if exit
            m_DISI_curTileType = TileType.Walkable;
            this.m_RotationOffset = 0.0f;
            this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
            this.m_DISI_newPosition.y = 0;
            this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
            if (this.m_DISI_CurI == m_ExitPos.x && this.m_DISI_CurJ == m_ExitPos.y)
            {

                isWithinMazeBounds = (this.m_DISI_CurI - 1 >= 0 && this.m_DISI_CurI - 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                north = isWithinMazeBounds && (this.m_DISI_CurI > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI - 1][this.m_DISI_CurJ] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI + 1 >= 0 && this.m_DISI_CurI + 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                south = isWithinMazeBounds && (this.m_DISI_CurI < this.m_DebugMazeInstantiation_Sliced.Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI + 1][this.m_DISI_CurJ] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ - 1 >= 0 && this.m_DISI_CurJ - 1 < this.m_MazeHeight);
                west = isWithinMazeBounds && (this.m_DISI_CurJ > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ - 1] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ + 1 >= 0 && this.m_DISI_CurJ + 1 < this.m_MazeHeight);
                east = isWithinMazeBounds && (this.m_DISI_CurJ < this.m_DebugMazeInstantiation_Sliced[0].Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ + 1] >= 1);

                // now do rotatin
                if (north) this.m_RotationOffset = 90.0f;
                else if (east) this.m_RotationOffset = 0.0f;
                else if (south) this.m_RotationOffset = 270.0f;
                else if (west) this.m_RotationOffset = -180.0f;
                else this.m_RotationOffset = 180.0f;

                m_DISI_curTileType = TileType.Exit;

                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_RotationOffset, 0),
                    this.transform
                );

                this.m_ExitTileRef = this.m_TileRef;
            }
            else
            {
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_RotationOffset, 0),
                    this.transform
                );
            }

            // if (m_DISI_curTileType == TileType.Exit)
            // {
            //     if (north) this.m_TileRef.name = "Exit_North";
            //     else if (east) this.m_TileRef.name = "Exit_East";
            //     else if (south) this.m_TileRef.name = "Exit_South";
            //     else if (west) this.m_TileRef.name = "Exit_West";
            //     else this.m_TileRef.name = "Exit_????";
            // }

            //this.m_TileRef.name = $"Walkable_Debug_{i}_{j}_empty_walkable";
            this.m_TileRef.transform.parent = transform;

            this.m_PiecesSinceLastTile = 0;
        }
        else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
        {
            // Not instantiating a piece is worth 10% cpu time
            this.m_PiecesSinceLastTile++;
        }

        m_DebugInstantiation_Counter++;

        // Calculate i and j using the full range width/height
        this.m_DISI_CurI = (m_DebugInstantiation_Counter / this.m_DISI_totalWidthExtended) - 6;
        this.m_DISI_CurJ = (m_DebugInstantiation_Counter % this.m_DISI_totalHeightExtended) - 6;

        // Check if we've reached the end of the extended area
        if (this.m_DISI_CurI >= m_TotalRows + 6 || this.m_PiecesSinceLastTile > this.m_DISI_totalWidthExtended * 2)
        {
            this.m_IsInstantiatingDebugMaze_Sliced = false;
            //Debug.Log("DebugInstantiateMaze_Slice_Iteration: done");
            SendCustomEventDelayedSeconds(
                nameof(this.DebugInstantiateMaze_Slice_Loops),
                this.m_OptimisationDelayKek
            );

            return;
        }

        this.m_DISI_curIsWithinMazeBounds = (
            this.m_DISI_CurI >= 0 &&
            this.m_DISI_CurI < this.m_MazeWidth &&
            this.m_DISI_CurJ >= 0 &&
            this.m_DISI_CurJ < this.m_MazeHeight
        );



        this.m_DISI_curIsOnEntranceArea = (
            (this.m_DISI_CurI < 1 && this.m_DISI_CurJ < 6) ||
            (this.m_DISI_CurI == -1 && this.m_DISI_CurJ == 0) ||
            (this.m_DISI_CurI == 0 && this.m_DISI_CurJ == -1) ||
            (this.m_DISI_CurI == 1 && this.m_DISI_CurJ == -1)
        );

        if ((!this.m_DISI_curIsWithinMazeBounds || this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] <= 0) && !this.m_DISI_curIsOnEntranceArea)
        {
            this.m_DISI_curDistance = DistanceToWalkableSpace();
            if (this.m_DISI_curDistance < 7 && this.m_DISI_curDistance != -1)
            {
                this.m_DISI_offsetX = this.m_ColliderSizeRef.x / 2;
                this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
                this.m_DISI_newPosition.y = 0;
                this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
                // Create tile at origin first
                m_DISI_curTileType = this.GetTileType(
                    out this.m_DISI_curRotation
                    //out this.m_DISI_curCardinal
                );
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(this.m_DISI_curDistance),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_DISI_curRotation, 0),
                    this.transform
                );

                // get the first child of the tree
                if (this.m_TileRef.transform.childCount > 0)
                {
                    this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(0).gameObject;
                    // add a random scale to the tree between 0.75 and 1
                    this.m_DISI_randomTreeScale = NextFloat(0.2f, 0.25f);
                    this.m_DISI_TreeChild.transform.localScale.Set(
                        this.m_DISI_randomTreeScale,
                        this.m_DISI_randomTreeScale,
                        this.m_DISI_randomTreeScale
                    );
                    // add a random rotation in the y axis
                    this.m_DISI_TreeChild.transform.Rotate(0, NextInt(0, 360), 0);
                    // add a random offset
                    if (this.m_DISI_curDistance > 0)
                    {
                        this.m_DISI_TreeChild.transform.position.Set(
                            this.m_DISI_TreeChild.transform.position.x + NextFloat(-0.5f, 0.5f),
                            this.m_DISI_TreeChild.transform.position.y,
                            this.m_DISI_TreeChild.transform.position.z + NextFloat(-0.5f, 0.5f)
                        );
                    }
                    // add a tiny random y offset
                    this.m_DISI_TreeChild.transform.position.Set(
                        this.m_DISI_TreeChild.transform.position.x,
                        this.m_DISI_TreeChild.transform.position.y + NextFloat(-0.3f, 0.0f),
                        this.m_DISI_TreeChild.transform.position.z
                    );
                }

                this.m_PiecesSinceLastTile = 0;
            }
            else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
            {
                // Not instantiating a piece is worth 10% cpu time
                this.m_PiecesSinceLastTile++;
            }
        }
        else if (this.m_DISI_curIsWithinMazeBounds && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] >= 1)
        {
            // first check if exit
            m_DISI_curTileType = TileType.Walkable;
            this.m_RotationOffset = 0.0f;
            this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
            this.m_DISI_newPosition.y = 0;
            this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;

            if (this.m_DISI_CurI == m_ExitPos.x && this.m_DISI_CurJ == m_ExitPos.y)
            {

                isWithinMazeBounds = (this.m_DISI_CurI - 1 >= 0 && this.m_DISI_CurI - 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                north = isWithinMazeBounds && (this.m_DISI_CurI > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI - 1][this.m_DISI_CurJ] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI + 1 >= 0 && this.m_DISI_CurI + 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                south = isWithinMazeBounds && (this.m_DISI_CurI < this.m_DebugMazeInstantiation_Sliced.Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI + 1][this.m_DISI_CurJ] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ - 1 >= 0 && this.m_DISI_CurJ - 1 < this.m_MazeHeight);
                west = isWithinMazeBounds && (this.m_DISI_CurJ > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ - 1] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ + 1 >= 0 && this.m_DISI_CurJ + 1 < this.m_MazeHeight);
                east = isWithinMazeBounds && (this.m_DISI_CurJ < this.m_DebugMazeInstantiation_Sliced[0].Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ + 1] >= 1);

                // now do rotatin
                if (north) this.m_RotationOffset = 90.0f;
                else if (east) this.m_RotationOffset = 0.0f;
                else if (south) this.m_RotationOffset = 270.0f;
                else if (west) this.m_RotationOffset = -180.0f;
                else this.m_RotationOffset = 180.0f;

                m_DISI_curTileType = TileType.Exit;

                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_RotationOffset, 0),
                    this.transform
                );

                this.m_ExitTileRef = this.m_TileRef;
            }
            else
            {
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_RotationOffset, 0),
                    this.transform
                );
            }

            // if (m_DISI_curTileType == TileType.Exit)
            // {
            //     if (north) this.m_TileRef.name = "Exit_North";
            //     else if (east) this.m_TileRef.name = "Exit_East";
            //     else if (south) this.m_TileRef.name = "Exit_South";
            //     else if (west) this.m_TileRef.name = "Exit_West";
            //     else this.m_TileRef.name = "Exit_????";
            // }

            this.m_PiecesSinceLastTile = 0;
        }
        else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
        {
            // Not instantiating a piece is worth 10% cpu time
            this.m_PiecesSinceLastTile++;
        }

        m_DebugInstantiation_Counter++;

        // Calculate i and j using the full range width/height
        this.m_DISI_CurI = (m_DebugInstantiation_Counter / this.m_DISI_totalWidthExtended) - 6;
        this.m_DISI_CurJ = (m_DebugInstantiation_Counter % this.m_DISI_totalHeightExtended) - 6;

        // Check if we've reached the end of the extended area
        if (this.m_DISI_CurI >= m_TotalRows + 6 || this.m_PiecesSinceLastTile > this.m_DISI_totalWidthExtended * 2)
        {
            this.m_IsInstantiatingDebugMaze_Sliced = false;

            SendCustomEventDelayedSeconds(
                nameof(this.DebugInstantiateMaze_Slice_Loops),
                this.m_OptimisationDelayKek
            );

            return;
        }

        this.m_DISI_curIsWithinMazeBounds = (
            this.m_DISI_CurI >= 0 &&
            this.m_DISI_CurI < this.m_MazeWidth &&
            this.m_DISI_CurJ >= 0 &&
            this.m_DISI_CurJ < this.m_MazeHeight
        );

        this.m_DISI_curIsOnEntranceArea = (
            (this.m_DISI_CurI < 1 && this.m_DISI_CurJ < 6) ||
            (this.m_DISI_CurI == -1 && this.m_DISI_CurJ == 0) ||
            (this.m_DISI_CurI == 0 && this.m_DISI_CurJ == -1) ||
            (this.m_DISI_CurI == 1 && this.m_DISI_CurJ == -1)
        );

        if ((!this.m_DISI_curIsWithinMazeBounds || this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] <= 0) && !this.m_DISI_curIsOnEntranceArea)
        {
            this.m_DISI_curDistance = DistanceToWalkableSpace();
            if (this.m_DISI_curDistance < 7 && this.m_DISI_curDistance != -1)
            {
                this.m_DISI_offsetX = this.m_ColliderSizeRef.x / 2;
                this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
                this.m_DISI_newPosition.y = 0;
                this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
                // Create tile at origin first
                m_DISI_curTileType = this.GetTileType(
                    out this.m_DISI_curRotation
                    //out this.m_DISI_curCardinal
                );
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(this.m_DISI_curDistance),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_DISI_curRotation, 0),
                    this.transform
                );

                // get the first child of the tree
                if (this.m_TileRef.transform.childCount > 0)
                {
                    this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(0).gameObject;
                    // add a random scale to the tree between 0.75 and 1
                    this.m_DISI_randomTreeScale = NextFloat(0.2f, 0.25f);
                    this.m_DISI_TreeChild.transform.localScale.Set(
                        this.m_DISI_randomTreeScale,
                        this.m_DISI_randomTreeScale,
                        this.m_DISI_randomTreeScale
                    );
                    // add a random rotation in the y axis
                    this.m_DISI_TreeChild.transform.Rotate(0, NextInt(0, 360), 0);
                    // add a random offset
                    if (this.m_DISI_curDistance > 0)
                    {
                        this.m_DISI_TreeChild.transform.position.Set(
                            this.m_DISI_TreeChild.transform.position.x + NextFloat(-0.5f, 0.5f),
                            this.m_DISI_TreeChild.transform.position.y,
                            this.m_DISI_TreeChild.transform.position.z + NextFloat(-0.5f, 0.5f)
                        );
                    }
                    // add a tiny random y offset
                    this.m_DISI_TreeChild.transform.position.Set(
                        this.m_DISI_TreeChild.transform.position.x,
                        this.m_DISI_TreeChild.transform.position.y + NextFloat(-0.3f, 0.0f),
                        this.m_DISI_TreeChild.transform.position.z
                    );
                }
                //get the third child
                if (this.m_TileRef.transform.childCount > 2)
                {
                    this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(2).gameObject;
                    // add a random rotation in the y axis
                    this.m_DISI_TreeChild.transform.Rotate(0, NextInt(0, 360), 0);
                }

                this.m_PiecesSinceLastTile = 0;
            }
            else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
            {
                // Not instantiating a piece is worth 10% cpu time
                this.m_PiecesSinceLastTile++;
            }
        }
        else if (this.m_DISI_curIsWithinMazeBounds && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] >= 1)
        {
            //Vector3 position = new Vector3(j, 0, i);

            // first check if exit
            m_DISI_curTileType = TileType.Walkable;
            this.m_RotationOffset = 0.0f;
            this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
            this.m_DISI_newPosition.y = 0;
            this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
            if (this.m_DISI_CurI == m_ExitPos.x && this.m_DISI_CurJ == m_ExitPos.y)
            {

                isWithinMazeBounds = (this.m_DISI_CurI - 1 >= 0 && this.m_DISI_CurI - 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                north = isWithinMazeBounds && (this.m_DISI_CurI > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI - 1][this.m_DISI_CurJ] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI + 1 >= 0 && this.m_DISI_CurI + 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                south = isWithinMazeBounds && (this.m_DISI_CurI < this.m_DebugMazeInstantiation_Sliced.Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI + 1][this.m_DISI_CurJ] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ - 1 >= 0 && this.m_DISI_CurJ - 1 < this.m_MazeHeight);
                west = isWithinMazeBounds && (this.m_DISI_CurJ > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ - 1] >= 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ + 1 >= 0 && this.m_DISI_CurJ + 1 < this.m_MazeHeight);
                east = isWithinMazeBounds && (this.m_DISI_CurJ < this.m_DebugMazeInstantiation_Sliced[0].Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ + 1] >= 1);

                // now do rotatin
                if (north) this.m_RotationOffset = 90.0f;
                else if (east) this.m_RotationOffset = 0.0f;
                else if (south) this.m_RotationOffset = 270.0f;
                else if (west) this.m_RotationOffset = -180.0f;
                else this.m_RotationOffset = 180.0f;

                m_DISI_curTileType = TileType.Exit;

                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_RotationOffset, 0),
                    this.transform
                );

                this.m_ExitTileRef = this.m_TileRef;
            }
            else
            {
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_RotationOffset, 0),
                    this.transform
                );
            }

            // if (m_DISI_curTileType == TileType.Exit)
            // {
            //     if (north) this.m_TileRef.name = "Exit_North";
            //     else if (east) this.m_TileRef.name = "Exit_East";
            //     else if (south) this.m_TileRef.name = "Exit_South";
            //     else if (west) this.m_TileRef.name = "Exit_West";
            //     else this.m_TileRef.name = "Exit_????";
            // }

            //this.m_TileRef.name = $"Walkable_Debug_{i}_{j}_empty_walkable";
            this.m_TileRef.transform.parent = transform;

            this.m_PiecesSinceLastTile = 0;
        }
        else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
        {
            // Not instantiating a piece is worth 10% cpu time
            this.m_PiecesSinceLastTile++;
        }

        m_DebugInstantiation_Counter++;
    }

    private Vector2Int[][] m_Loops;
    private bool m_IsProcessingIslandsAlready = false;
    public void DebugInstantiateMaze_Slice_Loops()
    {
        if (this.m_IsProcessingIslandsAlready) return;
        this.m_IsProcessingIslandsAlready = true;

        Debug.Log("DebugInstantiateMaze_Slice_Loops: started");
        // Initialize the loops array with Vector2Int(-999, -999)
        this.m_Loops = new Vector2Int[this.m_DebugInstantiation_Size][];

        for (int i = 0; i < this.m_DebugInstantiation_Size; i++)
        {
            this.m_Loops[i] = new Vector2Int[this.m_DebugInstantiation_IslandSizes[i]];
            for (int j = 0; j < this.m_DebugInstantiation_IslandSizes[i]; j++)
            {
                this.m_Loops[i][j] = new Vector2Int(-999, -999);
            }
        }

        // Start processing islands one by one
        m_CurrentLoopIslandIndex = 0;
        this.m_TotalEnemies = 0;
        SendCustomEventDelayedSeconds(nameof(ProcessIslandLoop), 0.1f);
    }

    private int m_CurrentLoopIslandIndex = 0;

    public void ProcessIslandLoop()
    {
        if (m_CurrentLoopIslandIndex >= this.m_DebugInstantiation_Size)
        {
            // All islands processed
            if (this.m_MazeTheme == 0)
            {
                DebugPrintLoops(this.m_Loops, colors);
            }

            SendCustomEventDelayedSeconds(
                nameof(this.DebugInstantiateMaze_Slice_AI),
                this.m_OptimisationDelayKek
            );
            return;
        }

        int i = m_CurrentLoopIslandIndex;

        if (this.m_DebugInstantiation_IslandSizes[i] < 80)
        {
            Vector2Int[] newLoop = FindLoopAroundIsland(
                ref this.m_DebugInstation_Sliced_Islands[i],
                this.m_DebugInstantiation_IslandSizes[i],
                ref this.m_DebugMazeInstantiation_Sliced,
                out bool foundLoop
            );

            if (foundLoop)
            {
                this.m_Loops[i] = newLoop;
            }
            else
            {
                this.m_Loops[i] = new Vector2Int[0]; // No loop found
            }
        }
        else
        {
            this.m_Loops[i] = new Vector2Int[0]; // Island too large, no loop
        }

        m_CurrentLoopIslandIndex++;

        // Schedule the next island processing
        SendCustomEventDelayedSeconds(nameof(ProcessIslandLoop), 0.1f);
    }
    //public void DebugInstantiateMAze_Slice_Loops()
    //{
    //    //Initialize the loops array with Vector2Int(-999, -999)
    //    this.m_Loops = new Vector2Int[this.m_DebugInstantiation_Size][];

    //    for (int i = 0; i < this.m_DebugInstantiation_Size; i++)
    //    {
    //        this.m_Loops[i] = new Vector2Int[this.m_DebugInstantiation_IslandSizes[i]]; // Assuming islandSizes is defined and holds the size of each island
    //        for (int j = 0; j < this.m_DebugInstantiation_IslandSizes[i]; j++)
    //        {
    //            this.m_Loops[i][j] = new Vector2Int(-999, -999); // Initialize each element with (-999, -999)
    //        }
    //    }

    //    // Find loops for islands with size less than 50
    //    for (int i = 0; i < this.m_DebugInstantiation_Size; i++)
    //    {
    //        if (this.m_DebugInstantiation_IslandSizes[i] < 80)
    //        {
    //            Vector2Int[] newLoop = FindLoopAroundIsland(ref this.m_DebugInstation_Sliced_Islands[i], this.m_DebugInstantiation_IslandSizes[i], ref this.m_DebugMazeInstantiation_Sliced, out bool foundLoop);
    //            if (foundLoop)
    //            {
    //                this.m_Loops[i] = newLoop;
    //            }
    //            else
    //            {
    //                this.m_Loops[i] = new Vector2Int[0]; // No loop if island is too large
    //            }
    //        }
    //        else
    //        {
    //            this.m_Loops[i] = new Vector2Int[0]; // No loop if island is too large
    //        }
    //    }

    //    if(this.m_MazeTheme == 0)
    //    {
    //        DebugPrintLoops(this.m_Loops, colors);
    //    }

    //    SendCustomEventDelayedSeconds(
    //        nameof(this.DebugInstantiateMaze_Slice_AI),
    //        this.m_OptimisationDelayKek
    //    );
    //}

    private void DebugPrintLoops(Vector2Int[][] loops, Color[] colors)
    {
        // Iterate through each island loop
        for (int islandIndex = 0; islandIndex < loops.Length; islandIndex++)
        {
            // Check if the loop is not empty
            if (loops[islandIndex].Length > 0)
            {
                // Loop through each coordinate in the island's loop
                for (int i = 0; i < loops[islandIndex].Length; i++)
                {
                    Vector2Int loopCoordinate = loops[islandIndex][i];
                    if (loopCoordinate.x == -999 && loopCoordinate.y == -999) { continue; }

                    // Instantiate a sphere at the loop's position
                    Vector3 position = new Vector3(loopCoordinate.y, 0, loopCoordinate.x);
                    GameObject pathNode = UnityEngine.Object.Instantiate(m_DebugTorusMarkerPrefab1x1, position, Quaternion.identity);

                    // Name the path node for debugging purposes
                    pathNode.name = $"LoopNode_{loopCoordinate.x}_{loopCoordinate.y}";

                    // Set the color based on the island's index
                    pathNode.GetComponent<Renderer>().material.color = colors[islandIndex % (colors.Length - 1)];

                    // Set the parent to organize in the scene hierarchy
                    pathNode.transform.parent = transform;
                }
            }
        }
    }

    public GameObject m_DebugAICylinderPrefab1x1;

    public int m_TotalEnemies = 0;
    public void DebugInstantiateMaze_Slice_AI()
    {
        // Array to store the indexes of valid loops
        int[] validLoopIndexes = new int[this.m_Loops.Length];
        int validLoopCount = 0;

        // Filter valid loops that are away from spawn
        for (int islandIndex = 0; islandIndex < this.m_Loops.Length; islandIndex++)
        {
            if (this.m_Loops[islandIndex].Length > 0)
            {
                bool isAwayFromSpawn = true;
                foreach (Vector2Int point in this.m_Loops[islandIndex])
                {
                    if (point.x == -999 && point.y == -999) continue;

                    float distance = Mathf.Sqrt(
                        Mathf.Pow(point.x - 3, 2) +
                        Mathf.Pow(point.y - 3, 2)
                    );

                    if (distance <= 11.0f)
                    {
                        isAwayFromSpawn = false;
                        break;
                    }
                }

                if (isAwayFromSpawn)
                {
                    validLoopIndexes[validLoopCount++] = islandIndex; // Store the index of the valid loop
                }
            }
        }

        // If there are more valid loops than maxEnemies, spread them out using Manhattan distance
        int maxEnemies = this.m_Game.m_EasyModeOn ? 5 : 15;
        int[] selectedLoopIndexes = new int[maxEnemies];
        int selectedLoopCount = 0;

        if (validLoopCount > maxEnemies)
        {
            // Select loops that are farthest apart
            selectedLoopIndexes[selectedLoopCount++] = validLoopIndexes[0]; // Start with the first loop
            while (selectedLoopCount < maxEnemies)
            {
                int farthestLoopIndex = -1;
                float maxDistance = float.MinValue;

                for (int i = 0; i < validLoopCount; i++)
                {
                    bool alreadySelected = false;
                    for (int j = 0; j < selectedLoopCount; j++)
                    {
                        if (validLoopIndexes[i] == selectedLoopIndexes[j])
                        {
                            alreadySelected = true;
                            break;
                        }
                    }
                    if (alreadySelected) continue;

                    float minDistanceToSelected = float.MaxValue;
                    for (int j = 0; j < selectedLoopCount; j++)
                    {
                        Vector2Int loopA = this.m_Loops[validLoopIndexes[i]][0];
                        Vector2Int loopB = this.m_Loops[selectedLoopIndexes[j]][0];

                        float manhattanDistance = Mathf.Abs(loopA.x - loopB.x) + Mathf.Abs(loopA.y - loopB.y);
                        if (manhattanDistance < minDistanceToSelected)
                        {
                            minDistanceToSelected = manhattanDistance;
                        }
                    }

                    if (minDistanceToSelected > maxDistance)
                    {
                        maxDistance = minDistanceToSelected;
                        farthestLoopIndex = validLoopIndexes[i];
                    }
                }

                if (farthestLoopIndex != -1)
                {
                    selectedLoopIndexes[selectedLoopCount++] = farthestLoopIndex;
                }
            }
        }
        else
        {
            // Use all valid loops if they are fewer than maxEnemies
            for (int i = 0; i < validLoopCount; i++)
            {
                selectedLoopIndexes[selectedLoopCount++] = validLoopIndexes[i];
            }
        }

        // Instantiate enemies at the selected loops
        for (int i = 0; i < selectedLoopCount; i++)
        {
            int loopIndex = selectedLoopIndexes[i];
            Vector2Int loopCoordinate = this.m_Loops[loopIndex][0];

            if (!this.m_TestIslandFindsOnly)
            {
                EnemyAI_Animated.InstantiateAI(
                    this.m_DebugAICylinderPrefab1x1,
                    transform,
                    this.m_Loops[loopIndex],
                    colors[loopIndex % (colors.Length - 1)],
                    loopIndex,
                    this.m_Game
                );
                this.m_TotalEnemies++;
            }
            else
            {
                this.m_TotalEnemies++;
            }
        }

        SendCustomEventDelayedSeconds(
            nameof(this.Done),
            this.m_OptimisationDelayKek
        );
    }

    public void Done()
    {
        if (this.m_TestIslandFindsOnly)
        {
            SendCustomEventDelayedSeconds(
                nameof(this.OnDoLoopsTestComplete),
                0.1f
            );

            return;
        }
        
        if (m_Game != null)
        {
            this.m_Game.On_LocalPlayer_MazeGenerated(ref this.m_ExitTileRef);
        }
    }

    public void ResetAllStateVariables()
    {
        m_CurrentI = 0;
        m_CurrentJ = 0;
        m_CurrentIsland = 0;
        m_MaxIslands = 0;
        m_MaxX = 0;
        m_MaxY = 0;
        m_MaxIslandSize = 0;
        m_StackPointer = 0;
        m_IsFindingIslands = false;
        m_Path = null;
        m_Visited = null;
        m_TP_Current = new Vector2Int();
        m_Next = new Vector2Int();
        m_PathIndex = 0;
        m_TP_Moved = false;
        m_LastVisitedCell = new Vector2Int();
        m_IsInstantiatingDebugMaze_Sliced = false;
        m_DebugMazeInstantiation_Sliced = null;
        m_DebugInstation_Sliced_Islands = null;
        m_DebugInstantiation_Size = 0;
        m_DebugInstantiation_IslandSizes = null;
        m_MazeTheme = 0;
        m_ExitPos = new Vector2Int();
        m_newI = 0;
        m_newJ = 0;
        m_curJ = 0;
        m_CurDistance = 1;
        m_CurDInner = 0;
        m_DebugInstantiation_Counter = 0;
        m_TotalRows = -999;
        m_TotalCols = -999;
        isWithinMazeBounds = false;
        north = false;
        south = false;
        west = false;
        east = false;
        // Uncommenting these causes minor bugs
        //m_TileRef = null;
        //m_ExitTileRef = null;
        //m_ColliderSizeRef = Vector3.zero;
        //m_BoxColliderRef = null;
        //m_CapsuleColliderRef = null;
        m_DISI_CurI = 0;
        m_DISI_CurJ = 0;
        m_DISI_totalWidthExtended = 0;
        m_DISI_totalHeightExtended = 0;
        m_DISI_curIsWithinMazeBounds = false;
        m_DISI_curIsOnEntranceArea = false;
        m_DISI_curCardinal = string.Empty;
        m_DISI_curRotation = 0.0f;
        m_DISI_curDistance = 0;
        m_DISI_TreeChild = null;
        m_DISI_randomTreeScale = 0.0f;
        m_DISI_newPosition = Vector3.zero;
        m_DISI_offsetX = 0.0f;
        m_PiecesSinceLastTile = 0;
        m_IsProcessingIslandsAlready = false;
        m_CurrentLoopIslandIndex = 0;
    }

}
