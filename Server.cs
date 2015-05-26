using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using System.IO;

using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

using VJoyPad;

namespace PSV_Server
{
    //Created a struct to hold the server configuration data...
    struct Configuration
    {
        public uint clientTimeOut;
        public uint keyRepeatDelayMS; 

        //DEADZONES  0 - 1.0f 
        public float LSDeadZone; //Left Stick Dead Zone
        public float RSDeadZone; //Right Stick Dead Zone 

        //Gyro DEADZONES 
        public float gyroXDeadZone;
        public float gyroYDeadZone;
        public float gyroZDeadZone;

        public bool gyroXEnabled;
        public bool gyroYEnabled;
        public bool gyroZEnabled; 

    }


    class Server
    {
        // DLL Imports/ External 
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        //

        //!< Bind Dictionarys - Holds Buttons ID's and Bound input events
        // uint : PSVKeyType, String : IdToIdentifyButton(Similar syntax to sendkeys)
        Dictionary<uint, string> keyboardBinds = new Dictionary<uint, string>();
        Dictionary<uint, string> mouseBinds = new Dictionary<uint, string>(); 

        //Threading 
        private TcpListener tcpListener;
        private Thread listenThread;

        //Client Attributes 
        private int clientNum = 0;

        string[] clientIP = new string[2];
        bool[] activeClient = new bool[2];
        TcpClient[] tcpClients = new TcpClient[2]; 
        Stopwatch[] clientTimeOutSW = new Stopwatch[2];
        

        //!< PSVPAD 
        VJoy psvpad = new VJoy(); //!< The Virtual Joypad
        InputEmulation Input = new InputEmulation(); //!< Emulates key down/up events
        public InputData data;//Holds the data recieved from client
        public InputData previousInput;//holds the previous input data
        public Configuration config = new Configuration(); 

        //!< ASCII Encoder for all your encoding needs?
        ASCIIEncoding encoder = new ASCIIEncoding();

        //Various Timers 
        Stopwatch psvpadsw;
        Stopwatch keyRepeatTimer = new Stopwatch(); 
        int timeout = 10;//how many miliseconds does it take to time out. 


        

        public void addBind(uint keyType, string keys)
        {
            if (this.keyboardBinds.ContainsKey(keyType)){
                this.keyboardBinds[keyType] = keys; 
                return; 
            }
            this.keyboardBinds.Add(keyType, keys); 
        }

        public void removeBind(uint keyType)
        {
            if (this.keyboardBinds.ContainsKey(keyType))
            {
                this.keyboardBinds.Remove(keyType);
                return; 
            }

            if (this.mouseBinds.ContainsKey(keyType))
            {
                this.mouseBinds.Remove(keyType);
                return; 
            }
        }

        //Clears all binds
        public void clearAllBinds()
        {
            this.keyboardBinds.Clear();
            this.mouseBinds.Clear();

            foreach (KeyValuePair<uint, Button> entry in PSVServer.buttonDict)
            {
                entry.Value.Text = "";
            }
        }

        //!< Naturally the mouse binding was added later 
        public void addMouseBind(uint keyType, string keys)
        {
            if (this.mouseBinds.ContainsKey(keyType))
            {
                this.mouseBinds[keyType] = keys;
                return; 
            }
            this.mouseBinds.Add(keyType, keys); 
        }

        //there was lots of duplicate code... clean up someday sigh
        //function used to toggle button attributes i.e. colour and call any bound binds in dictionary
        //Updates button// calls binds etc... 
        private void updateButton(uint ButtonKey, bool Pressed, float ButtonValue)
        {
            if (PSVServer.buttonDict.ContainsKey(ButtonKey))
            {
                Button button = PSVServer.buttonDict[ButtonKey];
                if (Pressed)
                {
                    if (button.BackColor != Color.Green)
                        button.BackColor = Color.Green;
                }
                else
                {
                    if (button.BackColor != Color.Transparent)
                        button.BackColor = Color.Transparent;
                }

            }

            if (this.keyboardBinds.ContainsKey(ButtonKey))
            {
                //Sigh - Does the job 
                /*if (Pressed && !keyRepeat)
                {
                }*/
                //else

                {
                    Input.sendInput(this.keyboardBinds[ButtonKey], Pressed);
                }
            }

            if (this.mouseBinds.ContainsKey(ButtonKey))
            {
                Input.sendMouseInput(this.mouseBinds[ButtonKey], Pressed, ButtonValue);
            }

        }

        //!< Used to determine if a keyDown event should occur
        //!< Only applys to keyboard keyDown events.
        //!< Awkward but needs access elsewhere :/
        private bool keyRepeat = false;

        //Check whether the key has been pressed if so pass send keys.
        public void checkBinds(InputData Data)
        {
            if (this.previousInput == null)
                this.previousInput = Data;

            keyRepeat = false;
            if (!keyRepeatTimer.IsRunning)
            {
                keyRepeatTimer.Start();
            }
            if (keyRepeatTimer.ElapsedMilliseconds >= this.config.keyRepeatDelayMS)
            {
                keyRepeat = true;
                keyRepeatTimer.Reset(); 
            }           

            IntPtr window = GetActiveWindow();
            SetForegroundWindow(window);

            #region Check KeyData Binds
            //Check button binds (keyData). 
            for (int i = 0; i < 32; i++)
            {
                
                if ((Data.keyData & (uint)(0x1 << i)) != 0)
                {
                    if (PSVServer.buttonDict.ContainsKey((uint)0x1 << i)){
                        Button button = PSVServer.buttonDict[(uint)0x1 << i];
                        if (button.BackColor != Color.Green)
                            button.BackColor = Color.Green;
                    }

                    if (this.keyboardBinds.ContainsKey((uint)0x1 << i))
                    {
                        //SendKeys.SendWait(this.keyboardBinds[(uint)0x1 << i]);
                        //////////////////////////////////////////ONLY CHECK KEY REPEAT IF PREVIOUS STATE was NOT PRESSED... TODO -> Done ./
                        if (keyRepeat || ((this.previousInput.keyData & (uint)(0x1 << i)) == 0)) 
                            Input.sendInput(this.keyboardBinds[(uint)0x1 << i], true); 
                    }

                    if (this.mouseBinds.ContainsKey((uint)0x1 << i))
                    {
                        Input.sendMouseInput(this.mouseBinds[(uint)0x1 << i], true, 1); 
                    }

                }
                else
                {
                    if (PSVServer.buttonDict.ContainsKey((uint)0x1 << i))
                    {
                        Button button = PSVServer.buttonDict[(uint)0x1 << i];
                        if (button.BackColor != Color.Transparent)
                            button.BackColor = Color.Transparent;
                    }

                    //Check if key has been released by checking if the keyData was previously set.
                    if (this.previousInput != null)
                    if ((this.previousInput.keyData & (uint)(0x1 << i)) != 0)
                    {
                        if (this.keyboardBinds.ContainsKey((uint)0x1 << i))
                        {
                            Input.sendInput(this.keyboardBinds[(uint)0x1 << i], false);
                        }

                        if (this.mouseBinds.ContainsKey((uint)0x1 << i))
                        {
                            Input.sendMouseInput(this.mouseBinds[(uint)0x1 << i], false,1);
                        }
                    }
                }


            }
            #endregion

            #region check axis binds && Motion/ Gyro Binds 
            if (Data.lx > this.config.LSDeadZone){
                //if (keyRepeat || this.previousInput.lx == 0) 
                this.updateButton((uint)PSVServer.AxisButton.LX_POS, true, Data.lx);
            }
            else if (this.previousInput.lx > this.config.LSDeadZone)
            {
                this.updateButton((uint)PSVServer.AxisButton.LX_POS, false, Data.lx);
            }

            if (Data.lx < -this.config.LSDeadZone)
            {
                //if (keyRepeat || this.previousInput.lx == 0) 
                this.updateButton((uint)PSVServer.AxisButton.LX_NEG, true, Data.lx);
            }
            else if (this.previousInput.lx < -this.config.LSDeadZone)
            {
                this.updateButton((uint)PSVServer.AxisButton.LX_NEG, false, Data.lx);
            }

            if (Data.ly > this.config.LSDeadZone)
            {
                //if (keyRepeat || this.previousInput.ly == 0) 
                this.updateButton((uint)PSVServer.AxisButton.LY_NEG, true, Data.ly);
            }
            else if (this.previousInput.ly > this.config.LSDeadZone)
            {
                this.updateButton((uint)PSVServer.AxisButton.LY_NEG, false, Data.ly);
            }

            if (Data.ly < -this.config.LSDeadZone)
            {
                this.updateButton((uint)PSVServer.AxisButton.LY_POS, true, Data.ly);
            }
            else if (this.previousInput.ly < -this.config.LSDeadZone)
            {
                //if (keyRepeat || this.previousInput.ly == 0) 
                this.updateButton((uint)PSVServer.AxisButton.LY_POS, false, Data.ly);
            }

            //Right stick 
            if (Data.rx > this.config.RSDeadZone)
            {
                //if (keyRepeat || this.previousInput.rx == 0) 
                this.updateButton((uint)PSVServer.AxisButton.RX_POS, true, Data.rx);
            }
            else if (this.previousInput.rx > this.config.RSDeadZone)
            {
                //if (keyRepeat || this.previousInput.rx == 0) 
                this.updateButton((uint)PSVServer.AxisButton.RX_POS, false, Data.rx);
            }

            if (Data.rx < -this.config.RSDeadZone)
            {
                //if (keyRepeat || this.previousInput.rx == 0) 
                this.updateButton((uint)PSVServer.AxisButton.RX_NEG, true, Data.rx);
            }
            else if (this.previousInput.rx < -this.config.RSDeadZone)
            {
                this.updateButton((uint)PSVServer.AxisButton.RX_NEG, false, Data.rx);
            }

            if (Data.ry > this.config.RSDeadZone)
            {
                //if (keyRepeat || this.previousInput.ry == 0) 
                this.updateButton((uint)PSVServer.AxisButton.RY_NEG, true, Data.ry);
            }
            else if (this.previousInput.ry > this.config.RSDeadZone)
            {
                this.updateButton((uint)PSVServer.AxisButton.RY_NEG, false, Data.ry);
            }

            if (Data.ry < -this.config.RSDeadZone)
            { 
                //if (keyRepeat || this.previousInput.ry == 0) 
                    this.updateButton((uint)PSVServer.AxisButton.RY_POS, true, Data.ry);
            }
            else if (this.previousInput.ry < -this.config.RSDeadZone)
            {
                this.updateButton((uint)PSVServer.AxisButton.RY_POS, false, Data.ry);
            }


            //Check Gyro binds 
            if (this.config.gyroXEnabled)
            {
                if (Data.motionX > this.config.gyroXDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROX_POS, true, Data.motionX);
                }
                else if (this.previousInput.motionX > this.config.gyroXDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROX_POS, false, Data.motionX);
                }

                if (Data.motionX < -this.config.gyroXDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROX_NEG, true, Data.motionX);
                }
                else if (this.previousInput.motionX < -this.config.gyroXDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROX_NEG, false, Data.motionX);
                }
            }
            else
            {
                this.updateButton((uint)PSVServer.AxisButton.GYROX_POS, false, 0);
                this.updateButton((uint)PSVServer.AxisButton.GYROX_NEG, false, 0);
            }


            if (this.config.gyroYEnabled)
            {
                if (Data.motionY > this.config.gyroYDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROY_POS, true, Data.motionY);
                }
                else if (this.previousInput.motionY > this.config.gyroYDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROY_POS, false, Data.motionY);
                }

                if (Data.motionY < -this.config.gyroYDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROY_NEG, true, Data.motionY);
                }
                else if (this.previousInput.motionY < -this.config.gyroYDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROY_NEG, false, Data.motionY);
                }
            }
            else
            {
                this.updateButton((uint)PSVServer.AxisButton.GYROY_POS, false, 0);
                this.updateButton((uint)PSVServer.AxisButton.GYROY_NEG, false, 0);
            }


            if (this.config.gyroZEnabled)
            {
                if (Data.motionZ > this.config.gyroZDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROZ_POS, true, Data.motionZ);
                }
                else if (this.previousInput.motionZ > this.config.gyroZDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROZ_POS, false, Data.motionZ);
                }

                if (Data.motionZ < -this.config.gyroZDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROZ_NEG, true, Data.motionZ);
                }
                else if (this.previousInput.motionZ < -this.config.gyroZDeadZone)
                {
                    this.updateButton((uint)PSVServer.AxisButton.GYROZ_NEG, false, Data.motionZ);
                }

            }
            else
            {
                this.updateButton((uint)PSVServer.AxisButton.GYROZ_POS, false, 0);
                this.updateButton((uint)PSVServer.AxisButton.GYROZ_NEG, false, 0);
            }
            #endregion 

            #region Rear Touch Binds 
            for (int i = 0; i < 4; i++)
            {
                if ((Data.rearTouch & (uint)(0x1 << i)) != 0)
                {
                    this.updateButton(((uint)(0x1 << i) + 18), true, 1.0f);
                }
                else
                {
                    if (this.previousInput != null)
                    {
                        if ((this.previousInput.rearTouch & (uint)(0x1 << i)) != 0)
                        {
                            this.updateButton(((uint)(0x1 << i) + 18), false, 1.0f);
                        }
                    }
                }
            }
            #endregion 

            this.previousInput = Data;

        }

        //Constructor 
        public Server()
        {
            //Configuration 
            this.config.clientTimeOut = 300;
            this.config.keyRepeatDelayMS = 200; 
            this.config.LSDeadZone = 0.0f;
            this.config.RSDeadZone = 0.0f;
            this.config.gyroXDeadZone = 0.0f;
            this.config.gyroYDeadZone = 0.0f;
            this.config.gyroZDeadZone = 0.0f;
            this.config.gyroXEnabled = false;
            this.config.gyroYEnabled = false;
            this.config.gyroZEnabled = false;


            psvpad.Initialize();
            psvpad.Reset();
            psvpad.Update(0);
            psvpad.Update(1);

            psvpadsw = new Stopwatch();
            psvpadsw.Start();

            Console.WriteLine("PSV Pad Server Started");
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

        }

        //!< Listen for clients throughout program duration
        private void ListenForClients()
        {
            Console.WriteLine("Listening for Clients");
            //Start the tcp listener
            this.tcpListener.Start();

            while (Program.isRunning)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();

                IPEndPoint ep = (IPEndPoint)client.Client.RemoteEndPoint;
                string address = ep.Address.ToString();
                Console.WriteLine("PsVita IP: "+address);

                PSVServer.isConnected = true;
                PSVServer.refresh();

                if (clientIP[0] == address || clientIP[1] == address)
                {
                    Console.WriteLine("Error: Client already connected");
                    Console.WriteLine("This is all a bit buggy... If your having problems restart app (:");
                    client.Close();
                    continue; 
                }

                if (clientIP[0] == "")
                {
                    clientNum = 0; 
                }

               /* if (clientIP[0] == address){
                    Console.WriteLine("Error: Client already connected");
                    Console.WriteLine("This is all a bit buggy... If your having problems restart app (:");
                    clientNum = 0; 
                }
                else if (clientIP[1] == address)
                {
                    Console.WriteLine("Error: Client already connected");
                    Console.WriteLine("This is all a bit buggy... If your having problems restart app (:");
                    clientNum = 1; 
                }*/

                clientIP[clientNum] = address;
                tcpClients[clientNum] = client; 

                //start the timeout timer. 
                
                if (clientTimeOutSW[clientNum] == null)
                {
                    clientTimeOutSW[clientNum] = new Stopwatch();
                }
                clientTimeOutSW[clientNum].Reset();
                clientTimeOutSW[clientNum].Start();


                clientNum++;

                if (clientNum > 1)
                {
                    clientNum = 0; 
                }

                //create a thread to handle the communication with the client 
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);

            }
        }

        private void disconnectClient(int clientID, TcpClient client)
        {
            if (recorder.isRecording)
            {
                recorder.stopRecording();
                recorder.audioQueue.Clear(); 
            }

            PSVServer.isConnected = false;
            PSVServer.refresh();
            clientIP[clientID] = "";
            psvpad.Reset(clientID);
            client.Close();
        }

        //!< Handles each clients communication with the server.
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            IPEndPoint ep = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
            string address = ep.Address.ToString();

            clientStream.WriteTimeout = 200; 

            //Work out which client is currently communicating with server...
            int clientID = 0; 
            if (address == clientIP[0])
            {
                clientID = 0;
            }
            else
            {
                clientID = 1; 
            }



            //!< Byte array holds data recieved from client 
            byte[] message = new byte[256];
            int bytesRead; // Number of bytes read 

            //!< Client connected confirmation 
            Console.WriteLine("Connected to Client...");

            //create new Joypad + delete old 
            psvpad = new VJoy();
            psvpad.Initialize();
            psvpadsw.Reset();
            psvpadsw.Start();

            while (Program.isRunning)
            {
                
                //!< If no messages recieved for (clientTimeOut) period then close disconnect
                if (clientTimeOutSW[clientID].ElapsedMilliseconds > this.config.clientTimeOut)
                {
                    //close connection --> get rid of thread? . 
                    Console.WriteLine("Connection to Client Timed Out");
                    this.disconnectClient(clientID, tcpClient);
                    return;  
                }

                //!< Reset bytes read to 0
                bytesRead = 0; 

                //Thread.Sleep(10);
                try
                {
                    //!< Continues to read from stream until full message is recieved 
                    int read = 0;
                    while (read < 256)
                    {
                        bytesRead = clientStream.Read(message, read, message.Length - read);
                        read += bytesRead;
                        if (bytesRead == 0)
                            break;
                    }

                }
                catch
                {
                    //Console.WriteLine("Socket Error-> connection may be weak, checking stream again...");
                    bytesRead = 0;       
                }

                if (bytesRead == 0)
                {
                    //the client has probably disconnected from the server
                    //Console.WriteLine("No bytes read, trying again...");             
                }

                //message has successfully been received
                if (bytesRead != 0)
                {
                    //reset the timer if a message is recieved from the client. 
                    clientTimeOutSW[clientID].Reset();
                    clientTimeOutSW[clientID].Start(); 

                    //!< Sends a confirmation message to the client.
                    SendConfirmationToClient(clientStream);

                    //wait for data. 
                    data = toInputData(message);
                    if (data != null)
                    {
                        psvpad.SetButton(clientID, 0, (bitAnd((uint)PSVKeyType.Cross, data.keyData)));
                        psvpad.SetButton(clientID, 1, (bitAnd((uint)PSVKeyType.Circle, data.keyData)));
                        psvpad.SetButton(clientID, 2, (bitAnd((uint)PSVKeyType.Square, data.keyData)));
                        psvpad.SetButton(clientID, 3, (bitAnd((uint)PSVKeyType.Triangle, data.keyData)));
                        psvpad.SetButton(clientID, 4, (bitAnd((uint)PSVKeyType.L, data.keyData)));
                        psvpad.SetButton(clientID, 5, (bitAnd((uint)PSVKeyType.R, data.keyData)));
                        psvpad.SetButton(clientID, 6, (bitAnd((uint)PSVKeyType.Start, data.keyData)));
                        psvpad.SetButton(clientID, 7, (bitAnd((uint)PSVKeyType.Select, data.keyData)));
                        psvpad.SetButton(clientID, 8, (bitAnd((uint)PSVKeyType.Right, data.keyData)));
                        psvpad.SetButton(clientID, 9, (bitAnd((uint)PSVKeyType.Left, data.keyData)));
                        psvpad.SetButton(clientID, 10, (bitAnd((uint)PSVKeyType.Up, data.keyData)));
                        psvpad.SetButton(clientID, 11, (bitAnd((uint)PSVKeyType.Down, data.keyData)));

                        psvpad.SetButton(clientID, 12, (bitAnd((uint)PSVKeyType.B1, data.keyData)));
                        psvpad.SetButton(clientID, 13, (bitAnd((uint)PSVKeyType.B2, data.keyData)));
                        psvpad.SetButton(clientID, 14, (bitAnd((uint)PSVKeyType.B3, data.keyData)));
                        psvpad.SetButton(clientID, 15, (bitAnd((uint)PSVKeyType.B4, data.keyData)));
                        psvpad.SetButton(clientID, 16, (bitAnd((uint)PSVKeyType.B5, data.keyData)));
                        psvpad.SetButton(clientID, 17, (bitAnd((uint)PSVKeyType.B6, data.keyData)));
                        psvpad.SetButton(clientID, 18, (bitAnd((uint)PSVKeyType.B7, data.keyData)));
                        psvpad.SetButton(clientID, 19, (bitAnd((uint)PSVKeyType.B8, data.keyData)));
                        psvpad.SetButton(clientID, 20, (bitAnd((uint)PSVKeyType.B9, data.keyData)));
                        psvpad.SetButton(clientID, 21, (bitAnd((uint)PSVKeyType.B10, data.keyData)));
                        psvpad.SetButton(clientID, 22, (bitAnd((uint)PSVKeyType.B11, data.keyData)));
                        psvpad.SetButton(clientID, 23, (bitAnd((uint)PSVKeyType.B12, data.keyData)));
                        psvpad.SetButton(clientID, 24, (bitAnd((uint)PSVKeyType.B13, data.keyData)));
                        psvpad.SetButton(clientID, 25, (bitAnd((uint)PSVKeyType.B14, data.keyData)));

                        // Analog Input 
                        psvpad.SetYAxis(clientID,(short)( data.ly * 32767));
                        psvpad.SetXAxis(clientID,(short)( data.lx * 32767));
                        psvpad.SetXRotation(clientID,(short)( data.rx * 32767));
                        psvpad.SetYRotation(clientID,(short)(data.ry * 32767));

                        //Rear Touch Input 
                        psvpad.SetButton(clientID, 26, (bitAnd((uint)PSVRearTouch.SWIPE_UP, data.rearTouch)));
                        psvpad.SetButton(clientID, 27, (bitAnd((uint)PSVRearTouch.SWIPE_DOWN, data.rearTouch)));
                        psvpad.SetButton(clientID, 28, (bitAnd((uint)PSVRearTouch.LEFT_TOUCH, data.rearTouch)));
                        psvpad.SetButton(clientID, 29, (bitAnd((uint)PSVRearTouch.RIGHT_TOUCH, data.rearTouch)));

                        //Axises used for vita motion/ Gyro controls
                        if (this.config.gyroXEnabled)
                            psvpad.SetZRotation(clientID, (short)(data.motionX * 32767));
                        else
                            psvpad.SetZRotation(clientID, 0);

                        if (this.config.gyroYEnabled)
                            psvpad.SetSlider(clientID, (short)(data.motionY * 32767));
                        else
                            psvpad.SetSlider(clientID, 0);

                        if (this.config.gyroZEnabled)
                            psvpad.SetDial(clientID, (short)(data.motionZ * 32767));
                        else
                            psvpad.SetDial(clientID, 0);

                        //Set Keyboard data 
                        if (data.keyboardDat != 0)
                        {
                            this.Input.sendKeyDown((uint)data.keyboardDat);
                            this.Input.sendKeyUp((uint)data.keyboardDat); 
                            //Schedule the key up event so its actually detected by windows...keep code will use later for something else 
                            /*System.Threading.Timer timer = new System.Threading.Timer(
                                new TimerCallback(KeyboardUpScheduled_Callback), data.keyboardDat, 100, System.Threading.Timeout.Infinite);*/
                        }

                        this.checkBinds(data);

                    }

                    if (psvpadsw.ElapsedMilliseconds > 240000)
                    {
                        //create new Joypad + delete old 
                        psvpad.Shutdown();
                        psvpad = new VJoy();
                        psvpad.Initialize();
                        psvpadsw.Reset();
                        psvpadsw.Start();
                    }
                }
                
                //Set the Hat
                psvpad.SetPOV(clientID, 0, VJoy.POVType.Nil);
          
                //Updates joypads
                psvpad.Update(clientID);
                //psvpad.Reset();

            }
            //not hit 
            this.disconnectClient(clientID, tcpClient);
        }

        /*private void KeyboardUpScheduled_Callback(object State){ WASN'T NEEDED IN END
            byte val = (byte)State; 
            this.Input.sendInput("{" + (Convert.ToChar(val).ToString() + "}"), false);
        }
        */
        //!< Logical And Operation 
        private bool bitAnd(uint one,uint two){
            if ((one & two) != 0)
            {
                return true;
            }

            return false;
        }

        //!< Depricated 
        private InputData WaitForData(NetworkStream clientStream)
        {

            byte[] message = new byte[256];
            int bytesRead;

            InputData val = null;
            bool recieved = false;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Waits for the value to be sent to the value coresponding to the AnalogRightY position
            while (!recieved && stopwatch.ElapsedMilliseconds < timeout)
            {
                try
                {
                    bytesRead = clientStream.Read(message, 0, 256);
                    recieved = true;
                    val = toInputData(message);
                    //Console.WriteLine(val);
                    SendConfirmationToClient(clientStream);
                }
                catch
                {
                    recieved = false;
                }

            }

            return val;
        }

        //Sound testing 
        AudioRecorder recorder = new AudioRecorder();

        //!< Sends confirmation to client 
        private void SendConfirmationToClient(NetworkStream clientStream)
        {
            byte[] buffer;

            if (!recorder.isRecording)
                recorder.startRecording();

            if (recorder.audioQueue.Count > 0)
            {
                buffer = recorder.audioQueue.Dequeue();
            }
            else
            {
                buffer = encoder.GetBytes("OK"); 
            }


            /*if (!recorder.isRecording)
            {
                //copy stream to client 
                buffer = recorder.getAudioBuffer();
                //start recording 
                recorder.startRecording();
            }
            else
            {
                buffer = encoder.GetBytes("OK");
            }*/
           
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
            
        }


        public byte[] toByteArray(InputData data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            formatter.Serialize(stream, data);
            byte[] byteData = stream.GetBuffer();

            stream.Close();

            return byteData;
        }

        public InputData toInputData(byte[] array)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(array);
            formatter.Binder = new vitaAssemblyBinder();
            //stream.Seek(0, SeekOrigin.Begin);
            stream.Position = 0;
            InputData _data = null;
            try
            {
                _data = (InputData)formatter.Deserialize(stream);
            }
            catch
            {
                Console.WriteLine("Error Deserializing");
            }
            stream.Close();

            return _data;
        }

    }
}
