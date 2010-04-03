using System;

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
        }
    }
}

