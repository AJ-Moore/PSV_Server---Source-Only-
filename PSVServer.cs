using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
namespace PSV_Server
{
    public partial class PSVServer : Form
    {
        public string ipAddress;
        static public bool isConnected = false;
        Keyboard keyboard;
        Panel current_Panel;

        static public uint lastButtonPressed = 0;
/*
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                cp.Style |= 0x02000000;
                return cp;
            }
        } */

        //Analogue codes for use with buttonDict-> fill in the gaps non power 2 only 
        public enum AxisButton : uint 
        {
            LY_POS = 0x3, //left stick Y axis positive
            LY_NEG = 0x5, //left stick Y axis negative 
            LX_POS = 0x6, 
            LX_NEG = 0x7, 
            RY_POS = 0x9,
            RY_NEG = 0xA, 
            RX_POS = 0xB, 
            RX_NEG = 0xC,

            GYROX_POS = 0x21,
            GYROX_NEG = 0x22,
            
            GYROY_POS = 0x23,
            GYROY_NEG = 0x24,
            
            GYROZ_POS = 0x25,
            GYROZ_NEG = 0x26
        }

        //I know this code is getting messy 
        //Dictionary which links PSVKeyType to specific button. Sure why not... 
        static public Dictionary<uint, Button> buttonDict = new Dictionary<uint, Button>();

        private Dictionary<uint, Panel> subPanelDictionary = new Dictionary<uint, Panel>();

        //List of all sub panels used in application. 
        private enum SubPanels : uint
        {
            FRONT_PANEL = 0x1, 
            BACK_PANEL = 0x2, 
            GYRO_PANEL = 0x3, 
            ADDITIONAL_PANEL = 0x4, 
            LIST_PANEL = 0x5, 
            JOY_PANEL = 0x6
        }

        static public void refresh()
        {
            Program.serverForm.Invalidate();
        }

        public PSVServer()
        {
            InitializeComponent();

            /*SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);*/

            //!< Add Child Binds Panels 
            this.Vita_Panel.Parent = this.Binds_Panel;
            this.JoyBind_Panel.Parent = this.Binds_Panel;
            this.BindList_Panel.Parent = this.Binds_Panel;
            this.Back_Panel.Parent = this.Binds_Panel;
            this.Additional_Panel.Parent = this.Binds_Panel;
            this.Gyro_Panel.Parent = this.Binds_Panel; 

            this.Vita_Panel.Location = new Point(100,0);
            this.JoyBind_Panel.Location = new Point(100, 0);
            this.BindList_Panel.Location = new Point(100, 0);
            this.Back_Panel.Location = new Point(100, 0);
            this.Additional_Panel.Location = new Point(100, 0);
            this.Gyro_Panel.Location = new Point(100, 0); 

            initButtDictionary();
            initSubPanels();

            this.Size = new Size(655, 385);

            //Create keyboard... 
            this.keyboard = new Keyboard();
            this.keyboard.FormClosed += (sender, e) => { this.keyboard = null; };
        }

        //Adds the buttons to the dictionary with the corresponding key values... 
        private void initButtDictionary(){
            buttonDict.Add((uint)PSVKeyType.L, this.Button_L);
            buttonDict.Add((uint)PSVKeyType.R, this.Button_R);
            buttonDict.Add((uint)PSVKeyType.Triangle, this.Button_Triangle);
            buttonDict.Add((uint)PSVKeyType.Square, this.Button_Square);
            buttonDict.Add((uint)PSVKeyType.Circle, this.Button_Circle);
            buttonDict.Add((uint)PSVKeyType.Cross, this.Button_Cross);
            buttonDict.Add((uint)PSVKeyType.Up, this.Button_DUp);
            buttonDict.Add((uint)PSVKeyType.Down, this.Button_DDown);
            buttonDict.Add((uint)PSVKeyType.Left, this.Button_DLeft);
            buttonDict.Add((uint)PSVKeyType.Right, this.Button_DRight);
            buttonDict.Add((uint)PSVKeyType.Start, this.Button_Start);
            buttonDict.Add((uint)PSVKeyType.Select, this.Button_Select);
            buttonDict.Add((uint)PSVKeyType.B1, this.B1Btn);
            buttonDict.Add((uint)PSVKeyType.B2, this.B2Btn);
            buttonDict.Add((uint)PSVKeyType.B3, this.B3Btn);
            buttonDict.Add((uint)PSVKeyType.B4, this.B4Btn);
            buttonDict.Add((uint)PSVKeyType.B5, this.B5Btn);
            buttonDict.Add((uint)PSVKeyType.B6, this.B6Btn);
            buttonDict.Add((uint)PSVKeyType.B7, this.B7Btn);
            buttonDict.Add((uint)PSVKeyType.B8, this.B8Btn);
            buttonDict.Add((uint)PSVKeyType.B9, this.B9Btn);
            buttonDict.Add((uint)PSVKeyType.B10, this.B10Btn);
            buttonDict.Add((uint)PSVKeyType.B11, this.B11Btn);
            buttonDict.Add((uint)PSVKeyType.B12, this.B12Btn);
            buttonDict.Add((uint)PSVKeyType.B13, this.B13Btn);
            buttonDict.Add((uint)PSVKeyType.B14, this.B14Btn);

            buttonDict.Add((uint)AxisButton.LY_POS, this.Button_LAUp);
            buttonDict.Add((uint)AxisButton.LY_NEG, this.Button_LADown);
            buttonDict.Add((uint)AxisButton.LX_NEG, this.Button_LALeft);
            buttonDict.Add((uint)AxisButton.LX_POS, this.Button_LARight);
            buttonDict.Add((uint)AxisButton.RY_POS, this.Button_RAUp);
            buttonDict.Add((uint)AxisButton.RY_NEG, this.Button_RADown);
            buttonDict.Add((uint)AxisButton.RX_NEG, this.Button_RALeft);
            buttonDict.Add((uint)AxisButton.RX_POS, this.Button_RARight);

            //!< IDS between 18 and 31; for rear touch 13 buttons max for back touch under current configuration
            buttonDict.Add((uint)PSVRearTouch.LEFT_TOUCH + 18, this.BackLeftDownBtn);
            buttonDict.Add((uint)PSVRearTouch.RIGHT_TOUCH + 18, this.BackRightDownBtn);

            //33 to 64 for gyro 
            buttonDict.Add((uint)AxisButton.GYROX_NEG, this.GyroXNegBtn);
            buttonDict.Add((uint)AxisButton.GYROX_POS, this.GyroXPosBtn);

            buttonDict.Add((uint)AxisButton.GYROY_NEG, this.GyroYNegBtn);
            buttonDict.Add((uint)AxisButton.GYROY_POS, this.GyroYPosBtn);

            buttonDict.Add((uint)AxisButton.GYROZ_NEG, this.GyroZNegBtn);
            buttonDict.Add((uint)AxisButton.GYROZ_POS, this.GyroZPosBtn);





        }

        //!< Creates a dictionary of all the sub panels to make it easier to switch between them and add new panels.
        private void initSubPanels()
        {
            this.subPanelDictionary.Add((uint)SubPanels.FRONT_PANEL, this.Vita_Panel);
            this.subPanelDictionary.Add((uint)SubPanels.BACK_PANEL, this.Back_Panel);
            this.subPanelDictionary.Add((uint)SubPanels.GYRO_PANEL, this.Gyro_Panel );
            this.subPanelDictionary.Add((uint)SubPanels.ADDITIONAL_PANEL, this.Additional_Panel);
            this.subPanelDictionary.Add((uint)SubPanels.LIST_PANEL, this.BindList_Panel);
            this.subPanelDictionary.Add((uint)SubPanels.JOY_PANEL, this.JoyBind_Panel);
        }


        private void PSVServer_Load(object sender, EventArgs e)
        {
            this.RefreshTimer.Enabled = true;
            this.ResumeLayout();

            this.ipLable.Text = ipAddress;

            /*ConsoleWriter writer = new ConsoleWriter(this.Console_Box);
            Console.SetOut(writer);
            Console.Out.NewLine = "\r";

            Console.WriteLine("Console Switched");
            Console.WriteLine("PSV Pad Server");
            Console.WriteLine("Developed by AJ Moore");
            Console.WriteLine("Unorthodox Game Studios");
            Console.WriteLine("www.unorthodoxgamestudios.co.uk");
            Console.WriteLine("===============================");
            Console.WriteLine();


            Console.WriteLine("Enter the IP below on the PSVPAD vita app and press connect");
            Console.WriteLine(this.ipAddress);*/
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //linkLabel1.LinkVisited = true;
            //System.Diagnostics.Process.Start("http://www.unorthodoxgamestudios.co.uk"); 
        }

        private void PSVServer_Paint(object sender, PaintEventArgs e)
        {
            if (isConnected == true)
            {
                this.Status.Text = "CONNECTED";
                this.Status.ForeColor = Color.Green;
                this.Status1.Text = "CONNECTED";
                this.Status1.ForeColor = Color.Green;
            }
            else
            {
                this.Status.Text = "DISCONNECTED";
                this.Status.ForeColor = Color.Red;
                this.Status1.Text = "DISCONNECTED";
                this.Status1.ForeColor = Color.Red;
            }

            
        }

        private void Button_L_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.L; 
            this.openKeyboard(); 
        }


        void openKeyboard(){
            if (this.keyboard != null)
            {
                keyboard.Show();
            }
            else{
                //Create keyboard... 
                this.keyboard = new Keyboard();
                this.keyboard.FormClosed += (sender, e) => { this.keyboard = null; };
                keyboard.Show();
            }
        }

        void closeKeyboard(){
            this.keyboard.Close();
        }

        private void Button_R_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.R;
            this.openKeyboard(); 
        }

        private void Button_Triangle_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Triangle;
            this.openKeyboard(); 
        }

        private void Button_Circle_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Circle;
            this.openKeyboard(); 
        }

        private void Button_Cross_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Cross;
            this.openKeyboard(); 
        }

        private void Button_Square_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Square;
            this.openKeyboard(); 
        }

        private void Button_DUp_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Up;
            this.openKeyboard(); 
        }

        private void Button_DLeft_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Left;
            this.openKeyboard(); 
        }

        private void Button_DRight_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Right;
            this.openKeyboard(); 
        }

        private void Button_DDown_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Down;
            this.openKeyboard(); 
        }

        private void Button_Select_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Select;
            this.openKeyboard(); 
        }

        private void Button_Start_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.Start;
            this.openKeyboard(); 
        }

        private void Button_LAUp_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.LY_POS;
            this.openKeyboard(); 
        }

        private void Button_LADown_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.LY_NEG;
            this.openKeyboard(); 
        }

        private void ButtonLALeft_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.LX_NEG;
            this.openKeyboard(); 
        }

        private void ButtonLARight_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.LX_POS;
            this.openKeyboard(); 
        }

        private void Button_RAUp_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.RY_POS;
            this.openKeyboard(); 
        }

        private void Button_RADown_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.RY_NEG;
            this.openKeyboard(); 
        }

        private void Button_RALeft_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.RX_NEG;
            this.openKeyboard(); 
        }

        private void Button_RARight_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.RX_POS;
            this.openKeyboard(); 
        }

        Point panel_Position = new Point(0, 30);
        //Point panel_Hidden = new Point(650, 30); 
        Point panel_Hidden = new Point(0, 310); 

        private void Set_Panel(Panel _Panel)
        {
            this.SuspendLayout();

            if (_Panel == this.current_Panel)
                return;

            if (this.current_Panel != null){
                this.current_Panel.Location = panel_Hidden;
                this.current_Panel.Visible = false;
            } 

            if (!_Panel.Visible){
                _Panel.Visible = true;
            }

            _Panel.Size = new Size(650, 280);  
            _Panel.Location = panel_Position;
            //_Panel.Location = panel_Hidden;

            //_Panel.Refresh();

            this.current_Panel = _Panel;

            //this.Invalidate();
            this.ResumeLayout();

            //experiments
            //fade in panel
            /*Stopwatch deltaAnim = new Stopwatch();
            deltaAnim.Start();*/
            /*while (this.current_Panel.Location != panel_Position)
            {
                //slideDelta = (int)deltaAnim.ElapsedMilliseconds;
                fadeIn_CallBack((object)this.current_Panel);
                //deltaAnim.Reset();
                //deltaAnim.Start();
            }*/
            /*System.Threading.Timer timer = new System.Threading.Timer(
                new TimerCallback(fadeIn_CallBack), this.current_Panel, 100, System.Threading.Timeout.Infinite);*/
            
        }

        int slideDelta = 1;

        //Object = Object to be slided in
        void fadeIn_CallBack(object State)
        {
            this.SuspendLayout();
            Panel _panel = (Panel)State;

            if (_panel.Location != panel_Position)
            {
                //check if close enough...within 2 pixels

                int xPos = _panel.Location.X;
                int yPos = _panel.Location.Y;
                if (_panel.Location.X != panel_Position.X)
                {
                    if (_panel.Location.X > panel_Position.X)
                    {
                        xPos -= 10;
                    }
                    else
                    {
                        xPos += 10;
                    }
                }
                if (_panel.Location.Y != panel_Position.Y)
                {
                    if (_panel.Location.Y > panel_Position.Y)
                    {
                        yPos -= 10 * slideDelta;
                    }
                    else
                    {
                        yPos += 10 * slideDelta;
                    }
                }

                _panel.Location = new Point(xPos, yPos); 

                /*if (_panel.Location != panel_Position){
                    System.Threading.Timer timer = new System.Threading.Timer(
                        new TimerCallback(fadeIn_CallBack), this.current_Panel, 10, System.Threading.Timeout.Infinite);
                }*/
                _panel.Refresh();
                this.ResumeLayout();
            }


        }


        private void Button_Main_Click(object sender, EventArgs e)
        {
            this.Set_Panel(Main_Panel);
        }

        private void Button_Key_Binds_Click(object sender, EventArgs e)
        {
            this.Set_Panel(Binds_Panel);
        }

        private void Set_Sub_Panel(uint PanelID)
        {
            this.SuspendLayout();
            //iterate through panels set to invisible... 
            foreach (KeyValuePair<uint, Panel> entry in this.subPanelDictionary)
            {
                entry.Value.Visible = false; 
            }

            if (this.subPanelDictionary.ContainsKey(PanelID)){
                this.subPanelDictionary[PanelID].Visible = true; 
            }
            this.ResumeLayout();
        }

        private void Button_Visual_Click(object sender, EventArgs e)
        {
            this.Set_Sub_Panel((uint)SubPanels.FRONT_PANEL);
        }

        private void Button_List_Click(object sender, EventArgs e)
        {
            this.Set_Sub_Panel((uint)SubPanels.LIST_PANEL);
        }

        private void Button_Joy_Click(object sender, EventArgs e)
        {
            this.Set_Sub_Panel((uint)SubPanels.JOY_PANEL);
        }

        private void Console_Button_Click(object sender, EventArgs e)
        {
            this.Set_Panel(Console_Panel);
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            //this.ResumeLayout();
            this.GyroXAxisVisual.Refresh();
            this.GyroYAxisVisual.Refresh();
            this.GyroZAxisVisual.Refresh();
            this.LeftStickXAxis.Refresh();
            this.LeftStickYAxis.Refresh();
            this.RightStickXAxis.Refresh();
            this.RightStickYAxis.Refresh();
            //this.SuspendLayout();
        }

        private void Button_Config_Click(object sender, EventArgs e)
        {
            this.Set_Panel(this.Config_Panel);
        }

        private void Button_Help_Click(object sender, EventArgs e)
        {
            this.Set_Panel(this.Help_Panel);
        }

        private void GyroXDeadZone_Slider_Scroll(object sender, EventArgs e)
        {
            this.GyroXDeadZoneLbl.Text = (((float)GyroXDeadZone_Slider.Value) / 10.0f).ToString();
            Program.PSVServer.config.gyroXDeadZone = ((float)GyroXDeadZone_Slider.Value) / 1000.0f;
        }




        //!< called to paint a bar representing the current axis on the control -> Both Axis Value and DeadZone Passed in between same ranges
        private void AxisBarCustomPaint_Event(Object Sender, PaintEventArgs e, float DeadZone, float AxisValue)
        {
            // Call the OnPaint method of the base class.
            base.OnPaint(e);
            System.Drawing.Brush myPen;
            // Create a new pen/s for drawing
            if (AxisValue > DeadZone || AxisValue < -DeadZone)
            {
                myPen = new System.Drawing.SolidBrush(Color.Green);
            }
            else
            {
                myPen = new System.Drawing.SolidBrush(Color.Red);
            }

            Point _center = new Point(0,0);
            _center.X = (e.ClipRectangle.Width / 2);
            _center.Y = (e.ClipRectangle.Height / 2);

            if (AxisValue > 0)
            {
                e.Graphics.FillRectangle(myPen, new Rectangle(_center.X, 0, (int)((float)_center.X *AxisValue), e.ClipRectangle.Height));
            }
            else
            {
                e.Graphics.FillRectangle(myPen, new Rectangle(_center.X + (int)(_center.X * AxisValue), 0, -(int)(_center.X * AxisValue), e.ClipRectangle.Height));
            }
            this.Invalidate();
        }

        private void GyroXAxisVisual_Paint(object sender, PaintEventArgs e)
        {
            float _axisValue = 0; 
            if (Program.PSVServer.data != null && Program.PSVServer.config.gyroXEnabled){
                _axisValue = Program.PSVServer.data.motionX;
            }
            this.AxisBarCustomPaint_Event(sender, e, ((float)this.GyroXDeadZone_Slider.Value) / 1000.0f, _axisValue);
        }

        private void GyroYDeadZone_Slider_Scroll(object sender, EventArgs e)
        {
            this.GyroYDeadZoneLbl.Text = (((float)GyroYDeadZone_Slider.Value) / 10.0f).ToString();
            Program.PSVServer.config.gyroYDeadZone = ((float)GyroYDeadZone_Slider.Value) / 1000.0f;
        }

        private void GyroZDeadZone_Slider_Scroll(object sender, EventArgs e)
        {
            this.GyroZDeadZoneLbl.Text = (((float)GyroZDeadZone_Slider.Value) / 10.0f).ToString();
            Program.PSVServer.config.gyroZDeadZone = ((float)GyroZDeadZone_Slider.Value) / 1000.0f;
        }

        private void GyroYAxisVisual_Paint(object sender, PaintEventArgs e)
        {
            float _axisValue = 0;
            if (Program.PSVServer.data != null && Program.PSVServer.config.gyroYEnabled)
            {
                _axisValue = Program.PSVServer.data.motionY;
            }
            this.AxisBarCustomPaint_Event(sender, e, ((float)this.GyroYDeadZone_Slider.Value) / 1000.0f, _axisValue);
        }

        private void GyroZAxisVisual_Paint(object sender, PaintEventArgs e)
        {
            float _axisValue = 0;
            if (Program.PSVServer.data != null && Program.PSVServer.config.gyroZEnabled)
            {
                _axisValue = Program.PSVServer.data.motionZ;
            }
            this.AxisBarCustomPaint_Event(sender, e, ((float)this.GyroZDeadZone_Slider.Value) / 1000.0f, _axisValue);
        }

        private void PSVServer_Move(object sender, EventArgs e)
        {
            this.SuspendLayout();
            this.RefreshTimer.Enabled = false;
            
        }

        private void PSVServer_ResizeEnd(object sender, EventArgs e)
        {
            this.ResumeLayout();
            this.RefreshTimer.Enabled = true;
        }

        private void BindInfoToolTip_Popup(object sender, PopupEventArgs e)
        {


        }


        private void BindInfoToolTip_CustomHover(object sender, EventArgs e)
        {
            Button _button = (Button)sender;
            BindInfoToolTip.SetToolTip(_button, _button.Text); 
        }

        private void ClientTimeOut_Box_ValueChanged(object sender, EventArgs e)
        {
            Program.PSVServer.config.clientTimeOut = (uint)ClientTimeOut_Box.Value;
        }

        private void GyroAxisXCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Program.PSVServer.config.gyroXEnabled = GyroAxisXCheckBox.Checked; 
        }

        private void GyroAxisYCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Program.PSVServer.config.gyroYEnabled = GyroAxisYCheckBox.Checked; 
        }

        private void GyroAxisZCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Program.PSVServer.config.gyroZEnabled = GyroAxisZCheckBox.Checked; 
        }

        private void KeyRepeatDelay_Box_ValueChanged(object sender, EventArgs e)
        {
            Program.PSVServer.config.keyRepeatDelayMS = (uint)KeyRepeatDelay_Box.Value;
        }

        private void MouseSpeed_Slider_Scroll(object sender, EventArgs e)
        {
            this.MouseSpeedBox.Value = this.MouseSpeed_Slider.Value;
        }

        private void KeyRepeat_Slider_Scroll(object sender, EventArgs e)
        {
            this.KeyRepeatDelay_Box.Value = this.KeyRepeat_Slider.Value;
        }

        private void LeftStickDeadZone_Slider_Scroll(object sender, EventArgs e)
        {
            Program.PSVServer.config.LSDeadZone = ((float)this.LeftStickDeadZone_Slider.Value) / 1000;
            this.LeftStickDeadZoneLbl.Text = (((float)this.LeftStickDeadZone_Slider.Value) / 10).ToString();
        }

        private void RightStickDeadZone_Slider_Scroll(object sender, EventArgs e)
        {
            Program.PSVServer.config.RSDeadZone = ((float)this.RightStickDeadZone_Slider.Value) / 1000;
            this.RightStickDeadZoneLbl.Text = (((float)this.RightStickDeadZone_Slider.Value) / 10).ToString();
        }

        private void LeftStickXAxis_Paint(object sender, PaintEventArgs e)
        {
            float _axisValue = 0;
            if (Program.PSVServer.data != null){
                _axisValue = Program.PSVServer.data.lx;
            }
            this.AxisBarCustomPaint_Event(sender, e, ((float)this.LeftStickDeadZone_Slider.Value) / 1000.0f, _axisValue);
        }

        private void LeftStickYAxis_Paint(object sender, PaintEventArgs e)
        {
            float _axisValue = 0;
            if (Program.PSVServer.data != null)
            {
                _axisValue = Program.PSVServer.data.ly;
            }
            this.AxisBarCustomPaint_Event(sender, e, ((float)this.LeftStickDeadZone_Slider.Value) / 1000.0f, _axisValue);
        }

        private void RightStickXAxis_Paint(object sender, PaintEventArgs e)
        {
            float _axisValue = 0;
            if (Program.PSVServer.data != null)
            {
                _axisValue = Program.PSVServer.data.rx;
            }
            this.AxisBarCustomPaint_Event(sender, e, ((float)this.RightStickDeadZone_Slider.Value) / 1000.0f, _axisValue);
        }

        private void RightStickYAxis_Paint(object sender, PaintEventArgs e)
        {
            float _axisValue = 0;
            if (Program.PSVServer.data != null)
            {
                _axisValue = Program.PSVServer.data.ry;
            }
            this.AxisBarCustomPaint_Event(sender, e, ((float)this.RightStickDeadZone_Slider.Value) / 1000.0f, _axisValue);
        }

        private void Button_Back_Click(object sender, EventArgs e)
        {
            this.Set_Sub_Panel((uint)SubPanels.BACK_PANEL);
        }

        private void Button_Gyro_Click(object sender, EventArgs e)
        {
            this.Set_Sub_Panel((uint)SubPanels.GYRO_PANEL);
        }

        private void Button_Additional_Click(object sender, EventArgs e)
        {
            this.Set_Sub_Panel((uint)SubPanels.ADDITIONAL_PANEL);
        }

        private void BackLeftDownBtn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVRearTouch.LEFT_TOUCH +18;
            this.openKeyboard(); 
        }

        private void BackRightDownBtn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVRearTouch.RIGHT_TOUCH +18;
            this.openKeyboard(); 
        }

        private void B13Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B13;
            this.openKeyboard(); 
        }

        private void B14Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B14;
            this.openKeyboard(); 
        }

        private void B1Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B1;
            this.openKeyboard(); 
        }

        private void B2Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B2;
            this.openKeyboard(); 
        }

        private void B3Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B3;
            this.openKeyboard(); 
        }

        private void B4Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B4;
            this.openKeyboard(); 
        }

        private void B5Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B5;
            this.openKeyboard(); 
        }

        private void B6Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B6;
            this.openKeyboard(); 
        }

        private void B7Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B7;
            this.openKeyboard(); 
        }

        private void B8Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B8;
            this.openKeyboard(); 
        }

        private void B9Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B9;
            this.openKeyboard(); 
        }

        private void B10Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B10;
            this.openKeyboard(); 
        }

        private void B11Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B11;
            this.openKeyboard(); 
        }

        private void B12Btn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)PSVKeyType.B12;
            this.openKeyboard(); 
        }

        private void Vita_Panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Gyro_Panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.keyboard.Close();
            Program.CloseApplication();
            //Application.Exit();
        }

        private void GyroXNegBtn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.GYROX_NEG;
            this.openKeyboard(); 
        }

        private void GyroYNegBtn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.GYROY_NEG;
            this.openKeyboard(); 
        }

        private void GyroZNegBtn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.GYROZ_NEG;
            this.openKeyboard(); 
        }

        private void GyroXPosBtn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.GYROX_POS;
            this.openKeyboard(); 
        }

        private void GyroYPosBtn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.GYROY_POS;
            this.openKeyboard(); 
        }

        private void GyroZPosBtn_Click(object sender, EventArgs e)
        {
            lastButtonPressed = (uint)AxisButton.GYROZ_POS;
            this.openKeyboard(); 
        }

        private void Button_Wifi_Click(object sender, EventArgs e)
        {
            this.ConnectionStatusPanel.Location = new Point(450, 265);

            this.ConnectionStatusPanel.Visible = !this.ConnectionStatusPanel.Visible;
        }

        private void ClearBinds_Click(object sender, EventArgs e)
        {
            Program.PSVServer.clearAllBinds();
        }









    }
}
