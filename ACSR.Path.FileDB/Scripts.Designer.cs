﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ACSR.Path.FileDB {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Scripts {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Scripts() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ACSR.Path.FileDB.Scripts", typeof(Scripts).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE TABLE [DISKS] (
        ///[DISK_ID] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
        ///[SIGNATURE] TEXT  NULL);
        ///CREATE TABLE [FILES] (
        ///[FILE_ID] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL,
        ///[FILE_NAME] TEXT  NULL,
        ///[EXT] TEXT  NULL,
        ///[MD5] TEXT  NULL,
        ///[FILE_TYPE] TEXT  NULL,
        ///[FILE_SIZE] NUMERIC  NULL,
        ///[DATE_MODIFIED] TEXT  NULL,
        ///[DATE_ADDED] TEXT  NULL,
        ///[FILE_MODIFIED] TEXT  NULL,
        ///[DIRECTORY] TEXT  NULL,
        ///[DISK_ID] INTEGER  NULL,
        ///[CATEGORY_ID] INTEGER DEFAULT &apos;0&apos; NOT NULL
        ///, FAST_MD5 TEXT);
        ///CREATE INDEX [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateDB {
            get {
                return ResourceManager.GetString("CreateDB", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to create virtual table fts_names using fts3(&apos;file_id&apos;, &apos;file_name&apos;)
        ///.
        /// </summary>
        internal static string CreateDBFts {
            get {
                return ResourceManager.GetString("CreateDBFts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to           SELECT d.USER_LABEL, d.LABEL, d.DRIVE_LETTER, FTS.fts_names.file_name
        ///          from
        ///             FTS.fts_names join FILES f on FTS.fts_names.file_id = f.file_id join disks d on
        ///             d.disk_id = f.disk_id
        ///          where FTS.fts_names.file_name match ? %s
        ///.
        /// </summary>
        internal static string SearchFts {
            get {
                return ResourceManager.GetString("SearchFts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to fdsfdsf.
        /// </summary>
        internal static string String1 {
            get {
                return ResourceManager.GetString("String1", resourceCulture);
            }
        }
    }
}
