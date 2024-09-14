//A: Evan Glaizel
//F: LinkedList.cs
//P: HostileKnight
//C: 2022/12/4
//M: 2022/12/4
//D: The node of a linked list that manages a room of the game in the linked list

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostileKnight
{
    class Node
    {
        //Store the next node in the linked list
        private Node next;

        //Store the room of the linked list
        private Room room;

        //Pre: room is the cargo for the node
        //Post: N/A
        //Desc: Constructs the node
        public Node(Room room)
        {
            //Set the room of the node
            this.room = room;
        }

        //Pre: newNode is the node to set the next node to
        //Post: N/A
        //Desc: Sets the next node of the node
        public void SetNext(Node newNode)
        {
            //Set the next node
            next = newNode;
        }

        //Pre: N/A
        //Post: Return the next node in the list
        //Desc: Returns the next node in the linked list
        public Node GetNext()
        {
            //Return the next node in the list
            return next;
        }
        
        //Pre: N/A
        //Post: Return the room of the node
        //Desc: Returns the cargo of the node
        public Room GetCargo()
        {
            //Return the room of the node
            return room;
        }
    }
}
