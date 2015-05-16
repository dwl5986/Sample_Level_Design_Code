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
using System.IO;

namespace Natural_Selection
{
    class Level
    {
        // Physical Structure of Level
        private Tile[,] tiles;
        private Tile[,] bkgTiles;
        public ContentManager content;
        public Player player;
        public Rectangle window;
        public int totalWidth;
        public List<Gamepiece> objectList = new List<Gamepiece>();
        public List<MovingPlatform> movingList = new List<MovingPlatform>();
        
        // Tiles loaded
        Tile sky;
        Tile platform;
        Tile ground;
        Tile sideMoving;
        Tile upMoving;
        Tile spike;
        Tile bkg;
        Tile bentTree;
        Tile tree;
        Tile bigTree;
        Tile bush;
        Tile doubleBush;
        Tile dirt;
        Tile hiddenGround;
        Tile crawlSpace;

        // Tile Images for Load
        Texture2D skyImage;
        Texture2D platformImage;
        Texture2D groundImage;
        Texture2D spikeImage;
        Texture2D bkgImage;
        Texture2D bentImage;
        Texture2D treeImage;
        Texture2D bigTreeImage;
        Texture2D bushImage;
        Texture2D doubleBushImage;
        Texture2D dirtImage;
        Texture2D hiddenImage;
        Texture2D crawlSpaceImage;
        Texture2D cliffImage;

        private bool thisIsMap = false;
        private bool firstLoad = true;
        private bool direction = true;
        public Collisions collisions;

        /// <summary>
        /// Creates a new Level
        /// </summary>
        /// <param name="fileStream">The file path for the tile data</param>
        /// <param name="levelIndex">The level being loaded</param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, Stream bkgFileStream, int levelIndex, Player p)
        {
            player = p;
            collisions = new Collisions(objectList);
            // Makes a Content Manager to handle the level's stuff
            content = new ContentManager(serviceProvider, "Content");

            LoadTextures();
            LoadBackground(bkgFileStream);
            LoadTiles(fileStream);



            WindowSet();
        }

        #region Level Bounds

        // Makes a Rectangle for every tile
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.width, y * Tile.height, Tile.width, Tile.height);
        }

        // Height of Level measured in Tiles
        public int Height
        {
            get { return tiles.GetLength(1) * Tile.height; }
        }

        // Width of Level measured in Tiles
        public int Width
        {
            get { return tiles.GetLength(0) * Tile.width; }
        }
        #endregion

        #region Window

        public Rectangle Screen
        {
            get { return window; }
            set { window = value; }
        }

        public void WindowSet()
        {
            // Height and Width of Scrolling Window
            window.Width = 800;
            window.Height = Height;

            window.X = 0;
            window.Y = 0;

            window = new Rectangle(window.X, window.Y, window.Width, window.Height);
        }

        public void Movement()
        {
            KeyboardState testKeys = Keyboard.GetState();

            // Tests whether the screen will follow the character and scroll across the level
            if (testKeys.IsKeyDown(Keys.Right) == true)
            {
                if (window.X != totalWidth - 800)
                {
                    if (player.CollisionBox.X < Screen.Width / 2 - 30)
                    {
                        player.CollisionBox = new Rectangle(player.CollisionBox.X + 5, player.CollisionBox.Y, player.CollisionBox.Width, player.CollisionBox.Height);
                    }
                    else
                    {
                        Screen = new Rectangle(Screen.X + 5, Screen.Y, Screen.Width, Screen.Height);
                    }
                }
                else
                {
                    if (player.CollisionBox.X + 60 < Screen.Width)
                    {
                        player.CollisionBox = new Rectangle(player.CollisionBox.X + 5, player.CollisionBox.Y, player.CollisionBox.Width, player.CollisionBox.Height);
                    }
                }
            }
            if (testKeys.IsKeyDown(Keys.Left) == true)
            {
                if (window.X != 0)
                {
                    if (player.CollisionBox.X > Screen.Width / 2 - 30)
                    {
                        player.CollisionBox = new Rectangle(player.CollisionBox.X - 5, player.CollisionBox.Y, player.CollisionBox.Width, player.CollisionBox.Height);
                    }
                    else
                    {
                        Screen = new Rectangle(Screen.X - 5, Screen.Y, Screen.Width, Screen.Height);
                    }
                }
                else
                {
                    if (player.CollisionBox.X > 0)
                    {
                        player.CollisionBox = new Rectangle(player.CollisionBox.X - 5, player.CollisionBox.Y, player.CollisionBox.Width, player.CollisionBox.Height);
                    }
                }
            }
        }

        #endregion

        // Level Content
        public ContentManager Content
        {
            get { return content; }
        }

        #region Loading
        public void LoadTextures()
        {
            skyImage = this.Content.Load<Texture2D>("Sky");
            platformImage = this.Content.Load<Texture2D>("platform1");
            groundImage = this.Content.Load<Texture2D>("groundBlock1");
            cliffImage = this.Content.Load<Texture2D>("cliffsideBlock");
            spikeImage = this.Content.Load<Texture2D>("forrestSpikes");
            bkgImage = this.Content.Load<Texture2D>("forrestBackground");
            bentImage = this.Content.Load<Texture2D>("bentTree");
            treeImage = this.Content.Load<Texture2D>("roundTree");
            bigTreeImage = this.Content.Load<Texture2D>("biggerTree");
            bushImage = this.Content.Load<Texture2D>("bushes");
            doubleBushImage = this.Content.Load<Texture2D>("twoBushes");
            dirtImage = this.Content.Load<Texture2D>("Dirt");
            crawlSpaceImage = this.Content.Load<Texture2D>("crawlSpace");

        }

        // Loads tiles
        private void LoadTiles(Stream fileStream)
        {
            int width;
            // Stores each Line in the text Document in a list
            List<String> lines = new List<string>();
            // Create StreamReader
            StreamReader reader = new StreamReader(fileStream);

            // Read the Text File
            String line = reader.ReadLine();
            // Makes the width equal to the number of characters in each line
            width = line.Length;
            totalWidth = width;

            while (line != null)
            {
                lines.Add(line);
                line = reader.ReadLine();
            }

            // Makes a grid for the tiles
            tiles = new Tile[width, lines.Count];

            // Loop through every position in the Text Document
            for (int y = 0; y < lines.Count; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }
        }

        private void LoadBackground(Stream fileStream)
        {
            int width;
            // Stores each Line in the text Document in a list
            List<String> lines = new List<string>();
            // Create StreamReader
            StreamReader reader = new StreamReader(fileStream);

            // Read the Text File
            String line = reader.ReadLine();
            // Makes the width equal to the number of characters in each line
            width = line.Length;

            while (line != null)
            {
                lines.Add(line);
                line = reader.ReadLine();
            }

            // Makes a grid for the tiles
            bkgTiles = new Tile[width, lines.Count];

            // Loop through every position in the Text Document
            for (int y = 0; y < lines.Count; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char tileType = lines[y][x];
                    bkgTiles[x, y] = LoadTile(tileType, x, y);
                }
            }
        }

        // Loads an individual Tile depending on the type
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank Space
                case '.':
                    sky = new Tile(new Sky(GetBounds(x, y), GetBounds(x, y), skyImage));
                    return sky;

                case '-':
                    platform = new Tile(new StaticPlatform(GetBounds(x, y), GetBounds(x, y), platformImage));
                    AddtoList(platform);
                    return platform;

                case '#':
                    ground = new Tile(new Ground(GetBounds(x, y), GetBounds(x, y), groundImage));
                    AddtoList(ground);
                    return ground;

                case '>':
                    sideMoving = new Tile(new MovingPlatform(GetBounds(x, y), GetBounds(x, y), platformImage, player, true));
                    AddtoMovingList(sideMoving);
                    AddtoList(sideMoving);
                    return sideMoving;

                case '^':
                    upMoving = new Tile(new MovingPlatform(GetBounds(x, y), GetBounds(x, y), platformImage, player, false));
                    AddtoMovingList(upMoving);
                    AddtoList(upMoving);
                    return upMoving;

                case '=':
                    ground = new Tile(new Ground(GetBounds(x, y), GetBounds(x, y), cliffImage));
                    AddtoList(ground);
                    return ground;

                case 'A':
                    spike = new Tile(new Spikes(GetBounds(x, y), GetBounds(x, y), spikeImage));
                    AddtoList(spike);
                    return spike;

                case 'M':
                    bkg = new Tile(new Sky(GetBounds(x, y), GetBounds(x, y), bkgImage));
                    thisIsMap = true;
                    return bkg;

                case '/':
                    bentTree = new Tile(new BackGround(GetBounds(x, y), new Rectangle(GetBounds(x,y).X, GetBounds(x,y).Y, 125, 300), bentImage));
                    return bentTree;

                case '|':
                    tree = new Tile(new BackGround(GetBounds(x, y), new Rectangle(GetBounds(x, y).X, GetBounds(x, y).Y, 125, 300), treeImage));
                    return tree;

                case 'Y':
                    bigTree = new Tile(new BackGround(GetBounds(x, y), new Rectangle(GetBounds(x, y).X, GetBounds(x, y).Y, 180, 450), bigTreeImage));
                    return bigTree;

                case 'B':
                    bush = new Tile(new BackGround(GetBounds(x, y), new Rectangle(GetBounds(x, y).X, GetBounds(x, y).Y, 150, 125), bushImage));
                    return bush;

                case 'D':
                    doubleBush = new Tile(new BackGround(GetBounds(x, y), new Rectangle(GetBounds(x, y).X, GetBounds(x, y).Y, 150, 125), doubleBushImage));
                    return doubleBush;

                case '+':
                    dirt = new Tile(new BackGround(GetBounds(x, y), new Rectangle(GetBounds(x, y).X, GetBounds(x, y).Y, 100, 40), dirtImage));
                    return dirt;

                case '_':
                    hiddenGround = new Tile(new Ground(GetBounds(x, y), new Rectangle(GetBounds(x, y).X, GetBounds(x, y).Y, 100, 40), dirtImage));
                    AddtoList(hiddenGround);
                    return hiddenGround;

                case 'C':
                    crawlSpace = new Tile(new Crawlspace(new Rectangle(GetBounds(x, y).X, GetBounds(x,y).Y, 100, 250), new Rectangle(GetBounds(x, y).X, GetBounds(x, y).Y, 125, 325), crawlSpaceImage));
                    AddtoList(crawlSpace);
                    return crawlSpace;


                default:
                    throw new Exception("Unsupported tile type");
            }
        }

        public void AddtoList(Tile tile)
        {
            objectList.Add(tile.piece);
        }

        public void AddtoMovingList(Tile tile)
        {
            movingList.Add((MovingPlatform)tile.piece);
        }

        #endregion

        #region Draw
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            DrawTile(spriteBatch, gameTime);
        }

        private void DrawTile(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //Console.WriteLine("Screen X: " + Screen.X);
            //Console.WriteLine("Character X: " + player.CollisionBox.X);
            //Console.WriteLine("Character Global X: " + player.GlobalPos);
            //Console.WriteLine("Platform Global X: " + tiles[16, 10].piece.GlobalPos);
            collisions = new Collisions(objectList);

            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    if (tiles[x, y].piece.Texture != null)
                    {
                        if (tiles[x, y].piece is Ground)
                        {
                            if (tiles[x, y].piece.GlobalPos <= Screen.X + 950 && tiles[x, y].piece.GlobalPos >= Screen.X - 150)
                            {
                                if (x != 0)
                                {
                                    if (tiles[x - 1, y].piece is Ground)
                                    {
                                        spriteBatch.Draw(tiles[x, y].piece.Texture, new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y, tiles[x, y].piece.ImageWidth, tiles[x, y].piece.ImageHeight), Color.White);
                                        tiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y + 30, tiles[x, y].piece.CollisionBox.Width, tiles[x, y].piece.CollisionBox.Height);
                                    }
                                    else
                                    {
                                        spriteBatch.Draw(tiles[x, y].piece.Texture, new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y, tiles[x, y].piece.ImageWidth, tiles[x, y].piece.ImageHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                                        tiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y + 30, tiles[x, y].piece.CollisionBox.Width, tiles[x, y].piece.CollisionBox.Height);
                                    }
                                }
                                else
                                {
                                    spriteBatch.Draw(tiles[x, y].piece.Texture, new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y, tiles[x, y].piece.ImageWidth, tiles[x, y].piece.ImageHeight), Color.White);
                                    tiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y + 30, tiles[x, y].piece.CollisionBox.Width, tiles[x, y].piece.CollisionBox.Height);
                                }
                            }
                        }
                        else if (tiles[x, y].piece is Spikes)
                        {
                            if (tiles[x, y].piece.GlobalPos <= Screen.X + 950 && tiles[x, y].piece.GlobalPos >= Screen.X - 150)
                            {
                                spriteBatch.Draw(tiles[x, y].piece.Texture, new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y, tiles[x, y].piece.ImageWidth, tiles[x, y].piece.ImageHeight), Color.Black);
                                tiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y + 15, tiles[x, y].piece.CollisionBox.Width, tiles[x, y].piece.CollisionBox.Height);
                            }
                        }
                        else if (tiles[x, y].piece is MovingPlatform)
                        {

                            MovingPlatform m = (MovingPlatform)tiles[x, y].piece;

                            if (direction)
                            {
                                m.PlatformMove++;

                                if (m.PlatformMove == 150)
                                {
                                    direction = false;
                                    m.PlatformMove--;
                                }
                            }
                            else
                            {
                                m.PlatformMove--;

                                if (m.PlatformMove == 0)
                                {
                                    direction = true;
                                    m.PlatformMove++;
                                }
                            }


                            if (firstLoad)
                            {
                                spriteBatch.Draw(tiles[x, y].piece.Texture, new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y, tiles[x, y].piece.ImageWidth, tiles[x, y].piece.ImageHeight), Color.White);
                            }
                            else if (firstLoad == false)
                            {
                                if (m.HorV) //Horizontal
                                {
                                    spriteBatch.Draw(tiles[x, y].piece.Texture, new Rectangle((GetBounds(x, y).X - Screen.X) - (tiles[x, y].piece.PlatformMove * 2), GetBounds(x, y).Y, tiles[x, y].piece.ImageWidth, tiles[x, y].piece.ImageHeight), Color.White);
                                    tiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X - (tiles[x, y].piece.PlatformMove * 2), GetBounds(x, y).Y + 15, tiles[x, y].piece.CollisionBox.Width, tiles[x, y].piece.CollisionBox.Height);

                                    if (player.CollisionBox.Bottom == tiles[x, y].piece.CollisionBox.Top &&
                                        player.CollisionBox.X + (player.ImageWidth / 2) >= tiles[x, y].piece.CollisionBox.X &&
                                        player.CollisionBox.X + (player.ImageWidth / 2) <= tiles[x, y].piece.CollisionBox.X + tiles[x, y].piece.ImageWidth &&
                                        direction)
                                    {
                                        Screen = new Rectangle(Screen.X - 2, Screen.Y, Screen.Width, Screen.Height);
                                    }
                                    else if (player.CollisionBox.Bottom == tiles[x, y].piece.CollisionBox.Top &&
                                        player.CollisionBox.X + (player.ImageWidth / 2) >= tiles[x, y].piece.CollisionBox.X &&
                                        player.CollisionBox.X + (player.ImageWidth / 2) <= tiles[x, y].piece.CollisionBox.X + tiles[x, y].piece.ImageWidth &&
                                        direction == false)
                                    {
                                        Screen = new Rectangle(Screen.X + 2, Screen.Y, Screen.Width, Screen.Height);
                                    }
                                }
                                else //Vertical
                                {
                                    spriteBatch.Draw(tiles[x, y].piece.Texture, new Rectangle((GetBounds(x, y).X - Screen.X), GetBounds(x, y).Y - (tiles[x, y].piece.PlatformMove * 2), tiles[x, y].piece.ImageWidth, tiles[x, y].piece.ImageHeight), Color.White);
                                    tiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X, (GetBounds(x, y).Y + 15) - (tiles[x, y].piece.PlatformMove * 2), tiles[x, y].piece.CollisionBox.Width, tiles[x, y].piece.CollisionBox.Height);

                                    if (player.CollisionBox.X + (player.ImageWidth / 2) >= tiles[x, y].piece.CollisionBox.X &&
                                        player.CollisionBox.X + (player.ImageWidth / 2) <= tiles[x, y].piece.CollisionBox.X + tiles[x, y].piece.ImageWidth &&
                                        direction)
                                    {
                                        if (player.CollisionBox.Bottom <= (tiles[x, y].piece.CollisionBox.Top + 5) && player.CollisionBox.Bottom >= (tiles[x, y].piece.CollisionBox.Top - 5))
                                        {
                                            player.JumpingUp = false;
                                            collisions.vPlat = true;
                                            player.CollisionBox = new Rectangle(player.CollisionBox.X, player.CollisionBox.Y - 2, player.ImageWidth, player.ImageHeight);
                                        }
                                    }
                                    else if (player.CollisionBox.X + (player.ImageWidth / 2) >= tiles[x, y].piece.CollisionBox.X &&
                                        player.CollisionBox.X + (player.ImageWidth / 2) <= tiles[x, y].piece.CollisionBox.X + tiles[x, y].piece.ImageWidth &&
                                        direction == false)
                                    {
                                        if (player.CollisionBox.Bottom <= (tiles[x, y].piece.CollisionBox.Top + 5) && player.CollisionBox.Bottom >= (tiles[x, y].piece.CollisionBox.Top - 5))
                                        {
                                            collisions.vPlat = true;
                                            player.CollisionBox = new Rectangle(player.CollisionBox.X, player.CollisionBox.Y + 2, player.ImageWidth, player.ImageHeight);
                                        }

                                    }

                                }
                            }

                        }
                        else
                        {
                            if (tiles[x, y].piece.GlobalPos <= Screen.X + 950 && tiles[x, y].piece.GlobalPos >= Screen.X - 150)
                            {
                                spriteBatch.Draw(tiles[x, y].piece.Texture, new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y, tiles[x, y].piece.ImageWidth, tiles[x, y].piece.ImageHeight), Color.White);
                                tiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y + 15, tiles[x, y].piece.CollisionBox.Width, tiles[x, y].piece.CollisionBox.Height);
                            }
                        }
                    }

                }
            }

            firstLoad = false;
        }

        public void DrawBackground(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int y = 0; y < bkgTiles.GetLength(1); y++)
            {
                for (int x = 0; x < bkgTiles.GetLength(0); x++)
                {
                    if (bkgTiles[x, y].piece != null)
                    {
                        if (thisIsMap)
                        {
                            spriteBatch.Draw(bkgTiles[x, y].piece.Texture, new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y, bkgTiles[x, y].piece.ImageWidth, bkgTiles[x, y].piece.ImageHeight), Color.White);
                            bkgTiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y + 15, bkgTiles[x, y].piece.CollisionBox.Width, bkgTiles[x, y].piece.CollisionBox.Height);
                        }

                        spriteBatch.Draw(bkgTiles[x, y].piece.Texture, new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y, bkgTiles[x, y].piece.ImageWidth, bkgTiles[x, y].piece.ImageHeight), Color.White);
                        bkgTiles[x, y].piece.CollisionBox = new Rectangle(GetBounds(x, y).X - Screen.X, GetBounds(x, y).Y + 15, bkgTiles[x, y].piece.CollisionBox.Width, bkgTiles[x, y].piece.CollisionBox.Height);
                    }
                }
            }
        }
        #endregion
    }
}