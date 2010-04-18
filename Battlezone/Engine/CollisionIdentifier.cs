using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battlezone
{
    class CollisionIdentifier
    {
        public static int NONCOLLIDING = -1;
        public static int ASTEROID = 0;
        public static int PLAYER_TANK = 1;
        public static int AI_TANK = 2;
        public static int SHELL = 3;
        public static int BUILDING = 4;
    }
}
