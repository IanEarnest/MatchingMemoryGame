using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace MMG
{
    public partial class Form1 :Form
    {
        // Use this Random object to choose random icons for the squares
        Random random = new Random();
        Color selectedColour;
        Color hiddenColour;
        Color assignedColour;
        int players = 3;
        int player = 1;
        int[] playersScores;

        List<string> iconToMake = new List<string>()
        {
            "!", "N", "2", "k", "b", "v", "w", "z", ".", ","
        };

        // Each of these letters is an interesting icon
        // in the Webdings font,
        // and each icon appears twice in this list
        List<string> icons = new List<string>();
        List<Label> pLabels = new List<Label>();
        // List of clicks, foreach through them to change instead of needing to add more code
        // for each of the clicks?

        // firstClicked points to the first Label control that the player clicks, but it will be null 
        // if the player hasn't clicked a label yet
        Label firstClicked = null;
        // secondClicked points to the second Label control that the player clicks
        Label secondClicked = null;
        Label thirdClicked = null;

        // not yet implemented
        public void restart()
        {
            infoLabel.Text = "Hello";

            // Set 0
            //playersScores = new int[players];
            setScores();
            //p1Label.Text = "0";

            MakeListOfIcons();
            AssignIconsToSquares(); // Set grid
            
            player = 1;
        }

        public Form1()
        {
            InitializeComponent();
            infoLabel.Text = "Hello";

            playersScores = new int[players];
            //foreach (int item in playersScores)
            //{
            //    playersScores[item] = 0;
            //}
            addPLabels(); //pLabels.Add(p1Label);

            setScores();
            pLabels[player - 1].ForeColor = Color.Red;
            //p1Label.Text = "10";
            //p2Label.Text = "11";
            //p3Label.Text = "12";
            //p4Label.Text = "13";

            // Set colours then make icon list and assign icons to squares
            selectedColour = Color.Black;
            //hiddenColour = this.BackColor;
            //assignedColour = this.BackColor;
            hiddenColour = Color.White;
            assignedColour = Color.White; // testing

            MakeListOfIcons();
            AssignIconsToSquares(); // Set grid
        }
        public void addPLabels()
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
        }
        public void setScores()
        {
            for (int i = 0; i < players; i++)
            {
                //Debug.WriteLine("before: " + pLabels[i].Text);
                // Set each label to player score
                pLabels[i].Text = $"Player {i+1}: {playersScores[i].ToString()}";
                
                //Debug.WriteLine("after: " + pLabels[i].Text);
            }
            //foreach (int p in playersScores)
            //{
            //pLabels[player - 1].Text += " - Your Turn";
            //}
        }
        private void MakeListOfIcons()
        {
            foreach (string icon in iconToMake)
            {
                icons.Add(icon);
                icons.Add(icon);
                icons.Add(icon);
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

        /// <summary>
        /// Assign each icon from the list of icons to a random square
        /// </summary>
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
                    icons.RemoveAt(randomNumber);
                }
            }
        }

        
        /// <summary>
        /// Every label's Click event is handled by this event handler
        /// </summary>
        /// <param name="sender">The label that was clicked</param>
        /// <param name="e"></param>
        private void label_Click(object sender, EventArgs e)
        {
            // label31 = player turn

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

                // 3 same icons
                // If the player clicked three matching icons, keep them black 
                //  and reset firstClicked and secondClicked and thirdClicked
                // so the player can click another icon
                if (firstClicked.Text == secondClicked.Text && 
                    secondClicked.Text == thirdClicked.Text)
                {
                    playerScored();
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
        private void playerScored()
        {
            // This player wins cards
            infoLabel.Text = $"P{player} scored: {firstClicked.Text} {secondClicked.Text} {thirdClicked.Text}";
            //p1Label.Text += $"{firstClicked.Text} {secondClicked.Text} {thirdClicked.Text}";

            playersScores[player - 1]++; // add to player score
            setScores();
            removeClicked(); // remove text
            resetClicked();
            CheckForWinner();
        }

        /// <summary>
        /// This timer is started when the player clicks 
        /// two icons that don't match,
        /// so it counts three quarters of a second 
        /// and then turns itself off and hides both icons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            timer1.Stop();

            // Hide all icons
            setClickedIconsColour(hiddenColour);//firstClicked.BackColor); // all clicked have same backgrounds
            //setClickedIconsColour(this.BackColor); // Same?

            // Reset firstClicked and secondClicked and thirdClicked
            // so the next time a label is clicked will be first click
            resetClicked();

            // Player 1, 2 ,3
            if (player < players)
            {
                player++;
            }
            else
            {
                player = 1;
            }
            infoLabel.Text = $"Turn: P{player}";
            
            for (int i = 0; i <= players-1; i++)
            {
                pLabels[i].ForeColor = Color.Black; //?
            }
            pLabels[player - 1].ForeColor = Color.Red;
            //pLabels[player - 1].Text += " - Your Turn";
        }

        private void resetClicked()
        {
            firstClicked = null;
            secondClicked = null;
            thirdClicked = null;
        }
        private void removeClicked()
        {
            firstClicked.Text = "";
            secondClicked.Text = "";
            thirdClicked.Text = "";
        }

        private void setClickedIconsColour(Color colour)
        {
            firstClicked.ForeColor = colour;
            secondClicked.ForeColor = colour;
            thirdClicked.ForeColor = colour;
        }

        /// <summary>
        /// Check every icon to see if it is matched, by 
        /// comparing its foreground color to its background color. 
        /// If all of the icons are matched, the player wins
        /// </summary>
        private void CheckForWinner()
        {
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

            // count player score

            string congrats = "Congratulations! ";
            int winner = 0;
            int winnerScore = 0;
            for (int i = 1; i <= players; i++)
            {
                // Highest score
                if (playersScores[i - 1] > winnerScore)
                {
                    winnerScore = playersScores[i - 1];
                    winner = i;
                }
                congrats += $"P{i}:{playersScores[i-1]} "; // P1 is score of pScores pos 0
            }
            congrats += $"\nWinner: P{winner} with {winnerScore} points";

            //Congratulations, P1:, P2:, P3:
            MessageBox.Show($"{congrats}", "You matched all the icons!");

            Close(); // instead of close, wait for restart
        }
    }
}
