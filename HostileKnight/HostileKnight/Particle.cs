//A: Evan Glaizel
//F: Particle.cs
//P: HostileKnight
//C: 2022/12/25
//M:
//D: The particles of the game. Used for soul mechanic and visual clarity

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
    class Particle
    {
        //Store the image and hitbox of the particle
        private Texture2D img;
        protected Rectangle hitBox;

        //Store the lifespan of the particle
        private int lifespan;
        private Timer lifeTimer;

        //Store the speed to send the particle flying, and the current speed
        protected Vector2 maxSpeed;
        protected Vector2 speed;

        //Store the gravity strength on the particle
        private float gravity;

        //Store the colour of the particle
        private Color colour;

        //Pre: img is the image of the particle, startLoc is the starting location of the particle, lifespan is the lifespan of the particle in milliseconds, maxSpeed is the speed to send the particle flying in each direction,
             //hitAngle is the angle to send the particle, gravity is the strength of gravity being applied to the particle, sizeMultiplier is the multiplier of the particle size, and colour is the colour to draw the particle
        //Post: N/A
        //Desc: N/A
        public Particle(Texture2D img, Vector2 startLoc, int lifespan, int maxSpeed, double hitAngle, float gravity, float sizeMultiplier, Color colour)
        {
            //Set the partcile data
            this.img = img;
            this.lifespan = lifespan;
            this.gravity = gravity;
            this.colour = colour;

            //Set the max and starting speed
            this.maxSpeed.X = (float)Math.Cos(hitAngle);
            this.maxSpeed.Y = (float)Math.Sin(hitAngle);

            //If the max speed is close enough to 0, set it to 0  (Math.Cos(Math.PI) / 2 gives me a very close number to 0, but not 0)
            if (Math.Abs(this.maxSpeed.X) < 0.0001)
            {
                //Set the max speed to 0
                this.maxSpeed.X = 0;
            }

            //If the max speed is close enough to 0, set it to 0
            if (Math.Abs(this.maxSpeed.Y) < 0.0001)
            {
                //Set the max speed to 0
                this.maxSpeed.Y = 0;
            }

            this.maxSpeed.Normalize();
            this.maxSpeed *= maxSpeed;
            speed = this.maxSpeed;

            //Set the particle hitbox
            hitBox = new Rectangle((int)startLoc.X, (int)startLoc.Y, (int)(img.Width * sizeMultiplier), (int)(img.Height * sizeMultiplier));

            //Set the lifespawn timer
            lifeTimer = new Timer(lifespan, true);
        }

        //Pre: gameTime stores the time in the game, and playerRect is the location of the player
        //Post: N/A
        //Desc: Updates the particle
        public virtual void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the particle timer
            lifeTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Apply gravity to the particle
            speed.Y += gravity;

            //Update the location of the particle hitbox
            hitBox.X += (int)speed.X;
            hitBox.Y += (int)speed.Y;
        }

        //Pre: N/A
        //Post: Returns if the particle should be destroyed
        //Desc: Tells the emmiter if the particle should be destroyed
        public virtual bool KillParticle()
        {
            //Return true if the timer is finished
            return lifeTimer.IsFinished();
        }

        //Pre: N/A
        //Post: Returns the hitbox of the particle
        //Desc: Returns the particle hitbox to the program
        public Rectangle GetHitBox()
        {
            //Return the particle hitbox
            return hitBox;
        }

        //Pre: spriteBatch allows the particle to draw itself, and transparancy is how transparent to draw the particle
        //Post: N/A
        //Desc: Draws the particle
        public void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the particle
            spriteBatch.Draw(img, hitBox, colour * transparancy);
        }
    }
}
