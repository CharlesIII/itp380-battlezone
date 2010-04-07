using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

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
        private const int EXPECTED_NUMBER_OF_TOKENS = 2;
        private const int EXPECTED_TOKEN_LENGTH = 3;

        StreamReader fileReader;
        Hashtable graph;    //key-value pair between "xyz" and Vertex objects

        /// <summary>
        /// Constructs a new pathfinder object using the given input file for generating the graph with.
        /// </summary>
        /// <param name="fileName">Text file containing the data to generate a graph</param>
        public PathFinder(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File not found.",fileName);
                //Console.WriteLine("not found");
                //Console.WriteLine(Directory.GetCurrentDirectory());
            }
            else
            {
                graph = new Hashtable(10);

                //parse input file to build graph
                fileReader = File.OpenText(fileName);
                string input;
                string[] tokens;
                while ((input = fileReader.ReadLine()) != null)
                {
                    tokens = input.Split(' ');
                    if (tokens.Length != EXPECTED_NUMBER_OF_TOKENS)
                        throw new Exception("Input file is incorrectly formatted.");
                    else
                    {
                        string pos1 = tokens[0];
                        string pos2 = tokens[1];

                        if (pos1.Length != EXPECTED_TOKEN_LENGTH || pos2.Length != EXPECTED_TOKEN_LENGTH)
                            throw new Exception("Input file is incorrectly formatted.");
                        else
                        {
                            Vertex firstVertex;
                            Vertex secondVertex;

                            if (graph.ContainsKey(pos1))
                            {
                                firstVertex = (Vertex)graph[pos1];
                            }
                            else
                            {
                                firstVertex = new Vertex(float.Parse(pos1[0].ToString()), float.Parse(pos1[1].ToString()), float.Parse(pos1[2].ToString()));
                                graph.Add(pos1, firstVertex);
                            }

                            if (graph.ContainsKey(pos2))
                            {
                                secondVertex = (Vertex)graph[pos2];
                            }
                            else
                            {
                                secondVertex = new Vertex(float.Parse(pos2[0].ToString()), float.Parse(pos2[1].ToString()), float.Parse(pos2[2].ToString()));
                                graph.Add(pos2, secondVertex);
                            }

                            //establish link between both vertice
                            firstVertex.connectedVertices.Add(secondVertex);
                            secondVertex.connectedVertices.Add(firstVertex);
                        }
                    }
                }
                Console.WriteLine("Graph successfully generated.");
            }
        }

        //compute heuristic as search progress
        //straight line distance from beginning node to current node + straight line distance from current node to target node
        private class Vertex
        {
            public ArrayList connectedVertices;
            public Vector3 position;

            /// <summary>
            /// Constructs a Vertex with the given position in World Space.
            /// </summary>
            /// <param name="x">x coordinate in world space.</param>
            /// <param name="y">y coordinate in world space.</param>
            /// <param name="z">z coordinate in world space.</param>
            public Vertex(float x, float y, float z)
            {
                position = new Vector3(x, y, z);
                connectedVertices = new ArrayList(10);
            }
        }
    }
}
