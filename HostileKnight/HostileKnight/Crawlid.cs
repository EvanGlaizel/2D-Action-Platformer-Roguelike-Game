//A: Evan Glaizel
//F: Crawlid.cs
//P: HostileKnight
//C: 2022/12/10
//M: 
//D: The enemy that crawls on the ground  until it hits a wall or platform edge

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
    class Crawlid : Enemy
    {
        //Store all of the states of the crawlid
        private enum EnemyState
        {
            WALK,
            TURN,
            DEATH
        }

        //Store the enemy state of the crawlid
        private EnemyState enemyState;

        //Store a left hitbox and right hitbox to detect when the enemy is coming out of an edge
        private Rectangle leftHitBox;
        private Rectangle rightHitBox;

        //Store a turn cooldown timer
        private Timer turnTimer;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the crawlid, particleImg is the image of the particles, animScale is the scale of the animations, 
               startLoc is the starting location of the crawlid, health is the health of the crawlid, maxSpeed is the max speed of the crawlid, weight is the weight of the crawlid, hitboxOffset is the hitbox 
               offset from the animation, sizeOffset is the hitbox size offset from the image, and enemySnds are the sounds of the enemy, and particleSnds are the sound effects of the particles */
        //Post: N/A
        //Desc: Constructs the crawlid
        public Crawlid(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Set the crawlids starting direction based on the speed
            if (speed.X > 0)
            {
                //Face the crawlid right
                drawDir = Animation.FLIP_HORIZONTAL;

                //Set the walk speed
                speed.X = maxSpeed;
            }
            else
            {
                //Face the crawlid left
                drawDir = Animation.FLIP_NONE;

                //Set the walk speed
                speed.X = -maxSpeed;
            }

            //Set the speed tolerance
            speedTolerance = maxSpeed * 0.2;

            //Set the left and right hitboxes to detect when the crawlid as at the edge
            leftHitBox = hitBox;
            rightHitBox = hitBox;

            //Set the turn timer
            turnTimer = new Timer(500, true);
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the crawlid animations
            anims = new Animation[3];
            anims[(int)EnemyState.WALK] = new Animation(imgs[(int)EnemyState.WALK], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 8, startLoc, animScale, true);
            anims[(int)EnemyState.TURN] = new Animation(imgs[(int)EnemyState.TURN], 1, 2, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 6, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH] = new Animation(imgs[(int)EnemyState.DEATH], 2, 3, 5, 0, 5, Animation.ANIMATE_ONCE, 6, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the crawlid
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the current animation
            anims[(int)enemyState].Update(gameTime);

            //Apply gravity to the crawlid
            ApplyGravity(maxSpeed * 9);

            //Update the game logic based on the enemy state
            switch (enemyState)
            {
                case EnemyState.WALK:
                    //Update the walk state
                    UpdateWalk(gameTime);
                    break;
                case EnemyState.TURN:
                    //Update the turn
                    UpdateTurn();
                    break;
            }

            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect);
        }

        //Pre: gameTime allows the timer to update
        //Post: N/A
        //Desc: Update the walk state
        private void UpdateWalk(GameTime gameTime)
        {
            //Update the turn timer
            turnTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Change the crawlids speed based on their direction and speed
            if (drawDir == Animation.FLIP_NONE)
            {
                //Work towards getting the crawlids speed to max
                BringToTargetSpeedX(-maxSpeed);
            }
            else if (drawDir == Animation.FLIP_HORIZONTAL)
            {
                //Work towards getting the crawlids speed to max
                BringToTargetSpeedX(maxSpeed);
            }
        }


        //Pre: N/A
        //Post: N/A
        //Desc: Update the turn state
        private void UpdateTurn()
        {
            //Send the crawlid back to walking once the turn animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Set the crawlid to a walk
                enemyState = EnemyState.WALK;

                //Reverse the crawlids direction based on its current direction
                if (drawDir == Animation.FLIP_NONE)
                {
                    //Face the crawlid right
                    drawDir = Animation.FLIP_HORIZONTAL;

                    //Move the crawlid to the right
                    speed.X = maxSpeed;
                }
                else
                {
                    //Face the crawlid left
                    drawDir = Animation.FLIP_NONE;

                    //Move the crawlid to the left
                    speed.X = -maxSpeed;
                }
            }
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and killEnemy tracks if the collision should kill the enemy
        //Post: N/A
        //Desc: Does basic collision detection for the crawlid 
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
                    //Do different things based on the part of the enemy that connected with it
                    if (Util.Intersects(hitBoxes[(int)BodyPart.LEGS], testedHitBox))
                    {
                        //Turn the crawlid around if their side rectangles aren't touching the wall the crawlid is on
                        if ((!Util.Intersects(leftHitBox, testedHitBox) || !Util.Intersects(rightHitBox, testedHitBox)) && enemyState == EnemyState.WALK && turnTimer.IsFinished())
                        {
                            //Move the crawlid off of the edge
                            hitBox.X += -Math.Sign(speed.X) * 2;

                            //Reset the turn timer
                            turnTimer.ResetTimer(true);

                            //Turn the crawlid around
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;
                            speed.X = 0f;
                        }

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
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.LEFT], testedHitBox))
                    {
                        //Set the enemy just to the right of the intersected rectangle
                        hitBox.X = testedHitBox.X + testedHitBox.Width;

                        //Move the crawlid off of the wall
                        hitBox.X += -Math.Sign(speed.X) * 5;

                        //Reset the crawlids speed
                        speed.X = 0f;

                        //Turn the crawlid if its not dead
                        if (enemyState != EnemyState.DEATH)
                        {
                            //Turn the crawlid
                            enemyState = EnemyState.TURN;
                            anims[(int)enemyState].isAnimating = true;
                        }
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox))
                    {
                        //Set the enemy just to the left pf the intersected rectangle
                        hitBox.X = testedHitBox.X - hitBox.Width;

                        //Move the crawlid off of the wall
                        hitBox.X += -Math.Sign(speed.X) * 5;

                        //Reset the crawlids speed
                        speed.X = 0f;

                        //Turn the crawlid if its not dead
                        if (enemyState != EnemyState.DEATH)
                        {
                            //Turn the crawlid
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
                    speed.X -= (float)(Math.Cos(hitAngle) * (maxSpeed * 3) / (weight));
                    speed.Y -= (float)(Math.Sin(hitAngle) * (maxSpeed * 3) / (weight));

                    //Damage the enemy
                    DamageEnemy();
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the mob, and decreases their health
        protected override void DamageEnemy()
        {
            //Damage the crawlid, and perform all logic related to that
            base.DamageEnemy();

            //Kill the crawlid if their health reaches 0
            if (health == 0)
            {
                //Start the crawlids death animation
                enemyState = EnemyState.DEATH;

                //Change the draw direciton based on the speed of the enemy
                CalcDir();

                //Reset the hitbox speed
                speed.X = 0;
                speed.Y = 0;

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

            //Update the animation location based on the state of the enemy
            if (enemyState == EnemyState.DEATH)
            {
                //Update the animation location
                anims[(int)enemyState].destRec.X = hitBox.X - hitBox.Width;
                anims[(int)enemyState].destRec.Y = hitBox.Y - (int)(hitboxOffset.Y / 2);
            }
            else
            {
                //Update the animation location
                anims[(int)enemyState].destRec.X = hitBox.X;
                anims[(int)enemyState].destRec.Y = hitBox.Y - (int)(hitboxOffset.Y / 2);
            }

            //Update the left and right hitboxes (Set them below the crawlid so collision between platforms under crawlid can be checked)
            leftHitBox.X = hitBox.X - hitBox.Width;
            leftHitBox.Y = hitBox.Y + 10;
            rightHitBox.X = hitBox.Right;
            rightHitBox.Y = hitBox.Y + 10;
        }

        //Pre: spriteBatch allows the crawlid to be drawn, and transparancy is how transparent to draw the crawlid
        //Post: N/A
        //Desc: Draws the crawlid to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the crawlid to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour * transparancy, drawDir);
            
            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
