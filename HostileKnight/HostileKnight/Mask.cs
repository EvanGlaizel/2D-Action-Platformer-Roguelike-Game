//A: Evan Glaizel
//F: Mask.cs
//P: HostileKnight
//C: 2022/12/12
//M: 2022/12/7
//D: The mask that the player has

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
    class Mask
    {
        //Store the state of the mask
        private enum MaskState
        {
            FULL,
            EMPTY,
            BREAK,
            GAIN
        }

        //Store the current mask state
        private MaskState maskState = MaskState.FULL;


        //Store the masks images and animations
        private Texture2D maskFullImg;
        private Texture2D maskEmptyImg;
        private Animation maskBreakAnim;
        private Animation maskGainAnim;

        //Store the location of the mask
        private Vector2 maskLoc;

        //Pre: maskFullImg is the full health image, maskEmptyImg is the empty health img (no health), maskBreakImgs is a spritesheet plays when the player loses health,
             //maskGainImgs is a spritesheet that plays when the player gains health, and maskLoc is the starting location of the mask
        //Post: N/A
        //Desc: Construct the mask
        public Mask(Texture2D maskFullImg, Texture2D maskEmptyImg, Texture2D maskBreakImgs, Texture2D maskGainImgs, Vector2 maskLoc)
        {
            //Set the mask images and animations
            this.maskFullImg = maskFullImg;
            this.maskEmptyImg = maskEmptyImg;
            this.maskLoc = maskLoc;

            //Set the mask animations
            maskBreakAnim = new Animation(maskBreakImgs, 6, 1, 6, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 5, maskLoc, 1f, false);
            maskGainAnim = new Animation(maskGainImgs, 5, 1, 5, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 5, maskLoc, 1f, false);
        }

        //Pre: gameTime tracks the time in the game
        //Post: N/A
        //Desc: Updates the animations of the mask
        public void Update(GameTime gameTime)
        {
            //Change the updated animation based on the current animation
            switch (maskState)
            {
                case MaskState.BREAK:
                    //Update the break animation
                    maskBreakAnim.Update(gameTime);

                    //finishing losing the mask if the break animation is finished
                    if (!maskBreakAnim.isAnimating)
                    {
                        //Finish losing the players mask
                        maskState = MaskState.EMPTY;
                    }
                    break;
                case MaskState.GAIN:
                    //Update the gain animation
                    maskGainAnim.Update(gameTime);

                    //finishing gaining the mask if the gain animation is finished
                    if (!maskGainAnim.isAnimating)
                    {
                        //Give the player another mask
                        maskState = MaskState.FULL;
                    }
                    break;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Remove a players mask
        public void BreakMask()
        {
            //Start the mask break animation
            maskState = MaskState.BREAK;
            maskBreakAnim.isAnimating = true;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Gain a new mask
        public void GainMask()
        {
            //Start the mask break animation
            maskState = MaskState.GAIN;
            maskGainAnim.isAnimating = true;
        }

        //Pre: spriteBatch allows the mask to draw itsself to the screen, and transparancy is how transparent to draw the mask
        //Post: N/A
        //Desc: Draw the mask to the screen
        public void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw the mask to the screen based on its state
            switch (maskState)
            {
                case MaskState.FULL:
                    //Draw the full mask image
                    spriteBatch.Draw(maskFullImg, maskLoc, Color.White * transparancy);
                    break;
                case MaskState.EMPTY:
                    //Draw the empty mask image
                    spriteBatch.Draw(maskEmptyImg, maskLoc, Color.White * transparancy);
                    break;
                case MaskState.BREAK:
                    //Draw the break animation
                    maskBreakAnim.Draw(spriteBatch, Color.White * transparancy, Animation.FLIP_NONE);
                    break;
                case MaskState.GAIN:
                    //Draw the gain animation
                    maskGainAnim.Draw(spriteBatch, Color.White * transparancy, Animation.FLIP_NONE);
                    break;
            }
        }
    }
}
