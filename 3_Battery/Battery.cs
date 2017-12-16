using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3_Battery
{
    class Battery
    {
        string status /*= SystemInformation.PowerStatus.PowerLineStatus.ToString()*/;
        float percentLife;
        int timeLife;
        int DEFAULT_TIMEOUT;

        public Battery()
        {
            PowerStatus batteryInfo = SystemInformation.PowerStatus;

            GetScreenTime();
            status      = batteryInfo.PowerLineStatus.ToString();
            percentLife = batteryInfo.BatteryLifePercent;
            timeLife    = batteryInfo.BatteryLifeRemaining;
        }

        public string GetStatus() { return status; }
        public int GetPercentLife() { return (int)(percentLife * 100); }
        public int GetTimeLife() { return timeLife; }
        public int GetDefaultTimeout() { return DEFAULT_TIMEOUT; }

        private void GetScreenTime()
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = "/c powercfg /q";
            process.Start();

            var powerSchemes = process.StandardOutput.ReadToEnd();
            var someString = new Regex("VIDEOIDLE.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n.*");
            var videoidle = someString.Match(powerSchemes).Value;
            DEFAULT_TIMEOUT = Convert.ToInt32(videoidle.Substring(videoidle.Length - 11).TrimEnd(), 16) / 60;
        }

    }
}
