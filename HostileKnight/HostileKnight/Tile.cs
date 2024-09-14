//A: Evan Glaizel
//F: Tile.cs
//P: HostileKnight
//C: 2022/12/5
//M: 2022/12/06
//D: The tiles of the game. This is the basis of the level

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HostileKnight
{
    class Tile
    {
        //Store each tile locations of the tile, and the image to draw at that tile
        protected List<Vector2> tileLocs;
        protected List<Texture2D> imgs;

        //Store the hitbox of the tiles
        protected Rectangle hitBox;

        //Store the friction and speed multipler to give the player when they interact with the tile
        protected float frictionMultiplier;
        protected float speedMultiplier;

        //Pre: tileLocs is a list of the location of each of the tiles, and imgs is the images of the tile
        //Post: N/A
        //Desc: Construct the tile
        public Tile(List<Vector2> tileLocs, List<Texture2D> imgs)
        {
            //Set the tile locations
            this.tileLocs = tileLocs;
            this.imgs = imgs;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Constructs the hitbox of the tile
        protected virtual void SetHitBox()
        {
            //Create the hitbox of the tile
            hitBox = new Rectangle((int)tileLocs[0].X, (int)tileLocs[0].Y, (int)(tileLocs[tileLocs.Count - 1].X + imgs[0].Width - tileLocs[0].X), (int)(tileLocs[tileLocs.Count - 1].Y + imgs[0].Height - tileLocs[0].Y));
        }

        //Pre: N/A
        //Post: Returns the hitbox of the tile
        //Desc: Returns the hitbox of the tile for colission purposes
        public Rectangle GetHitBox()
        {
            //Return the hitbox of the tile
            return hitBox;
        }

        //Pre: N/A
        //Post: Returns the friction multiplier of the tile
        //Desc: Returns the friction multiplier of the tile for player friction purposes
        public float GetFrictionMultiplier()
        {
            //Return the friction multiplier of the tile
            return frictionMultiplier;
        }

        //Pre: N/A
        //Post: Returns the speed multiplier of the tile
        //Desc: Returns the speed multiplier of the tile for player speed purposes
        public float GetSpeedMultiplier()
        {
            //Return the speed multiplier of the tile
            return speedMultiplier;
        }

        //Pre: spriteBatch is what allows the tile to be drawn, and transparancy is how transparent to draw the tile
        //Post: N/A
        //Desc: Draws the tile to the screen
        public virtual void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Loop through each tile location, and draw each tile
            for (int i = 0; i < tileLocs.Count; i++)
            {
                //Draw a tile to the screen
                spriteBatch.Draw(imgs[i], tileLocs[i], Color.White * transparancy);
            }
        }
    }
}
