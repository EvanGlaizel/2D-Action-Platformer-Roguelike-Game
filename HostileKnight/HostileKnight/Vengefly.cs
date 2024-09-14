//A: Evan Glaizel
//F: Vengefly.cs
//P: HostileKnight
//C: 2022/12/10
//M: 
//D: A basic enemy that tracks the player once it sees it

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
    class Vengefly : Enemy
    {
        //Store all of the states of the vengefly
        private enum EnemyState
        {
            IDLE,
            AGRO,
            CHASE,
            TURN,
            DEATH
        }

        //Store the enemy state of the vengefly
        private EnemyState enemyState;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the vengefly, particleImg is the image of the particles, animScale is the scale of the animations, 
               startLoc is the starting location of the vengefly,health is the health of the vengefly, maxSpeed is the max speed of the vengefly, weight is the weight of the vengefly, hitboxOffset is the
               hitbox offset from the animation, sizeOffset is the hitbox size offset from the image, and enemySnds are the sounds of the enemy, and particleSnds are the sound effects of the particles */
        //Post: N/A
        //Desc: Constructs the vengefly
        public Vengefly(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Face the vengefly left
            drawDir = Animation.FLIP_NONE;

            //Calculate the line of sight
            calcLineOfSight = true;

            //Set the speed to bring the vengefly to its desired speed during pathfinding
            speedTolerance = maxSpeed * 0.1;
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the vengefly animations
            anims = new Animation[5];
            anims[(int)EnemyState.IDLE] = new Animation(imgs[(int)EnemyState.IDLE], 2, 3, 5, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 7, startLoc, animScale, true);
            anims[(int)EnemyState.AGRO] = new Animation(imgs[(int)EnemyState.AGRO], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, false);
            anims[(int)EnemyState.CHASE] = new Animation(imgs[(int)EnemyState.CHASE], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 7, startLoc, animScale, true);
            anims[(int)EnemyState.TURN] = new Animation(imgs[(int)EnemyState.TURN], 1, 2, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH] = new Animation(imgs[(int)EnemyState.DEATH], 2, 2, 3, 0, 3, Animation.ANIMATE_ONCE, 6, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the vengefly
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
                case EnemyState.AGRO:
                    //Update the agro state
                    UpdateAgro();
                    break;
                case EnemyState.CHASE:
                    //Update the chase state
                    UpdateChase(playerRect);
                    break;
                case EnemyState.TURN:
                    //Update the turn state
                    UpdateTurn(playerRect);
                    break;
                case EnemyState.DEATH:
                    //Update the death state
                    UpdateDeath();
                    break;
            }

            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect); 
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the idle state
        private void UpdateIdle()
        {     
            //Agro the vengefly if the line of sight isn't being tested anymore (player has been spotted)
            if (!calcLineOfSight)
            {
                //Agro the vengefly
                enemyState = EnemyState.AGRO;
                anims[(int)enemyState].isAnimating = true;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the agro state
        private void UpdateAgro()
        {
            //Send the vengefly back to walking once the agro animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Set the vengefly to chase the player
                enemyState = EnemyState.CHASE;

                //Start pathfinding towards the player
                pathFinding = true;
            }
        }

        //Pre: playerRect is the players hitbox
        //Post: N/A
        //Desc: Update the chase state
        private void UpdateChase(Rectangle playerRect)
        {
            //Move the vengefly along the path
            MoveOnPath();

            //Slowly bring the vengefly to its desired speed (On the pathfinding path)
            BringToTargetSpeedX(desiredSpeed.X);
            BringToTargetSpeedY(desiredSpeed.Y);

            //Turn the vengefly if it needs to be turned
            if ((drawDir == Animation.FLIP_NONE && playerRect.Center.X > hitBox.Center.X) || (drawDir == Animation.FLIP_HORIZONTAL && playerRect.Center.X < hitBox.Center.X))
            {
                //Turn the vengefly 
                enemyState = EnemyState.TURN;
                anims[(int)enemyState].isAnimating = true;
            }
        }

        //Pre: playerRect is the players hitbox
        //Post: N/A
        //Desc: Update the turn state
        private void UpdateTurn(Rectangle playerRect)
        {
            //Move the vengefly along the path
            MoveOnPath();

            //Slowly bring the vengefly to its desired speed (On the pathfinding path)
            BringToTargetSpeedX(desiredSpeed.X);
            BringToTargetSpeedY(desiredSpeed.Y);

            //Go back to the agro state if the turn is complete
            if (!anims[(int)enemyState].isAnimating)
            {
                //Continue chasing the player
                enemyState = EnemyState.CHASE;

                //Switch the direction of the vengefly
                SwitchDir();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the death state
        private void UpdateDeath()
        {
            //Apply gravity to the vengefly
            ApplyGravity(maxSpeed * 5);
        }

        //Pre: testedHitBox is the rectangle the vengefly is testing collision against, and killvengefly tracks if the collision should kill the vengefly
        //Post: N/A
        //Desc: Does basic collision detection for the vengefly (Doesn't let them fall through floors or walls)
        public override void TestCollision(Rectangle testedHitBox, bool killEnemy)
        {
            //Only check the body part that collided with the vengefly if the main hitbox collides
            if (Util.Intersects(hitBox, testedHitBox))
            {
                //Kill the vengefly if they should be killed
                if (killEnemy && health > 0)
                {
                    //Propel the vengefly backwards, and kill them (damage vengefly brings their health down one more)
                    speed *= -1;
                    health = 1;
                    DamageEnemy();
                }
                else
                {
                    //Do different things based on the part of the vengefly that connected with it
                    if (Util.Intersects(hitBoxes[(int)BodyPart.LEGS], testedHitBox))
                    {
                        //Set the vengefly just ontop of the intersected rectangle
                        hitBox.Y = testedHitBox.Y - hitBox.Height;
                        speed.Y = 0f;
                        speed.X = Math.Sign(pathLoc.X - hitBox.X) * maxSpeed;

                        //If the vengefly is dead, stop them on the ground
                        if (enemyState == EnemyState.DEATH)
                        {
                            //Stop the vengeflys speed on the ground to stop them from sliding
                            speed.X = 0f;
                        }
                    }
                    if (Util.Intersects(hitBoxes[(int)BodyPart.HEAD], testedHitBox))
                    {
                        //Set the vengefly just below the intersected rectangle
                        hitBox.Y = testedHitBox.Y + testedHitBox.Height;
                        speed.Y = 0f;
                        speed.X = Math.Sign(pathLoc.X - hitBox.X) * maxSpeed;
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.LEFT], testedHitBox))
                    {
                        //Set the vengefly just to the right of the intersected rectangle
                        hitBox.X = testedHitBox.X + testedHitBox.Width;
                        speed.X = 0f;
                        speed.Y = Math.Sign(pathLoc.Y - hitBox.Y) * maxSpeed;
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox))
                    {
                        //Set the vengefly just to the left pf the intersected rectangle
                        hitBox.X = testedHitBox.X - hitBox.Width;
                        speed.X = 0f;
                        speed.Y = Math.Sign(pathLoc.Y - hitBox.Y) * maxSpeed;
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
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the vengefly, and decreases their health
        protected override void DamageEnemy()
        {
            //Damage the vengefly, and perform all logic related to that
            base.DamageEnemy();

            //Turn the vengefly if its idling and it can't see the player
            if (enemyState == EnemyState.IDLE)
            {
                //Turn the vengefly and agro it
                enemyState = EnemyState.TURN;
                anims[(int)enemyState].isAnimating = true;

                //Start pathfinding towards the player
                pathFinding = true;
            }

            //Kill the vengefly if their health reaches 0
            if (health == 0)
            {
                //Start the vengeflys air death animation
                enemyState = EnemyState.DEATH;

                //Multiply the vengeflys speed to get a more powerful death effect
                speed *= 2;

                //Dont test for collision with the vengefly
                testForCollision = false;

                //Stop tracking the player
                pathFinding = false;
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

        //Pre: spriteBatch allows the vengefly to be drawn, and transparancy is how transparent to draw the vengefly
        //Post: N/A
        //Desc: Draws the vengefly to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the vengefly to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour * transparancy, drawDir);

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
