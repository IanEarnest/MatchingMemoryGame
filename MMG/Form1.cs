using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MMG
{
    public partial class Form1 :Form
    {

        // timer interval was 750
        /* Future
        game timer, 
        sounds for match, not match, hide icons
        bigger board
        timer after first card picked, ends turn
        expand to 20 players?
        */

        /* Player 1 gets stats of game? how to handle quit early?
        Singleplayer game?/ local
        layout set then later set to 0?


        TO CHECK
            start client with already running game(button should open new window, close this one)
        */
        MySQL MySQLConn = new MySQL();
        public bool thisIsClosed;


        // Use this Random object to choose random icons for the squares
        Random random = new Random();
        Color selectedColour;
        Color hiddenColour;
        Color assignedColour;
        int iAmPlayer;
        int playersCount = 3;
        int playerTurn = 1;
        int[] playersScores = new int[6];

        List<string> iconToMake = new List<string>()
        {
            "!", "N", "2", "k", "b", "v", "w", "z", ".", ","
        };

        // Each of these letters is an interesting icon
        // in the Webdings font,
        // and each icon appears twice in this list
        List<string> icons = new List<string>();
        List<string> iconsLayout = new List<string>();
        List<Label> pLabels = new List<Label>();
        // List of clicks, foreach through them to change instead of needing to add more code
        // for each of the clicks?

        // firstClicked points to the first Label control that the player clicks, but it will be null 
        // if the player hasn't clicked a label yet
        Label firstClicked = null;
        // secondClicked points to the second Label control that the player clicks
        Label secondClicked = null;
        Label thirdClicked = null;






        // Database stuff = set score, set table, set turn, set gameClientRunning, setGameAlreadyStarted
        // CheckGame, CheckPlayers, AddPlayer
        // Player 1 gets stats of game? how to handle quit early?
        // layout set then later set to 0?

        // On close, gameRunning = false, 
        // gameClientRunning = while client running, false when no Player 1, otherwise true
        // gameAlreadyStarted = while running (no users can join), false when gameClientRunning is false, and when waiting for users to join, true when started
        // true, true means game is running and players are playing

        public void CloseForm()
        {
            this.Close();
        }

        // Check server running/ Player 1 chosen, other players wait
        public Form1()
        {
            InitializeComponent();
            //init();
        }

        public void init()
        {
            MySQLConn.myForm = this;

            // set thisIsClosed

            MySQLConn.ServerDataResetMessageBox();//SetDefault(); // ServerDataResetMessageBox

            // For "Server" button
            infoLabel.BeginInvoke((Action)(() => {
                if (!thisIsClosed)
                CheckGameRunning();
            }));
            if (thisIsClosed) // Cancel does this
                this.Close();

            // After checking game but before game started
            // Colours - Set colours then make icon list and assign icons to squares
            selectedColour = Color.Black;
            hiddenColour = Color.White; //hiddenColour = this.BackColor;        // testing
            assignedColour = Color.White;  //assignedColour = this.BackColor;   // testing
        }

        public void CheckGameRunning()
        {
            Debug.WriteLine("1. CheckGameRunning");
            // Check server for game running
            MySQLConn.CheckGameState(); // checks gameClientRunning/ gameAlreadyStarted and sets in "gameInfo" variable

            // Stop client if game already started, press restart to reload
            if (MySQLConn.isGameAlreadyStarted) //(MySQL.message.Contains("gameAlreadyStarted=true"))
            {
                Debug.WriteLine("1.2 Game Already Started");
                infoLabel.Text = "Game Already Started!";
                restartBtn.Enabled = true;
                restartBtn.Text = "Restart Client";


                // Disable Form
                //foreach (Label l in Form)
                foreach (Control control in tableLayoutPanel1.Controls)
                {
                    Label iconLabel = control as Label;
                    //iconLabel.Enabled = false; // ERROR?
                }
                Debug.WriteLine("1.2 ICONS DISABLED");
                return;
            }

            // You are player 1
            if (MySQLConn.isGameClientRunning == false)
            {
                Debug.WriteLine("1.2 No game running, you are Player 1");
                // gameClientRunning = while client running (waiting for users to join)
                // false when no Player 1, otherwise true
                // if MySQL.gameInfo

                // Restart enabled for player 1 only
                // Restart is "Start Game" before start

                restartBtn.Enabled = true;
                //restart();
                restartBtn.Text = "Start Game";
                infoLabel.Text = "Waiting for players";

                // add async infoLabel update for players joined
                iAmPlayer = 1;

                // SQL
                MySQLConn.SetIsGameClientRunning(true); // gameClientRunning = true
                MySQLConn.AddPlayer(); // 1 player in game

                // stops from adding players on restart?
                //if (iAmPlayer.ToString() == null)
                //{
                //    iAmPlayer = 1;
                //    MySQLConnection.AddPlayer(); // 1 player in game
                //}
                MessageBox.Show($"You are player {iAmPlayer}");

                // Async - NOT WORKING
                //Debug.WriteLine("Before ASYNC");
                //UpdateWaitingInfo(); // Unable to do ASYNC??
                //Debug.WriteLine("After ASYNC");
                Thread newThread = new Thread(async delegate ()
                {
                    //UpdateWaitingInfo(); // Async?
                    Debug.WriteLine("1.3 WaitingInfo thread started");
                    await Task.Run(() => WaitingInfoP1());
                    //WaitingInfo();
                });
                newThread.Start();
                return;
            }

            // You are player 2/ 3
            else if (MySQLConn.isGameClientRunning)
            {
                Debug.WriteLine("1.2 Game is running, you are other player");
                // if iAmPlayer not set
                //if (iAmPlayer.ToString() == null)
                //{
                //    MySQLConnection.AddPlayer(); // 1 player in game
                //    iAmPlayer = MySQLConnection.playersCount;
                //}

                // SQL
                MySQLConn.AddPlayer();// player = +1 of PlayersCount
                iAmPlayer = MySQLConn.playersCount; // player 2

                MessageBox.Show($"You are player {iAmPlayer}");
                // Every second other players check game has started

                // Asnyc - NOT WORKING
                //GameStartedCheck();
                //StartedCheck2();
                Thread newThread = new Thread(async delegate ()
                {
                    //GameStartedCheck(); // Async?
                    //await StartedCheck();
                    Debug.WriteLine("1.3 StartedCheck thread running");
                    await Task.Run(() => StartedCheck2()); // StartedCheck()
                });
                newThread.Start();
                // Outside loop (Game started)
                //SetupGame();

                // finishes with colours in constructor
            }
        }
        // Check state and players connected
        public async void WaitingInfoP1()
        {
            //Debug.WriteLine("BeginAsync WaitingInfo");

            // New thread
            string waiting = "";
            while (MySQLConn.isGameAlreadyStarted == false)
            {
                Debug.WriteLine(".");
                // not working?
                //Task.Delay(5000).Wait();
                //MySQLConn.CheckGameState
                Debug.WriteLine("1.4 Waiting for Players");
                try
                {
                    if (MySQLConn.conn.State == System.Data.ConnectionState.Closed)
                    {
                        MySQLConn.CheckGameState(); // this after Thread causes Error - cannot connect to server?
                        MySQLConn.CheckPlayersCount();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Exception: {e.Message}");
                }
                Thread.Sleep(4000);

                //if (InvokeRequired)
                //{
                //    //Debug.WriteLine("Waiting InvokeRequired");
                //}
                // Invoke label
                //infoLabel.BeginInvoke(new Action(() => infoLabel.Text = "new text"));
                infoLabel.BeginInvoke((Action)(() => {
                    infoLabel.Text = $"Waiting for Players {waiting}" +
                                 $"\nPlayers = {MySQLConn.playersCount}";
                }));
                waiting += ".";
                Debug.WriteLine(".");
            }
            // Stop waiting for players

            // if invokerequired
            // Invoke label
            //infoLabel.BeginInvoke((Action)(() => {
            //    infoLabel.Text = $"waiting for players ..";
            //}));
            //infoLabel.Text = $"Waiting for Players ..";
            Debug.WriteLine(".EndAsync WaitingInfo");
        }

        // restart/ new game/ restart client button
        public void Restart()
        {
            Debug.WriteLine("2. Start game pressed...");
            // Check state already done in update info thread
            //MySQLConn.CheckGameState(); // maybe do this in WaitingInfoP1 so that state is updated

            // Should be Start pressed - p1 started first?
            //??

            // Game running, cannot join, restart client (restart button), CHANGE TO "Check Game"?
            // Midgame restart for player 1
            if (MySQLConn.isGameAlreadyStarted)
            {
                Debug.WriteLine("2.2 Mid-Game restart/ new form/ quit");
                // Midgame restart
                if (iAmPlayer == 1)
                {
                    MySQLConn.SetDefault();
                    //MySQLConn.NewGame();
                    //MySQLConn.SetIsGameAlreadyStarted(false);
                    Debug.Print("2.2 Restarted");
                    //return;
                }
                // Exit current app to start new app?
                //this.Visible = false;
                //FormIssueTracker mySecondForm = new FormIssueTracker();

                Debug.WriteLine("2.2 New form/ quit");
                // ?Perhaps don't use a new thread
                Thread newThread = new Thread(delegate ()
                {
                    Application.Run(new Form1()); //FormIssueTracker
                });

                newThread.Start();
                this.Close();
                return; // unreachable?
            }

            // Start pressed, Player 1 started, not started game with players //gameAlreadyStarted is false
            if (MySQLConn.isGameClientRunning)
            {
                Debug.WriteLine("2.1 Game Started");
                // Start game/ restart
                infoLabel.Text = "Game Started";

                // MySQLConnection.NewGame 
                MySQLConn.NewGame(); // set playersCount, Scores<list>, playersTurn = 1, isGameAlreadyStarted true
                // thread stop "WaitingInfoP1"
                MySQLConn.CheckGameState(); // update MySQL, dont need if newgame updates state?
                MySQLConn.CheckPlayersCount();
                MySQLConn.CheckScores();
                MySQLConn.CheckTurn();
                playersCount = MySQLConn.playersCount;
                playersScores = MySQLConn.playersScores; // reference?
                playerTurn = MySQLConn.playerTurn;
                // Update Table
                restartBtn.Text = "Restart";

                Debug.WriteLine("2.2 Game Started - New game info set, P1 form set");
                SetupGameP1();
                // At end of game set gameAlreadyStarted=false
                return;
            }

            // other players cannot press restart
        }
        public void SetupGameP1()
        {
            // Local settings
            //playerTurn = 1; // set by NewGame?
            PlayersScoresInit(); // Needed? init playersScores to 0
            AddPLabels(); // Set labels to player count and set each player label to player score
            // Colours - Set colours then make icon list and assign icons to squares
            selectedColour = Color.Black;
            hiddenColour = Color.White; //hiddenColour = this.BackColor;        // testing
            assignedColour = Color.White;  //assignedColour = this.BackColor;   // testing
            // -------------------------Make this board the same for everyone (iconsLayout)-------------------------
            // Setup game board
            MakeListOfIcons();
            AssignIconsToSquares(); // Set grid, random

            // Set P1 label active
            resetPlayerLabelsColour();
            infoLabel.Text = "Game Started! Player 1 turn";
            // Player 1 start/ play
            Debug.WriteLine("3. P1 game setup, Player 1 Start/ play");
        }
        
        // Other player waiting info
        public void StartedCheck2()
        {
            string waiting = "";
            while (MySQLConn.isGameAlreadyStarted == false)
            {
                Debug.WriteLine("Waiting for P1 to start");
                infoLabel.BeginInvoke((Action)(() => {
                    infoLabel.Text = $"Waiting for Player 1 to Start The Game {waiting}" +
                                 $"\nPlayers = {MySQLConn.playersCount}";
                }));
                Thread.Sleep(2000); //4000

                MySQLConn.CheckGameState();
                //infoLabel.BeginInvoke((Action)(() => {
                //    infoLabel.Text = "Your turn!";
                //}));
                MySQLConn.CheckPlayersCount();

                //Task.Delay(1000).Wait();
                // Animated "..."
                waiting += ".";
                //if (waiting.Contains("..."))
                //{
                //    waiting += "";
                //}// exception?
            }
            infoLabel.BeginInvoke((Action)(() => {
                // game started, now what? update info label...
                infoLabel.Text = "Game Started, P1 turn!";
            }));
            // Outside loop (Game started)
            SetupGameOtherPlayer();
        }

        public void SetupGameOtherPlayer()
        {
            // Both already set in P1 Game Started
            //playersCount
            //MySQLConn.CheckGame();
            // Set playersCount, playerTurn, playersScores, playersScores Labels, grid colours, 
            //playersCount = MySQLConn.playersCount; //# mySQL playersCount
            
            // Local settings
            playerTurn = 1;
            MySQLConn.CheckTurn();
            MySQLConn.CheckPlayersCount();
            playersCount = MySQLConn.playersCount;

            // No player scores when starting?
            //playersScores = MySQLConn.playersScores;//new int[playersCount]; // still null??
            PlayersScoresInit(); // Needed? init playersScores to 0
            // p1Label
            infoLabel.BeginInvoke((Action)(() => {
                AddPLabels(); // Set labels to player count and set each player label to player score


                // Colours - Set colours then make icon list and assign icons to squares
                selectedColour = Color.Black;
                hiddenColour = Color.White; //hiddenColour = this.BackColor;        // testing
                assignedColour = Color.White;  //assignedColour = this.BackColor;   // testing
                // -------------------------Make this board the same for everyone (iconsLayout)-------------------------
                // Setup game board
                MakeListOfIcons();
                AssignIconsToSquares(); // Set grid, random


                resetPlayerLabelsColour();

                //infoLabel.BeginInvoke((Action)(() => {
                infoLabel.Text = "Game Started! Player 1 turn";
            }));
            
            // Is this a thread starting a new thread?
            // Async - NOT WORKING
            Thread newThread = new Thread(async delegate ()
            {
                //GameStartedCheck(); // Async?
                //await StartedCheck();
                //await Task.Run(() => StartedCheck()); // CheckingTurn()
                Debug.WriteLine("Checking turn for other player - thread started");
                
                resetPlayerLabelsColour(); //also does - pLabels[MSQLConn.playerTurn - 1].ForeColor = Color.Red;
                // Set colours of table (not clickable)
                SetAllIconsColours(Color.Green); // does not work??
                Debug.WriteLine("-GREEN SET");
                await Task.Run(() => CheckingTurn2());
                
            });
            newThread.Start();
            //CheckingTurn2(); //GameLoop();
            //setScores(); // only after scoring
            //p1Label.Text = "10"; //p2Label.Text = "11";
        }

        /*public async void UpdateWaitingInfo()
        {
            await Task.Run(() => WaitingInfo());
            // Task.Run = background
            //this.BeginInvoke((Action)(() =>
            //{
            //    WaitingInfo();
            //}));
            //await Task.Run(WaitingInfo);
        }
        */
        

        // Waiting for start of game
        /*public async Task GameStartedCheck()
        {
            await Task.Run(StartedCheck);
        }
        */
        /*public async Task StartedCheck()
        {
            string waiting = "";
            while (MySQLConn.isGameAlreadyStarted == false)
            {
                Thread.Sleep(4000);
                MySQLConn.CheckGame();

                infoLabel.BeginInvoke((Action)(() => {
                    infoLabel.Text = $"Waiting for Player 1 to Start The Game {waiting}" +
                                 $"\nPlayers = {MySQLConn.playersCount}";
                }));


                Debug.WriteLine("Waiting for P1 to start");
                //Task.Delay(4000).Wait();
                // Animated "..."
                waiting += ".";
                //if (waiting.Contains("..."))
                //{
                //    waiting += "";
                //}// exception?
            }

            // game started, now what? update info label...
            infoLabel.BeginInvoke((Action)(() => {
                infoLabel.Text = "Game Started, P1 turn!";
            }));

            // Outside loop (Game started)
            //Thread.Sleep(4000);
            SetupGame(); // already?
        }
        */

        // Waiting players for turn
        /*public async Task GameLoop()
        {
            // player waiting turn/ update cards
            // Disable labels?
            await Task.Run(CheckingTurn);
        }
        */
        /*public async Task CheckingTurn()
        {
            while (playerTurn != iAmPlayer) // Not this players turn
            {
                Task.Delay(4000).Wait();
                MySQLConn.CheckGame();
                //Thread.Sleep(1000); ///Task.Delay(3000).Wait();
                Debug.WriteLine("Not my turn, Waiting");



                resetPlayerLabelsColour(); //also does - pLabels[MSQLConn.playerTurn - 1].ForeColor = Color.Red;
                // Set colours of table (not clickable)
                SetAllIconsColours(Color.Green);

                // update table, 
                // update score, check for game stopped?
                playersScores = MySQLConn.playersScores; // ref?

                // Check playerTurn
                playerTurn = MySQLConn.playerTurn; // MySQL (playerTurn=1)

                infoLabel.BeginInvoke((Action)(() => {
                    infoLabel.Text += ".";
                }));

                // Game Ended
                if (MySQLConn.isGameAlreadyStarted == false)
                {
                    // Message box somebody won
                    WinningMessageBox();
                    return; // check this exits both the while and GameLoop()
                }
            }

            // Your Turn! 
            // (outside loop)
            // Enable labels
            // Press labels and score points
            //pLabels[playerTurn - 1].Text += "- (ME)"; // Set scores does this
            infoLabel.BeginInvoke((Action)(() => {
                infoLabel.Text = "Your turn!";
            }));
            resetPlayerLabelsColour();
            SetAllIconsColours(Color.White);
            // SetScores(); //?
        }
        */
        public void CheckingTurn2()
        {
            // if not my turn
            while (playerTurn != iAmPlayer) // Not this players turn
            {
                Debug.WriteLine(".");
                // Update turn, update score, check for game ended
                Debug.WriteLine("Not my turn, Waiting");
                Thread.Sleep(4000); ///Task.Delay(3000).Wait();
                
                // update turn
                MySQLConn.CheckTurn();
                playerTurn = MySQLConn.playerTurn; // MySQL (playerTurn=1)

                // update score, check for game stopped?
                MySQLConn.CheckScores();
                playersScores = MySQLConn.playersScores; // ref?

            infoLabel.BeginInvoke((Action)(() => {
                infoLabel.Text += "."; // Waiting

                SetScoresLocalUI(); // set ui
            }));
                // update table...
                //?

                // Game Ended
                if (MySQLConn.isGameAlreadyStarted == false)
                {
                    // Message box somebody won
                    WinningMessageBox();
                    return; // check this exits both the while and GameLoop()
                }
                SetAllIconsColours(Color.Green);
                Debug.WriteLine(".");
            }

            // Your Turn! - update scores, enable labels
            // (outside loop)
            MySQLConn.CheckScores();
            playersScores = MySQLConn.playersScores;

            resetPlayerLabelsColour();
            SetAllIconsColours(Color.White); // black = disabled


            infoLabel.BeginInvoke((Action)(() => {
                infoLabel.Text = "Your turn!";
            Debug.WriteLine("Your turn!");
            SetScoresLocalUI(); // just for ui

            }));
        }

        public void PlayersScoresInit()
        {
            foreach (int item in playersScores)
            {
                playersScores[item] = 0;
            }
        }
        public void AddPLabels()
        {
            pLabels.Add(p1Label);
            pLabels.Add(p2Label);
            pLabels.Add(p3Label);
            pLabels.Add(p4Label);
            pLabels.Add(p5Label);
            pLabels.Add(p6Label);
            // Reset all values
            for (int i = 0; i <= pLabels.Count-1; i++)
            {
                pLabels[i].Text = "";
            }
            // Needed? // Set each label to player score
            for (int i = 0; i < MySQLConn.playersCount; i++)
            {
                pLabels[i].Text = $"Player {i + 1}: {playersScores[i].ToString()}";
            }
        }
        private void MakeListOfIcons()
        {
            foreach (string icon in iconToMake)
            {
                icons.Add(icon);
                icons.Add(icon);
                icons.Add(icon);

                //iconsLayout.Add(icon);
                //iconsLayout.Add(icon);
                //iconsLayout.Add(icon);
            }
            //{
            //    "!", "!", "!",
            //    "N", "N", "N", 
            //    "2", "2", "2", 
            //    "k", "k", "k",
            //    "b", "b", "b", 
            //    "v", "v", "v", 
            //    "w", "w", "w", 
            //    "z", "z", "z",
            //    ".", ".", ".",
            //    ",", ",", ",",
            //};
        }
        public void resetPlayerLabelsColour()
        {
            // other labels black
            //for (int i = 1; i <= playersCount; i++)
            //{
            //    pLabels[i].ForeColor = Color.Black; // starts at 0??
            //}
            //pLabels[MySQLConn.playerTurn - 1].ForeColor = Color.Red;
            
            for (int i = 0; i < MySQLConn.playersCount; i++)
            {
                pLabels[i].ForeColor = Color.Black; // starts at 0
            }
            pLabels[MySQLConn.playerTurn - 1].ForeColor = Color.Red;
        }
        // Assign each icon from the list of icons to a random square
        private void AssignIconsToSquares()
        {
            // The TableLayoutPanel has 16 labels, and the icon list has 16 icons,
            // so an icon is pulled at random from the list and added to each label
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null && 
                    tableLayoutPanel1.GetColumn(control) != 0 && 
                    tableLayoutPanel1.GetRow(control) != 6)
                {
                    int randomNumber = random.Next(icons.Count);
                    iconLabel.Text = icons[randomNumber];
                    iconLabel.ForeColor = assignedColour;//iconLabel.BackColor; // set invisible

                    //iconsLayout[randomNumber] = icons[randomNumber];
                    icons.RemoveAt(randomNumber);
                }
            }
        }
        private void SetAllIconsColours(Color color)
        {
            // The TableLayoutPanel has 16 labels, and the icon list has 16 icons,
            // so an icon is pulled at random from the list and added to each label
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != 6)
                {
                    iconLabel.ForeColor = color;//iconLabel.BackColor;
                }
            }
        }
        private void SetClickedIconsColour(Color colour)
        {
            firstClicked.ForeColor = colour;
            secondClicked.ForeColor = colour;
            thirdClicked.ForeColor = colour;
        }
        public void SetScoresLocalUI()
        {
            // Update local
            // Set each label to player score
            for (int i = 0; i < MySQLConn.playersCount; i++)
            {
                pLabels[i].Text = $"Player {i + 1}: {playersScores[i].ToString()}";

                // Add "ME" next to your score
                if (i == iAmPlayer - 1)//playerTurn - 1)
                {
                    pLabels[i].Text += $" - (ME)";
                    //pLabels[playerTurn - 1].Text += "- (ME)"
                }
            }
        }
        public void SetScores()
        {
            SetScoresLocalUI();
            MySQLConn.SetScores(playersScores);
            // {playersScores[0]}, {playersScores[1]}, {playersScores[2]

            //foreach (int p in playersScores)
            //{
            //pLabels[player - 1].Text += " - Your Turn";
            //}
        }

        // Every label's Click event is handled by this event handler
        private void label_Click(object sender, EventArgs e)
        {
            // database things
            //MySQLConnection.MySQLConnect(); // MySQL Connection
            //MySQLConnection.SelectAll(); // SelectAll
            //infoLabel.Text = $"Hello: {MySQL.message}";
            //MySQLConnection.Update($"Score:{playersScores[0]}, {playersScores[1]}, {playersScores[2]}"); // P1 score
            //MySQLConnection.SelectAll();
            //infoLabel.Text = $"Hello: {MySQL.message}";

            // Set label clicked?
            //MySQLConnection.Update($"Score: {playersScores[0]}, {playersScores[1]}, {playersScores[2]} " +
            //                            $"gameClientRunning=true, " +
            //                            $"gameAlreadyStarted=false, " +
            //                            $"LabelClicked=null");





            // Player Clicked label

            // The timer is only on after three non-matching icons have been shown to the player, 
            // so ignore any clicks if the timer is running
            if (timer1.Enabled == true)
                return;

            // Clicked
            Label clickedLabel = sender as Label;


            // Check label is clicked
            // if already selected, return
            // if firstClicked, set, return
            // if secondClicked, set, return
            // if thirdClicked, set and check
            // else 3 different icons, reset
            if (clickedLabel != null)
            {
                // If the clicked label is black, the player clicked an icon that's already been revealed --
                // ignore the click
                if (clickedLabel.ForeColor == selectedColour)
                    return;

                // If firstClicked is null, this is the first icon in the trio that the player clicked, 
                // so set firstClicked to the label that the player clicked, change its color to black, and return
                if (firstClicked == null)
                {
                    firstClicked = clickedLabel;
                    firstClicked.ForeColor = selectedColour;
                    //MySQLConnection.Update($"FirstClicked={clickedLabel}");
                    return;
                }

                // If the player gets this far, the timer isn't
                // running and firstClicked isn't null,
                // so this must be the second icon the player clicked
                // Set its color to black
                if (secondClicked == null)
                {
                    secondClicked = clickedLabel;
                    secondClicked.ForeColor = selectedColour;
                    return;
                }


                // Third click
                thirdClicked = clickedLabel;
                thirdClicked.ForeColor = selectedColour;




                // After clicks, check same/ different selected icons

                // 3 same icons
                // If the player clicked three matching icons, keep them black 
                //  and reset firstClicked and secondClicked and thirdClicked
                // so the player can click another icon
                if (firstClicked.Text == secondClicked.Text && 
                    secondClicked.Text == thirdClicked.Text)
                {
                    PlayerScored();
                    return;
                }


                // 3 different icons (This is set in timer)
                //if (firstClicked.Text != null && 
                //    secondClicked.Text != null && 
                //    thirdClicked.Text != null)
                //{
                //    if (firstClicked.Text != secondClicked.Text ||
                //        secondClicked.Text != thirdClicked.Text)
                //    {
                //        firstClicked.ForeColor = this.BackColor;
                //        firstClicked = null;
                //        return;
                //    }
                //}

                // 3 different icons
                // If the player gets this far, the player clicked three different icons,
                // so start the timer (which will wait three quarters of a second, and then hide the icons)
                timer1.Start(); // Hides icons
            }
        }
        private void PlayerScored()
        {
            // This player wins cards
            infoLabel.Text = $"P{playerTurn} scored: {firstClicked.Text} {secondClicked.Text} {thirdClicked.Text}";
            //p1Label.Text += $"{firstClicked.Text} {secondClicked.Text} {thirdClicked.Text}";

            //System.NullReferenceException: 'Object reference not set to an instance of an object.'
            if(playersScores != null)
                playersScores[playerTurn - 1]++; // add to player score
            SetScores();
            RemoveClicked(); // remove text
            ResetClicked(); // reset players clicked cards
            CheckForWinner(); // not working?
        }
        // This timer is started when the player clicks three icons that don't match,
        // so it counts three quarters of a second, and then turns itself off and hides both icons
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            timer1.Stop();

            // Hide all icons
            SetClickedIconsColour(hiddenColour);//firstClicked.BackColor); // all clicked have same backgrounds
            //setClickedIconsColour(this.BackColor); // Same?

            // Reset firstClicked and secondClicked and thirdClicked
            // so the next time a label is clicked will be first click
            ResetClicked();

            // Turn ended 

            // Player 1, 2 ,3
            if (playerTurn < playersCount)
            {
                playerTurn++;
            }
            else
            {
                playerTurn = 1;
            }
            infoLabel.Text = $"Turn: P{playerTurn}";

            resetPlayerLabelsColour();
            //pLabels[player - 1].Text += " - Your Turn";

            // Update tables
            //?

            // Update playerTurn
            MySQLConn.SetPlayerTurn(playerTurn);


            // waiting, not my turn - Async (move partial outside of thread?)
            Thread newThread = new Thread(delegate ()
            {
                Debug.WriteLine("Changing turn, CheckingTurn2 thread started");
                if (playerTurn != iAmPlayer)
                {
                    resetPlayerLabelsColour(); //also does - pLabels[MSQLConn.playerTurn - 1].ForeColor = Color.Red;
                                               // Set colours of table (not clickable)
                    SetAllIconsColours(Color.Green);

                    CheckingTurn2();
                }
            });
            newThread.Start();

            
            //GameLoop(); // always true? non-player-turn cannot access here?
        }
        private void ResetClicked()
        {
            firstClicked = null;
            secondClicked = null;
            thirdClicked = null;
        }
        private void RemoveClicked()
        {
            firstClicked.Text = "";
            secondClicked.Text = "";
            thirdClicked.Text = "";
        }

        // Check every icon to see if it is matched, by comparing its foreground color to its background color. 
        // If all of the icons are matched, the player wins
        private void CheckForWinner()
        {
            Debug.WriteLine("Checking win!");
            // Go through all of the labels in the TableLayoutPanel, checking each one to see if its icon is matched
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != 6)
                {
                    // If any labels are hidden, no win
                    if (iconLabel.ForeColor == hiddenColour)//iconLabel.BackColor)
                        return;
                }
            }

            // If the loop didn't return, it didn't find any unmatched icons
            // That means the user won. Show a message and close the form


            // Game has ended, update all players
            // At end of game set gameAlreadyStarted=false
            // playerTurn = null/ 0
            MySQLConn.SetIsGameAlreadyStarted(false);
            MySQLConn.SetPlayerTurn(0);

            //Close(); // instead of close, wait for restart
            Debug.WriteLine("Someone won!");
            WinningMessageBox();
        }
        // Winner and other players show box
        public void WinningMessageBox()
        {
            // winningMessageBox
            // count player score
            string congrats = "Congratulations! ";
            int winner = 0;
            int winnerScore = 0;
            for (int i = 1; i <= playersCount; i++)
            {
                // Highest score
                if (playersScores[i - 1] > winnerScore)
                {
                    winnerScore = playersScores[i - 1];
                    winner = i;
                }
                congrats += $"P{i}:{playersScores[i - 1]} "; // P1 is score of pScores pos 0
            }
            congrats += $"\nWinner: P{winner} with {winnerScore} points";

            Debug.WriteLine("winningmessagebox!");
            // Disable form for everyone?


            // On End of game
            if (iAmPlayer == 1)
            {
                MySQLConn.SetIsGameClientRunning(false);
                MySQLConn.SetIsGameAlreadyStarted(false);
                CheckGameRunning();
            }
            else
            {
                // wait for players to catch up
                Thread.Sleep(2000);
                CheckGameRunning();
            }


            //Congratulations, P1:, P2:, P3:
            Debug.WriteLine("Show win");
            MessageBox.Show($"{congrats}", "All the icons matched!");
        }


        // Buttons
        private void restartBtn_Click(object sender, EventArgs e)
        {
            Restart();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread newThread = new Thread(delegate ()
            {
                //infoLabel.Text
                
                    init(); // server
                //
            });
            newThread.Start();
        }

        private void newWindowBtn_Click(object sender, EventArgs e)
        {
            //var form2 = new Form1();
            //form2.Show();
            Thread newThread = new Thread(delegate ()
            {
                Application.Run(new Form1());
            });

            newThread.Start();
        }
    }
}
