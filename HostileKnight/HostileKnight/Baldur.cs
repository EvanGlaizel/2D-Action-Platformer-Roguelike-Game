//A: Evan Glaizel
//F: Baldur.cs
//P: HostileKnight
//C: 2022/12/17
//M: 
//D: A ground enemy that attacks in a horizontal lunging attack once it sees it

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
    class Baldur : Enemy
    {
        //Store all of the states of the baldur
        private enum EnemyState
        {
            WALK,
            ROLL_START,
            ROLL,
            TURN,
            DEATH
        }

        //Store all of the baldur sounds
        private enum BaldurSoundEffects
        {
            ROLL_WINDUP,
            ROLL,
            ROLL_END
        }

        //Store the enemy state of the baldur
        private EnemyState enemyState;

        //Store the max walking speed
        private int maxWalkSpeed = 1;

        //Store a left hitbox and right hitbox to detect when the enemy is coming to an edge
        private Rectangle leftHitBox;
        private Rectangle rightHitBox;

        //Store a turn cooldown timer
        private Timer turnTimer;

        //Store the sound effects for the baldur
        private SoundEffect[] baldurSnds;

        //Store the repeating sound of the baldur
        private SoundEffectInstance rollSnd;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the baldur, particleImg is the image of the particles, animScale is the scale of the animations,
               startLoc is the starting location of the baldur, health is the health of the baldur, maxSpeed is the max speed of the baldur, weight is the weight of the baldur, hitboxOffset is the hitbox
               offset from the animation, sizeOffset is the hitbox size offset from the image, enemySnds are the sounds of the enemy, and baldurSnds are the sound effects of the baldur, and particleSnds
               are the sound effects of the particles */
        //Post: N/A
        //Desc: Constructs the baldur
        public Baldur(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] baldurSnds, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Set the baldur sound effects
            this.baldurSnds = baldurSnds;

            //Face the baldur left
            drawDir = Animation.FLIP_NONE;

            //Calculate the line of sight
            calcLineOfSight = true;

            //Set the tolerance level
            speedTolerance = maxWalkSpeed * 0.2;

            //Set the left and right hitboxes to detect when the baldur as at the edge
            leftHitBox = hitBox;
            rightHitBox = hitBox;

            //Set the turn timer
            turnTimer = new Timer(500, true);

            //Set the roll sound
            rollSnd = baldurSnds[(int)BaldurSoundEffects.ROLL].CreateInstance();
            rollSnd.IsLooped = true;
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the baldur animations
            anims = new Animation[5];
            anims[(int)EnemyState.WALK] = new Animation(imgs[(int)EnemyState.WALK], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 7, startLoc, animScale, true);
            anims[(int)EnemyState.ROLL_START] = new Animation(imgs[(int)EnemyState.ROLL_START], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, false);
            anims[(int)EnemyState.ROLL] = new Animation(imgs[(int)EnemyState.ROLL], 2, 2, 3, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 7, startLoc, animScale, true);
            anims[(int)EnemyState.TURN] = new Animation(imgs[(int)EnemyState.TURN], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH] = new Animation(imgs[(int)EnemyState.DEATH], 2, 3, 6, 0, 6, Animation.ANIMATE_ONCE, 5, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the baldur
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the current animation
            anims[(int)enemyState].Update(gameTime);

            //Update the game logic based on the enemy state
            switch (enemyState)
            {
                case EnemyState.WALK:
                    //Update the walk state
                    UpdateWalk(gameTime);
                    break;
                case EnemyState.ROLL_START:
                    //Update the start of the roll state
                    UpdateRollStart();
                    break;
                case EnemyState.ROLL:
                    //Update the roll state
                    UpdateRoll();
                    break;
                case EnemyState.TURN:
                    //Update the turn state
                    UpdateTurn();
                    break;
            }

            //Apply gravity to the baldur
            ApplyGravity(maxSpeed * 7);

            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect);
        }

        //Pre: gameTime allows the turn timer to update
        //Post: N/A
        //Desc: Update the walk state
        private void UpdateWalk(GameTime gameTime)
        {
            //Update the turn timer
            turnTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Change the baldurs speed based on their direction and speed
            if (drawDir == Animation.FLIP_NONE)
            {
                //Work towards getting the baldurs speed to max
                BringToTargetSpeedX(-maxWalkSpeed);
            }
            else if (drawDir == Animation.FLIP_HORIZONTAL)
            {
                //Work towards getting the baldurs speed to max
                BringToTargetSpeedX(maxWalkSpeed);
            }

            //Agro the baldur if the line of sight isn't being tested anymore (player has been spotted)
            if (!calcLineOfSight)
            {
                //Start the baldurs roll
                enemyState = EnemyState.ROLL_START;
                anims[(int)enemyState].isAnimating = true;

                //Play the roll windup sound
                baldurSnds[(int)BaldurSoundEffects.ROLL_WINDUP].CreateInstance().Play();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the attack windup state
        private void UpdateRollStart()
        {
            //Start the damaging part of the baldurs attack if the animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Start the damaging part of the baldurs attack
                enemyState = EnemyState.ROLL;

                //Start the rolling sound
                rollSnd.Play();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the roll state
        private void UpdateRoll()
        {
            //Change the direction the baldur is attacking based on its direction
            if (drawDir == Animation.FLIP_NONE)
            {
                //Move the baldur left
                speed.X = -maxSpeed;
            }
            else
            {
                //Move the baldur right
                speed.X = maxSpeed;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the turn state
        private void UpdateTurn()
        {
            //Go back to idling if the turn animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Put the baldur back in walk mode
                enemyState = EnemyState.WALK;

                //Calculate the line of sight again
                calcLineOfSight = true;
            }
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and killEnemy tracks if the collision should kill the enemy
        //Post: N/A
        //Desc: Does collision detection for the baldur
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

                        //Stop the baldur if they're dead and turn the baldur around if their side rectangles aren't touching the wall the baldur is on, and the turn cooldown is up
                        if (enemyState == EnemyState.DEATH)
                        {
                            //Stop the baldur from moving horizontally
                            speed.X = 0f;
                        }
                        else if ((!Util.Intersects(leftHitBox, testedHitBox) || !Util.Intersects(rightHitBox, testedHitBox)) && enemyState == EnemyState.WALK && turnTimer.IsFinished())
                        {
                            //Move the baldur off of the edge
                            hitBox.X += -Math.Sign(speed.X) * 10;

                            //Turn the baldur around
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;
                            speed.X = 0f;

                            //Reset the turn timer
                            turnTimer.ResetTimer(true);

                            //Flip the baldurs direction
                            SwitchDir();
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

                        //Move the baldur off of the wall
                        hitBox.X += -Math.Sign(speed.X) * 5;

                        //Reset the baldurs speed
                        speed.X = 0f;

                        //Turn the baldur around if it's walking or rolling
                        if (enemyState == EnemyState.WALK || enemyState == EnemyState.ROLL)
                        {
                            //Finish the roll sounds if the enemy is rolling
                            if (enemyState == EnemyState.ROLL)
                            {
                                //Stop playing the roll sound
                                rollSnd.Stop();

                                //Play the roll ending sound
                                baldurSnds[(int)BaldurSoundEffects.ROLL_END].CreateInstance().Play();
                            }

                            //Stop the baldurs attack and turn them
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;

                            //Flip the baldurs direction
                            SwitchDir();
                        }

                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox))
                    {
                        //Set the enemy just to the left pf the intersected rectangle
                        hitBox.X = testedHitBox.X - hitBox.Width;

                        //Move the baldur off of the wall
                        hitBox.X += -Math.Sign(speed.X) * 5;

                        //Reset the baldurs speed
                        speed.X = 0f;

                        //Turn the baldur around if it's walking or rolling
                        if (enemyState == EnemyState.WALK || enemyState == EnemyState.ROLL)
                        {
                            //Finish the roll sounds if the enemy is rolling
                            if (enemyState == EnemyState.ROLL)
                            {
                                //Stop playing the roll sound
                                rollSnd.Stop();

                                //Play the roll ending sound
                                baldurSnds[(int)BaldurSoundEffects.ROLL_END].CreateInstance().Play();
                            }

                            //Stop the baldurs attack and turn them
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;

                            //Flip the baldurs direction
                            SwitchDir();
                        }
                    }
                }
            }
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and playerHitBox is the hitbox of the player that used the attack
        //Post: Return the enemies hitbox if there was a collision
        //Desc: Does collision detection for the attacks (Doesn't let them fall through floors or walls)
        public override void TestAttackCollision(Rectangle testedHitBox, Rectangle playerHitBox)
        {
            //Only test collision if the attack cooldown is up and the crawlid is not dead
            if (!hitCooldown.IsActive() && enemyState != EnemyState.DEATH)
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
                    speed.X -= (float)(Math.Cos(hitAngle) / (weight));
                    speed.Y -= (float)(Math.Sin(hitAngle) / (weight));

                    //Damage the enemy
                    DamageEnemy();
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the baldur, and decreases their health
        protected override void DamageEnemy()
        {
            //Damage the baldur, and perform all logic related to that
            base.DamageEnemy();

            //Start the baldurs attack if they get hit and they are walking
            if (enemyState == EnemyState.WALK)
            {
                //Start the baldurs roll
                enemyState = EnemyState.ROLL_START;
                anims[(int)enemyState].isAnimating = true;

                //Play the roll windup sound
                baldurSnds[(int)BaldurSoundEffects.ROLL_WINDUP].CreateInstance().Play();
            }

            //Kill the baldur if their health reaches 0
            if (health == 0)
            {
                //Start the baldurs air death animation
                enemyState = EnemyState.DEATH;

                //Don't test colliision with the hitbox anymore
                testForCollision = false;

                //Stop the baldur in place
                speed *= 0f;

                //Stop playing the roll sound
                rollSnd.Stop();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Stops all repeatable sounds in the baldur
        public override void StopAllSounds()
        {
            //Stop the baldurs roll sound
            rollSnd.Stop();
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
            anims[(int)enemyState].destRec.Y = hitBox.Y - (int)hitboxOffset.Y + 5;

            //Update the left and right hitboxes (Set them below the baldur so collision between platforms under crawlid can be checked)
            leftHitBox.X = hitBox.X - hitBox.Width;
            leftHitBox.Y = hitBox.Y + 10;
            rightHitBox.X = hitBox.Right;
            rightHitBox.Y = hitBox.Y + 10;
        }

        //Pre: spriteBatch allows the baldur to be drawn, and transparancy is how transparent to draw the baldur
        //Post: N/A
        //Desc: Draws the baldur to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the baldur to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour * transparancy, drawDir);

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
