using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Reflection;
using ACSR.Core.Strings;
using ACSR.Core.Disk;
using System.IO;

namespace ACSR.Path.FileDB
{
    public class FileDB
    {

        SQLiteConnection _conn;
        SQLiteConnection _connFTS;
        private DriveList _Drives;
        private DriveList Drives
        {
            get
            {
                if (_Drives == null)
                {
                    _Drives = new DriveList();
                    _Drives.Refresh();
                }
                return _Drives;
            }
        }

        public void Open(string FileName)
        {
            var dbDS = string.Format("Data Source={0}", FileName);
            var dbFtsDS = string.Format("Data Source={0}.fts.sqb", FileName);
            var ftsFn = FileName + ".fts.sqb";
            var createDB = !File.Exists(FileName);
            _conn = new SQLiteConnection(dbDS);
            _conn.Open();
            if (createDB)
            {
                var sql = Scripts.CreateDB;// StringTools.StreamToString(s);
                var cmd = _conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            createDB = !(File.Exists(ftsFn));
            _connFTS = new SQLiteConnection(dbFtsDS);
            _connFTS.Open();
            if (createDB)
            {
                var sql = Scripts.CreateDBFts;// StringTools.StreamToString(s);
                var cmd = _connFTS.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }

            var cmd = _conn.CreateCommand();
            cmd.CommandText = string.Format("attach '{0}' as FTS", ftsFn);
            //cmd.ExecuteNonQuery
            
        }
        public IEnumerable<DtoFile> Find(string FileName)
        {
            var cmd = _conn.CreateCommand();
            cmd.CommandText = Scripts.SearchFts;
            using (SQLiteDataReader reader = cmd.ExecuteReader)
            {
                while (reader.Read())
                {
                    reader[0]
                }
            }
        }

    }
    public class DtoDisk
    {
        public string Signature;
        public long ID;
        
        public DtoDisk(long DiskId, string Signature)
        {
            this.ID = DiskId;
            this.Signature = Signature;
        }
    }
    public class DtoFile
    {
        public string FileName;
        DtoDisk _disk;
        public DtoDisk Disk
        {
            get
            {
                return _disk;
            }
        }
        public DtoFile(DtoDisk Disk, string FileName)
        {
            this._disk = Disk;
            this.FileName = FileName;
        }
    }
}
