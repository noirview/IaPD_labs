using System;
using System.Collections.Generic;
using System.Management;
using System.IO;

namespace PCI_devices
{
    class lsPCI
    {
        static string STATUS_EMPTY = "";

        static string vendorName = STATUS_EMPTY,
                      deviceName = STATUS_EMPTY;

        private static void ParseReport(IEnumerable<string> sampleReport,
                                                string sampleVendorID,
                                                string sampleDeviceID)
        {
            foreach (var line in sampleReport)
            {
                if (line.StartsWith(sampleVendorID) && 
                    vendorName == STATUS_EMPTY)
                {
                    vendorName = line.Substring(6);
                    continue;
                }

                if (line.StartsWith("\t" + sampleDeviceID) && 
                    vendorName != STATUS_EMPTY)
                {
                    deviceName = line.Substring(7);
                    break;
                }
            }

            if(vendorName == STATUS_EMPTY)
            {
                vendorName = "NOT_FOUND";
                deviceName = "NOT_FOUND";
            }
        }

        static void Main(string[] args)
        {
            string vendorID, deviceID;

            var report = File.ReadLines("pci.ids");
            var sercher = new ManagementObjectSearcher("root\\CIMV2", 
                                                       "SELECT * FROM Win32_PnPEntity");

            foreach(ManagementObject queryDevice in sercher.Get())
            {
                if (queryDevice["DeviceID"].ToString().Substring(0, 3) == "PCI")
                {
                    vendorID = queryDevice["DeviceID"].ToString().Substring(8, 4).ToLower();
                    deviceID = queryDevice["DeviceID"].ToString().Substring(17, 4).ToLower();
                    ParseReport(report,
                                vendorID,
                                deviceID);

                    Console.WriteLine("Vendor: 0x{0}({1})", vendorID, vendorName);
                    Console.WriteLine("Device: 0x{0}({1})", deviceID, deviceName);
                    Console.WriteLine();                    
                }
            }
        }
    }
}
