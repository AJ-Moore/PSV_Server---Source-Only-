using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PSV_Server
{
    class InputEmulation
    {
        //Import some stuff -> MapVirtualKey used for getting scancode.
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        internal static extern int MapVirtualKey(int uCode, int uMapType);

        //Dictionary that holds string values and corresponding key codes. //Ignores case -> NOT CASE SENSATIVE :D 
        Dictionary<String, uint> keyCodes = new Dictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);

        //!< Holds values assoicated with mouse input. 
        Dictionary<String, uint> mouseCodes = new Dictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase); 

        /// <summary>
        /// Import SendInput method
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern uint SendInput(
            uint nInputs,
            [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
            int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            internal uint type;
            internal IUnion input;
            internal static int Size
            {
                get { return Marshal.SizeOf(typeof(INPUT)); }
            }
        }

        
        [StructLayout(LayoutKind.Explicit)]
        internal struct IUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            internal short wVk;
            internal short wScan;
            internal uint dwFlags;
            internal int time;
            internal UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            internal int dx;
            internal int dy;
            internal uint mouseData;
            internal uint dwFlags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            internal int uMsg;
            internal short wParamL;
            internal short wParamH;
        }

        //If no translation returns 0 <-- 
        //uMapType enumeration // see msdn for full details http://msdn.microsoft.com/en-gb/library/windows/desktop/ms646306(v=vs.85).aspx
        enum MapType : uint
        {
            /// <summary>
            /// //uCode is virtual key code translated into unshifted character
            /// </summary>
            MAPVK_VK_TO_CHAR = 0x2, 
            /// <summary>
            /// uCode is virtual key code translated into scan code 
            /// </summary>
            MAPVK_VK_TO_VSC = 0x0, 
            /// <summary>
            /// uCode is scan code and translated into virtual key code (not distinguish left, right hand keys)
            /// </summary>
            MAPVK_VSC_TO_VK = 0x1,
            /// <summary>
            /// uCode is scan code and is translated into virtual key code (does distinguish left, right hand keys)
            /// </summary>
            MAPVK_VSC_TO_VK_EX = 0x3
        }


        enum InputType : uint
        {
            INPUT_MOUSE = 0x0,
            INPUT_KEYBOARD = 0x1,
            INPUT_HARDWARE = 0x2
        }

        //http://msdn.microsoft.com/en-gb/library/windows/desktop/ms646271(v=vs.85).aspx
        enum KeyboardFlags : uint
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002, //If specified key is being released, else pressed
            KEYEVENTF_SCANCODE = 0x0008, //if specified wScan identifies the key and wVK is ignored
            KEYEVENTF_UNICODE = 0x0004
        }

        enum MouseFlags : uint
        {
            MOUSEEVENTF_ABSOLUTE = 0x8000,
            MOUSEEVENTF_HWHEEL = 0x01000, 
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004, 
            MOUSEEVENTF_RIGHTDOWN = 0x0008, 
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020, 
            MOUSEEVENTF_MIDDLEUP = 0x0040, 
            MOUSEEVENTF_VIRTUALDESK = 0x4000, 
            MOUSEEVENTF_WHEEL = 0x0800, 
            MOUSEEVENTF_XDOWN = 0x0080, 
            MOUSEEVENTF_XUP = 0x0100 
        }

        //Additional enumeration to help with (sendMouseInput)
        enum MouseCodes : uint 
        {
            LEFT_CLICK = 0x1, 
            RIGHT_CLICK = 0x2,
            MIDDLE_CLICK = 0x4,
            X_CLICK_1 = 0x8,
            X_CLICK_2 = 0x16,
            SCROLL_UP = 0x32, 
            SCROLL_DOWN = 0x64,
            MOVE_LEFT = 0x128,
            MOVE_RIGHT = 0x256, 
            MOVE_UP = 0x512, 
            MOVE_DOWN = 0x1024
        }

        public InputEmulation()
        {
            this.initialise();   
        }

        /// <summary>
        /// Keycode dictionary.
        /// </summary>
        private void initialise()
        {
            this.keyCodes.Add("LButton", 0x01); //Left Mouse Button 
            this.keyCodes.Add("RButton", 0x02); //Right Mouse Button 
            this.keyCodes.Add("Cancel", 0x3);
            this.keyCodes.Add("MButton", 0x4); //Middle Mouse Button
            this.keyCodes.Add("XButton1", 0x5); //X1 mouse button 
            this.keyCodes.Add("XButton2", 0x6); //X2 mouse button 
            this.keyCodes.Add("Backspace", 0x8); //Back
            this.keyCodes.Add("Tab", 0x9); //Tab
            this.keyCodes.Add("Clear", 0x0C); //Clear
            this.keyCodes.Add("Enter", 0x0D); //Enter/ Return key  
            this.keyCodes.Add("Shift", 0x10); //Shift 
            this.keyCodes.Add("Control", 0x11);
            this.keyCodes.Add("Alt", 0x12);
            this.keyCodes.Add("Pause", 0x13);
            this.keyCodes.Add("Capslock", 0x14);
            this.keyCodes.Add("Kana", 0x15);
            this.keyCodes.Add("Hangul", 0x15);
            this.keyCodes.Add("Junja", 0x17);
            this.keyCodes.Add("Final", 0x18);
            this.keyCodes.Add("Hanja", 0x19);
            this.keyCodes.Add("Kanji", 0x19);
            this.keyCodes.Add("Escape", 0x1B);
            this.keyCodes.Add("Esc", 0x1B);
            this.keyCodes.Add("Convert", 0x1C);
            this.keyCodes.Add("Nonconvert", 0x1D);
            this.keyCodes.Add("Accept", 0x1E);
            this.keyCodes.Add("Modechange", 0x1F);
            this.keyCodes.Add("Space", 0x20);
            this.keyCodes.Add("Pgup", 0x21);
            this.keyCodes.Add("Pgdn", 0x22);
            this.keyCodes.Add("End", 0x23);
            this.keyCodes.Add("Home", 0x24);
            this.keyCodes.Add("Left", 0x25); //Left Arrow Key 
            this.keyCodes.Add("Up", 0x26);
            this.keyCodes.Add("Right", 0x27);
            this.keyCodes.Add("Down", 0x28);
            this.keyCodes.Add("Select", 0x29); //Select key?
            this.keyCodes.Add("Print", 0x2A);
            this.keyCodes.Add("Execute", 0x2B);//Execute key 
            this.keyCodes.Add("Prtscr", 0x2C);
            this.keyCodes.Add("Insert", 0x2D);
            this.keyCodes.Add("Delete", 0x2E);
            this.keyCodes.Add("Del", 0x2E);
            this.keyCodes.Add("Help", 0x2F);
            this.keyCodes.Add("Win", 0x5B); //Left Windows
            this.keyCodes.Add("LWin", 0x5B); //Left Windows 
            this.keyCodes.Add("RWin", 0x5C); //Right Windows
            this.keyCodes.Add("Apps", 0x5D); //Application Key
            this.keyCodes.Add("Sleep", 0x5F);
            this.keyCodes.Add("Num0", 0x60); //NumPAD 0 "" 
            this.keyCodes.Add("Num1", 0x61);
            this.keyCodes.Add("Num2", 0x62);
            this.keyCodes.Add("Num3", 0x63);
            this.keyCodes.Add("Num4", 0x64);
            this.keyCodes.Add("Num5", 0x65);
            this.keyCodes.Add("Num6", 0x66);
            this.keyCodes.Add("Num7", 0x67); 
            this.keyCodes.Add("Num8", 0x68); 
            this.keyCodes.Add("Num9", 0x69);
            this.keyCodes.Add("F1", 0x70); //Function 1// Functions 1 - 12.
            this.keyCodes.Add("F2", 0x71);
            this.keyCodes.Add("F3", 0x72);
            this.keyCodes.Add("F4", 0x73);
            this.keyCodes.Add("F5", 0x74);
            this.keyCodes.Add("F6", 0x75);
            this.keyCodes.Add("F7", 0x76);
            this.keyCodes.Add("F8", 0x77);
            this.keyCodes.Add("F9", 0x78);
            this.keyCodes.Add("F10", 0x79);
            this.keyCodes.Add("F11", 0x7A);
            this.keyCodes.Add("F12", 0x7B); 
            this.keyCodes.Add("Numlock",0x90); 
            this.keyCodes.Add("Scrolllock", 0x91);
            this.keyCodes.Add("LShift", 0xA0);
            this.keyCodes.Add("RShift", 0xA1);
            this.keyCodes.Add("LCtrl", 0xA2);
            this.keyCodes.Add("RCtrl", 0xA3);
            this.keyCodes.Add("0", 0x30);
            this.keyCodes.Add("1", 0x31);
            this.keyCodes.Add("2", 0x32);
            this.keyCodes.Add("3", 0x33);
            this.keyCodes.Add("4", 0x34);
            this.keyCodes.Add("5", 0x35);
            this.keyCodes.Add("6", 0x36);
            this.keyCodes.Add("7", 0x37);
            this.keyCodes.Add("8", 0x38);
            this.keyCodes.Add("9", 0x39);
            this.keyCodes.Add("a", 0x41);
            this.keyCodes.Add("b", 0x42);
            this.keyCodes.Add("c", 0x43);
            this.keyCodes.Add("d", 0x44);
            this.keyCodes.Add("e", 0x45);
            this.keyCodes.Add("f", 0x46);
            this.keyCodes.Add("g", 0x47);
            this.keyCodes.Add("h", 0x48);
            this.keyCodes.Add("i", 0x49);
            this.keyCodes.Add("j", 0x4A);
            this.keyCodes.Add("k", 0x4B);
            this.keyCodes.Add("l", 0x4C);
            this.keyCodes.Add("m", 0x4D);
            this.keyCodes.Add("n", 0x4E);
            this.keyCodes.Add("o", 0x4F);
            this.keyCodes.Add("p", 0x50);
            this.keyCodes.Add("q", 0x51);
            this.keyCodes.Add("r", 0x52);
            this.keyCodes.Add("s", 0x53);
            this.keyCodes.Add("t", 0x54);
            this.keyCodes.Add("u", 0x55);
            this.keyCodes.Add("v", 0x56);
            this.keyCodes.Add("w", 0x57);
            this.keyCodes.Add("x", 0x58);
            this.keyCodes.Add("y", 0x59);
            this.keyCodes.Add("z", 0x5A);

            this.keyCodes.Add(";", 0xBA);
            this.keyCodes.Add("=", 0xBB);
            this.keyCodes.Add("-", 0xBD);
            this.keyCodes.Add("/", 0xBF);
            this.keyCodes.Add("'", 0xC0);
            this.keyCodes.Add("[", 0xDB);
            this.keyCodes.Add(@"\", 0xDC);
            this.keyCodes.Add("]", 0xDD);
            this.keyCodes.Add("#", 0xDE);
            this.keyCodes.Add(",", 0xBC);
            this.keyCodes.Add(".", 0xBE);


            //Mouse Key Codes 
            this.mouseCodes.Add("LClick",       (uint)MouseCodes.LEFT_CLICK);
            this.mouseCodes.Add("RClick",       (uint)MouseCodes.RIGHT_CLICK);
            this.mouseCodes.Add("MClick",       (uint)MouseCodes.MIDDLE_CLICK);
            this.mouseCodes.Add("XClick1",      (uint)MouseCodes.X_CLICK_1); 
            this.mouseCodes.Add("XClick2",      (uint)MouseCodes.X_CLICK_2); 
            this.mouseCodes.Add("ScrollUp",     (uint)MouseCodes.SCROLL_UP); 
            this.mouseCodes.Add("ScrollDown",   (uint)MouseCodes.SCROLL_DOWN);
            this.mouseCodes.Add("MoveLeft",     (uint)MouseCodes.MOVE_LEFT); 
            this.mouseCodes.Add("MoveRight",    (uint)MouseCodes.MOVE_RIGHT); 
            this.mouseCodes.Add("MoveUp",       (uint)MouseCodes.MOVE_UP); 
            this.mouseCodes.Add("MoveDown",     (uint)MouseCodes.MOVE_DOWN);

        }

        
        public void sendInput(string input, bool keyDown)
        {
            if (input.Length == 0)
                return; 

            bool shift = false;
            bool alt = false;
            bool ctrl = false;

            if (input[0] == '+' || input[0] == '%' || input[0] == '^')
            {
                //Iterate through potencial special characters/ break if an opening bracket is found.
                for (int c = 0; c < 3; c++)
                {
                    if (c >= input.Length)
                        break; 

                    if (input[c] == '(' || input[c] == '{')
                        break;

                    if (input[c] == '+')
                        shift = true;

                    if (input[c] == '%')
                        alt = true;

                    if (input[c] == '^')
                        ctrl = true; 
                }
            }

            int keyStart = 0;
            int keyEnd = 0; 

            //work out input 
            for (int c = 0; c < input.Length; c++ )
            {
                if (input[c] == '{' || input[c] == '(')
                    keyStart = c + 1;

                if (input[c] == '}' || input[c] == ')')
                {
                    keyEnd = c;
                    break;//end found break
                }
            }

            if (keyStart == 0 || keyEnd == 0){
                return; 
            }

            char[] keyCodeC = new char[keyEnd - keyStart];
            input.CopyTo(keyStart, keyCodeC,0,keyEnd - keyStart);
            string keyCode = new string (keyCodeC);

            if (keyDown){
                ///firstly send shift/ alt/ ctrl modifier keys (if set)
                if (ctrl)
                    sendKeyDown(this.keyCodes["LCtrl"]);
                if (alt)
                    sendKeyDown(this.keyCodes["Alt"]); 
                if (shift)
                    sendKeyDown(this.keyCodes["Shift"]);

                if (this.keyCodes.ContainsKey(keyCode))
                    sendKeyDown(this.keyCodes[keyCode]);
            }
            else{
                if (ctrl)
                    sendKeyUp(this.keyCodes["LCtrl"]);
                if (alt)
                    sendKeyDown(this.keyCodes["Alt"]); 
                if (shift)
                    sendKeyUp(this.keyCodes["Shift"]);


                if (this.keyCodes.ContainsKey(keyCode))
                    sendKeyUp(this.keyCodes[keyCode]);

            }


        }

        /// <summary>
        /// uint keyCode -> virtual key code -> as defined in keyCodes dictionary.
        /// </summary>
        /// <param name="keyCode"></param>
        public void sendKeyDown(uint keyCode){
            INPUT[] inputs = new INPUT[1]; 

            inputs[0].type = (uint)InputType.INPUT_KEYBOARD;
            inputs[0].input.ki.dwFlags = (uint)KeyboardFlags.KEYEVENTF_SCANCODE;
            inputs[0].input.ki.wScan = (short)MapVirtualKey((int)keyCode, (int)MapType.MAPVK_VK_TO_VSC);
            inputs[0].input.ki.time = 0;// MDSN: The time stamp for event in MS, if zero system provides own.
            //inputs[0].input.ki.dwExtraInfo//Extra info? none passed 

            //Think thats it?
            SendInput(1, inputs, INPUT.Size);
        }

        public void sendKeyUp(uint keyCode)
        {
            INPUT[] inputs = new INPUT[1];

            inputs[0].type = (uint)InputType.INPUT_KEYBOARD;
            inputs[0].input.ki.dwFlags = (uint)KeyboardFlags.KEYEVENTF_SCANCODE | (uint)KeyboardFlags.KEYEVENTF_KEYUP;
            inputs[0].input.ki.wScan = (short)MapVirtualKey((int)keyCode, (int)MapType.MAPVK_VK_TO_VSC);
            inputs[0].input.ki.time = 0;// MDSN: The time stamp for event in MS, if zero system provides own.
            //inputs[0].input.ki.dwExtraInfo//Extra info? none passed 

            //Think thats it?
            SendInput(1, inputs, INPUT.Size);
        }

        /// <summary>
        /// Used for sending mouse events to windows 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="keyDown"></param>
        public void sendMouseInput(string input, bool keyDown, float keyVal){

            int scrollSpeed = 120;
            float mouseSpeed = (8.0f * Math.Abs(keyVal)); 

            
            uint _input;
            if (this.mouseCodes.ContainsKey(input))
                _input = this.mouseCodes[input];
            else
                return; //Not a valid input 


            //!< HOLD MOUSE INPUT EVENT DATA 
            int _dx = 0;
            int _dy = 0;
            uint _mouseData = 0;
            uint _mouseFlags = 0; 


            if (keyDown)
            {
                //Select event type
                if (_input <= 0x16) //<! If less than or equal to 16 must be a button event.
                {
                    switch(_input)
                    {
                        case (uint)MouseCodes.LEFT_CLICK:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_LEFTDOWN;
                            break; 
                        case (uint)MouseCodes.RIGHT_CLICK:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_RIGHTDOWN;
                            break; 
                        case (uint)MouseCodes.MIDDLE_CLICK:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_MIDDLEDOWN;
                            break; 
                        case (uint)MouseCodes.X_CLICK_1:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_XDOWN;
                            _mouseData = 0x0001;//Indication of which x button is pressed...
                            break; 
                        case (uint)MouseCodes.X_CLICK_2:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_XDOWN;
                            _mouseData = 0x0001;// ditto 
                            break; 
                        default:
                            break; 
                    }
                }
                else if (_input <= 0x64) //must be a mouse scroll event
                {
                    switch (_input)
                    {
                        case (uint)MouseCodes.SCROLL_UP:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_WHEEL;
                            _mouseData = (uint)scrollSpeed; 
                            break; 
                        case (uint)MouseCodes.SCROLL_DOWN:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_WHEEL;
                            _mouseData = (uint)-scrollSpeed; 
                            break; 
                        default:
                            break; 
                    }
                }
                else //Must be a mouse move event 
                {
                    _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_MOVE;

                    switch (_input)
                    {
                        case (uint)MouseCodes.MOVE_LEFT:
                            _dx = (int)-mouseSpeed; 
                            break;
                        case (uint)MouseCodes.MOVE_RIGHT:
                            _dx = (int)mouseSpeed;
                            break;
                        case (uint)MouseCodes.MOVE_UP:
                            _dy = (int)-mouseSpeed; 
                            break;
                        case (uint)MouseCodes.MOVE_DOWN:
                            _dy = (int)mouseSpeed; 
                            break;

                    }

                }

            }
            else
            {
                //!< We only care about button events on key release 
                if (_input <= 16){
                    switch (_input)
                    {
                        case (uint)MouseCodes.LEFT_CLICK:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_LEFTUP;
                            break;
                        case (uint)MouseCodes.RIGHT_CLICK:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_RIGHTUP;
                            break;
                        case (uint)MouseCodes.MIDDLE_CLICK:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_MIDDLEUP;
                            break;
                        case (uint)MouseCodes.X_CLICK_1:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_XUP;
                            _mouseData = 0x0001;//Indication of which x button is pressed...
                            break;
                        case (uint)MouseCodes.X_CLICK_2:
                            _mouseFlags |= (uint)MouseFlags.MOUSEEVENTF_XUP;
                            _mouseData = 0x0001;// ditto 
                            break;
                        default:
                            break;
                    }
                }
                else{
                    //done
                    
                }

            }



            //!< Send Event
            INPUT[] inputs = new INPUT[1];

            inputs[0].type = (uint)InputType.INPUT_MOUSE;
            inputs[0].input.mi.dx = _dx;
            inputs[0].input.mi.dy = _dy;
            inputs[0].input.mi.mouseData = _mouseData;
            inputs[0].input.mi.dwFlags = _mouseFlags;
            inputs[0].input.mi.time = 0;

            SendInput(1, inputs, INPUT.Size);




        }

    }
}
