using System;
using Battlezone.Engine;

namespace Battlezone
{
    static class BattlezoneProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            
            using (BattlezoneGame game = new BattlezoneGame())
            {
                game.Run();
            }
             
            //PathFinder pf = new PathFinder(@"..\..\..\BattlezoneObjects\Navigation Nodes.txt");
        }
    }
}

