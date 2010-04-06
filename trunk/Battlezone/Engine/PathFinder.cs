using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Battlezone.Engine
{
    /// <summary>
    /// This class implements A* for pathfinding. It handles the loading and generation of a graph from a text file
    /// with the following format:
    /// "xyz xyz"
    /// where xyz are coordinates for navigation nodes in world space. Two nodes on a line indicate that an edge 
    /// exists between those nodes.
    /// </summary>
    public class PathFinder
    {
        FileStream fileReader;

        /// <summary>
        /// Constructs a new pathfinder object using the given input file for generating the graph with.
        /// </summary>
        /// <param name="fileName">Text file containing the data to generate a graph</param>
        public PathFinder(string fileName)
        {

        }
    }
}
