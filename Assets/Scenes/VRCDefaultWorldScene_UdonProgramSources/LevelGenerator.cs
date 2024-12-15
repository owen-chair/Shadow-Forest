
using UdonSharp;
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

    private void InitializeIslandsArray()
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
            if (this.m_CopiedMaze[i][j] == 0)
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
            if (x < 0 || x >= this.m_MazeWidth || y < 0 || y >= this.m_MazeHeight || this.m_CopiedMaze[x][y] == 1)
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

    public int GenerateMaze(int width, int height)
    {
        if(this.m_Game == null) return -1;
        this.ResetTwistyPathVariables();

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
            
        int size;
        path = GenerateTwistyPath(width, height, new Vector2Int(1, 1), this.m_Game.m_InitialMazeDataExit, out size);
        
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

        if(foundExit)
        {
            //carve the path
            for (int i = 0; i < path.Length; i++)
            {
                this.m_Game.m_InitialMazeData[path[i].x][path[i].y] = 1;
            }

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

            return 1;
        }
        else
        {
            this.ResetTwistyPathVariables();
            return 0;
        }
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

    public Vector2Int[] GenerateTwistyPath(int width, int height, Vector2Int start, Vector2Int exit, out int size)
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
                    if ((UnityEngine.Random.Range(0, 100) > 2 && (m_Next.x - exit.x) * (m_Next.x - exit.x) + (m_Next.y - exit.y) * (m_Next.y - exit.y) > 25) || 
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
            int randomIndex = UnityEngine.Random.Range(i, directions.Length);
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

        // Randomly choose between four edges (0-3): 0 = top, 1 = bottom, 2 = left, 3 = right
        int randEdge = UnityEngine.Random.Range(0, 2);

        if (randEdge == 0) // Bottom edge
        {
            result.x = UnityEngine.Random.Range(0, width);
            result.y = height - 1;
        }
        else // Right edge
        {
            result.x = width - 1;
            result.y = UnityEngine.Random.Range(0, height);
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

            // Check if this island cell is adjacent to a '1' cell in the maze (water)
            for (int dir = 0; dir < 4; dir++) // Check all 4 directions
            {
                int newX = x + dirX[dir];
                int newY = y + dirY[dir];

                // Ensure the new position is within bounds
                if (newX >= 0 && newX < this.m_MazeWidth && newY >= 0 && newY < this.m_MazeHeight)
                {
                    // If the neighbor is water ('1'), we've found the perimeter
                    if (maze[newX][newY] == 1)
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
                    maze[nextX][nextY] == 1 && (nextX != previous.x || nextY != previous.y))
                {
                    // Ensure that the new position is adjacent to an island cell
                    bool adjacentToIsland = false;
                    for (int dir = 0; dir < 8; dir++)
                    {
                        int checkX = nextX + dirX_D[dir];
                        int checkY = nextY + dirY_D[dir];

                        // Check if this new position is adjacent to an island cell
                        if (checkX >= 0 && checkX < this.m_MazeWidth && checkY >= 0 && checkY < this.m_MazeHeight &&
                            maze[checkX][checkY] == 0) // Adjacent to island cell (water cell)
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
            this.m_DebugMazeInstantiation_Sliced = this.m_Game.m_Maze;
            this.m_TotalRows = -999;
            this.m_TotalCols = -999;
            this.m_ExitPos.x = this.m_Game.m_MazeExit.x;
            this.m_ExitPos.y = this.m_Game.m_MazeExit.y;
            this.m_MazeTheme = theme;

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
    private int DistanceToWalkableSpace(int centerI, int centerJ)
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
                this.m_newI = centerI + this.m_Directions[0][0];
                this.m_newJ = centerJ + this.m_Directions[0][1];
                if (this.m_newI >= 0 && this.m_newI < this.m_TotalRows && this.m_newJ >= 0 && this.m_newJ < this.m_TotalCols)
                {
                    if (this.m_DebugMazeInstantiation_Sliced[this.m_newI][this.m_newJ] == 1)
                        return this.m_CurDistance - 1;
                }

                // Down direction
                this.m_newI = centerI + this.m_Directions[1][0];
                this.m_newJ = centerJ + this.m_Directions[1][1];
                if (this.m_newI >= 0 && this.m_newI < this.m_TotalRows && this.m_newJ >= 0 && this.m_newJ < this.m_TotalCols)
                {
                    if (this.m_DebugMazeInstantiation_Sliced[this.m_newI][this.m_newJ] == 1)
                        return this.m_CurDistance - 1;
                }

                // Left direction
                this.m_newI = centerI + this.m_Directions[2][0];
                this.m_newJ = centerJ + this.m_Directions[2][1];
                if (this.m_newI >= 0 && this.m_newI < this.m_TotalRows && this.m_newJ >= 0 && this.m_newJ < this.m_TotalCols)
                {
                    if (this.m_DebugMazeInstantiation_Sliced[this.m_newI][this.m_newJ] == 1)
                        return this.m_CurDistance - 1;
                }

                // Up direction
                this.m_newI = centerI + this.m_Directions[3][0];
                this.m_newJ = centerJ + this.m_Directions[3][1];
                if (this.m_newI >= 0 && this.m_newI < this.m_TotalRows && this.m_newJ >= 0 && this.m_newJ < this.m_TotalCols)
                {
                    if (this.m_DebugMazeInstantiation_Sliced[this.m_newI][this.m_newJ] == 1)
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
    public GameObject[] m_EmptyForestPrefabs;
    public GameObject m_FullBlockForestPrefab;
    public GameObject m_ThreeSidedForestPrefab;
    public GameObject m_CornerForestPrefab;
    public GameObject m_OneSidedForestPrefab;
    public GameObject m_DoubleSidedForestPrefab;
    public GameObject m_WalkableTileForestPrefab;

    public GameObject[] m_ForestFlowerPrefabs;

    public GameObject GetRandomFlower()
    {
        return this.m_ForestFlowerPrefabs[
            UnityEngine.Random.Range(
                0,
                this.m_ForestFlowerPrefabs.Length)
            ];
    }

    private GameObject GetTilePrefabForType(TileType type, int lod)
    {
        if(this.m_MazeTheme == 1)
        {
            switch(type)
            {
                case TileType.Walkable: return m_WalkableTileForestPrefab;
                case TileType.FullBlock: return m_FullBlockForestPrefab;
                case TileType.ThreeSided: return m_ThreeSidedForestPrefab;
                case TileType.Corner: return m_CornerForestPrefab;
                case TileType.OneSided: return m_OneSidedForestPrefab;
                case TileType.DoubleSided: return m_DoubleSidedForestPrefab;
                case TileType.Exit: return m_Exit;
                default:
                    return m_EmptyForestPrefabs[
                        Mathf.Clamp(lod, 0, m_EmptyForestPrefabs.Length - 1)
                    ];
            }
        }
        else
        {
            switch(type)
            {
                case TileType.Walkable: return m_EmptyDebugPrefab;
                case TileType.FullBlock: return m_FullBlockDebugPrefab;
                case TileType.ThreeSided: return m_ThreeSidedDebugPrefab;
                case TileType.Corner: return m_CornerDebugPrefab;
                case TileType.OneSided: return m_OneSidedDebugPrefab;
                case TileType.DoubleSided: return m_DoubleSidedDebugPrefab;
                default: return m_EmptyDebugPrefab;
            }
        }
    }

    bool isWithinMazeBounds = false;
    bool north = false;
    bool south = false;
    bool west = false;
    bool east = false;

    private TileType GetTileType(int i, int j, out float rotation)//, out string cardinal)
    {
        isWithinMazeBounds = (i-1 >= 0 && i-1 < this.m_MazeWidth && j >= 0 && j < this.m_MazeHeight);
        north = isWithinMazeBounds && (i > 0 && this.m_DebugMazeInstantiation_Sliced[i-1][j] == 1);
        
        isWithinMazeBounds = (i+1 >= 0 && i+1 < this.m_MazeWidth && j >= 0 && j < this.m_MazeHeight);
        south = isWithinMazeBounds && (i < this.m_DebugMazeInstantiation_Sliced.Length-1 && this.m_DebugMazeInstantiation_Sliced[i+1][j] == 1);

        isWithinMazeBounds = (i >= 0 && i < this.m_MazeWidth && j-1 >= 0 && j-1 < this.m_MazeHeight);
        west = isWithinMazeBounds && (j > 0 && this.m_DebugMazeInstantiation_Sliced[i][j-1] == 1);

        isWithinMazeBounds = (i >= 0 && i < this.m_MazeWidth && j+1 >= 0 && j+1 < this.m_MazeHeight);
        east = isWithinMazeBounds && (j < this.m_DebugMazeInstantiation_Sliced[0].Length-1 && this.m_DebugMazeInstantiation_Sliced[i][j+1] == 1);

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
            this.m_Game.m_FPS == 0 ? 0.1f : Mathf.Clamp((1.0f / this.m_Game.m_FPS) / 10.0f, 0.0001f, 0.1f)
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

        if ((!this.m_DISI_curIsWithinMazeBounds || this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 0) && !this.m_DISI_curIsOnEntranceArea)
        {
            this.m_DISI_curDistance = DistanceToWalkableSpace(this.m_DISI_CurI, this.m_DISI_CurJ);
            if(this.m_DISI_curDistance < 7 && this.m_DISI_curDistance != -1)
            {
                this.m_DISI_offsetX = this.m_ColliderSizeRef.x / 2;
                this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
                this.m_DISI_newPosition.y = 0;
                this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
                // Create tile at origin first
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(
                        this.GetTileType(
                            this.m_DISI_CurI,
                            this.m_DISI_CurJ,
                            out this.m_DISI_curRotation
                            //out this.m_DISI_curCardinal
                        ),
                        this.m_DISI_curDistance
                    ),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_DISI_curRotation, 0),
                    this.transform
                );

                if(this.m_DISI_curIsWithinMazeBounds && this.m_MazeTheme == 0)
                {
                    int isPartOfIsland = IsPartOfAnyIsland(
                        this.m_DISI_CurI,
                        this.m_DISI_CurJ,
                        this.m_DebugInstantiation_Size
                    );

                    if (isPartOfIsland != -1)
                    {
                        foreach (Transform child in this.m_TileRef.transform)
                        {
                            Renderer r = child.GetComponent<Renderer>();
                            if (r != null)
                            {
                                r.material.color = colors[isPartOfIsland % (colors.Length - 1)];
                            }
                        }
                    }
                }
                else if(this.m_MazeTheme == 1)
                {
                    // get the first child of the tree
                    if(this.m_TileRef.transform.childCount > 0)
                    {
                        this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(0).gameObject;
                        // add a random scale to the tree between 0.75 and 1
                        this.m_DISI_randomTreeScale = UnityEngine.Random.Range(0.2f, 0.25f);
                        this.m_DISI_TreeChild.transform.localScale.Set(
                            this.m_DISI_randomTreeScale,
                            this.m_DISI_randomTreeScale,
                            this.m_DISI_randomTreeScale
                        );
                        // add a random rotation in the y axis
                        this.m_DISI_TreeChild.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
                        // add a random offset
                        if (this.m_DISI_curDistance > 0)
                        {
                            this.m_DISI_TreeChild.transform.position += new Vector3(
                                UnityEngine.Random.Range(-0.5f, 0.5f),
                                0,
                                UnityEngine.Random.Range(-0.5f, 0.5f)
                            );
                        }
                        // add a tiny random y offset
                        this.m_DISI_TreeChild.transform.position += new Vector3(
                            0,
                            UnityEngine.Random.Range(-0.3f, 0.0f),
                            0
                        );
                    }
                    //get the third child
                    if (this.m_TileRef.transform.childCount > 2)
                    {
                        this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(2).gameObject;
                        // add a random rotation in the y axis
                        this.m_DISI_TreeChild.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
                    }
                }
                    
                this.m_PiecesSinceLastTile = 0;
            }
            else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
            {
                // Not instantiating a piece is worth 10% cpu time
                this.m_PiecesSinceLastTile++;
            }
        }
        else if (this.m_DISI_curIsWithinMazeBounds && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 1)
        {
            //Vector3 position = new Vector3(j, 0, i);

            // first check if exit
            TileType tileType = TileType.Walkable;
            float rotationOffset = 0.0f;
            this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
            this.m_DISI_newPosition.y = 0;
            this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
            if (this.m_DISI_CurI == m_ExitPos.x && this.m_DISI_CurJ == m_ExitPos.y)
            {
                    
                isWithinMazeBounds = (this.m_DISI_CurI-1 >= 0 && this.m_DISI_CurI-1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                north = isWithinMazeBounds && (this.m_DISI_CurI > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI-1][this.m_DISI_CurJ] == 1);
                    
                isWithinMazeBounds = (this.m_DISI_CurI+1 >= 0 && this.m_DISI_CurI+1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                south = isWithinMazeBounds && (this.m_DISI_CurI < this.m_DebugMazeInstantiation_Sliced.Length-1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI+1][this.m_DISI_CurJ] == 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ-1 >= 0 && this.m_DISI_CurJ-1 < this.m_MazeHeight);
                west = isWithinMazeBounds && (this.m_DISI_CurJ > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ-1] == 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ+1 >= 0 && this.m_DISI_CurJ+1 < this.m_MazeHeight);
                east = isWithinMazeBounds && (this.m_DISI_CurJ < this.m_DebugMazeInstantiation_Sliced[0].Length-1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ+1] == 1);

                // now do rotatin
                if(north) rotationOffset = 90.0f;
                else if(east) rotationOffset = 0.0f;
                else if(south) rotationOffset = 270.0f;
                else if (west) rotationOffset = -180.0f;
                else rotationOffset = 180.0f;

                tileType = TileType.Exit;

                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(tileType, 0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, rotationOffset, 0),
                    this.transform
                );

                this.m_ExitTileRef = this.m_TileRef;
            }
            else
            {
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(tileType, 0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, rotationOffset, 0),
                    this.transform
                );
            }

            if(tileType == TileType.Exit)
            {
                if(north) this.m_TileRef.name = "Exit_North";
                else if(east) this.m_TileRef.name = "Exit_East";
                else if(south) this.m_TileRef.name = "Exit_South";
                else if (west) this.m_TileRef.name = "Exit_West";
                else this.m_TileRef.name = "Exit_????";
            }
                
            if(this.m_MazeTheme == 0)
            {
                // get the first child of the tile and color it
                if(this.m_TileRef.transform.childCount > 0)
                {
                    GameObject tileChild = this.m_TileRef.transform.GetChild(0).gameObject;
                    // grassy green
                    tileChild.GetComponent<Renderer>().material.color = new Color(0.0f, 0.5f, 0.0f);
                }
            }

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

        if ((!this.m_DISI_curIsWithinMazeBounds || this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 0) && !this.m_DISI_curIsOnEntranceArea)
        {
            this.m_DISI_curDistance = DistanceToWalkableSpace(this.m_DISI_CurI, this.m_DISI_CurJ);
            if (this.m_DISI_curDistance < 7 && this.m_DISI_curDistance != -1)
            {
                this.m_DISI_offsetX = this.m_ColliderSizeRef.x / 2;
                this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
                this.m_DISI_newPosition.y = 0;
                this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
                // Create tile at origin first
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(
                        this.GetTileType(
                            this.m_DISI_CurI,
                            this.m_DISI_CurJ,
                            out this.m_DISI_curRotation
                        //out this.m_DISI_curCardinal
                        ),
                        this.m_DISI_curDistance
                    ),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_DISI_curRotation, 0),
                    this.transform
                );

                if (this.m_DISI_curIsWithinMazeBounds && this.m_MazeTheme == 0)
                {
                    int isPartOfIsland = IsPartOfAnyIsland(
                        this.m_DISI_CurI,
                        this.m_DISI_CurJ,
                        this.m_DebugInstantiation_Size
                    );

                    if (isPartOfIsland != -1)
                    {
                        foreach (Transform child in this.m_TileRef.transform)
                        {
                            Renderer r = child.GetComponent<Renderer>();
                            if (r != null)
                            {
                                r.material.color = colors[isPartOfIsland % (colors.Length - 1)];
                            }
                        }
                    }
                }
                else if (this.m_MazeTheme == 1)
                {
                    // get the first child of the tree
                    if (this.m_TileRef.transform.childCount > 0)
                    {
                        this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(0).gameObject;
                        // add a random scale to the tree between 0.75 and 1
                        this.m_DISI_randomTreeScale = UnityEngine.Random.Range(0.2f, 0.25f);
                        this.m_DISI_TreeChild.transform.localScale.Set(
                            this.m_DISI_randomTreeScale,
                            this.m_DISI_randomTreeScale,
                            this.m_DISI_randomTreeScale
                        );
                        // add a random rotation in the y axis
                        this.m_DISI_TreeChild.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
                        // add a random offset
                        if (this.m_DISI_curDistance > 0)
                        {
                            this.m_DISI_TreeChild.transform.position += new Vector3(
                                UnityEngine.Random.Range(-0.5f, 0.5f),
                                0,
                                UnityEngine.Random.Range(-0.5f, 0.5f)
                            );
                        }
                        // add a tiny random y offset
                        this.m_DISI_TreeChild.transform.position += new Vector3(
                            0,
                            UnityEngine.Random.Range(-0.3f, 0.0f),
                            0
                        );
                    }

                    //get the third child
                    if (this.m_TileRef.transform.childCount > 2)
                    {
                        this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(2).gameObject;
                        // add a random rotation in the y axis
                        this.m_DISI_TreeChild.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);

                        // add a random flower
                        GameObject flower = GetRandomFlower();
                        UnityEngine.Object.Instantiate(
                            flower,
                            new Vector3(
                                this.m_DISI_newPosition.x + UnityEngine.Random.Range(-0.5f, 0.5f),
                                0,
                                this.m_DISI_newPosition.z + UnityEngine.Random.Range(-0.5f, 0.5f)
                            ),
                            Quaternion.Euler(0, this.m_DISI_curRotation, 0),
                            this.m_TileRef.transform
                        );
                    }
                }

                this.m_PiecesSinceLastTile = 0;
            }
            else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
            {
                // Not instantiating a piece is worth 10% cpu time
                this.m_PiecesSinceLastTile++;
            }
        }
        else if (this.m_DISI_curIsWithinMazeBounds && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 1)
        {
            // first check if exit
            TileType tileType = TileType.Walkable;
            float rotationOffset = 0.0f;
            this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
            this.m_DISI_newPosition.y = 0;
            this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;

            if (this.m_DISI_CurI == m_ExitPos.x && this.m_DISI_CurJ == m_ExitPos.y)
            {

                isWithinMazeBounds = (this.m_DISI_CurI - 1 >= 0 && this.m_DISI_CurI - 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                north = isWithinMazeBounds && (this.m_DISI_CurI > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI - 1][this.m_DISI_CurJ] == 1);

                isWithinMazeBounds = (this.m_DISI_CurI + 1 >= 0 && this.m_DISI_CurI + 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                south = isWithinMazeBounds && (this.m_DISI_CurI < this.m_DebugMazeInstantiation_Sliced.Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI + 1][this.m_DISI_CurJ] == 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ - 1 >= 0 && this.m_DISI_CurJ - 1 < this.m_MazeHeight);
                west = isWithinMazeBounds && (this.m_DISI_CurJ > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ - 1] == 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ + 1 >= 0 && this.m_DISI_CurJ + 1 < this.m_MazeHeight);
                east = isWithinMazeBounds && (this.m_DISI_CurJ < this.m_DebugMazeInstantiation_Sliced[0].Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ + 1] == 1);

                // now do rotatin
                if (north) rotationOffset = 90.0f;
                else if (east) rotationOffset = 0.0f;
                else if (south) rotationOffset = 270.0f;
                else if (west) rotationOffset = -180.0f;
                else rotationOffset = 180.0f;

                tileType = TileType.Exit;

                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(tileType, 0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, rotationOffset, 0),
                    this.transform
                );

                this.m_ExitTileRef = this.m_TileRef;
            }
            else
            {
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(tileType, 0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, rotationOffset, 0),
                    this.transform
                );
            }

            if (tileType == TileType.Exit)
            {
                if (north) this.m_TileRef.name = "Exit_North";
                else if (east) this.m_TileRef.name = "Exit_East";
                else if (south) this.m_TileRef.name = "Exit_South";
                else if (west) this.m_TileRef.name = "Exit_West";
                else this.m_TileRef.name = "Exit_????";
            }

            if (this.m_MazeTheme == 0)
            {
                // get the first child of the tile and color it
                if (this.m_TileRef.transform.childCount > 0)
                {
                    GameObject tileChild = this.m_TileRef.transform.GetChild(0).gameObject;
                    // grassy green
                    tileChild.GetComponent<Renderer>().material.color = new Color(0.0f, 0.5f, 0.0f);
                }
            }

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

        if ((!this.m_DISI_curIsWithinMazeBounds || this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 0) && !this.m_DISI_curIsOnEntranceArea)
        {
            this.m_DISI_curDistance = DistanceToWalkableSpace(this.m_DISI_CurI, this.m_DISI_CurJ);
            if (this.m_DISI_curDistance < 7 && this.m_DISI_curDistance != -1)
            {
                this.m_DISI_offsetX = this.m_ColliderSizeRef.x / 2;
                this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
                this.m_DISI_newPosition.y = 0;
                this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
                // Create tile at origin first
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(
                        this.GetTileType(
                            this.m_DISI_CurI,
                            this.m_DISI_CurJ,
                            out this.m_DISI_curRotation
                        //out this.m_DISI_curCardinal
                        ),
                        this.m_DISI_curDistance
                    ),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, this.m_DISI_curRotation, 0),
                    this.transform
                );

                if (this.m_DISI_curIsWithinMazeBounds && this.m_MazeTheme == 0)
                {
                    int isPartOfIsland = IsPartOfAnyIsland(
                        this.m_DISI_CurI,
                        this.m_DISI_CurJ,
                        this.m_DebugInstantiation_Size
                    );

                    if (isPartOfIsland != -1)
                    {
                        foreach (Transform child in this.m_TileRef.transform)
                        {
                            Renderer r = child.GetComponent<Renderer>();
                            if (r != null)
                            {
                                r.material.color = colors[isPartOfIsland % (colors.Length - 1)];
                            }
                        }
                    }
                }
                else if (this.m_MazeTheme == 1)
                {
                    // get the first child of the tree
                    if (this.m_TileRef.transform.childCount > 0)
                    {
                        this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(0).gameObject;
                        // add a random scale to the tree between 0.75 and 1
                        this.m_DISI_randomTreeScale = UnityEngine.Random.Range(0.2f, 0.25f);
                        this.m_DISI_TreeChild.transform.localScale.Set(
                            this.m_DISI_randomTreeScale,
                            this.m_DISI_randomTreeScale,
                            this.m_DISI_randomTreeScale
                        );
                        // add a random rotation in the y axis
                        this.m_DISI_TreeChild.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
                        // add a random offset
                        if (this.m_DISI_curDistance > 0)
                        {
                            this.m_DISI_TreeChild.transform.position += new Vector3(
                                UnityEngine.Random.Range(-0.5f, 0.5f),
                                0,
                                UnityEngine.Random.Range(-0.5f, 0.5f)
                            );
                        }
                        // add a tiny random y offset
                        this.m_DISI_TreeChild.transform.position += new Vector3(
                            0,
                            UnityEngine.Random.Range(-0.3f, 0.0f),
                            0
                        );
                    }
                    //get the third child
                    if (this.m_TileRef.transform.childCount > 2)
                    {
                        this.m_DISI_TreeChild = this.m_TileRef.transform.GetChild(2).gameObject;
                        // add a random rotation in the y axis
                        this.m_DISI_TreeChild.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
                    }
                }

                this.m_PiecesSinceLastTile = 0;
            }
            else if (this.m_DISI_CurI > 0 && this.m_DISI_CurJ > 0)
            {
                // Not instantiating a piece is worth 10% cpu time
                this.m_PiecesSinceLastTile++;
            }
        }
        else if (this.m_DISI_curIsWithinMazeBounds && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ] == 1)
        {
            //Vector3 position = new Vector3(j, 0, i);

            // first check if exit
            TileType tileType = TileType.Walkable;
            float rotationOffset = 0.0f;
            this.m_DISI_newPosition.x = this.m_DISI_CurJ - this.m_DISI_offsetX;
            this.m_DISI_newPosition.y = 0;
            this.m_DISI_newPosition.z = this.m_DISI_CurI - this.m_DISI_offsetX;
            if (this.m_DISI_CurI == m_ExitPos.x && this.m_DISI_CurJ == m_ExitPos.y)
            {

                isWithinMazeBounds = (this.m_DISI_CurI - 1 >= 0 && this.m_DISI_CurI - 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                north = isWithinMazeBounds && (this.m_DISI_CurI > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI - 1][this.m_DISI_CurJ] == 1);

                isWithinMazeBounds = (this.m_DISI_CurI + 1 >= 0 && this.m_DISI_CurI + 1 < this.m_MazeWidth && this.m_DISI_CurJ >= 0 && this.m_DISI_CurJ < this.m_MazeHeight);
                south = isWithinMazeBounds && (this.m_DISI_CurI < this.m_DebugMazeInstantiation_Sliced.Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI + 1][this.m_DISI_CurJ] == 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ - 1 >= 0 && this.m_DISI_CurJ - 1 < this.m_MazeHeight);
                west = isWithinMazeBounds && (this.m_DISI_CurJ > 0 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ - 1] == 1);

                isWithinMazeBounds = (this.m_DISI_CurI >= 0 && this.m_DISI_CurI < this.m_MazeWidth && this.m_DISI_CurJ + 1 >= 0 && this.m_DISI_CurJ + 1 < this.m_MazeHeight);
                east = isWithinMazeBounds && (this.m_DISI_CurJ < this.m_DebugMazeInstantiation_Sliced[0].Length - 1 && this.m_DebugMazeInstantiation_Sliced[this.m_DISI_CurI][this.m_DISI_CurJ + 1] == 1);

                // now do rotatin
                if (north) rotationOffset = 90.0f;
                else if (east) rotationOffset = 0.0f;
                else if (south) rotationOffset = 270.0f;
                else if (west) rotationOffset = -180.0f;
                else rotationOffset = 180.0f;

                tileType = TileType.Exit;

                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(tileType, 0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, rotationOffset, 0),
                    this.transform
                );

                this.m_ExitTileRef = this.m_TileRef;
            }
            else
            {
                this.m_TileRef = UnityEngine.Object.Instantiate(
                    this.GetTilePrefabForType(tileType, 0),
                    this.m_DISI_newPosition,
                    Quaternion.Euler(0, rotationOffset, 0),
                    this.transform
                );
            }

            if (tileType == TileType.Exit)
            {
                if (north) this.m_TileRef.name = "Exit_North";
                else if (east) this.m_TileRef.name = "Exit_East";
                else if (south) this.m_TileRef.name = "Exit_South";
                else if (west) this.m_TileRef.name = "Exit_West";
                else this.m_TileRef.name = "Exit_????";
            }

            if (this.m_MazeTheme == 0)
            {
                // get the first child of the tile and color it
                if (this.m_TileRef.transform.childCount > 0)
                {
                    GameObject tileChild = this.m_TileRef.transform.GetChild(0).gameObject;
                    // grassy green
                    tileChild.GetComponent<Renderer>().material.color = new Color(0.0f, 0.5f, 0.0f);
                }
            }

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

    public void DebugInstantiateMaze_Slice_AI()
    {
        // Iterate through each island loop
        for (int islandIndex = 0; islandIndex < this.m_Loops.Length; islandIndex++)
        {
            // Check if the loop is not empty
            if (this.m_Loops[islandIndex].Length > 0)
            {
                // Check that all positions in the loop are greater than 10.0 distance away from 3,3
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

                if (!isAwayFromSpawn) continue;

                Vector2Int loopCoordinate = this.m_Loops[islandIndex][0];
                if (loopCoordinate.x == -999 && loopCoordinate.y == -999) { continue; }

                EnemyAI_Animated.InstantiateAI(this.m_DebugAICylinderPrefab1x1, transform, this.m_Loops[islandIndex], colors[islandIndex % (colors.Length - 1)], islandIndex, this.m_Game);
            }
        }

        SendCustomEventDelayedSeconds(
            nameof(this.Done),
            this.m_OptimisationDelayKek
        );
    }

    public void Done()
    {
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
