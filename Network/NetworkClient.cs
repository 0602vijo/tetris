using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Raylib_Tetris
{
    public class NetworkClient : GameData
    {
        public static IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        public static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public static PeriodicTimer gameTimer = new(TimeSpan.FromMilliseconds(1000 / 30));

        const byte delimit = 255;
        const byte internal_delimit = 254;
        const byte null_player_id = 253;
        const byte eom = 252;
        const byte atk = 41; //send/receive attack
        const byte board = 42; //get/send board
        const byte sync = 53; //get player id
        const byte player_num = 54; //player amount
        const byte start = 73; //start game

        //standard message format
        //[255, 42, X, 254, 1, 1, 0, 0 ... 3, 3, 255 || 254]
        //  ^       ^       ^
        //start  player id  board data
        //      ^      ^                            ^
        //   msg type  start of board    end of message / begin next
        public static async Task RecieveAsync(IPEndPoint endPoint)
        {
            IPEndPoint remote = new IPEndPoint(endPoint.Address, 0);
            socket.Bind(remote);

            List<byte> data = new List<byte>();

            while (true)
            {
                byte[] buffer = new byte[1024 * 32];
                try
                {
                    await socket.ReceiveFromAsync(buffer, SocketFlags.None, remote);
                    data.AddRange(buffer);
                    if (data.Count > 0)
                    {
                        ParseAsync(data);
                    }
                    buffer = new byte[1024 * 32];
                    data.Clear();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

        }
        //public only for testing
        public static async Task ParseAsync(List<byte> input)
        {
            List<List<byte>> data = new List<List<byte>>();
            data = ListSplit(input, delimit);
            foreach (var list in data)
            {
                switch (list[0])
                {
                    case atk:
                        Attack(list[1]);
                        break;
                    case board:
                        if (list[1] == GameData.playerID)
                        {
                            continue;
                        }
                        else
                        {
                            GameData.playerBoards.TryAdd(list[1], list[3..233]);
                            GameData.playerBoards[list[1]] = list[3..233];
                        }
                        break;
                    case sync:
                        GameData.playerID = list[1];
                        break;
                    case start:
                        //Gamedata.allow_movement = true;       
                        break;
                    case player_num:
                        GameData.num_players = list[1];
                        break;
                }
            }
        }

        public static async Task SendAsync(List<byte> data, byte msg_type)
        {
            List<byte> msg = new List<byte>();
            switch (msg_type)
            {
                case board:
                    msg = [255, msg_type, (byte)GameData.playerID, 254];
                    msg.AddRange(data);
                    socket.SendToAsync(msg.ToArray(), SocketFlags.None, endPoint);
                    break;
                case atk:
                    msg = [255, msg_type, (byte)GameData.playerID];
                    msg.AddRange(data);
                    socket.SendToAsync(msg.ToArray(), SocketFlags.None, endPoint);
                    break;

            }
        }

        public static List<List<byte>> ListSplit(List<byte> input, byte delimit)
        {
            List<List<byte>> output = new List<List<byte>>();
            List<byte> temp_storage = new List<byte>();
            foreach (var item in input)
            {
                if (item == delimit)
                {
                    if (temp_storage.Count > 0)
                    {
                        output.Add(new List<byte>(temp_storage));
                        temp_storage.Clear();
                    }
                }
                else if (item == eom)
                {
                    if (temp_storage.Count > 0)
                    {
                        output.Add(new List<byte>(temp_storage));
                        return output;
                    }
                }
                else
                {
                    temp_storage.Add(item);
                }
            }
            if (temp_storage.Count > 0)
            {
                output.Add(new List<byte>(temp_storage));
            }
            return output;
        }
        //placeholder, not implemented yet
        static List<(byte, byte)> RunLengthEncoding(List<byte> input)
        {
            var values = new List<(byte Count, byte Value)>();

            byte i = 0;
            while (i < input.Count)
            {
                byte value = input[i];
                byte count = 1;
                while (i + count < input.Count && input[i + count] == value)
                {
                    count++;
                }
                values.Add((count, value));
                i += count;
            }
            return values;
        }

        static List<byte> RunLengthDecoding(List<(byte Count, byte Value)> input)
        {
            var values = new List<byte>();
            foreach (var (count, value) in input)
            {
                foreach(int i in Enumerable.Range(0, count))
                {
                    values.Add(value);
                }
            }
            return values;
        }
    }
}
