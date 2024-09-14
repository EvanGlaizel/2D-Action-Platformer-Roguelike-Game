//A: Evan Glaizel
//F: AspidHunter.cs
//P: HostileKnight
//C: 2022/12/20
//M: 
//D: An enemy that shoots a projectile at the player

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
    class AspidHunter : Enemy
    {
        //Store all of the states of the aspid hunter
        private enum EnemyState
        {
            IDLE,
            SHOOT,
            DEATH
        }

        //Store the type of projectile sounds
        private enum ProjectileSoundEffects
        {
            DESTROY,
            FIRE
        }


        //Store the enemy state of the aspid hunter
        private EnemyState enemyState;

        //Store the shoot cooldown timer
        private Timer shootTimer;

        //Store the projectile image
        private Texture2D projectileImg;

        //Store the angle to shoot the projectile
        private double shootAngle;
        private bool calcShootAngle = true;

        //Store a list of the aspid hunters projectiles
        List<Projectile> projectiles = new List<Projectile>();

        //Store the sound effect of the projectile
        private SoundEffect[] projectileSnds;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the aspid hunter, particleImg is the image of the particles, animScale is the scale of the animations, 
               startLoc is the starting location of the aspid hunter, health is the health of the aspid hunter, maxSpeed is the max speed of the aspid hunter, weight is the weight of the aspid hunter, hitboxOffset
               is the hitbox offset from the animation, hitboxOffset is the hitbox offset from the animation, sizeOffset is the hitbox size offset from the image, enemySnds are the sounds of the enemy, and projectileSnds
               is the sound effects of the aspid hunters projectiles, and particleSnds are the sound effects of the particles*/
        //Post: N/A
        //Desc: Constructs the aspid hunter
        public AspidHunter(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] projectileSnds, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Set the projectile sound effect of the aspid hunter
            this.projectileSnds = projectileSnds;

            //Set the projectile image
            projectileImg = particleImg;

            //Face the aspid hunter left
            drawDir = Animation.FLIP_NONE;

            //Calculate the line of sight
            calcLineOfSight = true;

            //Set the shoot timer
            shootTimer = new Timer(1500, false);

            //Set the speed tolerance
            speedTolerance = 0.2f;
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the aspid hunter animations
            anims = new Animation[3];
            anims[(int)EnemyState.IDLE] = new Animation(imgs[(int)EnemyState.IDLE], 2, 3, 6, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 10, startLoc, animScale, true);
            anims[(int)EnemyState.SHOOT] = new Animation(imgs[(int)EnemyState.SHOOT], 3, 4, 12, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 8, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH] = new Animation(imgs[(int)EnemyState.DEATH], 2, 3, 6, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 8, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the aspid hunter
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
                case EnemyState.DEATH:
                    //Update the death state
                    UpdateDeath();
                    break;
            }

            //Loop through each projectile and update it
            for (int i = 0; i < projectiles.Count; i++)
            {
                //Update each projectile
                projectiles[i].Update(gameTime);
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
            shootTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Agro the aspid hunter if the line of sight isn't being tested anymore (player has been spotted) and the shoot cooldown is up
            if (!calcLineOfSight && !shootTimer.IsActive())
            {
                //Start the shot of the aspid hunter
                enemyState = EnemyState.SHOOT;
                anims[(int)enemyState].isAnimating = true;
            }

            //Work towards getting the aspid hunters speed to 0
            BringToTargetSpeedX(0);
            BringToTargetSpeedY(0);
        }

        //Pre: playerRect is the rectangle of the player
        //Post: N/A
        //Desc: Update the attack state
        private void UpdateAttack(Rectangle playerRect)
        {
            //If it's the 4th frame and an angle hasn't been calculated yet, calculate the hit angle
            if (anims[(int)enemyState].curFrame == 6 && calcShootAngle)
            {
                //Don't calculate the shoot angle
                shootAngle = Math.Atan2(playerRect.X - hitBox.X, playerRect.Y - hitBox.Y);

                //Don't calculate the shoot angle because it's already been calculated
                calcShootAngle = false;
            }

            //Shoot the attack if the aspid hunter is on its 9th frame
            if (anims[(int)enemyState].curFrame == 9 && !shootTimer.IsActive())
            {
                //Reset the shoot timer
                shootTimer.ResetTimer(true);

                //Shoot a projectile
                projectiles.Add(new Projectile(projectileImg, new Vector2(hitBox.Center.X, hitBox.Y), 8, shootAngle));

                //Play the projectile shoot sound
                projectileSnds[(int)ProjectileSoundEffects.FIRE].CreateInstance().Play();

                //Allow the shoot angle to be calculated again because it was used
                calcShootAngle = true;
            }

            //Send the aspid hunter back to walking once the attack animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Idle the player again
                enemyState = EnemyState.IDLE;

                //Calculate the line of sight
                calcLineOfSight = true;

                //Switch the aspid hunters direction based on its direction to the player
                CalcDir(playerRect);
            }

            //Work towards getting the aspid hunters speed to 0
            BringToTargetSpeedX(0);
            BringToTargetSpeedY(0);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the death state
        private void UpdateDeath()
        {
            //Apply gravity to the aspid hunter
            ApplyGravity(maxSpeed * 5);
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and killEnemy tracks if the collision should kill the enemy
        //Post: N/A
        //Desc: Does basic collision detection for the enemy (Doesn't let them fall through floors or walls)
        public override void TestCollision(Rectangle testedHitBox, bool killEnemy)
        {
            //Do basic collision detection
            base.TestCollision(testedHitBox, killEnemy);

            //Loop through each projectile and test collision with it and the tile
            for (int i = 0; i < projectiles.Count; i++)
            {
                //Remove the projectile if it collides with the tile
                if (projectiles[i].TestCollision(testedHitBox))
                {
                    //Remove the projectile from the list
                    projectiles.Remove(projectiles[i]);

                    //Play the projectile destroy sound
                    projectileSnds[(int)ProjectileSoundEffects.DESTROY].CreateInstance().Play();
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
        //Desc: Damages the aspid hunter, and decreases their health
        protected override void DamageEnemy()
        {
            //Damage the aspid hunter, and perform all logic related to that
            base.DamageEnemy();

            //Kill the aspid hunter if their health reaches 0
            if (health == 0)
            {
                //Start the aspid hunters death animation
                enemyState = EnemyState.DEATH;

                //Multiply the aspid hunters speed to get a more powerful death effect
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

        /// <summary>
        /// Returns the projectiles of the enemy
        /// </summary>
        public List<Projectile> GetProjectiles()
        {
            //Return the enemy projectiles
            return projectiles;
        }

        //Pre: spriteBatch allows the aspid hunter to be drawn, and transparancy is how transparent to draw the aspid hunter
        //Post: N/A
        //Desc: Draws the aspid hunter to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the aspid hunter to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour, drawDir);

            //Loop through each projectile and draw it to the screen
            for (int i = 0; i < projectiles.Count; i++)
            {
                //Draw each projectile to the screen
                projectiles[i].Draw(spriteBatch);
            }

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
