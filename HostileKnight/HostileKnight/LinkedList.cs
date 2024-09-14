//A: Evan Glaizel
//F: LinkedList.cs
//P: HostileKnight
//C: 2022/12/4
//M: 2022/12/5
//D: The linked list that manages the rooms of the game

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostileKnight
{
    class LinkedList
    {
        //Store the first node in the linked list
        private Node head;

        //Store the total amount of nodes in the linked list
        private int size;

        //Pre: N/A
        //Post: N/A
        //Desc: Constructs the linked list
        public LinkedList()
        {
        }

        //Pre: newNode is the node to add to the tail
        //Post: N/A
        //Desc: Adds a new node to the tail
        public void AddToTail(Node newNode)
        {
            //Add the node to the head if the linked list is empty
            if (size == 0)
            {
                //Set the head node
                head = newNode;
            }
            else
            {
                //Store the node to loop through the linked list to add the new node behind
                Node testNode = head;

                //Loop the testNode through the linked list, until it is the last one
                while (testNode.GetNext() != null)
                {
                    //Incriment the testNode to the next node in the list
                    testNode = testNode.GetNext();
                }

                //Add the new node after the last one
                testNode.SetNext(newNode);
            }

            //Increase the size of the node
            size++;
        }

        //Pre: N/A
        //Post: Return the head of the linked list
        //Desc: Returns the head of the linked list
        public Node GetHead()
        {
            //Return the head of the linked list
            return head;
        }
    }
}
