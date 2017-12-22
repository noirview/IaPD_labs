using SimpleWifi.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SimpleWifi.Win32.Interop;

namespace SimpleWifi
{
	public class AccessPoint
	{
        // Represents a Wifi network interface.
        private WlanInterface _interface;
        // Contains information about an available wireless network.
        private WlanAvailableNetwork _network;

		internal AccessPoint(WlanInterface interfac, WlanAvailableNetwork network)
		{
			_interface = interfac;
			_network = network;
		}

		public string Name
		{
			get
			{
				return Encoding.ASCII.GetString(_network.dot11Ssid.SSID, 0, (int)_network.dot11Ssid.SSIDLength);
			}
		}

		public uint SignalStrength
		{
			get
			{
				return _network.wlanSignalQuality;
			}
		}

	    public string AuthType
	    {
	        get
	        {
	            return _network.dot11DefaultAuthAlgorithm.ToString();
	        }
	    }

	    public string MacAddress
	    {
	        get
	        {
	            return GetRelatedMacAddress();
	        }
	    }

	    private string GetRelatedMacAddress()
	    {
	        var wlanClient = _interface.GetWlanClient();
	        if (wlanClient.Interfaces == null)
	            return string.Empty;

	        foreach (var wlanInterface in wlanClient.Interfaces)
	        {
	            var wlanBssEntries = wlanInterface.GetNetworkBssList();
	            foreach (var wlanBssEntry in wlanBssEntries)
	            {
	                if (IsSsidTheSame(wlanBssEntry.dot11Ssid.SSID))
	                {
	                    var macAddr = wlanBssEntry.dot11Bssid;
	                    return ConvertMacAddressToStr(macAddr);
                    }
	            }
	        }
	        return string.Empty;
	    }

	    private bool IsSsidTheSame(byte[] ssid)
	    {
	        return SSID.SequenceEqual(ssid);
	    }

        public byte[] SSID
	    {
	        get
	        {
	            return _network.dot11Ssid.SSID;
	        }
	    }

        /// <summary>
        /// If the computer has a connection profile stored for this access point
        /// profiles store various wireless configurations
        /// profile is associated with SSID
        /// </summary>
        public bool HasProfile
		{
			get
			{
				try
				{
					return _interface.GetProfiles()
                        .Where(p => p.profileName == Name)
                        .Any();
				}
				catch 
				{ 
					return false; 
				}
			}
		}
		
		public bool IsSecure
		{
			get
			{
				return _network.securityEnabled;
			}
		}

		public bool IsConnected
		{
			get
			{
				try
				{
					var a = _interface.CurrentConnection; // This prop throws exception if not connected, which forces me to this try catch. Refactor plix.
					return a.profileName == _network.profileName;
				}
				catch
				{
					return false;
				}
			}

		}

		/// <summary>
		/// Returns the underlying network object.
		/// </summary>
		internal WlanAvailableNetwork Network
		{
			get
			{
				return _network;
			}
		}


		/// <summary>
		/// Returns the underlying interface object.
		/// </summary>
		internal WlanInterface Interface
		{
			get
			{
				return _interface;
			}
		}

		/// <summary>
		/// Checks that the password format matches this access point's encryption method.
		/// </summary>
		public bool IsValidPassword(string password)
		{
			return PasswordHelper.IsValid(password, _network.dot11DefaultCipherAlgorithm);
		}		
		
		/// <summary>
		/// Connect synchronous to the access point.
		/// </summary>
		public bool Connect(AuthRequest request, bool overwriteProfile = false)
		{
			// No point to continue with the connect if the password is not valid if overwrite is true or profile is missing.
			if (!request.IsPasswordValid && (!HasProfile || overwriteProfile))
				return false;

			// If we should create or overwrite the profile, do so.
			if (!HasProfile || overwriteProfile)
			{				
				if (HasProfile)
					_interface.DeleteProfile(Name);

				request.Process();				
			}


			// TODO: Auth algorithm: IEEE80211_Open + Cipher algorithm: None throws an error.
			// Probably due to connectionmode profile + no profile exist, cant figure out how to solve it though.
			return _interface.ConnectSynchronously(WlanConnectionMode.Profile, _network.dot11BssType, Name, 6000);			
		}

		/// <summary>
		/// Connect asynchronous to the access point.
		/// </summary>
		public void ConnectAsync(AuthRequest request, bool overwriteProfile = false, Action<bool> onConnectComplete = null)
		{
			// TODO: Refactor -> Use async connect in wlaninterface.
			ThreadPool.QueueUserWorkItem(new WaitCallback((o) => {
				bool success = false;

				try
				{
					success = Connect(request, overwriteProfile);
				}
				catch (Win32Exception)
				{					
					success = false;
				}

				if (onConnectComplete != null)
					onConnectComplete(success);
			}));
		}
				
		public string GetProfileXML()
		{
			if (HasProfile)
				return _interface.GetProfileXml(Name);
			else
				return string.Empty;
		}

		public void DeleteProfile()
		{
			try
			{
				if (HasProfile)
					_interface.DeleteProfile(Name);
			}
			catch { }
		}

		public override sealed string ToString()
		{
			StringBuilder info = new StringBuilder();
			info.AppendLine("Interface: " + _interface.InterfaceName);
			info.AppendLine("Auth algorithm: " + _network.dot11DefaultAuthAlgorithm);
			info.AppendLine("Cipher algorithm: " + _network.dot11DefaultCipherAlgorithm);
			info.AppendLine("BSS type: " + _network.dot11BssType);
			info.AppendLine("Connectable: " + _network.networkConnectable);
			
			if (!_network.networkConnectable)
				info.AppendLine("Reason to false: " + _network.wlanNotConnectableReason);

			return info.ToString();
		}

	    private static string ConvertMacAddressToStr(byte[] macAddr)
	    {
	        var macAddrLen = (uint)macAddr.Length;
	        var str = new string[(int)macAddrLen];
	        for (var i = 0; i < macAddrLen; i++)
	        {
	            str[i] = macAddr[i].ToString("x2");
	        }
	        return string.Join("", str);
	    }
    }
}
