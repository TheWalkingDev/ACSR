using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
/*
 *  [00000007] Intel(R) 82566DC-2 Gigabit Network Connection
                      ArpAlwaysSourceRoute ==== <Null>
                      ArpUseEtherSNAP ==== <Null>
                      Caption ==== [00000007] Intel(R) 82566DC-2 Gigabit Network Connection
                      DatabasePath ==== %SystemRoot%\System32\drivers\etc
                      DeadGWDetectEnabled ==== <Null>
                      DefaultIPGateway ==== System.String[]
                      DefaultTOS ==== <Null>
                      DefaultTTL ==== <Null>
                      Description ==== Intel(R) 82566DC-2 Gigabit Network Connection
                      DHCPEnabled ==== True
                      DHCPLeaseExpires ==== 20100126173430.000000+120
                      DHCPLeaseObtained ==== 20100118173430.000000+120
                      DHCPServer ==== 172.31.1.10
                      DNSDomain ==== sys.co.za
                      DNSDomainSuffixSearchOrder ==== System.String[]
                      DNSEnabledForWINSResolution ==== False
                      DNSHostName ==== ctdevnl07
                      DNSServerSearchOrder ==== System.String[]
                      DomainDNSRegistrationEnabled ==== False
                      ForwardBufferMemory ==== <Null>
                      FullDNSRegistrationEnabled ==== True
                      GatewayCostMetric ==== System.UInt16[]
                      IGMPLevel ==== <Null>
                      Index ==== 7
                      InterfaceIndex ==== 11
                      IPAddress ==== System.String[]
                      IPConnectionMetric ==== 10
                      IPEnabled ==== True
                      IPFilterSecurityEnabled ==== False
                      IPPortSecurityEnabled ==== <Null>
                      IPSecPermitIPProtocols ==== System.String[]
                      IPSecPermitTCPPorts ==== System.String[]
                      IPSecPermitUDPPorts ==== System.String[]
                      IPSubnet ==== System.String[]
                      IPUseZeroBroadcast ==== <Null>
                      IPXAddress ==== <Null>
                      IPXEnabled ==== <Null>
                      IPXFrameType ==== <Null>
                      IPXMediaType ==== <Null>
                      IPXNetworkNumber ==== <Null>
                      IPXVirtualNetNumber ==== <Null>
                      KeepAliveInterval ==== <Null>
                      KeepAliveTime ==== <Null>
                      MACAddress ==== 00:16:D1:91:13:59
                      MTU ==== <Null>
                      NumForwardPackets ==== <Null>
                      PMTUBHDetectEnabled ==== <Null>
                      PMTUDiscoveryEnabled ==== <Null>
                      ServiceName ==== e1express
                      SettingID ==== {DFFCB6DD-597C-4224-96F7-48C0C12DDE95}
                      TcpipNetbiosOptions ==== 0
                      TcpMaxConnectRetransmissions ==== <Null>
                      TcpMaxDataRetransmissions ==== <Null>
                      TcpNumConnections ==== <Null>
                      TcpUseRFC1122UrgentPointer ==== <Null>
                      TcpWindowSize ==== <Null>
                      WINSEnableLMHostsLookup ==== True
                      WINSHostLookupFile ==== <Null>
                      WINSPrimaryServer ==== 172.31.1.10
                      WINSScopeID ====
                      WINSSecondaryServer ==== 172.30.2.1



 * 
 */
namespace ACSR.Core.Networking
{
    public class NetworkAdapterList : List<NetworkAdapter>
    {
        public NetworkAdapterList(bool AutoRefresh) : base()
        {
           Refresh();
        }
        public NetworkAdapterList() : this(true)
        {            
        }
        public void Refresh()
        {
            Clear();
            ManagementClass objMC = new ManagementClass(
                "Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            
            foreach (ManagementObject objMO in objMOC)
            {
                /*
                Console.WriteLine(objMO["Caption"].ToString());
                foreach (var property in  objMO.Properties)
                {
                    string s =  property.Value == null ? "<Null>" : property.Value.ToString();
                    
                    Console.WriteLine("                      " + property.Name + " ==== " + s);
                }
                Console.ReadKey();*/
                Add(new NetworkAdapter(objMO));
            }
        }
    }

    public class NetworkAdapter
    {
        private ManagementObject _mo = null;
        public NetworkAdapter(ManagementObject AObject)
        {
            _mo = AObject;
        }
        public string Caption
        {
            get
            {
                return _mo["Caption"].ToString();
            }
        }
        public string Description
        {
            get
            {
                return _mo["Description"].ToString();
            }
        }
        public string ServiceName
        {
            get
            {
                return _mo["ServiceName"].ToString();
            }
        }
        public string MacAddress
        {
            get
            {
                return _mo["MACAddress"].ToString();
            }
        }
        public bool IpEnabled
        {
            get
            {
                return (bool) _mo["ipEnabled"];
            }
        }
        private string GetFirstListItem(string [] AList)
        {
            if (AList == null || AList.Length == 0)
                return "";
            else
                return AList[0];
        }
        public string FirstIP
        {
            get
            {
                return GetFirstListItem(this.IpAddress);

            }
        }
        public string FirstIPSubnet
        {
            get
            {
                return GetFirstListItem(this.SubnetMask);

            }
        }
        public string FirstIPGateway
        {
            get
            {
                return GetFirstListItem(this.DefaultGateway);

            }
        }

        public string[] IpAddress
        {
            get
            {
                if (!IpEnabled)
                {
                    return new string[] {};
                }
                else
                {
                    return  (string[])_mo["IPAddress"]; 
                }
            }
          
        }
        public string[] DefaultGateway
        {
            get
            {
                if (!IpEnabled)
                {
                    return new string[] { };
                }
                else
                {
                    return (string[])_mo["DefaultIPGateway"];
                }
            }
           
        }
        public string[] SubnetMask
        {
            get
            {
                if (!IpEnabled)
                {
                    return new string[] { };
                }
                else
                {
                    return (string[])_mo["IPSubnet"];
                }
            }
           
        }
        public void SetIPAddress(string [] AIpAddress, string [] ASubnetmask, string [] ADefaultGateway)
        {
            if (!IpEnabled)
                throw new Exception("The Adapter is not IPEnabled");

            ManagementBaseObject objNewIP = null;
            ManagementBaseObject objSetIP = null;
            ManagementBaseObject objNewGate = null;


            objNewIP = _mo.GetMethodParameters("EnableStatic");
            objNewGate = _mo.GetMethodParameters("SetGateways");



            //Set DefaultGateway

            objNewGate["DefaultIPGateway"] = ADefaultGateway;
            objNewGate["GatewayCostMetric"] = new int[] { 1 };


            //Set IPAddress and Subnet Mask

            objNewIP["IPAddress"] = AIpAddress;
            objNewIP["SubnetMask"] = ASubnetmask;

            objSetIP = _mo.InvokeMethod("EnableStatic", objNewIP, null);
            objSetIP = _mo.InvokeMethod("SetGateways", objNewGate, null);


        }        
        public void SetDHCPIPAddress ()
        {
            _mo.InvokeMethod("EnableDHCP", null);
            _mo.InvokeMethod("SetDNSServerSearchOrder", null);

        }
        public void RenewDHCPLease ()
        {
            _mo.InvokeMethod("RenewDHCPLease", null);
            
        }

        public void SetIPAddress(string AIpAddres, string  ASubnetmask, string  ADefaultGateway)
        {
            SetIPAddress(new string[] {AIpAddres}, new string[] {ASubnetmask}, new string[] {ADefaultGateway} );
        }
    }
}
