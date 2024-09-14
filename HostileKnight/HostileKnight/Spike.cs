//A: Evan Glaizel
//F: Spike.cs
//P: HostileKnight
//C: 2022/12/5
//M: 2022/12/06
//D: The spikes of the level that damage the players and kill enemies

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HostileKnight
{
    class Spike : Tile
    {
        //Store the grid length
        private const int GRID_LENGTH = 60;

        //Pre: tileLocs is the location of all the tiles in the spike, img is the image of the spike
        //Post: N/A
        //Desc: Constructs the spike
        public Spike(List<Vector2> tileLocs, List<Texture2D> img) : base(tileLocs, img)
        {
            //Set the friction and speed multipliers
            frictionMultiplier = 1f;
            speedMultiplier = 1f;

            //Sets the hitbox of the spike
            SetHitBox();
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Constructs the hitbox of the spike
        protected override void SetHitBox()
        {
            //Create the hitbox of the tile
            hitBox = new Rectangle((int)tileLocs[0].X + (imgs[0].Width / 4), (int)tileLocs[0].Y + (imgs[0].Height / 2), (int)(tileLocs[tileLocs.Count - 1].X + imgs[0].Width - tileLocs[0].X - (imgs[0].Width / 2)), (int)(tileLocs[tileLocs.Count - 1].Y + imgs[0].Height - tileLocs[0].Y - (imgs[0].Height / 2)));
        }
    }
}
