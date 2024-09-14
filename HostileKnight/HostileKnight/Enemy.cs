//A: Evan Glaizel
//F: Enemy.cs
//P: HostileKnight
//C: 2022/12/5
//M: 
//D: The enemies that the player fights

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
    class Enemy
    {
        //Store the random number generator
        private Random rng = new Random();

        //Store the gravity
        protected const float GRAVITY = 38f / 60;

        //Store the different hitboxes of the enemy
        protected enum BodyPart
        {
            HEAD,
            LEFT,
            RIGHT,
            LEGS
        }

        //Store the different sound effects of the enemy
        protected enum SoundEffects
        {
            DAMAGE,
            DEATH
        }

        //Store the particle sound effects
        protected enum ParticleSoundEffects
        {
            CREATE,
            DESTROY
        }

        //Store the direction to draw the enemy
        protected SpriteEffects drawDir = Animation.FLIP_HORIZONTAL;

        //Store the emitter that manages all the particles
        protected Emitter emitter;

        //Store the animation scale
        protected float animScale;

        //Store all of the enemies images and animations
        protected Texture2D[] imgs;
        protected Animation[] anims;

        //Store the hitbox of the enemy
        protected Rectangle hitBox;
        protected Rectangle[] hitBoxes = new Rectangle[Enum.GetNames(typeof(BodyPart)).Length];

        //Store the colour to draw the enemy and the colour it turns to
        protected Color orange;
        protected Color enemyColour;

        //Store the health and max speed of the enemy
        protected int health;
        protected int maxSpeed;

        //Store the speed of the enenmy
        protected Vector2 speed;

        //Store the hitbox offset
        protected Vector2 hitboxOffset;

        //Store the angle at which the enemy is hit
        protected double hitAngle;

        //Store the weight of the enemy
        protected double weight;

        //Store a speed tolerance to set the enemies speed to the correct one if its close enough
        protected double speedTolerance;

        //Store if the mob should be tested for collision
        protected bool testForCollision = true;

        //Store the desired speed of the enemy (used for pathfinding)
        protected Vector2 desiredSpeed = new Vector2();

        //Store a timer to store a cooldown for when the enemy gets it
        protected Timer hitCooldown;

        //Store the line of sight for the enemy
        protected GameLine lineOfSight;
        protected bool calcLineOfSight = false;

        //Store the pathfinding path and a bool that determines if pathfinding should happen
        protected Vector2 pathLoc;
        protected bool pathFinding = false;

        //Store the sounds of the enemy
        protected SoundEffect[] enemySnds;
        protected SoundEffect[] particleSnds;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the enemy, particleImg is the image of the particles, animScale is the scale of the animations, 
               startLoc is the starting location of the enemy, health is the health of the enemy, maxSpeed is the max speed of the enemy, weight is the weight of the enemy, hitboxOffset is the hitbox 
               offset from the animation, sizeOffset is the hitbox size offset from the image, particleImg is the image of the particle, enemySnds are the sounds of the enemy, and particleSnds are the
               sound effects of the particles */
        //Post: N/A
        //Desc: Constructs the enemy
        public Enemy(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] particleSnds)
        {
            //Set the values of the enemy
            this.imgs = imgs;
            this.animScale = animScale;
            this.health = health;
            this.maxSpeed = maxSpeed;
            this.weight = weight;
            this.hitboxOffset = hitboxOffset;
            this.enemySnds = enemySnds;
            this.particleSnds = particleSnds;

            //Setup the emitter
            emitter = new Emitter(particleImg, particleSnds[(int)ParticleSoundEffects.CREATE]);

            //Set the hit cooldown
            hitCooldown = new Timer(400, false);

            //Set the enemies draw colour and the value of orange
            orange = new Color(255, 50, 0);
            enemyColour = Color.White;

            //Set the line of sight
            lineOfSight = new GameLine(gd, startLoc, new Vector2(0, 0));

            //Setup the animations
            SetupAnims(startLoc);

            //Setup the hitboxes
            SetupHitBoxes(startLoc, sizeOffset);
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected virtual void SetupAnims(Vector2 startLoc)
        {
        }

        //Pre: startLoc is the starting location of the enemy, and sizeOffset is the hitbox offset 
        //Post: N/A
        //Desc: Sets the default location of the hitboxes
        private void SetupHitBoxes(Vector2 startLoc, Vector2 sizeOffset)
        {
            //Setup the main hitbox of the enemy
            hitBox = new Rectangle((int)startLoc.X + (int)hitboxOffset.X, (int)startLoc.Y + (int)hitboxOffset.Y, anims[0].destRec.Width - (int)sizeOffset.X, anims[0].destRec.Height - (int)sizeOffset.Y);

            //Setup the other hitboxes of the enemy
            hitBoxes[(int)BodyPart.HEAD] = new Rectangle(hitBox.X + hitBox.Width / 4, hitBox.Y, hitBox.Width / 2, hitBox.Height / 4);
            hitBoxes[(int)BodyPart.LEFT] = new Rectangle(hitBox.X - 10, hitBoxes[(int)BodyPart.HEAD].Bottom, (hitBox.Width / 2) + 10, hitBox.Height / 2);
            hitBoxes[(int)BodyPart.RIGHT] = new Rectangle(hitBoxes[(int)BodyPart.LEFT].Right, hitBoxes[(int)BodyPart.HEAD].Bottom, (hitBox.Width / 2) + 10, hitBox.Height / 2);
            hitBoxes[(int)BodyPart.LEGS] = new Rectangle(hitBox.X + hitBox.Width / 4, hitBoxes[(int)BodyPart.LEFT].Bottom, hitBox.Width / 2, hitBox.Bottom - hitBoxes[(int)BodyPart.LEFT].Bottom);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the enemy
        public virtual void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the cooldown timer
            hitCooldown.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Reset the enemies colour if its been long enough
            if (hitCooldown.GetTimePassed() > 300 && hitCooldown.IsActive())
            {
                //Reset the enemies colour
                enemyColour = Color.White;
            }

            //Update the emitter
            emitter.Update(gameTime, playerRect);

            //Set the location of the main hitbox
            hitBox.X += (int)speed.X;
            hitBox.Y += (int)speed.Y;

            //Update the enemies position
            UpdateGamePos();
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and killEnemy tracks if the collision should kill the enemy
        //Post: N/A
        //Desc: Does basic collision detection for the enemy (Doesn't let them fall through floors or walls)
        public virtual void TestCollision(Rectangle testedHitBox, bool killEnemy)
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
                        //Set the enemy just ontop of the intersected rectangle
                        hitBox.Y = testedHitBox.Y - hitBox.Height;
                        speed.Y = 0f;
                    }
                    if (Util.Intersects(hitBoxes[(int)BodyPart.HEAD], testedHitBox))
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
        public virtual void TestAttackCollision(Rectangle testedHitBox, Rectangle playerHitBox)
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

                    //Change the direction of the enemy based on its speed
                    CalcDir();

                    //Damage the enemy
                    DamageEnemy();
                }
            }
        }

        //Pre: playerRect is the rectangle of the player to put the line of sight on
        //Post: N/A
        //Desc: Repositions the game line based on the player
        public void RecalculateLineOfSight(Rectangle playerRect)
        {
            //Recalculate the line of sight if it should be calculated and the enemy is facing the player
            if (calcLineOfSight && (drawDir == Animation.FLIP_NONE && playerRect.X < hitBox.X) || (drawDir == Animation.FLIP_HORIZONTAL && playerRect.X > hitBox.X))
            {
                //Reset the line of sight
                lineOfSight.SetPt1(new Vector2(hitBox.Center.X, hitBox.Center.Y));
                lineOfSight.SetPt2(new Vector2(playerRect.Center.X, playerRect.Center.Y));
            }
            else
            {
                //Put the line of sight in a block
                lineOfSight.SetPt1(new Vector2(0, 0));
            }
        }

        //Pre: tiles is a list of all the rectangles of the platform the line of sight is testing collision against
        //Post: N/A
        //Desc: Calculates the tiles that are in the way of the line of sight
        public void CalcLineOfSightCollision(List<Tile> tiles)
        {
            //Test collision on the line of sight if it should be tested
            if (calcLineOfSight)
            {
                //Store if there is something in the way of the line of sight
                bool lineObstructed = false;

                //Loop through each tile to test collision between the line of sight and the tiles
                for (int i = 0; i < tiles.Count; i++)
                {
                    //Test line of sight collision on each tile
                    if (Util.Intersects(tiles[i].GetHitBox(), lineOfSight))
                    {
                        //There is something in the way of the line of sight
                        lineObstructed = true;
                        break;
                    }
                }

                //Chase the player if the line of sight isnt obstructed
                if (!lineObstructed)
                {
                    //Don't calculate the line of sight anymore
                    calcLineOfSight = false;
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: 

        //Pre: N/A
        //Post: N/A
        //Desc: Updates all hitboxes and animations to the correct frame
        public virtual void UpdateGamePos()
        {
            //Set the location of the specific hitboxes in relation to the main hitbox
            hitBoxes[(int)BodyPart.HEAD].X = hitBox.X + hitBox.Width / 4;
            hitBoxes[(int)BodyPart.HEAD].Y = hitBox.Y;
            hitBoxes[(int)BodyPart.LEFT].X = hitBox.X - 10;
            hitBoxes[(int)BodyPart.LEFT].Y = hitBoxes[(int)BodyPart.HEAD].Bottom;
            hitBoxes[(int)BodyPart.RIGHT].X = hitBoxes[(int)BodyPart.LEFT].Right;
            hitBoxes[(int)BodyPart.RIGHT].Y = hitBoxes[(int)BodyPart.HEAD].Bottom;
            hitBoxes[(int)BodyPart.LEGS].X = hitBox.X + hitBox.Width / 4;
            hitBoxes[(int)BodyPart.LEGS].Y = hitBoxes[(int)BodyPart.LEFT].Bottom;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the mob, and decreases their health
        protected virtual void DamageEnemy()
        {
            //Decrease the mobs health
            health--;

            //Start the hit cooldown timer
            hitCooldown.ResetTimer(true);

            //Make the enemy orange for improved visual clarity
            enemyColour = orange;

            //Create a death particle if the enemy is at 0 health
            if (health == 0)
            {
                //Create a death particle
                emitter.CreateParticle(hitBox.Center.ToVector2(), 0, 5, rng.Next(0, 361) * Math.PI / 180, 0, 0.67f, Color.White, 50, Emitter.ParticleType.DEATH);

                //Play the death sound
                enemySnds[(int)SoundEffects.DEATH].CreateInstance().Play();
            }

            //Create particles for better visual effect
            for (int i = 0; i < 8; i++)
            {
                //Create a particle
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), rng.Next(0, 361) * Math.PI / 180, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
            }

            //Play the damage sound
            enemySnds[(int)SoundEffects.DAMAGE].CreateInstance().Play();
        }

        //Pre: targetSpeed is the target speed to bring the enemy back to
        //Post:
        //Desc: Brings the enemy back to its target X speed
        protected void BringToTargetSpeedX(float targetSpeed)
        {
            //Work towards getting the enemies x speed to the target speed
            if (speed.X > targetSpeed)
            {
                //Slow down the enemy
                speed.X -= (float)speedTolerance;

                //Bring the enemy to its target X speed if it's close enough
                if (speed.X <= targetSpeed + speedTolerance)
                {
                    //Bring the enemy to its target X speed
                    speed.X = targetSpeed;
                }
            }
            else if (speed.X < targetSpeed)
            {
                //Speed up the enemy
                speed.X += (float)speedTolerance;

                //Bring the enemy to its target X speed if it's close enough
                if (speed.X >= targetSpeed - speedTolerance)
                {
                    //Bring the enemy to its target X speed
                    speed.X = targetSpeed;
                }
            }
        }

        //Pre: targetSpeed is the target speed to bring the enemy back to
        //Post:
        //Desc: Brings the enemy back to its target Y speed
        protected void BringToTargetSpeedY(float targetSpeed)
        {
            //Work towards getting the enemies y speed to the target speed
            if (speed.Y > targetSpeed)
            {
                //Slow down the enemy
                speed.Y -= (float)speedTolerance;

                //Bring the enemy to its target Y speed if it's close enough
                if (speed.Y <= targetSpeed + speedTolerance)
                {
                    //Bring the enemy to its target Y speed
                    speed.Y = targetSpeed;
                }
            }
            else if (speed.Y < targetSpeed)
            {
                //Speed up the enemy
                speed.Y += (float)speedTolerance;

                //Bring the enemy to its target Y speed if it's close enough
                if (speed.Y >= targetSpeed - speedTolerance)
                {
                    //Bring the enemy to its target Y speed
                    speed.Y = targetSpeed;
                }
            }
        }

        //Pre: N/A
        //Post: Returns a bool that represents if the mob should be killed
        //Desc: Tells the room if the mob should be deleted
        public virtual bool KillEnemy()
        {
            //Returns true if the mob is dead and has no particles or has somehow escaped the boundaries of the screen
            return ((health == 0 && emitter.GetCount() == 0) || hitBox.Right < 0 || hitBox.Bottom < 0 || hitBox.X > 1260 || hitBox.Y > 720);
        }

        //Pre: speedCap is the max speed the enemy can fall at
        //Post: N/A
        //Desc: Applies gravity to the enemy
        protected void ApplyGravity(int speedCap)
        {
            //Apply gravity to the gruzzer if they're under the maximum falling speed threshold
            if (speed.Y < speedCap)
            {
                //Apply gravity to the gruzzer
                speed.Y += GRAVITY;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Switches the direction the enemy is facing
        protected void SwitchDir()
        {
            //Flip the enemies direction
            if (drawDir == Animation.FLIP_NONE)
            {
                //Face the enemy right
                drawDir = Animation.FLIP_HORIZONTAL;
            }
            else
            {
                //Face the enemy left
                drawDir = Animation.FLIP_NONE;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Calcualates the direction the enemy is facing based on speed
        protected void CalcDir()
        {
            //Flip the enemies direction
            if (speed.X > 0)
            {
                //Face the enemy right
                drawDir = Animation.FLIP_HORIZONTAL;
            }
            else if (speed.X < 0)
            {
                //Face the enemy left
                drawDir = Animation.FLIP_NONE;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Calcualates the direction the enemy is facing based on the player location
        protected void CalcDir(Rectangle playerRect)
        {
            //Flip the enemies direction
            if (playerRect.Center.X > hitBox.Center.X)
            {
                //Face the enemy right
                drawDir = Animation.FLIP_HORIZONTAL;
            }
            else if (playerRect.Center.X < hitBox.Center.X)
            {
                //Face the enemy left
                drawDir = Animation.FLIP_NONE;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Calcualates the direction the enemy is facing based on the player location
        public void CalcDir(Vector2 playerLoc)
        {
            //Flip the enemies direction
            if (playerLoc.X > hitBox.Center.X)
            {
                //Face the enemy right
                drawDir = Animation.FLIP_HORIZONTAL;
            }
            else if (playerLoc.X < hitBox.Center.X)
            {
                //Face the enemy left
                drawDir = Animation.FLIP_NONE;
            }
        }

        //Pre: N/A
        //Post: Return the hitbox of the enemy
        //Desc: Return the hitbox of the enemy
        public virtual Rectangle GetHitBox()
        { 
            //Return the hitbox of the enemy if the enemy should be collided with
            if (testForCollision)
            {
                //Return the hitbox of the enemy
                return hitBox;
            }
            //Return nothing
            return Rectangle.Empty;
        }

        //Pre: N/A
        //Post: Return the health of the enemy
        //Desc: Returns the current health left of the enemy
        public int GetHealth()
        {
            //Return the health of the enemy
            return health;
        }

        //Pre: N/A
        //Post: Return the pathfinding variable to the room
        //Desc: Tells the pathfinding code if pathfinding should be calculated for this enemy
        public bool FindPath()
        {
            //Return the pathfinding state
            return pathFinding;
        }

        //Pre: new path is the new location on the path for the enemy to follow
        //Post: N/A
        //Desc: Gets the path for the enemy to follow
        public void CalcPath(List<Vector2> newPath)
        {
            //Reset the path if there is a new one
            if (newPath.Count > 0)
            {
                //Set the path
                pathLoc = newPath[0];
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Stops all repeatable sounds in the enemy
        public virtual void StopAllSounds()
        {
            //The base enemy does not have any repeatable sounds
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Brings the enemy to the next spot in the path
        protected void MoveOnPath()
        {
            //Move the enemy along their path based on if they're at their path
            if (!(pathLoc.X == hitBox.Center.X && hitBox.Center.Y == pathLoc.Y))
            {
                //Set the distance between the hitbox and the path location
                Vector2 dist = new Vector2();

                //Set the distance between the x and y values
                dist.X = hitBox.Center.X - pathLoc.X;
                dist.Y = hitBox.Center.Y - pathLoc.Y;

                //Normalize the distance to calculate the exact spot the enemy needs to move to catch up with the next spot on the path
                //dist.Normalize();

                //Move the enemy to the right location
                //hitBox.X -= (int)(dist.X * maxSpeed);
                //hitBox.Y -= (int)(dist.Y * maxSpeed);

                //Set the speed the enemy wants to move at
                desiredSpeed.X -= (int)dist.X;
                desiredSpeed.Y -= (int)dist.Y;

                //Normalize the speed to stop it from moving fast
                if (desiredSpeed.X != 0 && desiredSpeed.Y != 0)
                {
                    //Move the enemy to the right speed
                    desiredSpeed.Normalize();
                    desiredSpeed *= maxSpeed;
                }
                
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Removes the soul particle if it should be removed
        public void RemoveSoulParticle()
        {
            //Loop through each particle and remove the soul particle
            for (int i = 0; i < emitter.GetCount(); i++)
            {
                //Remove the particle hitbox if its a soul particle
                if (emitter.GetParticles()[i] is SoulParticle)
                {
                    //Remove the soul particle
                    ((SoulParticle)emitter.GetParticles()[i]).ActivateKill();

                    //Play the soul particle kill sound effect
                    particleSnds[(int)ParticleSoundEffects.DESTROY].CreateInstance().Play();
                }
            }
        }

        //Pre: N/A
        //Post: returns the soul particle hitbox of the enemy
        //Desc: Gets and returns the enemies soul particle hitbox
        public Rectangle GetSoulParticleHitBox()
        {
            //Loop through the emmiters particles to return the soul particles hitbox
            for (int i = 0; i < emitter.GetCount(); i++)
            {
                //Return the particle hitbox if its a soul particle
                if (emitter.GetParticles()[i] is SoulParticle)
                {
                    //Return the hitbox of the soul particle
                    return emitter.GetParticles()[i].GetHitBox();
                }
            }

            //Return an empty rectangle. The enemy doesn't have a soul particle
            return Rectangle.Empty;
        }

        //Pre: spriteBatch allows the enemy to be drawn to the screen, and transparancy is how transparent to draw the enemy
        //Post: N/A
        //Desc: Draws the enemy to the screen
        public virtual void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the particles to the screen
            emitter.Draw(spriteBatch, transparancy);
        }
    }
}
