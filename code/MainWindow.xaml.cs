namespace test
{
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Windows;
    using WindowsInput;

    public partial class MainWindow : Window
    {
        public SerialPort SP = new SerialPort();

        internal InputSimulator sim = new InputSimulator();//for the keyboard

        internal new ACCELERO AX1 = new ACCELERO("accelerometer1");
        internal new ACCELERO AX2 = new ACCELERO("accelerometer2");
        internal new ACCELERO AX3 = new ACCELERO("accelerometer3");

        public int param = 20;

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
                }
                if (valUnderParam && goingUp == false && goingDown == false && UpsRebound == false && DownsRebound == false)
                {
                    goingDown = true;
                    down = true;
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
                }
                if (inCenter && DownsRebound == true)
                {
                    DownsRebound = false;
                    down = false;
                }
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
            SP.PortName = "COM3";//Set your board COM
            SP.BaudRate = 9600;
            SP.DataReceived += SP_DataReceived;
            SP.Open();
        }

        private void SP_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // sim.Keyboard.KeyDown(VirtualKeyCode.VK_W);

            Debug.WriteLine(SP.ReadLine());
            string[] inputList = SP.ReadLine().Split(',');

            AX1.value = int.Parse(inputList[0]);
            AX2.value = int.Parse(inputList[1]);
            AX3.value = int.Parse(inputList[2]);

            //GUI elements
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
            //accelorometer ALGO......
            {
                AX1.UpdateParameterMath(param);
                AX2.UpdateParameterMath(param);
                AX3.UpdateParameterMath(param);

                Debug.Write(AX1.up + " " + AX1.down);

            }
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
            logBOX.AppendText(System.Environment.NewLine + "#" + message);
            logBOX.ScrollToEnd();
        }

        private void KeyBoardEnabe_Click(object sender, RoutedEventArgs e)
        {
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

        private void Reset_step_Click(object sender, RoutedEventArgs e)
        {
            AX1.RESET();
            AX2.RESET();
            AX3.RESET();
        }
    }
}
