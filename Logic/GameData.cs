using static Raylib_cs.Raylib;
using System.Numerics;
using System.Collections.Concurrent;
using System.Diagnostics;
using Raylib_cs;

namespace Raylib_Tetris
{
    public class GameData : BagHold
    {
        public static byte[,] board = new byte[23, 10];
        static byte[,] tempBoard;
        public static List<byte[,]> upcomingHold = new();
        public static int playerID = 1;
        public static bool spectator = false;
        public static Vector2 position = new Vector2();
        static int rotationState = 0;
        public static bool DropToBottom;
        public static int num_players = 0;
        public static bool networking = false;
        public static bool start = false;
        public static bool t_spin = false;
        
        // int == playerid, List<byte> == boards,
        public static ConcurrentDictionary<int, List<byte>> playerBoards = new();

        public void Update()
        {
            playerBoards[playerID].Clear();
            SpawnTetromino();
            for (int y = 0; y < 23; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    tempBoard[y, x] += board[y, x];
                    if (y >= 20)
                    {
                        if (tempBoard[y, x] == 0)
                            playerBoards[playerID].Add(1);
                        else
                            playerBoards[playerID].Add(tempBoard[y, x]);
                    }
                    else
                    {
                        playerBoards[playerID].Add(tempBoard[y, x]);
                    }
                }
            }
            AddToUpcomingHold();
        }
        
        void SpawnTetromino()
        {
            tempBoard = new byte[23,10];
            int temp = GetTetrominoLength();
            Vector2 fakePos = position;
            while(AllowMove(KeyboardKey.Down, false, fakePos))
            {
                fakePos.Y--;
            }
            for(int y = 0; y < temp; y++)
            {
                for (int x = 0; x < temp; x++)
                {
                    if (Tetromino[active_piece][rotationState][y,x] != 0)
                    {
                        tempBoard[(temp - y) + (int)fakePos.Y, x + (int)position.X] = 3;
                        tempBoard[(temp - y) + (int)position.Y,x+(int)position.X] = Tetromino[active_piece][rotationState][y, x];
                    }
                }
            }
        }

        public void MoveHandler(KeyboardKey movement)
        {
            if(movement == KeyboardKey.Down)
            {
                if (DropToBottom)
                {
                    if (!FindLowest())
                    {
                        HardDrop();
                    }
                }
                else if(AllowMove(movement, true, position))
                {
                    position.Y--;
                }
            }
            else if(AllowMove(movement, true, position))
            {
                switch (movement)
                {
                    case KeyboardKey.Left:
                        position.X--;
                        break;
                    case KeyboardKey.Right:
                        position.X++;
                        break;
                }
            }
        }
        //checks if move collides with anything
        public bool AllowMove(KeyboardKey movement, bool shouldDrop, Vector2 pos)
        {
            int temp = GetTetrominoLength();

            for(int y = 0; y < temp; y++)
            {
                for(int x = 0; x < temp; x++)
                {
                    if (Tetromino[active_piece][rotationState][y,x] != 0)
                    {
                        if(movement == KeyboardKey.Right)
                        {
                            if((x + pos.X >= 9) || (board[(temp - y) + (int)pos.Y, x + (int)pos.X + 1] != 0))
                            {
                                return false;
                            }
                        }
                        else if(movement == KeyboardKey.Left)
                        {
                            if((x + pos.X <= 0) || (board[(temp - y) + (int)pos.Y, x + (int)pos.X - 1] != 0))
                            {
                                return false;
                            }
                        }
                        else if(movement == KeyboardKey.Down)
                        {
                            if(temp - y + (int)pos.Y == 0 || (board[(temp - y) + ((int)pos.Y - 1), x + (int)pos.X] != 0))
                            {
                                if (shouldDrop)
                                {
                                    Drop();
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void RotationHandler(KeyboardKey rotate)
        {
            int testRotation = rotationState;
            if(rotate == KeyboardKey.Space)
            {
                rotationState = 0;
                testRotation = 0;
            }
            if (active_piece != BlockType.Opiece)
            {
                switch (rotate)
                {
                    case KeyboardKey.Z:
                        testRotation--;
                        if (testRotation == -1)
                            testRotation = 3;
                    break;
                    case KeyboardKey.X:
                        testRotation++;
                        if (testRotation == 4)
                            testRotation = 0;
                    break;
                    default:
                        rotationState = 0;
                        testRotation = 0;
                    break;
                }
                if(Wallkick(testRotation))
                {
                    rotationState = testRotation;
                }
            }
        }
        // https://harddrop.com/wiki/SRS#Wall_Kicks
        // wallkick data and offsets
        bool Wallkick(int testRotation)
        {
            int temp = GetTetrominoLength();
            Vector2 positionTest = position;
            for(int testCase = 0; testCase < 5; testCase++)
            {
                if(testCase < 0 && active_piece == BlockType.Tpiece)
                {
                    t_spin = true;
                }
                else
                {
                    t_spin = false;
                }
                if (active_piece == BlockType.Ipiece)
                {
                    //0 -> R, L -> 2 ( 0, 0) (-2, 0) (+1, 0) (-2,-1) (+1,+2)
                    if ((rotationState == 0 && testRotation == 1) || (rotationState == 3 && testRotation == 2))
                    {
                        switch (testCase)
                        {
                            case 0:
                                positionTest = position;
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 1:
                                positionTest = position + new Vector2(-2, 0);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 2:
                                positionTest = position + new Vector2(+1, 0);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 3:
                                positionTest = position + new Vector2(-2, -1);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 4:
                                positionTest = position + new Vector2(+1, +2);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 5:
                                return false;
                        }
                    }
                    //R -> 0, 2 -> L ( 0, 0) (+2, 0) (-1, 0) (+2,+1) (-1,-2)
                    if ((rotationState == 1 && testRotation == 0) || (rotationState == 2 && testRotation == 3))
                    {
                        switch (testCase)
                        {
                            case 0:
                                positionTest = position;
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 1:
                                positionTest = position + new Vector2(+2, 0);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 2:
                                positionTest = position + new Vector2(-1, 0);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 3:
                                positionTest = position + new Vector2(+2, +1);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 4:
                                positionTest = position + new Vector2(-1, -2);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 5:
                                return false;
                        }
                    }
                    //R -> 2, 0 -> L ( 0, 0) (-1, 0) (+2, 0) (-1,+2) (+2,-1)
                    if ((rotationState == 1 && testRotation == 2) || (rotationState == 0 && testRotation == 3))
                    {
                        switch (testCase)
                        {
                            case 0:
                                positionTest = position;
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 1:
                                positionTest = position + new Vector2(-1, 0);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 2:
                                positionTest = position + new Vector2(+2, 0);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 3:
                                positionTest = position + new Vector2(-1, +2);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 4:
                                positionTest = position + new Vector2(+2, -1);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 5:
                                return false;
                        }
                    }
                    //2 -> R, L -> 0 ( 0, 0) (+1, 0) (-2, 0) (+1,-2) (-2,+1)
                    if ((rotationState == 2 && testRotation == 1) || (rotationState == 3 && testRotation == 0))
                    {
                        switch (testCase)
                        {
                            case 0:
                                positionTest = position;
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 1:
                                positionTest = position + new Vector2(+1, 0);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 2:
                                positionTest = position + new Vector2(-2, 0);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 3:
                                positionTest = position + new Vector2(+1, -2);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 4:
                                positionTest = position + new Vector2(-2, +1);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 5:
                                return false;
                        }

                    }
                }
                else
                {
                    //0 -> R, 2 -> R ( 0, 0) (-1, 0) (-1,+1) ( 0,-2) (-1,-2)
                    if ((rotationState == 0 && testRotation == 1) || (rotationState == 2 && testRotation == 1))
                    {
                        switch (testCase)
                        {
                            case 0:
                                positionTest = position;
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 1:
                                positionTest = position + new Vector2(-1, 0);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 2:
                                positionTest = position + new Vector2(-1, +1);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 3:
                                positionTest = position + new Vector2(0, +2);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 4:
                                positionTest = position + new Vector2(-1, -2);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 5:
                                return false;
                        }
                    }
                    //R -> 0, R -> 2 ( 0, 0) (+1, 0) (+1,-1) ( 0,+2) (+1,+2)
                    if ((rotationState == 1 && testRotation == 0) || (rotationState == 1 && testRotation == 2))
                    {
                        switch (testCase)
                        {
                            case 0:
                                positionTest = position;
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 1:
                                positionTest = position + new Vector2(+1, 0);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 2:
                                positionTest = position + new Vector2(+1, -1);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 3:
                                positionTest = position + new Vector2(0, +2);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 4:
                                positionTest = position + new Vector2(+1, +2);
                                if(RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 5:
                                return false;
                        }
                    }
                    //2 -> L, 0 -> L ( 0, 0) (+1, 0) (+1,+1) ( 0,-2) (+1,-2)
                    if ((rotationState == 2 && testRotation == 3) || (rotationState == 0 && testRotation == 3))
                    {
                        switch (testCase)
                        {
                            case 0:
                                positionTest = position;
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 1:
                                positionTest = position + new Vector2(+1, 0);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 2:
                                positionTest = position + new Vector2(+1, +1);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 3:
                                positionTest = position + new Vector2(0, -2);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 4:
                                positionTest = position + new Vector2(+1, -2);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 5:
                                return false;
                        }
                    }
                    //L -> 2, L -> 0 ( 0, 0) (-1, 0) (-1,-1) ( 0,+2) (-1,+2)
                    if ((rotationState == 3 && testRotation == 2) || (rotationState == 3 && testRotation == 0))
                    {
                        switch (testCase)
                        {
                            case 0:
                                positionTest = position;
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 1:
                                positionTest = position + new Vector2(-1, 0);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 2:
                                positionTest = position + new Vector2(-1, -1);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 3:
                                positionTest = position + new Vector2(0, +2);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 4:
                                positionTest = position + new Vector2(-1, +2);
                                if (RotationTester(testRotation, positionTest)) { position = positionTest; return true; }
                            break;
                            case 5:
                                return false;
                        }

                    }
                }
            }
            return false;
        }
        
        //out of bounds and collission checking for rotation
        bool RotationTester(int testRotation, Vector2 testPosition)
        {
            int temp = GetTetrominoLength();
            for(int y = 0; y < temp; y++)
            {
                for(int x = 0; x < temp; x++)
                {
                    if(Tetromino[active_piece][testRotation][y,x] != 0)
                    {
                        if(temp - y + (int)testPosition.Y < 0)
                        {
                            return false;
                        }
                        else if(x+testPosition.X >= 10)
                        {
                            return false;
                        }
                        else if(x+testPosition.X <= -1)
                        {
                            return false;
                        }
                        else if((board[(temp - y) + (int)testPosition.Y, x + (int)testPosition.X] != 0))
                        {
                            return false;
                        }

                    }
                }
            }
            return true;
        }

        public override void HardDrop()
        {
            FindLowest();
            PasteTetromino();
            RotationHandler(KeyboardKey.Space);
            base.HardDrop();
            CheckLines();
        }

        public void Drop()
        {
            PasteTetromino();
            RotationHandler(KeyboardKey.Space);
            base.HardDrop();
            CheckLines();
        }

        void CheckLines()
        {
            int attack = 0;
            bool all_clear = true;
            for (int y = 0; y < 20; y++)
            {
                bool toClear = true;
                for(int x = 0; x < 10; x++)
                {
                    if (board[y,x] == 0)
                    {
                        toClear = false;
                        break;
                    }
                }
                if (toClear)
                {
                    MoveBoard(y);
                    y--;
                    attack++;
                    //CheckLines();
                }
            }
            if(attack > 0)
            {
                for(int y = 0; y < 20; y++)
                {
                    for(int x = 0; x < 10; x++)
                    {
                        if (board[y, x] != (byte)BlockType.Background && board[y,x] != (byte)BlockType.White)
                        {
                            Debug.WriteLine($"board: {board[y, x]}, y{y}, x{x}");
                            all_clear = false;
                        }
                    }
                }
                Debug.WriteLine($"sent all clear: {all_clear}");
                SendLines(attack, all_clear);
            }
        }
        //multiplayer attacks
        void SendLines(int lines, bool all_clear)
        {
            if (all_clear == true)
            {
                NetworkClient.SendAsync(new List<byte> { 10 }, 41);
            }
            else if (t_spin == true)
            {
                NetworkClient.SendAsync(new List<byte> { 4 }, 41);
                t_spin = false;
            }
            else if (lines == 2)
            {
                NetworkClient.SendAsync(new List<byte> { 1 }, 41);
            }
            else if (lines == 3)
            {
                NetworkClient.SendAsync(new List<byte> { 2 }, 41);
            }
            else if (lines == 4)
            {
                NetworkClient.SendAsync(new List<byte> { 4 }, 41);
            }
        }
        //receive attacks
        public static void Attack(int atk_count)
        {
            Random rand = new Random();
            int atk_row = rand.Next(0, 10);
            for(int i = 0; i < atk_count; i++)
            {
                for (int y = 23; y >= 0; y--)
                {
                    for(int x = 0; x < 10; x++)
                    {
                        if(y == 23)
                        {
                            continue;
                        }
                        else
                        {
                            if(y-1 >= 0)
                            {
                                board[y, x] = board[y - 1, x];
                            }
                            else
                            {
                                board[0, x] = 0;
                            }
                        }
                    }
                }
                for(int x = 0; x < 10; x++)
                {
                    if(x == atk_row)
                    {
                        continue;
                    }
                    else
                    {
                        board[0, x] = 2;
                    }
                }
            }
        }

        void MoveBoard(int offset)
        {
            for(int y = offset; y < 20; y++)
            { 
                for(int x = 0; x < 10; x++)
                {
                    board[y,x] = board[y+1,x];
                }
            }
        }

        void PasteTetromino()
        {
            int temp = GetTetrominoLength();
            for(int y = 0; y < temp; y++)
            {
                for(int x = 0; x < temp; x++)
                {
                    if (Tetromino[active_piece][rotationState][y,x] != 0)
                    {
                        board[temp - y + (int)position.Y, x + (int)position.X] = Tetromino[active_piece][rotationState][y, x];
                    }
                }
            }
        }

        bool FindLowest()
        {
            int startPos = (int)position.Y;
            while(AllowMove(KeyboardKey.Down, false, position))
            {
                position.Y--;
            }
            if(startPos == position.Y)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        int GetTetrominoLength()
        {
            return Tetromino[active_piece][rotationState].GetLength(0);
        }

        public void Init()
        {
            InitBags();
            InitBoard();
            Update();
        }

        void InitBoard()
        {
            playerBoards.TryAdd(playerID, new List<byte>());
        }
        //legacy from opengl version, prevents crash from non-defined BlockType enum
        public void PleaseDoNotCrashTheServerPlease()
        {
            for (int y = 0; y < 23; y++)
            {
                for(int x = 0; x < 10; x++)
                {
                    if (board[y, x] >= 11 || playerBoards[playerID][y * 10 + x] >= 11)
                    {
                        board[y, x] = 2;
                        playerBoards[playerID][y * 10 + x] = 2;
                        spectator = true;
                        GameOver();
                    }
                }
            }
        }
        void GameOver()
        {
            for(int y = 0 ;y < 23; y++)
            {
                for(int x = 0; x < 10; x++)
                {
                    if (playerBoards[playerID][y * 10 + x] > 3)
                    {
                        playerBoards[playerID][y * 10 + x] = 2;
                    }
                }
            }
        }
        void AddToUpcomingHold()
        {
            for(int i = 0; i < 6; i++)
            {
                for(int y = 0; y < 3; y++)
                {
                    for(int x = 0; x < 4; x++)
                    {
                        switch (displayed_blocks[i])
                        {
                            case BlockType.Empty:
                                playerBoards[playerID].Add(1);
                                break;
                            case BlockType.Ipiece:
                                if(Tetromino[displayed_blocks[i]][1][x,y+1] == 0)
                                    playerBoards[playerID].Add(1);
                                else
                                    playerBoards[playerID].Add(4);
                                break;
                            case BlockType.Opiece:
                                //skrivet så här för att det bara tar längre tid att göra på korrekt sätt
                                if (x == 0 || x == 3)
                                    playerBoards[playerID].Add(1);
                                else if (y == 2)
                                    playerBoards[playerID].Add(1);
                                else
                                    playerBoards[playerID].Add(5);
                                break;
                            default:
                                if (x == 0 || Tetromino[displayed_blocks[i]][0][y,x-1] == 0)
                                    playerBoards[playerID].Add(1);
                                else
                                    playerBoards[playerID].Add(Tetromino[displayed_blocks[i]][0][y,x-1]);
                                break;
                        }
                    }
                }
            }
            //playerBoards[playerID].Add(Tetromino[displayed_blocks[i]][1][y, x]);
        }
        //blocks and their rotation in data form
        public static readonly Dictionary<BlockType, byte[][,]> Tetromino = new Dictionary<BlockType, byte[][,]>{
            {BlockType.Ipiece, new byte[][,]{
                new byte[,] {
                    {0,0,0,0},
                    {4,4,4,4},
                    {0,0,0,0},
                    {0,0,0,0}},
                new byte[,] {
                    {0,0,4,0},
                    {0,0,4,0},
                    {0,0,4,0},
                    {0,0,4,0}},
                new byte[,] {
                    {0,0,0,0},
                    {0,0,0,0},
                    {4,4,4,4},
                    {0,0,0,0}},
                new byte[,] {
                    {0,4,0,0},
                    {0,4,0,0},
                    {0,4,0,0},
                    {0,4,0,0}}
                }},
            {BlockType.Opiece, new byte[][,]{
                new byte[,] {
                    {5,5},
                    {5,5}},
                }},
            {BlockType.Tpiece, new byte[][,]{
                new byte[,]{
                    {0,6,0},
                    {6,6,6},
                    {0,0,0}},
                new byte[,]{
                    {0,6,0},
                    {0,6,6},
                    {0,6,0}},
                new byte[,]{
                    {0,0,0},
                    {6,6,6},
                    {0,6,0}},
                new byte[,]{
                    {0,6,0},
                    {6,6,0},
                    {0,6,0}}
            }},
            {BlockType.Spiece, new byte[][,]{
                new byte[,]{
                    {0,7,7},
                    {7,7,0},
                    {0,0,0}},
                new byte[,]{
                    {0,7,0},
                    {0,7,7},
                    {0,0,7}},
                new byte[,]{
                    {0,0,0},
                    {0,7,7},
                    {7,7,0}},
                new byte[,]{
                    {7,0,0},
                    {7,7,0},
                    {0,7,0}}
            }},
            {BlockType.Zpiece, new byte[][,]{
                new byte[,]{
                    {8,8,0},
                    {0,8,8},
                    {0,0,0}},
                new byte[,]{
                    {0,0,8},
                    {0,8,8},
                    {0,8,0}},
                new byte[,]{
                    {0,0,0},
                    {8,8,0},
                    {0,8,8}},
                new byte[,]{
                    {0,8,0},
                    {8,8,0},
                    {8,0,0}}
            }},
            {BlockType.Jpiece, new byte[][,]{
                new byte[,]{
                   {9,0,0},
                   {9,9,9},
                   {0,0,0}},
                new byte[,]{
                    {0,9,9},
                    {0,9,0},
                    {0,9,0}},
                new byte[,]{
                    {0,0,0},
                    {9,9,9},
                    {0,0,9}},
                new byte[,]{
                    {0,9,0},
                    {0,9,0},
                    {9,9,0}}
            }},
            {BlockType.Lpiece, new byte[][,]{
                new byte[,]{
                    {0,0,10},
                    {10,10,10},
                    {0,0,0}},
                new byte[,]{
                    {0,10,0},
                    {0,10,0},
                    {0,10,10}},
                new byte[,]{
                    {0,0,0},
                    {10,10,10},
                    {10,0,0}},
                new byte[,]{
                    {10,10,0},
                    {0,10,0},
                    {0,10,0}}
            }},
        };
        
        public void DebugClear()
        {
            for(int y = 0; y < 23; y++)
            {
                for(int x = 0; x < 10; x++)
                {
                    board[y,x] = 0;
                }
            }
        }
    }
}
