using Raylib_cs;

namespace Raylib_Tetris
{
    internal class Textures
    {
        //used to be a uv-map in opengl version, now only blocktype to color
        public static readonly Dictionary<BlockType, Color> blockColors = new Dictionary<BlockType, Raylib_cs.Color>()
        {
            {BlockType.Background, new Color(38,38,38) },
            {BlockType.BackgroundOuter, new Color(73,73,73)},
            {BlockType.Empty, new Color(38, 38, 38) },
            {BlockType.EmptyOuter, new Color(38,38,38)},
            {BlockType.Garbage, new Color (98,98,98) },
            {BlockType.GarbageOuter, new Color(38,38,38)},
            {BlockType.White, new Color(255,255,255) },
            {BlockType.WhiteOuter, new Color(99,99,99)},
            {BlockType.Ipiece, new Color(84,236,236) },
            {BlockType.IpieceOuter, new Color(7,51,51)},
            {BlockType.Opiece, new Color(242,241,47) },
            {BlockType.OpieceOuter, new Color(53,53,0)},
            {BlockType.Tpiece, new Color(158,43,169) },
            {BlockType.TpieceOuter, new Color(45,9,48)},
            {BlockType.Spiece, new Color(71,195,57) },
            {BlockType.SpieceOuter, new Color(21,65,16)},
            {BlockType.Zpiece, new Color(182,29,30) },
            {BlockType.ZpieceOuter, new Color(62,11,12)},
            {BlockType.Jpiece, new Color(58,94,217) },
            {BlockType.JpieceOuter, new Color(6,14,42)},
            {BlockType.Lpiece, new Color(242,147,47) },
            {BlockType.LpieceOuter, new Color(56,32,6)},
        };
    }
}