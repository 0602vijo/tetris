
namespace Raylib_Tetris
{
    //implementation of https://harddrop.com/wiki/Random_Generator
    public class BagHold
    {
        internal int selected_bag = 0;
        internal int index = 0;
        internal BlockType[] bag1 = new BlockType[7];
        internal BlockType[] bag2 = new BlockType[7];
        public static BlockType[] displayed_blocks = new BlockType[6];
        public static BlockType held_piece = BlockType.Empty;
        public static BlockType active_piece = BlockType.Empty;
        internal bool allow_hold = true;
        public void Advance()
        {
            active_piece = ActiveBag()[index];
            ActiveBag()[index] = BlockType.Empty;
            index++;
            displayed_blocks = DisplayedBlocks();
        }
        public void Hold()
        {
            if (allow_hold)
            {
                if (held_piece == BlockType.Empty)
                {
                    if(index >= 7)
                    {
                        SwitchBag();
                        index = 0;
                    }
                    held_piece = active_piece;
                    active_piece = ActiveBag()[index];
                    Advance();
                }
                else if (held_piece != BlockType.Empty)
                {
                    BlockType temp_block = held_piece;
                    held_piece = active_piece;
                    active_piece = temp_block;
                }
                allow_hold = false;
                NewActivePos();
            }
            displayed_blocks = DisplayedBlocks();
        }
        public virtual void HardDrop()
        {
            if(index >= 7)
            {
                SwitchBag();
                index = 0;
            }
            active_piece = ActiveBag()[index];
            Advance();
            allow_hold = true;
            NewActivePos();
        }
        public void NewActivePos()
        {
            switch (active_piece)
            {
                case BlockType.Opiece:
                    GameData.position.X = 4;
                    GameData.position.Y = 19;
                    break;
                case BlockType.Ipiece:
                    GameData.position.X = 3;
                    GameData.position.Y = 17;
                    break;
                default:
                    GameData.position.X = 3;
                    GameData.position.Y = 18;
                    break;
            }
        }
        public BlockType[] DisplayedBlocks()
        {
            BlockType[] displayBlocks = new BlockType[6];
            for (int i = 0; i < 5; i++)
            {
                if((i + index) < 7)
                {
                    displayBlocks[i] = ActiveBag()[i + index];
                }
                else
                {
                    displayBlocks[i] = InactiveBag()[i + index - 7];
                }
            }
            displayBlocks[5] = held_piece;
            return displayBlocks;
        }
        public void InitBags()
        {
            RandomizeBag(bag1);
            RandomizeBag(bag2);
            displayed_blocks = DisplayedBlocks();
            Advance();
            NewActivePos();
        }
        BlockType[] InactiveBag()
        {
            if (selected_bag == 0) 
                return bag2;
            else 
                return bag1;
        }
        BlockType[] ActiveBag()
        {
            if (selected_bag == 0)
                return bag1;
            else
                return bag2;
        }
        void SwitchBag()
        {
            if (selected_bag == 0)
                selected_bag = 1;
            else selected_bag = 0;
            RandomizeBag(InactiveBag());
        }
        BlockType[] RandomizeBag(BlockType[] bag)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            BlockType[] blocks = Enum.GetValues<BlockType>();
            BlockType[] temp_blocks = new BlockType[7];
            for (int i = 0; i < 7; i++)
            {
                bag[i] = BlockType.Empty;
            }
            int temp;
            for(int i = 0; i < 7; i++)
            {
                temp = rand.Next(0, 7);
                if (bag.Contains(blocks[temp+4]))
                {
                    i--;
                }
                else
                {
                    bag[i] = blocks[temp+4];
                }
            }
            return bag;
        }
    }
}