using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ViperEngine.Level;
using ScreenManager.Screens;
using ViperEngine.Camera;
using ViperEngine.GameObjects;
using ScreenManager.Models;
using ViperEngine;
using ScreenManager.Models.Transitions;
using ViperEngine.AI;

namespace Viper
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private const string playersString = "Players: {0}";
        private const int AIIndex = 2;

        SpriteFont baseFont;

        Vector2 playerOneStartPos, playerTwoStartPos;

        Player player1, player2, AI;

        int player1Index, player2Index;

        Camera camera1, camera2;
        private Viewport _defaultView, _leftView, _rightView;

        Controls controls;

        public static GameDifficulty dificulty;

        Level map;
        Texture2D tileSheet, border, objectSheet;

        int objectWidth, objectHeight;

        #region Menus
        ScreenManager.Screens.ScreenManager manager;
        Screen mainMenu, optionsMenu, playersMenu, deadMenu, pauseMenu, informationMenu;
        List<Selection> mainMenuSelectionList, optionsMenuSelectionList, playersMenuSelectionList, deadMenuSelectionList,
            pauseMenuSelectionList, informationMenuSelectionList;
        List<Text> mainMenuTextList, informationMenuTextList;
        #endregion

        CollisionDetection detectionPlayer1, detectionPlayer2, detectionAI;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 640;

            graphics.ApplyChanges();

            #region ScreenManager
            ScreenManager.Screens.ScreenManager.Play = "PLAY";
            ScreenManager.Screens.ScreenManager.Exit = "EXIT";

            Text playersText = new Text { Txt = playersString, Position = new Vector2(10, 10) };
            playersText.OnUpdate += new Text.TextUpdateEventHandeler(playersText_OnUpdate);

            manager = new ScreenManager.Screens.ScreenManager();
            mainMenuSelectionList = new List<Selection>();
            Selection play = new Selection { Text = "Play", Targeted = true, Description = "Click here to play the game", Action = "PLAY" };
            play.OnClick += new Selection.SelectionEventHandeler(play_OnClick);
            mainMenuSelectionList.Add(play);
            mainMenuSelectionList.Add(new Selection { Text = "Options", Description = "Click here to manage the options.", Action = "Options" });
            mainMenuSelectionList.Add(new Selection { Text = "Information", Description = "Click here to see the information screen.", Action = "Information" });
            mainMenuSelectionList.Add(new Selection { Text = "Exit", Description = "Click here to exit the game", Action = "EXIT" });
            mainMenu = new Screen(800, 640, mainMenuSelectionList);
            mainMenu.Active = true;
            mainMenu.BaseFontPath = @"Font/BaseFont";
            mainMenu.DescriptionFontPath = @"Font/DescriptionFont";
            mainMenu.Name = "Main menu";

            mainMenuTextList = new List<Text>();
            mainMenuTextList.Add(playersText);
            mainMenu.Texts.AddRange(mainMenuTextList);

            optionsMenuSelectionList = new List<Selection>();
            optionsMenuSelectionList.Add(new Selection { Text = "Players", Description = "Click here so change the amount of players.", Targeted = true, Action = "Players" });
            optionsMenuSelectionList.Add(new Selection { Text = "Back", Description = "Go back to main menu", Action = "Main menu" });
            optionsMenu = new Screen(800, 640, optionsMenuSelectionList);
            optionsMenu.Active = false;
            optionsMenu.BaseFontPath = @"Font/BaseFont";
            optionsMenu.DescriptionFontPath = @"Font/DescriptionFont";
            optionsMenu.Name = "Options";
            optionsMenu.Texts.Add(playersText);

            playersMenuSelectionList = new List<Selection>();
            Selection one = new Selection { Text = "One", Description = "Click here to chose 1 player mode.", Targeted = Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.ONE ? true : false
            , Action = "Options" };
            one.OnClick += new Selection.SelectionEventHandeler(one_OnClick);
            playersMenuSelectionList.Add(one);
            Selection two = new Selection { Text = "Two", Description = "Click here to chose 2 player mode.", Targeted = Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.TWO ? true : false
            , Action = "Options" };
            two.OnClick += new Selection.SelectionEventHandeler(two_OnClick);
            playersMenuSelectionList.Add(two);
            playersMenuSelectionList.Add(new Selection { Text = "Back", Description = "Click here to go back to options", Targeted = false, Action = "Options" });
            playersMenu = new Screen(800, 640, playersMenuSelectionList);
            playersMenu.Active = false;
            playersMenu.BaseFontPath = @"Font/BaseFont";
            playersMenu.DescriptionFontPath = @"Font/DescriptionFont";
            playersMenu.Name = "Players";
            playersMenu.Texts.Add(playersText);

            deadMenuSelectionList = new List<Selection>();
            Selection mainMenuSelection = new Selection { Text = "Main menu", Targeted = true, Description = "Back to main menu", Action = "Main menu" };
            mainMenuSelection.OnClick += new Selection.SelectionEventHandeler(mainMenu_OnClick);
            deadMenuSelectionList.Add(mainMenuSelection);
            deadMenuSelectionList.Add(new Selection { Text = "Exit", Description = "Exit the game", Action = "EXIT" });
            deadMenu = new Screen(800, 640, deadMenuSelectionList);
            deadMenu.Active = false;
            deadMenu.BaseFontPath = @"Font/BaseFont";
            deadMenu.DescriptionFontPath = @"Font/DescriptionFont";
            deadMenu.Name = "Dead menu";

            pauseMenuSelectionList = new List<Selection>();
            pauseMenuSelectionList.Add(mainMenuSelection);
            Selection resumeGameSelection = new Selection { Text = "Resume game", Description = "Resume the game." };
            resumeGameSelection.OnClick += new Selection.SelectionEventHandeler(resumeGameSelection_OnClick);
            pauseMenuSelectionList.Add(resumeGameSelection);
            pauseMenuSelectionList.Add(new Selection { Text = "Exit", Description = "Exit the game", Action = "EXIT" });
            pauseMenu = new Screen(800, 640, pauseMenuSelectionList);
            pauseMenu.Active = false;
            pauseMenu.BaseFontPath = @"Font/BaseFont";
            pauseMenu.DescriptionFontPath = @"Font/DescriptionFont";
            pauseMenu.Name = "Pause menu";

            informationMenuSelectionList = new List<Selection>();
            informationMenuSelectionList.Add(mainMenuSelection);
            informationMenu = new Screen(800, 640, informationMenuSelectionList);
            informationMenu.Name = "Information";
            informationMenu.Active = false;
            informationMenu.BaseFontPath = @"Font/BaseFont";
            informationMenu.DescriptionFontPath = @"Font/DescriptionFont";
            informationMenuTextList = new List<Text>();
            #region Good objects
            Text goodInformationText = new Text("Good objects", new Vector2(150, 200));
            informationMenu.Texts.Add(goodInformationText);

            Text appleText = new Text("Apple: 10p", new Vector2(150, 230));
            informationMenu.Texts.Add(appleText);
            Image appleImage = new Image(new Vector2(120, 230), Content.Load<Texture2D>(@"GameObjects/objects"), 0, 1, 16);
            informationMenu.Images.Add(appleImage);

            Text carrotText = new Text("Carrot: 25p", new Vector2(150, 260));
            informationMenu.Texts.Add(carrotText);
            Image carrotImage = new Image(new Vector2(120, 260), Content.Load<Texture2D>(@"GameObjects/objects"), 0, 0, 16);
            informationMenu.Images.Add(carrotImage);

            Text bananaText = new Text("Banana: 35p", new Vector2(150, 290));
            informationMenu.Texts.Add(bananaText);
            Image bananaImage = new Image(new Vector2(120, 290), Content.Load<Texture2D>(@"GameObjects/objects"), 1, 1, 16);
            informationMenu.Images.Add(bananaImage);

            Text chiliText = new Text("Chili: 50p", new Vector2(150, 320));
            informationMenu.Texts.Add(chiliText);
            Image chiliImage = new Image(new Vector2(120, 320), Content.Load<Texture2D>(@"GameObjects/objects"), 2, 0, 16);
            informationMenu.Images.Add(chiliImage);
            #endregion
            #region Evil objects
            Text evilInformationText = new Text("Evil objects", new Vector2(500, 200));
            informationMenu.Texts.Add(evilInformationText);

            Text brokenBottleText = new Text("Broken bottle: -10p", new Vector2(500, 230));
            informationMenu.Texts.Add(brokenBottleText);
            Image brokenBottleImage = new Image(new Vector2(470, 230), Content.Load<Texture2D>(@"GameObjects/objects"), 9, 6, 16);
            informationMenu.Images.Add(brokenBottleImage);

            Text toxicTubeText = new Text("Toxic tube: -50p", new Vector2(500, 260));
            informationMenu.Texts.Add(toxicTubeText);
            Image toxicTubeImage = new Image(new Vector2(470, 260), Content.Load<Texture2D>(@"GameObjects/Toxictube"), 0, 0, 16);
            informationMenu.Images.Add(toxicTubeImage);
            #endregion

            manager.Screens.Add(mainMenu);
            manager.Screens.Add(optionsMenu);
            manager.Screens.Add(playersMenu);
            manager.Screens.Add(deadMenu);
            manager.Screens.Add(pauseMenu);
            manager.Screens.Add(informationMenu);

            manager.GameState = GameState.MENU;
            #endregion

            // TODO: Add your initialization logic here
            map = new Level();
            map.TileHeight = 32;
            map.TileWidth = 32;

            objectHeight = 16;
            objectWidth = 16;

            playerOneStartPos = new Vector2(160, 160);
            playerTwoStartPos = new Vector2(800 - 160, 160);

            player1 = new Player(playerOneStartPos.X, playerOneStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left);
            player1.Die += new EventHandler(player_Die);
            player1.Color = Color.Purple;
            Engine.Singleton.Players.Add(player1);

            player2 = new Player(playerTwoStartPos.X, playerTwoStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left);
            player2.Die += new EventHandler(player_Die);
            player2.Color = Color.Tan;
            Engine.Singleton.Players.Add(player2);

            AI = new AIEasy(320, playerTwoStartPos.Y);
            AI.Die += new EventHandler(player_Die);
            AI.Color = Color.Yellow;
            Engine.Singleton.Players.Add(AI);

            camera1 = new Camera(GraphicsDevice.Viewport);

            controls = new Controls();
            controls.MakeMouseVisible += new EventHandler(controls_MakeMouseVisible);

            Engine.Singleton.numberOfPlayers = Engine.NumberOfPlayers.ONE;

            detectionPlayer1 = new CollisionDetection(player1Index, map);
            detectionPlayer2 = new CollisionDetection(player2Index, map);
            detectionAI = new CollisionDetection(AIIndex, map);

            player1Index = 0;
            player2Index = 1;

            Engine.Singleton.Players[player1Index].Name = "Player 1";
            Engine.Singleton.Players[player2Index].Name = "Player 2";
            Engine.Singleton.Players[AIIndex].Name = "Easy AI";

            Engine.Singleton.currentMap = map;

            base.Initialize();
        }

        private void Reinitialize()
        {
            if (Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.ONE)
            {
                Engine.Singleton.Players[player1Index].Reinitialize(playerOneStartPos.X, playerOneStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left); //= new Player(playerOneStartPos.X, playerOneStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left);
                Engine.Singleton.Players[AIIndex].Reinitialize(playerTwoStartPos.X, playerTwoStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left);
            }
            else if (Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.TWO)
            {
                Engine.Singleton.Players[player1Index].Reinitialize(playerOneStartPos.X, playerOneStartPos.Y, Keys.W, Keys.S, Keys.D, Keys.A); //= new Player(playerOneStartPos.X, playerOneStartPos.Y, Keys.W, Keys.S, Keys.D, Keys.A);

                Engine.Singleton.Players[player2Index].Reinitialize(playerTwoStartPos.X, playerTwoStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left); //= new Player(playerTwoStartPos.X, playerTwoStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left);
            }
        }

        private void AddGameObjects()
        {
            Engine.Singleton.ClearGameObjects();
            Engine.Singleton.AddApple(objectSheet, map);
        }

        #region Events

        void playersText_OnUpdate(Text sender)
        {
            sender.Txt = string.Format(playersString, Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.ONE ? 1 : 2);
        }

        void controls_MakeMouseVisible(object sender, EventArgs e)
        {
            this.IsMouseVisible = true;
        }

        void resumeGameSelection_OnClick(Screen sender, SelectionArgs e)
        {
            pauseMenu.Active = false;
            manager.GameState = GameState.PLAY;
        }

        void player_Die(object sender, EventArgs e)
        {
            Player player = sender as Player;
            player.Alive = false;
            this.IsMouseVisible = true;
            manager.GameState = GameState.FREEZE;
            deadMenu.Active = true;
            deadMenuSelectionList[0].Targeted = true;
        }

        void mainMenu_OnClick(Screen sender, SelectionArgs e)
        {
            foreach (Screen item in manager.Screens)
            {
                item.Active = false;
            }

            mainMenu.Active = true;

            manager.GameState = GameState.MENU;
        }

        void play_OnClick(Screen sender, SelectionArgs e)
        {
            if ((!Engine.Singleton.Players[player1Index].Alive && Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.ONE)
                || (Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.TWO && (!Engine.Singleton.Players[player1Index].Alive || !Engine.Singleton.Players[player2Index].Alive)))
            {
                Reinitialize();
            }

            Engine.Singleton.Players[player1Index].PlayerEffect.Parameters["key_color"].SetValue(Color.White.ToVector3());
            Engine.Singleton.Players[player1Index].PlayerEffect.Parameters["new_color"].SetValue(Engine.Singleton.Players[player1Index].Color.ToVector3());

            Engine.Singleton.Players[player2Index].PlayerEffect.Parameters["key_color"].SetValue(Color.White.ToVector3());
            Engine.Singleton.Players[player2Index].PlayerEffect.Parameters["new_color"].SetValue(Engine.Singleton.Players[player2Index].Color.ToVector3());

            Engine.Singleton.Players[player1Index].PlayerEffect.CurrentTechnique = Engine.Singleton.Players[player1Index].PlayerEffect.Techniques["Player"];
            Engine.Singleton.Players[player2Index].PlayerEffect.CurrentTechnique = Engine.Singleton.Players[player2Index].PlayerEffect.Techniques["Player"];
            Engine.Singleton.Players[AIIndex].PlayerEffect.CurrentTechnique = Engine.Singleton.Players[AIIndex].PlayerEffect.Techniques["Player"];

            manager.GameState = GameState.PLAY;

            AddGameObjects();

            this.IsMouseVisible = false;
        }

        void two_OnClick(Screen sender, SelectionArgs e)
        {
            Engine.Singleton.numberOfPlayers = Engine.NumberOfPlayers.TWO;

            Engine.Singleton.Players[player1Index].Reinitialize(playerOneStartPos.X, playerOneStartPos.Y, Keys.W, Keys.S, Keys.D, Keys.A); //= new Player(playerOneStartPos.X, playerOneStartPos.Y, Keys.W, Keys.S, Keys.D, Keys.A);
            camera1 = new Camera(_leftView);
            //Engine.Singleton.Players[player1Index].LoadContent(Content);
            detectionPlayer1 = new CollisionDetection(player1Index, player2Index, map);
            //Engine.Singleton.Players[player1Index].Die += new EventHandler(player_Die);
            //Engine.Singleton.Players[player1Index].Name = "Player 1";

            Engine.Singleton.Players[player2Index].Reinitialize(playerTwoStartPos.X, playerTwoStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left); //= new Player(playerTwoStartPos.X, playerTwoStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left);
            camera2 = new Camera(_rightView);
            //Engine.Singleton.Players[player2Index].LoadContent(Content);
            detectionPlayer2 = new CollisionDetection(player2Index, player1Index, map);
            //Engine.Singleton.Players[player2Index].Die += new EventHandler(player_Die);
            //Engine.Singleton.Players[player2Index].Name = "Player 2";

            Engine.Singleton.Players[player1Index].PlayerEffect = Content.Load<Effect>(@"Effects/Player1Color");
            Engine.Singleton.Players[player2Index].PlayerEffect = Content.Load<Effect>(@"Effects/Player2Color");
        }

        void one_OnClick(Screen sender, SelectionArgs e)
        {
            Engine.Singleton.numberOfPlayers = Engine.NumberOfPlayers.ONE;

            Engine.Singleton.Players[player1Index] = new Player(playerOneStartPos.X, playerOneStartPos.Y, Keys.Up, Keys.Down, Keys.Right, Keys.Left);
            Engine.Singleton.Players[player1Index].LoadContent(Content);
            detectionPlayer1 = new CollisionDetection(player1Index, map);
            Engine.Singleton.Players[player1Index].Die += new EventHandler(player_Die);
            Engine.Singleton.Players[player1Index].Name = "Player 1";

            Engine.Singleton.Players[player1Index].PlayerEffect = Content.Load<Effect>(@"Effects/Player1Color");
            Engine.Singleton.Players[player2Index].PlayerEffect = Content.Load<Effect>(@"Effects/Player2Color");
        }
        #endregion

        protected override void LoadContent()
        {
            _defaultView = GraphicsDevice.Viewport;
            _leftView = _defaultView;
            _rightView = _defaultView;
            _leftView.Width = _leftView.Width / 2;
            _rightView.Width = _rightView.Width / 2;
            _rightView.X = _leftView.Width;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            manager.LoadContent(Content);

            tileSheet = Content.Load<Texture2D>(@"TileSet/Woodland_Tileset");
            map.LoadMap(@"Maps/ForestViper.txt", tileSheet);
            map.LoadTileSet(tileSheet);
            map.PopulateCollisionLayer();

            border = Content.Load<Texture2D>("Border");

            Engine.Singleton.Players[player1Index].LoadContent(Content);
            Engine.Singleton.Players[player2Index].LoadContent(Content);
            Engine.Singleton.Players[AIIndex].LoadContent(Content);

            objectSheet = Content.Load<Texture2D>(@"GameObjects/objects");
            Engine.Singleton.ToxicTubeTexture = Content.Load<Texture2D>(@"GameObjects/Toxictube");

            baseFont = Content.Load<SpriteFont>(@"Font/GameBaseFont");
            Engine.Singleton.GameObjectFont = Content.Load<SpriteFont>(@"Font/GameObjectFont");

            Engine.Singleton.Players[player1Index].PlayerEffect = Content.Load<Effect>(@"Effects/Player1Color");
            Engine.Singleton.Players[player2Index].PlayerEffect = Content.Load<Effect>(@"Effects/Player2Color");
            Engine.Singleton.Players[AIIndex].PlayerEffect = Content.Load<Effect>(@"Effects/Player2Color");

            Engine.Singleton.TransitionTextures.Add(Content.Load<Texture2D>(@"Transition/Black800x640"));

            mainMenuSelectionList.ForEach(s => s.Transition.Add(new FadeOut { Texture = Engine.Singleton.TransitionTextures.First() }));
            optionsMenuSelectionList.ForEach(o => o.Transition.Add(new FadeOut { Texture = Engine.Singleton.TransitionTextures.First() }));
            playersMenuSelectionList.ForEach(p => p.Transition.Add(new FadeOut { Texture = Engine.Singleton.TransitionTextures.First() }));
            informationMenuSelectionList.ForEach(i => i.Transition.Add(new FadeOut { Texture = Engine.Singleton.TransitionTextures.First() }));

            Engine.Singleton.objectsTextureSheet = objectSheet;
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (manager.GameState == GameState.PLAY)
            {
                Engine.Singleton.Update(gameTime);

                controls.Update(ref Engine.Singleton.Players[player1Index].lastkeystate, Engine.Singleton.Players[player1Index].Up,
                    Engine.Singleton.Players[player1Index].Down, Engine.Singleton.Players[player1Index].Right,
                    Engine.Singleton.Players[player1Index].Left, ref manager);
                
                //Engine.Singleton.Players[player1Index].Update(gameTime);
                detectionPlayer1.Update(ref manager);
                camera1.Update(Engine.Singleton.Players[player1Index].Position);

                if (Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.TWO)
                {
                    controls.Update(ref Engine.Singleton.Players[player2Index].lastkeystate, Engine.Singleton.Players[player2Index].Up,
                        Engine.Singleton.Players[player2Index].Down, Engine.Singleton.Players[player2Index].Right,
                        Engine.Singleton.Players[player2Index].Left, ref manager);

                    Engine.Singleton.Players[player2Index].Update(gameTime);
                    detectionPlayer2.Update(ref manager);
                    camera2.Update(Engine.Singleton.Players[player2Index].Position);
                }
                else
                {
                    Engine.Singleton.Players[AIIndex].Update(gameTime);
                    detectionAI.Update(ref manager);
                }

                Engine.Singleton.GameObjects.ForEach(a => a.Update(gameTime));
            }
            else if (manager.GameState == GameState.MENU)
            {
                manager.Update(gameTime);
            }
            else if (manager.GameState == GameState.PAUSE)
            {
                controls.Update(ref player1.lastkeystate, player1.Up, player1.Down, player1.Right, player1.Left, ref manager);
                manager.Update(gameTime);
            }
            else if (manager.GameState == GameState.FREEZE)
            {
                if (!Engine.Singleton.Players[player1Index].Alive || !Engine.Singleton.Players[player2Index].Alive)
                {
                    manager.Update(gameTime);
                }
            }
            else if (manager.GameState == GameState.QUIT)
            {
                this.Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (manager.GameState == GameState.PLAY)
            {
                GraphicsDevice.Clear(Color.Gray);

                if (Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.TWO)
                {
                    GraphicsDevice.Viewport = _leftView;
                    DrawSprites(camera1, gameTime);

                    spriteBatch.Begin();
                    spriteBatch.DrawString(baseFont, Engine.Singleton.Players[player1Index].Name, new Vector2(10, 10), Color.Black);
                    spriteBatch.DrawString(baseFont, string.Format("Score: {0}", Engine.Singleton.Players[player1Index].Points), new Vector2(100, 10), Color.Black);
                    spriteBatch.End();

                    GraphicsDevice.Viewport = _rightView;
                    DrawSprites(camera2, gameTime);

                    spriteBatch.Begin();
                    spriteBatch.DrawString(baseFont, Engine.Singleton.Players[player2Index].Name, new Vector2(10, 10), Color.Black);
                    spriteBatch.DrawString(baseFont, string.Format("Score: {0}", Engine.Singleton.Players[player2Index].Points), new Vector2(100, 10), Color.Black);
                    spriteBatch.End();

                    GraphicsDevice.Viewport = _defaultView;

                    spriteBatch.Begin();
                    spriteBatch.Draw(border, new Vector2(graphics.PreferredBackBufferWidth / 2 - 2.5f, 0), new Rectangle(0, 0, border.Width, graphics.PreferredBackBufferHeight), Color.White);
                    spriteBatch.End();
                }
                else
                {
                    DrawSprites(camera1, gameTime);

                    spriteBatch.Begin();
                    spriteBatch.DrawString(baseFont, Engine.Singleton.Players[player1Index].Name, new Vector2(10, 10), Color.Black);
                    spriteBatch.DrawString(baseFont, string.Format("Score: {0}", Engine.Singleton.Players[player1Index].Points), new Vector2(100, 10), Color.Black);
                    spriteBatch.End();
                }
            }
            else if (manager.GameState == GameState.MENU)
            {
                GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin();
                manager.Draw(spriteBatch);
                spriteBatch.End();
            }
            else if (manager.GameState == GameState.PAUSE)
            {
                GraphicsDevice.Clear(Color.Gray);

                if (Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.TWO)
                {
                    GraphicsDevice.Viewport = _leftView;
                    DrawSprites(camera1, gameTime);

                    GraphicsDevice.Viewport = _rightView;
                    DrawSprites(camera2, gameTime);

                    GraphicsDevice.Viewport = _defaultView;

                    spriteBatch.Begin();
                    spriteBatch.Draw(border, new Vector2(graphics.PreferredBackBufferWidth / 2 - 2.5f, 0), new Rectangle(0, 0, border.Width, graphics.PreferredBackBufferHeight), Color.White);
                    spriteBatch.End();
                }
                else
                {
                    DrawSprites(camera1, gameTime);
                }

                spriteBatch.Begin();
                manager.Draw(spriteBatch);
                spriteBatch.End();
            }
            else if (manager.GameState == GameState.FREEZE)
            {
                GraphicsDevice.Clear(Color.Gray);

                if (Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.TWO)
                {
                    GraphicsDevice.Viewport = _leftView;
                    DrawSprites(camera1, gameTime);

                    GraphicsDevice.Viewport = _rightView;
                    DrawSprites(camera2, gameTime);

                    GraphicsDevice.Viewport = _defaultView;

                    spriteBatch.Begin();
                    spriteBatch.Draw(border, new Vector2(graphics.PreferredBackBufferWidth / 2 - 2.5f, 0), Color.White);
                    spriteBatch.End();
                }
                else
                {
                    DrawSprites(camera1, gameTime);
                }

                string deadText = "{0} has died";

                if (!Engine.Singleton.Players[player1Index].Alive)
                {
                    deadText = string.Format(deadText, Engine.Singleton.Players[player1Index].Name);
                }
                else if (!Engine.Singleton.Players[player2Index].Alive)
                {
                    deadText = string.Format(deadText, Engine.Singleton.Players[player2Index].Name);
                }

                if (!Engine.Singleton.Players[player1Index].Alive || !Engine.Singleton.Players[player2Index].Alive)
                {
                    foreach (var item in manager.Screens)
                    {
                        item.Active = false;
                    }

                    deadMenu.Active = true;
                    deadMenu.Name = deadText;
                    deadMenu.SetSelectionPositions();

                    spriteBatch.Begin();
                    manager.Draw(spriteBatch);
                    spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }

        private void DrawSprites(Camera camera, GameTime gameTime)
        {
            if (Engine.Singleton.Debug)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, null, camera._transform);

                RasterizerState state = new RasterizerState();
                state.FillMode = FillMode.WireFrame;
                spriteBatch.GraphicsDevice.RasterizerState = state;
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    null, null, null, null,
                    camera._transform);
            }

            map.DrawMap(spriteBatch, new Vector2(0, 0));

            foreach (GameObject item in Engine.Singleton.GameObjects)
            {
                item.Draw(spriteBatch, gameTime);
            }

            spriteBatch.End();

            //Draw players last
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, Engine.Singleton.Players[player1Index].PlayerEffect, camera._transform);
            Engine.Singleton.Players[player1Index].Draw(gameTime, spriteBatch);
            spriteBatch.End();

            if (Engine.Singleton.numberOfPlayers == Engine.NumberOfPlayers.TWO)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, Engine.Singleton.Players[player2Index].PlayerEffect, camera._transform);
                Engine.Singleton.Players[player2Index].Draw(gameTime, spriteBatch);
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, Engine.Singleton.Players[AIIndex].PlayerEffect, camera._transform);
                Engine.Singleton.Players[AIIndex].Draw(gameTime, spriteBatch);
                spriteBatch.End();
            }
        }
    }
}
