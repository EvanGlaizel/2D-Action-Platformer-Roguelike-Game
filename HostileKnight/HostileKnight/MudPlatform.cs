//A: Evan Glaizel
//F: MudPlatform.cs
//P: HostileKnight
//C: 2022/12/24
//M: 
//D: The mud platforms of the level that greatly increases the players friction

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HostileKnight
{
    class MudPlatform : Tile
    {
        //Pre: tileLocs is the location of all the tiles in the mud platform, img is the image of the mud platform
        //Post: N/A
        //Desc: Constructs the mud platform
        public MudPlatform(List<Vector2> tileLocs, List<Texture2D> img) : base(tileLocs, img)
        {
            //Set the friction and speed multipliers
            frictionMultiplier = 2f;
            speedMultiplier = 0.6f;

            //Sets the hitbox of the mud platform
            SetHitBox();
        }
    }
}
