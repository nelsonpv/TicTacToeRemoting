using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.Remoting;
using System.Configuration;

using TicTacToeSharedLib;

namespace TicTacToe
{
    partial class frmTicTacToe : Form
    {        
        private ITicTacToeEndPoint ticTacToeEndpoint;
        TicTacToeEndpointEventWrapper ticTacToeEndpointWrapper;
        private List<MovedEventArgs> paintBucket = new List<MovedEventArgs>();
        private bool?[,] matrix;
        private bool flag;
        private WIN_VECTOR winVector = WIN_VECTOR.NONE;
        private bool isYourTurn;
        private bool isGameOver;

        public frmTicTacToe()
        {
            InitializeComponent();
        }

        #region Methods

        public void Refetch()
        { 
            ticTacToeEndpoint = (ITicTacToeEndPoint)Activator.GetObject(typeof(ITicTacToeEndPoint), (string)ConfigurationManager.AppSettings["TicTacToeServerURI"]);

            matrix = ticTacToeEndpoint.Matrix;
            flag = ticTacToeEndpoint.Flag;
        }

        private void Store(MovedEventArgs args)
        {
            int x = args.X;
            int y = args.Y;
            bool value = args.Value;

            // First
            if ((x >= 0 && x <= 100) && (y >= 0 && y <= 100))
            {
                matrix[0, 0] = value;
            }

            // Second
            else if ((x >= 100 && x <= 200) && (y >= 0 && y <= 100))
            {
                matrix[0, 1] = value;
            }

            // Third
            else if ((x >= 200 && x <= 300) && (y >= 0 && y <= 100))
            {
                matrix[0, 2] = value;
            }

            // Fourth
            else if ((x >= 0 && x <= 100) && (y >= 100 && y <= 200))
            {
                matrix[1, 0] = value;
            }

            // Fifth
            else if ((x >= 100 && x <= 200) && (y >= 100 && y <= 200))
            {
                matrix[1, 1] = value;
            }

            // Sixth
            else if ((x >= 200 && x <= 300) && (y >= 100 && y <= 200))
            {
                matrix[1, 2] = value;
            }

            // Seventh
            else if ((x >= 0 && x <= 100) && (y >= 200 && y <= 300))
            {
                matrix[2, 0] = value;
            }

            // Eight
            else if ((x >= 100 && x <= 200) && (y >= 200 && y <= 300))
            {
                matrix[2, 1] = value;
            }

            // Nineth
            else if ((x >= 200 && x <= 300) && (y >= 200 && y <= 300))
            {
                matrix[2, 2] = value;
            }

            ticTacToeEndpoint.Matrix = matrix;
        }

        private bool HasValueInCell(int x, int y)
        {
            bool? hasValueInCell = null;

            // First
            if ((x >= 0 && x <= 100) && (y >= 0 && y <= 100))
            {
                hasValueInCell = matrix[0, 0];
            }

            // Second
            else if ((x >= 100 && x <= 200) && (y >= 0 && y <= 100))
            {
                hasValueInCell = matrix[0, 1];
            }

            // Third
            else if ((x >= 200 && x <= 300) && (y >= 0 && y <= 100))
            {
                hasValueInCell = matrix[0, 2];
            }

            // Fourth
            else if ((x >= 0 && x <= 100) && (y >= 100 && y <= 200))
            {
                hasValueInCell = matrix[1, 0];
            }

            // Fifth
            else if ((x >= 100 && x <= 200) && (y >= 100 && y <= 200))
            {
                hasValueInCell = matrix[1, 1];
            }

            // Sixth
            else if ((x >= 200 && x <= 300) && (y >= 100 && y <= 200))
            {
                hasValueInCell = matrix[1, 2];
            }

            // Seventh
            else if ((x >= 0 && x <= 100) && (y >= 200 && y <= 300))
            {
                hasValueInCell = matrix[2, 0];
            }

            // Eight
            else if ((x >= 100 && x <= 200) && (y >= 200 && y <= 300))
            {
                hasValueInCell = matrix[2, 1];
            }

            // Nineth
            else if ((x >= 200 && x <= 300) && (y >= 200 && y <= 300))
            {
                hasValueInCell = matrix[2, 2];
            }

            return hasValueInCell.HasValue;
        }

        #endregion

        #region Events

        void ticTacToeEndpointWrapper_GameAbort()
        {
            MessageBox.Show("Other player left the game in between", "Tic Tac Toe");
            Application.Exit();
        }

        private void ticTacToeEndpointWrapper_GameDraw()
        {
            isGameOver = true;
            MessageBox.Show("Game is a draw.", "Tic Tac Toe");
        }

        private void ticTacToeEndpointWrapper_GameStart()
        {
            isYourTurn = true;
        }

        private void ticTacToeEndpointWrapper_GameWon(bool value, WIN_VECTOR wv)
        {
            isGameOver = true;
            winVector = wv;
            isYourTurn = false;
            this.Invalidate();
            MessageBox.Show(String.Format("{0} won the game.", (value == true ? "0" : "X")), "Tic Tac Toe");
            ticTacToeEndpoint.RecycleMemory();
        }

        private void ticTacToeEndpointWrapper_Moved(MovedEventArgs args)
        {
            paintBucket.Add(args);
            this.Invalidate();
            isYourTurn = true;
        }

        private void frmTicTacToe_Click(object sender, EventArgs e)
        {
            if (!isYourTurn)
                return;

            Refetch();

            int x = (e as MouseEventArgs).X;
            int y = (e as MouseEventArgs).Y;

            if (!HasValueInCell(x, y))
            {
                if (flag == false)
                {
                    MovedEventArgs args = new MovedEventArgs();

                    args.Value = flag;
                    args.X = x;
                    args.Y = y;
                    args.Color = Color.Blue;

                    Store(args);
                    ticTacToeEndpoint.Move(args);

                    isYourTurn = false;
                }
                else
                {
                    MovedEventArgs args = new MovedEventArgs();

                    args.Value = flag;
                    args.X = x;
                    args.Y = y;
                    args.Color = Color.Red;

                    Store(args);
                    ticTacToeEndpoint.Move(args);

                    isYourTurn = false;
                }
            }
        }

        private void frmTicTacToe_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();

            // Border
            g.DrawLine(new Pen(Color.Gray, 1), 100, 0, 100, 300);
            g.DrawLine(new Pen(Color.Gray, 1), 200, 0, 200, 300);
            g.DrawLine(new Pen(Color.Gray, 1), 0, 100, 300, 100);
            g.DrawLine(new Pen(Color.Gray, 1), 0, 200, 300, 200);

            // X and 0
            foreach (MovedEventArgs arg in paintBucket)
            {
                g.DrawString((arg.Value == true ? "0" : "X"), new Font("Arial", 30), new SolidBrush(arg.Color), arg.X - 10, arg.Y - 10);
            }

            // Win Vector
            switch ((int)winVector)
            {
                case 1:
                    g.DrawLine(new Pen(Color.Green, 4), 0, 50, 300, 50);
                    break;
                case 2:
                    g.DrawLine(new Pen(Color.Green, 4), 0, 150, 300, 150);
                    break;
                case 3:
                    g.DrawLine(new Pen(Color.Green, 4), 0, 250, 300, 250);
                    break;
                case 4:
                    g.DrawLine(new Pen(Color.Green, 4), 50, 0, 50, 300);
                    break;
                case 5:
                    g.DrawLine(new Pen(Color.Green, 4), 150, 0, 150, 300);
                    break;
                case 6:
                    g.DrawLine(new Pen(Color.Green, 4), 250, 0, 250, 300);
                    break;
                case 7:
                    g.DrawLine(new Pen(Color.Green, 4), 0, 0, 300, 300);
                    break;
                case 8:
                    g.DrawLine(new Pen(Color.Green, 4), 0, 300, 300, 0);
                    break;
                default:
                    break;
            }
        }

        private void frmTicTacToe_Load(object sender, EventArgs e)
        {
            RemotingConfiguration.Configure("TicTacToe.exe.config", true);

            Refetch();

            ticTacToeEndpointWrapper = new TicTacToeEndpointEventWrapper();

            ticTacToeEndpoint.Moved += new MovedEventHandler(ticTacToeEndpointWrapper.MovedEventHandlerWrapper);
            ticTacToeEndpoint.GameWon += new GameWonEventHandler(ticTacToeEndpointWrapper.GameWonEventHandlerWrapper);
            ticTacToeEndpoint.GameStart += new GameStartEventHandler(ticTacToeEndpointWrapper.GameStartEventHandlerWrapper);
            ticTacToeEndpoint.GameDraw += new GameDrawEventHandler(ticTacToeEndpointWrapper.GameDrawEventHandlerWrapper);
            ticTacToeEndpoint.GameAbort += new GameAbortEventHandler(ticTacToeEndpointWrapper.GameAbortEventHandlerWrapper);

            ticTacToeEndpointWrapper.Moved += new MovedEventHandler(ticTacToeEndpointWrapper_Moved);
            ticTacToeEndpointWrapper.GameWon += new GameWonEventHandler(ticTacToeEndpointWrapper_GameWon);
            ticTacToeEndpointWrapper.GameStart += new GameStartEventHandler(ticTacToeEndpointWrapper_GameStart);
            ticTacToeEndpointWrapper.GameDraw += new GameDrawEventHandler(ticTacToeEndpointWrapper_GameDraw);
            ticTacToeEndpointWrapper.GameAbort += new GameAbortEventHandler(ticTacToeEndpointWrapper_GameAbort);

            if (ticTacToeEndpoint.PlayerCount < 2)
            {
                ticTacToeEndpoint.Register();
            }
            else
            {
                ticTacToeEndpoint.Moved -= new MovedEventHandler(ticTacToeEndpointWrapper.MovedEventHandlerWrapper);
                ticTacToeEndpoint.GameWon -= new GameWonEventHandler(ticTacToeEndpointWrapper.GameWonEventHandlerWrapper);
                ticTacToeEndpoint.GameStart -= new GameStartEventHandler(ticTacToeEndpointWrapper.GameStartEventHandlerWrapper);
                ticTacToeEndpoint.GameDraw -= new GameDrawEventHandler(ticTacToeEndpointWrapper.GameDrawEventHandlerWrapper);
                ticTacToeEndpoint.GameAbort -= new GameAbortEventHandler(ticTacToeEndpointWrapper.GameAbortEventHandlerWrapper);

                ticTacToeEndpointWrapper.Moved -= new MovedEventHandler(ticTacToeEndpointWrapper_Moved);
                ticTacToeEndpointWrapper.GameWon -= new GameWonEventHandler(ticTacToeEndpointWrapper_GameWon);
                ticTacToeEndpointWrapper.GameStart -= new GameStartEventHandler(ticTacToeEndpointWrapper_GameStart);
                ticTacToeEndpointWrapper.GameDraw -= new GameDrawEventHandler(ticTacToeEndpointWrapper_GameDraw);
                ticTacToeEndpointWrapper.GameAbort -= new GameAbortEventHandler(ticTacToeEndpointWrapper_GameAbort);

                MessageBox.Show("Only two players are allowed", "Tic Tac Toe");
                Application.Exit();
            }
        }

        private void frmTicTacToe_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isGameOver && e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("Do you want to leave the game?", "Tic Tac Toe", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Unsubscribe event notification.
                    ticTacToeEndpoint.GameAbort -= new GameAbortEventHandler(ticTacToeEndpointWrapper.GameAbortEventHandlerWrapper);
                    ticTacToeEndpointWrapper.GameAbort -= new GameAbortEventHandler(ticTacToeEndpointWrapper_GameAbort);

                    ticTacToeEndpoint.AbortGame();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion
    }
}