//A: Evan Glaizel
//F: AspidHunter.cs
//P: HostileKnight
//C: 2022/12/20
//M: 2023/01/15
//D: An enemy that spawns other weaker enemies

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
    class AspidMother : Enemy
    {
        //Store all of the states of the aspid mother
        private enum EnemyState
        {
            IDLE,
            SHOOT,
            DEATH_AIR,
            DEATH_GROUND
        }

        //Store the enemy state of the aspid mother
        private EnemyState enemyState;

        //Store the timer to spawn new hatchlings
        private Timer spawnTimer;

        //Store a bool that represents if a hatchling should be spawned
        private bool spawnHatchling;

        //Store the sound effect that plays when the aspid mother gives birth
        private SoundEffect birthSnd;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the enemy, particleImg is the image of the particles, animScale is the scale of the animation, startLoc is
               the starting location of the aspid mother, health is the health of the aspid mother, maxSpeed is the max speed of the aspid mother, weight is the weight of the aspid mother, hitboxOffset is the hitbox
               offset from the animation, sizeOffset is the hitbox size offset from the image, enemySnds are the sounds of the enemy, birthSnd is the sound effect of the aspid mother making a hatchling, and particleSnds
               are the sound effects of the particles*/
        //Post: N/A
        //Desc: Constructs the aspid mother
        public AspidMother(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect birthSnd, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Set the birth sound
            this.birthSnd = birthSnd;

            //Face the aspid mother left
            drawDir = Animation.FLIP_NONE;

            //Calculate the line of sight
            calcLineOfSight = true;

            //Set the shoot timer
            spawnTimer = new Timer(2000, false);

            //Set the speed tolerance
            speedTolerance = 0.2f;
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the aspid mother animations
            anims = new Animation[4];
            anims[(int)EnemyState.IDLE] = new Animation(imgs[(int)EnemyState.IDLE], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 9, startLoc, animScale, true);
            anims[(int)EnemyState.SHOOT] = new Animation(imgs[(int)EnemyState.SHOOT], 3, 3, 8, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 9, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH_AIR] = new Animation(imgs[(int)EnemyState.DEATH_AIR], 1, 1, 1, 0, 0, 0, 1, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH_GROUND] = new Animation(imgs[(int)EnemyState.DEATH_GROUND], 2, 2, 3, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the aspid mother
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the current animation
            anims[(int)enemyState].Update(gameTime);

            //Update the game logic based on the enemy state
            switch (enemyState)
            {
                case EnemyState.IDLE:
                    //Update the idle state
                    UpdateIdle(gameTime, playerRect);
                    break;
                case EnemyState.SHOOT:
                    //Update the attack state
                    UpdateAttack(playerRect);
                    break;
                case EnemyState.DEATH_AIR:
                    //Update the air death state
                    UpdateAirDeath();
                    break;
            }

            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the rectangle of the player
        //Post: N/A
        //Desc: Update the idle state
        private void UpdateIdle(GameTime gameTime, Rectangle playerRect)
        {
            //Update the attack cooldown timer
            spawnTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Agro the aspid mother if the line of sight isn't being tested anymore (player has been spotted) and the spawn cooldown is up
            if (!calcLineOfSight && !spawnTimer.IsActive())
            {
                //Start the shot of the aspid mother
                enemyState = EnemyState.SHOOT;
                anims[(int)enemyState].isAnimating = true;
            }

            //Work towards getting the aspid mothers speed to 0
            BringToTargetSpeedX(0);
            BringToTargetSpeedY(0);
        }

        //Pre: playerRect is the rectangle of the player
        //Post: N/A
        //Desc: Update the attack state
        private void UpdateAttack(Rectangle playerRect)
        {
            //Shoot the attack if the aspid mother is on its 6th frame
            if (anims[(int)enemyState].curFrame == 6 && !spawnTimer.IsActive())
            {
                //Reset the shoot timer
                spawnTimer.ResetTimer(true);

                //Spawn a hatchling
                spawnHatchling = true;

                //Propel the aspid mother upwards
                speed.Y = -5;

                //Play the giving birth sound effect
                birthSnd.CreateInstance().Play();
            }

            //Send the aspid mother back to walking once the attack animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Idle the player again
                enemyState = EnemyState.IDLE;

                //Calculate the line of sight
                calcLineOfSight = true;
            }

            //Work towards getting the aspid mothers speed to 0
            BringToTargetSpeedX(0);
            BringToTargetSpeedY(0);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the death state
        private void UpdateAirDeath()
        {
            //Apply gravity to the aspid mother
            ApplyGravity(maxSpeed * 12);
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and killEnemy tracks if the collision should kill the enemy
        //Post: N/A
        //Desc: Does collision detection for the leaping husk
        public override void TestCollision(Rectangle testedHitBox, bool killEnemy)
        {
            //Only check the body part that collided with the enemy if the main hitbox collides
            if (Util.Intersects(hitBox, testedHitBox))
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
                    //Do different collision detection based on the part of the enemy that connected with it
                    if (Util.Intersects(hitBoxes[(int)BodyPart.LEGS], testedHitBox))
                    {
                        //Set the enemy just ontop of the intersected rectangle
                        hitBox.Y = testedHitBox.Y - hitBox.Height;
                        speed.Y = 0f;

                        //Put the leaping husk in the final stage of its death if its in its first stage
                        if (enemyState == EnemyState.DEATH_AIR)
                        {
                            //Put the leaping husk in the final stage of its death
                            enemyState = EnemyState.DEATH_GROUND;

                            //Stop the enemy from moving
                            speed.X = 0f;
                        }
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.HEAD], testedHitBox))
                    {
                        //Set the enemy just below the intersected rectangle
                        hitBox.Y = testedHitBox.Y + testedHitBox.Height;
                        speed.Y = 0f;
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.LEFT], testedHitBox))
                    {
                        //Set the enemy just to the right of the intersected rectangle
                        hitBox.X = testedHitBox.X + testedHitBox.Width;
                        speed.X = 0f;
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox))
                    {
                        //Set the enemy just to the left pf the intersected rectangle
                        hitBox.X = testedHitBox.X - hitBox.Width;
                        speed.X = 0f;
                    }
                }
            }
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and playerHitBox is the hitbox of the player that used the attack
        //Post: Return the enemies hitbox if there was a collision
        //Desc: Does collision detection for the attacks (Doesn't let them fall through floors or walls)
        public override void TestAttackCollision(Rectangle testedHitBox, Rectangle playerHitBox)
        {
            //Only test collision if the attack cooldown is up and the enemy should be tested for collision
            if (!hitCooldown.IsActive() && testForCollision)
            {
                //Only test specific location collision if the main hitbox collides with the tested hit box
                if (Util.Intersects(hitBox, testedHitBox))
                {
                    //Calculate the angle at which to launch the enemy
                    hitAngle = Math.Atan2(playerHitBox.Center.Y - hitBox.Center.Y, playerHitBox.Center.X - hitBox.Center.X);

                    //Normalize the speed if the speed isnt 0
                    if (speed.X != 0 && speed.Y != 0)
                    {
                        //Normalize the crawlids speed
                        speed.Normalize();
                    }

                    //Add to the speed based on the angle to allow for an attack to change the trajectory of the enemy
                    speed.X -= (float)Math.Cos(hitAngle) / (float)(weight);
                    speed.Y -= (float)Math.Sin(hitAngle) / (float)(weight);

                    //Damage the enemy
                    DamageEnemy();

                    //Calculate the position of the enemy based on the players hitbox
                    CalcDir(playerHitBox);
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the aspid mother, and decreases their health
        protected override void DamageEnemy()
        {
            //Damage the aspid mother, and perform all logic related to that
            base.DamageEnemy();

            //Kill the aspid mother if their health reaches 0
            if (health == 0)
            {
                //Start the aspid mothers air death animation
                enemyState = EnemyState.DEATH_AIR;

                //Multiply the aspid mothers speed to get a more powerful death effect
                speed *= 2;

                //Don't test colliision with the hitbox anymore
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

        //Pre: N/A
        //Post: N/A
        //Desc: Stops spawning a hatchling
        public void DontSpawnHatchling()
        {
            //Stop spawning a hatchling
            spawnHatchling = false;
        }

        //Pre: N/A
        //Post: Return the bool value of spwaning a hatchling
        //Desc: Tell the program to spawn a hatchling
        public bool SpawnHatchling()
        {
            //Return a bool that represents if a hatchling should be spawned
            return spawnHatchling;
        }

        //Pre: spriteBatch allows the aspid mother to be drawn, and transparancy is how transparent to draw the aspid mother
        //Post: N/A
        //Desc: Draws the aspid mother to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the aspid mother to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour, drawDir);

            //lineOfSight.Draw(spriteBatch, Color.White);

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
