using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3_Battery
{
    class Battery
    {
        string status /*= SystemInformation.PowerStatus.PowerLineStatus.ToString()*/;
        float percentLife;
        int timeLife;

        public Battery()
        {
            PowerStatus batteryInfo = SystemInformation.PowerStatus;

            status      = batteryInfo.PowerLineStatus.ToString();
            percentLife = batteryInfo.BatteryLifePercent;
            timeLife    = batteryInfo.BatteryLifeRemaining;
        }

        public string GetStatus() { return status; }
        public int GetPercentLife() { return (int)(percentLife * 100); }
        public int GetTimeLife() { return timeLife; }

    }
}
