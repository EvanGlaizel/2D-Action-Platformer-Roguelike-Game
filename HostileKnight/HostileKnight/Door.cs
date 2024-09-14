//A: Evan Glaizel
//F: Door.cs
//P: HostileKnight
//C: 2022/12/5
//M: 2022/12/06
//D: The door that blocks the entrance and exit between levels

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HostileKnight
{
    class Door : Tile
    {
        //Store the image of the door
        private Texture2D img;

        //Store the starting location of the door
        private Vector2 startLoc;

        //Store the draw location of the door
        private Vector2 drawLoc;

        //Store the orientation of the door
        private bool vertical;

        //Store if the door is the end door
        private bool isEndDoor;

        //Store the speed to open the door
        private int openSpeed;

        //Pre: tileLocs is the location of all the tiles in the door, img is the image of the door, vertical stores if the door is vertical or horizontal, and isEndDoor stores if the door is the end door
        //Post: N/A
        //Desc: Constructs the door
        public Door(List<Vector2> tileLocs, List<Texture2D> img, bool vertical, bool isEndDoor) : base(tileLocs, img)
        {
            //Set the image, orientation, and door type
            this.img = img[0];
            this.vertical = vertical;
            this.isEndDoor = isEndDoor;

            //Set the draw and start location
            startLoc = tileLocs[0];
            drawLoc = tileLocs[0];

            //Set the friction and speed multipliers
            frictionMultiplier = 1f;
            speedMultiplier = 1f;

            //Set the open speed of the door
            openSpeed = 5;

            //Sets the hitbox of the door
            SetHitBox();
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Constructs the hitbox of the door
        protected override void SetHitBox()
        {
            //Create the hitbox of the door
            hitBox = new Rectangle((int)startLoc.X, (int)startLoc.Y, img.Width, img.Height);
        }

        //Pre: N/A
        //Post: return a value based on if the door is closed
        //Post: Opens the door
        public bool OpenDoor()
        {
            //Change the direction of opening based on the direction of the door
            if (vertical)
            {
                //Move the door and tile location up to open it
                hitBox.Y += openSpeed;
                drawLoc.Y += openSpeed;

                //Return true if the door has been opened
                if (Math.Abs(startLoc.Y - drawLoc.Y) > img.Height)
                {
                    //The door has been fully opened
                    return false;
                }
            }
            else
            {
                //Move the door right to open it
                hitBox.X -= openSpeed;
                drawLoc.X -= openSpeed;

                //Return true if the door has been opened
                if (Math.Abs(startLoc.X - drawLoc.X) > img.Width)
                {
                    //The door has been fully opened
                    return false;
                }
            }

            //The door has not been fully opened yet
            return true;
        }

        //Pre: N/A
        //Post: Return if the door is vertical or not
        //Desc: Returns the orientation of the door
        public bool IsVertical()
        {
            //Return true if the door is vertical
            return vertical;
        }

        //Pre: N/A
        //Post: Return if the door is the end door or not
        //Desc: Returns the door type
        public bool IsEndDoor()
        {
            //Return true if the door is the end door
            return isEndDoor;
        }

        //Pre: spriteBatch is what allows the tile to be drawn, and transparancy is how transparent to draw the door
        //Post: N/A
        //Desc: Draws the door to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the door to the screen
            spriteBatch.Draw(img, drawLoc, Color.White * transparancy);
        }
    }
}
