//A: Evan Glaizel
//F: Room.cs
//P: HostileKnight
//C: 2022/12/15
//M: 
//D: Keeps track of each tile for pathfinding purposes

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostileKnight
{
    class PathFindingNode
    {

        //Store the different node costs
        private int fCost;
        private int gCost;
        private int hCost;

        //Store the parent of the node
        private PathFindingNode parent;

        //Store the node data
        private int row;
        private int col;
        private bool collidable;

        //Store the adjacent nodes
        private List<PathFindingNode> adjacentNodes = new List<PathFindingNode>();

        //Pre: row and col are the row and coluoum of the node in relation to the 2D array, collidable represents if the node is something the enemy can pathfind through
        //Post: N/A
        //Desc: Construct the node for path finding
        public PathFindingNode(int row, int col, bool collidable)
        {
            //Set the node data
            this.row = row;
            this.col = col;
            this.collidable = collidable;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Recalculates the f cost
        public void CalcFCost()
        {
            //Recalculate the f cost
            fCost = gCost + hCost;
        }

        //Pre: endNode is the final node in the path
        //Post: N/A
        //Desc: Calculates the h cost of the node to the end of the path
        public void CalcHCost(PathFindingNode endNode)
        {
            //Calculate the h cost between the node and the end node
            hCost = 10 * (Math.Abs(col - endNode.GetCol()) + Math.Abs(row - endNode.GetRow()));
        }

        //Pre: newGCost is the new gCost to set
        //Post: N/A
        //Desc: Calculates the g cost of the node
        public void SetGCost(int newGCost)
        {
            //Set the gCost to the new gCost
            gCost = newGCost;
        }

        //Pre: prevNoce is the previous node connecting to this one
        //Post: Return the possible g cost of the node
        //Desc: Calculates the g cost of the node without setting it
        public int CalcGCost(PathFindingNode prevNode)
        {
            //Store the new G cost
            int newGCost = 0;

            //Set the G cost based on its position in relation to its parent
            if (prevNode.GetCol() != col && prevNode.GetRow() != row)
            {
                //Add the diagonal cost to the node
                newGCost += 14;
            }
            else
            {
                //Add the horizontal or vertical cost to the node
                newGCost += 10;
            }

            //Add the parents g cost to the cost
            newGCost += prevNode.GetGCost();

            return newGCost;
        }

        //Pre: newParent is the new value to set the parent to
        //Post: N/A
        //Desc: Sets the nodes parent
        public void SetParent(PathFindingNode newParent)
        {
            //Set the new parent
            parent = newParent;
        }

        //Pre: N/A
        //Post: Return the row of the node
        //Desc: Get and return the row of the node
        public int GetRow()
        {
            //Return the row of the node
            return row;
        }

        //Pre: N/A
        //Post: Return the column of the node
        //Desc: Get and return the column of the node
        public int GetCol()
        {
            //Return the column of the node
            return col;
        }

        //Pre: N/A
        //Post: Return the f cost of the node
        //Desc: Get and return the f cost of the node
        public int GetFCost()
        {
            //Return the f cost of the node
            return fCost;
        }

        //Pre: N/A
        //Post: Return the g cost of the node
        //Desc: Get and return the g cost of the node
        public int GetGCost()
        {
            //Return the g cost of the node
            return gCost;
        }

        //Pre: N/A
        //Post: Return the parent of the node
        //Desc: Get and return the parent of the node
        public PathFindingNode GetParent()
        {
            //Return the parent of the node
            return parent;
        }

        //Pre: N/A
        //Post: Return if the node is collidable
        //Desc: Get and return the collidable state of the node
        public bool IsCollidable()
        {
            //Return the collidable state of the node
            return collidable;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Resets the g cost
        public void ResetGCost()
        {
            //Resets the g cost
            gCost = 0;
        }

        //Pre: nodeMap is a 2D array of all nodes
        //Post: N/A
        //Desc: Set all the adjacent nodes 
        public void SetAdjacentNodes(PathFindingNode[,] nodeMap)
        {
            //Loop through the rows of the nodes directly adjacent to the current node
            for (int i = col - 1; i < col + 2; i++)
            {
                //Loop through the column of the nodes directly adjacent to the current node
                for (int j = row - 1; j < row + 2; j++)
                {
                    //Set the adjacent node if the node in the node is not out of bounds, and not the current node
                    if (!(i == col && j == row) && i >= 0 && i < nodeMap.GetLength(0) && j >= 0 && j < nodeMap.GetLength(1))
                    {
                        //Add the adjacent node to the list if the node isn't collidable
                        if (!nodeMap[i, j].IsCollidable())
                        {
                            //Add the adjacent node to the list
                            adjacentNodes.Add(nodeMap[i, j]);
                        }
                    }
                }
            }
        }

        //Pre: N/A
        //Post: Return the list of adjacent nodes
        //Desc: Returns the list of adjacent nodes to continue finding the path
        public List<PathFindingNode> GetAdjacentNodes()
        {
            //Return the list of adjacent nodes
            return adjacentNodes;
        }
    }
}
