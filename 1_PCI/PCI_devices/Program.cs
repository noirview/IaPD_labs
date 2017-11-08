using System;
using System.Collections.Generic;
using System.Management;
using System.IO;

namespace PCI_devices
{
    class lsPCI
    {
        static string vendor = "", device = "";

        static void Main(string[] args)
        {
            string vendorID, deviceID;

            var report = File.ReadLines("reports.csv");
            var sercher = new ManagementObjectSearcher("root\\CIMV2", 
                                                       "SELECT * FROM Win32_PnPEntity");

            foreach(ManagementObject queryDevice in sercher.Get())
            {
                if (queryDevice["DeviceID"].ToString().Substring(0, 3) == "PCI")
                {
                    vendorID = "0x" + queryDevice["DeviceID"].ToString().Substring(8, 4);
                    deviceID = "0x" + queryDevice["DeviceID"].ToString().Substring(17, 4);
                    FindVendorAndDevice(report,
                                        vendorID,
                                        deviceID,
                                        queryDevice["Description"].ToString());

                    Console.WriteLine("Vendor: {0}({1})", vendorID, vendor);
                    Console.WriteLine("Device: {0}({1})", deviceID, device);
                    Console.WriteLine();                    
                }
            }
        }

        private static void FindVendorAndDevice(IEnumerable<string> sampleReport,
                                         string sampleVendorID,
                                         string sampleDeviceID,
                                         string sampleDescription)
        {
            foreach (var line in sampleReport)
            {
                String[] arrayLines = line.Replace("\"", "").Split(',');

                if (arrayLines[0].Equals(sampleVendorID))
                {
                    vendor = arrayLines[2];
                    if (arrayLines[1].Equals(sampleDeviceID))
                    {
                        device = arrayLines[4];
                        return;
                    }
                }
            }

            if (vendor != "")
            {
                device = sampleDescription;
            }
            else
            {
                vendor = "unknown";
                device = "unknown";
            }
        }
    }
}
