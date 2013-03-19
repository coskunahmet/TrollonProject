using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Deneme1
{
    class HUD
    {
        GraphicsDeviceManager graphics;
        SpriteFont font;
        SpriteBatch spriteBatch;
        Timer timer;


        public HUD(GraphicsDeviceManager _graphics, SpriteBatch _spriteBatch ,SpriteFont _font)
        {
            this.graphics = _graphics;
            this.font = _font;
            this.spriteBatch = _spriteBatch;
            this.timer = new Timer();
        }

        // Draws time 
        public void DrawTime(Vector2 position, int minute, int second, Color color)
        {
            this.spriteBatch.DrawString(this.font, minute.ToString() + "/" + second.ToString(), position, color);
        }

        // Draws count / max. count 
        public void DrawMoveCount(Vector2 position, int moveCount, int maxMoveCount, Color color)
        {
            this.spriteBatch.DrawString(this.font, moveCount.ToString()+"/"+maxMoveCount.ToString(), position, color);
        }

        // Draw info text (with effect)
        public void DrawInfo(Vector2 position, String info, Color color, int second)
        {
            this.timer.Interval = second * 1000;
            this.timer.Enabled = true;
            this.timer.Elapsed += ((ElapsedEventHandler)delegate(object sender, ElapsedEventArgs args)
            {
                ElapsedTimeHandler(sender, info, color, position);
            });
            //this.timer.Enabled = false;
        }
        private void ElapsedTimeHandler(object sender, String info, Color color, Vector2 position)
        {
            this.spriteBatch.DrawString(this.font, info, position, color);
        }


    }
}
