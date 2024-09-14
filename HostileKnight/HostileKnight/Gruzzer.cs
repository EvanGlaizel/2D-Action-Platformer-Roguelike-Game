//A: Evan Glaizel
//F: Enemy.cs
//P: HostileKnight
//C: 2022/12/10
//M: 
//D: The enemy that bounces around the screen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Animation2D;
using Helper;

namespace HostileKnight
{
    class Gruzzer : Enemy
    {
        //Store a random variable to randomize bounce direction and the random range for the bounce direction
        private Random rng = new Random();
        private const int RANDOM_RANGE = 6;

        //Store all of the states of the gruzzer
        private enum EnemyState
        {
            FLY,
            DIE_AIR,
            DIE_GROUND
        }

        //Store the enemy state of the gruzzer
        private EnemyState enemyState;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the gruzzer, particleImg is the image of the particles, animScale is the scale of the animations, 
               startLoc is the starting location of the gruzzer, health is the health of the gruzzer, maxSpeed is the max speed of the gruzzer, weight is the weight of the gruzzer, hitboxOffset is the hitbox 
               offset from the animation, sizeOffset is the hitbox size offset from the image, and enemySnds are the sounds of the enemy, and particleSnds are the sound effects of the particles */
        //Post: N/A
        //Desc: Constructs the gruzzer
        public Gruzzer(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Keep setting the x speed aslong as its not 0
            while (speed.X == 0)
            {
                //Set the starting x speed
                speed.X = rng.Next(-maxSpeed, maxSpeed);
            }

            //Keep setting the y speed aslong as its not 0
            while (speed.Y == 0)
            {
                //Set the starting y speed
                speed.Y = rng.Next(-maxSpeed, maxSpeed);
            }

            //Set the gruzzers starting direction based on the speed
            CalcDir();
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the gruzzer animations
            anims = new Animation[3];
            anims[(int)EnemyState.FLY] = new Animation(imgs[(int)EnemyState.FLY], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 7, startLoc, animScale, true);
            anims[(int)EnemyState.DIE_AIR] = new Animation(imgs[(int)EnemyState.DIE_AIR], 1, 2, 2, 0, 2, Animation.ANIMATE_ONCE, 8, startLoc, animScale, true);
            anims[(int)EnemyState.DIE_GROUND] = new Animation(imgs[(int)EnemyState.DIE_GROUND], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the gruzzer
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the current animation
            anims[(int)enemyState].Update(gameTime);

            //Normalize the gruzzers speed if they're not dying and apply gravity if they are
            if (enemyState != EnemyState.DIE_AIR)
            {
                //Normalize the gruzzers speed if they are moving
                if (speed.X != 0 && speed.Y != 0)
                {
                    //Normalize the gruzzers speed, so they are not going too fast in either direction
                    speed.Normalize();
                }
                
                //Increase the gruzzers speed
                speed *= maxSpeed;
            }
            else
            {
                //Apply gravity to the gruzzer
                ApplyGravity(maxSpeed * 5);
            }

            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect);
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and killEnemy tracks if the collision should kill the enemy
        //Post: N/A
        //Desc: Detects collision between the gruzzer and the wall, and changes its direction based on the collision
        public override void TestCollision(Rectangle testedHitBox, bool killEnemy)
        {
            //Only check the body part that collided with the enemy if the main hitbox collides and the gruzzer isnt dead
            if (Util.Intersects(hitBox, testedHitBox) && enemyState != EnemyState.DIE_GROUND)
            {
                //Kill the enemy if they should be killed
                if (killEnemy && health > 0)
                {
                    //Propel the enemy backwards, and kill them (damage enemy brings their health down one more)
                    speed *= -1;
                    health = 1;
                    DamageEnemy();
                }
                else
                {
                    //Do different things based on the part of the enemy that connected with it
                    if (Util.Intersects(hitBoxes[(int)BodyPart.HEAD], testedHitBox))
                    {
                        //Bounce the gruzzer off the top or bottom wall and randomize its x speed for variety
                        speed.Y = Math.Abs(speed.Y);
                        speed.X += rng.Next(-RANDOM_RANGE, RANDOM_RANGE + 1);

                        //Randomize the x speed if its not 0
                        while (speed.X == 0)
                        {
                            //Randomise the x speed
                            speed.X += rng.Next(-RANDOM_RANGE, RANDOM_RANGE + 1);
                        }

                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.LEGS], testedHitBox))
                    {
                        //Bounce off the bottom wall or die on the ground based on the player state
                        if (enemyState == EnemyState.DIE_AIR)
                        {
                            //Start the final phase in the enemies death animation
                            enemyState = EnemyState.DIE_GROUND;

                            //Stop the gruzzers movement
                            speed.X = 0f;
                            speed.Y = 0f;
                        }
                        else
                        {
                            //Bounce the gruzzer off the bottom wall and randomize its x speed for variety
                            speed.Y = -Math.Abs(speed.Y);
                            speed.X += rng.Next(-RANDOM_RANGE, RANDOM_RANGE + 1);

                            //Randomize the x speed if its not 0
                            while (speed.X == 0)
                            {
                                //Randomise the x speed
                                speed.X += rng.Next(-RANDOM_RANGE, RANDOM_RANGE + 1);
                            }
                        }
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.LEFT], testedHitBox))
                    {
                        //Bounce the gruzzer off the left wall and randomize its x speed for variety
                        speed.X = Math.Abs(speed.Y);
                        speed.Y += rng.Next(-RANDOM_RANGE, RANDOM_RANGE + 1);

                        //Randomize the y speed if its 0
                        while (speed.Y == 0)
                        {
                            //Randomise the y speed
                            speed.Y += rng.Next(-RANDOM_RANGE, RANDOM_RANGE + 1);
                        }
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox))
                    {
                        //Bounce the gruzzer off the right wall and randomize its x speed for variety
                        speed.X = -Math.Abs(speed.Y);
                        speed.Y += rng.Next(-RANDOM_RANGE, RANDOM_RANGE + 1);

                        //Randomize the y speed if its not 0
                        while (speed.Y == 0)
                        {
                            //Randomise the y speed
                            speed.Y += rng.Next(-RANDOM_RANGE, RANDOM_RANGE + 1);
                        }
                    }

                    //Set the gruzzers direction based on the speed
                    if (speed.X > 0)
                    {
                        //Face the gruzzer right
                        drawDir = Animation.FLIP_HORIZONTAL;
                    }
                    else if (speed.X < 0)
                    {
                        //Face the gruzzer left
                        drawDir = Animation.FLIP_NONE;
                    }
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the mob, and decreases their health
        protected override void DamageEnemy()
        {
            //Damage the gruzzer, and perform all logic related to that
            base.DamageEnemy();

            //Kill the gruzzer if their health reaches 0
            if (health == 0)
            {
                //Start the gruzzers air death animation
                enemyState = EnemyState.DIE_AIR;

                //Multiply the gruzzers speed to get a more powerful death effect
                speed *= maxSpeed;

                //Dont let the gruzzer collide with the player anymore
                testForCollision = false;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates all hitboxes and animations to the correct frame
        public override void UpdateGamePos()
        {
            //Update the default positions
            base.UpdateGamePos();

            //Update the animation location
            anims[(int)enemyState].destRec.X = hitBox.X - (int)hitboxOffset.X;
            anims[(int)enemyState].destRec.Y = hitBox.Y - (int)hitboxOffset.Y;
        }

        //Pre: spriteBatch allows the gruzzer to be drawn, and transparancy is how transparent to draw the emitter
        //Post: N/A
        //Desc: Draws the gruzzer to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the gruzzer to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour * transparancy, drawDir);

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
