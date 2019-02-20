namespace test
{
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Windows;
    using WindowsInput;
    using System.Timers;
    using System;

    public partial class MainWindow : Window
    {
        public SerialPort SP = new SerialPort();
        internal InputSimulator sim = new InputSimulator();//for the keyboard

        internal new ACCELERO AX1 = new ACCELERO("accelerometer1");
        internal new ACCELERO AX2 = new ACCELERO("accelerometer2");
        internal new ACCELERO AX3 = new ACCELERO("accelerometer3");

        public int param = 20;

        public static string toLog = null;

        internal class ACCELERO
        {
            public string name { get; set; }
            //constructor
            public ACCELERO(string _name)
            {
                name = _name;
            }
            public int value = 0;
            public int centerCords = 0;
            public bool logEnabled = true;

            Timer count = new Timer();
            public int countTill = 2000;
            
            
            
            public bool valOverParam = false;
            public bool valUnderParam = false;
            public bool inCenter = true;

            public bool up = false;
            public bool down = false;

            public bool goingUp = false;
            public bool UpsRebound = false;
            public bool goingDown = false;
            public bool DownsRebound = false;

            public void UpdateParameterMath(int paramd)
            {

                if (value > (centerCords + paramd))
                {
                    valOverParam = true;
                    valUnderParam = false;
                    inCenter = false;
                    Debug.Write("over!!  ");
                }

                if (value < (centerCords - paramd))
                {
                    valOverParam = false;
                    valUnderParam = true;
                    inCenter = false;
                    Debug.Write("undrr!!  ");
                }
                if (value < (centerCords + paramd) && value > (centerCords - paramd))
                {
                    valOverParam = false;
                    valUnderParam = false;
                    inCenter = true;
                    Debug.Write("center!!  ");
                }

                if (valOverParam && goingUp == false && goingDown == false && UpsRebound == false && DownsRebound == false)
                {
                    goingUp = true;
                    up = true;
                    SetTimer(countTill);
                    if (logEnabled){toLog = name + " is going up";}
                }
                if (valUnderParam && goingUp == false && goingDown == false && UpsRebound == false && DownsRebound == false)
                {
                    goingDown = true;
                    down = true;
                    SetTimer(countTill);
                    if (logEnabled) { toLog = name + " is going down"; }
                }
                if (valUnderParam && goingUp == true)
                {
                    goingUp = false;
                    UpsRebound = true;
                }
                if (valOverParam && goingDown == true)
                {
                    goingDown = false;
                    DownsRebound = true;
                }
                if (inCenter && UpsRebound == true)
                {
                    UpsRebound = false;
                    up = false;
                    count.Stop();
                    
                    if (logEnabled) { toLog = name + " has finished going up"; }
                }
                if (inCenter && DownsRebound == true)
                {
                    DownsRebound = false;
                    down = false;
                    count.Stop();
                    if (logEnabled) { toLog = name + " has finished going down"; }
                }
            }
            void timerFinished(Object source, ElapsedEventArgs e)
            {
                count.Stop();
                RESET();
                if (logEnabled) { toLog = "finished (due to timeout)"; }
            }
            private  void SetTimer(int interval)
            {
                
                count.Interval = interval;
                count.Elapsed += new ElapsedEventHandler(timerFinished);
                count.Start();
            }

            internal void RESET()
            {

                up = false;
                down = false;

                goingUp = false;
                UpsRebound = false;
                goingDown = false;
                DownsRebound = false;
            }
        }

        public MainWindow()
        {
           
            
        }

        private void SP_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // sim.Keyboard.KeyDown(VirtualKeyCode.VK_W);

            
           
            try
            {
                Debug.WriteLine(SP.ReadLine());
                string[] inputList = SP.ReadLine().Split(',');
                AX1.value = int.Parse(inputList[0]);
                AX2.value = int.Parse(inputList[1]);
                AX3.value = int.Parse(inputList[2]);
            }
            catch
            {
                sendToLog("Error reading cereal values");
            }

            //GUI elements
            {
                this.Dispatcher.Invoke(() =>
                {
                    bar1.Value = AX1.value;
                    bar2.Value = AX2.value;
                    bar3.Value = AX3.value;

                    param = (int)paramVAL.Value;
                    val.Content = param;

                    //setting param tick things
                     Debug.Write("  " + AX1.valOverParam);
                    filterup1.IsChecked = AX1.valOverParam;
                    filterup2.IsChecked = AX2.valOverParam;
                    filterup3.IsChecked = AX3.valOverParam;
                    filterDown1.IsChecked = AX1.valUnderParam;
                    filterDown2.IsChecked = AX2.valUnderParam;
                    filterDown3.IsChecked = AX3.valUnderParam;

                    

                });
            }
            //accelorometer ALGO......
            {
                AX1.UpdateParameterMath(param);
                AX2.UpdateParameterMath(param);
                AX3.UpdateParameterMath(param);

                Debug.Write(AX1.up + " " + AX1.down);

            }

            //some debug stuff
            {
                if (toLog != null)
                {
                    sendToLog(toLog);
                    toLog = null;
                }
            }
        }
        void portScan()
        {
            //setting the dropDown COM list....
            this.Dispatcher.Invoke(() =>
            {
                string[] AvailableCOMS = SerialPort.GetPortNames();
                COMguiList.Items.Clear();
                for (int i = 0; i < AvailableCOMS.Length; i++)
                {
                    
                    COMguiList.Items.Insert(i, AvailableCOMS[i]);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AX1.centerCords = AX1.value;
            AX2.centerCords = AX2.value;
            AX3.centerCords = AX3.value;

            sendToLog("center cords have been set to " + AX1.centerCords.ToString() + ", " + AX2.centerCords.ToString() + "  and  " + AX3.centerCords.ToString());
        }
        private void sendToLog(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                logBOX.AppendText(System.Environment.NewLine + System.Environment.NewLine + "#" + message);
                logBOX.ScrollToEnd();

                //enabling/disabling LOG from accelerometers--
                {
                    AX1.logEnabled = AX1check.IsChecked.GetValueOrDefault();
                    AX2.logEnabled = AX2check.IsChecked.GetValueOrDefault();
                    AX3.logEnabled = AX3check.IsChecked.GetValueOrDefault();
                }
            });
        }
        private void KeyBoardEnabe_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Reset_step_Click(object sender, RoutedEventArgs e)
        {
            AX1.RESET();
            AX2.RESET();
            AX3.RESET();
            sendToLog("Reseted all current ALGO values for all axis");
        }
        private void COMguiList_Loaded(object sender, RoutedEventArgs e)
        {
            portScan();
            sendToLog("populating COM list");
        }
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                logBOX.Text = " ";
                
            });
        }
        private void ScanForPorts_Click(object sender, RoutedEventArgs e)
        {
            portScan();
            sendToLog("refreshing COM list");
        }
        private void Enable_COM_Click(object sender, RoutedEventArgs e)
        {
           
            SP.BaudRate = 9600;
            SP.DataReceived += SP_DataReceived;
            try {
                SP.PortName = COMguiList.SelectedValue.ToString();
                SP.Open();
                sendToLog("Connected :)");
            } catch {
                sendToLog("could not connect to COM");
            }
            

        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
        }
        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
        }
        private void CheckBox_Checked_2(object sender, RoutedEventArgs e)
        {
        }
        private void Bar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

       
    }
}
