#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;

#endregion

namespace XMLGenerator
{
	public struct NavGoal{
		public Vector2 Start { get; set; }
		public Vector2 End { get; set; }

		public NavGoal(Vector2 s, Vector2 e){
			Start = s;
			End = e;
		}
	}
	public struct WallData{
		public Vector2 position;
		public bool horizontal;

		public WallData(Vector2 pos, bool horiz){
			position = pos;
			horizontal = horiz;
		}
	}
	
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
			graphics.IsFullScreen = true;		
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			List<NavGoal> data = new List<NavGoal> ();
			data.Add (new NavGoal(new Vector2 (100, 200), new Vector2 (200, 200)));
			data.Add (new NavGoal(new Vector2 (300, 300), new Vector2 (400, 400)));
			data.Add (new NavGoal(new Vector2 (500, 500), new Vector2 (500, 100)));
			data.Add (new NavGoal(new Vector2 (800, 600), new Vector2 (600, 800)));
			data.Add (new NavGoal(new Vector2 (123, 456), new Vector2 (654, 321)));

			StreamWriter writer = new StreamWriter ("TestData.xml");
			XmlSerializer serializer = new XmlSerializer(typeof(List<NavGoal>));
			serializer.Serialize (writer, data);

			List<WallData> walls = new List<WallData> ();
			walls.Add (new WallData (new Vector2(512, 224),false));

			writer = new StreamWriter ("Walls.xml");
			serializer = new XmlSerializer (typeof(List<WallData>));
			serializer.Serialize (writer, walls);


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
			base.Update (gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
			//TODO: Add your drawing code here
            
			base.Draw (gameTime);
		}
	}
}

