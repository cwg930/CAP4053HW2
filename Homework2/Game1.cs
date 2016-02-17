﻿#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Xml.Serialization;

#endregion

namespace Homework2
{
	struct NavGoal
	{
		public Vector2 Start { get; set; }
		public Vector2 End { get; set; }
	}
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		const bool autonomousPlayer = true;
		const int maxCycles = 2000;
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont font;

		static List<Player> players;
		static List<Wall> walls;
		static List<Agent> agents;
		static List<NavGoal> testData;
		LinkedList<String> lines;
		int numWalls = 0;
		KeyboardState currentKeyboardState;
		KeyboardState previousKeyboardState;
		MouseState currentMouseState;
		MouseState previousMouseState;
		float playerMoveSpeed;
		float playerTurnSpeed;
		Vector2 moveTarget;
		bool targetReached = true;
		int numCycles;
		List<Genome> population;
		//Debug stuff
		Texture2D debugTex;

		public static List<Wall> Walls { get { return walls; } }
		public static List<Agent> Agents { get { return agents; } }

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";
			graphics.IsFullScreen = false;
			this.IsMouseVisible = true;

		}
	
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			// TODO: Add your initialization logic here
			player = new Player();
			playerMoveSpeed = 8.0f;
			playerTurnSpeed = MathHelper.ToRadians (1.0f); 
			walls = new List<Wall> (numWalls);
			for(int i = 0; i < numWalls; i++)
				walls[i] = new Wall();
			agents = new List<Agent> ();
			StreamReader sr = new StreamReader ("agents.txt");
			lines = new LinkedList<String>();
			while (!sr.EndOfStream) {
				lines.AddLast(sr.ReadLine ());
			}
			for (int i = 0; i < lines.Count; i++) {
				agents.Add (new Player ());
			}
			numCycles = 0;
			base.Initialize ();
				
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
			//TODO: use this.Content to load your game content here 
			Vector2 playerPosition = new Vector2 (GraphicsDevice.Viewport.TitleSafeArea.X + GraphicsDevice.Viewport.TitleSafeArea.Width / 2, 
				GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
	
			player.Initialize (Content.Load<Texture2D> ("Graphics/HW1Player"), playerPosition, 0.0f, playerTurnSpeed, playerMoveSpeed);
			Random r = new Random ();
			foreach (Wall w in walls) {				
				Vector2 wallPosition = new Vector2 (r.Next (0, GraphicsDevice.Viewport.TitleSafeArea.Width), 
					                      r.Next (0, GraphicsDevice.Viewport.TitleSafeArea.Height));
				if (r.Next (1, 10) <= 5)
					w.Initialize (Content.Load<Texture2D>("Graphics/HW1WallHorizontal"), wallPosition);
				else
					w.Initialize (Content.Load<Texture2D>("Graphics/HW1WallVertical"), wallPosition);
			}
			font = Content.Load<SpriteFont> ("Fonts/DebugText");
			//extremely hacky agent adding code, will break easily if agents.txt isn't formatted correctly
			foreach (Player agent in agents) {
				String[] tokens = lines.First.Value.Split (',');
				lines.RemoveFirst ();
				Vector2 agentPosition = new Vector2 (Convert.ToInt32(tokens[0]), Convert.ToInt32(tokens[1]));
				agent.Initialize (Content.Load<Texture2D> ("Graphics/HW1Agent2"), agentPosition, 
					MathHelper.ToRadians ((float)Convert.ToInt32(tokens[2])));
			}

			//debug texture for drawing collison rectangles
			debugTex = new Texture2D(GraphicsDevice, 1, 1);
			debugTex.SetData (new Color[] { Color.White });
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
			#if !__IOS__
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
			    Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
				Exit ();
			}
			#endif
			// TODO: Add your update logic here		
			previousKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState ();
			previousMouseState = currentMouseState;
			currentMouseState = Mouse.GetState ();
			//player.Update (gameTime, currentKeyboardState, walls, GraphicsDevice.Viewport);

			if (currentMouseState.LeftButton == ButtonState.Pressed && 
				previousMouseState.LeftButton == ButtonState.Released) {
				moveTarget = new Vector2 (currentMouseState.X, currentMouseState.Y);
				targetReached = false;
			}
			//stop
			if (currentKeyboardState.IsKeyDown (Keys.X) && previousKeyboardState.IsKeyUp (Keys.X)) {
				targetReached = true;
			}

			if(!targetReached)
				SeekTarget (gameTime, moveTarget);

			player.UpdateSensors ();

			base.Update (gameTime);
		}

		/*	Player movement method
		 *  to be replaced by player.Update()
		 * */

		private void UpdatePlayerManual(GameTime gameTime)
		{
			float velX = (float)(Math.Cos (player.Heading) * playerMoveSpeed);
			float velY = (float)(Math.Sin (player.Heading) * playerMoveSpeed);
			if (currentKeyboardState.IsKeyDown (Keys.W)) {
				player.Position += new Vector2 (velX, velY);
			}
			if (currentKeyboardState.IsKeyDown (Keys.S)) {
				player.Position -= new Vector2 (velX, velY);
			}
			if (currentKeyboardState.IsKeyDown (Keys.A)) {
				player.Heading -= playerTurnSpeed;
			}
			if (currentKeyboardState.IsKeyDown (Keys.D)) {
				player.Heading += playerTurnSpeed;
			}

			// Force heading to wrap around to prevent ambiguous orientation values.
			// For example if we don't wrap heading, if we rotate from 30 degrees one direction
			//   60 degrees and through the 0 angle, we would consider our heading (incorrectly)
			//   to be 30 degrees again.
			if (player.Heading < 0)
			{
				player.Heading += MathHelper.ToRadians (360);
			}
			// Clamp player heading between 0 and 360 degrees.
			player.Heading = (player.Heading % MathHelper.ToRadians(360));
			//simple collision detection for walls only
			foreach (Wall w in walls) {
				if (player.DetectCollision (w)) {
					if (currentKeyboardState.IsKeyDown(Keys.W)) {
						player.Position -= new Vector2 (velX, velY);
					} else {
						player.Position += new Vector2 (velX, velY);
					}
				}
			}

			float clampedX = MathHelper.Clamp (player.Position.X, player.Width / 2, GraphicsDevice.Viewport.Width - player.Width / 2);
			float clampedY = MathHelper.Clamp (player.Position.Y, player.Height / 2, GraphicsDevice.Viewport.Height - player.Height / 2);
			player.Position = new Vector2 (clampedX, clampedY);
		}

		private void SeekTarget(GameTime gameTime, Vector2 target)
		{
			float distance = Vector2.Distance (player.Position, target);
			if (distance >= 1f) {
				Console.WriteLine ("Distance: " + distance);
				Vector2 v = Vector2.Normalize (target - player.Position);
				float angle = (float)Math.Acos (Vector2.Dot (Vector2.Normalize (player.HeadingVector), v));
				Console.WriteLine ("angle: " + angle);
				if (angle >= 0.1f) {
					//float angle = (float)Math.Acos (Vector2.Dot (Vector2.Normalize (player.HeadingVector), v));
					float crossZ = Vector3.Normalize (Vector3.Cross (new Vector3 (player.Position.X, player.Position.Y, 0), new Vector3 (v.X, v.Y, 0))).Z;
					player.Heading += playerTurnSpeed * crossZ;
				} else {
					player.Position += v * distance/playerMoveSpeed;
				}
				float clampedX = MathHelper.Clamp (player.Position.X, player.Width / 2, GraphicsDevice.Viewport.Width - player.Width / 2);
				float clampedY = MathHelper.Clamp (player.Position.Y, player.Height / 2, GraphicsDevice.Viewport.Height - player.Height / 2);
				player.Position = new Vector2 (clampedX, clampedY);
			} else {
				targetReached = true;
			}
		
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Prsovides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
			//TODO: Add your drawing code here
			spriteBatch.Begin();
			for (int i = 0; i < numWalls; i++) {

				walls [i].Draw (spriteBatch);
			}
			foreach (Player p in players) {
				p.Draw (spriteBatch, font, 0);
			}
				
			//more debug
			//spriteBatch.Draw(debugTex, player.BoundingBox, Color.White);
			spriteBatch.End ();

			base.Draw (gameTime);
		}		

		private void LoadGoals()
		{
			FileStream fs = File.OpenRead ("Data/TestData");
			XmlSerializer serializer = new XmlSerializer (typeof(List<NavGoal>));
			testData = (List<NavGoal>)serializer.Deserialize (fs);
		}
	}
		
}

