#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace PongGame
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;

        float pauseAlpha;

        //Player 1
        Player player1;

        //Player 2
        Player player2;

        //Pong ball
        Ball ball;

        //Keyboard input
        KeyboardState currentKeyboardState;

        //Ball speed in X direction
        private int ballSpeedX;
        //Ball speed in Y direction
        private int ballSpeedY;

        //Starting speed of ball
        const int startingSpeed = 7;

        //The speed at which the player will move
        const int playerSpeed = 9;

        //Number of points to win
        const int playerScoreWin = 7;

        //State of the balls during the game
        public enum BallState
        {
            HitTopPlayer1 = 1,
            HitMidPlayer1,
            HitBottomPlayer1,
            HitTopPlayer2,
            HitMidPlayer2,
            HitBottomPlayer2,
            HitTopBorderRight,
            HitTopBorderLeft,
            HitBottomBorderRight,
            HitBottomBorderLeft,
            Disabled
        }

        //Holds the current state of the ball
        BallState currentState;
        //Holds the previous state of the ball
        BallState previousState;

        //Holds the direction to which direction the ball is moving
        bool ballGoingForward;
        bool ballGoingBackward;

        //Has the user pressed enter to start the game?
        bool startGame;

        //Holds the number of times the ball has collided with the players
        int collisionCount;

        //Font to draw score and text
        SpriteFont font;

        //Bottom and top borders for the screen
        Texture2D borderTexture;

        //Sound effects
        SoundEffect bounceSound;
        SoundEffect scoreSound;
        Song gameplayMusic;

        Vector2 player1Score;
        Vector2 player2Score;

        //Hitboxes for players and ball
        //Hitboxes for player1
        Rectangle player1Top;
        Rectangle player1Mid;
        Rectangle player1Bottom;

        //Hitboxes for player2
        Rectangle player2Top;
        Rectangle player2Mid;
        Rectangle player2Bottom;

        //Hitboxes for top and bottom borders
        Rectangle bottomBorder;
        Rectangle topBorder;

        //Collision box for pong ball
        Rectangle ballRectangle;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            player1 = new Player();
            player2 = new Player();
            ball = new Ball();

            currentState = BallState.Disabled;
            previousState = 0;

            startGame = false;

            ballGoingForward = false;
            ballGoingBackward = false;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            //***ALL THE TEXTURES ARE PLACED HERE***//
            Texture2D playerTexture = content.Load<Texture2D>("PongPlayer");
            Texture2D ballTexture = content.Load<Texture2D>("PongBall");
            borderTexture = content.Load<Texture2D>("pongborder");
            font = content.Load<SpriteFont>("pongFont");

            //***INITIALIZE STARTING POSITIONS OF TEXTURES***//
            Vector2 player1Position = new Vector2(playerTexture.Width, 
                    ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.Y 
                    + ScreenManager.GraphicsDevice.Viewport.Height * 3 / 7);

            Vector2 player2Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width
                    - playerTexture.Width * 2, ScreenManager.GraphicsDevice.Viewport.Height * 3 / 7);

            Vector2 ballPosition = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 + 8,
                    ScreenManager.GraphicsDevice.Viewport.Height / 2);

            //***INITIALIZE ALL GAME RESOURCES HERE***//
            player1.Initialize(playerTexture, player1Position);
            player2.Initialize(playerTexture, player2Position);
            ball.Initialize(ballTexture, ballPosition);

            //***ALL SOUNDS ARE LOADED HERE***//
            bounceSound = content.Load<SoundEffect>("sound/plink");
            scoreSound = content.Load<SoundEffect>("sound/score");
            gameplayMusic = content.Load<Song>("sound/Wonderwall");

            //PlayMusic(gameplayMusic);

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            //Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                //Get the current state of keyboard input
                currentKeyboardState = Keyboard.GetState();

                CreateCollisionBoxes();
                UpdateCollision();
                UpdateBall();
            }
        }

        public void UpdatePlayer()
        {
            //Buttons for player2
            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                player2.Position.Y += playerSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                player2.Position.Y -= playerSpeed;
            }

            //Buttons for player1
            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                player1.Position.Y -= playerSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                player1.Position.Y += playerSpeed;
            }

            //User has to press enter to start a game and release the pong ball
            if (currentKeyboardState.IsKeyDown(Keys.Enter))
            {
                startGame = true;

                //Reset each player's score once the score to
                //win has been reached by one of the players
                if (player1.Score == playerScoreWin || player2.Score == playerScoreWin)
                {
                    player1.Score = 0;
                    player2.Score = 0;
                }
            }

            //Bounds the player so that it can not move off the screen
            player1.Position.Y = MathHelper.Clamp(player1.Position.Y, 0 - player1.Height / 2, 
                ScreenManager.GraphicsDevice.Viewport.Height - player1.Height / 2);

            player2.Position.Y = MathHelper.Clamp(player2.Position.Y, 0 - player1.Height / 2, 
                ScreenManager.GraphicsDevice.Viewport.Height - player2.Height / 2);
        }

        //Updates the position of the ball with changes to the ball state
        public void UpdateBall()
        {
            //Update the state only when the player has
            //pressed enter and started the game
            if (startGame)
            {
                //Ball state machine
                switch (currentState)
                {
                    case BallState.HitTopPlayer1:
                        ball.Position.X += ballSpeedX;
                        ball.Position.Y -= ballSpeedY;
                        break;

                    case BallState.HitMidPlayer1:
                        ball.Position.X += ballSpeedX;
                        break;

                    case BallState.HitBottomPlayer1:
                        ball.Position.X += ballSpeedX;
                        ball.Position.Y += ballSpeedY;
                        break;

                    case BallState.HitTopPlayer2:
                        ball.Position.X -= ballSpeedX;
                        ball.Position.Y -= ballSpeedY;
                        break;

                    case BallState.HitMidPlayer2:
                        ball.Position.X -= ballSpeedX;
                        break;

                    case BallState.HitBottomPlayer2:
                        ball.Position.X -= ballSpeedX;
                        ball.Position.Y += ballSpeedY;
                        break;

                    case BallState.HitTopBorderRight:
                        ball.Position.X += ballSpeedX;
                        ball.Position.Y += ballSpeedY;
                        break;

                    case BallState.HitTopBorderLeft:
                        ball.Position.X -= ballSpeedX;
                        ball.Position.Y += ballSpeedY;
                        break;

                    case BallState.HitBottomBorderRight:
                        ball.Position.X += ballSpeedX;
                        ball.Position.Y -= ballSpeedY;
                        break;

                    case BallState.HitBottomBorderLeft:
                        ball.Position.X -= ballSpeedX;
                        ball.Position.Y -= ballSpeedY;
                        break;

                    case BallState.Disabled:
                        if (previousState == 0)
                        {
                            currentState = BallState.HitMidPlayer1;
                        }

                        else if (previousState == BallState.HitTopPlayer1 || 
                                 previousState == BallState.HitMidPlayer1 ||
                                 previousState == BallState.HitBottomPlayer1 || 
                                 previousState == BallState.HitBottomBorderRight ||
                                previousState == BallState.HitTopBorderRight)
                        {
                            player1.Score++;
                            scoreSound.Play();
                            currentState = BallState.HitMidPlayer1;
                        }

                        else if (previousState == BallState.HitTopPlayer2 || 
                                previousState == BallState.HitMidPlayer2 ||
                                previousState == BallState.HitBottomPlayer2 || 
                                previousState == BallState.HitBottomBorderLeft ||
                                previousState == BallState.HitTopBorderLeft)
                        {
                            player2.Score++;
                            scoreSound.Play();
                            currentState = BallState.HitMidPlayer2;
                        }

                        startGame = false;

                        ball.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, 
                            ScreenManager.GraphicsDevice.Viewport.Height / 2);

                        ballSpeedX = startingSpeed;
                        ballSpeedY = startingSpeed;
                        collisionCount = 1;

                        break;
                }

                previousState = currentState;
            }

            //Determines if the ball is going backward or forward
            if (currentState == BallState.HitMidPlayer1 || 
                currentState == BallState.HitBottomPlayer1 ||
                currentState == BallState.HitTopPlayer1)
            {
                ballGoingForward = true;
                ballGoingBackward = false;
            }

            else if (currentState == BallState.HitMidPlayer2 || 
                     currentState == BallState.HitBottomPlayer2 || 
                     currentState == BallState.HitTopPlayer2)
            {
                ballGoingForward = false;
                ballGoingBackward = true;
            }

            //The ball speed increases every four hits by the players
            if (collisionCount % 5 == 0)
            {
                ballSpeedX++;
                ballSpeedY++;
                collisionCount = 1;
            }
        }

        //Creates collision boxes for players and ball
        public void CreateCollisionBoxes()
        {
            player1Top = new Rectangle(player1.Width,
                (int)player1.Position.Y, player1.Width,
                player1.Height * 3 / 7);

            player2Top = new Rectangle(ScreenManager.GraphicsDevice.Viewport.Width
                - player2.Width * 2, (int)player2.Position.Y,
                player2.Width, player2.Height * 3 / 7);

            player1Mid = new Rectangle(player1.Width, (int)player1.Position.Y
                + player1.Height * 3 / 7, player2.Width,
                player1.Height / 7);

            player2Mid = new Rectangle(ScreenManager.GraphicsDevice.Viewport.Width
                - player2.Width * 2, (int)player2.Position.Y + player2.Height * 3 / 7,
                player2.Width, player2.Height / 7);

            player1Bottom = new Rectangle(player1.Width, (int)player1.Position.Y
                + player1.Height * 4 / 7, player1.Width, player1.Height * 3 / 7);

            player2Bottom = new Rectangle(ScreenManager.GraphicsDevice.Viewport.Width
                - player2.Width * 2, (int)player2.Position.Y + player2.Height * 4 / 7,
                player2.Width, player2.Height * 3 / 7);

            ballRectangle = new Rectangle((int)ball.Position.X,
                (int)ball.Position.Y, ball.Width, ball.Height);

            bottomBorder = new Rectangle(0, ScreenManager.GraphicsDevice.Viewport.Height,
                ScreenManager.GraphicsDevice.Viewport.Width, borderTexture.Height);

            topBorder = new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width,
                borderTexture.Height);
        }

        //Calculate the ball state based on ball collisions
        public void UpdateCollision()
        {
            //Update the current state of the pong ball when the pong
            //ball has collided with the players or borders
            if (ballRectangle.Intersects(player1Mid))
            {
                currentState = BallState.HitMidPlayer1;
                collisionCount++;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(player1Top))
            {
                currentState = BallState.HitTopPlayer1;
                collisionCount++;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(player1Bottom))
            {
                currentState = BallState.HitBottomPlayer1;
                collisionCount++;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(player2Mid))
            {
                currentState = BallState.HitMidPlayer2;
                collisionCount++;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(player2Top))
            {
                currentState = BallState.HitTopPlayer2;
                collisionCount++;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(player2Bottom))
            {
                currentState = BallState.HitBottomPlayer2;
                collisionCount++;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(topBorder) && ballGoingForward)
            {
                currentState = BallState.HitTopBorderRight;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(topBorder) && ballGoingBackward)
            {
                currentState = BallState.HitTopBorderLeft;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(bottomBorder) && ballGoingForward)
            {
                currentState = BallState.HitBottomBorderRight;
                bounceSound.Play();
            }

            else if (ballRectangle.Intersects(bottomBorder) && ballGoingBackward)
            {
                currentState = BallState.HitBottomBorderLeft;
                bounceSound.Play();
            }

            //Reset the ball to disabled state when it has gone out of bounds
            else if (ball.Position.X > ScreenManager.GraphicsDevice.Viewport.Width)
            {
                currentState = BallState.Disabled;
            }

            else if (ball.Position.X < 0)
            {
                currentState = BallState.Disabled;
            }
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                UpdatePlayer();
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(Color.Black);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            //Pong players
            player1.Draw(spriteBatch);
            player2.Draw(spriteBatch);

            //Pong ball
            ball.Draw(spriteBatch);

            //Draw Score board
            spriteBatch.DrawString(font, "" + player1.Score, player1Score, Color.Red);
            spriteBatch.DrawString(font, "" + player2.Score, player2Score, Color.Red);

            //Draw Top and bottom borders
            player1Score = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 3, 0);
            player2Score = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width * 2 / 3 - 15, 0);

            spriteBatch.Draw(borderTexture, Vector2.Zero, null, Color.White, 0f, 
                Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.Draw(borderTexture, new Vector2(0, ScreenManager.GraphicsDevice.Viewport.Height 
                - borderTexture.Height), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            //Displays the winner when the score to win has been reached
            if (player1.Score == playerScoreWin)
            {
                spriteBatch.DrawString(font, "Player1 Wins!", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 5, 
                                    (ScreenManager.GraphicsDevice.Viewport.Height / 2) + font.LineSpacing), Color.Orchid);
            }

            else if (player2.Score == playerScoreWin)
            {
                spriteBatch.DrawString(font, "Player2 Wins!", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 5, 
                                    (ScreenManager.GraphicsDevice.Viewport.Height / 2) + font.LineSpacing), Color.Orchid);
            }

            //Displays a notice to the user to press enter to start game
            if (!startGame)
            {
                spriteBatch.DrawString(font, "Press Enter", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 4, 
                    ScreenManager.GraphicsDevice.Viewport.Height - font.LineSpacing),Color.Aqua);
            }


            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }


        #endregion
    }
}
