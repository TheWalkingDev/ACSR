using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;

namespace ACSR.Core.Disk
{
    public class DriveList : List<Drive>
    {
        public void Refresh()
        {
            this.Clear();
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * From Win32_Diskdrive ");
            
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject x in queryCollection)
            {
                var drive  = new Drive(x.Properties);
                this.Add(drive);
                foreach (var p in x.Properties)
                {
                }
                var partitionObjects = x.GetRelated("Win32_DiskPartition");
                if (partitionObjects != null)
                {
                    foreach (ManagementObject b in partitionObjects)
                    {
                        var partition = new Partition(drive, b.Properties);
                        drive.Add(partition);
                        foreach (var p in b.Properties)
                        {

                        }

                        var logicalDiskObjects = b.GetRelated("Win32_LogicalDisk");
                        if (logicalDiskObjects != null)
                        {
                            foreach (ManagementBaseObject c in logicalDiskObjects)
                            {
                                var logicalDisk = new LogicalDisk(partition, c.Properties);
                                partition.Add(logicalDisk);
                                foreach (var p in c.Properties)
                                {

                                }
                            }
                        }
                    }
                }
            }
        }

        public LogicalDisk FindLogicalDiskByDrive(string ADrive)
        {
            
            foreach (var drive in this)
            {
                foreach (var p in drive)
                {
                    foreach (var logicalDisk in p)
                    {
                        if (string.Compare(logicalDisk.Caption, ADrive) == 0)
                        {
                            return logicalDisk;   
                        }
                    }
                }
            }
            return null;
        }

        public LogicalDisk FindLogicalDiskByPath(string APath)
        {
            var drive = Path.GetPathRoot(APath);
            if (drive[1] == ':')
            {
                return FindLogicalDiskByDrive(drive[0] + ":");    
            }
            return null;
        }

    }

    public class Drive : List<Partition>
    {
        public int Index;
        public string Caption;
        public string Signature;
        public Drive(PropertyDataCollection AProperties)
        {
            Index = Int32.Parse(AProperties["Index"].Value.ToString());
            Caption = AProperties["Caption"].Value.ToString();
            Signature = AProperties["Signature"].Value.ToString();

            
        }
    }

    public class Partition : List<LogicalDisk>
    {
        private Drive _Drive;
        public Drive Drive
        {
            get
            {
                return _Drive;
            }
        }
        public int Index;
        public Partition(Drive ADrive, PropertyDataCollection AProperties)
        {
            Index = Int32.Parse(AProperties["Index"].Value.ToString());
            _Drive = ADrive;
        }
    }

    public class LogicalDisk
    {
        private Partition _Partition;
        public Partition Partition
        {
            get
            {
                return _Partition;
            }
        }

        public string VolumeName;
        public string VolumeSerial;
        public string Caption;
        public LogicalDisk(Partition APartition, PropertyDataCollection AProperties)
        {
            try
            {
                VolumeName = AProperties["VolumeName"] == null ? "" : AProperties["VolumeName"].Value.ToString();
            }
            catch 
            {
                VolumeName = "";
            }
            try
            {
                VolumeSerial = AProperties["VolumeSerialNumber"] == null ? "" : AProperties["VolumeSerialNumber"].Value.ToString();
            }
            catch
            {
                VolumeSerial = "";
            }
            try
            {
                Caption = AProperties["Caption"] == null ? "" : AProperties["Caption"].Value.ToString();
            }
            catch
            {
                Caption = "";
            }
            _Partition = APartition;


        }        
    }
}