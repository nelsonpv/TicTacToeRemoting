using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using TicTacToeSharedLib;

namespace TicTacToeServer
{
    public class TicTacToeEndpoint : MarshalByRefObject, ITicTacToeEndPoint
    {
        #region ITicTacToe Members

        // GameStartEventHandler event gets fired when both the players X and 0 are ready.
        public event GameStartEventHandler GameStart;
       
        // GameOverEventHandler event gets fired when the game is Won.
        public event GameWonEventHandler GameWon;

        // GameDrawEventHandler event gets fired when the game is a Draw.
        public event GameDrawEventHandler GameDraw;

        // GameAbortEventHandler event gets fired when either of the players leaves the game in between.
        public event GameAbortEventHandler GameAbort;

        // MovedEventHandler event gets fired when either of the players successfully makes a move 
        // ie, by clicking on an empty cell.
        public event MovedEventHandler Moved;

        // 3x3 matrix
        private static bool?[,] matrix = new bool?[3, 3];

        // Flag to indicate whether it is X or 0 (x = false, 0 = true). Default value is false.
        private bool flag = false;

        // Player count.
        private short playerCount = 0;

        // Win Vector
        protected WIN_VECTOR winVector = WIN_VECTOR.NONE;

        // Needed for remoting runtime.
        public TicTacToeEndpoint(){}

        protected TicTacToeEndpoint(TicTacToeEndpoint t)
        {
            winVector = t.winVector;
            GameWon = t.GameWon;
            GameDraw = t.GameDraw;
            GameAbort = t.GameAbort;
        }

        public short PlayerCount
        {
            get { return playerCount; }
        }

        public bool?[,] Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }        

        public bool Flag
        {
            get { return flag; }
        }

        public void Register()
        {
            if (playerCount < 2)
            {
                playerCount += 1;
            }
            
            if (playerCount == 2)
            {
                // Send notification to both the players that the game can be started.
                if (GameStart != null)
                {
                    Delegate[] __d = GameStart.GetInvocationList();

                    foreach (Delegate d in __d)
                    {
                        ((GameStartEventHandler)d)();
                    }
                }
            }         
        }

        public void Move(MovedEventArgs args)
        {
            flag = !args.Value;            

            // Moved event should necessarity be executed synchronously.
            if (Moved != null)
            {
                Delegate[] __d = Moved.GetInvocationList();
                MovedEventHandler m = null;

                try
                {
                    foreach (Delegate d in __d)
                    {
                        m = (MovedEventHandler)d;
                        m(args);
                    }
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Console.WriteLine(e.Message);
                    Moved -= m;
                }                
            }
            
            GameOver(args.Value);
        }

        private void GameOver(bool value)
        {
            bool wonGame = false;

            // Horizontal
            for (int i = 0; i <= 2; i++)
            {
                if (matrix[i, 0] == value &&
                    matrix[i, 1] == value &&
                    matrix[i, 2] == value)
                {
                    if (i==0)
                        winVector = WIN_VECTOR.TOP;
                    else if (i ==1)
                        winVector = WIN_VECTOR.CENTER;
                    else if (i ==2)
                        winVector = WIN_VECTOR.BOTTOM;

                    wonGame = true;
                }
            }

            // Vertical
            if (wonGame == false)
            {
                for (int i = 0; i <= 2; i++)
                {
                    if (matrix[0, i] == value &&
                        matrix[1, i] == value &&
                        matrix[2, i] == value)
                    {
                        if (i == 0)
                            winVector = WIN_VECTOR.LEFT;
                        else if (i == 1)
                            winVector = WIN_VECTOR.MIDDLE;
                        else if (i == 2)
                            winVector = WIN_VECTOR.RIGHT;

                        wonGame = true;
                    }
                }
            }

            // Diagonal
            if (wonGame == false)
            {
                if (matrix[0, 0] == value &&
                    matrix[1, 1] == value &&
                    matrix[2, 2] == value)
                {
                    winVector = WIN_VECTOR.BACK_DIAGONAL;
                    wonGame = true;
                }
            }

            if (wonGame == false)
            {
                if (matrix[0, 2] == value &&
                    matrix[1, 1] == value &&
                    matrix[2, 0] == value)
                {
                    winVector = WIN_VECTOR.FORWARD_DIAGONAL;
                    wonGame = true;
                }
            }

            // Draw
            if (matrix[0, 0] != null &
                matrix[0, 1] != null &
                matrix[0, 2] != null &
                matrix[1, 0] != null &
                matrix[1, 1] != null &
                matrix[1, 2] != null &
                matrix[2, 0] != null &
                matrix[2, 1] != null &
                matrix[2, 2] != null &
                wonGame == false)
            {
                Delegate[] __d = GameDraw.GetInvocationList();

                foreach (Delegate d in __d)
                {
                    TicTacToeEndpointWorker worker = new TicTacToeEndpointWorker(this, d, false);

                    Thread thread = new Thread(worker.ExecuteGameDrawAsync);
                    thread.Start();
                } 

                RecycleMemory();
            }

            if (!wonGame) return;

            if (GameWon != null)
            {
                Delegate[] __d = GameWon.GetInvocationList();

                foreach (Delegate d in __d)
                {
                     TicTacToeEndpointWorker worker = new TicTacToeEndpointWorker(this, d, value);

                    Thread thread = new Thread(worker.ExecuteGameOverAsync);
                    thread.Start();
                }
            }
        }

        public void RecycleMemory()
        {
            matrix = new bool?[3, 3];
            flag = false;
            playerCount = 0;
            winVector = WIN_VECTOR.NONE;
            GameStart = null;
            GameWon = null;
            GameAbort = null;
            Moved = null;
        }

        public void AbortGame()
        {
            if (GameAbort != null)
            {
                Delegate[] __d = GameAbort.GetInvocationList();

                foreach (Delegate d in __d)
                {
                    TicTacToeEndpointWorker worker = new TicTacToeEndpointWorker(this, d, false);

                    Thread thread = new Thread(worker.ExecuteGameAbortAsync);
                    thread.Start();
                }

                RecycleMemory();
            }
        }

        #endregion
    }

    internal class TicTacToeEndpointWorker : TicTacToeEndpoint
    {
        // Parameters for async execution.
        private Delegate _del;
        private bool _value;

        public TicTacToeEndpointWorker(TicTacToeEndpoint t, Delegate del, bool value) : base(t)
        {
            _del = del;
            _value = value;
        }

        public void ExecuteGameOverAsync()
        {
            GameWonEventHandler go = null;

            try
            {
                go = (GameWonEventHandler)_del;
                go(_value, base.winVector);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                //Console.Write(e.ToString());
                base.GameWon -= go;
            }
        }

        public void ExecuteGameDrawAsync()
        {
            GameDrawEventHandler go = null;

            try
            {
                go = (GameDrawEventHandler)_del;
                go();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                //Console.Write(e.ToString());
                base.GameDraw -= go;
            }
        }

        public void ExecuteGameAbortAsync()
        {
            GameAbortEventHandler go = null;

            try
            {
                go = (GameAbortEventHandler)_del;
                go();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                //Console.Write(e.ToString());
                base.GameAbort -= go;
            }
        }
    }
}
