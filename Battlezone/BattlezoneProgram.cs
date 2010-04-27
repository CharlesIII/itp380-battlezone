#region File Description
//------------------------------------------------------------------------------
// BattlezonProgram.cs
//
// BATTLEZONE ENTRY POINT
//
// Copyright (C) Double X.L., Graham Cracka, Old Jamison Irish Whiskey, & C-Cubed.
// All rights reserved.
//------------------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using Microsoft.Xna.Framework;
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
            
            //A* test code
            /*
            PathFinder pf = new PathFinder(@"..\..\..\BattlezoneObjects\Navigation Nodes.txt");
            ArrayList nodes = pf.GetNavigationNodes();
            ArrayList path = pf.GetPath((Vector3)nodes[0], (Vector3)nodes[2]);
            foreach (Vector3 v in path)
            {
                Console.WriteLine(v.ToString());
            }
            */
        }
    }
}

