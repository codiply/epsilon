﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Epsilon.Resources.Logic.OutgoingVerification {
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
    public class OutgoingVerificationResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal OutgoingVerificationResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Epsilon.Resources.Logic.OutgoingVerification.OutgoingVerificationResources", typeof(OutgoingVerificationResources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Creating a new outgoing verification is temporarily disabled. Please try again later..
        /// </summary>
        public static string GlobalSwitch_PickOutgoingVerificationDisabled_Message {
            get {
                return ResourceManager.GetString("GlobalSwitch_PickOutgoingVerificationDisabled_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Address for outgoing verification was marked as invalid. Thank you!.
        /// </summary>
        public static string MarkAddressAsInvalid_SuccessMessage {
            get {
                return ResourceManager.GetString("MarkAddressAsInvalid_SuccessMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Outgoing verification was marked as sent..
        /// </summary>
        public static string MarkAsSent_SuccessMessage {
            get {
                return ResourceManager.GetString("MarkAsSent_SuccessMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are no verifications that can be assinged to you at the moment, please try again later..
        /// </summary>
        public static string Pick_NoVerificationAssignableToUser_RejectionMessage {
            get {
                return ResourceManager.GetString("Pick_NoVerificationAssignableToUser_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A new outgoing verification has been assigned to you. The instructions will be available for {0} days..
        /// </summary>
        public static string Pick_SuccessMessage {
            get {
                return ResourceManager.GetString("Pick_SuccessMessage", resourceCulture);
            }
        }
    }
}
