using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MMG
{
    public class MySQL
    {
        /* How to SQL Connect
        (old) Download MySQL Connector Net, project reference MySql.Data
              using mysql.data.mysqlclient
         
        1. Bluehost - cPanel
        Bluehost create database (MySQLDatabase, matchingGame)
            add user "player1" all privilages?
            create table phpmyadmin - matchingGame_data
            add columns - id, name(string), age
            insert (add any info), SQL Select (check)
        Database = ffunrnmy_matchingGame
	    User = ffunrnmy_player1 
        Password = password 
        Privilages = select, update

        Remote MySQL - 192.168.% (can do just %)

        Database - 162.241.230.107
           
        2. Programming
        NuGet package MySql.Data
        using MySql.Data.MySqlClient;
        //"server= 162.241.230.107;PORT=3306?;DATABASE=myDB;UID=user;PASSWORD=password;"
        */

        /* Players story
        (player 1)
        1. init, check started, check p1 selected
        2. you are p1, you have start button
        3. start - count players etc... everyone waiting now starts
        4. scoring
        5. ending

        (other players)
        1. init, check started, check p1 started
        2. you are p2/3, waiting
        3. start - setupGame, gameLoop, waiting for turn
        4. scoring
        5. ending
        
        */

        /* Database
        matchingGame_players = id, name, score
            0, playersCount, 3
            1, p1, 0
            2, p2, 0
            ... (upto 6 players)

        matchingGame_data = id, name, value
        1, isGameClientRunning, true
        2, isGameAlreadyStarted, false
        3, playerTurn, 1
        4, layout, ""
        // Future, Message of the day? Layout checked? Timer?

        // need to change user to player2...?
        // use Linq2SQL and Entity Framework?
        */
        //bool setDefault = false; // SQL defaults

        public MySqlConnection conn;
        public static Form1 myForm;
        string connString;
        public static string message;

        string _matchingGame_players = "matchingGame_players";
        List<string> matchingGame_playersColumns;
        string mG_playerColumn1 = "id";
        string mG_playerColumn2 = "name";
        string mG_playerColumn3 = "score";

        string _matchingGame_data = "matchingGame_data";
        List<string> matchingGame_dataColumns;
        string mG_dataColumn1 = "id";
        string mG_dataColumn2 = "name";
        string mG_dataColumn3 = "value";



        string _playersCount = "playersCount";
        List<string> playersList = new List<string>();
        string _p1 = "p1";
        string _p2 = "p2";
        string _p3 = "p3";
        string _p4 = "p4";
        string _p5 = "p5";
        string _p6 = "p6";

        string _isGameClientRunning = "isGameClientRunning";
        string _isGameAlreadyStarted = "isGameAlreadyStarted";
        string _playerTurn = "playerTurn";
        string _clickedLabel = "clickedLabel";
        string _layout = "layout";


        //List<string> iconToMake = new List<string>()
        //{
        //    "!", "N", "2", "k", "b", "v", "w", "z", ".", ","
        //};
        //public List<String[]> gameInfo = new List<String[]>();
        // (NOT YET)<List> sets gameInfo field? "gameInfo.gameAlreadyStarted...
        // set gameClientRunning and gameAlreadyStarted varialbes
        public bool isGameAlreadyStarted = false;
        public bool isGameClientRunning = false;
        public int playerTurn = 0;
        public int playersCount = 1; // default 1
        public int[] playersScores = new int[6];
        public string labelClicked = "";
        


        public MySQL()
        {
            // database - 162.241.230.107
            //"server= 162.241.230.107;PORT=3306?;DATABASE=myDB;UID=user;PASSWORD=password;"
            connString = "server= 162.241.230.107;" +
                        "PORT=3306;DATABASE=ffunrnmy_matchingGame;" +
                        "UID=ffunrnmy_player1;" +
                        "PASSWORD=password;";

            matchingGame_dataColumns = new List<string>(){
                mG_dataColumn1,
                mG_dataColumn2,
                mG_dataColumn3
            };
            matchingGame_playersColumns = new List<string>(){
                mG_playerColumn1,
                mG_playerColumn2,
                mG_playerColumn3
            };

            playersList.Add(_p1);
            playersList.Add(_p2);
            playersList.Add(_p3);
            playersList.Add(_p4);
            playersList.Add(_p5);
            playersList.Add(_p6);
            //gameInfo.Add(isGameAlreadyStarted.ToString());//"isGameAlreadyStarted, true");


            //if (setDefault)
            //    SetDefault(); // resets tables values
            Debug.WriteLine("Server - Init");
        }

        public void ServerDataResetMessageBox()
        {
            Debug.WriteLine("0. Server Data Reset Message Box");
            DialogResult result;

            // yes, no, yes and close
            result = MessageBox.Show($"Server data reset? (Yes, No, Yes and Close)", "Yes, No, Yes and Close", MessageBoxButtons.YesNoCancel);

            //result = MessageBox.Show(message, caption, buttons);
            if (result == DialogResult.Yes)
            {
                SetDefault();
            }
            else if (result == DialogResult.No)
            {
                // do nothing
            }
            else if (result == DialogResult.Cancel)
            {
                SetDefault();
                // this.close?
                myForm.thisIsClosed = true;
                //myForm.CloseForm(); // Closes the parent form.
            }
        }
        public void SetDefaultOLD()
        {
            try
            {
                Debug.WriteLine("Server - Setting defaults on server");
                // Reset isGameClientRunning
                MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = 'false' " +
                            $"WHERE {mG_dataColumn2} = '{_isGameClientRunning}'");
                conn.Close();

                // Reset isGameAlreadyStarted
                MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = 'false' " +
                            $"WHERE {mG_dataColumn2} = '{_isGameAlreadyStarted}'");
                conn.Close();

                // Reset playerTurn
                MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = '0' " +
                            $"WHERE {mG_dataColumn2} = '{_playerTurn}'");
                conn.Close();

                // Reset clickedLabel
                MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = '' " +
                            $"WHERE {mG_dataColumn2} = '{_clickedLabel}'");
                conn.Close();

                // Reset layout
                MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = '' " +
                            $"WHERE {mG_dataColumn2} = 'layout'");
                conn.Close();

                // Reset playersCount
                MySQLCMDRun($"UPDATE {_matchingGame_players} " +
                            $"SET {mG_playerColumn3} = '0' " +
                            $"WHERE {mG_playerColumn2} = '{_playersCount}'");
                conn.Close();

                // Reset p1, p2, p3...
                MySQLCMDRun($"UPDATE {_matchingGame_players} " +
                            $"SET {mG_playerColumn3} = '0' " +
                            $"WHERE NOT {mG_playerColumn2} = '{_playersCount}'");
                conn.Close();

                

                Debug.WriteLine($"Server - SetDefault Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"SetDefault Exception: {ex.Message}");
            }
        }
        // MySQL refactoring (performance)
        // DONE Test 1 = connect as P1 (faster?)     - uses SetDefault and P1Connected (new method)
        // Test 2 = Start game button (faster?) - uses P1StartedGame (new method)
        // Test 3 = Waiting for turn/ scoring   - uses CheckingTurnAndState (new method)

        //1. Tidy up SetDefault()
        //2. MySQLConn.P1Connected(); // In CheckGameRunning()
        //          MySQLConn.SetIsGameClientRunning(true);
        //          MySQLConn.AddPlayer(); 
        //3. MySQLConn.P1StartedGame(); // In StartGameAndRestart()
        //        MySQLConn.NewGame();
        //        MySQLConn.CheckGameState();
        //        MySQLConn.CheckScores();
        //        MySQLConn.CheckTurn();
        //4. MySQLConn.CheckingTurnsAndState();// In CheckingTurn()
        //      MySQLConn.CheckClickedLabel();
        //      MySQLConn.CheckTurn();
        //      MySQLConn.CheckScores();
        //      MySQLConn.CheckGameState();
        // also go to Scoring and only update by players count

        public void SetDefault()
        {
            try
            {
                Debug.WriteLine("Server - Setting defaults on server");
                // Reset isGameClientRunning
                // Reset isGameAlreadyStarted
                // Reset playerTurn
                // Reset clickedLabel
                // Reset layout
                // Reset playersCount
                // Reset p1, p2, p3...

                MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = 'false' " +
                            $"WHERE {mG_dataColumn2} = '{_isGameClientRunning}';" +
                            
                            $"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = 'false' " +
                            $"WHERE {mG_dataColumn2} = '{_isGameAlreadyStarted}';" +
                            
                            $"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = '0' " +
                            $"WHERE {mG_dataColumn2} = '{_playerTurn}';" +
                            
                            $"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = '' " +
                            $"WHERE {mG_dataColumn2} = '{_clickedLabel}';" +
                            
                            $"UPDATE {_matchingGame_data} " +
                            $"SET {mG_dataColumn3} = '' " +
                            $"WHERE {mG_dataColumn2} = 'layout';" +


                            $"UPDATE {_matchingGame_players} " +
                            $"SET {mG_playerColumn3} = '0' " +
                            $"WHERE {mG_playerColumn2} = '{_playersCount}';" +

                            $"UPDATE {_matchingGame_players} " +
                            $"SET {mG_playerColumn3} = '0' " +
                            $"WHERE NOT {mG_playerColumn2} = '{_playersCount}'");
                conn.Close();

                Debug.WriteLine($"Server - SetDefault2 Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"SetDefault2 Exception: {ex.Message}");
            }
        }

        public void P1Connected()
        {
            try
            {
                //MySQLConn.AddPlayer();  //Get then set players count
                MySqlDataReader dataReader2 = MySQLCMDRun($"SELECT * FROM {_matchingGame_players}");
                while (dataReader2.Read())
                {
                    // Name column
                    if (dataReader2[mG_playerColumn2].ToString() == $"{_playersCount}")
                    {
                        //Could not find specified column in results: playersCount
                        string temp = (dataReader2[mG_playerColumn3].ToString()); //mG_playerColumn2
                        playersCount = int.Parse(temp);
                    }
                }
                playersCount++;
                // MySQLConn.SetIsGameClientRunning(true) and set players count
                MySqlDataReader dataReader3 = MySQLCMDRun($"UPDATE {_matchingGame_players} " +
                                                                $"SET {mG_playerColumn3} = '{playersCount}' " +
                                                                $"WHERE {mG_playerColumn2} = '{_playersCount}';" + // ; to seperate statements

                                                                $"UPDATE {_matchingGame_data} " +
                                                                $"SET {mG_dataColumn3} = '{true}' " +
                                                                $"WHERE {mG_dataColumn2} = '{_isGameClientRunning}'");
                conn.Close();

                Debug.WriteLine($"Server - P1Connected Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"P1Connected Exception: {ex.Message}");
            }
        }

        public void P1StartedGame()
        {
            try
            {
                //        MySQLConn.NewGame();
                //        MySQLConn.CheckTurn();
                //        MySQLConn.CheckGameState();
                //        //MySQLConn.CheckPlayersCount(); // Set when checking players joined
                //        MySQLConn.CheckScores();

                // set playerTurn 1 // set isGameAlreadyStarted true //set PlayersCount  // set each players
                MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                                                                $"SET {mG_dataColumn3} = '1' " +
                                                                $"WHERE {mG_dataColumn2} = '{_playerTurn}';" +

                                                                $"UPDATE {_matchingGame_data} " +
                                                                $"SET {mG_dataColumn3} = '{true}' " +
                                                                $"WHERE name = '{_isGameAlreadyStarted}';" +

                                                                $"UPDATE {_matchingGame_players} " +
                                                                $"SET {mG_playerColumn3} = '{playersCount}' " +
                                                                $"WHERE {mG_playerColumn2} = '{_playersCount}';" +

                                                                $"UPDATE {_matchingGame_players} " +
                                                                $"SET {mG_playerColumn3} = '0' " +
                                                                $"WHERE NOT {mG_playerColumn2} = '{_playersCount}'"); // not "playersCount" = p1, p2, p3, p4, p5, p6?
                conn.Close();

                // Turn // Game state // Players count // players scores
                playerTurn = 1;
                isGameAlreadyStarted = true; // Game client is running and now game is already started
                //playersCount = ... // set when checking players joined
                // set all local scores to 0
                for (int i = 0; i < playersCount - 1; i++)
                {
                    playersScores[i] = 0;
                }

                Debug.WriteLine($"Server - P1StartedGame Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"P1StartedGame Exception: {ex.Message}");
            }
        }


        public void CheckingTurnsAndState()
        {
            try
            {
                //4. MySQLConn.CheckingTurnsAndState();// In CheckingTurn()
                //      MySQLConn.CheckClickedLabel();
                //      MySQLConn.CheckTurn();
                //      MySQLConn.CheckScores();
                //      MySQLConn.CheckGameState();



                //CheckClickedLabel
                MySqlDataReader dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_data}");
                //string query = $"SELECT * FROM {_matchingGame_data}";
                //MySqlCommand cmd = new MySqlCommand(query, conn);
                //MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    if (dataReader[mG_dataColumn2].ToString() == _clickedLabel)
                    {
                        labelClicked = dataReader[mG_dataColumn3].ToString();
                    }
                }
                conn.Close();

                //CheckTurn();
                dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_data}");
                while (dataReader.Read())
                {
                    if (dataReader[mG_dataColumn2].ToString() == _playerTurn)
                    {
                        playerTurn = int.Parse(dataReader[mG_dataColumn3].ToString());
                    }
                }
                conn.Close();

                //CheckScores();
                dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_players}");
                int[] tempplayersScores = new int[6];
                int i = 0;
                while (dataReader.Read())
                {
                    // Dont need to check?
                    if (dataReader[mG_playerColumn2].ToString() != _playersCount)
                    {
                        tempplayersScores[i] = (int)dataReader[mG_playerColumn3];
                        i++;
                    }
                }
                // Set local scores
                for (int j = 0; j < playersCount; j++)
                {
                    Debug.WriteLine($"Server - CheckScores - pScore? {j}= {playersScores[j]}");

                    playersScores[j] = tempplayersScores[j];
                }
                conn.Close();

                //CheckGameState();
                dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_data}");
                string tempisGameClientRunning = "false";
                string tempisGameAlreadyStarted = "false";
                while (dataReader.Read())
                {
                    if (dataReader[mG_dataColumn2].ToString() == _isGameClientRunning)
                    {
                        tempisGameClientRunning = (string)dataReader[mG_dataColumn3];
                    }
                    if (dataReader[mG_dataColumn2].ToString() == _isGameAlreadyStarted)
                    {
                        tempisGameAlreadyStarted = dataReader[mG_dataColumn3].ToString();
                    }
                }
                if (bool.Parse(tempisGameClientRunning))
                    isGameClientRunning = bool.Parse(tempisGameClientRunning);
                if (bool.Parse(tempisGameAlreadyStarted))
                    isGameAlreadyStarted = bool.Parse(tempisGameAlreadyStarted);
                conn.Close();
                Debug.WriteLine($"Server - CheckingTurnsAndState Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show
                Debug.WriteLine($"CheckingTurnsAndState Exception: {ex.Message}");
            }
        }


        // Empty name data method?

        //public void ConnStart()
        //{
        //    conn = new MySqlConnection();
        //    conn.ConnectionString = connString;
        //    conn.Open();
        //}
        public MySqlDataReader MySQLCMDRun(string query)
        {
            conn = new MySqlConnection();
            conn.ConnectionString = connString;
            conn.Open();

            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            return dataReader;
        }

        //checkgame exception - column "where" isgameclientrunning (forgot '' around value)

        // get isGameClientRunning and isGameAlreadyStarted
        public void CheckGameOLD()
        {
            try
            {
                /*
                MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_data}" +
                                                        $"SET {mG_dataColumn3} = '' " +
                                                        $"WHERE {mG_dataColumn2} = {_isGameClientRunning}");
                */
                string tempisGameClientRunning = "false";
                string tempisGameAlreadyStarted = "false";
                string tempplayerTurn = "0";
                MySqlDataReader dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_data}");
                while (dataReader.Read())
                {
                    //Debug.WriteLine($"col1 {dataReader[mG_dataColumn1]}");
                    //Debug.WriteLine($"col2 {dataReader[mG_dataColumn2]}");
                    //Debug.WriteLine($"col3 {dataReader[mG_dataColumn3]}");
                    if (dataReader[mG_dataColumn2].ToString() == _isGameClientRunning)
                    {
                        tempisGameClientRunning = (string)dataReader[mG_dataColumn3];//.ToString();
                        //Debug.WriteLine($"SET {tempisGameClientRunning} to {(string)dataReader[mG_dataColumn3]}");

                    }
                    if (dataReader[mG_dataColumn2].ToString() == _isGameAlreadyStarted)
                    {
                        tempisGameAlreadyStarted = dataReader[mG_dataColumn3].ToString();
                    }
                    if (dataReader[mG_dataColumn2].ToString() == _playerTurn)
                    {
                        tempplayerTurn = dataReader[mG_dataColumn3].ToString();
                    }
                }
                conn.Close();

                //Debug.WriteLine($"tempisGameClientRunning = {tempisGameClientRunning}");
                // Check is bool
                if (bool.Parse(tempisGameClientRunning))
                    isGameClientRunning = bool.Parse(tempisGameClientRunning);
                if (bool.Parse(tempisGameAlreadyStarted))
                    isGameAlreadyStarted = bool.Parse(tempisGameAlreadyStarted);
                
                playerTurn = int.Parse(tempplayerTurn);
                //Debug.WriteLine($"isGameClientRunning = {isGameClientRunning} - temp = {tempisGameClientRunning}");
                //Debug.WriteLine($"isGameAlreadyStarted = {isGameAlreadyStarted} - temp = {tempisGameAlreadyStarted}");
                //Debug.WriteLine($"playerTurn = {playerTurn} - temp = {tempplayerTurn}");


                // update players count
                int tempplayersCount = 0;
                MySqlDataReader dataReader2 = MySQLCMDRun($"SELECT * FROM {_matchingGame_players}");
                while (dataReader2.Read())
                {
                    if (dataReader2[mG_playerColumn2].ToString() == _playersCount)
                    {
                        tempplayersCount = (int)dataReader2[mG_playerColumn3];
                    }
                }
                conn.Close();
                playersCount = tempplayersCount;//int.Parse(tempplayersCount);
                //Debug.WriteLine($"Players count = {playersCount} - temp = {tempplayersCount}");


                // set local scores
                MySqlDataReader dataReader3 = MySQLCMDRun($"SELECT * FROM {_matchingGame_players}"); // +
                                                          //$"WHERE NOT {mG_playerColumn2} = '{_playersCount}'");// not "playersCount" = p1, p2, p3, p4, p5, p6?
                int[] tempplayersScores = new int[6]; // set 6??
                int i = 0;
                while (dataReader3.Read())
                {
                    // Dont need to check?
                    if (dataReader3[mG_playerColumn2].ToString() != _playersCount)
                    {
                        tempplayersScores[i] = (int)dataReader3[mG_playerColumn3];
                        i++;
                    }
                }
                conn.Close();
                // Set local scores
                for (int j = 0; j < playersCount; j++) // not -1?
                {
                    Debug.WriteLine($"Server - CheckGame - pScore? {j}= {playersScores[j]}");

                    playersScores[j] = tempplayersScores[j];
                } // p scores not being set on player 1 (p2 score)? FIXED...?

                Debug.WriteLine($"Server - CheckGame Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"CheckGame Exception: {ex.Message}");
            }
        }

        public void CheckGameState()
        {
            try
            {
                string tempisGameClientRunning = "false";
                string tempisGameAlreadyStarted = "false";
                MySqlDataReader dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_data}");
                while (dataReader.Read())
                {
                    if (dataReader[mG_dataColumn2].ToString() == _isGameClientRunning)
                    {
                        tempisGameClientRunning = (string)dataReader[mG_dataColumn3];
                    }
                    if (dataReader[mG_dataColumn2].ToString() == _isGameAlreadyStarted)
                    {
                        tempisGameAlreadyStarted = dataReader[mG_dataColumn3].ToString();
                    }
                }
                conn.Close();

                // Check is bool
                if (bool.Parse(tempisGameClientRunning))
                    isGameClientRunning = bool.Parse(tempisGameClientRunning);
                if (bool.Parse(tempisGameAlreadyStarted))
                    isGameAlreadyStarted = bool.Parse(tempisGameAlreadyStarted);

                Debug.WriteLine($"Server - CheckGameState Success - isGameClientRunning: {isGameClientRunning}, isGameAlreadyStarted: {isGameAlreadyStarted}");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"CheckGameState Exception: {ex.Message}");
            }
        }

        public void CheckTurn()
        {
            try
            {   
                string tempplayerTurn = "0";
                MySqlDataReader dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_data}");
                while (dataReader.Read())
                {
                    if (dataReader[mG_dataColumn2].ToString() == _playerTurn)
                    {
                        tempplayerTurn = dataReader[mG_dataColumn3].ToString();
                    }
                }
                conn.Close();

                playerTurn = int.Parse(tempplayerTurn);

                Debug.WriteLine($"Server - CheckTurn Success - Turn = {playerTurn}");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"CheckTurn Exception: {ex.Message}");
            }
        }
        public void CheckClickedLabel()
        {
            try
            {
                string tempLabelClicked = "";
                MySqlDataReader dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_data}");
                while (dataReader.Read())
                {
                    if (dataReader[mG_dataColumn2].ToString() == _clickedLabel)
                    {
                        tempLabelClicked = dataReader[mG_dataColumn3].ToString();
                    }
                }
                conn.Close();

                Debug.WriteLine($"Server - CheckLabelClicked Success - Label = {tempLabelClicked}");
                //return tempLabelClicked;
                labelClicked = tempLabelClicked;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"CheckLabelClicked Exception: {ex.Message}");
                //return "";
            }
        }
        public string CheckLayout()
        {
            try
            {
                string tempLayout = "";
                MySqlDataReader dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_data}");
                while (dataReader.Read())
                {
                    if (dataReader[mG_dataColumn2].ToString() == _layout)
                    {
                        tempLayout = dataReader[mG_dataColumn3].ToString();
                    }
                }
                conn.Close();

                Debug.WriteLine($"Server - CheckLayout Success - Layout = {tempLayout}");
                return tempLayout;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"CheckLayout Exception: {ex.Message}");
                return "";
            }
            catch (Exception ex)
            {
                // Exception here??
                MessageBox.Show($"CheckLayout Exception: {ex.Message}");
                return "";
            }
        }

        public void CheckPlayersCount()
        {
            try
            {
                // update players count
                int tempplayersCount = 0;
                MySqlDataReader dataReader2 = MySQLCMDRun($"SELECT * FROM {_matchingGame_players}");
                while (dataReader2.Read())
                {
                    if (dataReader2[mG_playerColumn2].ToString() == _playersCount)
                    {
                        tempplayersCount = (int)dataReader2[mG_playerColumn3];
                    }
                }
                conn.Close();
                playersCount = tempplayersCount;

                Debug.WriteLine($"Server - CheckPlayersCount Success - playersCount = {playersCount}");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"CheckPlayersCount Exception: {ex.Message}");
            }
        }

        public void CheckScores()
        {
            try
            {
                // get scores and set local scores
                MySqlDataReader dataReader3 = MySQLCMDRun($"SELECT * FROM {_matchingGame_players}"); // +
                                                                                                     //$"WHERE NOT {mG_playerColumn2} = '{_playersCount}'");// not "playersCount" = p1, p2, p3, p4, p5, p6?
                int[] tempplayersScores = new int[6]; // set 6??
                int i = 0;
                while (dataReader3.Read())
                {
                    // Dont need to check?
                    if (dataReader3[mG_playerColumn2].ToString() != _playersCount)
                    {
                        tempplayersScores[i] = (int)dataReader3[mG_playerColumn3];
                        i++;
                    }
                }
                conn.Close();
                //playersScores[j] = tempplayersScores[j];
                // Set local scores
                for (int j = 0; j < playersCount; j++) // not -1?
                {
                    Debug.WriteLine($"Server - CheckScores - pScore? {j}= {playersScores[j]}");

                    playersScores[j] = tempplayersScores[j];
                } // p scores not being set on player 1 (p2 score)? FIXED...?

                Debug.WriteLine($"Server - CheckScores Success - playersScores = p1-{playersScores[0]}, p2-{playersScores[1]}"); //? each one?
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"CheckScores Exception: {ex.Message}");
            }
        }

        // PlayersCount
        public void AddPlayer()
        {
            try
            {
                // Get playersCount
                MySqlDataReader dataReader = MySQLCMDRun($"SELECT * FROM {_matchingGame_players}");
                while (dataReader.Read())
                {
                    // Name column
                    if (dataReader[mG_playerColumn2].ToString() == $"{_playersCount}")
                    {
                        //Could not find specified column in results: playersCount
                        string temp = (dataReader[mG_playerColumn3].ToString()); //mG_playerColumn2
                        playersCount = int.Parse(temp);
                    }
                }

                //dataReader["name"].ToString()
                conn.Close();

                // Add Player
                playersCount++;

                // Set playersCount
                MySqlDataReader dataReader2 = MySQLCMDRun($"UPDATE {_matchingGame_players} " +
                                                        $"SET {mG_playerColumn3} = '{playersCount}' " +
                                                        $"WHERE {mG_playerColumn2} = '{_playersCount}'");
                conn.Close();
                Debug.WriteLine($"Server - AddPlayer Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"AddPlayer Exception: {ex.Message}");
            }
        }

        // set PlayersCount, playerTurn 1, isGameAlreadyStarted true, Scores<list>
        public void NewGame()
        {
            try
            {
                // set PlayersCount
                MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_players} " +
                                                        $"SET {mG_playerColumn3} = '{playersCount}' " +
                                                        $"WHERE {mG_playerColumn2} = '{_playersCount}'");
                conn.Close();

                // set playerTurn 1
                MySqlDataReader dataReader2 = MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                                                        $"SET {mG_dataColumn3} = '1' " +
                                                        $"WHERE {mG_dataColumn2} = '{_playerTurn}'");
                conn.Close();

                // set isGameAlreadyStarted true
                SetIsGameAlreadyStarted(true);

                // set each players
                MySqlDataReader dataReader3 = MySQLCMDRun($"UPDATE {_matchingGame_players} " +
                                                        $"SET {mG_playerColumn3} = '0' " +
                                                        $"WHERE NOT {mG_playerColumn2} = '{_playersCount}'"); // not "playersCount" = p1, p2, p3, p4, p5, p6?
                conn.Close();


                // set local scores
                for (int i = 0; i < playersCount - 1; i++)
                {
                    playersScores[i] = 0;
                }
                Debug.WriteLine($"Server - NewGame Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"NewGame Exception: {ex.Message}");
                //throw;
            }
        }

        public void SetIsGameClientRunning(bool value) // true
        {
            try
            {
                // SET Value = true
                MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                                                        $"SET {mG_dataColumn3} = '{value}' " +
                                                        $"WHERE {mG_dataColumn2} = '{_isGameClientRunning}'");

                conn.Close();
                Debug.WriteLine($"Server - SetIsGameClientRunning Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"SetIsGameClientRunning Exception: {ex.Message}");
            }
        }

        public void SetIsGameAlreadyStarted(bool value)
        {
            try
            {
                MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                                                        $"SET {mG_dataColumn3} = '{value}' " +
                                                        $"WHERE name = '{_isGameAlreadyStarted}'");

                conn.Close();
                Debug.WriteLine($"Server - SetIsGameAlreadyStarted Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"SetIsGameAlreadyStarted Exception: {ex.Message}");
            }
        }

        public void SetPlayerTurn(int pTurn)
        {
            try
            {
                playerTurn = pTurn;

                MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                                                        $"SET {mG_dataColumn3} = '{pTurn}' " +
                                                        $"WHERE {mG_dataColumn2} = '{_playerTurn}'");
                conn.Close();
                Debug.WriteLine($"Server - UpdateTurn Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"SetPlayerTurn Exception: {ex.Message}");
                //throw;
            }
        }
        public void SetClickedLabel(string labelName)
        {
            try
            {
                MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                                                        $"SET {mG_dataColumn3} = '{labelName}' " +
                                                        $"WHERE {mG_dataColumn2} = '{_clickedLabel}'");
                conn.Close();
                Debug.WriteLine($"Server - SetLabelClicked Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"SetLabelClicked Exception: {ex.Message}");
                //throw;
            }
        }
        public void SetLayout(string list)
        {
            try
            {
                MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_data} " +
                                                        $"SET {mG_dataColumn3} = '{list}' " +
                                                        $"WHERE {mG_dataColumn2} = '{_layout}'");
                conn.Close();
                Debug.WriteLine($"Server - SetLayout Success - {list}");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"SetLayout Exception: {ex.Message}");
                //throw;
            }
        }

        public void SetScores(int[] pScore)
        {
            try
            {
                playersScores = pScore;

                // set player 1 score


                // instead of each all players (playersList), make new list, with size of pScore 

                List<string> newPlayersList = new List<string>();
                for (int i = 0; i < pScore.Length; i++)
                {
                    newPlayersList.Add(playersList[i]); 
                }

                // "item" adds value instead of location
                //foreach (int item in pScore)
                //{
                //    newPlayersList.Add(playersList[item]);
                //}
                Debug.Write("Server - "); // LONG debug
                foreach (string pName in newPlayersList)
                {
                    //{p[playersList.IndexOf(pName)]}
                    // p 0 = playersList[0] // p1 location
                    //Debug.WriteLine("EXCEPTION?");
                    Debug.Write($"pScore? {pName}= {pScore[newPlayersList.IndexOf(pName)]}, Index = {newPlayersList.IndexOf(pName)}, ");
                    MySqlDataReader dataReader = MySQLCMDRun($"UPDATE {_matchingGame_players} " +
                                                        $"SET {mG_playerColumn3} = '{pScore[ newPlayersList.IndexOf(pName)]}'" +
                                                        $"WHERE {mG_playerColumn2} = '{pName}'");
                    conn.Close();
                }
                Debug.WriteLine($"Server - UpdateScore Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"Server - UpdateScores Exception: {ex.Message}");
                //throw;
            }
        }

    }

    /* //void MySQLConnect()
    void MySQLConnect()
        {
            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = connString;
                conn.Open();
                //MessageBox.Show($"Connected");
                conn.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
                //throw;
            }
        }
    */
    /* //void SelectAll()
        void SelectAll()
        {
            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = connString;
                conn.Open();
                string query = "SELECT * FROM matchingGame_data";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    //Debug.WriteLine($"{dataReader["id"]}");
                    //Debug.WriteLine($"{dataReader["name"]}");
                    //Debug.WriteLine($"{dataReader["age"]}");
                }
                message = dataReader["name"].ToString();
                conn.Close();
                //MessageBox.Show($"Connected");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
                //throw;
            }
        }
    */
    /* //void Update(string s)
        void Update(string s)
        {
            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = connString;
                conn.Open();
                //string query = "SELECT * FROM matchingGame_data";

                // INSERT ACCESS DENIED
                //string query = "INSERT INTO matchingGame_data " +
                //                $"VALUES(NULL, '{s}', '22')";

                string query = "UPDATE matchingGame_data " +
                                $"SET name = '{s}' " +
                                "WHERE id = 1";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                /*
                // SELECT ALL
                query = "SELECT * FROM matchingGame_data";
                cmd = new MySqlCommand(query, conn);

                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    Debug.WriteLine($"{dataReader["id"]}");
                    Debug.WriteLine($"{dataReader["name"]}");
                    Debug.WriteLine($"{dataReader["age"]}");
                }
                message = dataReader["name"].ToString();
                
    conn.Close();
                //MessageBox.Show($"Success");
                Debug.WriteLine($"Update Success");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
                //throw;
            }
        }
    */
}
