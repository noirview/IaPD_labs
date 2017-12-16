using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3_Battery
{
    public partial class Form1 : Form
    {
        Battery noteBattery = new Battery();
        Timer updateTimer = new Timer();
        //static int DEFAULT_TIMEOUT = 5;

        private static int GetScreenTime()
        {
            const string command = "c powercfg /q";
            Process cmd = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = command
                }
            };
            cmd.Start();

            var powerSchemes = cmd.StandardOutput.ReadToEnd();
            var someString = new Regex("VIDEOIDLE.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n.*");
            var videoidle = someString.Match(powerSchemes).Value;
            return Convert.ToInt32(videoidle.Substring(videoidle.Length - 11).TrimEnd(), 16) / 60;
        }

        int DEFAULT_TIMEOUT = GetScreenTime();

        public static void SetPowerTimeout(int minutes)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = "/c powercfg /x -monitor-timeout-dc " + minutes;
            process.Start();
        }

        private static String GetTime(int remainingTime)
        {
            int sec = remainingTime % 60;
            int min = remainingTime / 60 % 60;
            int hours = remainingTime / 3600 % 24;
            //int days = remainingTime / 3600 / 24;
            return String.Format("{0} hours {1} minutes {2} seconds", hours, min, sec);
        }

            public Form1()
        {
            InitializeComponent();
            updateTimer.Interval = 1000;
            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Start();
            Application.ApplicationExit += new EventHandler(this.Form1_FormClosing);
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            Form1_Load(null, null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text         =        (SystemInformation.PowerStatus.BatteryLifePercent * 100).ToString() + "% " + 
                                          SystemInformation.PowerStatus.PowerLineStatus;
            label2.Text         = GetTime(SystemInformation.PowerStatus.BatteryLifeRemaining);
            progressBar1.Value  =   (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);

            if (SystemInformation.PowerStatus.BatteryLifeRemaining == -1)
            {
                SetPowerTimeout(DEFAULT_TIMEOUT);
                label2.Text = "";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int minutes = Int32.Parse(textBox1.Text);
            SetPowerTimeout(minutes);
        }

        private void Form1_FormClosing(object sender, EventArgs e)
        {
            SetPowerTimeout(DEFAULT_TIMEOUT);
        }
    }
}
