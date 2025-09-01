using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using rlImGui_cs;
using ImGuiNET;
using System.Diagnostics;
using System.Net;

namespace Raylib_Tetris
{
    internal class Program
    {
        //gravity timers (ms)
        readonly static float[] levels = [
            1000.0F,
            792.7F,
            617.6F,
            472.5F,
            355.0F,
            262.0F,
            189.5F,
            134.8F,
            93.8F,
            64.1F,
            42.9F,
            28.2F,
            16F
        ];
        static void Main(string[] args)
        {
            Game();
        }

        static void Game()
        {
            //init
            GameData gameData = new GameData();

            InitWindow(1920, 1080, "Tetris");
            SetTargetFPS(60);

            rlImGui.Setup(true);

            //internal timers
            float delta_time = 0;
            float frame_timer = 0;
            float gravity_timer = 0;
            float level_up_timer = 0;
            float network_timer = 0;
            int level = 0;

            float left_timer = 0;
            float right_timer = 0;
            float down_timer = 0;

            bool allow_move = false;
            bool gravity_on = false;
            bool initialised = false;

            //default settings
            GameData.DropToBottom = false;

            float delayed_auto_repeat = 0.3f;
            float auto_repeat_rate = 0.075f;

            //raylib init
            int window_width = GetScreenWidth();
            int window_height = GetScreenHeight();

            Camera2D camera = new();
            camera.Target = new Vector2(0, 0);
            camera.Offset = new Vector2(window_width / 2, window_height / 2);
            camera.Rotation = 0;
            camera.Zoom = 1.0f;

            //multiplayer init
            bool multiplayer = false;
            string to_ip = string.Empty;
            bool valid_ip = false;
            IPAddress ip_address = IPAddress.Any;
            string to_port = string.Empty;
            bool valid_port = false;
            ushort port = 0;

            //game init
            while (!WindowShouldClose())
            {
                delta_time = GetFrameTime();

                {
                    if (GameData.spectator)
                    {
                        allow_move = false;
                        gravity_on = false;
                    }
                }
                //inputs ----------------------------------------------------
                {
                    if (allow_move)
                    {
                        if (IsKeyPressed(KeyboardKey.C))
                        {
                            gameData.RotationHandler(KeyboardKey.C);
                            gameData.Hold();
                        }
                        if (IsKeyPressed(KeyboardKey.Space))
                        {
                            gameData.HardDrop();
                        }
                        if (IsKeyPressed(KeyboardKey.Z))
                        {
                            gameData.RotationHandler(KeyboardKey.Z);
                        }
                        if (IsKeyPressed(KeyboardKey.X))
                        {
                            gameData.RotationHandler(KeyboardKey.X);
                        }
                        if (IsKeyPressed(KeyboardKey.Up))
                        {
                            gameData.RotationHandler(KeyboardKey.X);
                        }
                        if (IsKeyDown(KeyboardKey.Down))
                        {
                            if (down_timer == 0)
                            {
                                gameData.MoveHandler(KeyboardKey.Down);
                            }
                            else if (down_timer >= delayed_auto_repeat)
                            {
                                if ((down_timer - delayed_auto_repeat) % auto_repeat_rate >= 0.01f)
                                {
                                    gameData.MoveHandler(KeyboardKey.Down);
                                }
                            }
                            down_timer += delta_time;
                        }
                        else
                        {
                            down_timer = 0;
                        }
                        if (IsKeyDown(KeyboardKey.Left))
                        {
                            if (left_timer == 0)
                            {
                                gameData.MoveHandler(KeyboardKey.Left);
                            }
                            else if (left_timer >= delayed_auto_repeat)
                            {
                                if ((left_timer - delayed_auto_repeat) % auto_repeat_rate >= 0.01f)
                                {
                                    gameData.MoveHandler(KeyboardKey.Left);
                                }
                            }
                            left_timer += delta_time;
                        }
                        else
                        {
                            left_timer = 0;
                        }
                        if (IsKeyDown(KeyboardKey.Right))
                        {
                            if (right_timer == 0)
                            {
                                gameData.MoveHandler(KeyboardKey.Right);
                            }
                            else if (right_timer >= delayed_auto_repeat)
                            {
                                if ((right_timer - delayed_auto_repeat) % auto_repeat_rate >= 0.01f)
                                {
                                    gameData.MoveHandler(KeyboardKey.Right);
                                }
                            }
                            right_timer += delta_time;
                        }
                        else
                        {
                            right_timer = 0;
                        }
                    }
                    if (IsKeyDown(KeyboardKey.Escape))
                    {
                        WindowShouldClose();
                    }
                }
                //timers------------------------------------------------------
                {
                    frame_timer += delta_time;

                    if (gravity_on)
                    {
                        level_up_timer += delta_time;
                        gravity_timer += delta_time;
                    }
                    if (gravity_timer >= levels[level] / 1000) //gravity timers (S)
                    {
                        if (gameData.AllowMove(KeyboardKey.Down, true, GameData.position))
                        {
                            GameData.position.Y--;
                        }
                        gravity_timer = 0;
                    }
                    if (level_up_timer >= 30f)
                    {
                        if (level < 12)
                        {
                            level++;
                        }
                        level_up_timer = 0;
                    }
                    if (frame_timer >= 0.0166f && initialised)
                    {
                        gameData.Update();
                    }
                    if (multiplayer)
                    {
                        network_timer += delta_time;
                    }
                    if (network_timer >= 0.334f && multiplayer && initialised) //network rate = 30 ticks/s
                    {
                        NetworkClient.SendAsync(GameData.playerBoards[GameData.playerID].GetRange(0, 230), 42);
                        Console.WriteLine("sent msg");
                        network_timer = 0;
                    }
                }
                //rendering ---------------------------------------------------------------
                {
                    ClearBackground(new Color(38, 38, 38));
                    BeginDrawing();
                    if (initialised)
                    {
                        Renderer.GenP1Board(window_width, window_height);
                        Renderer.GenBoards(window_width, window_height);
                    }
                    //debug-----------------------------------------------------------------
                    DrawFPS(0, 0);
                    DrawText($"DT {delta_time}", 0, 20, 20, new Color(0, 158, 47));
                    DrawText($"LEVEL: {level}", 0, 40, 20, new Color(0, 158, 47));
                    //gui elements----------------------------------------------------------
                    {
                        rlImGui.Begin();
                        ImGui.Begin("Settings", ImGuiWindowFlags.NoScrollbar);
                        ImGui.Text("Delayed Auto Shift(Seconds)");
                        ImGui.InputFloat("DAS", ref delayed_auto_repeat);
                        ImGui.Text("Auto Repeat Rate(Seconds)");
                        ImGui.InputFloat("ARR", ref auto_repeat_rate);
                        ImGui.Checkbox("Drop to Bottom", ref GameData.DropToBottom);
                        ImGui.NewLine();
                        if (ImGui.Button("Start Singleplayer") && !multiplayer)
                        {
                            gameData.Init();
                            initialised = true;
                            allow_move = true;
                            gravity_on = true;
                            //dont_stop_the_music();
                        }
                        ImGui.NewLine();
                        ImGui.Text("Multiplayer");
                        if (ImGui.InputTextWithHint("IP Address", "127.0.0.1", ref to_ip, 15))
                        {
                            valid_ip = IPAddress.TryParse(to_ip, out ip_address);
                        }
                        if (!valid_ip)
                            ImGui.Text("Wrong Format");
                        else
                            ImGui.Text("Valid IP Address");
                        if (ImGui.InputTextWithHint("Port", "0-65535", ref to_port, 5))
                        {
                            valid_port = ushort.TryParse(to_port, out port);
                        }
                        if (!valid_port)
                            ImGui.Text("Wrong Format");
                        else
                            ImGui.Text("Valid Port");
                        if (ImGui.Button("Connect to server"))
                        {
                            gameData.Init();
                            initialised = true;
                            multiplayer = true;
                            NetworkClient.endPoint.Address = ip_address;
                            NetworkClient.endPoint.Port = port;
                            NetworkClient.RecieveAsync(NetworkClient.endPoint);
                            NetworkClient.SendAsync(GameData.playerBoards[GameData.playerID].GetRange(0, 230), 42);
                        }
                        ImGui.Text("[DEBUG]");
                        ImGui.InputInt("Player ID", ref GameData.playerID);
                        if (ImGui.Button("Debug Start MP"))
                        {
                            allow_move = true;
                            gravity_on = true;
                        }
                        ImGui.End();
                        rlImGui.End();
                    }
                    EndDrawing();
                }
            }
        }
    }
}
