﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Epsilon.Resources.Logic.AntiAbuse
{


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
    public class AntiAbuseResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AntiAbuseResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Epsilon.Resources.Logic.AntiAbuse.AntiAbuseResources", typeof(AntiAbuseResources).Assembly);
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
        ///   Looks up a localized string similar to There have been too many failed attempts to add an address from your IP address. Please try again in a few hours..
        /// </summary>
        public static string AddAddress_GeocodeFailureIpAddressFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("AddAddress_GeocodeFailureIpAddressFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have exceeded the maximum number of attempts to add an address. Please try again in a few hours..
        /// </summary>
        public static string AddAddress_GeocodeFailureUserFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("AddAddress_GeocodeFailureUserFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Due to big number of address added today, this function has been disabled. Please try again tomorrow..
        /// </summary>
        public static string AddAddress_GlobalFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("AddAddress_GlobalFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There have been too many addresses added by your IP address. Try again at a later time..
        /// </summary>
        public static string AddAddress_IpAddressFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("AddAddress_IpAddressFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can only add a certain number of addresses in a certain period..
        /// </summary>
        public static string AddAddress_UserFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("AddAddress_UserFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, we couldn&apos;t determine your country from your IP address..
        /// </summary>
        public static string CannotDetermineGeoipCountryErrorMessage {
            get {
                return ResourceManager.GetString("CannotDetermineGeoipCountryErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Due to big number of submissions created today, this function has been disabled. Please try again tomorrow..
        /// </summary>
        public static string CreateTenancyDetailsSubmission_GlobalFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("CreateTenancyDetailsSubmission_GlobalFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There have been too many submissions made by your IP address. Try again at a later time..
        /// </summary>
        public static string CreateTenancyDetailsSubmission_IpAddressFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("CreateTenancyDetailsSubmission_IpAddressFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can only make a certain number of submissions in a certain period of time..
        /// </summary>
        public static string CreateTenancyDetailsSubmission_UserFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("CreateTenancyDetailsSubmission_UserFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your IP address appears to be in Country {0}. We cannot process this request..
        /// </summary>
        public static string GeoipCountryMismatchErrorMessage {
            get {
                return ResourceManager.GetString("GeoipCountryMismatchErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Registration is temporarily disabled. Please try again later..
        /// </summary>
        public static string GlobalSwitch_RegisterDisabled_Message {
            get {
                return ResourceManager.GetString("GlobalSwitch_RegisterDisabled_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Due to big number of outgoing verifications today, this function has been disabled. Please try again tomorrow..
        /// </summary>
        public static string PickOutgoingVerification_GlobalFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("PickOutgoingVerification_GlobalFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There have been too many outgoing verifications from your IP address. Try again at a later time..
        /// </summary>
        public static string PickOutgoingVerification_IpAddressFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("PickOutgoingVerification_IpAddressFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have too many outstanding verifications. Once some of them have been completed you will be able to create more..
        /// </summary>
        public static string PickOutgoingVerification_MaxOutstandingFrequencyPerUserCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("PickOutgoingVerification_MaxOutstandingFrequencyPerUserCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Due to big number of registrations today, registration has been disabled. Please try again tomorrow..
        /// </summary>
        public static string Register_GlobalFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("Register_GlobalFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There have been too many registrations from your IP address. Please try again another time..
        /// </summary>
        public static string Register_IpAddressFrequencyCheck_RejectionMessage {
            get {
                return ResourceManager.GetString("Register_IpAddressFrequencyCheck_RejectionMessage", resourceCulture);
            }
        }
    }
}
