//A: Evan Glaizel
//F: Player.cs
//P: HostileKnight
//C: 2022/12/1
//M: 2023/01/02
//D: The player that the user controls to traverse the dungeons

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Animation2D;
using Helper;

namespace HostileKnight
{
    class Player
    {
        //Store the keyboard state
        private KeyboardState kb;
        private KeyboardState prevKb;

        //Store constant values for the players movement
        private const float ACCEL = 1.8f;
        private const float FRICTION = ACCEL * 1f;
        private const float TOLERANCE = FRICTION * 0.9f;
        private const float GRAVITY = 38f / 60;
        private const float ACCEL_AIR = 4f;
        private const float FRICTION_AIR = ACCEL_AIR * 1f;
        private const float TOLERANCE_AIR = FRICTION_AIR * 0.7f;
        private const float JUMP_SPEED = 19;

        //Store the speed the player gets launched at when taking damage
        private const float COLLISION_SPEED_X = 5f;
        private const float COLLISION_SPEED_Y = 15f;

        //Store all possible player states
        private enum PlayerState
        {
            IDLE,
            RUN,
            RISE,
            HOVER,
            FALL,
            HEAL,
            HURT
        }

        //Store the current player state
        private PlayerState playerState;

        //Store all possible attack states
        private enum AttackState
        {
            NONE = -1,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        //Store the current and previous attack state
        private AttackState attackState;
        private AttackState prevAttackState;

        //Store all hitbox names
        private enum BodyPart
        {
            HEAD,
            LEFT,
            RIGHT,
            LEGS
        }

        //Store the sound effects of the player
        private enum SoundEffects
        {
            RUN,
            JUMP,
            FALL,
            SOFT_LAND,
            HARD_LAND,
            HURT,
            HEAL_CHARGE,
            HEAL,
            ATTACK,
            ATTACK_SPIKE
        }

        //Store the player (hitboxes, animations etc.)
        private Animation[] anims;
        private Animation[] playerAttackAnims;
        private Rectangle hitBox;
        private Rectangle[] hitBoxes = new Rectangle[4];
        private SpriteEffects drawDir;

        //Store the player effects
        private Animation healAnim;
        private Animation healCompleteAnim;
        private Animation hurtAnim;

        //Store the UI images
        private Texture2D maskFullImg;
        private Texture2D maskEmptyImg;
        private Texture2D maskBreakImgs;
        private Texture2D maskGainImgs;
        private Texture2D[] soulGuageImgs;

        //Store the soul guage location
        private Vector2 soulGuageLoc;

        //Store the attacks of the player
        private Animation[] attackAnims = new Animation[4];
        private Attack[] attacks = new Attack[4];

        //Store the changable stats of the player
        private int maxHealth;
        private int health;
        private int soul;
        private Vector2 maxSpeed;
        private Vector2 speed;

        //Store the masks
        private Mask[] masks;

        //Store the colour to draw the player and how the colour is changing by
        private Color playerColour;
        private bool colourIncreasing = false;

        //Store the stats multipliers
        private float speedMultiplier;
        private float frictionMultiplier;

        //Store the players grounded state
        private bool grounded;

        //Store if the reason the player is accelerating upwards is due to a jump
        private bool isJump = true;

        //Store if the player is stationary and not affected by anything
        private bool stationary = false;

        //Store if the UI should be draw
        private bool drawUI = true;

        //Store the total amount of times the player has been hit
        private int timesHit = 0;

        //Store the y position the player jumped from
        private float jumpPos;

        //Store if the player has been already knocked back from their attack
        private bool attackKb = true;

        //Store the time it takes for the player to heal
        private Timer healTimer;

        //Store the cooldown time for the players attack
        private Timer attackTimer;

        //Store the timer to track the players knockback
        private Timer kbTimer;
        private bool prevKbTimerActive = false;

        //Store the timer to track how long the player should be hurt for
        private Timer hurtTimer;

        //Store the timer to track how long the player should be invunerable for
        private Timer invunerableTimer;

        //Store the sound effects of the player
        private SoundEffect[] playerSnds;

        //Store an instance of repeating sounds
        private SoundEffectInstance runSnd;
        private SoundEffectInstance fallSnd;
        private SoundEffectInstance healSnd;
        private SoundEffectInstance spikeSnd;

        /*Pre: anims is a list of the players animations, playerAttackAnims is an array of the players animations when they're attacking, attackAnims is an array of the attack animations, 
               healAnim is the animation that goes off when the player heals, healCompleteAnim is the animation that goes off when the player completes an animation, hurtAnim is the animation
               that plays when the player gets hurt, maskFullImg is the full health image, maskEmptyImg is the empty health img (no health), maskBreakImgs is a spritesheet plays when the
               player loses health, maskGainImgs is a spritesheet that plays when the player regains health, soulGuageImgs is the images of the soul guage, maxHealth is the starting health
               is the player, maxSpeed is the max speed the player can travel, maxSpeedAir is the max speed the player can travel in the air, and playerSnds are the sound effects of the player
        */
        //Post: N/A
        //Desc: Sets up the player
        public Player(Animation[] anims, Animation[] playerAttackAnims, Animation[] attackAnims, Animation healAnim, Animation healCompleteAnim, Animation hurtAnim, Texture2D maskFullImg, Texture2D maskEmptyImg, Texture2D maskBreakImgs, Texture2D maskGainImgs, Texture2D[] soulGuageImgs, int maxHealth, Vector2 maxSpeed, SoundEffect[] playerSnds)
        {
            //Store the data from the parameters of the constructor
            this.anims = anims;
            this.playerAttackAnims = playerAttackAnims;
            this.attackAnims = attackAnims;
            this.healAnim = healAnim;
            this.healCompleteAnim = healCompleteAnim;
            this.hurtAnim = hurtAnim;
            this.maskFullImg = maskFullImg;
            this.maskEmptyImg = maskEmptyImg;
            this.maskBreakImgs = maskBreakImgs;
            this.maskGainImgs = maskGainImgs;
            this.soulGuageImgs = soulGuageImgs;
            this.maxHealth = maxHealth;
            this.health = maxHealth;
            this.maxSpeed = maxSpeed;
            this.playerSnds = playerSnds;

            //Setup the constant player values
            SetupPlayer();
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Sets up the player by setting any default values
        private void SetupPlayer()
        {
            //Set the player and attack states
            playerState = PlayerState.IDLE;
            attackState = AttackState.NONE;

            //Sets default values that relate to the player
            soul = 0;
            speed = new Vector2(0, 0);
            grounded = true;

            //Sets the default values for the mulitpliers
            speedMultiplier = 1f;
            frictionMultiplier = 1f;

            //Set up the players hitboxes and draw direction
            hitBox = new Rectangle(anims[(int)PlayerState.IDLE].destRec.X + (anims[(int)PlayerState.IDLE].destRec.Width / 3), 540/*anims[(int)PlayerState.IDLE].destRec.Y*/, anims[(int)PlayerState.IDLE].destRec.Width / 3, anims[(int)PlayerState.IDLE].destRec.Height - 20);
            hitBoxes[(int)BodyPart.HEAD] = new Rectangle(hitBox.X + hitBox.Width / 4, hitBox.Y, hitBox.Width / 2, hitBox.Height / 4);
            hitBoxes[(int)BodyPart.LEFT] = new Rectangle(hitBox.X - 10, hitBoxes[(int)BodyPart.HEAD].Bottom, (hitBox.Width / 2) + 10, hitBox.Height / 2);
            hitBoxes[(int)BodyPart.RIGHT] = new Rectangle(hitBoxes[(int)BodyPart.LEFT].Right, hitBoxes[(int)BodyPart.HEAD].Bottom, (hitBox.Width / 2) + 10, hitBox.Height / 2);
            hitBoxes[(int)BodyPart.LEGS] = new Rectangle(hitBox.X + hitBox.Width / 4, hitBoxes[(int)BodyPart.LEFT].Bottom, hitBox.Width / 2, hitBox.Bottom - hitBoxes[(int)BodyPart.LEFT].Bottom);
            drawDir = Animation.FLIP_NONE;

            //Set the attacks for the player by looping through them
            for (int i = 0; i < attacks.Length; i++)
            {
                //Set each attack
                attacks[i] = new Attack(attackAnims[i], (Attack.Dir)i);
            }

            //Set the location of the attacks
            UpdateAttackLocs();

            //Set the size of the masks
            masks = new Mask[maxHealth];

            //Setup each health
            for (int i = 0; i < masks.Length; i++)
            {
                masks[i] = new Mask(maskFullImg, maskEmptyImg, maskBreakImgs, maskGainImgs, new Vector2(110 + (i * maskFullImg.Width) + (10 * i), 0));
            }

            //Set the soul guage location
            soulGuageLoc = new Vector2(10, 0);

            //Set the timers
            healTimer = new Timer(1000, true);
            attackTimer = new Timer(350, false);
            kbTimer = new Timer(200, false);
            hurtTimer = new Timer(1500, false);
            invunerableTimer = new Timer(1500, false);

            //Set the players draw colour
            playerColour = new Color(255f, 255f, 255f);

            //Don't animate the special effects
            healAnim.isAnimating = false;
            healCompleteAnim.isAnimating = false;
            hurtAnim.isAnimating = false;

            //Set the starting player location
            hitBox.X = 80;
            hitBox.Y = 560;

            //Set the repeatable sound effects
            runSnd = playerSnds[(int)SoundEffects.RUN].CreateInstance();
            runSnd.IsLooped = true;
            fallSnd = playerSnds[(int)SoundEffects.FALL].CreateInstance();
            fallSnd.IsLooped = true;
            healSnd = playerSnds[(int)SoundEffects.HEAL_CHARGE].CreateInstance();
            healSnd.IsLooped = true;
            spikeSnd = playerSnds[(int)SoundEffects.ATTACK_SPIKE].CreateInstance();
        }

        //Pre: GameTime stores the time of the game
        //Desc: Updates all logic relating to the player
        public void Update(GameTime gameTime)
        {
            //Update the players animation
            anims[(int)playerState].Update(gameTime);

            //Update the heal complete effect animation
            healCompleteAnim.Update(gameTime);

            //Set the previous timer state
            prevKbTimerActive = kbTimer.IsActive();

            //Update the knockback timer
            kbTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Update the hurt timer
            hurtTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Update the invunerable timer
            invunerableTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Get the keyboard state
            prevKb = kb;
            kb = Keyboard.GetState();

            //Set the previous attack state
            prevAttackState = attackState;

            //Apply gravity if the player is under a max speed
            if (speed.Y < maxSpeed.Y && !stationary)
            {
                //Apply gravity to the player
                speed.Y += GRAVITY;
            }

            //Change the logic of the player based on their current player state
            switch (playerState)
            {
                case PlayerState.IDLE:
                    //Update the idle state
                    UpdateIdle();
                    break;
                case PlayerState.RUN:
                    //Update the run state
                    UpdateRun();
                    break;
                case PlayerState.RISE:
                    //Update the rise state
                    UpdateRise();
                    break;
                case PlayerState.HOVER:
                    //Update the hover state
                    UpdateHover();
                    break;
                case PlayerState.FALL:
                    //Update the fall state
                    UpdateFall();
                    break;
                case PlayerState.HEAL:
                    //Update the heal state
                    UpdateHeal(gameTime);
                    break;
                case PlayerState.HURT:
                    //Update the hurt state
                    UpdateHurt(gameTime);
                    break;
            }

            //Update all logic relating to the players attack
            UpdateAttack(gameTime);
            
            //Calculate the direction to draw the player
            CalcDrawDir();

            //Update the masks
            UpdateMasks(gameTime);

            //Update the colour of the player
            UpdateColour();

            //Update the players hitboxes
            UpdateHitBoxes();
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the idle state
        private void UpdateIdle()
        {
            //Switch the player state based on the key that is being pressed
            if (kb.IsKeyDown(Keys.Space))
            {
                //Start the players jump
                playerState = PlayerState.RISE;
                speed.Y = -JUMP_SPEED;
                grounded = false;

                //Start the rise animation
                anims[(int)playerState].isAnimating = true;

                //Set the jump height position
                jumpPos = hitBox.Y;

                //Play the jump sound effect
                playerSnds[(int)SoundEffects.JUMP].Play();
            }
            else if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.A))
            {
                //Start the players run
                playerState = PlayerState.RUN;

                //Play the run sound
                runSnd.Play();
            }
            else if (kb.IsKeyDown(Keys.J) && health < maxHealth && soul > 2)
            {
                //Start the players heal
                playerState = PlayerState.HEAL;

                //Reset the players x speed
                speed.X = 0f;

                //Start the heal effect
                healAnim.isAnimating = true;

                //Start the heal sound
                healSnd.Play();
            }

            //If the player has speed while idling (caused by an outside force), apply friction to them
            if (speed.X != 0)
            {
                //Decrease the players speed based on friction
                speed.X += -Math.Sign(speed.X) * FRICTION * frictionMultiplier;

                //Put the player to a complete stop if they are slow enough
                if (Math.Abs(speed.X) <= TOLERANCE)
                {
                    //Stop the player in the x direction
                    speed.X = 0f;
                }
            }

            //If the player is falling while idling, force them into a fall
            if (speed.Y > 3)
            {
                //Make the player fall
                playerState = PlayerState.HOVER;
                anims[(int)playerState].isAnimating = true;
                grounded = false;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the run state
        private void UpdateRun()
        {
            //Control the character based on what's being pressed, and what's happening to the player
            if (kb.IsKeyDown(Keys.Space))
            {
                //Start the players jump
                playerState = PlayerState.RISE;
                speed.Y = -JUMP_SPEED;
                grounded = false;

                //Start the rise animation
                anims[(int)playerState].isAnimating = true;

                //Set the jump height position
                jumpPos = hitBox.Y;

                //Stop the run sound
                runSnd.Stop();

                //Play the jump sound effect
                playerSnds[(int)SoundEffects.JUMP].Play();
            }
            else if (kb.IsKeyDown(Keys.J) && health < maxHealth && soul > 2)
            {
                //Start the players heal and reset their x speed
                playerState = PlayerState.HEAL;
                speed.X = 0f;

                //Start the heal effect
                healAnim.isAnimating = true;

                //Stop the run sound
                runSnd.Stop();

                //Start the heal sound
                healSnd.Play();
            }
            else if (kb.IsKeyDown(Keys.D) && (!kbTimer.IsActive() || kbTimer.IsActive() && attackState == AttackState.LEFT))
            {
                //Increase the players speed
                speed.X += ACCEL;
                speed.X = MathHelper.Clamp(speed.X, -maxSpeed.X * speedMultiplier, maxSpeed.X * speedMultiplier);
            }
            else if (kb.IsKeyDown(Keys.A) && (!kbTimer.IsActive() || kbTimer.IsActive() && attackState == AttackState.RIGHT))
            {
                //Decrease the players speed
                speed.X -= ACCEL;
                speed.X = MathHelper.Clamp(speed.X, -maxSpeed.X * speedMultiplier, maxSpeed.X * speedMultiplier);
            }
            else if (speed.X == 0)
            {
                //Set the user to the idle state if they're not moving
                playerState = PlayerState.IDLE;

                //Stop the run sound
                runSnd.Stop();
            }
            else
            {
                //Decrease the players speed based on friction
                speed.X += -Math.Sign(speed.X) * FRICTION * frictionMultiplier;

                //Put the player to a complete stop if they are slow enough
                if (Math.Abs(speed.X) <= TOLERANCE)
                {
                    //Stop the player in the x direction
                    speed.X = 0f;
                }
            }

            //If the player is falling while running, force them into a fall
            if (speed.Y > 3)
            {
                //Make the player fall
                playerState = PlayerState.HOVER;
                anims[(int)playerState].isAnimating = true;
                grounded = false;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the rise state
        private void UpdateRise()
        {
            //Send the player to the next jump state if they let go of space or reach the peak of their jump, and it's a jump
            if ((kb.IsKeyUp(Keys.Space) || speed.Y > -4 && isJump) || !isJump && speed.Y > -4)
            {
                //Change the players y speed based on the reason they're going to the next stage in the jump
                if (speed.Y > -5)
                {
                    //Change the y speed to get the player to get to the ground faster
                    speed.Y = 3f;
                }
                else
                {
                    //Change the y speed to get the player to get to the ground faster.
                    speed.Y = 0f;
                }

                //Set the next reason the player is in the air to a jump
                isJump = true;

                //Bring the player to the next stage of their jump
                playerState = PlayerState.HOVER;

                //Start the hover animation
                anims[(int)playerState].isAnimating = true;
            }

            //Let the player move horizontally in the air
            HorizontalMoveAir();
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the hover state
        private void UpdateHover()
        {
            //Move to the last phase in the players jump if the animation is finished or if they're on the gorund
            if (!anims[(int)playerState].isAnimating || grounded)
            {
                //Move to the last phase in the players jump
                playerState = PlayerState.FALL;

                //Play the fall sound
                fallSnd.Play();
            }

            //Let the player move horizontally in the air
            HorizontalMoveAir();
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the fall state
        private void UpdateFall()
        {
            //Ground the player if they are on the ground
            if (grounded)
            {
                //Set the player to run
                playerState = PlayerState.RUN;

                //Stop the fall sound
                fallSnd.Stop();

                //Play the run sound
                runSnd.Play();
                
                //Change the landing sound effect that plays based on how far the player fell
                if (hitBox.Y - jumpPos > 140)
                {
                    //Play the hard landing sound
                    playerSnds[(int)SoundEffects.HARD_LAND].CreateInstance().Play();
                }
                else
                {
                    //Play the normal landing sound
                    playerSnds[(int)SoundEffects.SOFT_LAND].CreateInstance().Play();
                }
            }

            //Let the player move horizontally in the air
            HorizontalMoveAir();
        }

        //Pre: gameTime is the time tracked in the game
        //Post: N/A
        //Desc: Updates the heal state
        private void UpdateHeal(GameTime gameTime)
        {
            //Update the heal effect animation
            healAnim.Update(gameTime);

            //Update the heal timer
            healTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Keep healing the player aslong as they don't release the heal button and aren't falling
            if (kb.IsKeyUp(Keys.J) || speed.Y > 3)
            {
                //Stop the players heal
                playerState = PlayerState.IDLE;

                //Reset the heal timer for next the player tries to heal
                healTimer.ResetTimer(true);

                //Stop the heal effect
                healAnim.isAnimating = false;

                //Stop the heal charging sound
                healSnd.Stop();
            }

            //Heal the player if the timer is out
            if (healTimer.IsFinished())
            {
                //Heal the player
                health++;

                //Decrease the players soul
                soul -= 3;

                //Reset the heal timer
                healTimer.ResetTimer(true);

                //Play the heal complete effect and set its location
                healCompleteAnim.isAnimating = true;
                healCompleteAnim.destRec.X = hitBox.Center.X - healCompleteAnim.destRec.Width / 2;
                healCompleteAnim.destRec.Y = hitBox.Center.Y - healCompleteAnim.destRec.Height / 2;

                //Gain the players mask
                masks[health - 1].GainMask();

                //Stop healing if the player is at max health or doesn't have enough soul left
                if (health == maxHealth || soul < 3)
                {
                    //Stop healing the player
                    playerState = PlayerState.IDLE;

                    //Stop the heal charging sound
                    healSnd.Stop();
                }

                //Play the heal complete sound
                playerSnds[(int)SoundEffects.HEAL].CreateInstance().Play();
            }
        }

        //Pre: gameTime tracks the time in the game
        //Post: N/A
        //Desc: Updates the hurt state
        private void UpdateHurt(GameTime gameTime)
        {
            //Update the hurt animation
            hurtAnim.Update(gameTime);

            //Allow the player to move if they're far enough into their hurt state
            if (hurtTimer.GetTimeRemaining() < 1000)
            {
                //Let the player move
                stationary = false;              
            }

            //Bring the player back to normal if their time in the hurt state is up
            if (hurtTimer.GetTimeRemaining() < 700)
            {
                //Bring the player back to a falling state
                playerState = PlayerState.FALL;

                //Play the fall sound
                fallSnd.Play();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Control the players horizontal movement in the air
        private void HorizontalMoveAir()
        {
            //Horizontally accelerate in the air if the player presses a or d
            if (kb.IsKeyDown(Keys.D) && (!kbTimer.IsActive() || kbTimer.IsActive() && attackState == AttackState.LEFT))
            {
                //Increase the players speed
                speed.X += ACCEL_AIR;
                speed.X = MathHelper.Clamp(speed.X, -maxSpeed.X * speedMultiplier, maxSpeed.X * speedMultiplier);
            }
            else if (kb.IsKeyDown(Keys.A) && (!kbTimer.IsActive() || kbTimer.IsActive() && attackState == AttackState.RIGHT))
            {
                //Decrease the players speed
                speed.X -= ACCEL_AIR;
                speed.X = MathHelper.Clamp(speed.X, -maxSpeed.X * speedMultiplier, maxSpeed.X * speedMultiplier);
            }
            else
            {
                //Decrease the players speed in the air
                speed.X += -Math.Sign(speed.X) * FRICTION_AIR;

                //Put the player to a complete stop if they are slow enough
                if (Math.Abs(speed.X) <= TOLERANCE_AIR)
                {
                    //Bring the player to a complete stop
                    speed.X = 0f;
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Calculates the direction to draw the player
        private void CalcDrawDir()
        {
            //Switch the draw direction of the player based on their speed
            if (speed.X > 0)
            {
                //Draw the player facing right
                drawDir = Animation.FLIP_NONE;
            }
            else if (speed.X < 0)
            {
                //Draw the player facing left
                drawDir = Animation.FLIP_HORIZONTAL;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the colour of the player
        private void UpdateColour()
        {
            //Change the colour of the player if they are invunerable
            if (hurtTimer.IsActive() || invunerableTimer.IsActive() || playerColour.R != 255)
            {
                //Increase or decrease the colour based on if the colour should be increasing or decreasing
                if (colourIncreasing)
                {
                    //Increase the players colour
                    playerColour.R += 5;
                    playerColour.G += 5;
                    playerColour.B += 5;

                    //Start decreasing the colour if its reached its max colour (white)
                    if (playerColour.R == 255)
                    {
                        //Start decreasing the colour
                        colourIncreasing = false;
                    }
                }
                else
                {
                    //Decrease the players colour
                    playerColour.R -= 5;
                    playerColour.G -= 5;
                    playerColour.B -= 5;

                    //Start increasing the colour if its reached its max colour (white)
                    if (playerColour.R == 0)
                    {
                        //Start increasing the colour
                        colourIncreasing = true;
                    }
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates all hitboxes and animations to the correct location
        public void UpdateGamePos()
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

            //Update either the player animation location or the attack animation location based on the attack state
            if (attackState == AttackState.NONE)
            {
                //Set the location of the current animation in relation to the main hitbox
                anims[(int)playerState].destRec.X = hitBox.X - hitBox.Width;
                anims[(int)playerState].destRec.Y = hitBox.Y - 20;

                //If the player is healing, set the location of the effect
                if (playerState == PlayerState.HEAL)
                {
                    //Set the location of the heal effect
                    healAnim.destRec.X = hitBox.X - (hitBox.Width / 4);
                    healAnim.destRec.Y = hitBox.Y - 40;
                }
            }
            else
            {
                //Update the location of the player attack animations
                playerAttackAnims[(int)attackState].destRec.X = hitBox.X - hitBox.Width;
                playerAttackAnims[(int)attackState].destRec.Y = hitBox.Y - 20;

                //Update the location of the players attacks
                UpdateAttackLocs();
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the hitbox of the player
        private void UpdateHitBoxes()
        {
            //Set the location of the hitboxes if the player is not stationary
            if (!stationary)
            {
                //Set the location of the main hitbox
                hitBox.X += (int)speed.X;
                hitBox.Y += (int)speed.Y;

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
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Update the location of the attacks
        private void UpdateAttackLocs()
        {
            //Update the location of the attacks
            attacks[(int)AttackState.LEFT].SetLoc(hitBox.X - attacks[(int)AttackState.LEFT].GetHitBox().Width - 20, hitBox.Y - 20);
            attacks[(int)AttackState.RIGHT].SetLoc(hitBox.Right, hitBox.Top - 20);
            attacks[(int)AttackState.UP].SetLoc(hitBox.X - hitBox.Width, hitBox.Y - attacks[(int)AttackState.LEFT].GetHitBox().Height - 20);
            attacks[(int)AttackState.DOWN].SetLoc(hitBox.X - hitBox.Width, hitBox.Bottom - 40);
        }

        //Pre: gameTime tracks the time of the game
        //Post: N/A
        //Desc: Updates all logic relating to the players attack
        private void UpdateAttack(GameTime gameTime)
        {
            //Update the cooldown timer between attacks
            attackTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Change the attack state if the user presses the attack button and the attack cooldown is up
            if (kb.IsKeyDown(Keys.K) && !prevKb.IsKeyDown(Keys.K) && !attackTimer.IsActive() && (!hurtTimer.IsActive() || hurtTimer.IsActive() && hurtTimer.GetTimeRemaining() < 1000) && playerState != PlayerState.HEAL)
            {
                //Start the attack based on the direction of the player
                if (kb.IsKeyDown(Keys.W))
                {
                    //Start the players up attack
                    attackState = AttackState.UP;
                }
                else if (kb.IsKeyDown(Keys.S) && !grounded)
                {
                    //Start the players down attack
                    attackState = AttackState.DOWN;
                }
                else if (drawDir == Animation.FLIP_NONE)
                {
                    //Start the players right attack
                    attackState = AttackState.RIGHT;
                }
                else
                {
                    //Start the players left attack
                    attackState = AttackState.LEFT;
                }

                //Start the correct attack and the attack animation
                playerAttackAnims[(int)attackState].isAnimating = true;
                attacks[(int)attackState].StartAttack();

                //Move the attack hitboxes to the correct spot
                UpdateAttackLocs();

                //Reset the attack timer
                attackTimer.ResetTimer(true);

                //Play the attack sound effect
                playerSnds[(int)SoundEffects.ATTACK].CreateInstance().Play();
            }

            //Update the attacks and the respective animations if the player is attack
            if (attackState != AttackState.NONE)
            {
                //Update the player animation of the current attack
                playerAttackAnims[(int)attackState].Update(gameTime);

                //Update the animation of the current attack
                attacks[(int)attackState].UpdateAttack(gameTime);

                //Stop the attack if the animation is finished
                if (!attacks[(int)attackState].IsAnimating())
                {
                    //Keep the player facing the direction they are attacking
                    if (attackState == AttackState.RIGHT)
                    {
                        //Face the knight right
                        drawDir = Animation.FLIP_NONE;
                    }
                    else if (attackState == AttackState.LEFT)
                    {
                        //Face the knight left
                        drawDir = Animation.FLIP_HORIZONTAL;
                    }

                    //Reset the attack state
                    attackState = AttackState.NONE;

                    //Let the player get knocked back from an attack again
                    attackKb = true;
                }
            }
        }

        //Pre: testedHitBox is the hitbox that is being tested on, testedHitBoxImg is the image of the testedhitBox, doesDamage determines if the collided object does damage to the player, isTile determines if the collided rectangle is a tile,
             //newFrictionMultiplier is the multiplier for friction, newSpeedMultiplier is the multiplier for max speed, and isOneWay determines if the platform can only be collided with one way
        //Pre: Return a bool that represents if the player collided with the tile or enemy
        //Desc: Does collision detection between the knight and its environment
        public bool TestCollision(Rectangle testedHitBox, bool doesDamage, bool isTile, float newFrictionMultiplier, float newSpeedMultiplier, bool isOneWay)
        {
            //Only test collision if the player is not stationary
            if (!stationary)
            {
                //Only check the body part that collided with the enemy if the main hitbox collides
                if (Util.Intersects(hitBox, testedHitBox))
                {
                    //Do different things based on the part of the player that connected with it
                    if (Util.Intersects(hitBoxes[(int)BodyPart.HEAD], testedHitBox) && !isOneWay)
                    {
                        //Set the player just below the intersected rectangle if the rectangle is a tile
                        if (isTile)
                        {
                            //Set the player just below the intersected rectangle
                            hitBox.Y = testedHitBox.Y + testedHitBox.Height;
                            speed.Y = 0f;
                        }

                        //Damage the player if the collision does damamge if they're not invunerable
                        if (doesDamage && !hurtTimer.IsActive() && !invunerableTimer.IsActive())
                        {
                            //Damage the player
                            DamagePlayer(new Vector2(-speed.X, -COLLISION_SPEED_Y));
                        }
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.LEFT], testedHitBox) && !isOneWay)
                    {
                        //Set the player just to the right of the intersected rectangle if the rectangle is a tile
                        if (isTile)
                        {
                            //Set the player just to the right of the intersected rectangle
                            hitBox.X = testedHitBox.X + testedHitBox.Width;
                            speed.X = 0f;
                        }

                        //Damage the player if the collision does damamge if they're not invunerable
                        if (doesDamage && !hurtTimer.IsActive() && !invunerableTimer.IsActive())
                        {
                            //Damage the player
                            DamagePlayer(new Vector2(COLLISION_SPEED_X, -COLLISION_SPEED_Y));
                        }
                        else if (playerState == PlayerState.HURT)
                        {
                            //Stop the players hurt state and let them fall
                            playerState = PlayerState.FALL;

                            //Play the fall sound
                            fallSnd.Play();
                        }
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.RIGHT], testedHitBox) && !isOneWay)
                    {
                        //Set the player just to the left of the intersected rectangle if the rectangle is a tile
                        if (isTile)
                        {
                            //Set the player just to the left of the intersected rectangle
                            hitBox.X = testedHitBox.X - hitBox.Width;
                            speed.X = 0f;
                        }

                        //Damage the player if the collision does damamge if they're not invunerable
                        if (doesDamage && !hurtTimer.IsActive() && !invunerableTimer.IsActive())
                        {
                            //Damage the player
                            DamagePlayer(new Vector2(-COLLISION_SPEED_X, -COLLISION_SPEED_Y));
                        }
                        else if (playerState == PlayerState.HURT)
                        {
                            //Stop the players hurt state and let them fall
                            playerState = PlayerState.FALL;

                            //Play the fall sound
                            fallSnd.Play();
                        }
                    }
                    else if (Util.Intersects(hitBoxes[(int)BodyPart.LEGS], testedHitBox) && (!isOneWay || (isOneWay && speed.Y >= 0)))
                    {
                        //Set the player just above the intersected rectangle if the rectangle is a tile
                        if (isTile)
                        {
                            //Set the player just ontop of the intersected rectangle
                            hitBox.Y = testedHitBox.Y - hitBox.Height;
                            speed.Y = 0f;

                            //Set the new friction and speed multipliers
                            frictionMultiplier = newFrictionMultiplier;
                            speedMultiplier = newSpeedMultiplier;
                        }

                        //Damage the player if the collision does damage if they're not invunerable, and ground them if it doesn't
                        if (doesDamage && !hurtTimer.IsActive() && !invunerableTimer.IsActive())
                        {
                            //Damage the player
                            DamagePlayer(new Vector2(-speed.X, -COLLISION_SPEED_Y));
                        }
                        else if (!grounded && isTile)
                        {
                            //Ground the player
                            grounded = true;

                            //Reset the players state if they are hurt
                            if (playerState == PlayerState.HURT)
                            {
                                //Let the player run
                                playerState = PlayerState.RUN;

                                //Play the run sound
                                runSnd.Play();
                            }
                        }
                    }

                    //There was a collision
                    return true;
                }
            }

            //There wasn't a collision
            return false;
        }

        //Pre: testHitBox is the hitbox to test collision on, pogoable determines if the player can pogo off of the testedHitBox, and isSpike determines if the player hit a tile
        //Post: Return the hitbox that was collided with the enemy
        //Desc: Test collision between the current attack and other game object
        public bool TestAttackCollision(Rectangle testedHitBox, bool pogoable, bool isSpike) 
        {
            //Only test for collision if the player is attacking and not stationary
            if (attackState != AttackState.NONE && !stationary)
            {
                //If the attack and testedHitBox is colliding, tell the program there was a collision, and propel the player depending on the attack direction
                if (Util.Intersects(attacks[(int)attackState].GetHitBox(), testedHitBox))
                {
                    //Let the player attack if they are able to
                    if (attackKb)
                    {
                        //The player will be knocked back by an attack. 
                        attackKb = false;

                        //Propel the player in a different direction depending on the attack direction
                        switch (attackState)
                        {
                            case AttackState.LEFT:
                                //Propel the player to the right
                                speed.X = maxSpeed.X * 2;
                                kbTimer.ResetTimer(true);
                                break;

                            case AttackState.RIGHT:
                                //Propel the player to the left
                                speed.X = -maxSpeed.X * 2;
                                kbTimer.ResetTimer(true);
                                break;

                            case AttackState.UP:
                                //Propel the player downwards if they're not already on the ground
                                if (!grounded)
                                {
                                    //Propel the player down
                                    speed.Y += 0.5f;
                                }
                                break;

                            case AttackState.DOWN:
                                //Propel the player up if the object they collided with is pogoable
                                if (pogoable)
                                {
                                    //Set the player upwards, and put them in the air
                                    speed.Y = -JUMP_SPEED / 2;
                                    isJump = false;
                                    playerState = PlayerState.HOVER;
                                    anims[(int)playerState].isAnimating = true;
                                }
                                else
                                {
                                    //If the collided enemy isn't pogoable allow the player to be knocked back by an attack again (Allows the program to keep testing for collision until the enemy itself collides with the attack)
                                    attackKb = true;
                                }
                                break;
                        }

                        //Tell the program a collision occured
                        return true;
                    }
                    else if (isSpike && spikeSnd.State != SoundState.Playing)
                    {
                        //Play the spike collision sound if its not already playing and the tested tile is a spike
                        spikeSnd.Play();
                    }
                }
            }

            //Return false if the player isn't attacking
            return false;
        }

        //Pre: particleHitbox is the hitbox of the particle
        //Post: Return if the player intersected with the particle
        //Desc: Gives the player soul if they intersect with the hitbox
        public bool TestParticleCollision(Rectangle particleHitBox)
        {
            //Give the player soul if they intersect with the particle
            if (Util.Intersects(hitBox, particleHitBox))
            {
                //Give the player soul, but cap it at 9
                soul++;
                soul = MathHelper.Clamp(soul, 0, 9);

                //The player intersected with the soul particle
                return true;
            }

            //The player didn't intersect with the soul particle
            return false;
        }

        //Pre: launchDir is the direction to launch the player
        //Post: N/A
        //Desc: Damages the player, and does everything related to that
        private void DamagePlayer(Vector2 launchDir)
        {
            //Only damage the player if the player is not hurt
            if (playerState != PlayerState.HURT)
            {
                //Launch the player in their specified direction
                speed = launchDir;

                //Decrease the players health and start their hurt state
                playerState = PlayerState.HURT;
                health--;

                //Keep the player stationary
                stationary = true;

                //Break the players mask
                masks[health].BreakMask();

                //Start the hurt timer
                hurtTimer.ResetTimer(true);

                //Reset the players attack state
                attackState = AttackState.NONE;

                //Start the hurt animation
                hurtAnim.isAnimating = true;
                hurtAnim.destRec.X = hitBox.Center.X - (hurtAnim.destRec.Width / 2);
                hurtAnim.destRec.Y = hitBox.Center.Y - (hurtAnim.destRec.Height / 2);

                //Put the player off of the ground
                grounded = false;

                //Increase the number of times the player has been hurt
                timesHit++;

                //Play the hurt sound effect
                playerSnds[(int)SoundEffects.HURT].CreateInstance().Play();
            }
        }

        //Pre: gameTime tracks the time in the game
        //Post: N/A
        //Desc: Updates the masks on the screen
        private void UpdateMasks(GameTime gameTime)
        {
            //Loop through each mask and update it
            for (int i = 0; i < masks.Length; i++)
            {
                //Update the mask
                masks[i].Update(gameTime);
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Stop all looping sounds
        public void StopSounds()
        {
            //Stop each looping sound
            runSnd.Stop();
            fallSnd.Stop();
            healSnd.Stop();
        }

        //Pre: newLoc is the new location to set the player
        //Post: N/A
        //Desc: Moves the player to a new location on the screen
        public void SetLoc(Vector2 newLoc)
        {
            //Move the player to the new location
            hitBox.X = (int)newLoc.X;
            hitBox.Y = (int)newLoc.Y;

            //Update the location of the attacks
            UpdateAttackLocs();

            //Reset the players speed for the move
            speed *= 0;

            //Make the player fall in case they get teleported into the air
            playerState = PlayerState.FALL;

            //Make the player invunerable if they just got teleported
            invunerableTimer.ResetTimer(true);

            //Play the fall sound
            fallSnd.Play();
        }

        //Pre: drawUI determines if the UI should be drawn
        //Post: N/A
        //Desc: Determines if the UI of the player should be drawn
        public void DrawUI(bool drawUI)
        {
            //Draw or don't draw the UI depending on the parameter input
            this.drawUI = drawUI;
        }

        //Pre: N/A
        //Post: Return the players hitbox
        //Desc: Return the hitbox of the player for collision purposes
        public Rectangle GetHitBox()
        {
            //Return the players hitbox
            return hitBox;
        }

        //Pre: N/A
        //Post: Return the health of the player
        //Desc: Returns the players current health
        public int GetHealth()
        {
            //Return the players heatlh
            return health;
        }

        //Pre: N/A
        //Post: Return the times the player has been hit
        //Desc: Returns the total amount of times the player has been damaged
        public int GetNumTimesHit()
        {
            //Return the number of times the player has been hit
            return timesHit;
        }

        //Pre: N/A
        //Post: Return the hitbox of the current attack
        //Desc: Return the hitbox of the players current attack
        public Rectangle GetCurAttackHitBox()
        {
            //Return an attack if the player is currently attacking
            if (attackState == AttackState.NONE)
            {
                //return nothing
                return Rectangle.Empty;
            }
            else
            {
                //Return the current attack hitbox
                return attacks[(int)attackState].GetHitBox();
            }
        }


        //Pre: spriteBatch is what allows everything to be drawn to the screen, and transparancy is how transparent to draw the player
        //Post: N/A
        //Desc: Draws the player and everything relating to it to the screen
        public void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw either the attack animations, or the player animations based on if the player is attacking
            if (attackState == AttackState.NONE)
            {
                //Draw the current player animation
                anims[(int)playerState].Draw(spriteBatch, playerColour * transparancy, drawDir);
            }
            else
            {
                //Draw the players attack animation
                playerAttackAnims[(int)attackState].Draw(spriteBatch, playerColour * transparancy, Animation.FLIP_NONE);
                
                //Draw the players attack
                attacks[(int)attackState].Draw(spriteBatch, transparancy);
            }

            //Draw the heal effect if the player is healing
            if (playerState == PlayerState.HEAL)
            {
                //Draw the heal effect
                healAnim.Draw(spriteBatch, Color.White * transparancy, Animation.FLIP_NONE);
            }

            //Draw the heal complete effect
            healCompleteAnim.Draw(spriteBatch, Color.White * transparancy, Animation.FLIP_NONE);

            //Draw the hurt effect if the player is not dead
            if (health > 0)
            {
                //Draw the hurt effect
                hurtAnim.Draw(spriteBatch, Color.White * transparancy, Animation.FLIP_NONE);
            }

            //Draw the UI if it should be draw
            if (drawUI && health > 0)
            {
                //Draw the soul guage
                spriteBatch.Draw(soulGuageImgs[soul], soulGuageLoc, Color.White * transparancy);

                //Draw each mask on the screen
                for (int i = 0; i < maxHealth; i++)
                {
                    //Draw the next mask to the screen
                    masks[i].Draw(spriteBatch, transparancy);
                }
            }
        }
    }
}