using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MathQuiz
{
    public partial class Form1 :Form
    {
        Random r = new Random();
        int addend1; // addition
        int addend2; // addition
        int minuend;
        int subtrahend;
        int multiplicand;
        int multiplier;
        int dividend;
        int divisor;

        bool plusTicked;
        bool minusTicked;
        bool multiplyTicked;
        bool divideTicked;
        
        int timeLeft;

        public Form1()
        {
            InitializeComponent();
            defaultValues();
        }

        public void defaultValues()
        {
            // Disable UpDown
            sum.Enabled = false;
            difference.Enabled = false;
            product.Enabled = false;
            quotient.Enabled = false;

            // Remove Ticks
            plusTick.Text = "";
            minusTick.Text = "";
            multiplyTick.Text = "";
            divideTick.Text = "";
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            startTheQuiz();
            startBtn.Enabled = false;
        }

        // Start timer and fill in problems
        // Reset UI
        // Addition, Subtraction, Multiplication and Division
        // Start timer
        private void startTheQuiz()
        {
            resetUI();

            // Addition - two random numbers, set labels, init sum
            addend1 = r.Next(51);
            addend2 = r.Next(51);
            plusLeftLabel.Text = addend1.ToString();
            plusRightLabel.Text = addend2.ToString();
            sum.Value = 0;

            // Subtraction - two random numbers, set labels, init difference
            minuend= r.Next(1,101);
            subtrahend = r.Next(1, minuend); // only positive numbers
            minusLeftLabel.Text = minuend.ToString();
            minusRightLabel.Text = subtrahend.ToString();
            difference.Value = 0;

            // Multiplication - two random numbers, set labels, init product
            multiplicand = r.Next(2, 11);
            multiplier = r.Next(2, 11); 
            timesLeftLabel.Text = multiplicand.ToString();
            timesRightLabel.Text = multiplier.ToString();
            product.Value = 0;

            // Division  - two random numbers, set labels, init difference
            divisor = r.Next(2, 11);
            dividend = divisor * r.Next(2, 11); // a multiplication of divisor
            divideLeftLabel.Text = dividend.ToString();
            divideRightLabel.Text = divisor.ToString();
            quotient.Value = 0;


            // Timer
            timeLeft = 30;
            timeLabel.Text = $"{timeLeft.ToString()} Seconds";
            timer1.Start();
        }

        
        // UpDown enable, value 0, tick empty, bools false
        private void resetUI()
        {
            // Reset colour
            timeLabel.BackColor = Color.White;

            // UpDown
            sum.Enabled = true;
            difference.Enabled = true;
            product.Enabled = true;
            quotient.Enabled = true;
            //sum.Value = 0m; // already set
            //difference.Value = 0;
            //product.Value = 0;
            //quotient.Value = 0;


            // Tick Labels
            plusTick.Text = "";
            minusTick.Text = "";
            multiplyTick.Text = "";
            divideTick.Text = "";
            //plusTick.Visible = false;

            // Bools
            plusTicked = false;
            minusTicked = false;
            multiplyTicked = false;
            divideTicked = false;
        }

        // Sets timer red, checks answer and fail game
        private void timer1_Tick(object sender, EventArgs e)
        {
            // If all answers correct, stop
            // Else run down timer/ fail
            // with 5 seconds left, make label red
            if (timeLeft <= 5 && timeLeft > 0)
            {
                timeLabel.BackColor = Color.Red;
            }


            if (checkAnswer())
            {
                winGame();
            }
            else if (timeLeft > 0)
            {
                timeLeft--;
                timeLabel.Text = $"{timeLeft} Seconds";
            }
            else
            {
                failedGame();
            }
        }

        // Check all answers are correct
        private bool checkAnswer()
        {
            // Check Add, Sub, Multi and Division are correct
            if ((addend1 + addend2 == sum.Value) &&
                (minuend - subtrahend == difference.Value) &&
                (multiplicand * multiplier == product.Value) &&
                (dividend / divisor == quotient.Value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void winGame()
        {
            timer1.Stop();
            MessageBox.Show("Correct!", "Congrats!");
            startBtn.Enabled = true;
            SystemSounds.Hand.Play();
        }
        // Out of time, message box, show answers
        private void failedGame()
        {
            // out of time
            timer1.Stop();
            timeLabel.Text = "Time's up!";
            MessageBox.Show("Fail", "Sorry!");

            // Reset values/ show answers
            sum.Value = addend1 + addend2;
            difference.Value = minuend - subtrahend;
            product.Value = multiplicand * multiplier;
            quotient.Value = dividend / divisor;

            startBtn.Enabled = true;
            SystemSounds.Hand.Play();
        }

        
        // check answers are correct, set tick visible, play sound, disable box
        private void checkTick()
        {
            if (addend1 + addend2 == sum.Value && !plusTicked)
            {
                //plusTick.Visible = true;
                // play sound
                //System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer(@"c:\file.wav");
                //soundPlayer.Play();
                plusTick.Text = "✓";
                System.Media.SystemSounds.Asterisk.Play();
                sum.Enabled = false;
                plusTicked = true;
                // For scrolling
                //sum.Value = addend1 + addend2;
            }
            if (minuend - subtrahend == difference.Value && !minusTicked)
            {
                minusTick.Text = "✓";
                SystemSounds.Exclamation.Play();
                difference.Enabled = false;
                minusTicked = true;
                
            }
            if (multiplicand * multiplier == product.Value && !multiplyTicked)
            {
                multiplyTick.Text = "✓";
                SystemSounds.Beep.Play();
                product.Enabled = false;
                multiplyTicked = true;
            }
            if (dividend / divisor == quotient.Value && !divideTicked)
            {
                divideTick.Text = "✓";
                SystemSounds.Question.Play(); // No sound?
                quotient.Enabled = false;
                divideTicked = true;
            }

            // For scrolling
            if (plusTicked)
            {
                sum.Value = addend1 + addend2;
            }
            if (minusTicked)
            {
                difference.Value = minuend - subtrahend;
            }
            if (multiplyTicked)
            {
                product.Value = multiplicand * multiplier;
            }
            if (divideTicked)
            {
                quotient.Value = dividend / divisor;
            }
        }

        // Make UpDown empty
        private void answer_Enter(object sender, EventArgs e)
        {
            // Problem = 0 would stay in box, user types 1 = 10;
            // Solution = Make answerBox value selected
            NumericUpDown answerBox = sender as NumericUpDown;
            if (answerBox != null)
            {
                int lengthOfAnswer = answerBox.Value.ToString().Length;
                answerBox.Select(0, lengthOfAnswer);
            }
        }

        private void upDown_ValueChanged(object sender, EventArgs e)
        {
            // Set tick visible next to values and make noise
            // Check timer otherwise values always set
            if(timer1.Enabled)
            {
                checkTick();
            }
        }

        // Shortcut click tick to show solution
        private void tick_Click(object sender, EventArgs e)
        {
            Label btn = sender as Label;
            if (btn.Name == "plusTick")
            {
                sum.Value = addend1 + addend2;
            } 
            else if(btn.Name == "minusTick")
            {
                difference.Value = minuend - subtrahend;
            }
            else if (btn.Name == "multiplyTick")
            {
                product.Value = multiplicand * multiplier;
            }
            else if (btn.Name == "divideTick")
            {
                // Divide by 0 catch
                try
                {
                    quotient.Value = dividend / divisor;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
