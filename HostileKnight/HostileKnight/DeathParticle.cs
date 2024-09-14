//A: Evan Glaizel
//F: DeathParticle.cs
//P: HostileKnight
//C: 2022/12/26
//M:
//D: The particle that goes out in a starting speed, and slows down until stationary

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
    class DeathParticle : Particle
    {
        //Store the tolerance of the death particle
        private float tolerance;

        //Pre: img is the image of the particle, startLoc is the starting location of the particle, lifespan is the lifespan of the particle in milliseconds, maxSpeed is the speed to send the particle flying in each direction,
             //hitAngle is the angle to send the particle, gravity is the strength of gravity being applied to the particle, sizeMultiplier is the multiplier of the particle size, colour is the colour to draw the particle, and 
             //numFramesOutward is the number of frames to keep moving outwards by before coming to a stop
        //Post: N/A
        //Desc: N/A
        public DeathParticle(Texture2D img, Vector2 startLoc, int lifespan, int maxSpeed, double hitAngle, float gravity, float sizeMultiplier, Color colour, int numFramesOutwards) : base(img, startLoc, lifespan, maxSpeed, hitAngle, gravity, sizeMultiplier, colour)
        {
            //Set the tolerance of the death particle
            tolerance = maxSpeed * (1.0f / numFramesOutwards);
        }

        //Pre: gameTime stores the time in the game, and playerRect is the location of the player
        //Post: N/A
        //Desc: Updates the particle
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Slow down the particle
            speed.X += -Math.Sign(speed.X) * tolerance;
            speed.Y += -Math.Sign(speed.Y) * tolerance;

            //Stop the particle if it gets close enough to the tolerance
            if (Math.Abs(speed.X) <= tolerance)
            {
                //Reset the particles x speed
                speed.X = 0;
            }

            //Stop the particle if it gets close enough to the tolerance
            if (Math.Abs(speed.Y) <= tolerance)
            {
                //Reset the particles y speed
                speed.Y = 0;
            }

            //Update the location of the particle hitbox
            hitBox.X -= (int)speed.X;
            hitBox.Y -= (int)speed.Y;
        }

        //Pre: N/A
        //Post: Returns if the particle should be destroyed
        //Desc: Tells the emmiter if the particle should be destroyed
        public override bool KillParticle()
        {
            //Destroy the particle if the particle is still
            return (speed.X == 0 && speed.Y == 0);
        }
    }
}
