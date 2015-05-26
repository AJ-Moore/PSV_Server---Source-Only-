using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PSV_Server
{
    public partial class Keyboard : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        public Keyboard()
        {
            InitializeComponent();
            this.keyboardEventSubscribe();
        }

        //The same method will be called for each keyboard on click event.
        private void keyboardEventSubscribe()
        {
            this.Q_Key.Click += new EventHandler(keyboard_Click);
            this.W_Key.Click += new EventHandler(keyboard_Click);
            this.E_Key.Click += new EventHandler(keyboard_Click);
            this.Y_Key.Click += new EventHandler(keyboard_Click);
            this.T_Key.Click += new EventHandler(keyboard_Click);
            this.R_Key.Click += new EventHandler(keyboard_Click);
            this.RBracket_Key.Click += new EventHandler(keyboard_Click);
            this.LBracket_Key.Click += new EventHandler(keyboard_Click);
            this.P_Key.Click += new EventHandler(keyboard_Click);
            this.O_Key.Click += new EventHandler(keyboard_Click);
            this.I_Key.Click += new EventHandler(keyboard_Click);
            this.U_Key.Click += new EventHandler(keyboard_Click);
            this.Hash_Key.Click += new EventHandler(keyboard_Click);
            this.Quote_Key.Click += new EventHandler(keyboard_Click);
            this.SColon_Key.Click += new EventHandler(keyboard_Click);
            this.L_Key.Click += new EventHandler(keyboard_Click);
            this.K_Key.Click += new EventHandler(keyboard_Click);
            this.J_Key.Click += new EventHandler(keyboard_Click);
            this.H_Key.Click += new EventHandler(keyboard_Click);
            this.G_Key.Click += new EventHandler(keyboard_Click);
            this.F_Key.Click += new EventHandler(keyboard_Click);
            this.D_Key.Click += new EventHandler(keyboard_Click);
            this.S_Key.Click += new EventHandler(keyboard_Click);
            this.A_Key.Click += new EventHandler(keyboard_Click);
            this.FSlash_Key.Click += new EventHandler(keyboard_Click);
            this.Dot_Key.Click += new EventHandler(keyboard_Click);
            this.Commer_Key.Click += new EventHandler(keyboard_Click);
            this.M_Key.Click += new EventHandler(keyboard_Click);
            this.N_Key.Click += new EventHandler(keyboard_Click);
            this.B__Key.Click += new EventHandler(keyboard_Click);
            this.V_Key.Click += new EventHandler(keyboard_Click);
            this.C_Key.Click += new EventHandler(keyboard_Click);
            this.X_Key.Click += new EventHandler(keyboard_Click);
            this.Z_Key.Click += new EventHandler(keyboard_Click);
            this.BSlash_Key.Click += new EventHandler(keyboard_Click);
            this.Tab_Key.Click += new EventHandler(keyboard_Click);
            this.Caps_Key.Click += new EventHandler(keyboard_Click);
            this.LShift_Key.Click += new EventHandler(keyboard_Click);
            this.Hyphen_Key.Click += new EventHandler(keyboard_Click);
            this.Zero_Key.Click += new EventHandler(keyboard_Click);
            this.Nine_Key.Click += new EventHandler(keyboard_Click);
            this.Eight_Key.Click += new EventHandler(keyboard_Click);
            this.Seven_Key.Click += new EventHandler(keyboard_Click);
            this.Six_Key.Click += new EventHandler(keyboard_Click);
            this.Five_Key.Click += new EventHandler(keyboard_Click);
            this.Four_Key.Click += new EventHandler(keyboard_Click);
            this.Three_Key.Click += new EventHandler(keyboard_Click);
            this.Two_Key.Click += new EventHandler(keyboard_Click);
            this.One_Key.Click += new EventHandler(keyboard_Click);
            this.con_Key.Click += new EventHandler(keyboard_Click);
            this.Equals_Key.Click += new EventHandler(keyboard_Click);
            this.RShift_Key.Click += new EventHandler(keyboard_Click);
            this.Return_Key.Click += new EventHandler(keyboard_Click);
            this.Backspace_Key.Click += new EventHandler(keyboard_Click);
            this.F12_Key.Click += new EventHandler(keyboard_Click);
            this.F11_Key.Click += new EventHandler(keyboard_Click);
            this.F10_Key.Click += new EventHandler(keyboard_Click);
            this.F9_Key.Click += new EventHandler(keyboard_Click);
            this.F8_Key.Click += new EventHandler(keyboard_Click);
            this.F7_Key.Click += new EventHandler(keyboard_Click);
            this.F6_Key.Click += new EventHandler(keyboard_Click);
            this.F5_Key.Click += new EventHandler(keyboard_Click);
            this.F4_Key.Click += new EventHandler(keyboard_Click);
            this.F3_Key.Click += new EventHandler(keyboard_Click);
            this.F2_Key.Click += new EventHandler(keyboard_Click);
            this.F1_Key.Click += new EventHandler(keyboard_Click);
            this.Escape_Key.Click += new EventHandler(keyboard_Click);
            this.PageDown_Key.Click += new EventHandler(keyboard_Click);
            this.End_Key.Click += new EventHandler(keyboard_Click);
            this.Delete_Key.Click += new EventHandler(keyboard_Click);
            this.PageUp_Key.Click += new EventHandler(keyboard_Click);
            this.Home_Key.Click += new EventHandler(keyboard_Click);
            this.Insert_Key.Click += new EventHandler(keyboard_Click);
            this.Right_Key.Click += new EventHandler(keyboard_Click);
            this.Down_Key.Click += new EventHandler(keyboard_Click);
            this.Left_Key.Click += new EventHandler(keyboard_Click);
            this.Up_Key.Click += new EventHandler(keyboard_Click);
            this.LCtrl_Key.Click += new EventHandler(keyboard_Click);
            this.Win_Key.Click += new EventHandler(keyboard_Click);
            this.Alt_Key.Click += new EventHandler(keyboard_Click);
            this.RCtrl_Key.Click += new EventHandler(keyboard_Click);
            this.AltGr_Key.Click += new EventHandler(keyboard_Click);
            this.Space_Key.Click += new EventHandler(keyboard_Click);
            this.Pause_Key.Click += new EventHandler(keyboard_Click);
            this.SLock_Key.Click += new EventHandler(keyboard_Click);
            this.PrtScr_Key.Click += new EventHandler(keyboard_Click);
            this.Num6_Key.Click += new EventHandler(keyboard_Click);
            this.Num5_Key.Click += new EventHandler(keyboard_Click);
            this.Num4_Key.Click += new EventHandler(keyboard_Click);
            this.Num9_Key.Click += new EventHandler(keyboard_Click);
            this.Num8_Key.Click += new EventHandler(keyboard_Click);
            this.Num7_Key.Click += new EventHandler(keyboard_Click);
            this.NumAs_Key.Click += new EventHandler(keyboard_Click);
            this.NumSlash_Key.Click += new EventHandler(keyboard_Click);
            this.NumLock_Key.Click += new EventHandler(keyboard_Click);
            this.Num3_Key.Click += new EventHandler(keyboard_Click);
            this.Num2_Key.Click += new EventHandler(keyboard_Click);
            this.Num1_Key.Click += new EventHandler(keyboard_Click);
            this.NumMinus_Key.Click += new EventHandler(keyboard_Click);
            this.NumPlus_Key.Click += new EventHandler(keyboard_Click);
            this.Enter_Key.Click += new EventHandler(keyboard_Click);
            this.NumDel_Key.Click += new EventHandler(keyboard_Click);
            this.Num0_Key.Click += new EventHandler(keyboard_Click);
            
            //MOUSE 
            this.LeftMouseButton.Click += new EventHandler(mouse_Click);
            this.RightMouseButton.Click += new EventHandler(mouse_Click);
            this.MouseMoveLeftBtn.Click += new EventHandler(mouse_Click);
            this.MouseMoveRightBtn.Click += new EventHandler(mouse_Click);
            this.MouseMoveUpBtn.Click += new EventHandler(mouse_Click);
            this.MouseMoveDownBtn.Click += new EventHandler(mouse_Click);
            this.MouseScrollDownBtn.Click += new EventHandler(mouse_Click);
            this.MouseScrollUpBtn.Click += new EventHandler(mouse_Click);
        }

        private bool shiftDown = false;
        private bool altDown = false;
        private bool ctrlDown = false; 

        private void keyboard_Click(object sender, EventArgs e)
        {
            //Call method on main form to set button, add to dictionary etc... 
            Button button = (Button)sender;

            //Compile string to return 
            string keys = "";
            string description = "";

            //is there a modifier key 
            bool modKey = false;

            if (ctrlDown)
            {
                keys += "^";
                description += "Ctrl + ";
                modKey = true;
            }

            if (altDown)
            {
                keys += "%";
                description += "Alt + ";
                modKey = true;
            }

            if (shiftDown)
            {
                keys += "+";
                description += "Shift + ";
                modKey = true; 
            }
            

            


            //Work out the button text? 
            string buttonText = button.Tag.ToString().ToLower();

            /*if (buttonText == "shift" || buttonText == "ctrl" || buttonText == "alt")
            {
                //buttonText = "";
            }
            else
            {*/

                if (modKey)
                {
                    keys += "(" + buttonText + ")";
                }
                else
                {
                    keys += "{" + buttonText + "}";
                }
            /*}*/


            /*if (button.Text == "Space")
            {
                keys = " ";
            }*/

            Program.PSVServer.removeBind(PSVServer.lastButtonPressed);
            Program.PSVServer.addBind(PSVServer.lastButtonPressed, keys);

            if (PSVServer.buttonDict.ContainsKey(PSVServer.lastButtonPressed))
            {
                PSVServer.buttonDict[PSVServer.lastButtonPressed].Text = description + button.Text;
            }

            if (buttonText != "ctrl" && buttonText != "shift" && buttonText != "alt")
            {
                this.Close();
            }
            else
            {
                description = description.Remove(description.Length - 2);
                PSVServer.buttonDict[PSVServer.lastButtonPressed].Text = description;
            }

        }

        private void mouse_Click(Object sender, EventArgs e)
        {
            Button button = (Button)sender;

            //Work out the button text? 
            string buttonText = button.Tag.ToString();

            Program.PSVServer.removeBind(PSVServer.lastButtonPressed);
            Program.PSVServer.addMouseBind(PSVServer.lastButtonPressed, buttonText);

            if (PSVServer.buttonDict.ContainsKey(PSVServer.lastButtonPressed))
            {
                PSVServer.buttonDict[PSVServer.lastButtonPressed].Text = button.Text;
            }

            this.Close();
        }

        private void Escape_Key_Click(object sender, EventArgs e)
        {

        }

        private void LShift_Key_Click(object sender, EventArgs e)
        {
            //toggle shift
            this.shiftDown = !this.shiftDown;

            if (this.shiftDown)
            {
                ((Button)sender).BackColor = Color.Gray;
                this.keyboard_Click(sender, e); 
            }
            else
            {
                ((Button)sender).BackColor = Color.FromArgb(64,64,64);
            }

            
        }

        private void RShift_Key_Click(object sender, EventArgs e)
        {
            //toggle shift
            this.shiftDown = !this.shiftDown;

            if (this.shiftDown)
            {
                ((Button)sender).BackColor = Color.Gray;
                this.keyboard_Click(sender, e); 
            }
            else
            {
                ((Button)sender).BackColor =Color.FromArgb(64,64,64);
            }

            
        }

        private void LCtrl_Key_Click(object sender, EventArgs e)
        {
            //toggle Ctrl
            this.ctrlDown = !this.ctrlDown;

            if (this.ctrlDown)
            {
                ((Button)sender).BackColor = Color.Gray;
                this.keyboard_Click(sender, e);
            }
            else
            {
                ((Button)sender).BackColor = Color.FromArgb(64, 64, 64);
            }

             
        }

        private void RCtrl_Key_Click(object sender, EventArgs e)
        {
            //toggle Ctrl
            this.ctrlDown = !this.ctrlDown;

            if (this.ctrlDown)
            {
                ((Button)sender).BackColor = Color.Gray;
                this.keyboard_Click(sender, e); 
            }
            else
            {
                ((Button)sender).BackColor = Color.FromArgb(64, 64, 64);
            }

            
        }

        private void Alt_Key_Click(object sender, EventArgs e)
        {
            //toggle Ctrl
            this.altDown = !this.altDown;

            if (this.altDown)
            {
                ((Button)sender).BackColor = Color.Gray;
                this.keyboard_Click(sender, e);
            }
            else
            {
                ((Button)sender).BackColor = Color.FromArgb(64, 64, 64);
            }

             
        }

        private void AltGr_Key_Click(object sender, EventArgs e)
        {
            //toggle Ctrl
            this.altDown = !this.altDown;

            if (this.altDown)
            {
                ((Button)sender).BackColor = Color.Gray;
                this.keyboard_Click(sender, e); 
            }
            else
            {
                ((Button)sender).BackColor = Color.FromArgb(64, 64, 64);
            }

            
        }

        private void Clear_Button_Click(object sender, EventArgs e)
        {
            Program.PSVServer.removeBind(PSVServer.lastButtonPressed);

            if (PSVServer.buttonDict.ContainsKey(PSVServer.lastButtonPressed))
            {
                PSVServer.buttonDict[PSVServer.lastButtonPressed].Text = "";
            }
            this.Close();
        }

        private void Keyboard_Load(object sender, EventArgs e)
        {

        }




    }
}
