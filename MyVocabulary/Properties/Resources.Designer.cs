﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyVocabulary.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MyVocabulary.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Type filter text....
        /// </summary>
        internal static string FILTER_Text {
            get {
                return ResourceManager.GetString("FILTER_Text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Document is modified. Save changes?.
        /// </summary>
        internal static string MESSAGEBOX_DocumentModified {
            get {
                return ResourceManager.GetString("MESSAGEBOX_DocumentModified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure to delete selected {0} word(s)?.
        /// </summary>
        internal static string MESSAGEBOX_SureDeleteSelectedWords {
            get {
                return ResourceManager.GetString("MESSAGEBOX_SureDeleteSelectedWords", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Import.
        /// </summary>
        internal static string TAB_HEADER_Import {
            get {
                return ResourceManager.GetString("TAB_HEADER_Import", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        internal static string TITLE_Error {
            get {
                return ResourceManager.GetString("TITLE_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to My Vocabulary.
        /// </summary>
        internal static string TITLE_MainWindow {
            get {
                return ResourceManager.GetString("TITLE_MainWindow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning.
        /// </summary>
        internal static string TITLE_Warning {
            get {
                return ResourceManager.GetString("TITLE_Warning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bad Known.
        /// </summary>
        internal static string WORD_TYPE_BadKnown {
            get {
                return ResourceManager.GetString("WORD_TYPE_BadKnown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Known.
        /// </summary>
        internal static string WORD_TYPE_Known {
            get {
                return ResourceManager.GetString("WORD_TYPE_Known", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown.
        /// </summary>
        internal static string WORD_TYPE_Unknown {
            get {
                return ResourceManager.GetString("WORD_TYPE_Unknown", resourceCulture);
            }
        }
    }
}
