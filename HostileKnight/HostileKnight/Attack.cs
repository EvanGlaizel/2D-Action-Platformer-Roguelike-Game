//A: Evan Glaizel
//F: Attack.cs
//P: HostileKnight
//C: 2022/12/3
//M: 2022/12/3
//D: The weapon that the player uses to deal damage to enemies

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Animation2D;

namespace HostileKnight
{
    class Attack
    {
        //Store all directions of the attack
        public enum Dir
        {
            LEFT,
            RIGHT,
            UP,
            DOWN
        }


        //Store the animation and hitbox of the attack
        private Animation anim;
        private Rectangle hitBox;

        //Store the hitbox offset of the rectangle
        private Vector2 hitboxOffset = new Vector2();

        //Pre: anim is the attack animation, and attackDir is the direction of the attack
        //Post: N/A
        //Desc: Constructs the attack
        public Attack(Animation anim, Dir attackDir)
        {
            //Sets the attack animations
            this.anim = anim;

            //Change the hitbox based on the direction of the attack
            switch (attackDir)
            {
                case Dir.LEFT:
                    //Sets the hitbox of the attack
                    hitboxOffset.X = 20;
                    hitboxOffset.Y = 25;
                    hitBox = new Rectangle(anim.destRec.X + (int)hitboxOffset.X, anim.destRec.Y + (int)hitboxOffset.Y, anim.destRec.Width - (int)hitboxOffset.X, anim.destRec.Height - 60);
                    break;
                case Dir.RIGHT:
                    //Sets the hitbox of the attack
                    hitboxOffset.Y = 25;
                    hitBox = new Rectangle(anim.destRec.X, anim.destRec.Y + (int)hitboxOffset.Y, anim.destRec.Width - 20, anim.destRec.Height - 60);
                    break;
                case Dir.UP:
                    //Sets the hitbox of the attack
                    hitboxOffset.X = 10;
                    hitboxOffset.Y = 10;
                    hitBox = new Rectangle(anim.destRec.X + (int)hitboxOffset.X, anim.destRec.Y + (int)hitboxOffset.Y, anim.destRec.Width - 20, anim.destRec.Height - 20);
                    break;
                case Dir.DOWN:
                    //Sets the hitbox of the attack
                    hitboxOffset.X = 5;
                    hitboxOffset.Y = 10;
                    hitBox = new Rectangle(anim.destRec.X + (int)hitboxOffset.X, anim.destRec.Y, anim.destRec.Width - 10, anim.destRec.Height - 20);
                    break;
            }
        }

        //Pre: GameTime tracks the time in the game
        //Post: N/A
        //Desc: Updates the animation of the attack
        public void UpdateAttack(GameTime gameTime)
        {
            //Updates the animation of the attack
            anim.Update(gameTime);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Starts the attack
        public void StartAttack()
        {
            //Starts the attack
            anim.isAnimating = true;
        }

        //Pre: N/A
        //Post: Returns a bool that represents if the attack animation is animating
        //Desc: Tell the program if the attack is animating
        public bool IsAnimating()
        {
            return anim.isAnimating;
        }

        //Pre: N/A
        //Post: Returns the hitbox of the animation
        //Desc: Returns the hitbox of the attack for collision purposes
        public Rectangle GetHitBox()
        {
            //Return the hitbox of the attack
            return hitBox;
        }

        //Pre: loc is the location to set the hitbox of the animation to
        //Post: N/A
        //Desc: Sets the hitbox location of the attack
        public void SetLoc(int xLoc, int yLoc)
        {
            //Set the hitbox location
            hitBox.X = xLoc + (int)hitboxOffset.X;
            hitBox.Y = yLoc + (int)hitboxOffset.Y;

            //Set the animation location to match up with the hitbox
            anim.destRec.X = xLoc;
            anim.destRec.Y = yLoc;
        }

        //Pre: Spritebatch is the tool that allows the attack to draw itself, and transparancy is how transparent to draw the attack
        //Post: N/A
        //Desc: draws the attack to the screen
        public void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the attack animation
            anim.Draw(spriteBatch, Color.White * transparancy, Animation.FLIP_NONE);
        }
    }
}
