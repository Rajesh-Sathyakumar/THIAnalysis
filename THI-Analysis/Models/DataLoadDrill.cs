//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace THI_Analysis.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DataLoadDrill
    {
        public int ProjectKey { get; set; }
        public string Project_ProjectName { get; set; }
        public string Project_ID { get; set; }
        public string ProjectPhase { get; set; }
        public string TestDBName { get; set; }
        public string ProdDBName { get; set; }
        public string ShortDescription { get; set; }
        public string MemberSupport_SupportIssueNumber { get; set; }
        public Nullable<System.DateTime> DateTimeOpened { get; set; }
        public Nullable<System.DateTime> DateTimeClosed { get; set; }
        public Nullable<System.DateTime> ExpectedResolutionDateCalc { get; set; }
        public Nullable<int> DaysLate_DataLoad { get; set; }
        public Nullable<System.DateTime> FilesReceived { get; set; }
        public Nullable<System.DateTime> DataLoadPeriodEnd { get; set; }
        public Nullable<double> CrimsonDataLag { get; set; }
        public Nullable<System.DateTime> MovedtoProduction { get; set; }
        public Nullable<System.DateTime> DataSubmissionTargetDate { get; set; }
        public Nullable<int> ETLCompletion_DataLoad { get; set; }
        public Nullable<int> FilesLate_DataLoad { get; set; }
    }
}
