using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACSR.Security.MasterLock;
using ACSR.Core.Security.Encryption;
using System.IO;
using ACSR.Core.Strings;
using ACSR.Security.MasterLock.Locks;
using System.Reflection;
using ACSR.Core.Streams;

namespace ACSR.Security.MasterLock
{
    public class MasterLock
    {
        string GenerateRandomKey()
        {
            var r = new Random();
            var buf = new byte[255];
            for (int i=0;i<buf.Length;i++)
            {
                buf[i] = (byte)r.Next(Convert.ToInt16('A'), Convert.ToInt16('Z'));
                //buf[i] = c;
            }
            return ASCIIEncoding.ASCII.GetString(buf, 0, buf.Length);
        }


        public MemoryStream GenerateKey(string InputKey, BaseLock Lock)
        {
            var stack = new List<BaseLock>();
            stack.Add(Lock);
            var k = InputKey;
            if (string.IsNullOrEmpty(k))
            {
                //k = "SomeKey";                
                k = GenerateRandomKey();
            }
            return CreateLock(stack, k);    
        }

     //   Dictionary<string, BaseLock> _locks;
     //   Dictionary<string, ILockUI> _ui;
     /*  void RegisterUI(string Name, ILockUI UI)
        {
            _ui[Name] = UI;
        }*/
   /*     public ILockUI GetUIFor(string Name)
        {
            ILockUI result = null;
            if (_ui.TryGetValue(Name, out result))
                return result;
            else
                return null;
        }*/

     /*   void RegisterLock(BaseLock Lock)
        {
            _locks[Lock.Name] = Lock;
        }*/

        Dictionary<string, LockFactory> _LockFactories;
        void RegisterFactory(string Name, LockFactory factory)
        {
            _LockFactories[Name] = factory;
        }
        public MasterLock()
        {
          //  _locks = new Dictionary<string, BaseLock>();
          //  _ui = new Dictionary<string, ILockUI>();
            
           // RegisterLock(new LockPassword(this));
           // RegisterLock(new LockSID(this));
            _LockFactories = new Dictionary<string, LockFactory>();
            RegisterFactory("LockPassword", new GenericLockFactory<LockPassword>());
        }

        public MemoryStream AddLock(MemoryStream Key,  string LockType, string LockName)
        {
            return AddLock(Key, _LockFactories[LockType].CreateLock(this, LockName));
        }
        public MemoryStream AddLock(MemoryStream Key,  BaseLock Lock)
        {
            string unLockedKey = null;
            var stack = LoadLockStack(Key, out unLockedKey);
            stack.Add(Lock);

            return CreateLock(stack, unLockedKey);
        }

        string DecryptWithStack(List<BaseLock> Stack, string Value)
        {
            var result = Value;
            for (int i=Stack.Count-1;i>=0;i--)
            {
                var s = Stack[i];
                result = EncDec.Decrypt(result, s.GetKey());
            }
            return result;
        }
        string EncryptWithStack(List<BaseLock> Stack, string Value)
        {

            var result = Value;
            foreach (var s in Stack)
            {
                result = EncDec.Encrypt(result, s.GetKey());
            }
            return result;
        }

        void WriteName(MemoryStream s, string Name)
        {
            var bSize = BitConverter.GetBytes((Int32)Name.Length);
            s.Write(bSize, 0, bSize.Length);
            var buf = ASCIIEncoding.ASCII.GetBytes(Name); 
            s.Write(buf, 0, buf.Length);
        }

        string ReadName(MemoryStream s)
        {
            /*var buf = new byte[sizeof(Int32)];
            s.Read(buf, 0, buf.Length);
            var size = BitConverter.ToInt32(buf, 0);
            buf = new byte[size];
            s.Read(buf, 0, buf.Length);
            return ASCIIEncoding.ASCII.GetString(buf);*/
            return s.ReadSizedObjectAsString();
        }
        bool ReadBool(MemoryStream s)        
        {
            var buf = new byte[sizeof(bool)];
            s.Read(buf, 0, buf.Length);

            //return BitConverter.ToBoolean(ASCIIEncoding.ASCII.GetBytes(buf), 0);
            return BitConverter.ToBoolean(buf, 0);
        }
        void WriteBool(MemoryStream s, bool Value)
        {
            var bSize = BitConverter.GetBytes(Value);
            s.Write(bSize, 0, bSize.Length);
        }


        MemoryStream CreateLock(List<BaseLock> Stack, string Key)
        {
            var ss = new MemoryStream();
            
            var newStack = new List<BaseLock>();
            for (int i=0;i<Stack.Count;i++)
            {
                var L = Stack[i];
                WriteName(ss, EncryptWithStack(newStack, L.GetType().FullName));
                newStack.Add(L);
                WriteName(ss, EncryptWithStack(newStack, L.SaveToString()));
                WriteBool(ss, i == Stack.Count - 1);
                
            }
            WriteName(ss, EncryptWithStack(newStack, Key));
            return ss;
        }

        List<BaseLock> LoadLockStack(MemoryStream LockedKey, out string UnlockedKey)
        {
                var locks = new List<BaseLock>();
                bool eof = false;
                LockedKey.Position = 0;
                string lockTypeName = ReadName(LockedKey);
                string lockName = ReadName(LockedKey);
                string lockConfig = ReadName(LockedKey);                
                while (!eof)
                {
                    
                    eof = ReadBool(LockedKey);
                    var lockInstance = (BaseLock)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType(lockTypeName), this);
                    locks.Add(lockInstance);
                    lockConfig = DecryptWithStack(locks, lockConfig);
                    lockInstance.LoadFromString(lockConfig);
                    
                    if (!eof)
                    {
                        lockTypeName = ReadName(LockedKey);
                        lockTypeName = DecryptWithStack(locks, lockTypeName);
                        lockConfig = ReadName(LockedKey);                        
                    }
                }
                UnlockedKey = DecryptWithStack(locks, ReadName(LockedKey));
            
            return locks;
        }

        public string DecodeLock(MemoryStream Lock)
        {
            string unLockedKey = null;
            var stack = LoadLockStack(Lock, out unLockedKey);
            return unLockedKey;
        }

        public string DecodeString(MemoryStream Lock, string Data)
        {
            Lock.Position = 0;
            var key = DecodeLock(Lock);
            return EncDec.Decrypt(Data, key);
        }

        public string EncodeString(MemoryStream Lock, string Data)
        {
            Lock.Position = 0;
            var key = DecodeLock(Lock);
            return EncDec.Encrypt(Data, key);
        }
    }
    public class LockFactory
    {
        public virtual BaseLock CreateLock(MasterLock Master, string Name)
        {
            return null;
        }
    }
    public class GenericLockFactory<T> : LockFactory where T : BaseLock
    {
        public override BaseLock CreateLock(MasterLock Master, string Name)
        {            
            return (BaseLock)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType(typeof(T).FullName), Master, Name);
        }
    }
    
}
