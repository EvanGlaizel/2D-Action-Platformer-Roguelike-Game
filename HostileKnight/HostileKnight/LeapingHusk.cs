//A: Evan Glaizel
//F: LeapingHusk.cs
//P: HostileKnight
//C: 2022/12/19
//M: 
//D: An enemy that jumps at the player once it sees it

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
    class LeapingHusk : Enemy
    {
        //Store the jump speed of the leaping husk
        private const int JUMP_SPEED = 15;

        //Store all of the states of the leaping husk
        private enum EnemyState
        {
            WALK,
            JUMP,
            ATTACK,
            TURN,
            DEATH_AIR,
            DEATH_GROUND
        }

        //Store each leaping husk sound effect type
        private enum LeapingHuskSoundEffects
        {
            JUMP,
            LAND
        }


        //Store the enemy state of the leaping husk
        private EnemyState enemyState;

        //Store a left hitbox and right hitbox to detect when the enemy is coming to an edge
        private Rectangle leftHitBox;
        private Rectangle rightHitBox;

        //Store a cooldown on the leaping husks jumps
        private Timer jumpcdTimer;

        //Store a turn cooldown timer
        private Timer turnTimer;

        //Store the sound effects of the leaping husk
        private SoundEffect[] leapingHuskSnds;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the leaping husk, particleImg is the image of the particles, animScale is the scale of the animations, 
               startLoc is the starting location of the leaping husk, health is the health of the leaping husk, maxSpeed is the max speed of the leaping husk, weight is the weight of the leaping husk, hitboxOffset
               is the hitbox offset from the animation, sizeOffset is the hitbox size offset from the image, and enemySnds are the sounds of the enemy, leapingHuskSnds are the sound effects of the leaping husk, 
               and particleSnds are the sound effects of the particles */
        //Post: N/A
        //Desc: Constructs the leaping husk
        public LeapingHusk(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] leapingHuskSnds, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Set the leaping husk sounds
            this.leapingHuskSnds = leapingHuskSnds;

            //Face the leaping husk left
            drawDir = Animation.FLIP_NONE;

            //Calculate the line of sight
            calcLineOfSight = true;

            //Set the tolerance level
            speedTolerance = maxSpeed * 0.2;

            //Set the left and right hitboxes to detect when the leaping husk as at the edge
            leftHitBox = hitBox;
            rightHitBox = hitBox;

            //Set the jump cooldown timer
            jumpcdTimer = new Timer(2000, true);

            //Set the turn timer
            turnTimer = new Timer(1000, true);
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the leaping husk animations
            anims = new Animation[6];
            anims[(int)EnemyState.WALK] = new Animation(imgs[(int)EnemyState.WALK], 3, 3, 7, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 8, startLoc, animScale, true);
            anims[(int)EnemyState.JUMP] = new Animation(imgs[(int)EnemyState.JUMP], 4, 3, 11, 0, 11, Animation.ANIMATE_ONCE, 12, startLoc, animScale, false);
            anims[(int)EnemyState.ATTACK] = new Animation(imgs[(int)EnemyState.ATTACK], 2, 1, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 15, startLoc, animScale, false);
            anims[(int)EnemyState.TURN] = new Animation(imgs[(int)EnemyState.TURN], 2, 1, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 10, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH_AIR] = new Animation(imgs[(int)EnemyState.DEATH_AIR], 1, 1, 1, 0, 0, 0, 1, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH_GROUND] = new Animation(imgs[(int)EnemyState.DEATH_GROUND], 3, 3, 8, 0, 8, Animation.ANIMATE_ONCE, 10, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the leaping husk
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the current animation
            anims[(int)enemyState].Update(gameTime);

            //Update the jump timer
            jumpcdTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Update the turn timer
            turnTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Update the game logic based on the enemy state
            switch (enemyState)
            {
                case EnemyState.WALK:
                    //Update the walk state
                    UpdateWalk();
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

            //Apply gravity to the leaping husk
            ApplyGravity(maxSpeed * 9);
            
            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the walk state
        private void UpdateWalk()
        {
            //Change the leaping husks speed based on their direction and speed
            if (drawDir == Animation.FLIP_NONE)
            {
                //Work towards getting the leaping husks speed to max
                BringToTargetSpeedX(-maxSpeed);
            }
            else if (drawDir == Animation.FLIP_HORIZONTAL)
            {
                //Work towards getting the leaping husks speed to max
                BringToTargetSpeedX(maxSpeed);
            }

            //Agro the leaping husk if the line of sight isn't being tested anymore (player has been spotted), isn't moving in the y direction, and is done the cooldown timer
            if (!calcLineOfSight && speed.Y == 0 && jumpcdTimer.IsFinished())
            {
                //Start the leaping husks jump
                enemyState = EnemyState.JUMP;
                anims[(int)enemyState].isAnimating = true;

                //Launch the enemy towards the player
                speed.Y -= JUMP_SPEED;
                speed.X = Math.Sign(speed.X) * 3;

                //Reset the jump cooldown timer
                jumpcdTimer.ResetTimer(true);

                //Play the jump sound
                leapingHuskSnds[(int)LeapingHuskSoundEffects.JUMP].CreateInstance().Play();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the attack state
        private void UpdateAttack()
        {
            //Don't let the enemy move during the attack
            speed.X = 0;

            //Go back to idling if the turn animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Put the leaping husk back in walk mode
                enemyState = EnemyState.WALK;

                //Calculate the line of sight again
                calcLineOfSight = true;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the turn state
        private void UpdateTurn()
        {
            //Go back to walking if the turn animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Put the leaping husk back in walk mode
                enemyState = EnemyState.WALK;

                //Switch the direction of the leaping husk
                SwitchDir();
            }
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

                        //Turn the leaping husk around if their side rectangles aren't touching the wall the leaping husk is on, and the enemy is walking
                        if ((!Util.Intersects(leftHitBox, testedHitBox) || !Util.Intersects(rightHitBox, testedHitBox)) && enemyState == EnemyState.WALK && turnTimer.IsFinished())
                        {
                            //Move the leaping husk off of the edge
                            hitBox.X += -Math.Sign(speed.X) * 10;

                            //Reset the turn timer
                            turnTimer.ResetTimer(true);

                            //Turn the leaping husk around
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;
                            speed.X = 0f;
                        }

                        //Finish the leaping husks jump if they touch the ground
                        if (enemyState == EnemyState.JUMP)
                        {
                            //Bring the leaping husk back on the ground
                            enemyState = EnemyState.ATTACK;
                            anims[(int)enemyState].isAnimating = true;
                            speed.X = 0;

                            //Play the landing sound
                            leapingHuskSnds[(int)LeapingHuskSoundEffects.LAND].CreateInstance().Play();
                        }

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

                        //Move the leaping husk off of the wall
                        hitBox.X += -Math.Sign(speed.X) * 5;

                        //Reset the leaping husks speed
                        speed.X = 0f;

                        //Turn the leaping husk around if it's walking
                        if (enemyState == EnemyState.WALK)
                        {
                            //Stop the leaping husks attack and turn them
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;
                        }

                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox))
                    {
                        //Set the enemy just to the left pf the intersected rectangle
                        hitBox.X = testedHitBox.X - hitBox.Width;

                        //Move the leaping husk off of the wall
                        hitBox.X += -Math.Sign(speed.X) * 5;

                        //Reset the leaping husks speed
                        speed.X = 0f;

                        //Turn the leaping husk around if it's walking
                        if (enemyState == EnemyState.WALK)
                        {
                            //Stop the leaping husks attack and turn them
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;
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
            //Only test collision if the attack cooldown is up and the leaping husk is not dead
            if (!hitCooldown.IsActive() && enemyState != EnemyState.DEATH_AIR && enemyState != EnemyState.DEATH_GROUND)
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
                    speed.X -= (float)(Math.Cos(hitAngle) * (maxSpeed * 3) / (weight));
                    speed.Y -= (float)(Math.Sin(hitAngle) * (maxSpeed * 3) / (weight));

                    //Damage the enemy
                    DamageEnemy();
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the leaping husk, and decreases their health
        protected override void DamageEnemy()
        {
            //Damage the leaping husk, and perform all logic related to that
            base.DamageEnemy();

            //If the leaping husk is walking or turning, and the jump cooldown is up, start the jump
            if ((enemyState == EnemyState.WALK || enemyState == EnemyState.TURN) && jumpcdTimer.IsFinished())
            {
                //Start the leaping husks roll
                enemyState = EnemyState.JUMP;
                anims[(int)enemyState].isAnimating = true;

                //Launch the enemy towards the player
                speed.Y -= JUMP_SPEED;
                speed.X = Math.Sign(speed.X) * 3;

                //Reset the jump cooldown timer
                jumpcdTimer.ResetTimer(true);

                //Play the jump sound
                leapingHuskSnds[(int)LeapingHuskSoundEffects.JUMP].CreateInstance().Play();
            }

            //Kill the leaping husk if their health reaches 0
            if (health == 0)
            {
                //Start the leaping husks air death animation
                enemyState = EnemyState.DEATH_AIR;

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
            anims[(int)enemyState].destRec.Y = hitBox.Y - (int)hitboxOffset.Y + 5;

            //Update the left and right hitboxes (Set them below the leaping husk so collision between platforms under crawlid can be checked)
            leftHitBox.X = hitBox.X - hitBox.Width;
            leftHitBox.Y = hitBox.Y + 10;
            rightHitBox.X = hitBox.Right;
            rightHitBox.Y = hitBox.Y + 10;
        }

        //Pre: spriteBatch allows the leaping husk to be drawn, and transparancy is how transparent to draw the leaping husk
        //Post: N/A
        //Desc: Draws the leaping husk to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the leaping husk to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour, drawDir);

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
