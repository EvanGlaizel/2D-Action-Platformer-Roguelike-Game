//A: Evan Glaizel
//F: Squit.cs
//P: HostileKnight
//C: 2022/12/17
//M: 2022/12/31
//D: An enemy that attacks in a horizontal lunging attack once it sees it

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
    class Squit : Enemy
    {
        //Store all of the states of the squit
        private enum EnemyState
        {
            IDLE,
            ATTACK_WINDUP,
            ATTACK,
            TURN,
            DEATH
        }

        //Store the sound effects of the squit
        private enum SquitSoundEffects
        {
            CHARGE,
            WALL_HIT
        }


        //Store the enemy state of the squit
        private EnemyState enemyState;

        //Store the sound effects of the squit
        private SoundEffect[] squitSnds;

        //Store the roll sound
        SoundEffectInstance chargeSnd;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the squit, particleImg is the image of the particles, animScale is the scale of the animations,
               startLoc is the starting location of the squit, health is the health of the squit, maxSpeed is the max speed of the squit, weight is the weight of the squit, hitboxOffset is the hitbox
               offset from the animation, sizeOffset is the hitbox size offset from the image, enemySnds are the sounds of the enemy, and squitSnds are the sound effects of the squit, and particleSnds
               are the sound effects of the particles */
        //Post: N/A
        //Desc: Constructs the squit
        public Squit(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] squitSnds, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Set the sound effects of the squit
            this.squitSnds = squitSnds;

            //Face the squit left
            drawDir = Animation.FLIP_NONE;

            //Calculate the line of sight
            calcLineOfSight = true;

            //Set the tolerance level
            speedTolerance = 1;

            //Set the roll sound and allow it to be repeated forever
            chargeSnd = squitSnds[(int)SquitSoundEffects.CHARGE].CreateInstance();
            chargeSnd.IsLooped = true;
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the squit animations
            anims = new Animation[5];
            anims[(int)EnemyState.IDLE] = new Animation(imgs[(int)EnemyState.IDLE], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 8, startLoc, animScale, true);
            anims[(int)EnemyState.ATTACK_WINDUP] = new Animation(imgs[(int)EnemyState.ATTACK_WINDUP], 2, 3, 6, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 8, startLoc, animScale, false);
            anims[(int)EnemyState.ATTACK] = new Animation(imgs[(int)EnemyState.ATTACK], 1, 3, 3, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 8, startLoc, animScale, true);
            anims[(int)EnemyState.TURN] = new Animation(imgs[(int)EnemyState.TURN], 1, 2, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 8, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH] = new Animation(imgs[(int)EnemyState.DEATH], 2, 2, 3, 0, 3, Animation.ANIMATE_ONCE, 8, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the squit
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the current animation
            anims[(int)enemyState].Update(gameTime);

            //Update the game logic based on the enemy state
            switch (enemyState)
            {
                case EnemyState.IDLE:
                    //Update the idle state
                    UpdateIdle();
                    break;
                case EnemyState.ATTACK_WINDUP:
                    //Update the attack windup state
                    UpdateAttackWindup();
                    break;
                case EnemyState.ATTACK:
                    //Update the attack state
                    UpdateAttack();
                    break;
                case EnemyState.TURN:
                    //Update the turn state
                    UpdateTurn();
                    break;
            }

            //Normalize the squits speed if they're not dying and apply gravity if they are
            if (enemyState != EnemyState.DEATH)
            {
                //Normalize the squits speed if they are moving
                if (speed.X != 0 && speed.Y != 0)
                {
                    //Normalize the squits speed, so they are not going too fast in either direction
                    speed.Normalize();
                }

                //Increase the squits speed
                speed *= maxSpeed;
            }
            else
            {
                //Apply gravity to the squit
                ApplyGravity(maxSpeed * 3);
            }

            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the idle state
        private void UpdateIdle()
        {
            //Bring the squit back to no speed
            BringToTargetSpeedX(0);
            BringToTargetSpeedY(0);

            //Agro the squit if the line of sight isn't being tested anymore (player has been spotted)
            if (!calcLineOfSight)
            {
                //Start the squits attack
                enemyState = EnemyState.ATTACK_WINDUP;
                anims[(int)enemyState].isAnimating = true;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the attack windup state
        private void UpdateAttackWindup()
        {
            //Bring the squit back to no speed
            BringToTargetSpeedX(0);
            BringToTargetSpeedY(0);

            //Start the damaging part of the squits attack if the animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Start the damaging part of the squits attack
                enemyState = EnemyState.ATTACK;

                //Start the charge sound
                chargeSnd.Play();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the attack state
        private void UpdateAttack()
        {
            //Change the direction the squit is attacking based on its direction
            if (drawDir == Animation.FLIP_NONE)
            {
                //Move the squit left
                speed.X = -maxSpeed;
            }
            else
            {
                //Move the squit right
                speed.X = maxSpeed;
            }

            //Stop the squits y speed if it's not 0
            if (speed.Y != 0)
            {
                //Stop the squits y speed
                speed.Y = 0f;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the turn state
        private void UpdateTurn()
        {
            //Bring the squit back to no speed
            BringToTargetSpeedX(0);
            BringToTargetSpeedY(0);

            //Go back to idling if the turn animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Put the squit back in idle mode
                enemyState = EnemyState.IDLE;

                //Calculate the line of sight again
                calcLineOfSight = true;

                //Flip the squits direction
                SwitchDir();
            }
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and killEnemy tracks if the collision should kill the enemy
        //Post: N/A
        //Desc: Does collision detection for the squit
        public override void TestCollision(Rectangle testedHitBox, bool killEnemy)
        {
            //Only check the body part that collided with the enemy if the main hitbox collides
            if (Util.Intersects(hitBox, testedHitBox))
            {
                //Kill the enemy if they should be killed
                if (killEnemy && health > 0)
                {
                    //Propel the enemy backwards, and kill them (DamageEnemy() brings their health down one more)
                    speed *= 1;
                    health = 1;
                    DamageEnemy();
                }
                else
                {
                    //Do different collision detection based on the part of the enemy that connected with it (left or right side)
                    if (Util.Intersects(hitBoxes[(int)BodyPart.LEFT], testedHitBox))
                    {
                        //Set the enemy just to the right of the intersected rectangle
                        hitBox.X = testedHitBox.X + testedHitBox.Width;
                        speed.X = 0f;

                        //Turn the squit around if it's attacking
                        if (enemyState == EnemyState.ATTACK)
                        {
                            //Stop the squits attack and turn them
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;

                            //Stop playing the charge sound
                            chargeSnd.Stop();

                            //Play the sound that plays when the squit charges into the wall
                            squitSnds[(int)SquitSoundEffects.WALL_HIT].CreateInstance().Play();
                        }

                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox))
                    {
                        //Set the enemy just to the left pf the intersected rectangle
                        hitBox.X = testedHitBox.X - hitBox.Width;
                        speed.X = 0f;

                        //Turn the squit around if it's attacking
                        if (enemyState == EnemyState.ATTACK)
                        {
                            //Stop the squits attack and turn them
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;

                            //Stop playing the charge sound
                            chargeSnd.Stop();

                            //Play the sound that plays when the squit charges into the wall
                            squitSnds[(int)SquitSoundEffects.WALL_HIT].CreateInstance().Play();
                        }
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.LEGS], testedHitBox))
                    {
                        //Set the enemy just ontop of the intersected rectangle
                        hitBox.Y = testedHitBox.Y - hitBox.Height;
                        speed.Y = 0f;
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.HEAD], testedHitBox))
                    {
                        //Set the enemy just below the intersected rectangle
                        hitBox.Y = testedHitBox.Y + testedHitBox.Height;
                        speed.Y = 0f;
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
                        //Normalize the squits speed
                        speed.Normalize();
                    }

                    //Add to the speed if the squit isn't about to die
                    if (health > 1)
                    {
                        //Add to the speed based on the angle to allow for an attack to change the trajectory of the enemy
                        speed.X -= (float)Math.Cos(hitAngle) / (float)(weight);
                        speed.Y -= (float)Math.Sin(hitAngle) / (float)(weight);
                    }
                    else
                    {
                        //Change the speed based on the angle to allow for an attack to change the trajectory of the enemy
                        speed.X = (float)Math.Cos(hitAngle) / (float)(weight);
                        speed.Y = (float)Math.Sin(hitAngle) / (float)(weight);
                    }

                    //Damage the enemy
                    DamageEnemy();
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the squit, and decreases their health
        protected override void DamageEnemy()
        {
            //Damage the squit, and perform all logic related to that
            base.DamageEnemy();

            //Attack if the squit is idling
            if (enemyState == EnemyState.IDLE)
            {
                //Start attacking
                enemyState = EnemyState.ATTACK_WINDUP;
                anims[(int)enemyState].isAnimating = true;
            }

            //Kill the squit if their health reaches 0
            if (health == 0)
            {
                //Start the squits air death animation
                enemyState = EnemyState.DEATH;

                //Multiply the squits speed to get a more powerful death effect
                speed *= maxSpeed;

                //Don't test colliision with the hitbox anymore
                testForCollision = false;

                //Stop playing the charge sound
                chargeSnd.Stop();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Stops all repeatable sounds in the squit
        public override void StopAllSounds()
        {
            //Stop the squits charge sound
            chargeSnd.Stop();
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

        //Pre: spriteBatch allows the squit to be drawn, and transparancy is how transparent to draw the squit
        //Post: N/A
        //Desc: Draws the squit to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the squit to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour * transparancy, drawDir);

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
