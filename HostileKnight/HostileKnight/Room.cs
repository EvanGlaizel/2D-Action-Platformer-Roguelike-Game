//A: Evan Glaizel
//F: Room.cs
//P: HostileKnight
//C: 2022/12/4
//M: 
//D: The room of the game. Stores all data relating to the room, and constructs the room

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace HostileKnight
{
    class Room
    {
        //Store the grid size, and amount of grids
        private const int GRID_LENGTH = 60;
        private const int NUM_GRIDS_HORIZONTAL = 21;
        private const int NUM_GRIDS_VERTICAL = 12;

        //Store the enemy scales sizes
        const float VENGEFLY_SCALE = 0.7f;
        const float CRAWLID_SCALE = 0.62f;
        const float GRUZZER_SCALE = 0.75f;
        const float SQUIT_SCALE = 0.7f;
        const float BALDUR_SCALE = 0.65f;
        const float LEAPING_HUSK_SCALE = 0.65f;
        const float ASPID_HUNTER_SCALE = 0.6f;
        const float ASPID_MOTHER_SCALE = 0.6f;
        const float ASPID_HATCHLING_SCALE = 0.6f;
        const float BOSS_SCALE = 0.7f;

        //Store the end door index
        const int END_DOOR = 1;

        //Store all possible exit points
        private enum ExitPoint
        {
            RIGHT_BOTTOM,
            RIGHT_TOP,
            LEFT_BOTTOM,
            LEFT_TOP,
            TOP_LEFT,
            TOP_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_RIGHT
        }

        //Store all possible tile types
        enum TileTypes
        {
            PLATFORM,
            SPIKE,
            ONE_WAY,
            MUD,
            ICE,
            DOOR,
        }

        //Store possible tile locations
        private enum PossibleTiles
        {
            LEFT,
            RIGHT,
            UP,
            DOWN,
            UP_LEFT,
            UP_RIGHT,
            MIDDLE,
            DOWN_LEFT,
            DOWN_RIGHT
        }

        //Store all enemy types
        enum EnemyType
        {
            CRAWLID,
            VENGEFLY,
            GRUZZER,
            BALDUR,
            SQUIT,
            LEAPING_HUSK,
            ASPID_HUNTER,
            ASPID_MOTHER,
            ASPID_HATCHLING,
            BOSS,
            ALL
        }

        //Store the state of the door
        enum DoorState
        {
            OPENING,
            OPEN
        }

        //Store the streamreader to allow for the file to be read and level to be created
        private StreamReader inFile;

        //Store the graphics device
        private GraphicsDevice gd;

        //Store the room name of the room
        private string roomName;

        //Store the difficulty of the room
        private int difficulty;

        //Store the exit point of the room
        private ExitPoint exitPoint;

        //Store the spawn location for the player
        private Vector2 spawnLoc;

        //Store the direction of the doors
        private bool[] verticalDoor = new bool[2];

        //Store the state of the doors
        private bool doorIsClosed = true;

        //Store the tile indexes of the door
        private int[] doorIdx = new int[2];

        //Store the end requirement for the player to reach the next level
        int nextLvlHitbox;

        //Store a list of enemies and obstacles in the room
        private List<Enemy> enemies = new List<Enemy>();
        private List<Tile> tiles = new List<Tile>();

        //Store all possible tile images for the room
        private Texture2D[][] tileImgList;

        //Store all possible enemy images for the room
        private Texture2D[][] enemyImgs;

        //Store all of the enemy health, speeds, and weight
        private int[] enemyHealth = new int[Enum.GetNames(typeof(EnemyType)).Length];
        private int[] enemySpeed = new int[Enum.GetNames(typeof(EnemyType)).Length];
        private double[] enemyWeight = new double[Enum.GetNames(typeof(EnemyType)).Length];

        //Store the iamge of the particle
        private Texture2D particleImg;

        //Store the list of nodes to represent the map
        private PathFindingNode[,] nodeMap = new PathFindingNode[21, 12];

        //Store the room sound effects
        SoundEffect[][] enemySnds;
        SoundEffect[] doorOpenSnd;
        SoundEffect[] particleSnds;

        //Store if the door opening sound has played
        bool doorOpeningSndHasPlayed;

        /*Pre: gd is the graphics deevice that allows the gameline to be created, fileName is the name of the file, difficulty is the difficulty of the level, tileImgList is an array of an array of all tile images,
               enemyAnims is an array of an array of all enemy images, particleImg is the image of the projectile, enemySnds are the sound effects of the enemies, and doorOpenSnd is the sound effect of the door opening,
               and particleSnds are the sound effects of the particle system*/
        //Post: N/A
        //Desc: N/A
        public Room(GraphicsDevice gd, string roomName, int difficulty, Texture2D[][] tileImgList, Texture2D[][] enemyImgs, Texture2D particleImg, SoundEffect[][] enemySnds, SoundEffect[] doorOpenSnd, SoundEffect[] particleSnds)
        {
            //Set the file name, room difficulty, tile images, enemy animations, and particle image
            this.gd = gd;
            this.roomName = roomName;
            this.difficulty = difficulty;
            this.tileImgList = tileImgList;
            this.enemyImgs = enemyImgs;
            this.particleImg = particleImg;
            this.enemySnds = enemySnds;
            this.doorOpenSnd = doorOpenSnd;
            this.particleSnds = particleSnds;

            //Set the enemy health
            enemyHealth[(int)EnemyType.CRAWLID] = 3;
            enemyHealth[(int)EnemyType.VENGEFLY] = 4;
            enemyHealth[(int)EnemyType.GRUZZER] = 4;
            enemyHealth[(int)EnemyType.BALDUR] = 4;
            enemyHealth[(int)EnemyType.SQUIT] = 4;
            enemyHealth[(int)EnemyType.LEAPING_HUSK] = 5;
            enemyHealth[(int)EnemyType.ASPID_HUNTER] = 4;
            enemyHealth[(int)EnemyType.ASPID_MOTHER] = 6;
            enemyHealth[(int)EnemyType.ASPID_HATCHLING] = 1;
            enemyHealth[(int)EnemyType.BOSS] = 50;

            //Set the enemy speeds
            enemySpeed[(int)EnemyType.CRAWLID] = 1;
            enemySpeed[(int)EnemyType.VENGEFLY] = 3;
            enemySpeed[(int)EnemyType.GRUZZER] = 4;
            enemySpeed[(int)EnemyType.BALDUR] = 6;
            enemySpeed[(int)EnemyType.SQUIT] = 4;
            enemySpeed[(int)EnemyType.LEAPING_HUSK] = 1;
            enemySpeed[(int)EnemyType.ASPID_HUNTER] = 1;
            enemySpeed[(int)EnemyType.ASPID_MOTHER] = 1;
            enemySpeed[(int)EnemyType.ASPID_HATCHLING] = 3;
            enemySpeed[(int)EnemyType.BOSS] = 1;

            //Set the enemy weight
            enemyWeight[(int)EnemyType.CRAWLID] = 1;
            enemyWeight[(int)EnemyType.VENGEFLY] = 0.2;
            enemyWeight[(int)EnemyType.GRUZZER] = 1;
            enemyWeight[(int)EnemyType.BALDUR] = 1;
            enemyWeight[(int)EnemyType.SQUIT] = 4;
            enemyWeight[(int)EnemyType.LEAPING_HUSK] = 2;
            enemyWeight[(int)EnemyType.ASPID_HUNTER] = 0.2;
            enemyWeight[(int)EnemyType.ASPID_MOTHER] = 0.2;
            enemyWeight[(int)EnemyType.ASPID_HATCHLING] = 1;
            enemyWeight[(int)EnemyType.BOSS] = 10;

            //Construct the room
            ConstructRoom();

            //Loop through each node row to setup the adjacent nodes
            for (int i = 0; i < nodeMap.GetLength(0); i++)
            {
                //Loop through each node column to setup the adjacent nodes
                for (int j = 0; j < nodeMap.GetLength(1); j++)
                {
                    //Set the adjacent nodes for each node
                    nodeMap[i, j].SetAdjacentNodes(nodeMap);
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Contructs the room by reading in the file, setting all images, and creating the tiles and enemies
        private void ConstructRoom()
        {
            //Store the location of all tiles in the game
            List<List<Vector2>>[] tileLocs = new List<List<Vector2>>[6];

            //Store the image of all tiles in the game
            List<Texture2D>[][] tileImgs = new List<Texture2D>[Enum.GetNames(typeof(TileTypes)).Length][];

            //Store all data related to the tile
            int tileId;
            TileTypes tileType;
            string tileDifficulty;

            //Store the data related to the enemies
            string enemyDifficulty;
            EnemyType enemyType;

            //Store the tiles around each tile, to determine what image to draw on it
            bool[] tileAt = new bool[4];

            //Loop through the array of lists of lists to initialize each list of lists
            for (int i = 0; i < tileLocs.Length; i++)
            {
                //Initialize each List of Lists
                tileLocs[i] = new List<List<Vector2>>();
            }

            //Store the data for each square in the level
            string[] data;

            //Open the file to read whats in the level
            inFile = File.OpenText(roomName);

            //Loop through each grid row of the map
            for (int i = 0; i < NUM_GRIDS_VERTICAL; i++) 
            {
                //Set the value of each tile
                data = inFile.ReadLine().Split(',');

                //Loop through each grid colomn of the map
                for (int j = 0; j < NUM_GRIDS_HORIZONTAL; j++)
                {
                    //Add a new tile or enemy if the tile has data
                    if (Convert.ToInt32(data[j][0].ToString()) != 0)
                    {
                        //Add a new tile or enemy based on the what the file says    //1 for TILE, 2 for ENEMY
                        if (Convert.ToInt32(data[j][0].ToString()) == 1)
                        {
                            //Set the difficulty of the tile
                            tileDifficulty = data[j].Substring(4);

                            //Only add a new tile if its the right difficulty
                            if (tileDifficulty.Contains(difficulty.ToString()))
                            {
                                //Store the ID of the platform
                                tileId = Convert.ToInt32(data[j].Substring(2, 2));

                                //Set the tile type of the platform, and track the number of tiles in the level so far
                                tileType = (TileTypes)Convert.ToInt32(data[j][1].ToString());

                                //define a new list, if the ID of the tile read in is higher then what is already created
                                if (tileLocs[(int)tileType].Count <= tileId)
                                {
                                    //Loop through each new list of tiles needed to add, and create a new list up to the current highest tile ID
                                    for (int k = tileLocs[(int)tileType].Count; k <= tileId; k++)
                                    {
                                        //Add a new platform list to the list
                                        tileLocs[(int)tileType].Add(new List<Vector2>());
                                    }
                                }

                                //Add the location of the platform to the list of platforms for the specified tile
                                tileLocs[(int)tileType][tileId].Add(new Vector2(j * GRID_LENGTH, i * GRID_LENGTH));

                                //Add a new collidable node to the node map
                                nodeMap[j, i] = new PathFindingNode(i, j, true);
                            }
                            else if ((TileTypes)Convert.ToInt32(data[j][1].ToString()) == TileTypes.MUD || (TileTypes)Convert.ToInt32(data[j][1].ToString()) == TileTypes.ICE)
                            {
                                //Store the ID of the platform
                                tileId = Convert.ToInt32(data[j].Substring(2, 2));

                                //Set the tile type of the platform, and track the number of tiles in the level so far (Make sure the tileType is platform
                                tileType = TileTypes.PLATFORM;

                                //define a new list, if the ID of the tile read in is higher then what is already created
                                if (tileLocs[(int)tileType].Count <= tileId)
                                {
                                    //Loop through each new list of tiles needed to add, and create a new list up to the current highest tile ID
                                    for (int k = tileLocs[(int)tileType].Count; k <= tileId; k++)
                                    {
                                        //Add a new platform list to the list
                                        tileLocs[(int)tileType].Add(new List<Vector2>());
                                    }
                                }

                                //Add the location of the platform to the list of platforms for the specified tile
                                tileLocs[(int)tileType][tileId].Add(new Vector2(j * GRID_LENGTH, i * GRID_LENGTH));

                                //Add a new collidable node to the node map
                                nodeMap[j, i] = new PathFindingNode(i, j, true);
                            }
                            else
                            {
                                //Add a new non collidable node to the node map
                                nodeMap[j, i] = new PathFindingNode(i, j, false);
                            }
                        }
                        else if (Convert.ToInt32(data[j][0].ToString()) == 2)
                        {
                            //Set the difficulty of the enemy
                            enemyDifficulty = data[j].Substring(2);

                            //Only add a new tile if its the right difficulty
                            if (enemyDifficulty.Contains(difficulty.ToString()))
                            {
                                //Set the enemy type to add
                                enemyType = (EnemyType)Convert.ToInt32(data[j][1].ToString());

                                //Add a differerent enemy based on the enemy type
                                switch (enemyType)
                                {
                                    case EnemyType.CRAWLID:
                                        //Add a new crawlid
                                        enemies.Add(new Crawlid(gd, enemyImgs[(int)enemyType], particleImg, CRAWLID_SCALE, new Vector2(j * GRID_LENGTH, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(10, 50), new Vector2(5, 25), enemySnds[(int)EnemyType.ALL], particleSnds));
                                        break;
                                    case EnemyType.VENGEFLY:
                                        //Add a new vengefly
                                        enemies.Add(new Vengefly(gd, enemyImgs[(int)enemyType], particleImg, VENGEFLY_SCALE, new Vector2(j * GRID_LENGTH, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(20, 40), new Vector2(50, 50), enemySnds[(int)EnemyType.ALL], particleSnds));
                                        break;
                                    case EnemyType.GRUZZER:
                                        //Add a new gruzzer
                                        enemies.Add(new Gruzzer(gd, enemyImgs[(int)enemyType], particleImg, GRUZZER_SCALE, new Vector2(j * GRID_LENGTH, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(20, 20), new Vector2(45, 45), enemySnds[(int)EnemyType.ALL], particleSnds));
                                        break;
                                    case EnemyType.BALDUR:
                                        //Add a new baldur
                                        enemies.Add(new Baldur(gd, enemyImgs[(int)enemyType], particleImg, BALDUR_SCALE, new Vector2(j * GRID_LENGTH, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(30, 40), new Vector2(60, 40), enemySnds[(int)EnemyType.ALL], enemySnds[(int)enemyType], particleSnds));
                                        break;
                                    case EnemyType.SQUIT:
                                        //Add a new squit
                                        enemies.Add(new Squit(gd, enemyImgs[(int)enemyType], particleImg, SQUIT_SCALE, new Vector2(j * GRID_LENGTH, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(15, 25), new Vector2(50, 50), enemySnds[(int)EnemyType.ALL], enemySnds[(int)enemyType], particleSnds));
                                        break;
                                    case EnemyType.LEAPING_HUSK:
                                        //Add a new leaping husk
                                        enemies.Add(new LeapingHusk(gd, enemyImgs[(int)enemyType], particleImg, LEAPING_HUSK_SCALE, new Vector2(j * GRID_LENGTH, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(38, 30), new Vector2(80, 30), enemySnds[(int)EnemyType.ALL], enemySnds[(int)enemyType], particleSnds));
                                        break;
                                    case EnemyType.ASPID_HUNTER:
                                        //Add a new aspid hunter
                                        enemies.Add(new AspidHunter(gd, enemyImgs[(int)enemyType], particleImg, ASPID_HUNTER_SCALE, new Vector2(j * GRID_LENGTH, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(27, 27), new Vector2(60, 40), enemySnds[(int)EnemyType.ALL], enemySnds[(int)enemyType], particleSnds));
                                        break;
                                    case EnemyType.ASPID_MOTHER:
                                        //Add a new aspid mother
                                        enemies.Add(new AspidMother(gd, enemyImgs[(int)enemyType], particleImg, ASPID_MOTHER_SCALE, new Vector2(j * GRID_LENGTH, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(18, 45), new Vector2(60, 80), enemySnds[(int)EnemyType.ALL], enemySnds[(int)enemyType][0], particleSnds));
                                        break;
                                    case EnemyType.BOSS:
                                        //Add a new boss
                                        enemies.Add(new Boss(gd, enemyImgs[(int)enemyType], particleImg, BOSS_SCALE, new Vector2(j * GRID_LENGTH - 400, i * GRID_LENGTH), enemyHealth[(int)enemyType], enemySpeed[(int)enemyType], enemyWeight[(int)enemyType], new Vector2(520, 360), new Vector2(520, 200), enemySnds[(int)EnemyType.ALL], enemySnds[(int)enemyType], particleSnds));
                                        break;
                                }
                            }

                            //Add a new non colliable node to the node map
                            nodeMap[j, i] = new PathFindingNode(i, j, false);
                        }
                    }
                    else
                    {
                        //Add a new non collidable node to the node map
                        nodeMap[j, i] = new PathFindingNode(i, j, false);
                    }
                }
            }

            //Close the level file
            inFile.Close();

            //Loop through each tile type and determine the image for each tile
            for (int i = 0; i < Enum.GetNames(typeof(TileTypes)).Length; i++)
            {
                //Initialize the image array for the list of platforms
                tileImgs[i] = new List<Texture2D>[tileLocs[i].Count];

                //Determine the image of the tile based on the type of type it is
                switch (i)
                {
                    case (int)TileTypes.PLATFORM:
                    case (int)TileTypes.ICE:
                    case (int)TileTypes.MUD:
                    case (int)TileTypes.SPIKE:
                        //Set the platform, ice, and mud image by looping through each list of list of locations   
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Initialize the list of images
                            tileImgs[i][j] = new List<Texture2D>();

                            //loop through each element to get the tile everything is being compared against
                            for (int k = 0; k < tileLocs[i][j].Count; k++)
                            {
                                //Reset the tiles around the tile
                                tileAt[(int)PossibleTiles.LEFT] = false;
                                tileAt[(int)PossibleTiles.RIGHT] = false;
                                tileAt[(int)PossibleTiles.UP] = false;
                                tileAt[(int)PossibleTiles.DOWN] = false;

                                //Set the tiles next to the tested tile based on its proximity to left of the screen
                                if (tileLocs[i][j][k].X == 0)
                                {
                                    //The tested tile has a tile to its left
                                    tileAt[(int)PossibleTiles.LEFT] = true;
                                }

                                //Set the tiles next to the tested tile based on its proximity to right of the screen
                                if (tileLocs[i][j][k].X == NUM_GRIDS_HORIZONTAL * GRID_LENGTH - GRID_LENGTH)
                                {
                                    //The tested tile has a tile to its right
                                    tileAt[(int)PossibleTiles.RIGHT] = true;
                                }

                                //Set the tiles next to the tested tile based on its proximity to top of the screen
                                if (tileLocs[i][j][k].Y == 0)
                                {
                                    //The tested tile has a tile ontop of it
                                    tileAt[(int)PossibleTiles.UP] = true;
                                }

                                //Set the tiles next to the tested tile based on its proximity to bottom of the screen
                                if (tileLocs[i][j][k].Y == NUM_GRIDS_VERTICAL * GRID_LENGTH - GRID_LENGTH)
                                {
                                    //The tested tile has a tile under of it
                                    tileAt[(int)PossibleTiles.DOWN] = true;
                                }

                                //Loop through each type of tile to test against
                                for (int l = 0; l < Enum.GetNames(typeof(TileTypes)).Length - 1; l++)
                                {
                                    //Loop through each element by first looping through each platform list
                                    for (int m = 0; m < tileLocs[l].Count; m++)
                                    {
                                        //Finally loop through each individual element again, to compare each element against each possible tile location
                                        for (int n = 0; n < tileLocs[l][m].Count; n++)
                                        {
                                            //Set the location of other tiles based on the other tile's location in relation to this one
                                            if (!tileAt[(int)PossibleTiles.LEFT] && tileLocs[i][j][k].Y == tileLocs[l][m][n].Y && tileLocs[i][j][k].X == tileLocs[l][m][n].X + GRID_LENGTH)
                                            {
                                                //Only test ice tiles against other ice tiles and spike tiles with other non spike tiles to test which tiles are near them
                                                if (i == (int)TileTypes.ICE && l == (int)TileTypes.ICE)
                                                {
                                                    //The tested tile has a tile to its left
                                                    tileAt[(int)PossibleTiles.LEFT] = true;
                                                }
                                                else if (i == (int)TileTypes.SPIKE && l != (int)TileTypes.SPIKE)
                                                {
                                                    //The tested tile has a tile to its left
                                                    tileAt[(int)PossibleTiles.LEFT] = true;
                                                }
                                                else if (i != (int)TileTypes.ICE && i != (int)TileTypes.SPIKE)
                                                {
                                                    //The tested tile has a tile to its left
                                                    tileAt[(int)PossibleTiles.LEFT] = true;
                                                }
                                            }
                                            else if (!tileAt[(int)PossibleTiles.RIGHT] && tileLocs[i][j][k].Y == tileLocs[l][m][n].Y && tileLocs[i][j][k].X == tileLocs[l][m][n].X - GRID_LENGTH)
                                            {
                                                //Only test ice tiles against other ice tiles and spike tiles with other non spike tiles to test which tiles are near them
                                                if (i == (int)TileTypes.ICE && l == (int)TileTypes.ICE)
                                                {
                                                    //The tested tile has a tile to its right
                                                    tileAt[(int)PossibleTiles.RIGHT] = true;
                                                }
                                                else if (i == (int)TileTypes.SPIKE && l != (int)TileTypes.SPIKE)
                                                {
                                                    //The tested tile has a tile to its right
                                                    tileAt[(int)PossibleTiles.RIGHT] = true;
                                                }
                                                else if (i != (int)TileTypes.ICE && i != (int)TileTypes.SPIKE)
                                                {
                                                    //The tested tile has a tile to its right
                                                    tileAt[(int)PossibleTiles.RIGHT] = true;
                                                }
                                            }
                                            else if (!tileAt[(int)PossibleTiles.UP] && tileLocs[i][j][k].X == tileLocs[l][m][n].X && tileLocs[i][j][k].Y == tileLocs[l][m][n].Y + GRID_LENGTH)
                                            {
                                                //Only test ice tiles against other ice tiles and spike tiles with other non spike tiles to test which tiles are near them
                                                if (i == (int)TileTypes.ICE && l == (int)TileTypes.ICE)
                                                {
                                                    //The tested tile has a tile ontop of it
                                                    tileAt[(int)PossibleTiles.UP] = true;
                                                }
                                                else if (i == (int)TileTypes.SPIKE && l != (int)TileTypes.SPIKE)
                                                {
                                                    //The tested tile has a tile ontop of it
                                                    tileAt[(int)PossibleTiles.UP] = true;
                                                }
                                                else if (i != (int)TileTypes.ICE && i != (int)TileTypes.SPIKE)
                                                {
                                                    //The tested tile has a tile ontop of it
                                                    tileAt[(int)PossibleTiles.UP] = true;
                                                }
                                            }
                                            else if (!tileAt[(int)PossibleTiles.DOWN] && tileLocs[i][j][k].X == tileLocs[l][m][n].X && tileLocs[i][j][k].Y == tileLocs[l][m][n].Y - GRID_LENGTH)
                                            {
                                                //Only test ice tiles against other ice tiles and spike tiles with other non spike tiles to test which tiles are near them
                                                if (i == (int)TileTypes.ICE && l == (int)TileTypes.ICE)
                                                {
                                                    //The tested tile has a tile under of it
                                                    tileAt[(int)PossibleTiles.DOWN] = true;
                                                }
                                                else if (i == (int)TileTypes.SPIKE && l != (int)TileTypes.SPIKE)
                                                {
                                                    //The tested tile has a tile under of it
                                                    tileAt[(int)PossibleTiles.DOWN] = true;
                                                }
                                                else if (i != (int)TileTypes.ICE && i != (int)TileTypes.SPIKE)
                                                {
                                                    //The tested tile has a tile under of it
                                                    tileAt[(int)PossibleTiles.DOWN] = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                //Set the draw image based on its vertical position in relation to other squares based on the tile type
                                if (i == (int)TileTypes.SPIKE)
                                {
                                    //Set the spike draw image based on its vertical position in relation to other squares
                                    if (tileAt[(int)PossibleTiles.UP])
                                    {
                                        //Set the spike draw image based on its horizontal position in relation to other squares
                                        if (tileAt[(int)PossibleTiles.RIGHT])
                                        {
                                            //Set the image to the bottom left image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.DOWN_LEFT]);
                                        }
                                        else if (tileAt[(int)PossibleTiles.LEFT])
                                        {
                                            //Set the image to the bottom right image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.DOWN_RIGHT]);
                                        }
                                        else
                                        {
                                            //Set the image to the directly down image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.DOWN]);
                                        }
                                    }
                                    else if (tileAt[(int)PossibleTiles.DOWN])
                                    {
                                        //Set the draw image based on its horizontal position in relation to other squares
                                        if (tileAt[(int)PossibleTiles.LEFT])
                                        {
                                            //Set the image to the up right image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.UP_RIGHT]);
                                        }
                                        else if (tileAt[(int)PossibleTiles.RIGHT])
                                        {
                                            //Set the image to the up left image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.UP_LEFT]);
                                        }
                                        else
                                        {
                                            //Set the image to the up image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.UP]);
                                        }
                                    }
                                    else
                                    {
                                        //Set the draw image based on its horizontal position in relation to other squares
                                        if (tileAt[(int)PossibleTiles.LEFT])
                                        {
                                            //Set the image to the right image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.RIGHT]);
                                        }
                                        else
                                        {
                                            //Set the image to the left image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.LEFT]);
                                        }
                                    }
                                }
                                else
                                {
                                    //Set the draw image based on its vertical position in relation to other squares 
                                    if (!tileAt[(int)PossibleTiles.UP])
                                    {
                                        //Set the draw image based on its horizontal position in relation to other squares
                                        if (!tileAt[(int)PossibleTiles.LEFT])
                                        {
                                            //Set the image to the upper left image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.UP_LEFT]);
                                        }
                                        else if (!tileAt[(int)PossibleTiles.RIGHT])
                                        {
                                            //Set the image to the upper right image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.UP_RIGHT]);
                                        }
                                        else
                                        {
                                            //Set the image to the directly up image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.UP]);
                                        }
                                    }
                                    else if (!tileAt[(int)PossibleTiles.DOWN])
                                    {
                                        //Set the draw image based on its horizontal position in relation to other squares
                                        if (!tileAt[(int)PossibleTiles.LEFT])
                                        {
                                            //Set the image to the bottom left image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.DOWN_LEFT]);
                                        }
                                        else if (!tileAt[(int)PossibleTiles.RIGHT])
                                        {
                                            //Set the image to the bottom right image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.DOWN_RIGHT]);
                                        }
                                        else
                                        {
                                            //Set the image to the directly down image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.DOWN]);
                                        }
                                    }
                                    else
                                    {
                                        //Set the draw image based on its horizontal position in relation to other squares
                                        if (!tileAt[(int)PossibleTiles.LEFT])
                                        {
                                            //Set the image to the left image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.LEFT]);
                                        }
                                        else if (!tileAt[(int)PossibleTiles.RIGHT])
                                        {
                                            //Set the image to the right image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.RIGHT]);
                                        }
                                        else
                                        {
                                            //Set the image to the middle image
                                            tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.MIDDLE]);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case (int)TileTypes.DOOR:
                        //Set the door image by looping through each list of list of locations
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Initialize the list of images
                            tileImgs[i][j] = new List<Texture2D>();

                            //Set the image of the door based on its location
                            if (tileLocs[i][j][0].X == 0)
                            {
                                //Set the door to the right
                                tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.RIGHT]);

                                //Set the door to vertical
                                verticalDoor[j] = true;
                            }
                            else if (tileLocs[i][j][0].X == NUM_GRIDS_HORIZONTAL * GRID_LENGTH - GRID_LENGTH)
                            {
                                //Set the door to the left
                                tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.LEFT]);

                                //Set the door to vertical
                                verticalDoor[j] = true;
                            }
                            else if (tileLocs[i][j][0].Y == 0)
                            {
                                //Set the door facing down
                                tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.DOWN]);

                                //Set the door to horizontal
                                verticalDoor[j] = false;
                            }
                            else if (tileLocs[i][j][0].Y == NUM_GRIDS_VERTICAL * GRID_LENGTH - GRID_LENGTH)
                            {
                                //Set the door facing up
                                tileImgs[i][j].Add(tileImgList[i][(int)PossibleTiles.UP]);

                                //Set the door to horizontal
                                verticalDoor[j] = false;
                            }
                        }
                        break;
                    case (int)TileTypes.ONE_WAY:
                        //Set the one way image by looping through each list of list of locations
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Initialize the list of images
                            tileImgs[i][j] = new List<Texture2D>();

                            //loop through each element in the list of images to add each image to the tile images
                            for (int k = 0; k < tileLocs[i][j].Count; k++)
                            {
                                //Set the image of the one way platform
                                tileImgs[i][j].Add(tileImgList[i][0]);
                            }
                        }
                        break;
                }
            }

            //Loop through each tile type and create each tile
            for (int i = 0; i < Enum.GetNames(typeof(TileTypes)).Length; i++)
            {
                //Create a new type based on the type of tile
                switch (i)
                {
                    case (int)TileTypes.PLATFORM:
                        //Create each platform by looping through each list of list of locations      Loops through all tile lists created, to add each one to its own individual platform
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Add a new platform if it has data
                            if (tileLocs[i][j].Count > 0)
                            {
                                //Create each Platform in the platform index of the array of list of lists of vector2s.    Adds the platform list at index "i". 
                                tiles.Add(new Platform(tileLocs[i][j], tileImgs[i][j]));
                            }
                        }
                        break;
                    case (int)TileTypes.SPIKE:
                        //Create each spike by looping through each list of list of locations      Loops through all tile lists created, to add each one to its own individual spike
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Add a new spike if it has data
                            if (tileLocs[i][j].Count > 0)
                            {
                                //Create each Spike in the spike index of the array of list of lists of vector2s.    Adds the spike list at index "i". 
                                tiles.Add(new Spike(tileLocs[i][j], tileImgs[i][j]));
                            }
                        }
                        break;
                    case (int)TileTypes.ONE_WAY:
                        //Create each one way platform by looping through each list of list of locations      Loops through all tile lists created, to add each one to its own individual platform
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Add a new one way platform if it has data
                            if (tileLocs[i][j].Count > 0)
                            {
                                //Create each one way platform in the platform index of the array of list of lists of vector2s.    Adds the platform list at index "i". 
                                tiles.Add(new OneWayPlatform(tileLocs[i][j], tileImgs[i][j]));
                            }
                        }
                        break;
                    case (int)TileTypes.MUD:
                        //Create each mud platform by looping through each list of list of locations      Loops through all tile lists created, to add each one to its own individual platform
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Only add a new ice platform if it has data
                            if (tileLocs[i][j].Count > 0)
                            {
                                //Create each mud platform in the platform index of the array of list of lists of vector2s.    Adds the platform list at index "i". 
                                tiles.Add(new MudPlatform(tileLocs[i][j], tileImgs[i][j]));
                            }
                        }
                        break;
                    case (int)TileTypes.ICE:
                        //Create each ice platform by looping through each list of list of locations      Loops through all tile lists created, to add each one to its own individual platform
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Only add a new mud platform if  it has data
                            if (tileLocs[i][j].Count > 0)
                            {
                                //Create each ice platform in the platform index of the array of list of lists of vector2s.    Adds the platform list at index "i". 
                                tiles.Add(new IcePlatform(tileLocs[i][j], tileImgs[i][j]));
                            }
                        }
                        break;
                    case (int)TileTypes.DOOR:
                        //Create each platform by looping through each list of list of locations      Loops through all tile lists created, to add each one to its own individual platform
                        for (int j = 0; j < tileLocs[i].Count; j++)
                        {
                            //Add a new door if it has data
                            if (tileLocs[i][j].Count > 0)
                            {
                                //Create each Door in the door index of the array of list of lists of vector2s.    Adds the platform list at index "i". 
                                tiles.Add(new Door(tileLocs[i][j], tileImgs[i][j], verticalDoor[j], j == END_DOOR));

                                //Set the location of the doors
                                doorIdx[j] = tiles.Count - 1;

                                //Set the next level hitbox if it is the ending door
                                if (j == END_DOOR)
                                {
                                    //Set the next level hitbox based on its orientation
                                    if (verticalDoor[j])
                                    {
                                        //Set the next level hitbox
                                        nextLvlHitbox = tiles[doorIdx[j]].GetHitBox().Center.X;
                                    }
                                    else
                                    {
                                        //Set the next level hitbox
                                        nextLvlHitbox = tiles[doorIdx[j]].GetHitBox().Center.Y;
                                    }

                                    //Set the exit location of the door
                                    SetExitLoc(tileLocs[i][j][0]);
                                }
                                else
                                {
                                    //Set the start location based on the orientation of the door
                                    if (verticalDoor[j])
                                    {
                                        //Set the start location based the side of the door
                                        if (tileLocs[i][j][0].X == 0)
                                        {
                                            //Set the x spawn location
                                            spawnLoc.X = tileLocs[i][j][0].X + GRID_LENGTH + (GRID_LENGTH / 2);

                                        }
                                        else
                                        {
                                            //Set the x spawn location
                                            spawnLoc.X = tileLocs[i][j][0].X - GRID_LENGTH;
                                        }

                                        //Set the y spawn location
                                        spawnLoc.Y = tileLocs[i][j][1].Y;
                                    }
                                    else
                                    {
                                        //Set the start location based the side of the door
                                        if (tileLocs[i][j][0].Y == 0)
                                        {
                                            //Set the spawn location
                                            spawnLoc.X = tileLocs[i][j][1].X;
                                            spawnLoc.Y = GRID_LENGTH;
                                        }
                                        else
                                        {
                                            //Change the x spawn location based on where the entrance is
                                            if (tileLocs[i][j][0].X < NUM_GRIDS_HORIZONTAL * GRID_LENGTH / 2)
                                            {
                                                //Set the x spawn location
                                                spawnLoc.X = tileLocs[i][j][2].X + GRID_LENGTH + (GRID_LENGTH / 2);
                                            }
                                            else
                                            {
                                                //Set the x spawn location
                                                spawnLoc.X = tileLocs[i][j][0].X - GRID_LENGTH;
                                            }

                                            //Set the y spawn location
                                            spawnLoc.Y = (NUM_GRIDS_VERTICAL * GRID_LENGTH) - (3 * GRID_LENGTH);
                                        }
                                    }

                                    //Loop through each enemy and set its direction
                                    for (int k = 0; k < enemies.Count; k++)
                                    {
                                        //Set each enemies direction
                                        enemies[k].CalcDir(spawnLoc);
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        //Pre: doorLoc is the location of the door
        //Post: N/A
        //Desc: Determines and sets the exit location of the room
        private void SetExitLoc(Vector2 doorLoc)
        {
            //Set the exit point based on the orientation of the door
            if (verticalDoor[1])
            {
                //Set the exit location based the side of the door
                if (doorLoc.X == 0)
                {
                    //Set the exit location based on the location of the door
                    if (doorLoc.Y < NUM_GRIDS_VERTICAL * GRID_LENGTH / 2)
                    {
                        //Set the exit location 
                        exitPoint = ExitPoint.LEFT_TOP;
                    }
                    else
                    {
                        //Set the exit location 
                        exitPoint = ExitPoint.LEFT_BOTTOM;
                    }

                }
                else
                {
                    //Set the exit location based on the location of the door
                    if (doorLoc.Y < NUM_GRIDS_VERTICAL * GRID_LENGTH / 2)
                    {
                        //Set the exit location 
                        exitPoint = ExitPoint.RIGHT_TOP;
                    }
                    else
                    {
                        //Set the exit location 
                        exitPoint = ExitPoint.RIGHT_BOTTOM;
                    }
                }
            }
            else
            {
                //Set the exit location based the side of the door
                if (doorLoc.Y == 0)
                {
                    //Set the exit location based on the location of the door
                    if (doorLoc.X < NUM_GRIDS_HORIZONTAL * GRID_LENGTH / 2)
                    {
                        //Set the exit location 
                        exitPoint = ExitPoint.TOP_LEFT;
                    }
                    else
                    {
                        //Set the exit location 
                        exitPoint = ExitPoint.TOP_RIGHT;
                    }

                }
                else
                {
                    //Set the exit location based on the location of the door
                    if (doorLoc.Y < NUM_GRIDS_VERTICAL * GRID_LENGTH / 2)
                    {
                        //Set the exit location 
                        exitPoint = ExitPoint.BOTTOM_LEFT;
                    }
                    else
                    {
                        //Set the exit location 
                        exitPoint = ExitPoint.BOTTOM_RIGHT;
                    }
                }
            }
        }

        //Pre: playerRect is the hitbox of the player
        //Post: Return true of false based on if the game should go to the next level
        //Desc: Updates the room, and determines if it should transition to the next level
        public bool UpdateRoom(Rectangle playerRect)
        {
            //Bring up the doors and test for the end of the level if there are no enemies left
            if (enemies.Count == 0)
            {
                //If the door is closed, then open it
                if (doorIsClosed)
                {
                    //Loop through each tile to find the doors
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        //Open the door if it is a door and the end door
                        if (tiles[i] is Door)
                        {
                            //Open the door if the door is the end door
                            if (((Door)tiles[i]).IsEndDoor())
                            {
                                //Open the door
                                doorIsClosed = ((Door)tiles[i]).OpenDoor();

                                //Play the door opening sound if it hasn't played before
                                if (!doorOpeningSndHasPlayed)
                                {
                                    //Play the door opening sound
                                    doorOpenSnd[(int)DoorState.OPENING].CreateInstance().Play();
                                    doorOpeningSndHasPlayed = true;
                                }

                                //Play the door open sound if the door has just opened
                                if (!doorIsClosed)
                                {
                                    doorOpenSnd[(int)DoorState.OPEN].CreateInstance().Play();
                                }
                            }
                        }
                    }
                }

                //Bring the player to the next level if they're out of bounds based on the orientation of the end door
                if (verticalDoor[1])
                {
                    //Send the player to the next level if they go past the trigger based on the location of the trigger
                    if (nextLvlHitbox > NUM_GRIDS_HORIZONTAL * GRID_LENGTH / 2)
                    {
                        //Send the player to the next level if they go past the trigger
                        if (playerRect.Center.X > nextLvlHitbox)
                        {
                            //Send the player to the next level
                            return true;
                        }
                    }
                    else
                    {
                        //Send the player to the next level if they go past the trigger
                        if (playerRect.Center.X < nextLvlHitbox)
                        {
                            //Send the player to the next level
                            return true;
                        }
                    }
                }
                else
                {
                    //Send the player to the next level if they go past the trigger based on the location of the trigger
                     if (nextLvlHitbox > NUM_GRIDS_VERTICAL * GRID_LENGTH / 2)
                    {
                        //Send the player to the next level if they go past the trigger
                        if (playerRect.Center.Y > nextLvlHitbox)
                        {
                            //Send the player to the next level
                            return true;
                        }
                    }
                    else
                    {
                        //Send the player to the next level if they go past the trigger
                        if (playerRect.Center.Y < nextLvlHitbox)
                        {
                            //Send the player to the next level
                            return true;
                        }
                    }
                }
            }

            //Return false if the player hasn't entered the next room yet
            return false;
        }

        //Pre: gameTime tracks the time in the game, and playerRect is the rectangle of the player
        //Post: N/A
        //Desc: Update the room
        public void UpdateEnemies(GameTime gameTime, Rectangle playerRect)
        {
            //Loop through each enemy and update them
            for (int i = 0; i < enemies.Count; i++)
            {
                //Update the logic of an enenmy
                enemies[i].Update(gameTime, playerRect);

                //Realculate the line of sight position
                enemies[i].RecalculateLineOfSight(playerRect);

                //Test if there is something in the way of the line of sight
                enemies[i].CalcLineOfSightCollision(tiles);

                //Apply pathfinding to the enemy if they need to pathfind to the player
                if (enemies[i].FindPath())
                {
                    //Pathfind the enemy towards the player
                    enemies[i].CalcPath(PathFindingCalc(enemies[i].GetHitBox(), playerRect));
                }

                //Spawn a hatchling for the aspid mother if one should be spawned
                if (enemies[i] is AspidMother)
                {
                    //Spawn a hatchling if it should be spawned
                    if (((AspidMother)enemies[i]).SpawnHatchling())
                    {
                        //Spawn a hatchling
                        enemies.Add(new AspidHatchling(gd ,enemyImgs[(int)EnemyType.ASPID_HATCHLING], particleImg, ASPID_HATCHLING_SCALE, new Vector2(enemies[i].GetHitBox().Center.X - 20, enemies[i].GetHitBox().Center.Y), enemyHealth[(int)EnemyType.ASPID_HATCHLING], enemySpeed[(int)EnemyType.ASPID_HATCHLING], enemyWeight[(int)EnemyType.ASPID_HATCHLING], new Vector2(5, 15), new Vector2(20, 20), enemySnds[(int)EnemyType.ALL], enemySnds[(int)EnemyType.ASPID_HATCHLING][0], particleSnds));

                        //Stop spawning a hatchling
                        ((AspidMother)enemies[i]).DontSpawnHatchling();
                    }
                }

                //Kill the enemy if they should be killed
                TestEnemyKill(enemies[i]);
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the enemy positions
        public void UpdateEnemyPos()
        {
            //Loop through each enemy and update their game positions
            for (int i = 0; i < enemies.Count; i++)
            {
                //Update the logic of an enenmy
                enemies[i].UpdateGamePos();
            }
        }

        //Pre: testedHitBox is the rectangle the enemy is testing collision against, and testedAttackHitBox is the rectangle of the attack the enemy is testing collision against
        //Post: N/A
        //Desc: Does colission detection between the the enemies in the room, and the player and their attacks
        public void EnemyCollisionDetection(Rectangle testedHitBox, Rectangle testedAttackHitBox)
        {
            //Loop through each enemy, and test colission between them and the player, and players attacks
            for (int i = 0; i < enemies.Count; i++)
            {
                //Loop through each tile, and test collision between each tile and enemy
                for (int j = 0; j < tiles.Count; j++)
                {
                    //Kill the enemy when it collides with the tile based on the tile type
                    if (tiles[j] is Spike)
                    {
                        //Test collision between a tile and an enemy
                        enemies[i].TestCollision(tiles[j].GetHitBox(), true);
                    }
                    else
                    {
                        //Test collision between a tile and an enemy
                        enemies[i].TestCollision(tiles[j].GetHitBox(), false);
                    }
                    
                }

                //Test collision between the player, their attack and the enemies
                enemies[i].TestAttackCollision(testedAttackHitBox, testedHitBox);
            }
        }

        //Pre: enemy is the enemy to test if they should be killed
        //Post: N/A
        //Desc: Tests if each enemy should be killed, and removes them if so
        private void TestEnemyKill(Enemy enemy)
        {
            //Kill the enemy if they should be killed
            if (enemy.KillEnemy())
            {
                //Remove the enemy
                enemies.Remove(enemy);
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Stops all repeatable coming from the enemies
        public void StopAllSounds()
        {
            //Loop through each enemy to find the ones with repeatable sounds
            for (int i = 0; i < enemies.Count; i++)
            {
                //Stop the sounds of each enemy
                enemies[i].StopAllSounds();
            }
        }

        //Pre: startRect is the rectangle of the start path, and endRect is the rectangle of the ending path
        //Post: N/A
        //Desc: Handles platforming for each enemy that uses it
        public List<Vector2> PathFindingCalc(Rectangle startRect, Rectangle endRect) //Path is fine. Start node's parent is being set for some reason
        {
            //Store the lowest fCost
            int lowestFCost;

            //Store the start and end node of the pathfinding
            PathFindingNode start;
            PathFindingNode end;

            //Store the open and close lists for pathfinding
            List<PathFindingNode> open = new List<PathFindingNode>();
            List<PathFindingNode> closed = new List<PathFindingNode>();

            //Store the final path
            List<Vector2> finalPath = new List<Vector2>();

            //Store the currently tested node
            PathFindingNode curNode;

            //Set the start and ending node
            start = nodeMap[startRect.Center.X / GRID_LENGTH, startRect.Center.Y / GRID_LENGTH];
            end = nodeMap[endRect.Center.X / GRID_LENGTH, endRect.Center.Y / GRID_LENGTH];

            //Calculate all H costs of the nodes
            SetHCosts(end);

            //Resets the starting g value and set its new f cost
            start.ResetGCost();
            start.CalcFCost();

            //Add the start node to the open nodes list
            open.Add(start);

            //Keep looping until a path is found, or a path is impossible
            while (open.Count > 0)
            {
                //Set the lowest f cost node
                curNode = open[0];

                //Reset the lowest f cost
                lowestFCost = Int32.MaxValue;

                //Loop through the open node, find the one with the lowest f cost
                for (int i = 0; i < open.Count; i++)
                {
                    //If the tested fValue is the lowest on in the list, set it as the new lowest f cost
                    if (open[i].GetFCost() < lowestFCost)
                    {
                        //Set the new lowest f cost
                        lowestFCost = open[i].GetFCost();
                        curNode = open[i];
                    }
                }

                //Move the node with the lowest f cost to the closed list
                open.Remove(curNode);
                closed.Add(curNode);
                
                //Stop finding the path if the end path was added to the closed path
                if (curNode == end)
                {
                    //Stop pathfinding. A path has been found 
                    break;
                }

                //Loop through each of the current nodes to pathfind to it next
                for (int i = 0; i < curNode.GetAdjacentNodes().Count; i++) 
                {
                    //Only test pathfinding to the adjacent nodes if they're new nodes and not collidable
                    if (!curNode.GetAdjacentNodes()[i].IsCollidable() && !closed.Contains(curNode.GetAdjacentNodes()[i]))
                    {
                        //Calculate and set the new G cost 
                        int newGCost = curNode.GetAdjacentNodes()[i].CalcGCost(curNode);

                        //Compare the cost of the next node based on if it is the open node
                        if (open.Contains(curNode.GetAdjacentNodes()[i]))
                        {
                            //Recalculate the adjacent costs if its new g cost (after being added to this new path) is less than its current g cost (In its old path)
                            if (newGCost < curNode.GetAdjacentNodes()[i].GetGCost())
                            {
                                //Add the adjacent node to the new potential path
                                curNode.GetAdjacentNodes()[i].SetParent(curNode);
                                curNode.GetAdjacentNodes()[i].SetGCost(newGCost);
                                curNode.GetAdjacentNodes()[i].CalcFCost();
                            }
                        }
                        else
                        {             
                            //Set the adjacent nodes parent
                            curNode.GetAdjacentNodes()[i].SetParent(curNode);

                            //Calculate the new costs
                            curNode.GetAdjacentNodes()[i].SetGCost(newGCost);
                            curNode.GetAdjacentNodes()[i].CalcFCost();

                            //Add the adjacent node to the open list for testing
                            open.Add(curNode.GetAdjacentNodes()[i]);
                        }
                    }
                }
            }

            //Set and return the path if a path was found
            if (closed.Contains(end))
            {
                //Set the current node to end node to trace through the path backwards
                PathFindingNode returnNode = end;

                //Keep looping through the end closed list as long as the its not null
                while (returnNode != start)
                {
                    //Add another location to the list of nodes
                    finalPath.Insert(0, new Vector2((returnNode.GetCol() * GRID_LENGTH) + (GRID_LENGTH / 2), (returnNode.GetRow() * GRID_LENGTH) + (GRID_LENGTH / 2)));

                    //Keep going through the path to find the next location
                    returnNode = returnNode.GetParent();
                }
            }

            //Return a final path
            return finalPath;
        }

        //Pre: endNode is the final node in the sequence
        //Post: N/A
        //Desc: Calculates and sets the h cost for each node
        private void SetHCosts(PathFindingNode endNode)
        {
            //Loop through each row to set each rows h cost
            for (int i = 0; i < nodeMap.GetLength(0); i++)
            {
                //Loop through each column to set each column h cost
                for (int j = 0; j < nodeMap.GetLength(1); j++)
                {
                    //Calculate the h and f cost if the node isn't collidable
                    if (!nodeMap[i, j].IsCollidable())
                    {
                        //Set the h and f costs of each node
                        nodeMap[i, j].CalcHCost(endNode);
                        nodeMap[i, j].CalcFCost();
                    }
                }
            }
        }

        //Pre: N/A
        //Post: Return and interger value representing the exit point of the room
        //Desc: Gets and returns the exit point of the room
        public int GetExitPoint()
        {
            //Return the exit point
            return (int)exitPoint;
        }

        //Pre: N/A
        //Post: Return and spawn location of the player
        //Desc: Gets and returns the player spawn location
        public Vector2 GetSpawnLoc()
        {
            //Return the spawn point of the player
            return spawnLoc;
        }

        //Pre: N/A
        //Post: Return the list of enemies in the room
        //Desc: Returns all enemies currently alive in the room
        public List<Enemy> GetEnemies()
        {
            //Return the enemies in the room
            return enemies;
        }

        //Pre: N/A
        //Post: Return the list of tiles in the room
        //Desc: Returns all tiles drawn in the room
        public List<Tile> GetTiles()
        {
            //Return the enemies in the room
            return tiles;
        }

        //Pre: spriteBatch allows sprites and animations to be drawn to the screen, and transparancy is how transparent to draw the room
        //Post: N/A
        //Desc: Draws the room
        public void Draw(SpriteBatch spriteBatch, float transparancy)
        {
            //Draw each door before everything else
            tiles[doorIdx[0]].Draw(spriteBatch, transparancy);
            tiles[doorIdx[1]].Draw(spriteBatch, transparancy);

            //Loop through each tile to draw each one to the screen
            for (int i = 0; i < tiles.Count; i++)
            {
                //Draw the non-door tiles to the screen
                if (i != doorIdx[0] && i != doorIdx[1])
                {
                    //Draw a tile to the screen
                    tiles[i].Draw(spriteBatch, transparancy);
                }
            }

            //Loop through each enemy and draw them
            for (int i = 0; i < enemies.Count; i++)
            {
                //Draw an enemy to the screen
                enemies[i].Draw(spriteBatch, transparancy);
            }
        }
    }
}
