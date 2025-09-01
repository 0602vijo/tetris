using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;
using static Raylib_Tetris.GameData;

namespace Raylib_Tetris
{
    internal class Renderer
    {
        public static void GenP1Board(int width, int height)
        {
            //p1 board
            for (int y = 0; y < 23; y++)
            {
                for (int x = 10; x > 0; x--)
                {
                    DrawRectangle(
                        x * 40 + width/2 - 240,
                        y * 40 + height / 16,
                        40, 40,
                        GetP1BoardColor(y,x, true)
                    );
                    DrawRectangle(
                        x * 40 + width / 2 + 2 - 240,
                        y * 40 + height / 16 + 2,
                        36, 36,
                        GetP1BoardColor(y,x, false)
                    );
                }
            }
            //upcoming
            for (int y = 0; y < 15; y++)
            {
                for(int x = 4; x > 0; x--)
                {
                    DrawRectangle(
                        x * 40 + width / 2 + 180,
                        y * 40 + height / 16 + 120,
                        40, 40,
                        GetP1HoldUpcoming(y,x, true, 0)
                        );
                    DrawRectangle(
                        x * 40 + width / 2 + 180 + 2,
                        y * 40 + height / 16 + 2 + 120,
                        36, 36,
                        GetP1HoldUpcoming(y, x, false, 0)
                    );
                }
            }
            //hold
            for (int y = 0; y < 3; y++)
            {
                for (int x = 4;x > 0; x--)
                {
                    DrawRectangle(
                        x * 40 + width / 2 - 420,
                        y * 40 + height / 16 + 120,
                        40, 40,
                        GetP1HoldUpcoming(y, x, true, 60)
                        );
                    DrawRectangle(
                        x * 40 + width / 2 - 420 + 2,
                        y * 40 + height / 16 + 2 + 120,
                        36, 36,
                        GetP1HoldUpcoming(y, x, false, 60)
                    );
                }
            }
        }
        public static Color GetP1BoardColor(int y, int x, bool first_pass/*, int player_id*/)
        {
            //deathcheck
            if (!first_pass && playerBoards[playerID][219 - (y * 10 - x)] > 10)
            {
                spectator = true;
            }
            else if(first_pass && playerBoards[playerID][219 - (y * 10 - x)] > 10 + 11)
            {
                spectator = true;
            }
            if (spectator)
            {
                for (int i = 0; i < 230; i++)
                {
                    if (playerBoards[playerID][i] != 0 && playerBoards[playerID][i] != 1)
                    {
                        playerBoards[playerID][i] = 2;
                        if (first_pass)
                            return Textures.blockColors[BlockType.GarbageOuter];
                        else
                            return Textures.blockColors[BlockType.Garbage];
                    }
                }
            }
            if (first_pass)
            {
                if (playerBoards[playerID][219 - (y * 10 - x)] > 10)
                {
                    if (playerBoards[playerID][219 - (y * 10 - x)] != 0 && playerBoards[playerID][219 - (y * 10 - x)] != 1)
                    {
                        return Textures.blockColors[BlockType.GarbageOuter];
                    }
                }
                else
                {
                    return Textures.blockColors[(BlockType)playerBoards[playerID][219 - (y * 10 - x)] + 11];
                }
            }
            else
            {
                if (playerBoards[playerID][219 - (y * 10 - x)] > 10)
                {
                    if (playerBoards[playerID][219 - (y * 10 - x)] != 0 && playerBoards[playerID][219 - (y * 10 - x)] != 1)
                    {
                        return Textures.blockColors[BlockType.Garbage];
                    }
                }
                else
                {
                    return Textures.blockColors[(BlockType)playerBoards[playerID][219 - (y * 10 - x)]];
                }
            }
            return Textures.blockColors[BlockType.WhiteOuter];
        }
        public static Color GetP1HoldUpcoming(int y, int x, bool first_pass, int offset)
        {
            if (first_pass)
            {
                return Textures.blockColors[(BlockType)playerBoards[playerID][229 + offset + (y * 4 + x)] + 11];
            }
            else
            {
                return Textures.blockColors[(BlockType)playerBoards[playerID][229 + offset + (y * 4 + x)]];
            }
        }
        public static void GenBoards(int width, int height)
        {
            int virtual_key = 0;
            Vector2 offset = new(0, 0);
            foreach(var keys in playerBoards.Keys.Take(9))
            //for(int keys = 0; keys <= 9; keys++)
            {
                if(keys == playerID)
                {
                    continue;
                }
                switch (virtual_key)
                {
                    case 0:
                        offset = new Vector2(-width * 0.33F, height * 0F);
                        break;
                    case 1:
                        offset = new Vector2(width * 0.21F, height * 0F);
                        break;
                    case 2:
                        offset = new Vector2(-width * 0.33F, height * 0.45F);
                        break;
                    case 3:
                        offset = new Vector2(width * 0.21F, height * 0.45F);
                        break;
                    case 4:
                        offset = new Vector2(-width * 0.47F, height * 0F);
                        break;
                    case 5:
                        offset = new Vector2(width * 0.35F, height * 0F);
                        break;
                    case 6:
                        offset = new Vector2(-width * 0.47F, height * 0.45F);
                        break;
                    case 7:
                        offset = new Vector2(width * 0.35F, height * 0.45F);
                        break;
                }
                for (int y = 0; y < 23; y++)
                {
                    for (int x = 10; x > 0; x--)
                    {
                        DrawRectangle(
                            x * 20 + width / 2 + (int)offset.X,
                            y * 20 + height / 16 + (int)offset.Y,
                            20, 20,
                            RenderOtherPlayer(y,x,keys,true)
                        );
                        DrawRectangle(
                            x * 20 + width / 2 + 1 + (int)offset.X,
                            y * 20 + height / 16 + 1 + (int)offset.Y,
                            18, 18,
                            RenderOtherPlayer(y,x,keys,false)
                        );
                    }
                }
                virtual_key++;
            }
            /*foreach (var keys in playerBoards.Keys.Take(players))
            {
                if (keys == playerID)
                {
                    continue;
                }
                else
                {
                    switch (keys)
                    {
                        case 0:
                            offset = new Vector2(-0.45F, 0.1F);
                            break;
                        case 1:
                            offset = new Vector2(0.65F, 0.1F);
                            break;
                        case 2:
                            offset = new Vector2(-0.45F, -0.9F);
                            break;
                        case 3:
                            offset = new Vector2(0.65F, -0.9F);
                            break;
                        case 4:
                            offset = new Vector2(-0.7F, 0.1F);
                            break;
                        case 5:
                            offset = new Vector2(0.9F, 0.1F);
                            break;
                        case 6:
                            offset = new Vector2(-0.7F, -0.9F);
                            break;
                        case 7:
                            offset = new Vector2(0.9F, -0.9F);
                            break;
                    }
                    for (int y = 0; y < 23; y++)
                    {
                        for (int x = 10; x > 0; x--)
                        {
                            DrawRectangle(
                            x * 20 + width / 2,
                            y * 20 + height / 16,
                            20, 20,
                            GetP1BoardColor(y, x, true)
                        );
                            DrawRectangle(
                                x * 20 + width / 2 + 1,
                                y * 20 + height / 16 + 1,
                                18, 18,
                                GetP1BoardColor(y, x, false)
                            );
                        }
                    }
                }
            }*/
        }
        public static Color RenderOtherPlayer(int y, int x, int player_index, bool first_pass)
        {
            try
            {
                if (first_pass)
                {
                    return Textures.blockColors[(BlockType)playerBoards[player_index][219 - (y * 10 - x)] + 11];
                }
                else
                {
                    return Textures.blockColors[(BlockType)playerBoards[player_index][219 - (y * 10 - x)]];
                }
            }
            catch 
            {
                return Color.White;
            }

        }
    }
}
