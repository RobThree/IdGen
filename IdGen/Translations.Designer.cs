﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IdGen {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Translations {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Translations() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("IdGen.Translations", typeof(Translations).GetTypeInfo().Assembly);
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
        ///   Looks up a localized string similar to Clock moved backwards or wrapped around. Refusing to generate id for {0} ticks..
        /// </summary>
        internal static string ERR_CLOCK_MOVED_BACKWARDS {
            get {
                return ResourceManager.GetString("ERR_CLOCK_MOVED_BACKWARDS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GeneratorId cannot have more than 31 bits..
        /// </summary>
        internal static string ERR_GENERATORID_CANNOT_EXCEED_31BITS {
            get {
                return ResourceManager.GetString("ERR_GENERATORID_CANNOT_EXCEED_31BITS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GeneratorId must be from 0 to {0}..
        /// </summary>
        internal static string ERR_INVALID_GENERATORID {
            get {
                return ResourceManager.GetString("ERR_INVALID_GENERATORID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid system clock..
        /// </summary>
        internal static string ERR_INVALID_SYSTEM_CLOCK {
            get {
                return ResourceManager.GetString("ERR_INVALID_SYSTEM_CLOCK", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Number of bits used to generate Id&apos;s is not equal to 63..
        /// </summary>
        internal static string ERR_MUST_BE_63BITS_EXACTLY {
            get {
                return ResourceManager.GetString("ERR_MUST_BE_63BITS_EXACTLY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sequence cannot have more than 31 bits..
        /// </summary>
        internal static string ERR_SEQUENCE_CANNOT_EXCEED_31BITS {
            get {
                return ResourceManager.GetString("ERR_SEQUENCE_CANNOT_EXCEED_31BITS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sequence overflow..
        /// </summary>
        internal static string ERR_SEQUENCE_OVERFLOW {
            get {
                return ResourceManager.GetString("ERR_SEQUENCE_OVERFLOW", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sequence overflow. Refusing to generate id for rest of tick..
        /// </summary>
        internal static string ERR_SEQUENCE_OVERFLOW_EX {
            get {
                return ResourceManager.GetString("ERR_SEQUENCE_OVERFLOW_EX", resourceCulture);
            }
        }
    }
}
