//A: Evan Glaizel
//F: Game1.cs
//P: HostileKnight
//C: 2022/12/1
//M: 2023/01/22
//D: A 2D dungeon crawler game based on the likes of hollow knight. You go through the dungeon fighting enemies and avoiding obstacles in order to fight and beat the final boss. Once the boss is defeated, the gates open, and you unlock a new playable character. Get the fastest time to race to the top of the leaderboard

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Animation2D;
using Helper;

namespace HostileKnight
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Allow files to be read and read from the game
        static StreamReader inFile;
        static StreamWriter outFile;

        //Store a random variable to randomize the room created
        private Random rng = new Random();

        //Store the screen width and height
        int screenWidth;
        int screenHeight;

        //Store the emitter that manages the particles in the game
        Emitter emitter;

        //Store the knights scales sizes
        const float KNIGHT_SCALE = 0.75f;

        //Store the total number of levels
        const int NUM_LEVELS = 15;

        //Store the total amount of leaderboard entries
        const int NUM_LEADERBOARD_ENTRIES = 5;

        //Store the maximum characters in a name
        const int MAX_CHARS_IN_NAME = 3;

        //Store the speed of the screen transitions for the menu and game
        const float SCREEN_TRANSITION_SPEED = 0.04f;
        const float GAME_TRANSITION_SPEED = 0.06f;

        //Store all possible game states
        private enum GameState
        {
            EXIT = -1,
            MENU,
            LEADERBOARD,
            GAME,
            CUTSCENE,
            PAUSE,
            GAME_OVER,
            ENDGAME,
            LEADERBOARD_ENTRY
        }

        //Store the current gameState
        GameState gameState = GameState.MENU;

        //Store all possible player states
        enum PlayerState
        {
            IDLE,
            RUN,
            RISE,
            HOVER,
            FALL,
            HEAL,
            HURT
        }

        //Store all possible attack states
        enum AttackState
        {
            NONE = -1,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        //Store all possible entrance points
        enum EntrancePoint
        {
            LEFT_BOTTOM,
            LEFT_TOP,
            RIGHT_BOTTOM,
            RIGHT_TOP,
            BOTTOM_LEFT,
            BOTTOM_RIGHT,
            TOP_LEFT,
            TOP_RIGHT
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
        enum PossibleTiles
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

        //Store each button
        enum Buttons
        {
            ENDGAME_TO_MENU,
            MENU_TO_GAME,
            MENU_TO_LEADERBOARD,
            MENU_TO_EXIT,
            LEADERBOARD_TO_MENU,
            GAMEOVER_TO_MENU,
            KEYBOARD_TO_MENU,
            PAUSE_RESTART_TO_GAME,
            PAUSE_RESUME_TO_GAME,
            PAUSE_TO_MENU
        }

        //Store a hover and non hover state
        enum BtnState
        {
            NON_HOVER,
            HOVER
        }

        //Store each cutscene type
        enum CutsceneType
        {
            WIN,
            LOSE
        }

        //Store the current and previous mouse state
        MouseState mouse;
        MouseState prevMouse;

        //Store the current and previous keyboard state
        KeyboardState kb;
        KeyboardState prevKb;

        //Store the mouse location, and cursor image
        Texture2D mouseImg;
        Vector2 mouseLoc = new Vector2();
        Vector2 mouseLocBackdrop = new Vector2();

        //Store the image for the blank pixel
        Texture2D blankPixel;

        //Store the knight animations
        Vector2 playerPos = new Vector2(100, 100);
        Texture2D[] knightImgs = new Texture2D[7];
        Texture2D[] knightAttackImgs = new Texture2D[4];
        Animation[] knightAnims = new Animation[7];
        Animation[] knightAttackAnims = new Animation[4];

        //Store the attack animations
        Vector2 attackPos;
        Texture2D[] attackImgs = new Texture2D[4];
        Animation[] attackAnims = new Animation[4];

        //Store the particle image
        Texture2D particleImg;

        //Store the special effects
        Texture2D healImg;
        Texture2D healCompleteImg;
        Texture2D hurtImg;
        Texture2D spiritPickupImg;
        Animation healAnim;
        Animation healCompleteAnim;
        Animation hurtAnim;
        Animation spiritPickupAnim;

        //Store the UI images and animations
        Vector2 maskPos;
        Texture2D maskFullImg;
        Texture2D maskEmptyImg;
        Texture2D maskGainImgs;
        Texture2D maskBreakImgs;
        Texture2D[] soulGuageImgs = new Texture2D[10];

        //Store the enemy images and animations
        Vector2 enemyPos;
        Texture2D[][] enemyImgs = new Texture2D[Enum.GetNames(typeof(EnemyType)).Length][];

        //Store the tile images
        Texture2D[][] tileImgs = new Texture2D[Enum.GetNames(typeof(TileTypes)).Length][];

        //Store the background images
        Texture2D[] backgrounds = new Texture2D[Enum.GetNames(typeof(GameState)).Length];

        //Store the background rectangle
        Rectangle bgRect;

        //Store the background transparancies
        float[] bgTransparancy = new float[Enum.GetNames(typeof(GameState)).Length];

        //Store the player
        Player curPlayer;

        //Store the linked list that manages each room and the current room
        LinkedList llRoom = new LinkedList();
        Node curRoom;

        //Store the current level that the player is on
        int curLvl = 0;
        int lvlPercent;

        //Store the boss room
        Room bossRoom;

        //Store the file names of the levels
        List<string>[] fileNames = new List<string>[8];
        string[] bossFileNames = new string[Enum.GetNames(typeof(EntrancePoint)).Length];

        //Store the difficulty chances of the levels
        int[][] roomDifficultyChances = new int[NUM_LEVELS][]
        {
            new int[] {100,  0,  0,  0,   0},
            new int[] { 90, 10,  0,  0,   0},
            new int[] { 80, 15,  5,  0,   0},
            new int[] { 60, 30, 10,  0,   0},
            new int[] { 40, 40, 20,  0,   0},
            new int[] { 20, 60, 10, 10,   0},
            new int[] { 10, 40, 30, 20,   0},
            new int[] {  0, 30, 40, 30,   0},
            new int[] {  0, 20, 30, 40,  10},
            new int[] {  0, 10, 10, 60,  20},
            new int[] {  0,  0, 20, 40,  40},
            new int[] {  0,  0, 10, 30,  60},
            new int[] {  0,  0,  5, 15,  80},
            new int[] {  0,  0,  0, 10,  90},
            new int[] {  0,  0,  0,  0, 100}
        };

        //Store the menu images
        Texture2D menuTitleImg;
        Texture2D titleCoverTop;
        Texture2D titleCoverBot;
        Texture2D timerImg;

        //Store the fonts in the game
        SpriteFont titleFont;
        SpriteFont resultFont;
        SpriteFont buttonFont;
        SpriteFont insultFont;
        SpriteFont promptFont;
        SpriteFont instFont;

        //Store the menu image locations
        Rectangle menuTitleRect;
        Rectangle titleCoverTopRect;
        Rectangle titleCoverBotRect;
        Rectangle leaderboardTitleCoverTopRect;
        Rectangle leaderboardTitleCoverBotRect;
        Rectangle leaderboardEntryTitleCoverTopRect;
        Rectangle leaderboardEntryTitleCoverBotRect;
        Rectangle timerRect;
        Rectangle hurtImgRect;
        Animation hurtEffectAnim;
        Rectangle[] nameUnderlineRects = new Rectangle[MAX_CHARS_IN_NAME];

        //Store the button image locations
        Vector2[] btnLocs = new Vector2[Enum.GetNames(typeof(Buttons)).Length];
        Vector2[] btnBackdropLocs = new Vector2[Enum.GetNames(typeof(Buttons)).Length];
        Rectangle[] btnBackgroundRects = new Rectangle[Enum.GetNames(typeof(Buttons)).Length];

        //Store the background and boundaries of the leaderboard
        Rectangle leaderboardBackground;
        Rectangle[] leaderboardBoundaries = new Rectangle[6 + (NUM_LEADERBOARD_ENTRIES - 1)];

        //Store the leaderboard locations
        Vector2[] leaderboardNumLocs = new Vector2[NUM_LEADERBOARD_ENTRIES];
        Vector2[] leaderboardNameLocs = new Vector2[NUM_LEADERBOARD_ENTRIES];
        Vector2[] leaderboardTimeLocs = new Vector2[NUM_LEADERBOARD_ENTRIES];
        Vector2[] leaderboardNumBackdropLocs = new Vector2[NUM_LEADERBOARD_ENTRIES];
        Vector2[] leaderboardNameBackdropLocs = new Vector2[NUM_LEADERBOARD_ENTRIES];
        Vector2[] leaderboardTimeBackdropLocs = new Vector2[NUM_LEADERBOARD_ENTRIES];

        //Store the font locations
        Vector2 leaderboardTitleLoc;
        Vector2 leaderboardTitleLocBackdrop;
        Vector2 pausedTitleLoc;
        Vector2 pausedTitleLocBackdrop;
        Vector2 gameoverTitleLoc;
        Vector2 gameoverTitleLocBackdrop;
        Vector2 endgameTitleLoc;
        Vector2 endgameTitleLocBackdrop;
        Vector2[] leaderboardEntryTitleLoc = new Vector2[2];
        Vector2[] leaderboardEntryTitleLocBackdrop = new Vector2[2];
        Vector2 leaderboardEntryPromptLoc;
        Vector2 leaderboardEntryPromptLocBackdrop;
        Vector2 totalTimeLoc;
        Vector2 totalHurtLoc;
        Vector2[] controlsLoc = new Vector2[5];
        Vector2 completionPercentLoc;
        Vector2 soulImgInstLoc;
        Vector2[] soulInstLocs = new Vector2[7];

        //Store if the spirit pickup is active and its transparancy
        bool spiritPickupActive = false;
        float spiritPickupTransparancy = 0f;

        //Store if the user is hovering over the buttons
        bool[] btnHover = new bool[Enum.GetNames(typeof(Buttons)).Length];

        //Store if the button was clicked
        bool[] buttonPressed = new bool[Enum.GetNames(typeof(Buttons)).Length];

        //Store the button draw stats
        float[] btnBackgroundTransparancy = new float[Enum.GetNames(typeof(BtnState)).Length];
        Color[] btnColour = new Color[Enum.GetNames(typeof(BtnState)).Length];

        //Store a bool that stores if the player is dead
        bool playerDead;

        //Store a bool that stores if a room transition is happening
        bool roomTransition;

        //Store a list of all the insults to show the player when they lose the game
        string[][] insults;

        //Store the insult to show the player and its location
        int insultIdx;
        Vector2[] insultsLoc = new Vector2[2];

        //Store the leaderboard data of the game
        double[] leaderboardValue = new double[NUM_LEADERBOARD_ENTRIES];
        string[] leaderboardTime = new string[NUM_LEADERBOARD_ENTRIES];
        string[] leaderboardNames = new string[NUM_LEADERBOARD_ENTRIES];

        //Store the colours to draw each entry in the leaderboard
        Color[] leaderboardColours = new Color[NUM_LEADERBOARD_ENTRIES];

        //Store the colour to draw the leaderboard background
        Color leaderboardBackgroundColour;

        //Store the players name once they finish a run
        string playerName = "";
        Vector2[] playerNameLocs = new Vector2[MAX_CHARS_IN_NAME];
        Vector2[] playerNameLocsBackdrop = new Vector2[MAX_CHARS_IN_NAME];

        //Store a bool that represents if their should be sent to the leaderboard
        bool onLeaderboard;

        //Store the cutscene type
        CutsceneType cutsceneType;

        //Store the time the player is in the cutscene
        Timer cutsceneTimer;

        //Store the total time the player is in the game for
        Timer gameTimer;

        //Store the music for the game
        Song menuTheme;
        Song gameTheme;
        Song bossTheme;
        Song winTheme;
        Song loseTheme;
        
        //Store the sound effects of the game
        SoundEffect[] knightSnds = new SoundEffect[10];
        SoundEffect[][] enemySnds = new SoundEffect[Enum.GetNames(typeof(EnemyType)).Length][];
        SoundEffect[] particleSnds = new SoundEffect[2];
        SoundEffect btnSnd;
        SoundEffect[] doorOpenSnd = new SoundEffect[2];
        SoundEffect doorCloseSnd;
        SoundEffect soulPickupSnd;
        SoundEffect deathSnd;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Set the screen width and height
            graphics.PreferredBackBufferWidth = 1260;
            graphics.PreferredBackBufferHeight = 720;

            //Start the game in full screen
            //graphics.IsFullScreen = true;

            //Applys the screen size changes
            graphics.ApplyChanges();

            //Define the screen width and height
            screenWidth = graphics.GraphicsDevice.Viewport.Width;
            screenHeight = graphics.GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content. 
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Setup the enemy arrays
            enemyImgs[(int)EnemyType.CRAWLID] = new Texture2D[3];
            enemyImgs[(int)EnemyType.VENGEFLY] = new Texture2D[5];
            enemyImgs[(int)EnemyType.GRUZZER] = new Texture2D[3];
            enemyImgs[(int)EnemyType.BALDUR] = new Texture2D[5];
            enemyImgs[(int)EnemyType.SQUIT] = new Texture2D[5];
            enemyImgs[(int)EnemyType.LEAPING_HUSK] = new Texture2D[6];
            enemyImgs[(int)EnemyType.ASPID_HUNTER] = new Texture2D[3];
            enemyImgs[(int)EnemyType.ASPID_MOTHER] = new Texture2D[4];
            enemyImgs[(int)EnemyType.ASPID_HATCHLING] = new Texture2D[3];
            enemyImgs[(int)EnemyType.BOSS] = new Texture2D[13];

            //Setup the tile arrays
            tileImgs[(int)TileTypes.PLATFORM] = new Texture2D[9];
            tileImgs[(int)TileTypes.DOOR] = new Texture2D[4];
            tileImgs[(int)TileTypes.SPIKE] = new Texture2D[9];
            tileImgs[(int)TileTypes.ONE_WAY] = new Texture2D[1];
            tileImgs[(int)TileTypes.MUD] = new Texture2D[9];
            tileImgs[(int)TileTypes.ICE] = new Texture2D[9];

            //Setup the enemy sound arrays
            enemySnds[(int)EnemyType.ALL] = new SoundEffect[2];
            enemySnds[(int)EnemyType.BALDUR] = new SoundEffect[3];
            enemySnds[(int)EnemyType.SQUIT] = new SoundEffect[2];
            enemySnds[(int)EnemyType.LEAPING_HUSK] = new SoundEffect[2];
            enemySnds[(int)EnemyType.ASPID_HUNTER] = new SoundEffect[2];
            enemySnds[(int)EnemyType.ASPID_MOTHER] = new SoundEffect[1];
            enemySnds[(int)EnemyType.ASPID_HATCHLING] = new SoundEffect[1];
            enemySnds[(int)EnemyType.BOSS] = new SoundEffect[7];

            //Load the cursor image
            mouseImg = Content.Load<Texture2D>("Sprites/cursor");
            //Load the blank pixel
            blankPixel = Content.Load<Texture2D>("Sprites/BlankPixel");

            //Load each of the knights spritesheets
            knightImgs[(int)PlayerState.IDLE] = Content.Load<Texture2D>("Animation/Knight/KnightIdle");
            knightImgs[(int)PlayerState.RUN] = Content.Load<Texture2D>("Animation/Knight/KnightRun");
            knightImgs[(int)PlayerState.RISE] = Content.Load<Texture2D>("Animation/Knight/KnightRise");
            knightImgs[(int)PlayerState.HOVER] = Content.Load<Texture2D>("Animation/Knight/KnightHover");
            knightImgs[(int)PlayerState.FALL] = Content.Load<Texture2D>("Animation/Knight/KnightFall");
            knightImgs[(int)PlayerState.HEAL] = Content.Load<Texture2D>("Animation/Knight/KnightHealRescale");
            knightImgs[(int)PlayerState.HURT] = Content.Load<Texture2D>("Animation/Knight/KnightHurt");

            //Load each of the knight attack spritesheets
            knightAttackImgs[(int)AttackState.LEFT] = Content.Load<Texture2D>("Animation/Knight/KnightAttackLeft");
            knightAttackImgs[(int)AttackState.RIGHT] = Content.Load<Texture2D>("Animation/Knight/KnightAttackRight");
            knightAttackImgs[(int)AttackState.UP] = Content.Load<Texture2D>("Animation/Knight/KnightAttackUp");
            knightAttackImgs[(int)AttackState.DOWN] = Content.Load<Texture2D>("Animation/Knight/KnightAttackDown");

            //Load each of the attack spritesheets
            attackImgs[(int)AttackState.LEFT] = Content.Load<Texture2D>("Animation/Nail/NailLeft");
            attackImgs[(int)AttackState.RIGHT] = Content.Load<Texture2D>("Animation/Nail/NailRight");
            attackImgs[(int)AttackState.UP] = Content.Load<Texture2D>("Animation/Nail/NailUp");
            attackImgs[(int)AttackState.DOWN] = Content.Load<Texture2D>("Animation/Nail/NailDown");

            //Load each of the effect spritesheets
            healImg = Content.Load<Texture2D>("Animation/Effects/HealEffect");
            healCompleteImg = Content.Load<Texture2D>("Animation/Effects/HealComplete");
            hurtImg = Content.Load<Texture2D>("Animation/Effects/HitEffect");
            spiritPickupImg = Content.Load<Texture2D>("Animation/Effects/VengefulSpiritPickup");

            //Load the UI images
            maskFullImg = Content.Load<Texture2D>("Sprites/UI/Mask(1)");
            maskEmptyImg = Content.Load<Texture2D>("Sprites/UI/MaskEmpty(1)");
            maskGainImgs = Content.Load<Texture2D>("Animation/UI/MaskGain(1)");
            maskBreakImgs = Content.Load<Texture2D>("Animation/UI/MaskBreak(1)");
            
            //Loop through each soul guage image to load it
            for (int i = 0; i < soulGuageImgs.Length; i++)
            {
                //Load each soul guage image
                soulGuageImgs[i] = Content.Load<Texture2D>("Sprites/UI/SOUL_GUAGE_" + i);
            }

            //Load each of the enemy spritesheets
            enemyImgs[(int)EnemyType.CRAWLID][0] = Content.Load<Texture2D>("Animation/Enemy/Crawlid/CrawlidWalk");
            enemyImgs[(int)EnemyType.CRAWLID][1] = Content.Load<Texture2D>("Animation/Enemy/Crawlid/CrawlidTurn");
            enemyImgs[(int)EnemyType.CRAWLID][2] = Content.Load<Texture2D>("Animation/Enemy/Crawlid/CrawlidDeath");
            enemyImgs[(int)EnemyType.VENGEFLY][0] = Content.Load<Texture2D>("Animation/Enemy/Vengefly/VengeflyIdle");
            enemyImgs[(int)EnemyType.VENGEFLY][1] = Content.Load<Texture2D>("Animation/Enemy/Vengefly/VengeflyAgro");
            enemyImgs[(int)EnemyType.VENGEFLY][2] = Content.Load<Texture2D>("Animation/Enemy/Vengefly/VengeflyChase");
            enemyImgs[(int)EnemyType.VENGEFLY][3] = Content.Load<Texture2D>("Animation/Enemy/Vengefly/VengeflyTurn");
            enemyImgs[(int)EnemyType.VENGEFLY][4] = Content.Load<Texture2D>("Animation/Enemy/Vengefly/VengeflyDie");
            enemyImgs[(int)EnemyType.GRUZZER][0] = Content.Load<Texture2D>("Animation/Enemy/Gruzzer/GruzzerFly");
            enemyImgs[(int)EnemyType.GRUZZER][1] = Content.Load<Texture2D>("Animation/Enemy/Gruzzer/GruzzerDieAir");
            enemyImgs[(int)EnemyType.GRUZZER][2] = Content.Load<Texture2D>("Animation/Enemy/Gruzzer/GruzzerDieGround");
            enemyImgs[(int)EnemyType.BALDUR][0] = Content.Load<Texture2D>("Animation/Enemy/Baldur/BaldurWalk");
            enemyImgs[(int)EnemyType.BALDUR][1] = Content.Load<Texture2D>("Animation/Enemy/Baldur/BaldurRollStart");
            enemyImgs[(int)EnemyType.BALDUR][2] = Content.Load<Texture2D>("Animation/Enemy/Baldur/BaldurRoll");
            enemyImgs[(int)EnemyType.BALDUR][3] = Content.Load<Texture2D>("Animation/Enemy/Baldur/BaldurWalkTransition");
            enemyImgs[(int)EnemyType.BALDUR][4] = Content.Load<Texture2D>("Animation/Enemy/Baldur/BaldurDeath");
            enemyImgs[(int)EnemyType.SQUIT][0] = Content.Load<Texture2D>("Animation/Enemy/Squit/SquitIdle");
            enemyImgs[(int)EnemyType.SQUIT][1] = Content.Load<Texture2D>("Animation/Enemy/Squit/SquitAttackWindup");
            enemyImgs[(int)EnemyType.SQUIT][2] = Content.Load<Texture2D>("Animation/Enemy/Squit/SquitAttack");
            enemyImgs[(int)EnemyType.SQUIT][3] = Content.Load<Texture2D>("Animation/Enemy/Squit/SquitTurn");
            enemyImgs[(int)EnemyType.SQUIT][4] = Content.Load<Texture2D>("Animation/Enemy/Squit/SquitDeath");
            enemyImgs[(int)EnemyType.LEAPING_HUSK][0] = Content.Load<Texture2D>("Animation/Enemy/LeapingHusk/LeapingHuskWalk");
            enemyImgs[(int)EnemyType.LEAPING_HUSK][1] = Content.Load<Texture2D>("Animation/Enemy/LeapingHusk/LeapingHuskJump");
            enemyImgs[(int)EnemyType.LEAPING_HUSK][2] = Content.Load<Texture2D>("Animation/Enemy/LeapingHusk/LeapingHuskAttack");
            enemyImgs[(int)EnemyType.LEAPING_HUSK][3] = Content.Load<Texture2D>("Animation/Enemy/LeapingHusk/LeapingHuskTurn");
            enemyImgs[(int)EnemyType.LEAPING_HUSK][4] = Content.Load<Texture2D>("Animation/Enemy/LeapingHusk/LeapingHuskAirDeath");
            enemyImgs[(int)EnemyType.LEAPING_HUSK][5] = Content.Load<Texture2D>("Animation/Enemy/LeapingHusk/LeapingHuskGroundDeath");
            enemyImgs[(int)EnemyType.ASPID_HUNTER][0] = Content.Load<Texture2D>("Animation/Enemy/AspidHunter/AspidHunterFly");
            enemyImgs[(int)EnemyType.ASPID_HUNTER][1] = Content.Load<Texture2D>("Animation/Enemy/AspidHunter/AspidHunterShoot");
            enemyImgs[(int)EnemyType.ASPID_HUNTER][2] = Content.Load<Texture2D>("Animation/Enemy/AspidHunter/AspidHunterDeath");
            enemyImgs[(int)EnemyType.ASPID_MOTHER][0] = Content.Load<Texture2D>("Animation/Enemy/AspidMother/AspidMotherIdle");
            enemyImgs[(int)EnemyType.ASPID_MOTHER][1] = Content.Load<Texture2D>("Animation/Enemy/AspidMother/AspidMotherShoot");
            enemyImgs[(int)EnemyType.ASPID_MOTHER][2] = Content.Load<Texture2D>("Animation/Enemy/AspidMother/AspidMotherDeathAir");
            enemyImgs[(int)EnemyType.ASPID_MOTHER][3] = Content.Load<Texture2D>("Animation/Enemy/AspidMother/AspidMotherDeathGround");
            enemyImgs[(int)EnemyType.ASPID_HATCHLING][0] = Content.Load<Texture2D>("Animation/Enemy/AspidHatchling/AspidHatchlingFly");
            enemyImgs[(int)EnemyType.ASPID_HATCHLING][1] = Content.Load<Texture2D>("Animation/Enemy/AspidHatchling/AspidHatchlingTurn");
            enemyImgs[(int)EnemyType.ASPID_HATCHLING][2] = Content.Load<Texture2D>("Animation/Enemy/AspidHatchling/AspidHatchlingDeath");
            enemyImgs[(int)EnemyType.BOSS][0] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossIdle");
            enemyImgs[(int)EnemyType.BOSS][1] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossJumpAnticipate");
            enemyImgs[(int)EnemyType.BOSS][2] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossJump");
            enemyImgs[(int)EnemyType.BOSS][3] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossJumpAttack");
            enemyImgs[(int)EnemyType.BOSS][4] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossJumpAttackLand");
            enemyImgs[(int)EnemyType.BOSS][5] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossJumpAttackRebound");
            enemyImgs[(int)EnemyType.BOSS][6] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossAttackWindup");
            enemyImgs[(int)EnemyType.BOSS][7] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossAttack");
            enemyImgs[(int)EnemyType.BOSS][8] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossAttackRebound");
            enemyImgs[(int)EnemyType.BOSS][9] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossTurn");
            enemyImgs[(int)EnemyType.BOSS][10] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossKnockback");
            enemyImgs[(int)EnemyType.BOSS][11] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossExpose");
            enemyImgs[(int)EnemyType.BOSS][12] = Content.Load<Texture2D>("Animation/Enemy/Boss/BossVunerable");

            //Set the projectile image
            particleImg = Content.Load<Texture2D>("Animation/Effects/SoulOrb");

            //Load each of the tile images
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.UP_LEFT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_TOP_LEFT");
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.UP] = Content.Load<Texture2D>("Sprites/Platform/GROUND_TOP");
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.UP_RIGHT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_TOP_RIGHT");
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.LEFT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_LEFT");
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.MIDDLE] = Content.Load<Texture2D>("Sprites/Platform/GROUND_MIDDLE");
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.RIGHT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_RIGHT");
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.DOWN_LEFT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_BOTTOM_LEFT");
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.DOWN] = Content.Load<Texture2D>("Sprites/Platform/GROUND_BOTTOM");
            tileImgs[(int)TileTypes.PLATFORM][(int)PossibleTiles.DOWN_RIGHT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_BOTTOM_RIGHT");
            tileImgs[(int)TileTypes.DOOR][(int)PossibleTiles.UP] = Content.Load<Texture2D>("Sprites/Platform/DOOR_UP");
            tileImgs[(int)TileTypes.DOOR][(int)PossibleTiles.DOWN] = Content.Load<Texture2D>("Sprites/Platform/DOOR_DOWN");
            tileImgs[(int)TileTypes.DOOR][(int)PossibleTiles.LEFT] = Content.Load<Texture2D>("Sprites/Platform/Door_LEFT");
            tileImgs[(int)TileTypes.DOOR][(int)PossibleTiles.RIGHT] = Content.Load<Texture2D>("Sprites/Platform/Door_RIGHT");
            tileImgs[(int)TileTypes.SPIKE][(int)PossibleTiles.UP_LEFT] = Content.Load<Texture2D>("Sprites/Platform/Spikes_TOP_LEFT");
            tileImgs[(int)TileTypes.SPIKE][(int)PossibleTiles.UP] = Content.Load<Texture2D>("Sprites/Platform/Spikes_UP");
            tileImgs[(int)TileTypes.SPIKE][(int)PossibleTiles.UP_RIGHT] = Content.Load<Texture2D>("Sprites/Platform/Spikes_TOP_RIGHT");
            tileImgs[(int)TileTypes.SPIKE][(int)PossibleTiles.LEFT] = Content.Load<Texture2D>("Sprites/Platform/Spikes_LEFT");
            tileImgs[(int)TileTypes.SPIKE][(int)PossibleTiles.RIGHT] = Content.Load<Texture2D>("Sprites/Platform/Spikes_RIGHT");
            tileImgs[(int)TileTypes.SPIKE][(int)PossibleTiles.DOWN_LEFT] = Content.Load<Texture2D>("Sprites/Platform/Spikes_DOWN_LEFT");
            tileImgs[(int)TileTypes.SPIKE][(int)PossibleTiles.DOWN] = Content.Load<Texture2D>("Sprites/Platform/Spikes_DOWN");
            tileImgs[(int)TileTypes.SPIKE][(int)PossibleTiles.DOWN_RIGHT] = Content.Load<Texture2D>("Sprites/Platform/SPIKES_DOWN_RIGHT");
            tileImgs[(int)TileTypes.ONE_WAY][0] = Content.Load<Texture2D>("Sprites/Platform/OneWayPlat1");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.UP_LEFT] = Content.Load<Texture2D>("Sprites/Platform/MUD_TOP_LEFT");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.UP] = Content.Load<Texture2D>("Sprites/Platform/MUD_TOP");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.UP_RIGHT] = Content.Load<Texture2D>("Sprites/Platform/MUD_TOP_RIGHT");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.LEFT] = Content.Load<Texture2D>("Sprites/Platform/MUD_LEFT");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.MIDDLE] = Content.Load<Texture2D>("Sprites/Platform/MUD_MIDDLE");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.RIGHT] = Content.Load<Texture2D>("Sprites/Platform/MUD_RIGHT");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.DOWN_LEFT] = Content.Load<Texture2D>("Sprites/Platform/MUD_BOTTOM_LEFT");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.DOWN] = Content.Load<Texture2D>("Sprites/Platform/MUD_BOTTOM");
            tileImgs[(int)TileTypes.MUD][(int)PossibleTiles.DOWN_RIGHT] = Content.Load<Texture2D>("Sprites/Platform/MUD_BOTTOM_RIGHT");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.UP_LEFT] = Content.Load<Texture2D>("Sprites/Platform/ICE_TOP_LEFT");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.UP] = Content.Load<Texture2D>("Sprites/Platform/ICE_TOP");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.UP_RIGHT] = Content.Load<Texture2D>("Sprites/Platform/ICE_TOP_RIGHT");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.LEFT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_LEFT");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.MIDDLE] = Content.Load<Texture2D>("Sprites/Platform/GROUND_MIDDLE");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.RIGHT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_RIGHT");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.DOWN_LEFT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_BOTTOM_LEFT");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.DOWN] = Content.Load<Texture2D>("Sprites/Platform/GROUND_BOTTOM");
            tileImgs[(int)TileTypes.ICE][(int)PossibleTiles.DOWN_RIGHT] = Content.Load<Texture2D>("Sprites/Platform/GROUND_BOTTOM_RIGHT");

            //Load the game fonts
            titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
            resultFont = Content.Load<SpriteFont>("Fonts/ResultsFont");
            buttonFont = Content.Load<SpriteFont>("Fonts/ButtonFont");
            insultFont = Content.Load<SpriteFont>("Fonts/InsultFont");
            promptFont = Content.Load<SpriteFont>("Fonts/PromptFont");
            instFont = Content.Load<SpriteFont>("Fonts/InstFont");

            //Load the game backgrounds
            backgrounds[(int)GameState.MENU] = Content.Load<Texture2D>("Sprites/Backgrounds/MenuBackground");
            backgrounds[(int)GameState.LEADERBOARD] = Content.Load<Texture2D>("Sprites/Backgrounds/LeaderboardBackground");
            backgrounds[(int)GameState.GAME] = Content.Load<Texture2D>("Sprites/Backgrounds/PotentialGameBg");
            backgrounds[(int)GameState.PAUSE] = backgrounds[(int)GameState.GAME];
            backgrounds[(int)GameState.GAME_OVER] = Content.Load<Texture2D>("Sprites/Backgrounds/GameOverBackground");
            backgrounds[(int)GameState.ENDGAME] = Content.Load<Texture2D>("Sprites/Backgrounds/EndgameBG");
            backgrounds[(int)GameState.LEADERBOARD_ENTRY] = Content.Load<Texture2D>("Sprites/Backgrounds/KeyboardEntryBackground");

            //Load the menu images
            menuTitleImg = Content.Load<Texture2D>("Sprites/Menu/MenuTitle");
            titleCoverTop = Content.Load<Texture2D>("Sprites/Menu/TitleCoverTop");
            titleCoverBot = Content.Load<Texture2D>("Sprites/Menu/TitleCoverBot");
            timerImg = Content.Load<Texture2D>("Sprites/Menu/stopwatch");

            //Load the songs
            menuTheme = Content.Load<Song>("Sounds/Music/MenuTheme");
            gameTheme = Content.Load<Song>("Sounds/Music/GameTheme2");
            bossTheme = Content.Load<Song>("Sounds/Music/BossTheme");
            winTheme = Content.Load<Song>("Sounds/Music/WinTheme");
            loseTheme = Content.Load<Song>("Sounds/Music/LoseTheme");

            //Load the sound effects
            knightSnds[0] = Content.Load<SoundEffect>("Sounds/SoundEffects/hero_run_footsteps_stone");
            knightSnds[1] = Content.Load<SoundEffect>("Sounds/SoundEffects/hero_jump");
            knightSnds[2] = Content.Load<SoundEffect>("Sounds/SoundEffects/hero_falling");
            knightSnds[3] = Content.Load<SoundEffect>("Sounds/SoundEffects/hero_land_soft");
            knightSnds[4] = Content.Load<SoundEffect>("Sounds/SoundEffects/hero_land_hard");
            knightSnds[5] = Content.Load<SoundEffect>("Sounds/SoundEffects/hero_damage(1)");
            knightSnds[6] = Content.Load<SoundEffect>("Sounds/SoundEffects/focus_health_charging");
            knightSnds[7] = Content.Load<SoundEffect>("Sounds/SoundEffects/focus_health_heal");
            knightSnds[8] = Content.Load<SoundEffect>("Sounds/SoundEffects/sword_1");
            knightSnds[9] = Content.Load<SoundEffect>("Sounds/SoundEffects/sword_spike_hit");
            enemySnds[(int)EnemyType.ALL][0] = Content.Load<SoundEffect>("Sounds/SoundEffects/enemy_damage");
            enemySnds[(int)EnemyType.ALL][1] = Content.Load<SoundEffect>("Sounds/SoundEffects/enemy_death_sword");
            enemySnds[(int)EnemyType.BALDUR][0] = Content.Load<SoundEffect>("Sounds/SoundEffects/roller_curl");
            enemySnds[(int)EnemyType.BALDUR][1] = Content.Load<SoundEffect>("Sounds/SoundEffects/roller_rolling");
            enemySnds[(int)EnemyType.BALDUR][2] = Content.Load<SoundEffect>("Sounds/SoundEffects/roller_hit_wall");
            enemySnds[(int)EnemyType.SQUIT][0] = Content.Load<SoundEffect>("Sounds/SoundEffects/mosquito_charge_charge");
            enemySnds[(int)EnemyType.SQUIT][1] = Content.Load<SoundEffect>("Sounds/SoundEffects/mosquito_wall_hit");
            enemySnds[(int)EnemyType.ASPID_HUNTER][0] = Content.Load<SoundEffect>("Sounds/SoundEffects/mushroom_brawler_projectile_hit");
            enemySnds[(int)EnemyType.ASPID_HUNTER][1] = Content.Load<SoundEffect>("Sounds/SoundEffects/projectile_fire");
            enemySnds[(int)EnemyType.ASPID_MOTHER][0] = Content.Load<SoundEffect>("Sounds/SoundEffects/hatcher_give_birth");
            enemySnds[(int)EnemyType.ASPID_HATCHLING][0] = Content.Load<SoundEffect>("Sounds/SoundEffects/hatchling_explode");
            enemySnds[(int)EnemyType.BOSS][0] = Content.Load<SoundEffect>("Sounds/SoundEffects/false_knight_jump");
            enemySnds[(int)EnemyType.BOSS][1] = Content.Load<SoundEffect>("Sounds/SoundEffects/false_knight_land");
            enemySnds[(int)EnemyType.BOSS][2] = Content.Load<SoundEffect>("Sounds/SoundEffects/false_knight_strike_ground");
            enemySnds[(int)EnemyType.BOSS][3] = Content.Load<SoundEffect>("Sounds/SoundEffects/false_knight_swing");
            enemySnds[(int)EnemyType.BOSS][4] = Content.Load<SoundEffect>("Sounds/SoundEffects/false_knight_damage_armour");
            enemySnds[(int)EnemyType.BOSS][5] = Content.Load<SoundEffect>("Sounds/SoundEffects/boss_stun");
            enemySnds[(int)EnemyType.BOSS][6] = Content.Load<SoundEffect>("Sounds/SoundEffects/boss_explode_clean");
            enemySnds[(int)EnemyType.LEAPING_HUSK][0] = enemySnds[(int)EnemyType.BOSS][0];
            enemySnds[(int)EnemyType.LEAPING_HUSK][1] = enemySnds[(int)EnemyType.BOSS][3];
            particleSnds[0] = Content.Load<SoundEffect>("Sounds/SoundEffects/soul_totem_slash");
            particleSnds[1] = Content.Load<SoundEffect>("Sounds/SoundEffects/soul_pickup_1");
            btnSnd = Content.Load<SoundEffect>("Sounds/SoundEffects/ui_button_confirm");
            doorOpenSnd[0] = Content.Load<SoundEffect>("Sounds/SoundEffects/Door_Open_PT_1");
            doorOpenSnd[1] = Content.Load<SoundEffect>("Sounds/SoundEffects/Door_Open_PT_2");
            doorCloseSnd = doorOpenSnd[1];
            soulPickupSnd = Content.Load<SoundEffect>("Sounds/SoundEffects/spell_pickup_pickup");
            deathSnd = Content.Load<SoundEffect>("Sounds/SoundEffects/hero_death_extra_details");

            //Create the backround rectangles
            bgRect = new Rectangle(0, 0, screenWidth, screenHeight);

            //Set each of the knights animations
            knightAnims[(int)PlayerState.IDLE] = new Animation(knightImgs[(int)PlayerState.IDLE], 1, 1, 1, 0, 0, 0, 1, playerPos, KNIGHT_SCALE, false);
            knightAnims[(int)PlayerState.RUN] = new Animation(knightImgs[(int)PlayerState.RUN], 2, 4, 8, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 7, playerPos, KNIGHT_SCALE, true);
            knightAnims[(int)PlayerState.RISE] = new Animation(knightImgs[(int)PlayerState.RISE], 2, 2, 4, 0, 4, Animation.ANIMATE_ONCE, 8, playerPos, KNIGHT_SCALE, true);
            knightAnims[(int)PlayerState.HOVER] = new Animation(knightImgs[(int)PlayerState.HOVER], 1, 2, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 5, playerPos, KNIGHT_SCALE, true);
            knightAnims[(int)PlayerState.FALL] = new Animation(knightImgs[(int)PlayerState.FALL], 1, 2, 2, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 5, playerPos, KNIGHT_SCALE, true);
            knightAnims[(int)PlayerState.HEAL] = new Animation(knightImgs[(int)PlayerState.HEAL], 2, 2, 4, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 7, playerPos, KNIGHT_SCALE, true);
            knightAnims[(int)PlayerState.HURT] = new Animation(knightImgs[(int)PlayerState.HURT], 1, 1, 1, 0, 0, 0, 1, playerPos, KNIGHT_SCALE, false);

            //Set each of the knight attack and attack animations by looping through them
            for (int i = 0; i < attackAnims.Length; i++)
            {
                //Set the knight attack animations
                knightAttackAnims[i] = new Animation(knightAttackImgs[i], 2, 4, 8, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 2, playerPos, KNIGHT_SCALE, true);

                //Set the attack animation
                attackAnims[i] = new Animation(attackImgs[i], 8, 1, 8, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 2, attackPos, 1f, false);
            }

            //Set the animations for all the effects
            healAnim = new Animation(healImg, 7, 1, 7, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 3, playerPos, 1f, true);
            healCompleteAnim = new Animation(healCompleteImg, 6, 1, 6, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 3, playerPos, 4f, false);
            hurtAnim = new Animation(hurtImg, 7, 1, 7, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 3, playerPos, 1f, false);
            spiritPickupAnim = new Animation(spiritPickupImg, 5, 1, 5, 0, Animation.NO_IDLE, Animation.ANIMATE_FOREVER, 6, new Vector2((screenWidth / 2) - ((spiritPickupImg.Width / 5) / 2), (screenHeight / 2) - (spiritPickupImg.Height / 2) - 30), 1f, true);

            //Create the player
            curPlayer = new Player(knightAnims, knightAttackAnims, attackAnims, healAnim, healCompleteAnim, hurtAnim, maskFullImg, maskEmptyImg, maskBreakImgs, maskGainImgs, soulGuageImgs, 5, new Vector2(5, 17), knightSnds);

            //Create the emitter
            emitter = new Emitter(particleImg, particleSnds[0]);

            //Set the menu image locations
            menuTitleRect = new Rectangle((screenWidth / 2) - (menuTitleImg.Width / 3), -10, (int)(menuTitleImg.Width / 1.5), (int)(menuTitleImg.Height / 1.5));
            titleCoverTopRect = new Rectangle((screenWidth / 2) - (titleCoverTop.Width / 2), 10, titleCoverTop.Width, titleCoverTop.Height);
            titleCoverBotRect = new Rectangle((screenWidth / 2) - (titleCoverBot.Width / 2), 230, titleCoverBot.Width, titleCoverBot.Height);
            leaderboardTitleCoverTopRect = new Rectangle((screenWidth / 2) - (titleCoverTop.Width / 2), 0, titleCoverTop.Width, titleCoverTop.Height);
            leaderboardTitleCoverBotRect = new Rectangle((screenWidth / 2) - (titleCoverBot.Width / 2), 220, titleCoverBot.Width, titleCoverBot.Height);
            leaderboardEntryTitleCoverTopRect = new Rectangle((screenWidth / 2) - (titleCoverTop.Width / 2), 0, titleCoverTop.Width, titleCoverTop.Height);
            leaderboardEntryTitleCoverBotRect = new Rectangle((screenWidth / 2) - (titleCoverBot.Width / 2), 200, titleCoverBot.Width, titleCoverBot.Height);
            timerRect = new Rectangle(200, 280, (int)(timerImg.Width / 3.5), (int)(timerImg.Height / 3.5));
            hurtImgRect = new Rectangle(180, 430, knightImgs[(int)PlayerState.HURT].Width, knightImgs[(int)PlayerState.HURT].Height);
            hurtEffectAnim = new Animation(hurtImg, 7, 1, 7, 0, 0, Animation.ANIMATE_ONCE, 3, new Vector2(-60, 430), 1f, false);

            //Loop through each key underline rectangle, and set its position
            for (int i = 0; i < MAX_CHARS_IN_NAME; i++)
            {
                //Set the underline rectangle for the players name
                nameUnderlineRects[i] = new Rectangle(450 + (120 * i), 530, 100, 20);

                //Set the location to draw the players name
                playerNameLocs[i] = new Vector2(440 + (120 * i), 400);
                playerNameLocsBackdrop[i] = new Vector2(443 + (120 * i), 403);
            }

            //Set the button locations
            btnLocs[(int)Buttons.ENDGAME_TO_MENU] = new Vector2(250, 605);
            btnBackdropLocs[(int)Buttons.ENDGAME_TO_MENU] = new Vector2(253, 608);
            btnBackgroundRects[(int)Buttons.ENDGAME_TO_MENU] = new Rectangle((screenWidth / 2) - (800 / 2), 580, 800, 120);
            btnLocs[(int)Buttons.MENU_TO_GAME] = new Vector2(370, 275);
            btnBackdropLocs[(int)Buttons.MENU_TO_GAME] = new Vector2(373, 278);
            btnBackgroundRects[(int)Buttons.MENU_TO_GAME] = new Rectangle((screenWidth / 2) - (540 / 2), 250, 540, 120);
            btnLocs[(int)Buttons.MENU_TO_LEADERBOARD] = new Vector2(336, 425);
            btnBackdropLocs[(int)Buttons.MENU_TO_LEADERBOARD] = new Vector2(339, 428);
            btnBackgroundRects[(int)Buttons.MENU_TO_LEADERBOARD] = new Rectangle((screenWidth / 2) - (620 / 2), 400, 620, 120);
            btnLocs[(int)Buttons.MENU_TO_EXIT] = new Vector2(536, 575);
            btnBackdropLocs[(int)Buttons.MENU_TO_EXIT] = new Vector2(539, 578);
            btnBackgroundRects[(int)Buttons.MENU_TO_EXIT] = new Rectangle((screenWidth / 2) - (220 / 2), 550, 220, 120);
            btnLocs[(int)Buttons.GAMEOVER_TO_MENU] = new Vector2(250, 605);
            btnBackdropLocs[(int)Buttons.GAMEOVER_TO_MENU] = new Vector2(253, 608);
            btnBackgroundRects[(int)Buttons.GAMEOVER_TO_MENU] = new Rectangle((screenWidth / 2) - (800 / 2), 580, 800, 120);
            btnLocs[(int)Buttons.LEADERBOARD_TO_MENU] = new Vector2(250, 605);
            btnBackdropLocs[(int)Buttons.LEADERBOARD_TO_MENU] = new Vector2(253, 608);
            btnBackgroundRects[(int)Buttons.LEADERBOARD_TO_MENU] = new Rectangle((screenWidth / 2) - (800 / 2), 580, 800, 120);
            btnLocs[(int)Buttons.KEYBOARD_TO_MENU] = new Vector2(250, 605);
            btnBackdropLocs[(int)Buttons.KEYBOARD_TO_MENU] = new Vector2(253, 608);
            btnBackgroundRects[(int)Buttons.KEYBOARD_TO_MENU] = new Rectangle((screenWidth / 2) - (800 / 2), 580, 800, 120);
            btnLocs[(int)Buttons.PAUSE_RESUME_TO_GAME] = new Vector2(460, 320);
            btnBackdropLocs[(int)Buttons.PAUSE_RESUME_TO_GAME] = new Vector2(463, 323);
            btnBackgroundRects[(int)Buttons.PAUSE_RESUME_TO_GAME] = new Rectangle((screenWidth / 2) - (400 / 2), 300, 400, 120);
            btnLocs[(int)Buttons.PAUSE_RESTART_TO_GAME] = new Vector2(320, 460);
            btnBackdropLocs[(int)Buttons.PAUSE_RESTART_TO_GAME] = new Vector2(323, 463);
            btnBackgroundRects[(int)Buttons.PAUSE_RESTART_TO_GAME] = new Rectangle((screenWidth / 2) - (630 / 2), 440, 630, 120);
            btnLocs[(int)Buttons.PAUSE_TO_MENU] = new Vector2(300, 600);
            btnBackdropLocs[(int)Buttons.PAUSE_TO_MENU] = new Vector2(303, 603);
            btnBackgroundRects[(int)Buttons.PAUSE_TO_MENU] = new Rectangle((screenWidth / 2) - (700 / 2), 580, 700, 120);

            //Set the font locations
            leaderboardTitleLoc = new Vector2(75, 80);
            leaderboardTitleLocBackdrop = new Vector2(78, 83);
            pausedTitleLoc = new Vector2(340, 90);
            pausedTitleLocBackdrop = new Vector2(343, 93);
            gameoverTitleLoc = new Vector2(175, 90);
            gameoverTitleLocBackdrop = new Vector2(178, 93);
            endgameTitleLoc = new Vector2(250, 90);
            endgameTitleLocBackdrop = new Vector2(253, 93);
            leaderboardEntryTitleLoc[0] = new Vector2(220, 60);
            leaderboardEntryTitleLocBackdrop[0] = new Vector2(223, 63);
            leaderboardEntryTitleLoc[1] = new Vector2(10, 130);
            leaderboardEntryTitleLocBackdrop[1] = new Vector2(13, 133);
            leaderboardEntryPromptLoc = new Vector2(30, 260);
            leaderboardEntryPromptLocBackdrop = new Vector2(33, 263);
            totalTimeLoc = new Vector2(360, 300);
            totalHurtLoc = new Vector2(600, 430);
            controlsLoc[0] = new Vector2(20, 230);
            controlsLoc[1] = new Vector2(0, 310);
            controlsLoc[2] = new Vector2(20, 380);
            controlsLoc[3] = new Vector2(40, 450);
            controlsLoc[4] = new Vector2(70, 520);
            completionPercentLoc = new Vector2(0, 320);
            soulImgInstLoc = new Vector2(1010, 160);
            soulInstLocs[0] = new Vector2(970, 100);
            soulInstLocs[1] = new Vector2(975, 130);
            soulInstLocs[2] = new Vector2(975, 280);
            soulInstLocs[3] = new Vector2(970, 310);
            soulInstLocs[4] = new Vector2(935, 360);
            soulInstLocs[5] = new Vector2(970, 390);
            soulInstLocs[6] = new Vector2(955, 420);

            //Set the leaderboard background location
            leaderboardBackground = new Rectangle(380, 270, 600, 300);

            //Set the constant leaderboard boundaries
            leaderboardBoundaries[0] = new Rectangle(380, 270, 10, 300);
            leaderboardBoundaries[1] = new Rectangle(970, 270, 10, 300);
            leaderboardBoundaries[2] = new Rectangle(380, 270, 600, 10);
            leaderboardBoundaries[3] = new Rectangle(380, 570, 600, 10);
            leaderboardBoundaries[4] = new Rectangle(430, 270, 10, 300);
            leaderboardBoundaries[5] = new Rectangle(580, 270, 10, 300);

            //Loop through the rest of the leaderboard boundaries, and set them
            for (int i = 6; i < leaderboardBoundaries.Length; i++)
            {
                //Set the location of the leaderboard boundary
                leaderboardBoundaries[i] = new Rectangle(380, 330 + ((i - 6) * 60), 600, 10);
            }

            //Set the leaderboard locations
            for (int i = 0; i < NUM_LEADERBOARD_ENTRIES; i++)
            {
                //Set the location of a leaderboard entry
                leaderboardNumLocs[i].X = 400;
                leaderboardNumLocs[i].Y = 290 + (60 * i);
                leaderboardNameLocs[i].X = 450;
                leaderboardNameLocs[i].Y = 290 + (60 * i);
                leaderboardTimeLocs[i].X = 600;
                leaderboardTimeLocs[i].Y = 290 + (60 * i);
                leaderboardNumBackdropLocs[i].X = 402;
                leaderboardNumBackdropLocs[i].Y = 292 + (60 * i);
                leaderboardNameBackdropLocs[i].X = 452;
                leaderboardNameBackdropLocs[i].Y = 292 + (60 * i);
                leaderboardTimeBackdropLocs[i].X = 602;
                leaderboardTimeBackdropLocs[i].Y = 292 + (60 * i);

                //Set the colour to draw each leaderboard entry
                if (i == 0)
                {
                    //Set the first entry to gold
                    leaderboardColours[i] = Color.Gold;
                }
                else if (i == 1)
                {
                    //Set the second entry to silver
                    leaderboardColours[i] = Color.Silver;
                }
                else if (i == 2)
                {
                    //Set the third entry to bronze
                    leaderboardColours[i] = new Color(205, 127, 50);
                }
                else
                {
                    //The rest of the leaderboard entries are white
                    leaderboardColours[i] = Color.White;
                }
            }

            //Set the leaderboard background colour
            leaderboardBackgroundColour = new Color(37, 37, 37);

            //Set the button state stats
            btnBackgroundTransparancy[(int)BtnState.NON_HOVER] = 0.6f;
            btnBackgroundTransparancy[(int)BtnState.HOVER] = 0.8f;
            btnColour[(int)BtnState.NON_HOVER] = Color.White;
            btnColour[(int)BtnState.HOVER] = Color.Yellow;

            //Set the boss file names by looping through each one
            for (int i = 0; i < bossFileNames.Length; i++)
            {
                //Set each boss file name
                bossFileNames[i] = "Levels/BOSS/BOSS_" + ((EntrancePoint)i).ToString() + ".csv";
            }

            //Create each of the levels in the game
            CreateGameLvls();

            //Set the starting room
            curRoom = llRoom.GetHead();

            //Set the list of insults
            insults = new string[2][]
            {
                new string[]
                {
                    "Wow, you really suck",
                    "I thought you were",
                    "You're worse at",
                    "Ok, you can take",
                    "Would you like a",
                    "Remember, you can",
                    "Do I really need to make",
                    "My grandma can play",
                    "Thanks for verifying",
                    "I'm sure it was the" ,
                    "Do yourself a favour and"
                },

                new string[]
                {
                    "at this, don't you?",
                    "good at this game",
                    "this than I expected",
                    "off the blindfold now",
                    "participation trophy?",
                    "press A and D to move",
                    "an easy mode just for you?",
                    "this game better than you",
                    "that the death screen works",
                    "lag that killed you",
                    "book an eye doctor appointment"
                }
            };

            //Set the location of the insults
            insultsLoc[0] = new Vector2(0, 420);
            insultsLoc[1] = new Vector2(0, 480);

            //Set the cutscene timer
            cutsceneTimer = new Timer(3000, true);

            //Set the game timer to count up
            gameTimer = new Timer(Timer.INFINITE_TIMER, true);

            //Read in the games stats
            ReadLeaderboard();

            //Set the leaderboard times
            SetLeaderboardTimes("", Double.MaxValue);

            //Start the pause screen fully opaque
            bgTransparancy[(int)GameState.PAUSE] = 1f;

            //Start the menu music
            MediaPlayer.Play(menuTheme);
            MediaPlayer.IsRepeating = true;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Update the mouse position if the user isn't in a game or cutscene
            if (gameState != GameState.GAME && gameState != GameState.CUTSCENE)
            {
                //Update the mouse location
                prevMouse = mouse;
                mouse = Mouse.GetState();
                mouseLoc.X = mouse.X;
                mouseLoc.Y = mouse.Y;
                mouseLocBackdrop.X = mouseLoc.X + 3;
                mouseLocBackdrop.Y = mouseLoc.Y + 3;
            }

            //Set the keyboard
            prevKb = kb;
            kb = Keyboard.GetState();

            //Update the game based on the current game state
            switch (gameState)
            {
                case GameState.MENU:
                    //Update the menu
                    UpdateMenu();
                    break;
                case GameState.LEADERBOARD:
                    //Update the leaderboard
                    UpdateLeaderboard();
                    break;
                case GameState.GAME:
                    //Update the game
                    UpdateGame(gameTime);
                    break;
                case GameState.CUTSCENE:
                    //Update the cutscene
                    UpdateCutscene(gameTime);
                    break;
                case GameState.PAUSE:
                    //Update the pause menu
                    UpdatePause();
                    break;
                case GameState.GAME_OVER:
                    //Update the game over screen
                    UpdateGameOver();
                    break;
                case GameState.ENDGAME:
                    //Update the endgame screen
                    UpdateEndgame(gameTime);
                    break;
                case GameState.LEADERBOARD_ENTRY:
                    //Update the keyboard entry screen
                    UpdateKeyboardEntry();
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //Begin the spritebatch
            spriteBatch.Begin();

            //Draw the background of the game, if a cutscene isn't playing
            if (gameState != GameState.CUTSCENE)
            {
                //Draw the background of the game
                spriteBatch.Draw(backgrounds[(int)gameState], bgRect, Color.White * bgTransparancy[(int)gameState]);
            }

            //Draw the game based on the current game state
            switch (gameState)
            {
                case GameState.MENU:
                    //Draw the menu
                    DrawMenu();
                    break;
                case GameState.LEADERBOARD:
                    //Draw the leaderboard
                    DrawLeaderboard();
                    break;
                case GameState.GAME:
                    //Draw the game
                    DrawGame();
                    break;
                case GameState.CUTSCENE:
                    //Draw the cutscene
                    DrawCutscene();
                    break;
                case GameState.PAUSE:
                    //Draw the pause menu
                    DrawPause();
                    break;
                case GameState.GAME_OVER:
                    //Draw the game over screen
                    DrawGameOver();
                    break;
                case GameState.ENDGAME:
                    //Draw the endgame screen
                    DrawEndgame();
                    break;
                case GameState.LEADERBOARD_ENTRY:
                    //Draw the keyboard entry screen
                    DrawKeyboardEntry();
                    break;
            }

            //Draw the cursor if the user isn't in a game or the cutscene
            if (gameState != GameState.GAME && gameState != GameState.CUTSCENE)
            {
                //Draw the cursor
                spriteBatch.Draw(mouseImg, mouseLocBackdrop, Color.Black * bgTransparancy[(int)gameState]);
                spriteBatch.Draw(mouseImg, mouseLoc, Color.White * bgTransparancy[(int)gameState]);
            }

            //End the spritebatch
            spriteBatch.End();

            base.Draw(gameTime);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the menu
        private void UpdateMenu()
        {
            //If the menu screen isn't fully opaque keep making it more opaque
            if (bgTransparancy[(int)gameState] < 1f && !buttonPressed[(int)Buttons.MENU_TO_GAME] && !buttonPressed[(int)Buttons.MENU_TO_LEADERBOARD] && !buttonPressed[(int)Buttons.MENU_TO_EXIT])
            {
                //Make the menu less transparant
                bgTransparancy[(int)gameState] += SCREEN_TRANSITION_SPEED;
            }

            //Test for a button press, and start the game if there was one
            if (TestButtonPress(Buttons.MENU_TO_GAME, GameState.GAME))
            {
                //Start the game music
                MediaPlayer.Play(gameTheme);
                MediaPlayer.Volume = 0.4f;
                MediaPlayer.IsRepeating = true;
            }

            //Test for a button press, and send the user to the leaderboard if there was one
            TestButtonPress(Buttons.MENU_TO_LEADERBOARD, GameState.LEADERBOARD);

            //Test for a button press, and exit the game if there was one
            TestButtonPress(Buttons.MENU_TO_EXIT, GameState.EXIT);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the leaderboard
        private void UpdateLeaderboard()
        {
            //If the leaderboard screen isn't fully opaque keep making it more opaque
            if (bgTransparancy[(int)gameState] < 1f && !buttonPressed[(int)Buttons.LEADERBOARD_TO_MENU])
            {
                //Make the leaderboard less transparant
                bgTransparancy[(int)gameState] += SCREEN_TRANSITION_SPEED;
            }

            //Test for a button press, go back to the menu if there was one
            TestButtonPress(Buttons.LEADERBOARD_TO_MENU, GameState.MENU);
        }

        //Pre: gameTime tracks the time passed in the game
        //Post: N/A
        //Desc: Updates the game
        private void UpdateGame(GameTime gameTime)
        {
            //If the player is dead, fade the screen out until its transparent, and the user is sent to the next game state. If not, update the game
            if (playerDead)
            {
                //Make the game more transparant
                bgTransparancy[(int)gameState] -= SCREEN_TRANSITION_SPEED;

                //If the game is fully transparent, go to the game over cutscene
                if (bgTransparancy[(int)gameState] <= 0)
                {
                    //Go to the game over cutscene
                    gameState = GameState.CUTSCENE;
                    cutsceneType = CutsceneType.LOSE;
                    bgTransparancy[(int)GameState.CUTSCENE] = 0f;
                    playerDead = false;

                    //Start the lose cutscene fully opaque
                    bgTransparancy[(int)GameState.CUTSCENE] = 1f;

                    //Stop all sounds in the game
                    MediaPlayer.Volume = 0f;
                    curPlayer.StopSounds();
                    curRoom.GetCargo().StopAllSounds();

                    //Play the death sound
                    deathSnd.CreateInstance().Play();
                }
            }
            else
            {
                //Update the main logic of the game if there is no room transition
                if (!roomTransition)
                {
                    //Update the the total time the player is in the game for
                    gameTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

                    //Update the spirit pickup
                    UpdateSpiritPickup(gameTime);

                    //Update each enemy
                    curRoom.GetCargo().UpdateEnemies(gameTime, curPlayer.GetHitBox());

                    //Update the player
                    curPlayer.Update(gameTime);

                    //Do all colission detection in the game
                    CollisionDetection();

                    //Update the enemy positions
                    curRoom.GetCargo().UpdateEnemyPos();

                    //Update the players position
                    curPlayer.UpdateGamePos();

                    //Go to the next room if the player finishes the room
                    if (curRoom.GetCargo().UpdateRoom(curPlayer.GetHitBox()))
                    {
                        //Start transitioning rooms
                        roomTransition = true;
                    }

                    //Pause the game if the escape key is pressed
                    if (kb.IsKeyDown(Keys.Escape) && prevKb.IsKeyUp(Keys.Escape))
                    {
                        //Pause the game
                        gameState = GameState.PAUSE;

                        //Update the mouse location so it doesn't jump once pause is pressed
                        prevMouse = mouse;
                        mouse = Mouse.GetState();
                        mouseLoc.X = mouse.X;
                        mouseLoc.Y = mouse.Y;
                        mouseLocBackdrop.X = mouseLoc.X + 3;
                        mouseLocBackdrop.Y = mouseLoc.Y + 3;

                        //Lower the volume in the pause state
                        MediaPlayer.Volume = 0.15f;

                        //Stop all repeating player sounds
                        curPlayer.StopSounds();
                        curRoom.GetCargo().StopAllSounds();
                    }
                }

                //Make the game more opaque if it's not already, and is not transitioning between rooms
                if (bgTransparancy[(int)gameState] < 1 && !roomTransition)
                {
                    //Make the game more opaque
                    bgTransparancy[(int)gameState] += GAME_TRANSITION_SPEED;
                }
                else if (roomTransition)
                {
                    //Make the game more transparent
                    bgTransparancy[(int)gameState] -= GAME_TRANSITION_SPEED;

                    //If the room is fully transparent, set the game to the next room
                    if (bgTransparancy[(int)gameState] <= 0f)
                    {
                        //Stop transitioning rooms
                        roomTransition = false;

                        //Send the player to the next room
                        curRoom = curRoom.GetNext();

                        //Increase the room the player is on
                        curLvl++;

                        //Bring the player to the beginning of the room
                        curPlayer.SetLoc(curRoom.GetCargo().GetSpawnLoc());

                        //Play the door closing sound effect (The door behind the player just closed)
                        doorCloseSnd.CreateInstance().Play();

                        //If the new room is the boss room, start the boss music
                        if (curRoom.GetCargo() == bossRoom)
                        {
                            //Start the boss music
                            MediaPlayer.Play(bossTheme);
                            MediaPlayer.Volume = 0.4f;
                            MediaPlayer.IsRepeating = true;
                        }
                    }
                }
            }
        }

        //Pre: gameTime allows the particles to update
        //Post: N/A
        //Desc: Updates the game
        private void UpdateCutscene(GameTime gameTime)
        {
            //Update the emitter
            emitter.Update(gameTime, curPlayer.GetHitBox());

            //Perform different logic based on the cutscene type
            if (cutsceneType == CutsceneType.WIN)
            {
                //Loop through each particle, and end the game if the player touches a soul particle
                for (int i = 0; i < emitter.GetCount(); i++)
                {
                    //If the particle is a soul particle, and the player intersects with it, end the game
                    if (emitter.GetParticles()[i] is SoulParticle && Util.Intersects(curPlayer.GetHitBox(), emitter.GetParticles()[i].GetHitBox()))
                    {
                        //End the game
                        gameState = GameState.ENDGAME;

                        //Reset the endgame's transparancy
                        bgTransparancy[(int)gameState] = 0f;

                        //Remove all particles from the emitter
                        emitter.Clear();

                        //If the player is on the leaderboard, show the keyboard and allow them to add their name
                        if ((gameTimer.GetTimePassed() / 1000) < leaderboardValue[leaderboardValue.Length - 1])
                        {
                            //Show the keyboard and allow them to add their name
                            onLeaderboard = true;
                        }

                        //Start the win music
                        MediaPlayer.Play(winTheme);
                        MediaPlayer.Volume = 1f;
                        MediaPlayer.IsRepeating = true;
                    }
                }
            }
            else
            {
                //Update the cutscene timer
                cutsceneTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

                //If the timer is finished, send the player to the game over screen
                if (cutsceneTimer.IsFinished())
                {
                    //Make the game more transparant
                    bgTransparancy[(int)gameState] -= SCREEN_TRANSITION_SPEED;

                    //If the game is fully transparent, go to the game over state
                    if (bgTransparancy[(int)gameState] <= 0)
                    {
                        //Go to the game over state
                        gameState = GameState.GAME_OVER;
                        bgTransparancy[(int)GameState.GAME_OVER] = 0f;
                        playerDead = false;

                        //Set the insult to show the player
                        insultIdx = rng.Next(0, insults[0].Length);

                        //Calculate the percentage of the game the player compeleted
                        lvlPercent = curLvl * 100 / 16;
                        completionPercentLoc.X = (screenWidth / 2) - (lvlPercent.ToString().Length * 38);

                        //Set the location of the insults
                        insultsLoc[0].X = (screenWidth / 2) - (12 * insults[0][insultIdx].Length);
                        insultsLoc[1].X = (screenWidth / 2) - (12 * insults[1][insultIdx].Length);

                        //Reset the cutscene timer for the next game
                        cutsceneTimer.ResetTimer(true);

                        //Start the lose music
                        MediaPlayer.Play(loseTheme);
                        MediaPlayer.Volume = 1f;
                        MediaPlayer.IsRepeating = true;
                    }
                }
                else
                {
                    //Send particles out of the player
                    emitter.CreateParticle(new Vector2(curPlayer.GetHitBox().Center.X - 15, curPlayer.GetHitBox().Center.Y - 20), 3000, 12, rng.Next(0, 361) * Math.PI / 180, 0, rng.Next(20, 91) / 100f, Color.White, 0, Emitter.ParticleType.NORMAL);
                    emitter.CreateParticle(new Vector2(curPlayer.GetHitBox().Center.X - 15, curPlayer.GetHitBox().Center.Y - 20), 3000, 12, rng.Next(0, 361) * Math.PI / 180, 0, rng.Next(20, 91) / 100f, Color.White, 0, Emitter.ParticleType.NORMAL);
                    emitter.CreateParticle(new Vector2(curPlayer.GetHitBox().Center.X - 15, curPlayer.GetHitBox().Center.Y - 20), 3000, 12, rng.Next(0, 361) * Math.PI / 180, 0, rng.Next(20, 91) / 100f, Color.White, 0, Emitter.ParticleType.NORMAL);
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the pause screen
        private void UpdatePause()
        {
            //If the resume key is being hovered over set it to its hover state
            if (Util.Intersects(btnBackgroundRects[(int)Buttons.PAUSE_RESUME_TO_GAME], mouseLoc))
            {
                //The resume button is being hovered over
                btnHover[(int)Buttons.PAUSE_RESUME_TO_GAME] = true;
            }
            else
            {
                //The resume button is not being hovered over
                btnHover[(int)Buttons.PAUSE_RESUME_TO_GAME] = false;
            }

            //Unpause the game if the escape or resume key is pressed
            if ((kb.IsKeyDown(Keys.Escape) && prevKb.IsKeyUp(Keys.Escape)) || (btnHover[(int)Buttons.PAUSE_RESUME_TO_GAME] && mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed))
            {
                //Unpause the game
                gameState = GameState.GAME;

                //Play the button click sound
                btnSnd.CreateInstance().Play();

                //Increase the music volume
                MediaPlayer.Volume = 0.4f;
            }

            //If the restart game is pressed, restart the game
            if (TestButtonPress(Buttons.PAUSE_RESTART_TO_GAME, GameState.GAME))
            {
                //Reset the game
                ResetGame();

                //If the pause button was pressed, make the pause state fully opaque again
                bgTransparancy[(int)GameState.PAUSE] = 1f;
            }

            //Test for a button press, and send the user to the menu if there was one
            if (TestButtonPress(Buttons.PAUSE_TO_MENU, GameState.MENU))
            {
                //If the pause button was pressed, make the pause state fully opaque again
                bgTransparancy[(int)GameState.PAUSE] = 1f;

                //Play the menu music
                MediaPlayer.Play(menuTheme);
                MediaPlayer.Volume = 1f;
                MediaPlayer.IsRepeating = true;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the game over screen
        private void UpdateGameOver()
        {
            //If the endgame screen isn't fully opaque keep making it more opaque
            if (bgTransparancy[(int)gameState] < 1f && !buttonPressed[(int)Buttons.GAMEOVER_TO_MENU])
            {
                //Make the endgame less transparant
                bgTransparancy[(int)gameState] += SCREEN_TRANSITION_SPEED;
            }

            //Test for a button press, and send the user to the menu if there was one
            if (TestButtonPress(Buttons.GAMEOVER_TO_MENU, GameState.MENU))
            {
                //Reset the game
                ResetGame();

                //Start the menu music
                MediaPlayer.Play(menuTheme);
                MediaPlayer.IsRepeating = true;
            }
        }

        //Pre: gameTime allows the animations to update
        //Post: N/A
        //Desc: Updates the endgame screen
        private void UpdateEndgame(GameTime gameTime)
        {
            //Update the particles
            emitter.Update(gameTime, curPlayer.GetHitBox());

            //If the endgame screen isn't fully opaque keep making it more opaque
            if (bgTransparancy[(int)gameState] < 1f && !buttonPressed[(int)Buttons.ENDGAME_TO_MENU])
            {
                //Make the endgame less transparant
                bgTransparancy[(int)gameState] += SCREEN_TRANSITION_SPEED;
            }

            //Spawn a particle every few frames
            if (gameTime.TotalGameTime.Milliseconds % 32 == 0)
            {
                //Spawn a particle to slowly move up the screen
                emitter.CreateParticle(new Vector2(rng.Next(0, screenWidth), screenHeight + 50), 100000, rng.Next(-5, 0), Math.PI / 2, 0, rng.Next(10, 81) / 100f, Color.LightGoldenrodYellow * 0.6f, 0, Emitter.ParticleType.NORMAL);
            }

            //Change the place to send the user next based on if they made the leaderboard
            if (onLeaderboard)
            {
                //Test for a button press, and send the user to the menu if there was one
                TestButtonPress(Buttons.ENDGAME_TO_MENU, GameState.LEADERBOARD_ENTRY);
            }
            else
            {
                //Test for a button press, and send the user to the menu if there was one
                if (TestButtonPress(Buttons.ENDGAME_TO_MENU, GameState.MENU))
                {
                    //Reset the game
                    ResetGame();

                    //Start the menu music
                    MediaPlayer.Play(menuTheme);
                    MediaPlayer.IsRepeating = true;
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Updates the keyboard entry screen
        private void UpdateKeyboardEntry()
        {
            //Retrieve and store the users name from what they type on their keyboard
            playerName = UseKeyboard(playerName, MAX_CHARS_IN_NAME);

            //If the endgame screen isn't fully opaque keep making it more opaque
            if (bgTransparancy[(int)gameState] < 1f && !buttonPressed[(int)Buttons.KEYBOARD_TO_MENU])
            {
                //Make the keyboard entry less transparant
                bgTransparancy[(int)gameState] += SCREEN_TRANSITION_SPEED;
            }

            //Test for a button press, and send the user to the menu and update the leaderboard if there was one
            if (TestButtonPress(Buttons.KEYBOARD_TO_MENU, GameState.MENU))
            {
                //Recalculate the leaderboard based on the new time
                SetLeaderboardTimes(playerName, (int)gameTimer.GetTimePassed());

                //Reset the game
                ResetGame();

                //Start the menu music
                MediaPlayer.Play(menuTheme);
                MediaPlayer.IsRepeating = true;
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Draws the menu
        private void DrawMenu()
        {
            //Draw the menu title
            spriteBatch.Draw(menuTitleImg, menuTitleRect, Color.White * bgTransparancy[(int)gameState]);

            //Draw the menu's buttons
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.MENU_TO_GAME], Color.Black * btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.MENU_TO_GAME])] * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Start Game", btnLocs[(int)Buttons.MENU_TO_GAME], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Start Game", btnBackdropLocs[(int)Buttons.MENU_TO_GAME], btnColour[Convert.ToInt32(btnHover[(int)Buttons.MENU_TO_GAME])] * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.MENU_TO_LEADERBOARD], Color.Black * btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.MENU_TO_LEADERBOARD])] * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Leaderboard", btnLocs[(int)Buttons.MENU_TO_LEADERBOARD], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Leaderboard", btnBackdropLocs[(int)Buttons.MENU_TO_LEADERBOARD], btnColour[Convert.ToInt32(btnHover[(int)Buttons.MENU_TO_LEADERBOARD])] * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.MENU_TO_EXIT], Color.Black * btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.MENU_TO_EXIT])] * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Exit", btnLocs[(int)Buttons.MENU_TO_EXIT], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Exit", btnBackdropLocs[(int)Buttons.MENU_TO_EXIT], btnColour[Convert.ToInt32(btnHover[(int)Buttons.MENU_TO_EXIT])] * bgTransparancy[(int)gameState]);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Draws the leaderboard
        private void DrawLeaderboard()
        {
            //Draw the title
            spriteBatch.DrawString(titleFont, "Leaderboard", leaderboardTitleLocBackdrop, Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(titleFont, "Leaderboard", leaderboardTitleLoc, Color.White * bgTransparancy[(int)gameState]);

            //Draw the title covers
            spriteBatch.Draw(titleCoverTop, leaderboardTitleCoverTopRect, Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(titleCoverBot, leaderboardTitleCoverBotRect, Color.White * bgTransparancy[(int)gameState]);

            //Draw the return to menu button
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.LEADERBOARD_TO_MENU], Color.Black * btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.LEADERBOARD_TO_MENU])] * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Return To Menu", btnLocs[(int)Buttons.LEADERBOARD_TO_MENU], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Return To Menu", btnBackdropLocs[(int)Buttons.LEADERBOARD_TO_MENU], btnColour[Convert.ToInt32(btnHover[(int)Buttons.LEADERBOARD_TO_MENU])] * bgTransparancy[(int)gameState]);

            //Draw the background of the leaderboard
            spriteBatch.Draw(blankPixel, leaderboardBackground, Color.White * 0.4f * bgTransparancy[(int)gameState]);

            //Draw the actual leaderboard
            for (int i = 0; i < NUM_LEADERBOARD_ENTRIES; i++)
            {
                //Draw an individual leaderboard entry
                spriteBatch.DrawString(insultFont, (i + 1).ToString(), leaderboardNumBackdropLocs[i], Color.Black * bgTransparancy[(int)gameState]);
                spriteBatch.DrawString(insultFont, leaderboardNames[i], leaderboardNameBackdropLocs[i], Color.Black * bgTransparancy[(int)gameState]);
                spriteBatch.DrawString(insultFont, leaderboardTime[i], leaderboardTimeBackdropLocs[i], Color.Black * bgTransparancy[(int)gameState]);
                spriteBatch.DrawString(insultFont, (i + 1).ToString(), leaderboardNumLocs[i], leaderboardColours[i] * bgTransparancy[(int)gameState]);
                spriteBatch.DrawString(insultFont, leaderboardNames[i], leaderboardNameLocs[i], leaderboardColours[i] * bgTransparancy[(int)gameState]);
                spriteBatch.DrawString(insultFont, leaderboardTime[i], leaderboardTimeLocs[i], leaderboardColours[i] * bgTransparancy[(int)gameState]);
            }

            //Draw each leaderboard boundary
            for (int i = 0; i < leaderboardBoundaries.Length; i++)
            {
                //Draw the leaderboard entry
                spriteBatch.Draw(blankPixel, leaderboardBoundaries[i], leaderboardBackgroundColour * bgTransparancy[(int)gameState]);
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Draws the game
        private void DrawGame()
        {
            //Draw the current room
            curRoom.GetCargo().Draw(spriteBatch, bgTransparancy[(int)gameState]);

            //Draw the player
            curPlayer.Draw(spriteBatch, bgTransparancy[(int)gameState]);

            //Draw the spirit pickup animation if it should be drawn
            if (spiritPickupActive)
            {
                //Draw the spirit pickup animation
                spiritPickupAnim.Draw(spriteBatch, Color.White * spiritPickupTransparancy, Animation.FLIP_NONE);
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Draws the cutscene
        private void DrawCutscene()
        {
            //Draw the particles from the emitter
            emitter.Draw(spriteBatch, bgTransparancy[(int)gameState]);

            //Draw the player
            curPlayer.Draw(spriteBatch, bgTransparancy[(int)gameState]);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Draws the pause screen
        private void DrawPause()
        {
            //Draw the game
            DrawGame();

            //Darken the screen 
            spriteBatch.Draw(blankPixel, bgRect, Color.Black * 0.7f);

            //Draw the title
            spriteBatch.DrawString(titleFont, "Paused", pausedTitleLoc, Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(titleFont, "Paused", pausedTitleLocBackdrop, Color.White * bgTransparancy[(int)gameState]);

            //Draw the title covers
            spriteBatch.Draw(titleCoverTop, titleCoverTopRect, Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(titleCoverBot, titleCoverBotRect, Color.White * bgTransparancy[(int)gameState]);

            //Draw the pause states buttons
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.PAUSE_RESUME_TO_GAME], Color.Black * (btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.PAUSE_RESUME_TO_GAME])] + 0.15f) * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Resume", btnLocs[(int)Buttons.PAUSE_RESUME_TO_GAME], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Resume", btnBackdropLocs[(int)Buttons.PAUSE_RESUME_TO_GAME], btnColour[Convert.ToInt32(btnHover[(int)Buttons.PAUSE_RESUME_TO_GAME])] * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.PAUSE_RESTART_TO_GAME], Color.Black * (btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.PAUSE_RESTART_TO_GAME])] + 0.15f) * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Restart Game", btnLocs[(int)Buttons.PAUSE_RESTART_TO_GAME], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Restart Game", btnBackdropLocs[(int)Buttons.PAUSE_RESTART_TO_GAME], btnColour[Convert.ToInt32(btnHover[(int)Buttons.PAUSE_RESTART_TO_GAME])] * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.PAUSE_TO_MENU], Color.Black * (btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.PAUSE_TO_MENU])] + 0.15f) * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Save and Quit", btnLocs[(int)Buttons.PAUSE_TO_MENU], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Save and Quit", btnBackdropLocs[(int)Buttons.PAUSE_TO_MENU], btnColour[Convert.ToInt32(btnHover[(int)Buttons.PAUSE_TO_MENU])] * bgTransparancy[(int)gameState]);

            //Draw the controls list
            spriteBatch.DrawString(promptFont, "Controls:", controlsLoc[0], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(insultFont, "A and D to move", controlsLoc[1], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(insultFont, "SPACE to jump", controlsLoc[2], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(insultFont, "K to attack", controlsLoc[3], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(insultFont, "J to heal", controlsLoc[4], Color.Orange * bgTransparancy[(int)gameState]);

            //Draw the soul instructions
            spriteBatch.Draw(soulGuageImgs[4], soulImgInstLoc, Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(instFont, "This is your", soulInstLocs[0], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(instFont, "soul guage", soulInstLocs[1], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(instFont, "Kill enemies", soulInstLocs[2], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(instFont, "to gain soul", soulInstLocs[3], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(instFont, "Gain enough soul", soulInstLocs[4], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(instFont, "and you can ", soulInstLocs[5], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(instFont, "hold J to heal", soulInstLocs[6], Color.Orange * bgTransparancy[(int)gameState]);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Draws the game over screen
        private void DrawGameOver()
        {
            //Draw the title
            spriteBatch.DrawString(titleFont, "Game Over", gameoverTitleLocBackdrop, Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(titleFont, "Game Over", gameoverTitleLoc, Color.White * bgTransparancy[(int)gameState]);

            //Draw the title covers
            spriteBatch.Draw(titleCoverTop, titleCoverTopRect, Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(titleCoverBot, titleCoverBotRect, Color.White * bgTransparancy[(int)gameState]);

            //Draw the players completion percentage to the screen
            spriteBatch.DrawString(buttonFont, lvlPercent + "%", completionPercentLoc, Color.White * bgTransparancy[(int)gameState]);

            //Draw the insult to the screen
            spriteBatch.DrawString(insultFont, insults[0][insultIdx], insultsLoc[0], Color.Orange * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(insultFont, insults[1][insultIdx], insultsLoc[1], Color.Orange * bgTransparancy[(int)gameState]);

            //Draw the return to menu button
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.GAMEOVER_TO_MENU], Color.Black * btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.GAMEOVER_TO_MENU])] * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Return To Menu", btnLocs[(int)Buttons.GAMEOVER_TO_MENU], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Return To Menu", btnBackdropLocs[(int)Buttons.GAMEOVER_TO_MENU], btnColour[Convert.ToInt32(btnHover[(int)Buttons.GAMEOVER_TO_MENU])] * bgTransparancy[(int)gameState]);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Draws the endgame screen
        private void DrawEndgame()
        {
            //Draw the title
            spriteBatch.DrawString(titleFont, "You Win!", endgameTitleLocBackdrop, Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(titleFont, "You Win!", endgameTitleLoc, Color.White * bgTransparancy[(int)gameState]);

            //Draw the title covers
            spriteBatch.Draw(titleCoverTop, titleCoverTopRect, Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(titleCoverBot, titleCoverBotRect, Color.White * bgTransparancy[(int)gameState]);

            //Draw the players stats
            spriteBatch.Draw(timerImg, timerRect, Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(knightImgs[(int)PlayerState.HURT], hurtImgRect, Color.White * bgTransparancy[(int)gameState]);
            hurtEffectAnim.Draw(spriteBatch, Color.White * bgTransparancy[(int)gameState], Animation.FLIP_NONE);
            spriteBatch.DrawString(resultFont, ": " + (int)(gameTimer.GetTimePassed() / 60000) + ":" + ((int)(gameTimer.GetTimePassed() / 1000) % 60) + " minutes", totalTimeLoc, Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(resultFont, ": " + curPlayer.GetNumTimesHit() + " times hit", totalHurtLoc, Color.White * bgTransparancy[(int)gameState]);

            //Draw the return to menu button
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.ENDGAME_TO_MENU], Color.Black * btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.ENDGAME_TO_MENU])] * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Return To Menu", btnLocs[(int)Buttons.ENDGAME_TO_MENU], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Return To Menu", btnBackdropLocs[(int)Buttons.ENDGAME_TO_MENU], btnColour[Convert.ToInt32(btnHover[(int)Buttons.ENDGAME_TO_MENU])] * bgTransparancy[(int)gameState]);
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Draws the keyboard entry screen
        private void DrawKeyboardEntry()
        {
            //Draw the title
            spriteBatch.DrawString(resultFont, "Congratulations!", leaderboardEntryTitleLoc[0], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(resultFont, "Congratulations!", leaderboardEntryTitleLocBackdrop[0], Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(resultFont, "You made the leaderboard", leaderboardEntryTitleLoc[1], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(resultFont, "You made the leaderboard", leaderboardEntryTitleLocBackdrop[1], Color.White * bgTransparancy[(int)gameState]);

            //Draw the title covers
            spriteBatch.Draw(titleCoverTop, leaderboardEntryTitleCoverTopRect, Color.White * bgTransparancy[(int)gameState]);
            spriteBatch.Draw(titleCoverBot, leaderboardEntryTitleCoverBotRect, Color.White * bgTransparancy[(int)gameState]);

            //Draw the typing prompt
            spriteBatch.DrawString(promptFont, "Enter Your Initials (MAX 3 letters)", leaderboardEntryPromptLocBackdrop, Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(promptFont, "Enter Your Initials (MAX 3 letters)", leaderboardEntryPromptLoc, Color.Cyan * bgTransparancy[(int)gameState]);

            //Loop through each char underline and draw it
            for (int i = 0; i < MAX_CHARS_IN_NAME; i++)
            {
                //Draw the underline for the name
                spriteBatch.Draw(blankPixel, nameUnderlineRects[i], Color.White * bgTransparancy[(int)gameState]); 
            }

            //Loop through each player name character, and draw it
            for (int i = 0; i < playerName.Length; i++)
            {
                //Draw what the user is typing
                spriteBatch.DrawString(titleFont, playerName[i].ToString(), playerNameLocsBackdrop[i], Color.Black * bgTransparancy[(int)gameState]);
                spriteBatch.DrawString(titleFont, playerName[i].ToString(), playerNameLocs[i], Color.Orange * bgTransparancy[(int)gameState]);
            }         

            //Draw the return to menu button
            spriteBatch.Draw(blankPixel, btnBackgroundRects[(int)Buttons.KEYBOARD_TO_MENU], Color.Black * btnBackgroundTransparancy[Convert.ToInt32(btnHover[(int)Buttons.KEYBOARD_TO_MENU])] * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Return To Menu", btnLocs[(int)Buttons.KEYBOARD_TO_MENU], Color.Black * bgTransparancy[(int)gameState]);
            spriteBatch.DrawString(buttonFont, "Return To Menu", btnBackdropLocs[(int)Buttons.KEYBOARD_TO_MENU], btnColour[Convert.ToInt32(btnHover[(int)Buttons.KEYBOARD_TO_MENU])] * bgTransparancy[(int)gameState]);
        }

        //Pre: gameTime allows the animation to update
        //Post: N/A
        //Desc: Updates the spirit pickup
        private void UpdateSpiritPickup(GameTime gameTime)
        {
            //Update the spirit pickup if the boss is dead, but only if it's the boss room
            if (curRoom.GetCargo() == bossRoom) 
            {
                //Update the spirit pickup if the boss is dead
                if (curRoom.GetCargo().GetEnemies().Count == 0)
                {
                    //Update the spirit pickup
                    spiritPickupAnim.Update(gameTime);

                    //Activate the spirit pickup
                    spiritPickupActive = true;

                    //If the spirit pickup isnt fully transparent make it more transparent
                    if (spiritPickupTransparancy <= 1)
                    {
                        //Make the spirit pickup slightly more transparent
                        spiritPickupTransparancy += 0.01f;
                    }

                    //If the player intersects with the spirit pickup bring them to the cutscene
                    if (Util.Intersects(curPlayer.GetHitBox(), spiritPickupAnim.destRec))
                    {
                        //Start the winning cutscene
                        gameState = GameState.CUTSCENE;
                        cutsceneType = CutsceneType.WIN;

                        //Start the win cutscene as fully opaque
                        bgTransparancy[(int)GameState.CUTSCENE] = 1f;

                        //Stop drawing the player UI
                        curPlayer.DrawUI(false);

                        //Launch a bunch of particles from the player
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, 0, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI / 12, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI / 6, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI / 4, 0, 1f, Color.White, 175, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI / 3, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 5 / 12, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI / 2, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 7 / 12, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 2 / 3, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 3 / 4, 0, 1f, Color.White, 175, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 5 / 6, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 11 / 12, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 13 / 12, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 7 / 6, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 5 / 4, 0, 1f, Color.White, 175, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 4 / 3, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 17 / 12, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 3 / 2, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 5 / 3, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 7 / 4, 0, 1f, Color.White, 175, Emitter.ParticleType.DEATH);
                        emitter.CreateParticle(curPlayer.GetHitBox().Center.ToVector2(), 0, 10, Math.PI * 11 / 6, 0, 1f, Color.White, 150, Emitter.ParticleType.DEATH);

                        //Stop all repeating player sounds
                        curPlayer.StopSounds();

                        //Start the soul pickup sound
                        soulPickupSnd.CreateInstance().Play();
                    }
                }
                else if (curRoom.GetCargo().GetEnemies()[0].GetHealth() == 0)
                {
                    //If the boss is dead, fade out the boss music
                    MediaPlayer.Volume -= 0.01f;
                }
            }
        }

        //Pre: button is the button to test collision for, and nextGameState is the next game state to send the user to
        //Post: return if the button was pressed on that frame
        //Desc: Tests for collision on a specific button
        private bool TestButtonPress(Buttons button, GameState nextGameState)
        {
            //Test for a button press if the mouse intersects with the button
            if (Util.Intersects(btnBackgroundRects[(int)button], mouseLoc))
            {
                //If the mouse is clicked, bring the game to the menu
                if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
                {
                    //Exit the program if its supossed to
                    if (nextGameState == GameState.EXIT)
                    {
                        //Exit the program
                        Exit();
                    }

                    //Start transitioning to the next gameState
                    buttonPressed[(int)button] = true;

                    //Play the button click sound
                    btnSnd.CreateInstance().Play();
                }

                //The mouse is hovering over the button
                btnHover[(int)button] = true;
            }
            else
            {
                //The mouse is not hovering over the button
                btnHover[(int)button] = false;
            }

            //If the button was pressed, fade the screen out until its transparent, and the user is sent to the next game state
            if (buttonPressed[(int)button])
            {
                //Make the current gamestate more transparant
                bgTransparancy[(int)gameState] -= SCREEN_TRANSITION_SPEED;

                //If the current game state is fully transparent, go to the next game state
                if (bgTransparancy[(int)gameState] <= 0)
                {
                    //Go to the next game state 
                    gameState = nextGameState;
                    bgTransparancy[(int)nextGameState] = 0f;
                    buttonPressed[(int)button] = false;

                    //The button was pressed
                    return true;
                }
            }

            //The button wasn't pressed this frame
            return false;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Creates every room in the game
        private void CreateGameLvls()
        {
            //Store the previous exit point
            EntrancePoint prevExitPoint = EntrancePoint.LEFT_BOTTOM;

            //Store the previous room
            Room prevRoom;

            //Clear everything in the current linked list of rooms
            llRoom = new LinkedList();

            //Reset each of the level names
            SetLevelNames();

            //Create each level
            for (int i = 0; i < NUM_LEVELS; i++)
            {
                //Add the next room to the game
                prevRoom = AddRoom(roomDifficultyChances[i], prevExitPoint);

                //Set the new previous exit point
                prevExitPoint = (EntrancePoint)prevRoom.GetExitPoint();
            }

            //Add a boss room to the game once all regular levels are made
            prevRoom = AddBossRoom(prevExitPoint);
        }

        //Pre: difficultyChance is the chance a certain difficulty will be picked, and prevEntrancePoint is the exit point of the previous node
        //Post: Return the room that was just created
        //Desc: Adds the the room to the game
        private Room AddRoom(int[] difficultyChance, EntrancePoint prevExitPoint)
        {
            //Store the created room
            Room createdRoom = null;

            //Store the random variables for the room
            int randomDifficulty;
            int randomFileName;

            //Store the difficulty of the room
            int difficulty;

            //Store the previous difficulties
            int prevDifficulty = 0;

            //Set the random numbers for the room
            randomDifficulty = rng.Next(1, 100);
            randomFileName = rng.Next(0, fileNames[(int)prevExitPoint].Count);

            //Set the difficulty of the room based by looping through each difficulty option
            for (int i = 0; i < difficultyChance.Length; i++)
            {
                //Set the difficulty if the random number is less than the difficutly chance
                if (randomDifficulty < difficultyChance[i] + prevDifficulty)
                {
                    //Set the difficulty of the level
                    difficulty = i + 1;

                    //Store the created room
                    createdRoom = new Room(GraphicsDevice, fileNames[(int)prevExitPoint][randomFileName], i + 1, tileImgs, enemyImgs, particleImg, enemySnds, doorOpenSnd, particleSnds);
                    
                    //Break out of the loop
                    break;
                }

                //Increase the previous difficulty
                prevDifficulty += difficultyChance[i];
            }

            //Create the room
            llRoom.AddToTail(new Node(createdRoom));

            //Stop that room from appearing again
            fileNames[(int)prevExitPoint].RemoveAt(randomFileName);

            return createdRoom;
        }

        //Pre: prevEntrancePoint is the exit point of the previous node
        //Post: Return the room that was just created
        //Desc: Adds the the room to the game
        private Room AddBossRoom(EntrancePoint prevExitPoint)
        {
            //Store the created room
            Room createdRoom = null;

            //Store the created room
            createdRoom = new Room(GraphicsDevice, bossFileNames[(int)prevExitPoint], 1, tileImgs, enemyImgs, particleImg, enemySnds, doorOpenSnd, particleSnds);

            //Create the room
            llRoom.AddToTail(new Node(createdRoom));

            //Set the boss room
            bossRoom = createdRoom;

            return createdRoom;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Read the leaderboard from the file
        private void ReadLeaderboard()
        {
            //Stores the data from the file
            string[] data;

            try
            {
                //Open the stats file
                inFile = File.OpenText("Leaderboard.txt");

                //Loop through each line in the file
                for (int i = 0; i < NUM_LEADERBOARD_ENTRIES; i++)
                {
                    //Store each datapoint in the file
                    data = inFile.ReadLine().Split(',');

                    //Set the next leaderboard entry
                    leaderboardNames[i] = data[0];
                    leaderboardValue[i] = Convert.ToDouble(data[1]);
                }
            }
            catch (FormatException fe)
            {
                //Give an error message to the user
                Console.WriteLine("Error Reading in Leaderboard");
            }
            catch (Exception e)
            {
                //Give an error message to the user
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                //Close the file if its not null
                if (inFile != null)
                {
                    //Close the file
                    inFile.Close();
                }
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Writes the leadeboard to the file
        private void WriteLeaderboard()
        {
            try
            {
                //Open the stats file
                outFile = File.CreateText("Leaderboard.txt");

                //Loop through each leaderboard entry and write it to the file
                for (int i = 0; i < NUM_LEADERBOARD_ENTRIES; i++)
                {
                    //Write the next leaderboard entry to the file
                    outFile.WriteLine(leaderboardNames[i] + "," + leaderboardValue[i]);
                }
            }
            catch (Exception e)
            {
                //Give an error message to the user
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                //Close the file if its not null
                if (outFile != null)
                {
                    //Close the file
                    outFile.Close();
                }
            }
        }

        //Pre: newName is the new name to add to the leaderboard, and newValue is the new value to add to the leaderboard
        //Post: N/A
        //Desc: Calculates the value to display on the leaderboard
        private void SetLeaderboardTimes(string newName, double newValue)
        {
            //Convert the new value to from milliseconds to seconds
            newValue /= 1000;

            //Calculate the leaderboard order based on the new entry
            for (int i = 0; i < NUM_LEADERBOARD_ENTRIES; i++)
            {
                //If the new entry is bigger then the current one, put it in the leaderboard, and shift everything else down
                if (newValue < leaderboardValue[i])
                {
                    //Shift everything down by one
                    for (int j = NUM_LEADERBOARD_ENTRIES - 1; j > i; j--)
                    {
                        //Set the final leaderboard value to the one before (Shifts it down by one)
                        leaderboardNames[j] = leaderboardNames[j - 1];
                        leaderboardValue[j] = leaderboardValue[j - 1];
                    }

                    //Add the new value to the leaderboard
                    leaderboardNames[i] = newName;
                    leaderboardValue[i] = newValue;

                    //The new entry has been inserted. Break out of the loop
                    break;
                }
            }

            //Write the new leaderboard entries to the file
            WriteLeaderboard();

            //Loop through each leaderboard entry to set it
            for (int i = 0; i < NUM_LEADERBOARD_ENTRIES; i++)
            {
                //Add a 0 infront of the seconds if there is less than 10 seconds
                if (leaderboardValue[i] % 60 < 10)
                {
                    //Set the number to show on the leaderboard
                    leaderboardTime[i] = ((int)leaderboardValue[i] / 60) + ":0" + Math.Round((leaderboardValue[i] % 60), 2);
                }
                else
                {
                    //Set the number to show on the leaderboard
                    leaderboardTime[i] = ((int)leaderboardValue[i] / 60) + ":" + Math.Round((leaderboardValue[i] % 60), 2);
                }
                
            }
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Handles all colission detection for the game
        private void CollisionDetection()
        { 
            //Stores if the attack has collided with a tile in this frame
            bool attackTileCollision = false;

            //Store the room tile and enemy
            Tile roomTile;
            Enemy roomEnemy;

            //Loop through each tile and test colission with the player
            for (int i = 0; i < curRoom.GetCargo().GetTiles().Count; i++)
            {
                //Set the room tile
                roomTile = curRoom.GetCargo().GetTiles()[i];

                //Test different type of collision between the player and tile based on the tile type
                if (roomTile is Spike)
                {
                    //If the player collided with the spike, test if the player is dead
                    if (curPlayer.TestCollision(roomTile.GetHitBox(), true, true, roomTile.GetFrictionMultiplier(), roomTile.GetSpeedMultiplier(), false))
                    {
                        //If the player is going to dir, end the game
                        if (curPlayer.GetHealth() == 0)
                        {
                            //End the game
                            playerDead = true;
                        }
                    }
                }
                else if (roomTile is OneWayPlatform)
                {
                    //Damage and respawn the player if they get hit by the spike
                    curPlayer.TestCollision(curRoom.GetCargo().GetTiles()[i].GetHitBox(), false, true, curRoom.GetCargo().GetTiles()[i].GetFrictionMultiplier(), curRoom.GetCargo().GetTiles()[i].GetSpeedMultiplier(), true);
                }
                else
                {
                    //Test colission between the player and the given tile
                    curPlayer.TestCollision(roomTile.GetHitBox(), false, true, roomTile.GetFrictionMultiplier(), roomTile.GetSpeedMultiplier(), false);
                }

                //Test attack collision with tiles if it hasn't collided with a tile yet
                if (!attackTileCollision)
                {
                    //Let the player pogo off the tile based on its tile type
                    if (roomTile is Spike)
                    {
                        //Test colission between the players attacks and the given tile with pogoing
                        attackTileCollision = curPlayer.TestAttackCollision(roomTile.GetHitBox(), true, true);
                    }
                    else
                    {
                        //Test colission between the players attacks and the given tile without pogoing
                        attackTileCollision = curPlayer.TestAttackCollision(roomTile.GetHitBox(), false, false);
                    }
                }
            }

            //Loop through each enemy and test collision between it and the player
            for (int i = 0; i < curRoom.GetCargo().GetEnemies().Count; i++)
            {
                //Set the room enemy
                roomEnemy = curRoom.GetCargo().GetEnemies()[i];

                //If the player collided with the enemy, check if the player is dead
                if (curPlayer.TestCollision(roomEnemy.GetHitBox(), true, false, 1f, 1f, false))
                {
                    //If the player is going to dir, end the game
                    if (curPlayer.GetHealth() == 0)
                    {
                        //End the game
                        playerDead = true;
                    }
                }

                //Test attack collision between the player and enemy if the enemy isn't dead
                if (roomEnemy.GetHealth() > 0)
                {
                    //Test attack collision between the player and the enemy
                    curPlayer.TestAttackCollision(roomEnemy.GetHitBox(), true, false);
                }

                //Test attack collision between the player and enemy projectiles if the enemy shoots projectiles
                if (roomEnemy is AspidHunter)
                {
                    //Loop through each projectile to test collision between the player and the projectiles
                    for (int j = 0; j < ((AspidHunter)roomEnemy).GetProjectiles().Count; j++)
                    {
                        //If the player collided with the projectile, check if the player is dead
                        if (curPlayer.TestCollision(((AspidHunter)roomEnemy).GetProjectiles()[j].GetHitBox(), true, false, 1f, 1f, false))
                        {
                            //If the player is going to dir, end the game
                            if (curPlayer.GetHealth() == 0)
                            {
                                //End the game
                                playerDead = true;
                            }
                        }
                    }
                }

                //Test colission between the player and each enemies soul particle, and if there was one, remove it
                if (curPlayer.TestParticleCollision(roomEnemy.GetSoulParticleHitBox()))
                {
                    //Remove the enemies soul particle
                    roomEnemy.RemoveSoulParticle();
                }

                //If the enemy is the boss, test collision between the player and the boss's attacks
                if (roomEnemy is Boss)
                {
                    //If the player collided with the bosses attack, check if the player is dead
                    if (curPlayer.TestCollision(((Boss)curRoom.GetCargo().GetEnemies()[0]).GetAttackHitBox(), true, false, 1f, 1f, false))
                    {
                        //If the player is going to dir, end the game
                        if (curPlayer.GetHealth() == 0)
                        {
                            //End the game
                            playerDead = true;
                        }
                    }
                }
            }

            //Test collision between the player and their attack, against each enemy, and then again between the enemy and the player
            curRoom.GetCargo().EnemyCollisionDetection(curPlayer.GetHitBox(), curPlayer.GetCurAttackHitBox());

            //Reset the value of attackTileCollision for the next frame
            attackTileCollision = false;
        }
        //Pre: keyboardStr is the string to add to when the keyboard is pressed, and maxLength is the maximum length the string can be
        //Post: return the new string
        //Desc: Uses the keyboard for the player
        private string UseKeyboard(string keyboardStr, int maxLength)
        {
            //Test for a new key if there is room for it in the string
            if (maxLength > keyboardStr.Length)
            {
                //Add a new key to the string, if a key was pressed
                if (kb.IsKeyDown(Keys.A) && !prevKb.IsKeyDown(Keys.A))
                {
                    //Add an A to the string the user typed
                    keyboardStr += "A";
                }
                else if (kb.IsKeyDown(Keys.B) && !prevKb.IsKeyDown(Keys.B))
                {
                    //Add an B to the string the user typed
                    keyboardStr += "B";
                }
                else if (kb.IsKeyDown(Keys.C) && !prevKb.IsKeyDown(Keys.C))
                {
                    //Add an C to the string the user typed
                    keyboardStr += "C";
                }
                else if (kb.IsKeyDown(Keys.D) && !prevKb.IsKeyDown(Keys.D))
                {
                    //Add an D to the string the user typed
                    keyboardStr += "D";
                }
                else if (kb.IsKeyDown(Keys.E) && !prevKb.IsKeyDown(Keys.E))
                {
                    //Add an E to the string the user typed
                    keyboardStr += "E";
                }
                else if (kb.IsKeyDown(Keys.F) && !prevKb.IsKeyDown(Keys.F))
                {
                    //Add an F to the string the user typed
                    keyboardStr += "F";
                }
                else if (kb.IsKeyDown(Keys.G) && !prevKb.IsKeyDown(Keys.G))
                {
                    //Add an G to the string the user typed
                    keyboardStr += "G";
                }
                else if (kb.IsKeyDown(Keys.H) && !prevKb.IsKeyDown(Keys.H))
                {
                    //Add an H to the string the user typed
                    keyboardStr += "H";
                }
                else if (kb.IsKeyDown(Keys.I) && !prevKb.IsKeyDown(Keys.I))
                {
                    //Add an I to the string the user typed
                    keyboardStr += "I";
                }
                else if (kb.IsKeyDown(Keys.J) && !prevKb.IsKeyDown(Keys.J))
                {
                    //Add an J to the string the user typed
                    keyboardStr += "J";
                }
                else if (kb.IsKeyDown(Keys.K) && !prevKb.IsKeyDown(Keys.K))
                {
                    //Add an K to the string the user typed
                    keyboardStr += "K";
                }
                else if (kb.IsKeyDown(Keys.L) && !prevKb.IsKeyDown(Keys.L))
                {
                    //Add an L to the string the user typed
                    keyboardStr += "L";
                }
                else if (kb.IsKeyDown(Keys.M) && !prevKb.IsKeyDown(Keys.M))
                {
                    //Add an M to the string the user typed
                    keyboardStr += "M";
                }
                else if (kb.IsKeyDown(Keys.N) && !prevKb.IsKeyDown(Keys.N))
                {
                    //Add an N to the string the user typed
                    keyboardStr += "N";
                }
                else if (kb.IsKeyDown(Keys.O) && !prevKb.IsKeyDown(Keys.O))
                {
                    //Add an O to the string the user typed
                    keyboardStr += "O";
                }
                else if (kb.IsKeyDown(Keys.P) && !prevKb.IsKeyDown(Keys.P))
                {
                    //Add an P to the string the user typed
                    keyboardStr += "P";
                }
                else if (kb.IsKeyDown(Keys.Q) && !prevKb.IsKeyDown(Keys.Q))
                {
                    //Add an Q to the string the user typed
                    keyboardStr += "Q";
                }
                else if (kb.IsKeyDown(Keys.R) && !prevKb.IsKeyDown(Keys.R))
                {
                    //Add an R to the string the user typed
                    keyboardStr += "R";
                }
                else if (kb.IsKeyDown(Keys.S) && !prevKb.IsKeyDown(Keys.S))
                {
                    //Add an S to the string the user typed
                    keyboardStr += "S";
                }
                else if (kb.IsKeyDown(Keys.T) && !prevKb.IsKeyDown(Keys.T))
                {
                    //Add an T to the string the user typed
                    keyboardStr += "T";
                }
                else if (kb.IsKeyDown(Keys.U) && !prevKb.IsKeyDown(Keys.U))
                {
                    //Add an U to the string the user typed
                    keyboardStr += "U";
                }
                else if (kb.IsKeyDown(Keys.V) && !prevKb.IsKeyDown(Keys.V))
                {
                    //Add an V to the string the user typed
                    keyboardStr += "V";
                }
                else if (kb.IsKeyDown(Keys.W) && !prevKb.IsKeyDown(Keys.W))
                {
                    //Add an W to the string the user typed
                    keyboardStr += "W";
                }
                else if (kb.IsKeyDown(Keys.X) && !prevKb.IsKeyDown(Keys.X))
                {
                    //Add an X to the string the user typed
                    keyboardStr += "X";
                }
                else if (kb.IsKeyDown(Keys.Y) && !prevKb.IsKeyDown(Keys.Y))
                {
                    //Add an Y to the string the user typed
                    keyboardStr += "Y";
                }
                else if (kb.IsKeyDown(Keys.Z) && !prevKb.IsKeyDown(Keys.Z))
                {
                    //Add an Z to the string the user typed
                    keyboardStr += "Z";
                }
            }
            
            //Delete a key if backspace is pressed
            if (kb.IsKeyDown(Keys.Back) && !prevKb.IsKeyDown(Keys.Back) && keyboardStr.Length > 0)
            {
                //Delete the last character from the string
                keyboardStr = keyboardStr.Substring(0, keyboardStr.Length - 1);
            }

            //Return the new string
            return keyboardStr;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Resets the game
        private void ResetGame()
        {
            //Remake each level in the game
            CreateGameLvls();

            //Reset the game timer
            gameTimer = new Timer(Timer.INFINITE_TIMER, true);

            //Set the starting room
            curRoom = llRoom.GetHead();

            //Reset the current level to reset the completion percentage of the game
            curLvl = 0;

            //Reset all data relating to the player
            curPlayer = new Player(knightAnims, knightAttackAnims, attackAnims, healAnim, healCompleteAnim, hurtAnim, maskFullImg, maskEmptyImg, maskBreakImgs, maskGainImgs, soulGuageImgs, 5, new Vector2(5, 17), knightSnds);

            //Reset the total elapsed time in the game
            ResetElapsedTime();

            //Deactivate the spirit pickup and stop it from being seen anymore
            spiritPickupActive = false;
        }

        //Pre: N/A
        //Post: N/A
        //Desc: Set all of the level names
        private void SetLevelNames()
        {
            //Set the possible level names
            fileNames[(int)EntrancePoint.RIGHT_TOP] = new List<string> {
                "Levels/RIGHT_TOP/Lvl_1_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_2_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_3_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_4_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_5_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_6_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_7_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_8_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_9_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_10_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_11_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_12_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_13_RIGHT_TOP.csv",
                "Levels/RIGHT_TOP/Lvl_14_RIGHT_TOP.csv"
            };
            fileNames[(int)EntrancePoint.RIGHT_BOTTOM] = new List<string> {
                "Levels/RIGHT_BOTTOM/Lvl_1_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_2_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_3_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_4_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_5_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_6_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_7_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_8_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_9_RIGHT_BOTTOM.csv",
                "Levels/RIGHT_BOTTOM/Lvl_10_RIGHT_BOTTOM.csv"
            };
            fileNames[(int)EntrancePoint.LEFT_BOTTOM] = new List<string> {
                "Levels/LEFT_BOTTOM/Lvl_1_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_2_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_3_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_4_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_5_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_6_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_7_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_8_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_9_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_10_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_11_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_12_LEFT_BOTTOM.csv",
                "Levels/LEFT_BOTTOM/Lvl_13_LEFT_BOTTOM.csv"
            };
            fileNames[(int)EntrancePoint.LEFT_TOP] = new List<string> {
                "Levels/LEFT_TOP/Lvl_1_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_2_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_3_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_4_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_5_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_6_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_7_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_8_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_9_LEFT_TOP.csv",
                "Levels/LEFT_TOP/Lvl_10_LEFT_TOP.csv"
            };
            fileNames[(int)EntrancePoint.TOP_LEFT] = new List<string> {
                "Levels/TOP_LEFT/Lvl_1_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_2_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_3_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_4_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_5_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_6_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_7_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_8_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_9_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_10_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_11_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_12_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_13_TOP_LEFT.csv",
                "Levels/TOP_LEFT/Lvl_14_TOP_LEFT.csv"
            };
            fileNames[(int)EntrancePoint.TOP_RIGHT] = new List<string> {
                "Levels/TOP_RIGHT/Lvl_1_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_2_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_3_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_4_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_5_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_6_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_7_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_8_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_9_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_10_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_11_TOP_RIGHT.csv",
                "Levels/TOP_RIGHT/Lvl_12_TOP_RIGHT.csv"
            };
            fileNames[(int)EntrancePoint.BOTTOM_LEFT] = new List<string> {
                "Levels/BOTTOM_LEFT/Lvl_1_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_2_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_3_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_4_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_5_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_6_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_7_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_8_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_9_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_10_BOTTOM_LEFT.csv",
                "Levels/BOTTOM_LEFT/Lvl_11_BOTTOM_LEFT.csv"
            };
            fileNames[(int)EntrancePoint.BOTTOM_RIGHT] = new List<string> {
                "Levels/BOTTOM_RIGHT/Lvl_1_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_2_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_3_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_4_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_5_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_6_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_7_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_8_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_9_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_10_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_11_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_12_BOTTOM_RIGHT.csv",
                "Levels/BOTTOM_RIGHT/Lvl_13_BOTTOM_RIGHT.csv"
            };
        }
    }
}