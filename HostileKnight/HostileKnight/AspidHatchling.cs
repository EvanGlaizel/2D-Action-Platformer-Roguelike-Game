//A: Evan Glaizel
//F: AspidHatchling.cs
//P: HostileKnight
//C: 2022/12/21
//M: 2023/01/15
//D: A weak enemy that tracks the player

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
    class AspidHatchling : Enemy
    {
        //Store all of the states of the aspid hatchling
        private enum EnemyState
        {
            FLY,
            TURN,
            DEATH
        }

        //Store the enemy state of the aspid hatchling
        private EnemyState enemyState;

        //Store the death sound of the hatchling
        private SoundEffect deathSnd;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the aspid hatchling, particleImg is the image of the particles, animScale is the scale of the animations, startLoc
               is the starting location of the aspid hatchling, health is the health of the aspid hatchling, maxSpeed is the max speed of the aspid hatchling, weight is the weight of the aspid hatchling, hitboxOffset
               is the hitbox offset from the animation, and sizeOffset is the hitbox size offset from the image, particleImg is the image of the particle, enemySnds are the sounds of the enemy, and deathSnd is the
               sound effect of the aspid hatchling, and particleSnds are the sound effects of the particles*/
        //Post: N/A
        //Desc: Constructs the aspid hatchling
        public AspidHatchling(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect deathSnd, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Set the death sound of the hatchling
            this.deathSnd = deathSnd;

            //Face the aspid hatchling left
            drawDir = Animation.FLIP_NONE;

            //Pathfind towards the player
            pathFinding = true;
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the aspid hatchling animations
            anims = new Animation[3];
            anims[(int)EnemyState.FLY] = new Animation(imgs[(int)EnemyState.FLY], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 10, startLoc, animScale, true);
            anims[(int)EnemyState.TURN] = new Animation(imgs[(int)EnemyState.TURN], 1, 2, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 10, startLoc, animScale, false);
            anims[(int)EnemyState.DEATH] = new Animation(imgs[(int)EnemyState.DEATH], 5, 1, 5, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 10, startLoc, animScale, true);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the aspid hatchling
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the current animation
            anims[(int)enemyState].Update(gameTime);

            //Update the game logic based on the enemy state
            switch (enemyState)
            {
                case EnemyState.FLY:
                    //Update the fly state
                    UpdateFly();
                    break;
                case EnemyState.TURN:
                    //Update the turn state
                    UpdateTurn();
                    break;
            }

            //Normalize the aspid hatchlings speed if they're not dying and apply gravity if they are
            if (enemyState != EnemyState.DEATH)
            {
                //Normalize the aspid hatchlings speed if they are moving
                if (speed.X != 0 && speed.Y != 0)
                {
                    //Normalize the aspid hatchlings speed, so they are not going too fast in either direction
                    speed.Normalize();
                }

                //Increase the aspid hatchlings speed
                speed *= maxSpeed;
            }

            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the fly state
        private void UpdateFly()
        {
            //Move the aspid hatchling along the path
            MoveOnPath();
            speed = desiredSpeed;

            //Turn the aspid hatchling if they should be turned
            if ((drawDir == Animation.FLIP_NONE && speed.X > 0) || (drawDir == Animation.FLIP_HORIZONTAL && speed.X < 0))
            {
                //Turn the aspid hatchling
                enemyState = EnemyState.TURN;
                anims[(int)enemyState].isAnimating = true;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the turn state
        private void UpdateTurn()
        {
            //Move the aspid hatchling along the path
            MoveOnPath();
            speed = desiredSpeed;

            //Send the aspid hatchling back to walking once the agro animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Let the aspid hatchling fly normally
                enemyState = EnemyState.FLY;

                //Switch the direction of the aspid hatchling
                SwitchDir();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the aspid hatchling, and decreases their health
        protected override void DamageEnemy()
        {
            //Decrease the mobs health
            health--;

            //Start the hit cooldown timer
            hitCooldown.ResetTimer(true);

            //Make the enemy orange for improved visual clarity
            enemyColour = orange;

            //Kill the aspid hatchling if their health reaches 0
            if (health == 0)
            {
                //Start the aspid hatchlings air death animation
                enemyState = EnemyState.DEATH;

                //Stop the aspid hatchling in place
                speed *= 0;

                //Stop pathfinding to the player
                pathFinding = false;

                //Dont let the aspid hatchling collide with anything else
                testForCollision = false;

                //Play the death sound
                enemySnds[(int)SoundEffects.DEATH].CreateInstance().Play();
            }

            //Play the damage sound
            enemySnds[(int)SoundEffects.DAMAGE].CreateInstance().Play();
        }

        //Pre: N/A
        //Post: Returns a bool that represents if the aspid hatchling should be killed
        //Desc: Tells the room if the aspid hatchling should be deleted
        public override bool KillEnemy()
        {
            //Returns true if the mob is dead and has no particles
            return (health == 0 && !anims[(int)EnemyState.DEATH].isAnimating);
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

        //Pre: spriteBatch allows the aspid hatchling to be drawn, and transparancy is how transparent to draw the aspid hatchling
        //Post: N/A
        //Desc: Draws the aspid hatchling to the screen
        public override void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the aspid hatchling to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour * transparancy, drawDir);

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
