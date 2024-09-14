//A: Evan Glaizel
//F: Emitter.cs
//P: HostileKnight
//C: 2022/12/25
//M:
//D: The emitter which manages the particles in the game

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace HostileKnight
{
    class Emitter
    {
        //Store a list of each particle type
        public enum ParticleType
        {
            NORMAL,
            DEATH,
            SOUL
        }

        //Store a list of particles
        private List<Particle> particles = new List<Particle>();

        //Store the particle image
        private Texture2D particleImg;

        //Store the max speed of the soul particle
        private int maxSoulParticleSpeed;

        //Store the sound effects of the particle
        SoundEffect createSnd;

        //Pre: particleImg is the image of each particle, and particleSnds are the createSnd is the creation sound of the particle
        //Post: N/A
        //Desc: Constructs the emitter
        public Emitter(Texture2D particleImg, SoundEffect createSnd)
        {
            //Set the particle image and sound
            this.particleImg = particleImg;
            this.createSnd = createSnd;

            //Set the max soul particle speed
            maxSoulParticleSpeed = 2;
        }

        //Pre: gameTime is the time of the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the emitter and its particles
        public void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Loop through each particle to update and test to remove them
            for (int i = 0; i < particles.Count; i++)
            {
                //Update each particle
                particles[i].Update(gameTime, playerRect);

                //Destroy the particle if its lifespan is up
                if (particles[i].KillParticle())
                {
                    //Create a soul particle if the killed particle is a death particle
                    if (particles[i] is DeathParticle)
                    {
                        //Create a new soul particle
                        particles.Add(new SoulParticle(particleImg, new Vector2(particles[i].GetHitBox().X, particles[i].GetHitBox().Y), 0, maxSoulParticleSpeed, 0, 0, 0.67f, Color.White));
                    }

                    //Remove the particle
                    particles.RemoveAt(i);
                }
            }
        }

        //Pre: startLoc is the starting location of the particle, lifespan is the lifespan of the particle in milliseconds, maxSpeed is the speed to send the particle flying in each direction, hitAngle is the angle to send the particle
        //gravity is the strength of gravity being applied to the particle, sizeMultiplier is the multiplier of the particle size, colour is the colour to draw the particle, numFramesOutward is the number of frames to keep moving
             //outwards by before coming to a stop particleType is the type of particle
        //Post: N/A
        //Desc: Creates a new particle
        public void CreateParticle(Vector2 startLoc, int lifespan, int maxSpeed, double hitAngle, float gravity, float sizeMultiplier, Color colour, int numFramesOutward, ParticleType particleType)
        {
            //Create a new particle based on the particle type
            switch (particleType)
            {
                case ParticleType.NORMAL:
                    //Create a new normal particle
                    particles.Add(new Particle(particleImg, startLoc, lifespan, maxSpeed, hitAngle, gravity, sizeMultiplier, colour));
                    break;
                case ParticleType.DEATH:
                    //Create a new death particle
                    particles.Add(new DeathParticle(particleImg, startLoc, lifespan, maxSpeed, hitAngle, gravity, sizeMultiplier, colour, numFramesOutward));

                    //Play the death particle sound
                    createSnd.CreateInstance().Play();
                    break;
            }        
        }

        //Pre: N/A
        //Post: Return the number of particles in the emitter
        //Desc: Tells the program the amount of particles in the emitter
        public int GetCount()
        {
            //Return the number of particles 
            return particles.Count;
        }

        //Pre: N/A
        //Post: Return the particles
        //Desc: Returns each particle in the emitter
        public List<Particle> GetParticles()
        {
            //Return the emitters particles
            return particles;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Clears all particles from the current particle list
        public void Clear()
        {
            //Define a new list of particles to remove the old one
            particles = new List<Particle>();
        }

        //Pre: spriteBatch allows the particles to be drawn, and transparancy is how transparent to draw the particles
        //Post: N/A
        //Desc: Draws each particle to the screen
        public void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Loop through each particle to draw it to the screen
            for (int i = 0; i < particles.Count; i++)
            {
                //Draw each particle
                particles[i].Draw(spriteBatch, transparancy);
            }
        }
    }
}
