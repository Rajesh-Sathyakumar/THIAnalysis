namespace THI_Analysis.Models
{

    using System.ComponentModel.DataAnnotations;

    public partial class THI_AnalysisFeedback
    {
        public int AnalysisFeedbackKey { get; set; }


        [Required(ErrorMessage = "Select a valid Project Name !")]
        [Range(2, int.MaxValue, ErrorMessage = "Select a valid Project Name !")]
        public int Project { get; set; }

        [Required(ErrorMessage = "Analysis Summary is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "Analysis Summary is not Selected !")]
        public int AnalysisSummary { get; set; }

        [Required(ErrorMessage = "Analysis Notes is Blank!")]
        public string AnalysisNotes { get; set; }

        [Required(ErrorMessage = "Analysis recommendations is Blank !")]
        public string AnalysisRecommendations { get; set; }

        [Required(ErrorMessage = "Data Submission Timelines Field is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "Data Submission Timelines Field is not Selected !")]
        public int DataSubmissionTimelines { get; set; }

        [Required(ErrorMessage = "Comments on Data Submission Timelines is Blank !")]
        public string Comments_DataSubmissionTimelines { get; set; }

        [Required(ErrorMessage = "Data Lag Field is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "Data Lag Field is not Selected !")]
        public int DataLag { get; set; }

        [Required(ErrorMessage = "Comments on Data Lag is Blank !")]
        public string Comments_DataLag { get; set; }

        [Required(ErrorMessage = "Critical Diagnostics Field is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "Critical Diagnostics Field is not Selected!")]
        public int CriticalDiagnostics { get; set; }

        [Required(ErrorMessage = "Comments on Critical Diagnostics is Blank !")]
        public string Comments_CriticalDiagnostics { get; set; }

        [Required(ErrorMessage = "Member Support Tickets field is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "Member Support Tickets field is not Selected !")]
        public int MemberSupportTickets { get; set; }

        [Required(ErrorMessage = "Comments on Member Support Tickets is Blank !")]
        public string Comments_MemberSupportTickets { get; set; }

        [Required(ErrorMessage = "Data Elements Present Field is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "Data Elements Present Field is not Selected !")]
        public int DataElementsPresent { get; set; }

        [Required(ErrorMessage = "Comments on Data Elements Present is Blank !")]
        public string Comments_DataElementsPresent { get; set; }

        [Required(ErrorMessage = "Minesweeper Field is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "Minesweeper Field is not Selected !")]
        public int Minesweeper { get; set; }

        [Required(ErrorMessage = "Comments on Minesweeper is Blank !")]
        public string Comments_Minesweeper { get; set; }

        [Required(ErrorMessage = "DAS Findings is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "DAS Findings is not Selected !")]
        public int DAS_Findings { get; set; }

        [Required(ErrorMessage = "Comments on DAS Findings is Blank !")]
        public string Comments_DAS_Findings { get; set; }

        [Required(ErrorMessage = "SSA Findings is not Selected !")]
        [Range(0, int.MaxValue, ErrorMessage = "SSA Findings is not Selected !")]
        public int SSA_Findings { get; set; }

        [Required(ErrorMessage = "Comments on SSA Findings is Blank !")]
        public string Comments_SSA_Findings { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy}")]
        public System.DateTime AnalysisCreatedDate { get; set; }

        [Required(ErrorMessage = "Enter the Name of the Business Analyst !")]
        public string BusinessAnalyst { get; set; }

        [Required]
        public string LastModifiedBy { get; set; }


        [Required]
        [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy}")]
        public System.DateTime LastModifiedDate { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        public virtual AnalysisSummary AnalysisSummary1 { get; set; }
        public virtual CriticalDiagnostic CriticalDiagnostic { get; set; }
        public virtual DAS_Findings DAS_Findings1 { get; set; }
        public virtual DataElement DataElement { get; set; }
        public virtual DataLag DataLag1 { get; set; }
        public virtual DataLoadTimeliness DataLoadTimeliness { get; set; }
        public virtual MemberSupportTicket MemberSupportTicket { get; set; }
        public virtual Minesweeper Minesweeper1 { get; set; }
        public virtual SalesforceProject SalesforceProject { get; set; }
        public virtual SSA_Findings SSA_Findings1 { get; set; }
    }
}