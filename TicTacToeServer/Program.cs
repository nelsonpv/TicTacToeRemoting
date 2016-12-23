using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;

namespace TicTacToeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RemotingConfiguration.Configure("TicTacToeServer.exe.config", true);
                Console.WriteLine("Tic Tac Toe Game Server is listening for requests. Press Enter to exit...");
                Console.ReadLine();
            }
            catch (RemotingException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }
    }
}
