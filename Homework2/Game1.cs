#region Using Statements
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
	public struct NavGoal{
		public Vector2 Start { get; set; }
		public Vector2 End { get; set; }

		public NavGoal(Vector2 s, Vector2 e){
			Start = s;
			End = e;
		}
	}
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		const bool autonomousPlayer = true;
		const int maxTicks = 300;
		const int numPlayers = 10;
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont font;

		List<Player> players;
		Population population;
		static List<Agent> walls;
		static List<Agent> agents;
		List<NavGoal> testData;
		LinkedList<String> lines;
		int numWalls = 1;
		KeyboardState currentKeyboardState;
		KeyboardState previousKeyboardState;
		MouseState currentMouseState;
		MouseState previousMouseState;
		float playerMoveSpeed;
		float playerTurnSpeed;
		Vector2 moveTarget;
		bool targetReached = true;
		int numTicks;
		int testCounter;
		int generation;
		bool firstRun = true;
		//Debug stuff
		Texture2D debugTex;

		public List<Player> Players { get { return players; } }
		public static List<Agent> Walls { get { return walls; } }
		public static List<Agent> Agents { get { return agents; } }

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";
			graphics.IsFullScreen = false;
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;
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
			players = new List<Player>();
			for (int i = 0; i < numPlayers; i++) {
				players.Add (new Player ());
			}
			playerMoveSpeed = 8.0f;
			playerTurnSpeed = MathHelper.ToRadians (1.0f); 
			walls = new List<Agent> (numWalls);
			//for(int i = 0; i < numWalls; i++)
			//	walls.Add (new Wall());
			agents = new List<Agent> ();
			StreamReader sr = new StreamReader ("agents.txt");
			lines = new LinkedList<String>();
			while (!sr.EndOfStream) {
				lines.AddLast(sr.ReadLine ());
			}
			for (int i = 0; i < lines.Count; i++) {
				agents.Add (new Player ());
			}
			numTicks = maxTicks;
			testCounter = 0;
			generation = 0;
			LoadGoals ();
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
			Vector2 playerPosition = testData[testCounter].Start;
			foreach (Player p in players) {
				p.Initialize (Content.Load<Texture2D> ("Graphics/HW1Player"), playerPosition, 0.0f);
			}
			population = new Population (numPlayers, players [0].NavNetwork.GetNumWeights ());
			Random r = new Random ();
			Texture2D wallHorizTex = Content.Load<Texture2D> ("Graphics/HW1WallHorizontal");
			float position = 0;
			while (position < GraphicsDevice.Viewport.TitleSafeArea.Width) {
				Wall w = new Wall ();
				w.Initialize (wallHorizTex, new Vector2 (position, 0));
				walls.Add (w);
				w = new Wall ();
				w.Initialize (wallHorizTex, new Vector2 (position, 
					GraphicsDevice.Viewport.TitleSafeArea.Height - wallHorizTex.Height));
				walls.Add (w);
				position += wallHorizTex.Width;					
			}
			Texture2D wallVertTex = Content.Load<Texture2D> ("Graphics/HW1WallVertical");
			position = 0;
			while (position < GraphicsDevice.Viewport.TitleSafeArea.Height) {
				Wall w = new Wall ();
				w.Initialize (wallVertTex, new Vector2 (0, position));
				walls.Add (w);
				w = new Wall ();
				w.Initialize (wallVertTex, new Vector2 (GraphicsDevice.Viewport.TitleSafeArea.Width - wallVertTex.Width,
					position));
				walls.Add (w);
				position += wallVertTex.Height;					
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

			/*if (currentMouseState.LeftButton == ButtonState.Pressed && 
				previousMouseState.LeftButton == ButtonState.Released) {
				moveTarget = new Vector2 (currentMouseState.X, currentMouseState.Y);
				targetReached = false;
			}*/
			//stop
			if (currentKeyboardState.IsKeyDown (Keys.S) && previousKeyboardState.IsKeyUp (Keys.S)) {
				SaveWeights ();
			}
			/*
			if(!targetReached)
				targetReached = players[0].SeekTarget (moveTarget, GraphicsDevice.Viewport);
			*/

			UpdatePopulation ();
			if (firstRun) {
				firstRun = false;
			}
			/*if (!targetReached)
				targetReached = players [0].SeekTarget (moveTarget, GraphicsDevice.Viewport);
			*/

			foreach (Player p in players) {
				p.UpdateSensors ();
			}
			base.Update (gameTime);
		}
			/*
			 * Keep number of ticks 
			 */
		private void UpdatePopulation()
		{
			//Console.WriteLine (numTicks);
			if (numTicks < maxTicks) {
				foreach (Player p in players) {
					p.Update (moveTarget);
				}
				numTicks++;
			} else {
				if (!firstRun) {
					int mod = (((testCounter - 1) % testData.Count) + testData.Count) % testData.Count;
					foreach (Player p in players) {
						p.EndOfRun (testData[mod].Start, moveTarget);
					}
					population = population.Epoch ();
					generation++;
				}
				int i = 0;
				foreach (Player p in players) {
					p.UpdateGenotype(population.Genomes [i++]);
					p.Position = testData [testCounter].Start;
					p.Heading = 0;
					moveTarget = testData [testCounter].End;
				}
				numTicks = 0;
				testCounter = (testCounter + 1) % testData.Count ;
			}
		}


		/*	Player movement method
		 *  to be replaced by player.Update()
		 * */

		private void UpdatePlayerManual(GameTime gameTime, Player player)
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
		//Moved to player class (not yet tested) - delete this if it worked
		/*
		private void SeekTarget(GameTime gameTime, Player player, Vector2 target)
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
		*/
		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Prsovides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
			//TODO: Add your drawing code here
			spriteBatch.Begin();
			foreach (Wall w in walls) {
				w.Draw (spriteBatch);
			}
			int j = 0; 
			foreach (Player p in players) {
				if (j == 0)
					p.Draw (spriteBatch, font, 0, true);
				else
					p.Draw (spriteBatch, font, 0, false);
				j++;
			}
			Vector2 markerSize = font.MeasureString ("x");
			spriteBatch.DrawString (font, "x", new Vector2 (moveTarget.X - markerSize.X / 2, moveTarget.Y - markerSize.Y / 2), Color.Green);

			spriteBatch.DrawString (font, "Generation: " + generation, new Vector2 (32, GraphicsDevice.Viewport.TitleSafeArea.Height - font.LineSpacing - 32), Color.Black);
			//spriteBatch.Draw(debugTex, player.BoundingBox, Color.White);
			spriteBatch.End ();

			base.Draw (gameTime);
		}		

		private void LoadGoals()
		{
			FileStream fs = File.OpenRead ("Content/Data/TestData.xml");
			XmlSerializer serializer = new XmlSerializer (typeof(List<NavGoal>));
			testData = (List<NavGoal>)serializer.Deserialize (fs);
		}

		private void SaveWeights(){
			StreamWriter writer = new StreamWriter ("Genomes.xml");
			XmlSerializer serializer = new XmlSerializer (typeof(List<Genome>));
			serializer.Serialize (writer, population.Genomes);
		}
	}
		
}

