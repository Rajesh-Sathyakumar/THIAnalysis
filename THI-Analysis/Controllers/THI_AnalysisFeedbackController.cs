using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using SamlHelperLibrary;
using SamlHelperLibrary.Configuration;
using SamlHelperLibrary.Models;
using SamlHelperLibrary.Service;
using THI_Analysis.Constants;
using THI_Analysis.Models;

namespace THI_Analysis.Controllers
{
    public class THI_AnalysisFeedbackController : Controller
    {
        private readonly CCCOperationsEntities db = new CCCOperationsEntities();
        private UsageActivityList _usgAct = new UsageActivityList();


        [HttpPost]
        public void SetUsage(int usageActivity, XDocument paramXml = null)
        {
            if (Session != null && Session["UserSessionInfo"] != null)
            {
                var currentUsage = new UsageActivityLog();
                var userGuid = ((UserSessionInfo)Session["UserSessionInfo"]).userKey.ToString();
                var sessionId = System.Web.HttpContext.Current.Session.SessionID;
                if (paramXml != null)
                {
                    currentUsage.ParameterXML = paramXml.ToString();
                }
                currentUsage.AccessTime = DateTime.Now;
                currentUsage.UsageActivityKey = usageActivity;
                currentUsage.Userkey = db.Users.Where(a => a.UserGUID == userGuid).Max(a => a.Userkey);
                currentUsage.SessionID = sessionId;
                db.UsageActivityLogs.Add(currentUsage);

                db.SaveChanges();
            }            
        }

        public ActionResult EditUserPermission()
        {
            SiamRedirection(Request.Url.AbsoluteUri);
            return View();
        }

        [HttpPost]
        public void SiamRedirection(string returnUrl)
        {

            //if (Session["UserSessionInfo"] == null)
            //{
            //    var cas = new CasAuthenticationService(SamlHelperConfiguration.Config, UserSessionHandler.Get());
            //    var httpContextBase = new HttpContextWrapper(System.Web.HttpContext.Current);
            //    if (!cas.IsSAMLResponse(httpContextBase) && (Session == null || Session["UserSessionInfo"] == null))
            //    {
            //        cas.RedirectUserToCasLogin(
            //            new Guid("5B95F3B2-C265-4E1A-91AB-60FC449E96EB"),
            //            new Guid("85346158-DB2E-49CE-80AC-0E868527DF2B"),
            //            new Guid("37B473AE-B5A5-4839-91D5-80676A86B4B9"),
            //            returnUrl);
            //    }
            //    else
            //    {
            //        var sessionInfo = cas.GetSessionFromSaml(httpContextBase);

            //        if (sessionInfo != null)
            //        {
            //            HttpContext.Session.Add("UserSessionInfo", sessionInfo);
            //            HttpContext.Session.Timeout = 20;
            //            SetUsage(_usgAct.LogIn);
            //        }

            //        returnUrl = returnUrl.Replace("http://thi.advisory.com:81", "https://thi.advisory.com");

            //        Response.Redirect(returnUrl);
            //    }
            //}
        }

        public void Logout()
        {

            SetUsage(_usgAct.LogOut);
            var cas = new CasAuthenticationService(SamlHelperConfiguration.Config, UserSessionHandler.Get());
            cas.SendCasSingleSignOut();
            Session["UserSessionInfo"] = null;
        }

        [HttpPost]
        public JsonResult THIScoresDrillDown(int ProjectKey)
        {
            dynamic minesweeperData = null;
            XElement root = new XElement("Parameters");
            XElement projElem = new XElement("ProjectKey", ProjectKey);
            root.Add(projElem);
            XDocument doc = new XDocument(root);
            SetUsage(_usgAct.MemberThiScoresMoreDetails, doc);
            

            var dataLoadDrill = db.DataLoadDrills.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.Project_ProjectName,
                a.ShortDescription,
                a.MemberSupport_SupportIssueNumber,
                a.DataSubmissionTargetDate,
                a.FilesReceived,
                a.DaysLate_DataLoad,
                a.DateTimeClosed,
                a.ETLCompletion_DataLoad
            }).OrderBy(a=> a.DateTimeClosed);

            var missingElementsIp = db.MemberDataElements_IP.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.DataElement
            }).OrderBy(a=> a.DataElement);

            var missingElementsOp = db.MemberDataElements_OP.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.DataElement
            }).OrderBy(a => a.DataElement);

            var missingElementsOppe = db.MemberDataElements_OPPE.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.DataElement
            }).OrderBy(a => a.DataElement);

            var projectMinesweeperData = db.MinesweeperDatas.Where(a => a.projectkey == ProjectKey);

            if (projectMinesweeperData.Count() != 0)
            {
                var maxYear = int.Parse(projectMinesweeperData.Max(a => a.issueyear).ToString());

                var minesweeperRecentYearData =
                    db.MinesweeperDatas.Where(
                        a => a.projectkey == ProjectKey && a.issueyear == maxYear);

                var maxMonth = int.Parse(minesweeperRecentYearData.Max(a => a.issuemonth).ToString());

                minesweeperData =
                        db.MinesweeperDatas.Where(
                                a => a.projectkey == ProjectKey && a.issueyear == maxYear && a.issuemonth == maxMonth)
                            .Select(
                                x => new
                                {
                                    x.issuename,
                                    x.issuetype,
                                    x.issuecount,
                                    x.dischargecount,
                                    x.monthlybenchmark
                                });
            }

            

            return Json(new
            {
                dlDrill = dataLoadDrill,
                MissingIp = missingElementsIp,
                MissingOp = missingElementsOp,
                MissingOppe = missingElementsOppe,
                MinesweeperData = minesweeperData
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult THIScoresGeneration(int ProjectKey, int Year, int Month)
        {

            XElement root = new XElement("Parameters");
            XElement projElem = new XElement("ProjectKey", ProjectKey);
            XElement yrElem = new XElement("Year", Year);
            XElement mnthElem = new XElement("Month", Month);
            root.Add(projElem);
            root.Add(yrElem);
            root.Add(mnthElem);
            XDocument doc = new XDocument(root);
            SetUsage(_usgAct.GenerateThiScore, doc);

            var memberInfo =
                db.CCCTHIScoreColors.Where(a => a.ProjectKey == ProjectKey && a.Year == Year && a.Month == Month)
                    .Join(db.SalesforceProjects, a => a.ProjectKey, b => b.ProjectKey,
                        (a, b) => new {CCCTHIScoreColors = a, SalesforceProject = b})
                    .Select(a => new
                    {
                        a.SalesforceProject.ProjectName
                        ,
                        a.CCCTHIScoreColors.Data_Submission_Timeliness,
                        a.CCCTHIScoreColors.Data_Submission_Timeliness_CC
                        ,
                        a.CCCTHIScoreColors.C__Member_Support_Tickets,
                        a.CCCTHIScoreColors.C__Member_Support_Tickets_CC
                        ,
                        a.CCCTHIScoreColors.C__Critical_Diagnostics,
                        a.CCCTHIScoreColors.C__Critical_Diagnostics_CC
                        ,
                        a.CCCTHIScoreColors.C_Ability_to_Keep_up_w_Releases,
                        a.CCCTHIScoreColors.C_Ability_to_Keep_up_w_Releases_CC
                        ,
                        a.CCCTHIScoreColors.C_MineSweeper,
                        a.CCCTHIScoreColors.C_MineSweeper_CC
                        ,
                        a.CCCTHIScoreColors.C_Data_Lag,
                        a.CCCTHIScoreColors.C_Data_Lag_CC
                        ,
                        a.CCCTHIScoreColors.C__USE_SSA,
                        a.CCCTHIScoreColors.C__USE_SSA_CC
                        ,
                        a.CCCTHIScoreColors.C_USE_Compass_Connect,
                        a.CCCTHIScoreColors.C_USE_Compass_Connect_CC
                        ,
                        a.CCCTHIScoreColors.C_Data_Elements_Present,
                        a.CCCTHIScoreColors.C_Data_Elements_Present_CC
                        ,

                        a.CCCTHIScoreColors.C_CriticalInternalDiagnostics,
                        a.CCCTHIScoreColors.C_CriticalInternalDiagnostics_CC
                        ,

                        a.CCCTHIScoreColors.THIScore,
                        a.CCCTHIScoreColors.THISCORE_CC
                    });

            var ThiTrend = db.CCCTHIScoreColors.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.ProjectPhase,
                a.Year,
                a.Month,
                a.THIScore,
                a.THISCORE_CC
            }).OrderBy(a => new {a.Year, a.Month});

            var MemberContractInfo = db.CCCMemberContractInfoes.Where(
                    a => a.ProjectKey == ProjectKey && a.Year == Year && a.Month == Month)
                .Select(a => new
                {
                    a.AnnualContractValue,
                    a.LatestContractEndDate,
                    a.NextContractDecision,
                    a.OptOut,
                    a.CurrentContractRiskType,
                    a.UploadFrequency,
                    a.LoginCount
                });

            var ETLCompletion = db.ETLCompletion(ProjectKey, Year, Month).Select(a => a.Avg_ETLDurationn_DataLoad);

            return
                Json(
                    new
                    {
                        THIParamaterScores = memberInfo,
                        ThiTrendByPeriod = ThiTrend,
                        ContractInfo = MemberContractInfo,
                        AverageETL = ETLCompletion
                    }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult MemberTHIScores()
        {

            SiamRedirection(HttpContext.Request.Url.AbsoluteUri);
            SetUsage(_usgAct.MemberThiScoresTab);

            var projectList = db.SalesforceProjects.Where(a => a.ProjectPhase.ToLower().Contains("value stream")).OrderBy(a=>a.ProjectName);  


            ViewBag.Project = new SelectList(projectList, "ProjectKey", "ProjectName");
            ViewBag.RefreshDate = db.ToolRefreshDates.Max(a => a.RecentRundate);
            return View();
        }

        public ActionResult Index()
        {
            SiamRedirection(Request.Url.AbsoluteUri);
            SetUsage(_usgAct.ThiDataLogsTab);
            var projectList = db.SalesforceProjects.Where(a => a.ProjectPhase.ToLower().Contains("value stream")).OrderBy(a => a.ProjectName);
            ViewBag.Project = new SelectList(projectList, "ProjectKey", "ProjectName");
            var feedBackData =
                db.THI_AnalysisFeedback.Include(t => t.SalesforceProject)
                    .OrderByDescending(model => model.AnalysisCreatedDate);
            return View(feedBackData.ToList());
        }


        public ActionResult MemberDischargeVolumes()
        {
            SiamRedirection(HttpContext.Request.Url.AbsoluteUri);
            SetUsage(_usgAct.MemberStatisticsTab);
            var projectList = db.SalesforceProjects.Where(a => a.ProjectPhase.ToLower().Contains("value stream")).OrderBy(a => a.ProjectName);
            ViewBag.Project = new SelectList(projectList, "ProjectKey", "ProjectName");
            ViewBag.RefreshDate = db.ToolRefreshDates.Max(a => a.RecentRundate);
            return View();
        }

        [HttpPost]
        public JsonResult getMemberInformation(int project)
        {
            XElement root = new XElement("Parameters");
            XElement projElem = new XElement("ProjectKey", project);
            root.Add(projElem);
            XDocument doc = new XDocument(root);

            SetUsage(_usgAct.MemberStatisticsSelectionProject,doc);

            var memberInfo =
                db.SalesforceProjects.Where(a => a.ProjectKey == project)
                    .Join(db.CustomOPPEinfoes, a => a.ProjectKey, b => b.Project,
                        (a, b) => new {SalesforceProject = a, CustomOPPEinfo = b})
                    .Select(
                        x =>
                            new
                            {
                                x.CustomOPPEinfo.MaxRR,
                                x.CustomOPPEinfo.TotalCount,
                                x.SalesforceProject.RunAPRDRG,
                                x.SalesforceProject.CCC_3M,
                                x.SalesforceProject.APRDRGAggregate
                            });
            return new JsonResult {Data = memberInfo, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpPost]
        public JsonResult MemberDischargeVolumes(int facilitySelect)
        {

            XElement root = new XElement("Parameters");
            XElement projElem = new XElement("FacilityKey", facilitySelect);
            root.Add(projElem);
            XDocument doc = new XDocument(root);
            SetUsage(_usgAct.MemberStatisticsGeneratePatientVolumes,doc);

            var caseVolumes =
                db.DischargeVolumes.Where(fa => fa.ProjectHospital == facilitySelect)
                    .Select(mo => new {mo.Year, mo.Month, mo.IP_COUNT, mo.OP_COUNT})
                    .OrderByDescending(mo => new {mo.Year, mo.Month})
                    .ToList();
            return new JsonResult {Data = caseVolumes, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpPost]
        public JsonResult FilterHospitalsByProject(int project)
        {
            var projectHospitaList =
                db.ProjectHospitals.Where(a => a.Project == project)
                    .Select(b => new {b.ProjectHospitalKey, b.HospitalName})
                    .ToList();
            return new JsonResult {Data = projectHospitaList, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int? Project)
        {
            XElement root = new XElement("Parameters");
            XElement projElem = new XElement("ProjectKey", Project);
            root.Add(projElem);
            XDocument doc = new XDocument(root);
            SetUsage(_usgAct.ThiDataLogsFilterbyProjects, doc);

            var projectList = db.SalesforceProjects.Where(a => a.ProjectPhase.ToLower().Contains("value stream")).OrderBy(a => a.ProjectName);
            ViewBag.Project = new SelectList(projectList , "ProjectKey", "ProjectName");

            if (Project == 1)
            {
                var feedBackData =
                    db.THI_AnalysisFeedback.Include(t => t.SalesforceProject)
                        .OrderByDescending(model => model.AnalysisCreatedDate);
                return View(feedBackData.ToList());
            }
            else
            {
                var feedBackData =
                    db.THI_AnalysisFeedback.Include(t => t.SalesforceProject)
                        .Where(model => model.Project == Project)
                        .OrderByDescending(model => model.AnalysisCreatedDate);
                return View(feedBackData.ToList());
            }
        }

        // GET: THI_AnalysisFeedback/Details/5
        public ActionResult Details(int? id)
        {
            SiamRedirection(HttpContext.Request.Url.AbsoluteUri);

            XElement root = new XElement("Parameters");
            XElement feedbackElem = new XElement("FeedbackKey", id);
            root.Add(feedbackElem);
            XDocument doc = new XDocument(root);
            SetUsage(_usgAct.MemberThiFeedbackInformation, doc);
            var feedBackData = db.THI_AnalysisFeedback.Find(id);
            return View(feedBackData);
        }

        // GET: THI_AnalysisFeedback/Create
        public ActionResult Create()
        {
  
            SiamRedirection( HttpContext.Request.Url.AbsoluteUri);
            SetUsage(_usgAct.ThiDataFeedbackTab);

            ViewBag.AnalysisSummary = new SelectList(db.AnalysisSummaries, "AnalysisSummaryKey",
                "AnalysisSummaryDescription");
            ViewBag.CriticalDiagnostics = new SelectList(db.CriticalDiagnostics, "CriticalDiagnosticKey",
                "CriticalDiagnosticsDescription");
            ViewBag.DAS_Findings = new SelectList(db.DAS_Findings, "DAS_FindingsKey", "DAS_FindingsDescription");
            ViewBag.DataElementsPresent = new SelectList(db.DataElements, "DataElementsKey", "DataElementsDescription");
            ViewBag.DataLag = new SelectList(db.DataLags, "DataLagKey", "DataLagDescription");
            ViewBag.DataSubmissionTimelines = new SelectList(db.DataLoadTimelinesses, "DataLoadTimelinessKey",
                "DataLoadTimelinessDescription");
            ViewBag.MemberSupportTickets = new SelectList(db.MemberSupportTickets, "MemberSupportTicketsKey",
                "MemberSupportTicketsDescription");
            ViewBag.Minesweeper = new SelectList(db.Minesweepers, "MinesweeperKey", "MinesweeperDescription");

            var projectList = db.SalesforceProjects.Where(a => a.ProjectPhase.ToLower().Contains("value stream")).OrderBy(a => a.ProjectName);
            ViewBag.Project = new SelectList(projectList, "ProjectKey", "ProjectName");

            ViewBag.SSA_Findings = new SelectList(db.SSA_Findings, "SSA_FindingsKey", "SSA_FindingsDescription");
            return View();
        }

        // POST: THI_AnalysisFeedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
        [Bind(
            Include =
                "AnalysisFeedbackKey,Project,AnalysisSummary,BusinessAnalyst,AnalysisCreatedDate,AnalysisNotes,AnalysisRecommendations,DataSubmissionTimelines,Comments_DataSubmissionTimelines,DataLag,Comments_DataLag,CriticalDiagnostics,Comments_CriticalDiagnostics,MemberSupportTickets,Comments_MemberSupportTickets,DataElementsPresent,Comments_DataElementsPresent,Minesweeper,Comments_Minesweeper,DAS_Findings,Comments_DAS_Findings,SSA_Findings,Comments_SSA_Findings,LastModifiedBy,LastModifiedDate,IsDeleted"
        )] THI_AnalysisFeedback tHI_AnalysisFeedback)
        {
            tHI_AnalysisFeedback.AnalysisCreatedDate = DateTime.Now;
            tHI_AnalysisFeedback.LastModifiedDate = DateTime.Now;

            tHI_AnalysisFeedback.LastModifiedBy = tHI_AnalysisFeedback.BusinessAnalyst;
            tHI_AnalysisFeedback.IsDeleted = false;

            ModelState["LastModifiedBy"].Errors.Clear();

            if (ModelState.IsValid && tHI_AnalysisFeedback.AnalysisSummary != -1 &&
                tHI_AnalysisFeedback.DataSubmissionTimelines != -1
                && tHI_AnalysisFeedback.DataLag != -1 && tHI_AnalysisFeedback.CriticalDiagnostics != -1
                && tHI_AnalysisFeedback.MemberSupportTickets != -1 && tHI_AnalysisFeedback.DataElementsPresent != -1
                && tHI_AnalysisFeedback.Minesweeper != -1 && tHI_AnalysisFeedback.DAS_Findings != -1 &&
                tHI_AnalysisFeedback.SSA_Findings != -1
                && tHI_AnalysisFeedback.Project != 0 && tHI_AnalysisFeedback.Project != 1
            )
            {
                db.THI_AnalysisFeedback.Add(tHI_AnalysisFeedback);
                db.SaveChanges();
                XElement root = new XElement("Parameters");
                XElement feedbackElem1 = new XElement("ProjectKey", tHI_AnalysisFeedback.Project);
                XElement feedbackElem2 = new XElement("AS", tHI_AnalysisFeedback.AnalysisSummary);
                XElement feedbackElem3 = new XElement("BA", tHI_AnalysisFeedback.BusinessAnalyst);
                XElement feedbackElem4 = new XElement("AN", tHI_AnalysisFeedback.AnalysisNotes);
                XElement feedbackElem5 = new XElement("AR", tHI_AnalysisFeedback.AnalysisRecommendations);
                XElement feedbackElem6 = new XElement("DST", tHI_AnalysisFeedback.DataSubmissionTimelines);
                XElement feedbackElem7 = new XElement("CDST", tHI_AnalysisFeedback.Comments_DataSubmissionTimelines);
                XElement feedbackElem8 = new XElement("DL", tHI_AnalysisFeedback.DataLag);
                XElement feedbackElem9 = new XElement("CDL", tHI_AnalysisFeedback.Comments_DataLag);
                XElement feedbackElem10 = new XElement("CD", tHI_AnalysisFeedback.CriticalDiagnostics);
                XElement feedbackElem11 = new XElement("CCD", tHI_AnalysisFeedback.Comments_CriticalDiagnostics);
                XElement feedbackElem12 = new XElement("MST", tHI_AnalysisFeedback.MemberSupportTickets);
                XElement feedbackElem13 = new XElement("CMST", tHI_AnalysisFeedback.Comments_MemberSupportTickets);
                XElement feedbackElem14 = new XElement("DEP", tHI_AnalysisFeedback.DataElementsPresent);
                XElement feedbackElem15 = new XElement("CDEP", tHI_AnalysisFeedback.Comments_DataElementsPresent);
                XElement feedbackElem16 = new XElement("M", tHI_AnalysisFeedback.Minesweeper);
                XElement feedbackElem17 = new XElement("CM", tHI_AnalysisFeedback.Comments_Minesweeper);
                XElement feedbackElem18 = new XElement("DAS", tHI_AnalysisFeedback.DAS_Findings);
                XElement feedbackElem19 = new XElement("CDAS", tHI_AnalysisFeedback.Comments_DAS_Findings);
                XElement feedbackElem20 = new XElement("SSA", tHI_AnalysisFeedback.SSA_Findings);
                XElement feedbackElem21 = new XElement("CSSA", tHI_AnalysisFeedback.Comments_SSA_Findings);

                root.Add(feedbackElem1);
                root.Add(feedbackElem2);
                root.Add(feedbackElem3);
                root.Add(feedbackElem4);
                root.Add(feedbackElem5);
                root.Add(feedbackElem6);
                root.Add(feedbackElem7);
                root.Add(feedbackElem8);
                root.Add(feedbackElem9);
                root.Add(feedbackElem10);
                root.Add(feedbackElem11);
                root.Add(feedbackElem12);
                root.Add(feedbackElem13);
                root.Add(feedbackElem14);
                root.Add(feedbackElem15);
                root.Add(feedbackElem16);
                root.Add(feedbackElem17);
                root.Add(feedbackElem18);
                root.Add(feedbackElem19);
                root.Add(feedbackElem20);
                root.Add(feedbackElem21);                

                XDocument doc = new XDocument(root);
                SetUsage(_usgAct.ThiDataFeedbackSubmission, doc);
                return RedirectToAction("Index");
            }

            ViewBag.AnalysisSummary = new SelectList(db.AnalysisSummaries, "AnalysisSummaryKey",
                "AnalysisSummaryDescription", tHI_AnalysisFeedback.AnalysisSummary);
            ViewBag.CriticalDiagnostics = new SelectList(db.CriticalDiagnostics, "CriticalDiagnosticKey",
                "CriticalDiagnosticsDescription", tHI_AnalysisFeedback.CriticalDiagnostics);
            ViewBag.DAS_Findings = new SelectList(db.DAS_Findings, "DAS_FindingsKey", "DAS_FindingsDescription",
                tHI_AnalysisFeedback.DAS_Findings);
            ViewBag.DataElementsPresent = new SelectList(db.DataElements, "DataElementsKey", "DataElementsDescription",
                tHI_AnalysisFeedback.DataElementsPresent);
            ViewBag.DataLag = new SelectList(db.DataLags, "DataLagKey", "DataLagDescription",
                tHI_AnalysisFeedback.DataLag);
            ViewBag.DataSubmissionTimelines = new SelectList(db.DataLoadTimelinesses, "DataLoadTimelinessKey",
                "DataLoadTimelinessDescription", tHI_AnalysisFeedback.DataSubmissionTimelines);
            ViewBag.MemberSupportTickets = new SelectList(db.MemberSupportTickets, "MemberSupportTicketsKey",
                "MemberSupportTicketsDescription", tHI_AnalysisFeedback.MemberSupportTickets);
            ViewBag.Minesweeper = new SelectList(db.Minesweepers, "MinesweeperKey", "MinesweeperDescription",
                tHI_AnalysisFeedback.Minesweeper);

            var projectList = db.SalesforceProjects.Where(a => a.ProjectPhase.ToLower().Contains("value stream")).OrderBy(a => a.ProjectName);
            ViewBag.Project = new SelectList(projectList, "ProjectKey", "ProjectName",
                tHI_AnalysisFeedback.Project);
            ViewBag.SSA_Findings = new SelectList(db.SSA_Findings, "SSA_FindingsKey", "SSA_FindingsDescription",
                tHI_AnalysisFeedback.SSA_Findings);
            return View();
        }
    }
}