//A: Evan Glaizel
//F: IcePlatform.cs
//P: HostileKnight
//C: 2022/12/24
//M: 
//D: The ice platforms of the level that greatly reduce the players friction

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HostileKnight
{
    class IcePlatform : Tile
    {
        //Pre: tileLocs is the location of all the tiles in the ice platform, img is the image of the ice platform
        //Post: N/A
        //Desc: Constructs the ice platform
        public IcePlatform(List<Vector2> tileLocs, List<Texture2D> img) : base(tileLocs, img)
        {
            //Set the friction and speed multipliers
            frictionMultiplier = 0.1f;
            speedMultiplier = 1.6f;

            //Sets the hitbox of the mud platform
            SetHitBox();
        }
    }
}
