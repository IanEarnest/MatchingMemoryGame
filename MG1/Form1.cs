using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MG1
{
    public partial class Form1 :Form
    {
        public Form1()
        {
            InitializeComponent();
            changeColours();
        }

        private void changeColours()
        {
            // Form background colour
            this.BackColor = Color.Red;
            // Form Removes border
            this.FormBorderStyle = FormBorderStyle.None;
            // Form Close, Minimise and Maximise buttons
            this.ControlBox = false;
            // Form - Enter opens file dialog, ESC closes application
            this.AcceptButton = showBtn; 
            this.CancelButton = closeBtn;
            
            // Checkbox/ buttons Font and ForeColor
            checkBox1.Font = new Font(checkBox1.Font.FontFamily, 11.5f);
            checkBox1.ForeColor = Color.Green;

            closeBtn.Font = new Font(closeBtn.Font, FontStyle.Bold);
            closeBtn.ForeColor = Color.Green;

            backgroundBtn.ForeColor = Color.Green;
            clearBtn.ForeColor = Color.Green;
            showBtn.ForeColor = Color.Green;
        }
        // Event handlers
        private void closeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void backgroundBtn_Click(object sender, EventArgs e)
        {
            // Show the color dialog box. If the user clicks OK, change the
            // PictureBox control's background to the color the user chose.
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.BackColor = colorDialog1.Color;
            }
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            // Clear picture box
            pictureBox1.Image = null;
        }

        private void showBtn_Click(object sender, EventArgs e)
        {
            // Show the Open File dialog. If the user clicks OK, load the
            // picture that the user chose.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Load(openFileDialog1.FileName);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            }
        }
    }
}
