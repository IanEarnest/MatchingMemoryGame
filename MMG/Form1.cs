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
        // Server checking code = if (MySQLConn.conn.State == System.Data.ConnectionState.Closed)

        bool debug = false; // true, false
        MySQL MySQLConn = new MySQL();
        public bool thisIsClosed;


        // Use this Random object to choose random icons for the squares
        Random random = new Random();
        //Color selectedColour = Color.Black;
        //Color hiddenColour;     // white = debug
        //Color debugColour = Color.Green;
        Color isTurnBGColour;
        Color isNotTurnBGColour = Color.LightCyan; // for player to know not their turn
        Color selectedPlayerColour = Color.Red; // for player to know who's turn is now
        Color iAmPlayerColour = Color.Green; // for player to know which player they are

        // Originals
        //Color oSelectedColour = Color.Black;
        //Color oHiddenColour;


        int iAmPlayer;
        int playersCount = 3;
        int playerTurn = 1;
        int[] playersScores = new int[6];

        List<string> iconToMake = new List<string>()
        {
            // C = box
            //"T", "N", "L", "k", "b", "v", "w", "z", "f", "g"//"," cannot do comma, array is seperated by this when converting to string
            "b", "d", "e", "h", "o", "O", "L", "U", "!", "-"
        };

        // Each of these letters is an interesting icon
        // in the Webdings font,
        // and each icon appears twice in this list
        List<string> gameIcons = new List<string>();
        //List<string> gameIconsLayout = new List<string>();
        List<List<string>> gameIconsLayout = new List<List<string>>();
        

        List<Label> pLabels = new List<Label>();
        // List of clicks, foreach through them to change instead of needing to add more code
        // for each of the clicks?

        // firstClicked points to the first Label control that the player clicks, but it will be null 
        // if the player hasn't clicked a label yet
        Label firstClicked = null;
        // secondClicked points to the second Label control that the player clicks
        Label secondClicked = null;
        Label thirdClicked = null;
        int boardSize = 30;
        int boardSize_1 = 30;
        int boardSize_2 = 60;
        
        // looping through board
        int endRow = 6; // this is more when board is bigger

        string youMessage = "(YOU)";


        // Database stuff = set score, set table, set turn, set gameClientRunning, setGameAlreadyStarted
        // CheckGame, CheckPlayers, AddPlayer
        // Player 1 gets stats of game? how to handle quit early?
        // layout set then later set to 0?

        // On close, gameRunning = false, 
        // gameClientRunning = while client running, false when no Player 1, otherwise true
        // gameAlreadyStarted = while running (no users can join), false when gameClientRunning is false, and when waiting for users to join, true when started
        // true, true means game is running and players are playing

        // Don't need?
        /*public void CloseForm()
        {
            this.Close();
        }*/
        Thread WaitingInfoP1_Thread;    // P1   - updating infoLabel with playersCount, server - playersCount
        Thread StartedCheck2_Thread;    // P2   - updating infoLabel with playersCount, server - gameState, playersCount
                                        // - waiting for game start from P1 
        Thread CheckingTurn2_Thread;    // Player   - waiting for end of turn, check for winner, server - gameState, checkTurn, checkScores
                                        // - updating infoLabel, setUI 
        Thread ChangingTurn_Thread;     // Player   - end of turn, changing, update UI, green icons + CheckingTurn2
        Thread NewForm_Thread;          // (P2) restartBtn      - start new form, close old 
        Thread Init_thread;             // (debug) serverBtn    - message box/ reset server 
        Thread NewWindow_thread;        // (debug) newWindowBtn - start new form
                                        //WaitingInfoP1_Thread.isbackground?
        Thread GameEnded_Thread;
        //Thread GameEnded_Thread2;
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // still error in 
            //Exception thrown: 'System.Threading.ThreadInterruptedException' in System.Private.CoreLib.dll
            try
            {

            
            //1.4 Waiting for Players thread is still going??
            Debug.WriteLine("Closing... threads first check");
            if (WaitingInfoP1_Thread != null)
                Debug.WriteLine($"{WaitingInfoP1_Thread.IsAlive} - WaitingInfoP1_Thread alive?");
            if (StartedCheck2_Thread != null)
                Debug.WriteLine($"{StartedCheck2_Thread.IsAlive} - StartedCheck2_Thread alive?");
            if (CheckingTurn2_Thread != null)
                Debug.WriteLine($"{CheckingTurn2_Thread.IsAlive} - CheckingTurn2_Thread alive?");
            if (ChangingTurn_Thread != null)
                Debug.WriteLine($"{ChangingTurn_Thread.IsAlive} - ChangingTurn_Thread alive?");
            //if (NewForm_Thread != null)         Debug.WriteLine($"{NewForm_Thread.IsAlive} - NewForm_Thread alive?");
            if (Init_thread != null)
                Debug.WriteLine($"{Init_thread.IsAlive} - Init_thread alive?");
            //if (NewWindow_thread != null)       Debug.WriteLine($"{NewWindow_thread.IsAlive} - NewWindow_thread alive?");
            //GameEnded_Thread

            Debug.WriteLine("Closing... interrupting");
            // thread still being interrupted and causing exception??
            try
            {
                if (WaitingInfoP1_Thread != null)
                    WaitingInfoP1_Thread.Interrupt(); //.Abort
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Thread exception: {ex.Message}");
            }
            try
            {
                if (StartedCheck2_Thread != null)
                    StartedCheck2_Thread.Interrupt();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Thread exception: {ex.Message}");
            }
            try
            {
                if (CheckingTurn2_Thread != null)
                    CheckingTurn2_Thread.Interrupt();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Thread exception: {ex.Message}");
            }
            try
            {
                if (ChangingTurn_Thread != null)
                    ChangingTurn_Thread.Interrupt();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Thread exception: {ex.Message}");
            }
            try
            {
                if (Init_thread != null)
                    Init_thread.Interrupt();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Thread exception: {ex.Message}");
            }
            /*
            if (WaitingInfoP1_Thread != null)
                WaitingInfoP1_Thread.Interrupt(); //.Abort
            if (StartedCheck2_Thread != null)
                StartedCheck2_Thread.Interrupt();
            if (CheckingTurn2_Thread != null)
                CheckingTurn2_Thread.Interrupt();
            if (ChangingTurn_Thread != null)
                ChangingTurn_Thread.Interrupt();
            //if (NewForm_Thread != null) NewForm_Thread.Interrupt();
            if (Init_thread != null)
                Init_thread.Interrupt();
            //if (NewWindow_thread != null) NewWindow_thread.Interrupt();
            */

            Debug.WriteLine("Closing... threads second check");
            if (WaitingInfoP1_Thread != null)
                Debug.WriteLine($"{WaitingInfoP1_Thread.IsAlive} - WaitingInfoP1_Thread alive?");
            if (StartedCheck2_Thread != null)
                Debug.WriteLine($"{StartedCheck2_Thread.IsAlive} - StartedCheck2_Thread alive?");
            if (CheckingTurn2_Thread != null)
                Debug.WriteLine($"{CheckingTurn2_Thread.IsAlive} - CheckingTurn2_Thread alive?");
            if (ChangingTurn_Thread != null)
                Debug.WriteLine($"{ChangingTurn_Thread.IsAlive} - ChangingTurn_Thread alive?");
            //if (NewForm_Thread != null)         Debug.WriteLine($"{NewForm_Thread.IsAlive} - NewForm_Thread alive?");
            if (Init_thread != null)
                Debug.WriteLine($"{Init_thread.IsAlive} - Init_thread alive?");
            //if (NewWindow_thread != null)       Debug.WriteLine($"{NewWindow_thread.IsAlive} - NewWindow_thread alive?");
            Debug.WriteLine("..Threads closed");

            // InfoLabel remove invoke (not using?)
            // Invokes (not beginInvokes) create deadlocks, use dispose(waitHandle)?
            if (IsHandleCreated)
            {
                if (InvokeRequired)
                {
                    // infoLabel
                    //invokeInfoLabel("Quitting");
                    IAsyncResult myResult = infoLabel.BeginInvoke((Action)(() =>
                    {
                        infoLabel.Text = "Quitting";
                    }));
                    Debug.WriteLine("infoLabel Invoke removed");
                    infoLabel.EndInvoke(myResult);
                }
            }
            // try/catch EndInvoke
            //private delegate Bitmap EmbossDelegate(Bitmap bm);
            //EmbossDelegate caller = Emboss;
            //IAsyncResult result1 = caller.BeginInvoke(Images[0], null, null);
            //pictureBox1.Image = caller.EndInvoke(result1);

            //StartedCheck2_Thread.Interrupt();
            //CheckingTurn2_Thread.Interrupt();
            //ChangingTurn_Thread.Interrupt();
            //NewForm_Thread.Interrupt();
            //init_thread.Interrupt();
            //newWindow_thread.Interrupt();

            // reset server
            if (iAmPlayer == 1)
            {
                Debug.WriteLine($"Setting defaults on server before closing");
                MySQLConn.SetDefault();
            }

                // Try for second window closing issue
                /*try
                {
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Thread exception: {ex.Message}");
                }*/

                // Debug
                /*if (WaitingInfoP1_Thread != null)
                {
                    Debug.WriteLine($"{WaitingInfoP1_Thread.IsAlive} - WaitingInfoP1_Thread IsAlive");
                    WaitingInfoP1_Thread.Interrupt();

                    _shutdownEvent.Set();
                    WaitingInfoP1_Thread.Join();
                    Debug.WriteLine("Thread Stopped ");
                    //WaitingInfoP1_Thread.Suspend();
                    try
                    {
                        Debug.WriteLine($"{WaitingInfoP1_Thread.ThreadState} - WaitingInfoP1_Thread ThreadState");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Thread exception: {ex.Message}");
                    }
                    try
                    {
                        Debug.WriteLine($"{WaitingInfoP1_Thread.IsBackground} - WaitingInfoP1_Thread IsBackground");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Thread exception: {ex.Message}");
                    }
                    try
                    {
                        Debug.WriteLine($"{WaitingInfoP1_Thread.IsThreadPoolThread} - WaitingInfoP1_Thread IsThreadPoolThread");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Thread exception: {ex.Message}");
                    }

                }*/
                //Process.GetCurrentProcess().CloseMainWindow(); // normal
                //Application.ExitThread(); // normal
                //Process.GetCurrentProcess().Dispose(); // nothing?
                //Process.GetCurrentProcess().Kill(true); // quits everything, Cannot be used to terminate a process tree containing the calling process

                //Environment.Exit(Environment.ExitCode); // quits everything
                //(persistent thread = Environment.Exit instead of Application.Exit)
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"On Form Closing Exception: {ex.Message}");
            }
            Debug.WriteLine($"Program closed: reason = {e.CloseReason}");
            base.OnFormClosing(e);
        }
        ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        //protected virtual void OnFormClosing
        //private void Form1_FormClosing(Object sender, FormClosingEventArgs e)




        // Check server running/ Player 1 chosen, other players wait
        public Form1()
        {
            InitializeComponent();
            //init();
            formDefaults();
        }
        public void formDefaults()
        {
            this.Text = "Memory Matching Game";
            //info label
            infoLabel.Text = "Welcome, press start/ join";
            //restart button (Start Game)
            restartBtn.Text = "Start Game";
            //p1-p6 label
            p1Label.Text = "Player 1";
            p2Label.Text = "";
            p3Label.Text = "";
            p4Label.Text = "";
            p5Label.Text = "";
            p6Label.Text = "";
            gameSizePanel.Visible = true;
            serverBtnsPanel.Visible = true;

            // Colours - other defaults set above
            //hiddenColour = label1.BackColor; // label1 = first clickable spot
            //oHiddenColour = hiddenColour; //this.BackColor;
            //debugColour = hiddenColour;
            isTurnBGColour = tableLayoutPanel1.BackColor;//this.BackColor; // white??
            Debug.WriteLine($"isTurnBGColour = {isTurnBGColour.Name}");
            if (debug)
            {
                //Debug.WriteLine("-Debug colours used");
                //hiddenColour = Color.White; // Color.White = debug
                //oHiddenColour = hiddenColour;
                //debugColour = Color.Green;
            }

            disableForm(); // both players start without able to click on form
        }
        // This is inside a thread
        public void init()
        {
            MySQLConn.myForm = this; // Message box can close form

            // set thisIsClosed?

            MySQLConn.ServerDataResetMessageBox();//SetDefault(); // ServerDataResetMessageBox

            // at top?
            if (thisIsClosed) // Cancel does this?
                this.Close();

            // For "Server" button
            // App loading, set default?
            //isTurnBGColour = this.BackColor; // white??
            //Debug.WriteLine($"isTurnBGColour = {isTurnBGColour.Name}");

            if (InvokeRequired)
            {
                // Using infoLabel beginInvoke to update all UI on a thread
                infoLabel.BeginInvoke((Action)(() =>
                {
                    if (!thisIsClosed)
                        CheckGameRunning();
                }));
            }
            else
            {
                if (!thisIsClosed)
                    CheckGameRunning();
            }
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
                p1Label.Text = "";
                gameSizePanel.Visible = false;
                serverBtnsPanel.Visible = false;

                // already disabled?
                // Disable Form
                //foreach (Label l in Form)
                //foreach (Control control in tableLayoutPanel1.Controls)
                //{
                //    Label iconLabel = control as Label;
                //    //iconLabel.Enabled = false; // ERROR?
                //}
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

                // UI
                //RBtnSize30.Visible
                //gameSizePanel visible
                //serverBtnsPanel visible

                restartBtn.Enabled = true;
                //restart();
                restartBtn.Text = "Start Game";
                infoLabel.Text = "Waiting for players";

                // add async infoLabel update for players joined
                iAmPlayer = 1;
                this.Text += $" - Player {iAmPlayer}"; // form title

                // SQL
                MySQLConn.SetIsGameClientRunning(true); // gameClientRunning = true
                MySQLConn.AddPlayer(); // 1 player in game
                // local
                isGameClientRunning = true;

                // stops from adding players on restart?
                //if (iAmPlayer.ToString() == null)
                //{
                //    iAmPlayer = 1;
                //    MySQLConnection.AddPlayer(); // 1 player in game
                //}
                //MessageBox.Show($"You are player {iAmPlayer}");

                // Async - NOT WORKING
                //Debug.WriteLine("Before ASYNC");
                //UpdateWaitingInfo(); // Unable to do ASYNC??
                //Debug.WriteLine("After ASYNC");
                WaitingInfoP1_Thread = new Thread(async delegate ()
                {
                    //UpdateWaitingInfo(); // Async?
                    Debug.WriteLine("1.3 WaitingInfo thread started");
                    await Task.Run(() => WaitingInfoP1());
                    //WaitingInfo();
                });
                WaitingInfoP1_Thread.Start();
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

                // UI
                gameSizePanel.Visible = false;
                serverBtnsPanel.Visible = false;

                // SQL
                MySQLConn.AddPlayer();// player = +1 of PlayersCount
                iAmPlayer = MySQLConn.playersCount; // player 2
                this.Text += $" - Player {iAmPlayer}"; // form title

                //MessageBox.Show($"You are player {iAmPlayer}");
                // Every second other players check game has started

                // Asnyc - NOT WORKING
                //GameStartedCheck();
                //StartedCheck2();
                StartedCheck2_Thread = new Thread(async delegate ()
                {
                    //GameStartedCheck(); // Async?
                    //await StartedCheck();
                    Debug.WriteLine("1.3 StartedCheck thread running");
                    await Task.Run(() => StartedCheck2()); // StartedCheck()
                });
                StartedCheck2_Thread.Start();
                // Outside loop (Game started)
                //SetupGame();

                // finishes with colours in constructor
            }
        }
        // Check state and players connected
        public Task WaitingInfoP1() // async
        {
            //Debug.WriteLine("BeginAsync WaitingInfo");

            // Thread
            int waiting = 0;
            while (isGameAlreadyStarted == false) //MySQLConn.isGameAlreadyStarted == false)
            {
                Debug.WriteLine(".");
                Debug.WriteLine("1.4 Waiting for Players");

                //if (InvokeRequired)
                // Invoke label
                //if (IsHandleCreated)
                // Always synchronous.  (But you must watch out for cross-threading deadlocks!)
                invokeInfoLabel($"You are Player {iAmPlayer} of {MySQLConn.playersCount}" +
                                $"\nWaiting for players to join" +
                                $"\nYou have been waiting for {waiting} seconds in the lobby");
                if (InvokeRequired)
                {
                    infoLabel.BeginInvoke((Action)(() =>
                    {
                        PlayersScoresWaiting(); // update UI as players join
                    }));
                }
                else
                {
                    PlayersScoresWaiting();
                }

                //if (InvokeRequired)
                //{
                //    infoLabel.BeginInvoke((Action)(() =>
                //    {
                //        infoLabel.Text = $"Waiting for Players {waiting}" +
                //                     $"\nPlayers = {MySQLConn.playersCount}";
                //    }));
                //}
                // not working?
                //Task.Delay(5000).Wait();
                //MySQLConn.CheckGameState
                bool isFinished = false;
                while (isFinished == false)
                {
                    try
                    {
                        if (MySQLConn.conn.State == System.Data.ConnectionState.Closed)
                        {
                            MySQLConn.CheckPlayersCount();
                            isFinished = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Exception: {e.Message}");
                        Thread.Sleep(1000);
                        Debug.WriteLine($"try again");
                        Thread.Sleep(1000);
                    }
                }

                //try
                //{
                //    if (MySQLConn.conn.State == System.Data.ConnectionState.Closed)
                //    {
                //        //MySQLConn.CheckGameState(); // this after Thread causes Error - cannot connect to server?
                //        MySQLConn.CheckPlayersCount();
                //    }
                //}
                //catch (Exception e)
                //{
                //    Debug.WriteLine($"Exception: {e.Message}");
                //}

                // a repeat to update quicker
                invokeInfoLabel($"You are Player {iAmPlayer} of {MySQLConn.playersCount}" +
                                $"\nWaiting for players to join" +
                                $"\nYou have been waiting for {waiting} seconds in the lobby");
                // Stops "Handle required" exception
                //if (InvokeRequired)
                //{
                //    // second update after
                //    infoLabel.BeginInvoke((Action)(() =>
                //    {
                //        infoLabel.Text = $"Waiting for Players {waiting}" +
                //                     $"\nPlayers = {MySQLConn.playersCount}";
                //    }));
                //}
                //Thread.Sleep(4000);
                Task.Delay(4000).Wait();


                waiting++; //+= ".";
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
            return Task.CompletedTask;
        }
        bool isGameAlreadyStarted = false; // used only for WaitingInfo
        bool isGameClientRunning = false; // used only for P1

        // restart/ new game/ restart client button
        public void Restart()
        {
            //MySQLConn.CheckGameState(); // no longer checked in WaitingInfo

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
                    bool isFinished = false;

                    while (isFinished == false)
                    {
                        try
                        {
                            if (MySQLConn.conn.State == System.Data.ConnectionState.Closed)
                            {
                                MySQLConn.SetDefault();
                                Debug.WriteLine("Midgame restart - finished");
                                isFinished = true;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"Midgame restart - Exception: {e.Message}");
                            Thread.Sleep(1000);
                            Debug.WriteLine($"Midgame restart - try again");
                            Thread.Sleep(1000);
                        }
                    }
                    
                    //MySQLConn.NewGame();
                    //MySQLConn.SetIsGameAlreadyStarted(false);
                    Debug.Print("2.2 Restarted");
                    //return;
                }
                // Exit current app to start new app?
                //this.Visible = false;
                //FormIssueTracker mySecondForm = new FormIssueTracker();

                // Player 1/ 2 restart
                Debug.WriteLine("2.2 New form/ quit");
                // ?use task instead of thread?
                NewForm_Thread = new Thread(delegate ()
                {
                    Application.Run(new Form1()); //FormIssueTracker
                });

                NewForm_Thread.Start();
                this.Close();
                return; // unreachable?
            }

            // Start pressed, Player 1 started, not started game with players //gameAlreadyStarted is false
            if (isGameClientRunning)//MySQLConn.isGameClientRunning)
            {
                Debug.WriteLine("2.1 Game Started");

                // thread stop updating infoLabel
                //WaitingInfoP1Thread.Abort();// Not in .NET Core
                //WaitingInfoP1Thread.Interrupt();
                //Debug.WriteLine("Thread stopped - WaitingInfoP1Thread");
                isGameAlreadyStarted = true; // set false at end of game?
                // wait?

                // Start game/ restart
                infoLabel.Text = "Game Started";
                Debug.WriteLine($"2.2 Board Size - {boardSize}");
                // UI
                gameSizePanel.Visible = false;//gameSizePanel not visible
                                              //serverBtnsPanel visible

                // set layout for all players first, then start game
                MakeListOfIcons();
                AssignIconsToSquares(); // Set grid, random
                ShareLayout();

                // Start new game
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

            // Enable form
            //if (InvokeRequired) infoLabel.BeginInvoke((Action)(() => disableForm()));
            enableForm();

            //playerTurn = 1; // set by NewGame?
            PlayersScoresInit(); // Needed? init playersScores to 0
            AddPLabels(); // Set labels to player count and set each player label to player score
            // Colours - Set colours then make icon list and assign icons to squares
            // .. removed

            // -------------------------Make this board the same for everyone (iconsLayout)-------------------------
            // Setup game board
            //MakeListOfIcons();
            //AssignIconsToSquares(); // Set grid, random
            //ShareLayout();

            SetAllActiveCardsIcons(); // set all cards to background ([)

            resetPlayerLabelsColour(); //pLabels
            SetScoresLocalUI(); // adds (ME)

            // Set P1 label active - labels not updating, need invoke
            invokeInfoLabel($"Game Started! Player 1 turn");
            //if (InvokeRequired)
            //{
            //    infoLabel.BeginInvoke((Action)(() =>
            //    {
            //        infoLabel.Text = $"Game Started! Player 1 turn";
            //    }));
            //}
            //UpdateUI(); // doesn't work
            //infoLabel.Text = "Game Started! Player 1 turn"; // needs invoke to update

            if (debug)
            {
                ShowAllIcons();
                //SetAllIconsBG(debugColour); // UI for player waiting (not showing icons)
                //SetAllIconsColours(debugColour);
            }

            // Player 1 start/ play
            Debug.WriteLine("3. P1 game setup, Player 1 Start/ play");
        }

        // Other player waiting info
        public void StartedCheck2()
        {
            int waiting = 0;
            while (MySQLConn.isGameAlreadyStarted == false)
            {
                Debug.WriteLine("Waiting for P1 to start");

                invokeInfoLabel($"You are Player {iAmPlayer} of {MySQLConn.playersCount}" +
                                $"\nWaiting for Player 1 to Start The Game" +
                                $"\nYou have been waiting for {waiting} seconds in the lobby");
                if (InvokeRequired)
                {
                    infoLabel.BeginInvoke((Action)(() =>
                    {
                        PlayersScoresWaiting(); // update UI as players join
                    }));
                }
                else
                {
                    PlayersScoresWaiting();
                }
                if (InvokeRequired) infoLabel.BeginInvoke((Action)(() => restartBtn.Visible = false));
                //if (InvokeRequired)
                //{
                //    infoLabel.BeginInvoke((Action)(() =>
                //    {
                //        infoLabel.Text = $"Waiting for Player 1 to Start The Game {waiting}" +
                //                     $"\nPlayers = {MySQLConn.playersCount}";
                //    }));
                //}
                Thread.Sleep(2000); //4000

                MySQLConn.CheckGameState();
                //infoLabel.BeginInvoke((Action)(() => {
                //    infoLabel.Text = "Your turn!";
                //}));
                MySQLConn.CheckPlayersCount();

                //Task.Delay(1000).Wait();
                // Animated "..."
                waiting++; //+= ".";
                //if (waiting.Contains("..."))
                //{
                //    waiting += "";
                //}// exception?
            }

            // game started, now what? update info label...
            invokeInfoLabel("Game Started, P1 turn!");
            //if (InvokeRequired)
            //{
            //    infoLabel.BeginInvoke((Action)(() =>
            //    {
            //        infoLabel.Text = "Game Started, P1 turn!";
            //    }));
            //}
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

            // set ui first?

            // Disable form, already done?
            //if (InvokeRequired) infoLabel.BeginInvoke((Action)(() => disableForm()));
            //if (InvokeRequired) infoLabel.BeginInvoke((Action)(() => restartBtn.Visible = false));

            // Local settings
            playerTurn = 1;
            MySQLConn.CheckTurn();
            MySQLConn.CheckPlayersCount();
            playersCount = MySQLConn.playersCount;

            // No player scores when starting?
            //playersScores = MySQLConn.playersScores;//new int[playersCount]; // still null??
            PlayersScoresInit(); // Needed? init playersScores to 0
            // p1Label

            invokeInfoLabel("Game Started! Player 1 turn");
            if (InvokeRequired)
            {
                infoLabel.BeginInvoke((Action)(() =>
                {
                    AddPLabels(); // Set labels to player count and set each player label to player score


                    // Colours - Set colours then make icon list and assign icons to squares
                    // .. removed
                    // -------------------------Make this board the same for everyone (iconsLayout)-------------------------
                    // Setup game board
                    //MakeListOfIcons();
                    //AssignIconsToSquares(); // Set grid, random
                    GetLayoutAndSetup();
                    SetAllActiveCardsIcons(); // set all cards to background ([)

                    resetPlayerLabelsColour(); // set twice?

                    SetAllIconsBG(isNotTurnBGColour);
                    //SetAllIconsColours(bgColour);
                    if (debug)
                    {
                        ShowAllIcons();
                        //SetAllIconsColours(debugColour); // here and under in thread?
                    }
                    //infoLabel.Text = "Game Started! Player 1 turn";
                }));
            }
            else
            {
                AddPLabels();
                //selectedColour... removed
                MakeListOfIcons();
                AssignIconsToSquares();
                SetAllActiveCardsIcons(); // set all cards to background ([)
                resetPlayerLabelsColour(); // error
                SetAllIconsBG(isNotTurnBGColour);
                //SetAllIconsColours(bgColour);
                if (debug)
                {
                    ShowAllIcons();
                    //SetAllIconsColours(debugColour);
                }
            }

            // Is this a thread starting a new one?
            // Async - NOT WORKING
            CheckingTurn2_Thread = new Thread(async delegate ()
            {
                //GameStartedCheck(); // Async?
                //await StartedCheck();
                //await Task.Run(() => StartedCheck()); // CheckingTurn()
                Debug.WriteLine("Checking turn for other player - thread started");

                //resetPlayerLabelsColour(); //also does - pLabels[MSQLConn.playerTurn - 1].ForeColor = Color.Red;
                // Set colours of table (not clickable)
                //SetAllIconsColours(Color.Green); // does not work??
                await Task.Run(() => CheckingTurn2());

            });
            CheckingTurn2_Thread.Start();
            //CheckingTurn2(); //GameLoop();
            //setScores(); // only after scoring
            //p1Label.Text = "10"; //p2Label.Text = "11";
        }
        private void invokeInfoLabel(string message)
        {
            if (InvokeRequired)
            {
                infoLabel.BeginInvoke((Action)(() =>
                {
                    infoLabel.Text = message;
                }));
            }
            else
            {
                infoLabel.Text = message;
            }
        }
        private void invokeFormEnable(bool enable)
        {
            if (InvokeRequired)
            {
                infoLabel.BeginInvoke((Action)(() =>
                {
                    if (enable)
                    {
                        enableForm();
                    }
                    else
                    {
                        disableForm();
                    }
                }));
            }
            else
            {
                if (enable)
                {
                    enableForm();
                }
                else
                {
                    disableForm();
                }
            }
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
            int waiting = 0;
            int clicked = 1;
            string labelClicked1 = "";
            string labelClicked2 = "";
            string labelClicked3 = "";
            while (playerTurn != iAmPlayer) // Not this players turn
            {
                Debug.WriteLine(".");
                // Update turn, update score, check for game ended
                Debug.WriteLine("Not my turn, Waiting"); // not your turn

                invokeInfoLabel($"Please wait..." +
                                $"\nPlayer {MySQLConn.playerTurn}'s turn" +
                                $"\nYou have waited {waiting} seconds for your turn");
                waiting++;
                // Disable form
                //if (InvokeRequired) infoLabel.BeginInvoke((Action)(() => disableForm()));
                invokeFormEnable(false);

                Thread.Sleep(2000); ///Task.Delay(3000).Wait();

                // check other player has clicked, get name, check which click, show on board
                string labelClicked = MySQLConn.CheckClickedLabel();
                // More than 3 clicked, not show all cards clicked (if other player scores)
                if (InvokeRequired)
                {
                    infoLabel.BeginInvoke((Action)(() =>
                    {
                        if (clicked > 3)
                        {
                            clicked = 1;
                            labelClicked1 = "";
                            labelClicked2 = "";
                            labelClicked3 = "";
                            Debug.WriteLine($"label > 3 clicked, resetting");
                        }
                        if (labelClicked != "")
                        {
                            // label not empty
                            // Show card

                            if (clicked == 1 && labelClicked1 == "") //?
                            {
                                labelClicked1 = labelClicked;
                                ShowIconOnGame(labelClicked1, "");
                                Debug.WriteLine($"label 1 clicked");
                                clicked++; // where to put clicked? other player keeps checking clicked
                            }
                            else if (clicked == 2 && labelClicked2 == "" && 
                            labelClicked1 != labelClicked)
                            {
                                labelClicked2 = labelClicked;
                                ShowIconOnGame(labelClicked1, "");
                                ShowIconOnGame(labelClicked2, "");
                                Debug.WriteLine($"label 2 clicked");
                                clicked++;
                            }
                            else if (clicked == 3 && labelClicked3 == "" && 
                            labelClicked1 != labelClicked &&
                            labelClicked2 != labelClicked)
                            {
                                labelClicked3 = labelClicked;
                                ShowIconOnGame(labelClicked1, "");
                                ShowIconOnGame(labelClicked2, "");
                                ShowIconOnGame(labelClicked3, "");
                                //clicked++;
                                UpdateUI();
                                // UpdateUI(label1)
                                // wait a second, then hide them // doesn't work
                                Thread.Sleep(2000); 
                                // hide card... clicked1, clicked2, clicked3
                                HideIconOnGame(labelClicked1);
                                HideIconOnGame(labelClicked2);
                                HideIconOnGame(labelClicked3);
                                SetAllActiveCardsIcons();
                                Debug.WriteLine($"label 3 clicked, resetting");
                                clicked = 1;
                                labelClicked1 = "";
                                labelClicked2 = "";
                                labelClicked3 = "";
                                MySQLConn.SetClickedLabel("");
                            }
                            
                        }
                        else
                        {
                            // no label clicked
                            Debug.WriteLine($"No label clicked");
                        }
                    })); // outside label1 invoke
                } // else, no invoke, repeat code?
                /*
                    Server - clickedLabel
	                Server - matchingGame_data
	                SQL - CheckClickedLabel
	                SQL - SetClickedLabel
                */


                // update turn
                MySQLConn.CheckTurn();
                playerTurn = MySQLConn.playerTurn; // MySQL (playerTurn=1)

                // update score, check for game stopped?
                MySQLConn.CheckScores();
                playersScores = MySQLConn.playersScores; // ref?

                // works?
                invokeInfoLabel(infoLabel.Text + ".");
                if (InvokeRequired)
                {
                    infoLabel.BeginInvoke((Action)(() =>
                    {
                        //infoLabel.Text += "."; // Waiting
                        SetScoresLocalUI(); // set ui

                        SetAllIconsBG(isNotTurnBGColour); // UI for player waiting (not showing icons)
                        //SetAllIconsColours(isNotTurnBGColour);
                        if (debug)
                        {
                            ShowAllIcons();
                            //SetAllIconsColours(debugColour); // moved up to see quicker
                        }
                    }));
                }
                else
                {
                    //infoLabel.Text += "."; // Waiting
                    SetScoresLocalUI(); // set ui

                    SetAllIconsBG(isNotTurnBGColour);
                    //SetAllIconsColours(isNotTurnBGColour);
                    if (debug)
                    {
                        ShowAllIcons();
                        //SetAllIconsColours(debugColour); // moved up to see quicker
                    }
                }
                // update table...
                //?
                //SetAllIconsBG(isNotTurnBGColour);
                ////SetAllIconsColours(isNotTurnBGColour);
                //if (debug)
                //{
                //    ShowAllIcons();
                //    //SetAllIconsColours(debugColour); // moved up to see quicker
                //}

                MySQLConn.CheckGameState();
                // Game Ended
                if (MySQLConn.isGameAlreadyStarted == false || MySQLConn.playerTurn == 0)
                {
                    // Message box somebody won

                    if (InvokeRequired)
                    {
                        infoLabel.BeginInvoke((Action)(() =>
                        {
                            SetAllIconsNull(); // label25
                            WinningMessageBox(); // this after icons removed?
                        }));
                    }
                    else
                    {
                        SetAllIconsNull(); // label25
                        WinningMessageBox(); // this after icons removed?
                    }
                    Debug.WriteLine("OTHER PLAYER WON!");
                    return; // check this exits both the while and GameLoop()
                }
                Debug.WriteLine(".");
            }

            // Your Turn! - update scores, enable labels
            // (outside loop)

            // Enable form
            invokeFormEnable(true);
            //if (InvokeRequired) infoLabel.BeginInvoke((Action)(() => enableForm()));

            // check connection "System.InvalidOperationException: 'Connection must be valid and open.'"
            bool isFinished = false;
            while (isFinished == false)
            {
                try
                {
                    if (MySQLConn.conn.State == System.Data.ConnectionState.Closed)
                    {
                        MySQLConn.CheckScores();
                        isFinished = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Exception: {e.Message}");
                    Thread.Sleep(1000);
                    Debug.WriteLine($"try again");
                    Thread.Sleep(1000);
                }
            }
            playersScores = MySQLConn.playersScores;
            if (InvokeRequired)
            {
                infoLabel.BeginInvoke((Action)(() =>
                {
                    SetAllActiveCardsIcons();
                    resetPlayerLabelsColour();
                    //SetAllIconsColours(Color.White); // black = disabled
                    SetAllIconsBG(isTurnBGColour);
                    //SetAllIconsColours(hiddenColour);
                    // debug set colour?
                    SetAllActiveCardsIcons();
                    if (debug)
                    {
                        ShowAllIcons();
                        //SetAllIconsColours(debugColour);
                    }
                }));
            }
            else
            {
                SetAllActiveCardsIcons();
                resetPlayerLabelsColour();
                //SetAllIconsColours(Color.White); // black = disabled
                SetAllIconsBG(isTurnBGColour);
                SetAllActiveCardsIcons();// hide
                //SetAllIconsColours(hiddenColour);
                // debug set colour?
                if (debug)
                {
                    ShowAllIcons();
                    //SetAllIconsColours(debugColour);
                }
            }
            
            invokeInfoLabel("Your turn!");
            //infoLabel.Text = "Your turn!";
            //Debug.WriteLine("Your turn!");
            if (InvokeRequired)
            {
                infoLabel.BeginInvoke((Action)(() =>
                {
                    SetScoresLocalUI(); // just for ui
                }));
            }
            else
            {
                SetScoresLocalUI(); // just for ui
            }
        }

        public void PlayersScoresWaiting()
        {
            if (pLabels.Count == 0)
            {
                // should only happen once
                AddPLabels();
                Debug.WriteLine("NEW p labels");
            }

            for (int i = 0; i < MySQLConn.playersCount; i++)
            {
                pLabels[i].Text = $"Player {i + 1}";
            }
            pLabels[iAmPlayer - 1].ForeColor = iAmPlayerColour;
            pLabels[iAmPlayer - 1].Text += $"\n{youMessage}";
            if (MySQLConn.playerTurn > 0)
            {
                pLabels[MySQLConn.playerTurn - 1].ForeColor = selectedPlayerColour;
            }
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
            for (int i = 0; i <= pLabels.Count - 1; i++)
            {
                pLabels[i].Text = "";
            }
            // Needed? // Set each label to player score
            for (int i = 0; i < MySQLConn.playersCount; i++)
            {
                pLabels[i].Text = $"Player {i + 1}" +
                                  $"\nScore: {playersScores[i].ToString()}";
            }
            pLabels[iAmPlayer - 1].ForeColor = iAmPlayerColour;
            pLabels[iAmPlayer - 1].Text += $"\n{youMessage}";
            if (MySQLConn.playerTurn > 0) // cannot use if no players
            {
                pLabels[MySQLConn.playerTurn - 1].ForeColor = selectedPlayerColour;
            }
        }
        private void MakeListOfIcons()
        {
            foreach (string icon in iconToMake)
            {
                gameIcons.Add(icon);
                gameIcons.Add(icon);
                gameIcons.Add(icon);

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
            pLabels[iAmPlayer - 1].ForeColor = iAmPlayerColour;
            pLabels[iAmPlayer - 1].Text += $"\n{youMessage}";
            if (MySQLConn.playerTurn > 0)
            {
                pLabels[MySQLConn.playerTurn - 1].ForeColor = selectedPlayerColour;
            }
        }

        // same as SetAllActiveCardsIcons()
        private void HideIconsOnSquares()
        {
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != endRow)
                {
                    // only if card is still active
                    if(iconLabel.Text != "")
                        iconLabel.Text = "["; // or {
                }
            }
        }
        
        // Debug only
        private void ShowIconOnGame(string name, string value, bool setHidden = false)//FindLabelAndSet("name", "value")
        {
            // value not used, could use to set icon value (if needed), though icons are set in a list so not needed


            bool allIcons = false;
            //bool noValue = false;
            if (name == "" && value == "")
            {
                allIcons = true;
            } 
            //else if (value == "")
            //{
            //    noValue = true; // labelClicked called
            //}

            // Look for label
            Debug.WriteLine("DEBUG - ShowIcon");
            int i = 0;
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != endRow)
                {
                    if (allIcons)
                    {
                        if (iconLabel.Text != "" && gameIconsLayout[i][0] != "")
                        {
                            iconLabel.Text = gameIconsLayout[i][0]; // 0 = ("i", "label1"), 0 0 = ("i")
                            Debug.Write($" - {gameIconsLayout[i][0]}");
                        }
                        else
                        {
                            
                        }
                        //Debug.WriteLine($"ShowAllIcons - {item[1]}, {item[0]}");
                    }
                    else
                    {
                        // only one label to chage (clicked label)
                        if (iconLabel.Text != "")
                        {
                            if (iconLabel.Name == name) // labelClicked
                            {
                                if (!setHidden)
                                {
                                    iconLabel.Text = gameIconsLayout[i][0];
                                    Debug.WriteLine("");
                                    Debug.WriteLine($"changed - {gameIconsLayout[i][1]}");
                                }
                                else
                                {
                                    iconLabel.Text = "";
                                    gameIconsLayout[i][0] = ""; // removes icon from player waiting if active player scores
                                }
                            }
                            //Debug.Write($"c - {gameIconsLayout[i][1]}");
                        }
                        else
                        {
                            Debug.Write($" - Blank");
                        }
                        //Debug.Write($" - NOT {gameIconsLayout[i][0]}");
                    }
                    i++;
                }
            }
            Debug.WriteLine("");
        }
        // Debug only
        private void ShowAllIcons()
        {
            ShowIconOnGame("", "");

            //Debug.WriteLine("DEBUG - ShowAllIcons");
            //int i = 0;
            //foreach (Control control in tableLayoutPanel1.Controls)
            //{
            //    Label iconLabel = control as Label;

            //    // Column 0 and row 6 are for buttons, ignore labels there
            //    if (iconLabel != null &&
            //        tableLayoutPanel1.GetColumn(control) != 0 &&
            //        tableLayoutPanel1.GetRow(control) != endRow)
            //    {
            //        if (iconLabel.Text != "" && gameIconsLayout[i][0] != "")
            //        {
            //            iconLabel.Text = gameIconsLayout[i][0]; // 0 = ("i", "label1"), 0 0 = ("i")
            //            Debug.Write($" - {gameIconsLayout[i][0]}");
            //        }
            //        else
            //        {
            //            Debug.Write($" - NOT {gameIconsLayout[i][0]}");                    
            //        }
            //        //Debug.WriteLine($"ShowAllIcons - {item[1]}, {item[0]}");
            //        i++;
            //    }
            //}
            //Debug.WriteLine("");
            /*
            foreach (var item in gameIconsLayout)
            {
                //item.Find(clickedLabel.Name)

                // Not blank
                //if (item.Find(x => x.Contains(clickedLabel.Name)) != "")// default T - ""?
                if (item.Contains(clickedLabel.Name))// default T - ""?
                {
                    clickedLabel.Text = item[0];
                    Debug.WriteLine($"ShowAllIcons - {item[1]}, {item[0]}");
                }
            }
            */
        }
        
        private void HideIconOnGame(string name)
        {
            ShowIconOnGame("", "", true); 
        }
            private void ReAssignIconsToSquares(Label clickedLabel)
        {

            //foreach (Control control in tableLayoutPanel1.Controls)
            //{
            //    Label iconLabel = control as Label;
            //    int i = 0;

            //    // Column 0 and row 6 are for buttons, ignore labels there
            //    if (iconLabel != null &&
            //        tableLayoutPanel1.GetColumn(control) != 0 &&
            //        tableLayoutPanel1.GetRow(control) != endRow)
            //    {

            //        //int randomNumber = random.Next(icons.Count);
            //        iconLabel.Text = gameIcons[i];
            //        //iconLabel.ForeColor = hiddenColour;//iconLabel.BackColor; // set invisible

            //        //iconsLayout[randomNumber] = icons[randomNumber];
            //        i++;
            //    }
            //}

            foreach (var item in gameIconsLayout)
            {
                //item.Find(clickedLabel.Name)

                // Not blank
                //if (item.Find(x => x.Contains(clickedLabel.Name)) != "")// default T - ""?
                if (item.Contains(clickedLabel.Name))// default T - ""?
                {
                    Debug.WriteLine("Found somewhere?");
                    // matches label name
                    if (item[1] == clickedLabel.Name)
                    {
                        clickedLabel.Text = item[0];
                        Debug.WriteLine("Found?"); // working!
                    }
                }
            }
        }
        //"AssignIconsToSquares" creates "gameIconsLayout"
        //(remove from P2, only P1 can create layout)

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
                    tableLayoutPanel1.GetRow(control) != endRow)
                {
                    int randomNumber = random.Next(gameIcons.Count);
                    iconLabel.Text = gameIcons[randomNumber];
                    // don't change foreColor, already set, backColor only for turn change
                    //iconLabel.ForeColor = isTurnBGColour;//iconLabel.BackColor; // set invisible

                    //gameIconsLayout[randomNumber] = gameIcons[randomNumber];
                    AddToGameIconsLayout(iconLabel.Text, iconLabel.Name);
                    gameIcons.RemoveAt(randomNumber);
                }
            }
            Debug.WriteLine("LIST LIST DONE");
        }
        private void AssignIconsToSquares(List<string> icons)
        {
            // The TableLayoutPanel has 16 labels, and the icon list has 16 icons,
            // so an icon is pulled at random from the list and added to each label
            int i = 0;
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != endRow)
                {
                    iconLabel.Text = icons[i];
                    AddToGameIconsLayout(iconLabel.Text, iconLabel.Name);
                    i++;
                }
            }
            Debug.WriteLine("LIST LIST DONE");
        }
        private void AddToGameIconsLayout(string labelName, string icon)
        {
            //SetGameIconsLayout();
            List<string> temp = new List<string>();
            temp.Add($"{labelName}");
            temp.Add($"{icon}");
            // etc
            gameIconsLayout.Add(temp);
        }
        private void ShareLayout()
        {
            //Send gameIconsLayout
            // go through gameIconsLayout and set a new list of just values, make list into a string and send
            List<string> temp = new List<string>();
            foreach (var item in gameIconsLayout)
            {
                temp.Add(item[0]); // 0 = label text, 1 = label name
            }
            string tempString = string.Join(",", temp.ToArray());
            MySQLConn.SetLayout(tempString);

            /*
                string[] arr = new string[] { "one", "two", "three", "four" };
                Console.WriteLine(String.Join("\n", sarr)); 

                string dogCsv = string.Join(",", dogs.ToArray());
                
                foreach (string item in list) 
                */
        }
        private void GetLayoutAndSetup()
        {
            //Retrieve gameIconsLayout
            // make string into list and set gameIconsLayout values
            string tempString = MySQLConn.CheckLayout();
            if (tempString == "")
            {
                Debug.WriteLine("DEBUG - tempString is blank (MySQLConn.CheckLayout())");
                return;
            } 
            
            List<string> temp = new List<string>(tempString.Split(',')); //List<string> temp = tempString.Split(',').ToList(); // LINQ

            AssignIconsToSquares(temp);
        }

        private void SetAllActiveCardsIcons()
        {
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != endRow &&
                    iconLabel.Text != "" ) 
                {
                    iconLabel.Text = "["; // back of card text
                    //iconLabel.ForeColor = hiddenColour;//iconLabel.BackColor; // set invisible
                }
            }
        }
        private void SetGameIconsLayout()
        {
            // for each
            //gameIconsLayout[randomNumber] = gameIcons[randomNumber];
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
                    tableLayoutPanel1.GetRow(control) != endRow)
                {
                    //iconLabel.ForeColor = color;//iconLabel.BackColor;
                }
            }
        }
        private void SetAllIconsBG(Color color)
        {
            // The TableLayoutPanel has 16 labels, and the icon list has 16 icons,
            // so an icon is pulled at random from the list and added to each label
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != endRow)
                {
                    iconLabel.BackColor = color;//iconLabel.BackColor;
                }
            }
        }
        private void SetAllIconsNull()
        {
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != endRow)
                {
                    iconLabel.Text = "";
                    // should be no icons to see anyway
                    //iconLabel.ForeColor = isTurnBGColour;//iconLabel.BackColor; // set invisible
                }
            }
        }
        private void SetClickedIconsColour(Color colour)
        {
            //firstClicked.ForeColor = colour;
            //secondClicked.ForeColor = colour;
            //thirdClicked.ForeColor = colour;
        }
        public void SetScoresLocalUI()
        {
            // Update local
            // Set each label to player score
            for (int i = 0; i < MySQLConn.playersCount; i++)
            {
                pLabels[i].Text = $"Player {i + 1}" +
                                  $"\nScore: {playersScores[i].ToString()}";

                // Add "ME" next to your score
                //if (i == iAmPlayer - 1)//playerTurn - 1)
                //{
                //    pLabels[i].Text += $" - (ME)";
                //    //pLabels[playerTurn - 1].Text += "- (ME)"
                //}
            }
            pLabels[iAmPlayer - 1].ForeColor = iAmPlayerColour;
            pLabels[iAmPlayer - 1].Text += $"\n{youMessage}";
            if (MySQLConn.playerTurn > 0)
            {
                pLabels[MySQLConn.playerTurn - 1].ForeColor = selectedPlayerColour;
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


        int sleep = 4000; // wait for user to be ready first? (server value)
        // Every label's Click event is handled by this event handler
        private void ClickWait()
        {
            infoLabel.Text = "Wait";
            UpdateUI();
            Thread.Sleep(sleep);
            enableForm();
            infoLabel.Text = "Continue";
        }
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


            if (timer1.Enabled == true || isClickingDisabled)
            {
                return;
            }
            
            disableForm(); // isClickingDisabled = true
            
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

                // do this for all clicks?
                //List<string> temp = new List<string>();
                //temp.Add(clickedLabel.Name);

                // This makes correct symbol appear only when clicked
                //foreach (var item in gameIconsLayout)
                //{
                //    //item.Find(clickedLabel.Name)

                //    // Not blank
                //    if (item.Find(x => x.Contains(clickedLabel.Name)) != "")// default T - ""?
                //    {
                //        Debug.WriteLine("Found? blank");
                //        // matches label name
                //        if (item[0] == clickedLabel.Name)
                //        {
                //            clickedLabel.Text = item[0];
                //            Debug.WriteLine("Found?");
                //        }
                //    }
                //}

                // If the clicked label is black, the player clicked an icon that's already been revealed --
                // ignore the click

                // NO LONGER NEED, all is set to black now
                //if (clickedLabel.ForeColor == selectedColour)
                //    return;

                // back image is now icons in debug, so skip check
                if (debug)
                {
                    goto SkipBackImageCheck;
                }

                // if no back image, cannot click
                if (clickedLabel.Text != "[")
                {
                    Debug.WriteLine("No back image");
                    enableForm();
                    return;
                }


                // Click
                // Otherwise continue with click
            SkipBackImageCheck:
                // if conn open?
                MySQLConn.SetClickedLabel(clickedLabel.Name);
                ReAssignIconsToSquares(clickedLabel);

                // If firstClicked is null, this is the first icon in the trio that the player clicked, 
                // so set firstClicked to the label that the player clicked, change its color to black, and return
                if (firstClicked == null)
                {
                    firstClicked = clickedLabel;

                    ClickWait();
                    //firstClicked.ForeColor = selectedColour;
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
                    ClickWait();
                    //secondClicked.ForeColor = selectedColour;
                    return;
                }

                // Not showing selectedColour on 3rd clicked
                // Third click

                //// do this for all clicks?
                //List<string> temp = new List<string>();
                //temp.Add(clickedLabel.Name);
                //foreach (var item in gameIconsLayout)
                //{
                //    //item.Find(clickedLabel.Name)
                //    if (item.Find(x => x.Contains(clickedLabel.Name)) != "")// default T - ""?
                //    {
                //        clickedLabel.Text = item[0];
                //        Debug.WriteLine("Changing turn, CheckingTurn2 thread started");
                //    }                   
                //}

                //clickedLabel.Text = gameIconsLayout[gameIconsLayout.Find(clickedLabel.Name)][1];
                //System.InvalidOperationException: 'Failed to compare two elements in the array.'
                //clickedLabel.Text = gameIconsLayout[gameIconsLayout.BinarySearch(temp)][1]; // 0 = letter, 1 = name?
                thirdClicked = clickedLabel;
                //thirdClicked.ForeColor = selectedColour;
                thirdClicked.Refresh(); // doesn't update otherwise
                //UpdateUI(thirdClicked);
                // this takes time??
                //disableForm();
                infoLabel.Text = "Wait";
                UpdateUI();
                Thread.Sleep(sleep);
                infoLabel.Text = "Continue";

                // After clicks, check same/ different selected icons
                // 3 same icons
                // If the player clicked three matching icons, keep them black 
                //  and reset firstClicked and secondClicked and thirdClicked
                // so the player can click another icon
                if (firstClicked.Text == secondClicked.Text &&
                    secondClicked.Text == thirdClicked.Text)
                {
                     // stop more clicks
                    PlayerScored();
                    ClickWait();
                    //enableForm(); // only enable form again if player scores
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
                //timer1.Start(); // Hides icons
                hideIcons();
            }
        }
        private void UpdateUI(Label ui)
        {
            //lblStatus.Text = status;
            ui.Invalidate();
            ui.Update();
            ui.Refresh();
            Application.DoEvents();
        }
        private void UpdateUI()
        {
            //lblStatus.Text = status;
            infoLabel.Invalidate();
            infoLabel.Update();
            infoLabel.Refresh();
            Application.DoEvents();
        }

        private void PlayerScored()
        {
            // This player wins cards
            infoLabel.Text = $"P{playerTurn} scored: {firstClicked.Text} {secondClicked.Text} {thirdClicked.Text}";
            //p1Label.Text += $"{firstClicked.Text} {secondClicked.Text} {thirdClicked.Text}";

            //System.NullReferenceException: 'Object reference not set to an instance of an object.'
            if (playersScores != null)
                playersScores[playerTurn - 1]++; // add to player score
            SetScores();
            RemoveClicked(); // remove text
            ResetClicked(); // reset players clicked cards
            CheckForWinner(); // not working?
        }
        // This timer is started when the player clicks three icons that don't match,
        // so it counts three quarters of a second, and then turns itself off and hides both icons
        //private void timer1_Tick(object sender, EventArgs e)
        private void timer1_Tick(object sender, EventArgs e)
        {
        }
        private void hideIcons()
        {
            // Stop the timer
            //timer1.Stop();

            // Hide all icons
            //.....SetClickedIconsColour(hiddenColour);//firstClicked.BackColor); // all clicked have same backgrounds
            
            //setClickedIconsColour(this.BackColor); // Same?

            // Reset firstClicked and secondClicked and thirdClicked
            // so the next time a label is clicked will be first click
            ResetClickedAndSetBackground();

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
            SetAllActiveCardsIcons();
            resetPlayerLabelsColour();
            if (debug)
            {
                ShowAllIcons();
            }
            //pLabels[player - 1].Text += " - Your Turn";

            // Update tables
            //?

            // Update playerTurn
            MySQLConn.SetPlayerTurn(playerTurn);


            // waiting, is not my turn - Async (move partial outside of thread?)// not your turn
            ChangingTurn_Thread = new Thread(delegate ()
            {
                Debug.WriteLine("Changing turn, CheckingTurn2 thread started");
                if (playerTurn != iAmPlayer)
                {
                    if (InvokeRequired)
                    {
                        // Invoke to enable UI edit
                        infoLabel.BeginInvoke((Action)(() =>
                        {
                            SetAllActiveCardsIcons();
                            resetPlayerLabelsColour(); //also does - pLabels[MSQLConn.playerTurn - 1].ForeColor = Color.Red;
                                                       // Set colours of table (not clickable)

                            SetAllIconsBG(isNotTurnBGColour);
                            //SetAllIconsColours(isNotTurnBGColour);
                            if (debug)
                            {
                                ShowAllIcons();
                                //SetAllIconsColours(debugColour);
                            }
                        }));
                    }

                    CheckingTurn2();
                }
            });
            ChangingTurn_Thread.Start();


            //GameLoop(); // always true? non-player-turn cannot access here?
        }
        private void ResetClicked()
        {
            firstClicked = null;
            secondClicked = null;
            thirdClicked = null;
        }
        private void ResetClickedAndSetBackground()
        {
            firstClicked = null;
            secondClicked = null;
            thirdClicked = null;
            SetAllActiveCardsIcons();
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
            Debug.WriteLine("Win - Checking win!");
            // Go through all of the labels in the TableLayoutPanel, checking each one to see if its icon is matched
            bool winnerFound = false;
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Label iconLabel = control as Label;

                // Column 0 and row 6 are for buttons, ignore labels there
                if (iconLabel != null &&
                    tableLayoutPanel1.GetColumn(control) != 0 &&
                    tableLayoutPanel1.GetRow(control) != endRow &&
                    iconLabel.Text != "")
                {
                    // if any labels not blank, no win
                    Debug.WriteLine("Win - No winner yet");
                    //if (iconLabel.ForeColor == hiddenColour)//iconLabel.BackColor)
                    return; // stop from processing winner found
                }
                else
                {
                    // Win
                    winnerFound = true;
                }
            }
            // end for loop
            // Win
            if (winnerFound)
            {
                Debug.WriteLine("Win - Winner found!");
                WinningMessageBox();
            }
        }
        // Winner and other players show box
        public void WinningMessageBox()
        {
            // Game has ended, update all players
            // At end of game set gameAlreadyStarted=false

            // Only last scoring player updates game state
            if (playerTurn == iAmPlayer)
            {
                GameEnded_Thread = new Thread(async delegate ()
                {
                    //await StartedCheck();
                    Debug.WriteLine("Win - thread started");
                    bool isFinished = false;

                    await Task.Run(() =>
                    {
                        while (isFinished == false)
                        {
                            try
                            {
                                if (MySQLConn.conn.State == System.Data.ConnectionState.Closed)
                                {
                                    MySQLConn.SetIsGameAlreadyStarted(false);
                                    MySQLConn.SetPlayerTurn(0);
                                    Debug.WriteLine("Win - thread finished");
                                    isFinished = true;
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine($"Win - thread Exception: {e.Message}");
                                Thread.Sleep(1000);
                                Debug.WriteLine($"Win - thread try again");
                                Thread.Sleep(1000);
                            }
                        }
                        //MySQLConn.SetIsGameAlreadyStarted(false);
                        //MySQLConn.SetPlayerTurn(0); 
                    });
                });
                GameEnded_Thread.Start();
            }

            // count player score
            // congrats = 
                    //into "Game Over, scores = 
					//Player 1: 0
					//Player 2: 5
                    //
                    //Winner: P0 with 0 points
            string congrats = "Game Over! \nScores = ";
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
                congrats += $"\nPlayer {i}: {playersScores[i - 1]} "; // P1 is score of pScores pos 0
            }
            congrats += $"\nWinner: Player {winner} with {winnerScore} points!";

            Debug.WriteLine("WinningMessageBox!");
            // Disable form for everyone?


            playerTurn = 0;
            // On End of game
            if (iAmPlayer == 1)
            {
                MySQLConn.SetIsGameClientRunning(false);
                //MySQLConn.SetIsGameAlreadyStarted(false); // already set by winning player
                //CheckGameRunning();
            }
            else
            {
                // wait for players to catch up
                //Thread.Sleep(2000);
                //CheckGameRunning();
            }

            infoLabel.Text = "Game Finished!";
            restartBtn.Visible = true;
            restartBtn.Enabled = true;
            restartBtn.Text = "Restart Client";
            disableForm();

            //Congratulations, P1:, P2:, P3:
            Debug.WriteLine("Show win");
            MessageBox.Show($"{congrats}", "All the icons matched!");
        }
        bool isClickingDisabled = false;
        // This same loop is repeated a lot in the code
        private void disableForm(bool setDisable = true)
        {
            // isClickingDisabled = true; 
            isClickingDisabled = setDisable; // false if "enableForm" called, then clicking is enabled
            // Disable Form
            //foreach (Label l in Form)
            //foreach (Control control in tableLayoutPanel1.Controls)
            //{
            //    Label iconLabel = control as Label;

            //    if (iconLabel != null &&
            //        tableLayoutPanel1.GetColumn(control) != 0 &&
            //        tableLayoutPanel1.GetRow(control) != endRow)
            //    {
                    
            //        //iconLabel.Enabled = false;//iconLabel.Visible = false; // ERROR?
            //    }
            //}
            Debug.WriteLine($"ICONS are enabled = {isClickingDisabled}");
        }
        private void enableForm()
        {
            disableForm(false); // isClickingDisabled = false; 
        }

        // Buttons
        private void restartBtn_Click(object sender, EventArgs e)
        {
            Restart();
        }
        private void serverBtn_Click(object sender, EventArgs e)
        {
            Init_thread = new Thread(delegate ()
             {
                 //infoLabel.Text

                 init(); // server
                         //
             });
            Init_thread.Start();
        }

        private void newWindowBtn_Click(object sender, EventArgs e)
        {
            //var form2 = new Form1();
            //form2.Show();
            NewWindow_thread = new Thread(delegate ()
            {
                Application.Run(new Form1());
            });

            NewWindow_thread.Start();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void RBtnSize30_CheckedChanged(object sender, EventArgs e)
        {
            // does this work?
            boardSize = boardSize_1;
            Debug.WriteLine($"Board Size = {boardSize_1}");
        }

        private void RBtnSize60_CheckedChanged(object sender, EventArgs e)
        {
            boardSize = boardSize_2;
            Debug.WriteLine($"Board Size = {boardSize_2}");
        }

        private void debugLabel_Click(object sender, EventArgs e)
        {
            debug = !debug;
            debugLabel.Text = $"Debug = {debug}";

            //NewForm_Thread = new Thread(delegate ()
            //{
            //    Application.Run(new Form1());
            //});

            //NewForm_Thread.Start();
            //this.Close();
        }
    }
}
