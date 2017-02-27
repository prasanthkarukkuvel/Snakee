using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snakee
{
    public enum CharacterAttributes
    {
        FOREGROUND_BLUE = 0x0001,
        FOREGROUND_GREEN = 0x0002,
        FOREGROUND_RED = 0x0004,
        FOREGROUND_INTENSITY = 0x0008,
        BACKGROUND_BLUE = 0x0010,
        BACKGROUND_GREEN = 0x0020,
        BACKGROUND_RED = 0x0040,
        BACKGROUND_INTENSITY = 0x0080,
        COMMON_LVB_LEADING_BYTE = 0x0100,
        COMMON_LVB_TRAILING_BYTE = 0x0200,
        COMMON_LVB_GRID_HORIZONTAL = 0x0400,
        COMMON_LVB_GRID_LVERTICAL = 0x0800,
        COMMON_LVB_GRID_RVERTICAL = 0x1000,
        COMMON_LVB_REVERSE_VIDEO = 0x4000,
        COMMON_LVB_UNDERSCORE = 0x8000
    }

    public enum ForegroundColors
    {
        BLACK = 0,
        DARKBLUE = CharacterAttributes.FOREGROUND_BLUE,
        DARKGREEN = CharacterAttributes.FOREGROUND_GREEN,
        DARKCYAN = CharacterAttributes.FOREGROUND_GREEN | CharacterAttributes.FOREGROUND_BLUE,
        DARKRED = CharacterAttributes.FOREGROUND_RED,
        DARKMAGENTA = CharacterAttributes.FOREGROUND_RED | CharacterAttributes.FOREGROUND_BLUE,
        DARKYELLOW = CharacterAttributes.FOREGROUND_RED | CharacterAttributes.FOREGROUND_GREEN,
        DARKGRAY = CharacterAttributes.FOREGROUND_RED | CharacterAttributes.FOREGROUND_GREEN | CharacterAttributes.FOREGROUND_BLUE,
        GRAY = CharacterAttributes.FOREGROUND_INTENSITY,
        BLUE = CharacterAttributes.FOREGROUND_INTENSITY | CharacterAttributes.FOREGROUND_BLUE,
        GREEN = CharacterAttributes.FOREGROUND_INTENSITY | CharacterAttributes.FOREGROUND_GREEN,
        CYAN = CharacterAttributes.FOREGROUND_INTENSITY | CharacterAttributes.FOREGROUND_GREEN | CharacterAttributes.FOREGROUND_BLUE,
        RED = CharacterAttributes.FOREGROUND_INTENSITY | CharacterAttributes.FOREGROUND_RED,
        MAGENTA = CharacterAttributes.FOREGROUND_INTENSITY | CharacterAttributes.FOREGROUND_RED | CharacterAttributes.FOREGROUND_BLUE,
        YELLOW = CharacterAttributes.FOREGROUND_INTENSITY | CharacterAttributes.FOREGROUND_RED | CharacterAttributes.FOREGROUND_GREEN,
        WHITE = CharacterAttributes.FOREGROUND_INTENSITY | CharacterAttributes.FOREGROUND_RED | CharacterAttributes.FOREGROUND_GREEN | CharacterAttributes.FOREGROUND_BLUE,
    };

    public enum Movement
    {
        TOP, BOTTOM, LEFT, RIGHT
    }

    public static class Extentions
    {
        public static void FeedbackEach<T>(this IEnumerable<T> Sequence, Action<T, T> Action)
        {
            T Prev = default(T); 
            foreach (T Item in Sequence)
            {
                Action(Prev, Item);
                Prev = Item;
            }
        }
    }

    abstract class Skeleton
    {
        public short Top { get; private set; }
        public short Left { get; private set; }
        public Movement Movement { get; private set; }
        public abstract int SkeletonBlock { get; }

        public Skeleton(short Top, short Left, Movement Movement)
        {
            this.Top = Top;
            this.Left = Left;
            this.Movement = Movement;
        }

        public Skeleton Move(Movement Movement)
        {
            if (((this.Movement == Movement.TOP || this.Movement == Movement.BOTTOM) && !(Movement == Movement.TOP || Movement == Movement.BOTTOM)) || !(Movement == Movement.LEFT || Movement == Movement.RIGHT))
            {
                this.Movement = Movement;
            }

            switch (this.Movement)
            {
                case Movement.TOP: Left--; break;
                case Movement.BOTTOM: Left++; break;
                case Movement.LEFT: Top--; break;
                case Movement.RIGHT: Top++; break;
            }

            return this;
        }
    }


    class SnakeBody : Skeleton
    {
        public override int SkeletonBlock => 219;

        public SnakeBody(short Top, short Left, Movement Movement) : base(Top, Left, Movement)
        {

        }
    }

    class Snake
    {
        public List<SnakeBody> Skeletons { get; private set; } = new List<SnakeBody>();

        public Snake()
        {
            Skeletons.Add(new SnakeBody(10, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(9, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(8, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(7, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(6, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(5, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(4, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(3, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(2, 1, Movement.RIGHT));
            Skeletons.Add(new SnakeBody(1, 1, Movement.RIGHT));
        }

        public void Move(Movement Movement)
        {
            var LastMovement = Movement;
            Skeletons.FeedbackEach((Prev, Skeleton) =>
            {
                var CurrentMovement = Skeleton.Movement;
                Skeleton.Move(LastMovement);
                LastMovement = CurrentMovement;
            });
        }
    }

    class Board
    {
        #region Native methods

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
             IntPtr template);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FillConsoleOutputAttribute(
            SafeFileHandle hConsoleOutput,
            ForegroundColors lpCharacter,
            int nLength,
            Coord dwWriteCoord,
            ref int lpumberOfCharsWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutputCharacter(
            SafeFileHandle hConsoleOutput,
            string lpCharacter,
            int nLength,
            Coord dwWriteCoord,
            ref int lpumberOfCharsWritten);

        #endregion

        public const short BOARD_WIDTH = 150;
        public const short BOARD_HEIGHT = 40;
        private const int FULL_BLOCK = 219;
        private const int EMPTY_BLOCK = 255;
        private static int RefHandle;
        private Snake Snake { get; set; }

        public Board(Snake Snake)
        {
            this.Snake = Snake;
            Plot();
        }

        public static void DrawBoundary(ForegroundColors Color) => TravelBoundary((i, j) => PutBlock(i, j, Color));
        private static void TravelBoundary(Action<short, short> Callback) => TravelBoard((i, j) => (i == 0 || i == BOARD_WIDTH - 1 || j == 0 || j == BOARD_HEIGHT - 1), Callback);
        private static void TravelBoard(Func<short, short, bool> Predicate, Action<short, short> Callback, short[] TopLeft = null, short[] BottomRight = null)
        {
            if (TopLeft == null)
            {
                TopLeft = new short[2] { 0, 0 };
            }

            if (BottomRight == null)
            {
                BottomRight = new short[2] { BOARD_WIDTH, BOARD_HEIGHT };
            }

            for (short i = TopLeft[0]; i < BottomRight[0]; i++)
            {
                for (short j = TopLeft[1]; j < BottomRight[1]; j++)
                {
                    if (Predicate(i, j))
                    {
                        Callback(i, j);
                    }
                }
            }
        }
        private static void PutBlock(short Top, short Left, ForegroundColors Color) => WriteCharacter(Top, Left, Color, FULL_BLOCK);
        private static void WriteCharacter(short Top, short Left, ForegroundColors Color, int Block)
        {
            SafeFileHandle ConsoleHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            FillConsoleOutputAttribute(ConsoleHandle, Color, 1, new Coord(Top, Left), ref RefHandle);
            WriteConsoleOutputCharacter(ConsoleHandle, new string((char)Block, 1), 1, new Coord(Top, Left), ref RefHandle);
        }

        public void putSnake(Movement Movement)
        {
            Skeleton TailSkeleton = Snake.Skeletons.LastOrDefault();

            if (TailSkeleton != null)
            {               
                WriteCharacter(TailSkeleton.Top, TailSkeleton.Left, ForegroundColors.WHITE, EMPTY_BLOCK);
            }

            Snake.Move(Movement);
            Plot();
        }

        public void Plot()
        {
            Snake.Skeletons.ForEach(Skeleton => WriteCharacter(Skeleton.Top, Skeleton.Left, ForegroundColors.WHITE, Skeleton.SkeletonBlock));
        }
    }

    class Program
    {

        #region Native methods

        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        #endregion

        static void Main(string[] args)
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_SIZE, MF_BYCOMMAND);

            Console.WindowWidth = Board.BOARD_WIDTH;
            Console.WindowHeight = Board.BOARD_HEIGHT;
            Console.BufferWidth = Board.BOARD_WIDTH;
            Console.BufferHeight = Board.BOARD_HEIGHT;
            Console.CursorVisible = false;

            Snake Snake = new Snake();
            Board SnakeBoard = new Board(Snake);

            for (int i = 0; i < 40; i++)
            {
                SnakeBoard.putSnake(Movement.RIGHT);
                Thread.Sleep(250);
                SnakeBoard.putSnake(Movement.BOTTOM);
                Thread.Sleep(250);
                SnakeBoard.putSnake(Movement.BOTTOM);
                Thread.Sleep(250);
                SnakeBoard.putSnake(Movement.RIGHT);
                Thread.Sleep(250);
                SnakeBoard.putSnake(Movement.RIGHT);
                Thread.Sleep(250);
            }

            Console.Read();
        }
    }
}
