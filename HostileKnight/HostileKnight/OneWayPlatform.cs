//A: Evan Glaizel
//F: OneWayPlatform.cs
//P: HostileKnight
//C: 2022/12/24
//M: 
//D: The one way platform that the player can only go through from underneath

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HostileKnight
{
    class OneWayPlatform : Tile
    {
        //Pre: tileLocs is the location of all the tiles in the one way platform, img is the image of the one way platform
        //Post: N/A
        //Desc: Constructs the one way platform
        public OneWayPlatform(List<Vector2> tileLocs, List<Texture2D> img) : base(tileLocs, img)
        {
            //Set the friction and speed multipliers
            frictionMultiplier = 1f;
            speedMultiplier = 1f;

            //Sets the hitbox of the one way platform
            SetHitBox();
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Constructs the hitbox of the one way platform
        protected override void SetHitBox()
        {
            //Create the one way platforms hitbox when it's facing down
            hitBox = new Rectangle((int)tileLocs[0].X, (int)tileLocs[0].Y, (int)(tileLocs[tileLocs.Count - 1].X + imgs[0].Width - tileLocs[0].X), (int)(tileLocs[tileLocs.Count - 1].Y + imgs[0].Height - tileLocs[0].Y) / 6);
        }
    }
}
