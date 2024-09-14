//A: Evan Glaizel
//F: AspidHunter.cs
//P: HostileKnight
//C: 2022/12/20
//M: 
//D: The projectile that gets shot at the player

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Helper;

namespace HostileKnight
{
    class Projectile
    {
        //Store the image of the projectile
        private Texture2D img;

        //Store the hitbox of the projectile
        private Rectangle hitBox;

        //Store the speed of the projectile
        private int maxSpeed;
        private Vector2 speed;
        private double hitAngle;

        //Store the lifespan of the projectile
        private Timer lifespan;

        //Pre: img is the image of the projectile, startLoc is the starting location of the projectile, maxSpeed is the max speed the projectile can travel, and hitAngle is the angle 
        //Post: N/A
        //Desc: Construct the projectile
        public Projectile(Texture2D img, Vector2 startLoc, int maxSpeed, double hitAngle)
        {
            //Set the data of the projectile
            this.img = img;
            this.maxSpeed = maxSpeed;
            this.hitAngle = hitAngle;

            //Set the hitbox
            hitBox = new Rectangle((int)startLoc.X + img.Width / 4, (int)startLoc.Y + img.Height / 4, img.Width / 2, img.Height / 2);

            //Set the speed (saves sin and cos from being calculated every frame
            speed.X = (float)Math.Sin(hitAngle);
            speed.Y = (float)Math.Cos(hitAngle);
            speed *= maxSpeed;

            //Set the lifespan of the projectile
            lifespan = new Timer(Timer.INFINITE_TIMER, true);
        }

        //Pre: gameTime allows the timers to update
        //Post: N/A
        //Desc: Update the projectile
        public void Update(GameTime gameTime)
        {
            //Update the lifespan 
            lifespan.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Update the location of the hitbox
            hitBox.X += (int)speed.X;
            hitBox.Y += (int)speed.Y;
        }

        //Pre: tileRect is the rectangle of the tile
        //Post: N/A
        //Desc: tests collision detection between the attack and tiles
        public bool TestCollision(Rectangle tileRect)
        {
            //Return the state of the attack intersecting with the tile only if the projectile has been around for long enough
            return Util.Intersects(hitBox, tileRect) && lifespan.GetTimePassed() > 100;
        }

        /// <summary>
        /// Returns the hitbox
        /// </summary>
        /// <param></param>
        public Rectangle GetHitBox()
        {
            //Return the hitbox
            return hitBox;
        }

        //Pre: spriteBatch allows the projectile be drawn to the school
        //Post: N/A
        //Desc: draw the projectile
        public void Draw(SpriteBatch spriteBatch)
        {
            //Draw the projectile to the screen
            spriteBatch.Draw(img, hitBox, Color.Orange);
        }
    }
}
