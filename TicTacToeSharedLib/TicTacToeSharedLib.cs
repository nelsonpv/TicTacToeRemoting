using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TicTacToeSharedLib
{
    public delegate void GameStartEventHandler();
    public delegate void GameDrawEventHandler();
    public delegate void GameWonEventHandler(bool value, WIN_VECTOR winVector);
    public delegate void GameAbortEventHandler();
    public delegate void MovedEventHandler(MovedEventArgs args);

    public interface ITicTacToeEndPoint
    {
        event GameStartEventHandler GameStart;
        event GameDrawEventHandler GameDraw;
        event GameWonEventHandler GameWon;
        event GameAbortEventHandler GameAbort;
        event MovedEventHandler Moved;

        void Move(MovedEventArgs args);
        void Register();
        void RecycleMemory();
        void AbortGame();

        bool Flag { get; }
        bool?[,] Matrix { get; set; }
        short PlayerCount { get; }
    }

    public class TicTacToeEndpointEventWrapper : MarshalByRefObject
    {
        #region TicTacToeEndpointEventWrapper Members

        public event MovedEventHandler Moved;
        public event GameWonEventHandler GameWon;
        public event GameStartEventHandler GameStart;
        public event GameDrawEventHandler GameDraw;
        public event GameAbortEventHandler GameAbort;

        public void MovedEventHandlerWrapper(MovedEventArgs args)
        {
            if (Moved != null) Moved(args);
        }

        public void GameWonEventHandlerWrapper(bool value, WIN_VECTOR winVector)
        {
            if (GameWon != null) GameWon(value, winVector);
        }

        public void GameStartEventHandlerWrapper()
        {
            if (GameStart != null) GameStart();
        }

        public void GameDrawEventHandlerWrapper()
        {
            if (GameDraw != null) GameDraw();
        }

        public void GameAbortEventHandlerWrapper()
        {
            if (GameAbort != null) GameAbort();
        }

        #endregion
    }

    [Serializable]
    public class MovedEventArgs
    {
        private int x;
        private int y;
        private bool value;
        private Color color;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public bool Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }
    }

    [Serializable]
    public enum WIN_VECTOR
    {
        NONE = 0,
        TOP = 1,
        CENTER = 2,
        BOTTOM = 3,
        LEFT = 4,
        MIDDLE = 5,
        RIGHT = 6,
        BACK_DIAGONAL = 7,
        FORWARD_DIAGONAL = 8,
    }
}
