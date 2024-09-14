//A: Evan Glaizel
//F: Boss.cs
//P: HostileKnight
//C: 2023/01/03
//M: 
//D: The boss of the game

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
    class Boss : Enemy
    {
        //Store the random number generator to randomize the boss's attack
        private Random rng = new Random();

        //Store all of the states of the boss
        private enum EnemyState
        {
            IDLE,
            JUMP_WINDUP,
            JUMP,
            JUMP_ATTACK,
            JUMP_ATTACK_LAND,
            JUMP_ATTACK_REBOUND,
            ATTACK_WINDUP,
            ATTACK,
            ATTACK_REBOUND,
            TURN,
            KNOCKBACK,
            EXPOSE,
            VUNERABLE
        }

        //Store all of the boss attacks
        private enum Attacks
        {
            JUMP,
            JUMP_ATTACK,
            ATTACK
        }

        //Store each of the bosses sound effects
        private enum BossSoundEffects
        {
            JUMP,
            LAND,
            STRIKE,
            SWING,
            DAMAGE,
            STUN,
            FINAL_STRIKE
        }

        //Store the random attack
        private Attacks randomAttack;

        //Store the enemy state of the boss
        private EnemyState enemyState;

        //Store the knockback health triggers of the boss
        private int[] kbPoints = new int[4];

        //Store the knockback and jump speed
        private Vector2 kbSpeed;
        private Vector2 jumpSpeed;

        //Store the jump speed ranomness
        private int speedRandomness;

        //Store the attack cooldown timer
        private Timer attackCdTimer;

        //Store the death timer
        private Timer deathTimer;

        //Store the health the boss had during knockback
        private int kbHealth = -1;

        //Store the transparancy of the boss
        private float transparancy = 1f;

        //Store the other hitboxes of the boss
        private Rectangle attackHitBox;
        private Rectangle[] potentialAttackHitboxes = new Rectangle[2];
        private Rectangle vunerableHitBox;

        //Store the sound effects of the boss
        private SoundEffect[] bossSnds;

        /*Pre: gd is the graphics device that allows the gameline to be created, imgs are the images of the boss, particleImg is the image of the particles, animScale is the scale of the animations, 
               startLoc is the starting location of the boss, health is the health of the boss, maxSpeed is the max speed of the boss, weight is the weight of the boss, hitboxOffset is the hitbox 
               offset from the animation, sizeOffset is the hitbox size offset from the image, enemySnds are the sounds of the enemy, and bossSnds are the sound effects of the boss, and particleSnds
               are the sound effects of the particles */
        //Post: N/A
        //Desc: Constructs the boss
        public Boss(GraphicsDevice gd, Texture2D[] imgs, Texture2D particleImg, float animScale, Vector2 startLoc, int health, int maxSpeed, double weight, Vector2 hitboxOffset, Vector2 sizeOffset, SoundEffect[] enemySnds, SoundEffect[] bossSnds, SoundEffect[] particleSnds) : base(gd, imgs, particleImg, animScale, startLoc, health, maxSpeed, weight, hitboxOffset, sizeOffset, enemySnds, particleSnds)
        {
            //Set the sound effects of the boss
            this.bossSnds = bossSnds;

            //Set the knockback points
            kbPoints[0] = (health * 3 / 4);
            kbPoints[1] = health / 2;
            kbPoints[2] = health / 4;
            kbPoints[3] = 1;

            //Set the knockback and jump speed
            kbSpeed = new Vector2(5, -15);
            jumpSpeed = new Vector2(6, -20);

            //Set the speed randomness
            speedRandomness = 4;

            //Set the attack cooldown timer
            attackCdTimer = new Timer(750, true);

            //Set the death timer
            deathTimer = new Timer(3000, false);

            //Set the attack and vunerable hitboxes
            potentialAttackHitboxes[0] = new Rectangle(0, 0, 100, 100);
            potentialAttackHitboxes[1] = new Rectangle(0, 0, 300, hitBox.Height);
            vunerableHitBox = new Rectangle(0, 0, 50, 50);
            attackHitBox = potentialAttackHitboxes[0];
        }

        //Pre: startLoc is the starting location of the animation
        //Post: N/A
        //Desc: Sets up the enemy animations
        protected override void SetupAnims(Vector2 startLoc)
        {
            //Setup the boss animations
            anims = new Animation[imgs.Length];
            anims[(int)EnemyState.IDLE] = new Animation(imgs[(int)EnemyState.IDLE], 2, 3, 5, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 9, startLoc, animScale, true);
            anims[(int)EnemyState.JUMP_WINDUP] = new Animation(imgs[(int)EnemyState.JUMP_WINDUP], 1, 3, 3, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, false);
            anims[(int)EnemyState.JUMP] = new Animation(imgs[(int)EnemyState.JUMP], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 8, startLoc, animScale, true);
            anims[(int)EnemyState.JUMP_ATTACK] = new Animation(imgs[(int)EnemyState.JUMP_ATTACK], 2, 3, 5, 0, 5, Animation.ANIMATE_ONCE, 12, startLoc, animScale, true);
            anims[(int)EnemyState.JUMP_ATTACK_LAND] = new Animation(imgs[(int)EnemyState.JUMP_ATTACK_LAND], 2, 1, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 6, startLoc, animScale, false);
            anims[(int)EnemyState.JUMP_ATTACK_REBOUND] = new Animation(imgs[(int)EnemyState.JUMP_ATTACK_REBOUND], 4, 1, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 5, startLoc, animScale, false);
            anims[(int)EnemyState.ATTACK_WINDUP] = new Animation(imgs[(int)EnemyState.ATTACK_WINDUP], 2, 3, 6, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 6, startLoc, animScale, false);
            anims[(int)EnemyState.ATTACK] = new Animation(imgs[(int)EnemyState.ATTACK], 3, 1, 3, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 6, startLoc, animScale, false);
            anims[(int)EnemyState.ATTACK_REBOUND] = new Animation(imgs[(int)EnemyState.ATTACK_REBOUND], 5, 1, 5, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 5, startLoc, animScale, false);
            anims[(int)EnemyState.TURN] = new Animation(imgs[(int)EnemyState.TURN], 1, 2, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, false);
            anims[(int)EnemyState.KNOCKBACK] = new Animation(imgs[(int)EnemyState.KNOCKBACK], 2, 3, 5, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 7, startLoc, animScale, true);
            anims[(int)EnemyState.EXPOSE] = new Animation(imgs[(int)EnemyState.EXPOSE], 2, 5, 10, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 7, startLoc, animScale, false);
            anims[(int)EnemyState.VUNERABLE] = new Animation(imgs[(int)EnemyState.VUNERABLE], 1, 1, 1, 0, 1, 0, 7, startLoc, animScale, false);
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Updates the logic of the boss
        public override void Update(GameTime gameTime, Rectangle playerRect)
        {
            //Update the current animation
            anims[(int)enemyState].Update(gameTime);

            //Apply gravity to the boss if it's not exposed
            if (enemyState != EnemyState.VUNERABLE && enemyState != EnemyState.EXPOSE)
            {
                //Apply gravity to the boss
                ApplyGravity(maxSpeed * 8);
            }

            //Update the game logic based on the enemy state
            switch (enemyState)
            {
                case EnemyState.IDLE:
                    //Update the idle state
                    UpdateIdle(gameTime, playerRect);
                    break;
                case EnemyState.JUMP_WINDUP:
                    //Update the jump windup 
                    UpdateJumpWindup();
                    break;
                case EnemyState.JUMP_ATTACK_LAND:
                    //Update the land portion of the jump attack
                    UpdateJumpAttackLand();
                    break;
                case EnemyState.JUMP_ATTACK_REBOUND:
                case EnemyState.ATTACK_REBOUND:
                    //Update the rebound from attacks
                    UpdateRebound();
                    break;
                case EnemyState.ATTACK_WINDUP:
                    //Update the attack windup
                    UpdateAttackWindup();
                    break;
                case EnemyState.ATTACK:
                    //Update the attack state
                    UpdateAttack();
                    break;
                case EnemyState.TURN:
                    //Update the turn state
                    UpdateTurn(gameTime);
                    break;
                case EnemyState.EXPOSE:
                    //Update the expose state once the boss has been downed
                    UpdateExpose();
                    break;
                case EnemyState.VUNERABLE:
                    //Update the state when the boss is vunerable
                    UpdateVunerable(gameTime);
                    break;
            }

            //Perform standard logic for all enemies
            base.Update(gameTime, playerRect);
        }

        //Pre: gameTime allows the timers to update, and playerRect is the hitbox of the player
        //Post: N/A
        //Desc: Update the jump 
        private void UpdateIdle(GameTime gameTime, Rectangle playerRect)
        {
            //Update the attack cooldown timer
            attackCdTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Turn the boss if the boss isn't facing the player
            if ((hitBox.Center.X < playerRect.Center.X && drawDir == Animation.FLIP_HORIZONTAL) || (hitBox.Center.X > playerRect.Center.X && drawDir == Animation.FLIP_NONE))
            {
                //Turn the boss around
                enemyState = EnemyState.TURN;
                anims[(int)enemyState].isAnimating = true;

            }

            //Perform an attack once the attack timer is up
            if (attackCdTimer.IsFinished())
            {
                //Randomize the attack unless the player is too far away from the boss
                if (Math.Abs(hitBox.Center.X - playerRect.Center.X) > 300)
                {
                    //Make the jump the next attack
                    randomAttack = Attacks.JUMP;
                }
                else
                {
                    //Randomize the next attack
                    randomAttack = (Attacks)rng.Next(0, Enum.GetNames(typeof(Attacks)).Length);
                }

                //Start the attack based on the result of the ranomizer
                switch (randomAttack)
                {
                    case Attacks.JUMP:
                    case Attacks.JUMP_ATTACK:
                        //Start the jump
                        enemyState = EnemyState.JUMP_WINDUP;
                        anims[(int)enemyState].isAnimating = true;
                        break;
                    case Attacks.ATTACK:
                        //Start the attack windup
                        enemyState = EnemyState.ATTACK_WINDUP;
                        anims[(int)enemyState].isAnimating = true;
                        break;
                }

                //Reset the cooldown timer
                attackCdTimer.ResetTimer(true);
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the jump 
        private void UpdateJumpWindup()
        {
            //Send the boss to its jump state once the jump windup animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Start a different jump based on the jump type
                if (randomAttack == Attacks.JUMP)
                {
                    //The boss starts its jump
                    enemyState = EnemyState.JUMP;

                    //Jump in a different horizontal direction based on the boss's direction
                    if (drawDir == Animation.FLIP_NONE)
                    {
                        //Jump the boss to the right
                        speed.X += rng.Next((int)jumpSpeed.X - speedRandomness, (int)jumpSpeed.X + speedRandomness);
                    }
                    else
                    {
                        //Jump the boss to the left
                        speed.X -= rng.Next((int)jumpSpeed.X - speedRandomness, (int)jumpSpeed.X + speedRandomness);
                    }
                }
                else
                {
                    //Start the jump attack without moving horizontally
                    enemyState = EnemyState.JUMP_ATTACK;
                    anims[(int)enemyState].isAnimating = true;
                }
                
                //Move the boss in the air regardless of jump type
                speed.Y = jumpSpeed.Y;

                //Move the boss off of the ground, so it doesn't get clipped by it (Needs to happen because of everyone bouncing up and down on the floor glitch)
                hitBox.Y -= 5;

                //Play the jump sound
                bossSnds[(int)BossSoundEffects.JUMP].CreateInstance().Play();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the land portion of the jump attack
        private void UpdateJumpAttackLand()
        {
            //Change the hitbox location based on the direction of attack
            if (drawDir == Animation.FLIP_NONE)
            {
                //Move the attack hitbox based on the attack frame
                if (anims[(int)enemyState].curFrame == 0)
                {
                    //Set the attack hitbox
                    attackHitBox = potentialAttackHitboxes[0];

                    //The first attack frame is ontop of the boss
                    attackHitBox.X = hitBox.Center.X - 85;
                    attackHitBox.Y = hitBox.Y - attackHitBox.Height - 90;
                }
                else
                {
                    //Set the attack hitbox
                    attackHitBox = potentialAttackHitboxes[1];

                    //The final frame is to the right of the boss
                    attackHitBox.X = hitBox.Right - attackHitBox.Width + 170;
                    attackHitBox.Y = hitBox.Y;
                }
            }
            else
            {
                //Move the attack hitbox based on the attack frame
                if (anims[(int)enemyState].curFrame == 0)
                {
                    //Set the attack hitbox
                    attackHitBox = potentialAttackHitboxes[0];

                    //The first attack frame is ontop of the boss
                    attackHitBox.X = hitBox.Center.X + 65;
                    attackHitBox.Y = hitBox.Y - attackHitBox.Height - 90;
                }
                else
                {
                    //Set the attack hitbox
                    attackHitBox = potentialAttackHitboxes[1];

                    //The final frame is to the left of the boss
                    attackHitBox.X = hitBox.X - 170;
                    attackHitBox.Y = hitBox.Y;
                }
            }

            //Send the boss to its jump attack rebound state once the landing jump attack animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Send the boss to its rebound state
                enemyState = EnemyState.JUMP_ATTACK_REBOUND;
                anims[(int)enemyState].isAnimating = true;

                //Play the attack strike sound
                bossSnds[(int)BossSoundEffects.STRIKE].CreateInstance().Play();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the rebound from attacks
        private void UpdateRebound()
        {
            //Send the boss to its idle state once the rebound animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Send the boss to the idle state
                enemyState = EnemyState.IDLE;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the attack windup
        private void UpdateAttackWindup()
        {
            //Send the boss to its attack state once the attack windup animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Start the boss's attack
                enemyState = EnemyState.ATTACK;
                anims[(int)enemyState].isAnimating = true;

                //Play the attack swing sound
                bossSnds[(int)BossSoundEffects.SWING].CreateInstance().Play();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the attack state
        private void UpdateAttack()
        {
            //Change the hitbox location based on the direction of attack
            if (drawDir == Animation.FLIP_NONE)
            {
                //Move the attack hitbox based on the attack frame
                if (anims[(int)enemyState].curFrame == 0)
                {
                    //Set the attack hitbox
                    attackHitBox = potentialAttackHitboxes[0];

                    //The first attack frame is ontop of the boss
                    attackHitBox.X = hitBox.Center.X - 35;
                    attackHitBox.Y = hitBox.Y - attackHitBox.Height - 75;
                }
                else if (anims[(int)enemyState].curFrame == 1)
                {
                    //The first attack frame is diagonal from the boss
                    attackHitBox.X = hitBox.Right + 40;
                    attackHitBox.Y = hitBox.Center.Y - (attackHitBox.Height);
                }
                else if (anims[(int)enemyState].curFrame == 2)
                {
                    //Set the attack hitbox
                     attackHitBox = potentialAttackHitboxes[1];

                    //The final frame is to the right of the boss
                    attackHitBox.X = hitBox.Right - attackHitBox.Width + 140;
                    attackHitBox.Y = hitBox.Bottom - attackHitBox.Height;
                }
            }
            else
            {
                //Move the attack hitbox based on the attack frame
                if (anims[(int)enemyState].curFrame == 0)
                {
                    //Set the attack hitbox
                    attackHitBox = potentialAttackHitboxes[0];

                    //The first attack frame is ontop of the boss
                    attackHitBox.X = hitBox.Center.X + 65;
                    attackHitBox.Y = hitBox.Y - attackHitBox.Height - 75;
                }
                else if (anims[(int)enemyState].curFrame == 1)
                {
                    //The first attack frame is diagonal from the boss
                    attackHitBox.X = hitBox.X - 190;
                    attackHitBox.Y = hitBox.Center.Y - (attackHitBox.Height);
                }
                else if (anims[(int)enemyState].curFrame == 2)
                {
                    //Set the attack hitbox
                    attackHitBox = potentialAttackHitboxes[1];

                    //The final frame is to the left of the boss
                    attackHitBox.X = hitBox.X - 160;
                    attackHitBox.Y = hitBox.Y;
                }
            }

            //Send the boss to the attack rebound state once the attack animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Send the boss to the rebound state
                enemyState = EnemyState.ATTACK_REBOUND;
                anims[(int)enemyState].isAnimating = true;

                //Play the attack sound
                bossSnds[(int)BossSoundEffects.STRIKE].CreateInstance().Play();
            }
        }

        //Pre: gameTime allows the timer to update
        //Post: N/A
        //Desc: Update the turn state
        private void UpdateTurn(GameTime gameTime)
        {
            //Continue updating the attack cooldown timer even while turning
            attackCdTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Send the boss back to idling once the turn animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Send the boss to the idle state
                enemyState = EnemyState.IDLE;

                //Switch the direction of the boss
                SwitchDir();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the expose state once the boss has been downed
        private void UpdateExpose()
        {
            //Send the boss to its vunerable state once the expose animation is finished
            if (!anims[(int)enemyState].isAnimating)
            {
                //Send the boss to its vunerable state where it can be damaged
                enemyState = EnemyState.VUNERABLE;

                //Set the vunerable hitbox's y position
                vunerableHitBox.Y = hitBox.Bottom - vunerableHitBox.Height - 15;

                //Set the vunerable hitbox's x position based on direction
                if (drawDir == Animation.FLIP_NONE)
                {
                    //Set the vunerable hitbox's x position 
                    vunerableHitBox.X = hitBox.Right + 20;
                }
                else
                {
                    //Set the vunerable hitbox's x position 
                    vunerableHitBox.X = hitBox.X - vunerableHitBox.Width - 20;
                }
            }
        }

        //Pre: gameTime allows the timers to update
        //Post: N/A
        //Desc: Update the vunerable state once the boss has been downed
        private void UpdateVunerable(GameTime gameTime)
        {
            //Update the death timer
            deathTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Only try to bring the boss back to idling if it isn't the last knockback point
            if (kbHealth != kbPoints[kbPoints.Length - 1])
            {
                //Bring the boss back to idling if the player has done enough damage to the downed state
                if (health == kbHealth)
                {
                    //Bring the boss back to idling
                    enemyState = EnemyState.IDLE;
                }
            }

            //If the boss is dead constantly explode particles from it, and if it's finished make it invisible
            if (deathTimer.IsActive())
            {
                //Explode a few particle in a random direction
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 5000, 8, rng.Next(0, 361) * Math.PI / 180, 0, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 5000, 8, rng.Next(0, 361) * Math.PI / 180, 0, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 5000, 8, rng.Next(0, 361) * Math.PI / 180, 0, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
            }
            else if (deathTimer.IsFinished())
            {
                //Slowly make the boss phase into nonexistance
                transparancy -= 0.01f;
            }
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and killEnemy tracks if the collision should kill the enemy
        //Post: N/A
        //Desc: Does collision detection between the boss and tiles
        public override void TestCollision(Rectangle testedHitBox, bool killEnemy)
        {
            //Only check the body part that collided with the boss if the main hitbox collides
            if (Util.Intersects(hitBox, testedHitBox))
            {
                //Do different things based on the part of the boss that connected with it
                if (Util.Intersects(hitBoxes[(int)BodyPart.LEGS], testedHitBox))
                {
                    //Change the state of the boss once they touch the ground based on their current state
                    if (enemyState == EnemyState.JUMP)
                    {
                        //Bring the boss to an idle (the jump is done)
                        enemyState = EnemyState.IDLE;

                        //Play the landing sound
                        bossSnds[(int)BossSoundEffects.LAND].CreateInstance().Play();
                    }
                    else if (enemyState == EnemyState.JUMP_ATTACK)
                    {
                        //Start the damaging portion of the boss's jump attack
                        enemyState = EnemyState.JUMP_ATTACK_LAND;
                        anims[(int)enemyState].isAnimating = true;

                        //Play the attack swing sound
                        bossSnds[(int)BossSoundEffects.SWING].CreateInstance().Play();
                    }
                    else if (enemyState == EnemyState.KNOCKBACK)
                    {
                        //Start exposing the boss
                        enemyState = EnemyState.EXPOSE;
                        anims[(int)enemyState].isAnimating = true;
                    }

                    //Set the boss just ontop of the intersected rectangle
                    hitBox.Y = testedHitBox.Y - hitBox.Height;
                    speed.Y = 0f;
                    speed.X = 0f;
                }
                if (Util.Intersects(hitBoxes[(int)BodyPart.HEAD], testedHitBox))
                {
                    //Set the boss just below the intersected rectangle
                    hitBox.Y = testedHitBox.Y + testedHitBox.Height;
                    speed.Y = 0f;
                }
                else if (Util.Intersects(hitBoxes[(int)BodyPart.LEFT], testedHitBox))
                {
                    //Set the boss just to the right of the intersected rectangle
                    hitBox.X = testedHitBox.X + testedHitBox.Width;
                    speed.X = 0f;
                }
                else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox))
                {
                    //Set the boss just to the left pf the intersected rectangle
                    hitBox.X = testedHitBox.X - hitBox.Width;
                    speed.X = 0f;
                }
            }
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and playerHitBox is the hitbox of the player that used the attack
        //Post: Return the enemies hitbox if there was a collision
        //Desc: Does collision detection for the attacks (Doesn't let them fall through floors or walls)
        public override void TestAttackCollision(Rectangle testedHitBox, Rectangle playerHitBox)
        {
            //Only test collision if the attack cooldown is up, the boss getting into its vunerable animation, and the boss is alive
            if (!hitCooldown.IsActive() && enemyState != EnemyState.KNOCKBACK && enemyState != EnemyState.EXPOSE && health > 0)
            {
                //If the correct hitbox intersects with the attack hitbox, then damage the boss
                if ((enemyState == EnemyState.VUNERABLE && Util.Intersects(vunerableHitBox, testedHitBox)) || (enemyState != EnemyState.VUNERABLE && Util.Intersects(hitBox, testedHitBox)))
                {
                    //Damage the enemy (Boss is not able to be moved by the players attacks)
                    DamageEnemy();
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Damages the mob, and decreases their health
        protected override void DamageEnemy()
        {
            //Decrease the mobs health
            health--;

            //Start the hit cooldown timer
            hitCooldown.ResetTimer(true);

            //Make the enemy orange for improved visual clarity
            enemyColour = orange;

            //Test for knockback if the boss isn't already knockbacked
            if (enemyState != EnemyState.VUNERABLE)
            {
                //Loop through each knockback point, and bring the boss to a knockback if they are at that health point
                for (int i = 0; i < kbPoints.Length; i++)
                {
                    //Knockback the boss if their health reaches the checkpoint
                    if (health == kbPoints[i])
                    {
                        //Knockback the boss
                        enemyState = EnemyState.KNOCKBACK;

                        //Set the health the boss had when its knockbacked
                        kbHealth = kbPoints[i];

                        //Increase the enemies health
                        health += 5;

                        //Send the boss flying upwards
                        speed.Y = kbSpeed.Y;

                        //Get knockbacked in a different horizontal direction based on the boss's direction
                        if (drawDir == Animation.FLIP_NONE)
                        {
                            //Jump the boss to the right
                            speed.X -= kbSpeed.X;
                        }
                        else
                        {
                            //Jump the boss to the left
                            speed.X += kbSpeed.X;
                        }

                        //Play the knockback sound to signify the boss has been knocked back
                        bossSnds[(int)BossSoundEffects.STUN].CreateInstance().Play();

                        //Break out of the loop
                        break;
                    }
                }

                //Create particles for better visual effect
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), 0, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI / 6, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI / 3, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI / 2, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 2 / 3, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 3 / 4, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 5 / 6, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 7 / 6, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 5 / 4, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 4 / 3, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 3 / 2, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 5 / 3, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 7 / 4, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(hitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 11 / 6, GRAVITY, 0.2f, Color.Orange, 0, Emitter.ParticleType.NORMAL);

                //Play the armour damage sound (Only the armour is being attacked
                bossSnds[(int)BossSoundEffects.DAMAGE].CreateInstance().Play();
            }
            else
            {
                //If the boss is exposed then explode particles from it
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), 0, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI / 6, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI / 3, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI / 2, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 2 / 3, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 3 / 4, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 5 / 6, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 7 / 6, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 5 / 4, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 4 / 3, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 3 / 2, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 5 / 3, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 7 / 4, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);
                emitter.CreateParticle(vunerableHitBox.Center.ToVector2(), 500, rng.Next(2, 7), Math.PI * 11 / 6, GRAVITY, 0.4f, Color.Orange, 0, Emitter.ParticleType.NORMAL);

                //Play the squishy damage sound (The boss is vunerable and can actually be damaged
                enemySnds[(int)SoundEffects.DAMAGE].CreateInstance().Play();
            }

            //If the boss was killed activate the death timer and play the final strike sound
            if (health == 0)
            {
                //Activate the death timer
                deathTimer.Activate();

                //Stop testing for collision against the player
                testForCollision = false;

                //Play the final strike sound to signify the boss has been killed
                bossSnds[(int)BossSoundEffects.FINAL_STRIKE].CreateInstance().Play();
            }
        }

        //Pre: N/A
        //Post: Returns a bool that represents if the mob should be killed
        //Desc: Tells the room if the mob should be deleted
        public override bool KillEnemy()
        {
            //Kill the boss if they're invisible
            return transparancy < 0f;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates all hitboxes and animations to the correct frame
        public override void UpdateGamePos()
        {
            //Update the default positions
            base.UpdateGamePos();

            //Update the animation location
            anims[(int)enemyState].destRec.X = hitBox.X - (int)(hitboxOffset.X / 2);
            anims[(int)enemyState].destRec.Y = hitBox.Y - (int)(hitboxOffset.Y / 2);
        }

        //Pre: N/A
        //Post: Return the attack hitbox of the boss
        //Desc: Returns the attack hitbox of the boss that can damage the player
        public Rectangle GetAttackHitBox()
        {
            //If the player is in the right player state, return the attack hitbox
            if (enemyState == EnemyState.ATTACK || enemyState == EnemyState.JUMP_ATTACK_LAND)
            {
                //Return the attack hitbox
                return attackHitBox;
            }

            //Return an empty rectangle the player can't collide with
            return Rectangle.Empty;
        }

        //Pre: N/A
        //Post: Return the hitbox of the boss
        //Desc: Return the hitbox of the boss
        public override Rectangle GetHitBox()
        {
            //Return the hitbox of the enemy if the enemy should be collided with
            if (testForCollision)
            {
                //Return a different hitbox based on the boss state
                if (enemyState == EnemyState.VUNERABLE)
                {
                    //Return the vunerable hitbox of the boss
                    return vunerableHitBox;
                }

                //Return the hitbox of the boss
                return hitBox;
            }

            //Return nothing
            return Rectangle.Empty;
        }

        //Pre: spriteBatch allows the boss to be drawn, and gameTransparancy is how transparent to draw the boss
        //Post: N/A
        //Desc: Draws the boss to the screen
        public override void Draw(SpriteBatch spriteBatch, float gameTransparancy)
        {
            //Draw the boss to the screen
            anims[(int)enemyState].Draw(spriteBatch, enemyColour * transparancy * gameTransparancy, drawDir);

            //Draw the standard items to the screen
            base.Draw(spriteBatch, transparancy);
        }
    }
}
