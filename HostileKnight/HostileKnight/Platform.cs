//A: Evan Glaizel
//F: Platform.cs
//P: HostileKnight
//C: 2022/12/5
//M: 2022/12/06
//D: The platforms of the level. This is the basic building block of all levels

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HostileKnight
{
    class Platform : Tile
    {
        //Pre: tileLocs is the location of all the tiles in the platform, and img is the image of the platform
        //Post: N/A
        //Desc: Constructs the platform
        public Platform(List<Vector2> tileLocs, List<Texture2D> img) : base(tileLocs, img)
        {
            //Set the friction and speed multipliers
            frictionMultiplier = 1f;
            speedMultiplier = 1f;

            //Set the hitbox of the platform
            SetHitBox();
        }
    }
}
