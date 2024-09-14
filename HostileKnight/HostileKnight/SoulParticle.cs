//A: Evan Glaizel
//F: SoulParticle.cs
//P: HostileKnight
//C: 2022/12/26
//M: 2022/12/31
//D: The particle that tracks the player and goes towards them

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
    class SoulParticle : Particle
    {
        //Store the total max speed of the particle
        private int totalMaxSpeed;

        //Store if the particle should be killed
        private bool killParticle = false;

        //Pre: img is the image of the particle, startLoc is the starting location of the particle, lifespan is the lifespan of the particle in milliseconds, maxSpeed is the speed to send the particle flying in each direction,
        //hitAngle is the angle to send the particle, gravity is the strength of gravity being applied to the particle, sizeMultiplier is the multiplier of the particle size, and colour is the colour to draw the particle
        //Post: N/A
        //Desc: N/A
        public SoulParticle(Texture2D img, Vector2 startLoc, int lifespan, int maxSpeed, double hitAngle, float gravity, float sizeMultiplier, Color colour) : base(img, startLoc, lifespan, maxSpeed, hitAngle, gravity, sizeMultiplier, colour)
        {
            //Set the total max speed
            totalMaxSpeed = maxSpeed;

            //Stop the particle
            speed *= 0;
        }

        //Pre: gameTime stores the time in the game, and playerRect is the location of the player
        //Post: N/A
        //Desc: Updates the particle
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Store the distance between the particle and player
            Vector2 playerDist;

            //Set the distance between the x and y values
            playerDist.X = hitBox.Center.X - playerRect.Center.X;
            playerDist.Y = hitBox.Center.Y - playerRect.Center.Y;

            //Normalize the players distance to calculate the exact spot the particle needs to move to catch up with the player
            playerDist.Normalize();
            speed.X -= MathHelper.Clamp(playerDist.X * totalMaxSpeed, -totalMaxSpeed, totalMaxSpeed);
            speed.Y -= MathHelper.Clamp(playerDist.Y * totalMaxSpeed, -totalMaxSpeed, totalMaxSpeed);

            //Clamp the speed so the particle doesn't travel too fast
            speed.X = MathHelper.Clamp(speed.X, -18, 18);
            speed.Y = MathHelper.Clamp(speed.Y, -18, 18);

            //Update the location of the particle hitbox
            hitBox.X += (int)speed.X;
            hitBox.Y += (int)speed.Y;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Actives the kill to kill the soul particle
        public void ActivateKill()
        {
            //Kill the particle
            killParticle = true;
        }

        //Pre: N/A
        //Post: Returns if the particle should be destroyed
        //Desc: Tells the emmiter if the particle should be destroyed
        public override bool KillParticle()
        {
            //Destroy the particle if it should be killed
            return killParticle;
        }
    }
}
