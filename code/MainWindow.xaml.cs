using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using WindowsInput;

namespace test
{
    ///  put this in another file
    ///////////////////
    // you would port this service out to as wee so other service could use it keep it here for now

    public class AppLoggerService
    {
        public void SendToLog()
        {
            Debug.Write("Sent");
        }
    }
    public class AccelleroService
    {
        private readonly AppLoggerService _appLogger = new AppLoggerService();

        public ACCELERO UpdateParameterMath(int paramd, ACCELERO accelero)
        {
            if (accelero.value > (accelero.centerCords + accelero.paramd))
            {
                accelero.valOverParam = true;
                accelero.valUnderParam = false;
                accelero.inCenter = false;
                Debug.Write("over!!  ");
            }
            if (accelero.value < (accelero.centerCords - accelero.paramd))
            {
                accelero.valOverParam = false;
                accelero.valUnderParam = true;
                accelero.inCenter = false;
                Debug.Write("undrr!!  ");
            }
            if (accelero.value < (accelero.centerCords + accelero.paramd) && accelero.value > (accelero.centerCords - accelero.paramd))
            {
                accelero.valOverParam = false;
                accelero.valUnderParam = false;
                accelero.inCenter = true;
                Debug.Write("center!!  ");
            }
            if (accelero.valOverParam && accelero.goingUp == false && accelero.goingDown == false && accelero.UpsRebound == false && accelero.DownsRebound == false)
            {
                accelero.goingUp = true;
                accelero.up = true;
                _appLogger.sendToLog("yooo");
            }
            if (accelero.valUnderParam && accelero.goingUp == false && accelero.goingDown == false && accelero.UpsRebound == false && accelero.DownsRebound == false)
            {
                accelero.goingDown = true;
                accelero.down = true;
            }
            if (accelero.valUnderParam && accelero.goingUp == true)
            {
                accelero.goingUp = false;
                accelero.UpsRebound = true;
            }
            if (accelero.valOverParam && accelero.goingDown == true)
            {
                accelero.goingDown = false;
                accelero.DownsRebound = true;
            }
            if (accelero.inCenter && accelero.UpsRebound == true)
            {
                accelero.UpsRebound = false;
                accelero.up = false;
            }
            if (accelero.inCenter && accelero.DownsRebound == true)
            {
                accelero.DownsRebound = false;
                accelero.down = false;
            }
            return accelero;
        }
        public ACCELERO RESET(ACCELERO accelero)
        {
            accelero.up = false;
            accelero.down = false;
            accelero.goingUp = false;
            accelero.UpsRebound = false;
            accelero.goingDown = false;
            accelero.DownsRebound = false;
            return accelero;
        }
    }
    public class ACCELERO
    {
        public string name { get; set; }
        public int value { get; set; }
        public int centerCords { get; set; }
        public bool valOverParam { get; set; }
        public bool valUnderParam { get; set; }
        public bool inCenter { get; set; }
        public bool up { get; set; }
        public bool down { get; set; }
        public bool goingUp { get; set; }
        public bool UpsRebound { get; set; }
        public bool goingDown { get; set; }
        public bool DownsRebound { get; set; }
        //constructor
        public ACCELERO()
        {
            value = 0;
            centerCords = 0;
            valOverParam = false;
            valUnderParam = false;
            inCenter = true;
            up = false;
            down = false;
            goingUp = false;
            UpsRebound = false;
            goingDown = false;
            DownsRebound = false;
        }
    }
}
///  END OF put this in another file
namespace test
{
    public partial class MainWindow : Window
    {
        private SerialPort SP = new SerialPort();
        InputSimulator sim = new InputSimulator();//for the keyboard
        private AccelleroService accelleroService = new AccelleroService();
        private ACCELERO AX1 = new ACCELERO("accelerometer1");
        private ACCELERO AX2 = new ACCELERO("accelerometer2");
        private ACCELERO AX3 = new ACCELERO("accelerometer3");
        public int param = 20;
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
                ///// changed herer
                AX1 = accelleroService.UpdateParameterMath(param, AX1);
                AX2 = accelleroService.UpdateParameterMath(param, AX2);
                AX3 = accelleroService.UpdateParameterMath(param, AX3);
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
        public void sendToLog(string message)
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
            ///// changed herer
            AX1 = accelleroService.RESET(AX1);
            AX2 = accelleroService.RESET(AX2);
            AX3 = accelleroService.RESET(AX3);
        }
    }
}
