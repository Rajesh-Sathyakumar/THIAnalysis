using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

using System.Web.Mvc;
using System.Xml.Linq;
using SamlHelperLibrary;
using SamlHelperLibrary.Configuration;
using SamlHelperLibrary.Models;
using SamlHelperLibrary.Service;
using THI_Analysis.Constants;
using THI_Analysis.Dto;
using THI_Analysis.Models;
using THI_Analysis.Utility;

namespace THI_Analysis.Controllers
{
    public class THI_AnalysisFeedbackController : Controller
    {
        private readonly string _crimsonProvisioningService =
            ConfigurationManager.AppSettings["CrimsonProvisioningService"];

        private readonly CCCOperationsEntities _db = new CCCOperationsEntities();
        private readonly Guid _environmentKey = Guid.Parse(ConfigurationManager.AppSettings["EnvironmentKey"]);
        private readonly Guid _memberOrgKey = Guid.Parse(ConfigurationManager.AppSettings["MemberOrgKey"]);
        private readonly Guid _productKey = Guid.Parse(ConfigurationManager.AppSettings["ProductKey"]);
        private readonly string _siamBaseUrl = ConfigurationManager.AppSettings["SIAMBaseURL"];
        private readonly UserCreationService _userService = new UserCreationService();
        private readonly UsageActivityList _usgAct = new UsageActivityList();


        public ActionResult Errorpage()
        {
            return View();
        }


        public ActionResult Unauthorized()
        {
            return View();
        }

        [HttpPost]
        public void InsertPendingUser(ManageUserDetail userDetail)
        {

            _userService.ConfirmUsersEnrollment(new ConfirmUserEnrollment {
                ApplicationKey = _productKey,
                EnvironmentKey = _environmentKey,
                UserMemberOrgKeyList = new List<UserMemberOrgKeyPair>
                {
                    new UserMemberOrgKeyPair
                    {
                        UserKey = userDetail.UserGuid,
                        MemberOrgKey = _memberOrgKey
                    }
                }

            }, _siamBaseUrl+ _crimsonProvisioningService, "User/EnrollUsers", Session["SessionAuthToken"].ToString());

            _db.Users.Add(
                new User
                {
                    UserGUID = userDetail.UserGuid.ToString(),
                    FirstName = userDetail.FirstName,
                    LastName = userDetail.LastName,
                    Username = userDetail.UserId,
                    Admin = int.Parse(userDetail.IsAdmin),
                    IsActive = (userDetail.IsActive == "1"),
                    Email = userDetail.Email,
                    Role = userDetail.UserRole,
                    Product = userDetail.UserProduct,
                    CreatedDate = DateTime.Now
                }

             );

            var userKey = _db.Users.Where(a => a.UserGUID == userDetail.UserGuid.ToString()).Select(a => a.Userkey).SingleOrDefault();

            if (userDetail.Acls != null)
            {
                var userAcls = userDetail.Acls.Join(_db.ACLs, a => a.ToString(), b => b.AclName,
                    (a, b) => new { ACLBridge = a, ACL = b });

                foreach (var userAcl in userAcls)
                {
                    _db.ACLBridges.AddOrUpdate(new ACLBridge() { Userkey = userKey, AclKey = userAcl.ACL.AclKey });
                }
            }

            _db.SaveChanges();
        }


        [HttpPost]
        public JsonResult GetUserParameters()
        {
            
            var roles = _db.UserRoles.Select(a => new { a.RoleKey, a.RoleName });

            var products = _db.Products.Select(a => new { a.ProductKey, a.ProductName });

            var acls = _db.ACLs.Select(a => new { a.AclKey, a.AclName });

            return Json(new
            {
                Roles = roles,
                Products = products,
                Acls = acls
            });
        }


        [HttpPost]
        public JsonResult GetUserDetails(int userId)
          {
            var userAcls = _db.Users.Join(_db.ACLBridges, a => a.Userkey, b => b.Userkey,
                        (a, b) => new { Users = a, ACLBridge = b })
                        .Where(a => a.Users.Userkey == userId)
                        .Select(a => a.ACLBridge.AclKey.ToString()
                    );


            var userDetails = _db.Users.Where(a => a.Userkey == userId).Select(a => new
            {
                a.FirstName,
                a.LastName,
                a.Username,
                a.Email,
                a.Admin,
                a.IsActive,
                a.Product,
                a.Role
            });


            return Json(new
            {
                UserDetails = userDetails,
                UserAcls = userAcls,
            });
        }

        [HttpPost]
        public void UpdateUser(ManageUserDetail userData)
        {
            var dbUser = _db.Users.FirstOrDefault(a => a.Userkey == userData.UserKey);
            var userAclKeys = dbUser.ACLBridges.Where(a => a.Userkey == userData.UserKey);

            _db.ACLBridges.RemoveRange(userAclKeys);

            if (userData.Acls != null)
            {
                var userAcls = userData.Acls.Join(_db.ACLs, a => a.ToString(), b => b.AclName,
                    (a, b) => new { ACLBridge = a, ACL = b });

                foreach (var userAcl in userAcls)
                {
                    _db.ACLBridges.AddOrUpdate(new ACLBridge() { Userkey = userData.UserKey, AclKey = userAcl.ACL.AclKey });
                }
            }
            
            if (dbUser != null)
            {
                dbUser.Role = userData.UserRole;
                dbUser.Product = userData.UserProduct;
                dbUser.IsActive = userData.IsActive != "0";
                dbUser.Admin = int.Parse(userData.IsAdmin);
                _db.Entry(dbUser).State = EntityState.Modified;
            }

            _db.SaveChanges();
        }
        
        public ActionResult EditUserPermission()
        {
            if (Request.Url != null) {
                if(!SiamRedirection(Request.Url.AbsoluteUri + "?Acl=User Management"))
                {
                    return RedirectToAction("Unauthorized");
                }
            } 

            dynamic userStatus = new ExpandoObject();
            var getPendingUsers = new List<ManageUserDetail>();
            var deactivatedUsers = new List<ManageUserDetail>();

            if (Session?["SessionAuthToken"] != null)
            {
                getPendingUsers = _userService.GetUnprovisionedUsers( _productKey, _memberOrgKey, _environmentKey,
                        _siamBaseUrl + _crimsonProvisioningService,
                        string.Concat(
                            "User/GetUnprovisionedUsersByMemberApplicationEnvironment",
                            "?memberOrgId={{{0}}}&applicationId={{{1}}}&environmentId={{{2}}}"),
                        Session["SessionAuthToken"].ToString())
                    .Select(
                        a =>
                            new ManageUserDetail
                            {
                                UserKey = null,
                                FirstName = a.FirstName,
                                LastName = a.LastName,
                                Email = a.Email,
                                IsAdmin = "NA",
                                UserGuid = a.UserKey,
                                UserId = a.UserName
                            }).ToList();


                var deactivatedUsersCount = _userService.GetDeactivatedUsersCount(
                    new UsersCount
                    {
                        MemberOrgKeys = new[] {_memberOrgKey},
                        ApplicationKey = _productKey,
                        EnvironmentKeys = new[] {_environmentKey}
                    },
                    _siamBaseUrl + _crimsonProvisioningService,
                    "User/GetUserCountAndStatus", Session["SessionAuthToken"].ToString()
                );

                for (var i = 0; i < deactivatedUsersCount; i += 20)
                    deactivatedUsers.AddRange(_userService.GetDeactivatedUsers(new SearchUsersPostData
                            {
                                Keyword = "",
                                AccountStatus = 5,
                                UserType = 0,
                                SortColumn = "LastName,FirstName",
                                SortDirection = 0,
                                Index = i,
                                NoofItems = 20,
                                MemberOrgKeys = new[] {_memberOrgKey},
                                ApplicationKey = _productKey,
                                EnvironmentKeys = new[] {_environmentKey}
                            },
                            _siamBaseUrl + _crimsonProvisioningService,
                            "User/SearchUsers",
                            Session["SessionAuthToken"].ToString()
                        )
                        .Select(
                            a =>
                                new ManageUserDetail
                                {
                                    UserGuid = a.UserKey                                   
                                })
                        .ToList());

                foreach (var user in deactivatedUsers)
                {
                    var dbUser = _db.Users.FirstOrDefault(a => a.UserGUID == user.UserGuid.ToString());

                    if (dbUser != null)
                    {
                        dbUser.IsActive = false;
                        _db.Entry(dbUser).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                }
            }

            userStatus.pendingUsers = getPendingUsers;

            userStatus.inactiveUsers = _db.Users.Where(a => a.IsActive == false).Select(b => new ManageUserDetail
            {
                UserKey = b.Userkey,
                FirstName = b.FirstName,
                LastName = b.LastName,
                Email = b.Email,
                IsAdmin = b.Admin.ToString(),
                UserId = b.Username
            }).ToList();

            userStatus.activeUsers = _db.Users.Where(a => a.IsActive).Select(b => new ManageUserDetail
            {
                UserKey = b.Userkey,
                FirstName = b.FirstName,
                LastName = b.LastName,
                Email = b.Email,
                IsAdmin = b.Admin.ToString(),
                UserId = b.Username
            }).ToList();

            userStatus.allUsers = _db.Users.Select(b => new ManageUserDetail
            {
                UserKey = b.Userkey,
                FirstName = b.FirstName,
                LastName = b.LastName,
                Email = b.Email,
                IsAdmin = b.Admin.ToString(),
                UserId = b.Username
            }).ToList();

            return View(userStatus);
        }

        private static HttpClient ConstructRequest(SiamRequestServiceParam requestServiceParam)
        {
            var cookieContainer = new CookieContainer();

            var handler = new HttpClientHandler {CookieContainer = cookieContainer};

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = requestServiceParam.BaseAddress,
                Timeout =
                    TimeSpan.FromMilliseconds(requestServiceParam.HttpTimeOut)
            };

            httpClient.DefaultRequestHeaders.Add("Version", requestServiceParam.SiamApiVersion);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            foreach (var requestParam in requestServiceParam.FormParam)
                cookieContainer.Add(requestServiceParam.BaseAddress, new Cookie(requestParam.Key, requestParam.Value));

            return httpClient;
        }

        private string MakePostCallAndReturnCookie(SiamRequestServiceParam requestServiceParam)
        {
            var cookieValue = string.Empty;
            var httpClient = ConstructRequest(requestServiceParam);
            var psstringContent = new StringContent(
                requestServiceParam.SamlResponse,
                Encoding.UTF8,
                requestServiceParam.ContentType);
            var response = httpClient.PostAsync(requestServiceParam.FragmentUrl, psstringContent).Result;
            if (response.IsSuccessStatusCode)
            {
                IEnumerable<string> cookieHeader;
                response.Headers.TryGetValues("Set-Cookie", out cookieHeader);
                var cookie = cookieHeader.FirstOrDefault();
                if (cookie != null)
                    cookieValue = cookie.Split(';')[0].Split('=').ToList()[1] + '=';

                return cookieValue;
            }

            throw new WebException(response.Content.ReadAsStringAsync().Result);
        }


        private string PerformInitialAuthenticationAndCreateCookies(
            string siamApiUrl,
            string siamApiVersion,
            string samlRespone,
            int httpTimeOut)
        {
            var requestparam = new SiamRequestServiceParam
            {
                BaseAddress = new Uri(siamApiUrl),
                SamlResponse = samlRespone,
                FormParam = new List<KeyValuePair<string, string>>(),
                FragmentUrl = "Authenticate/",
                HttpTimeOut = httpTimeOut,
                SiamApiVersion = siamApiVersion,
                ContentType = "application/x-www-form-urlencoded"
            };
            return MakePostCallAndReturnCookie(requestparam);
        }

        private void UpdateSessions(UserSessionInfo sessionInfo)
        {
            if (HttpContext.Session != null)
            {
                HttpContext.Session.Add("UserSessionInfo", sessionInfo);
                HttpContext.Session.Timeout = 20;
            }

            var aclAccess = new Dictionary<string, bool>();

            foreach (var acls in _db.ACLs)
            {
                aclAccess.Add(
                    acls.AclName, false
                );
            }

            var user = _db.Users.Where(a => a.UserGUID == sessionInfo.userKey.ToString()).Select(a=> new {
                a.Userkey,
                a.IsActive,
                a.Admin
            }).FirstOrDefault();

            if (user == null)
            {
                RedirectToAction("Unauthorized");
            }

            
            Session["isAdmin"] = user.Admin;
            Session["isActive"] = user.IsActive;

            var userAcls = _db.ACLBridges.Join(_db.ACLs, a => a.AclKey, b => b.AclKey, (a, b) => new
            {
                userAcl = a,
                AclMaster = b
            }).Where(a => a.userAcl.Userkey == user.Userkey).Select(a => a.AclMaster.AclName);


            foreach (var useracl in userAcls)
            {
                aclAccess[useracl] = true;
            }

            Session["userAcls"] = aclAccess;

        }
        
        public bool SiamRedirection(string returnUrl)
        {
            //if (Session["UserSessionInfo"] == null)
            //{
            //    var cas = new CasAuthenticationService(SamlHelperConfiguration.Config, UserSessionHandler.Get());
            //    var httpContextBase = new HttpContextWrapper(System.Web.HttpContext.Current);
            //    if (!cas.IsSAMLResponse(httpContextBase) && (Session?["UserSessionInfo"] == null))
            //    {
            //        cas.RedirectUserToCasLogin(
            //            _memberOrgKey,
            //            _productKey,
            //            _environmentKey,
            //            returnUrl);
            //        return true;
            //    }
            //    else
            //    {

            //        var samlResponse = httpContextBase.Request.Form["SAMLResponse"];
            //        var relayState = httpContextBase.Request.Form["RelayState"];
            //        var samlAndRelayUrl =
            //            $"SAMLResponse={HttpUtility.UrlEncode(samlResponse)}&RelayState={HttpUtility.UrlEncode(relayState)}";
            //        var authToken = PerformInitialAuthenticationAndCreateCookies(
            //            _siamBaseUrl + _crimsonProvisioningService,
            //            "2",
            //            samlAndRelayUrl,
            //            10000000);
            //        Session["SessionAuthToken"] = authToken;
            //        var sessionInfo = cas.GetSessionFromSaml(httpContextBase);

            //        if (sessionInfo != null)
            //        {
            //            UpdateSessions(sessionInfo);
            //            SetUsage(_usgAct.LogIn);
            //        }

            //        Response.Cookies.Add(new HttpCookie("sessionId", authToken));
            //        returnUrl = returnUrl.Replace("http://thi.advisory.com:81", "https://thi.advisory.com");

            //        var urlSplit = returnUrl.Split('?');
            //        returnUrl = urlSplit[0];
            //        Response.Redirect(returnUrl);
            //    }
            //} else
            //{
            //    var urlSplit = returnUrl.Split('?');
            //    returnUrl = urlSplit[0];
            //    var aclName = urlSplit[1].Replace("Acl=", "");

            //    UpdateSessions((UserSessionInfo)Session["UserSessionInfo"]);

            //    if (Session?["userAcls"] != null && ((bool)Session["isActive"]) &&  ((int)Session["isAdmin"] == 1 ||
            //                  aclName == "Member Statistics" || (aclName != "User Management" &&
            //                  ((IDictionary<string, bool>)Session["userAcls"])[aclName])))
            //    {
            //        return true;
            //    }
            //}

            //return false;
            return true;
        }


        [HttpPost]
        public void SetUsage(int usageActivity, XDocument paramXml = null)
        {
            if (Session?["UserSessionInfo"] != null)
            {
                var currentUsage = new UsageActivityLog();
                var userGuid = ((UserSessionInfo) Session["UserSessionInfo"]).userKey.ToString();
                var sessionId = System.Web.HttpContext.Current.Session.SessionID;
                if (paramXml != null)
                    currentUsage.ParameterXML = paramXml.ToString();
                currentUsage.AccessTime = DateTime.Now;
                currentUsage.UsageActivityKey = usageActivity;
                currentUsage.Userkey = _db.Users.Where(a => a.UserGUID == userGuid).Max(a => a.Userkey);
                currentUsage.SessionID = sessionId;
                _db.UsageActivityLogs.Add(currentUsage);

                _db.SaveChanges();
            }
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
            var doc = new XDocument(root);
            SetUsage(_usgAct.MemberThiScoresMoreDetails, doc);


            var dataLoadDrill = _db.DataLoadDrills.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.Project_ProjectName,
                a.ShortDescription,
                a.MemberSupport_SupportIssueNumber,
                a.DataSubmissionTargetDate,
                a.FilesReceived,
                a.DaysLate_DataLoad,
                a.DateTimeClosed,
                a.ETLCompletion_DataLoad
            }).OrderBy(a => a.DateTimeClosed);

            var missingElementsIp = _db.MemberDataElements_IP.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.DataElement
            }).OrderBy(a => a.DataElement);

            var missingElementsOp = _db.MemberDataElements_OP.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.DataElement
            }).OrderBy(a => a.DataElement);

            var missingElementsOppe = _db.MemberDataElements_OPPE.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.DataElement
            }).OrderBy(a => a.DataElement);

            var projectMinesweeperData = _db.MinesweeperDatas.Where(a => a.projectkey == ProjectKey);

            if (projectMinesweeperData.Count() != 0)
            {
                var maxYear = int.Parse(projectMinesweeperData.Max(a => a.issueyear).ToString());

                var minesweeperRecentYearData =
                    _db.MinesweeperDatas.Where(
                        a => a.projectkey == ProjectKey && a.issueyear == maxYear);

                var maxMonth = int.Parse(minesweeperRecentYearData.Max(a => a.issuemonth).ToString());

                minesweeperData =
                        _db.MinesweeperDatas.Where(
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
            var root = new XElement("Parameters");
            var projElem = new XElement("ProjectKey", ProjectKey);
            var yrElem = new XElement("Year", Year);
            var mnthElem = new XElement("Month", Month);
            root.Add(projElem);
            root.Add(yrElem);
            root.Add(mnthElem);
            var doc = new XDocument(root);
            SetUsage(_usgAct.GenerateThiScore, doc);

            var memberInfo =
                _db.CCCTHIScoreColors.Where(a => a.ProjectKey == ProjectKey && a.Year == Year && a.Month == Month)
                    .Join(_db.SalesforceProjects, a => a.ProjectKey, b => b.ProjectKey,
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

            var ThiTrend = _db.CCCTHIScoreColors.Where(a => a.ProjectKey == ProjectKey).Select(a => new
            {
                a.ProjectPhase,
                a.Year,
                a.Month,
                a.THIScore,
                a.THISCORE_CC
            }).OrderBy(a => new {a.Year, a.Month});

            var MemberContractInfo = _db.CCCMemberContractInfoes.Where(
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

            var ETLCompletion = _db.ETLCompletion(ProjectKey, Year, Month).Select(a => a.Avg_ETLDurationn_DataLoad);

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

            if (Request.Url != null)
            {
                if (!SiamRedirection(Request.Url.AbsoluteUri + "?Acl=Member THI Scores"))
                {
                    return RedirectToAction("Unauthorized");
                }
            }
            SetUsage(_usgAct.MemberThiScoresTab);

            ViewBag.Project = new SelectList(_db.SalesforceProjects, "ProjectKey", "ProjectName");
            ViewBag.RefreshDate = _db.ToolRefreshDates.Max(a => a.RecentRundate);
            return View();
        }

        public ActionResult Index()
        {

            if (Request.Url != null)
            {
                if (!SiamRedirection(Request.Url.AbsoluteUri + "?Acl=THI Data Logs"))
                {
                    return RedirectToAction("Unauthorized");
                }
            }

            SetUsage(_usgAct.ThiDataLogsTab);

            ViewBag.Project = new SelectList(_db.SalesforceProjects, "ProjectKey", "ProjectName");
            var feedBackData =
                _db.THI_AnalysisFeedback.Include(t => t.SalesforceProject)
                    .OrderByDescending(model => model.AnalysisCreatedDate);
            return View(feedBackData.ToList());
        }


        public ActionResult MemberDischargeVolumes()
        {

            if (Request.Url != null)
            {
                if (!SiamRedirection(Request.Url.AbsoluteUri + "?Acl=Member Statistics"))
                {
                    return RedirectToAction("Unauthorized");
                }
            }
            SetUsage(_usgAct.MemberStatisticsTab);

            ViewBag.Project = new SelectList(_db.SalesforceProjects, "ProjectKey", "ProjectName");
            ViewBag.RefreshDate = _db.ToolRefreshDates.Max(a => a.RecentRundate);
            return View();
        }

        [HttpPost]
        public JsonResult getMemberInformation(int project)
        {
            var root = new XElement("Parameters");
            var projElem = new XElement("ProjectKey", project);
            root.Add(projElem);
            var doc = new XDocument(root);

            SetUsage(_usgAct.MemberStatisticsSelectionProject, doc);

            var memberInfo =
                _db.SalesforceProjects.Where(a => a.ProjectKey == project)
                    .Join(_db.CustomOPPEinfoes, a => a.ProjectKey, b => b.Project,
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
            var root = new XElement("Parameters");
            var projElem = new XElement("FacilityKey", facilitySelect);
            root.Add(projElem);
            var doc = new XDocument(root);
            SetUsage(_usgAct.MemberStatisticsGeneratePatientVolumes, doc);

            var caseVolumes =
                _db.DischargeVolumes.Where(fa => fa.ProjectHospital == facilitySelect)
                    .Select(mo => new {mo.Year, mo.Month, mo.IP_COUNT, mo.OP_COUNT})
                    .OrderByDescending(mo => new {mo.Year, mo.Month})
                    .ToList();
            return new JsonResult {Data = caseVolumes, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpPost]
        public JsonResult FilterHospitalsByProject(int? project)
        {
            var projectHospitaList =
                _db.ProjectHospitals.Where(a => a.Project == project)
                    .Select(b => new {b.ProjectHospitalKey, b.HospitalName})
                    .ToList();
            return new JsonResult {Data = projectHospitaList, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int? Project)
        {
            var root = new XElement("Parameters");
            var projElem = new XElement("ProjectKey", Project);
            root.Add(projElem);
            var doc = new XDocument(root);
            SetUsage(_usgAct.ThiDataLogsFilterbyProjects, doc);


            ViewBag.Project = new SelectList(_db.SalesforceProjects, "ProjectKey", "ProjectName");

            if (Project == 1)
            {
                var feedBackData =
                    _db.THI_AnalysisFeedback.Include(t => t.SalesforceProject)
                        .OrderByDescending(model => model.AnalysisCreatedDate);
                return View(feedBackData.ToList());
            }
            else
            {
                var feedBackData =
                    _db.THI_AnalysisFeedback.Include(t => t.SalesforceProject)
                        .Where(model => model.Project == Project)
                        .OrderByDescending(model => model.AnalysisCreatedDate);
                return View(feedBackData.ToList());
            }
        }

        // GET: THI_AnalysisFeedback/Details/5
        public ActionResult Details(int? id)
        {

            if (Request.Url != null)
            {
                if (!SiamRedirection(Request.Url.AbsoluteUri + "?Acl=THI Data Logs"))
                {
                    return RedirectToAction("Unauthorized");
                }
            }

            var root = new XElement("Parameters");
            var feedbackElem = new XElement("FeedbackKey", id);
            root.Add(feedbackElem);
            var doc = new XDocument(root);
            SetUsage(_usgAct.MemberThiFeedbackInformation, doc);
            var feedBackData = _db.THI_AnalysisFeedback.Find(id);
            return View(feedBackData);
        }

        // GET: THI_AnalysisFeedback/Create
        public ActionResult Create()
        {
            if (Request.Url != null)
            {
                if (!SiamRedirection(Request.Url.AbsoluteUri + "?Acl=THI Data Feedback"))
                {
                    return RedirectToAction("Unauthorized");
                }
            }

            SetUsage(_usgAct.ThiDataFeedbackTab);

            ViewBag.AnalysisSummary = new SelectList(_db.AnalysisSummaries, "AnalysisSummaryKey",
                "AnalysisSummaryDescription");
            ViewBag.CriticalDiagnostics = new SelectList(_db.CriticalDiagnostics, "CriticalDiagnosticKey",
                "CriticalDiagnosticsDescription");
            ViewBag.DAS_Findings = new SelectList(_db.DAS_Findings, "DAS_FindingsKey", "DAS_FindingsDescription");
            ViewBag.DataElementsPresent = new SelectList(_db.DataElements, "DataElementsKey", "DataElementsDescription");
            ViewBag.DataLag = new SelectList(_db.DataLags, "DataLagKey", "DataLagDescription");
            ViewBag.DataSubmissionTimelines = new SelectList(_db.DataLoadTimelinesses, "DataLoadTimelinessKey",
                "DataLoadTimelinessDescription");
            ViewBag.MemberSupportTickets = new SelectList(_db.MemberSupportTickets, "MemberSupportTicketsKey",
                "MemberSupportTicketsDescription");

            ViewBag.Minesweeper = new SelectList(_db.Minesweepers, "MinesweeperKey", "MinesweeperDescription");
            ViewBag.Project = new SelectList(_db.SalesforceProjects, "ProjectKey", "ProjectName");
            ViewBag.SSA_Findings = new SelectList(_db.SSA_Findings, "SSA_FindingsKey", "SSA_FindingsDescription");

            return View();
        }


        private XElement addXmlElement(string elem, dynamic elemValue)
        {
            var feedbackElem = new XElement(elem, elemValue);
            return feedbackElem;
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
                _db.THI_AnalysisFeedback.Add(tHI_AnalysisFeedback);
                _db.SaveChanges();
                var root = new XElement("Parameters");

                root.Add(addXmlElement("ProjectKey", tHI_AnalysisFeedback.Project));
                root.Add(addXmlElement("AS", tHI_AnalysisFeedback.AnalysisSummary));
                root.Add(addXmlElement("BA", tHI_AnalysisFeedback.BusinessAnalyst));
                root.Add(addXmlElement("AN", tHI_AnalysisFeedback.AnalysisNotes));
                root.Add(addXmlElement("AR", tHI_AnalysisFeedback.AnalysisRecommendations));
                root.Add(addXmlElement("DST", tHI_AnalysisFeedback.DataSubmissionTimelines));
                root.Add(addXmlElement("CDST", tHI_AnalysisFeedback.Comments_DataSubmissionTimelines));
                root.Add(addXmlElement("DL", tHI_AnalysisFeedback.DataLag));
                root.Add(addXmlElement("CDL", tHI_AnalysisFeedback.Comments_DataLag));
                root.Add(addXmlElement("CD", tHI_AnalysisFeedback.CriticalDiagnostics));
                root.Add(addXmlElement("CCD", tHI_AnalysisFeedback.Comments_CriticalDiagnostics));
                root.Add(addXmlElement("MST", tHI_AnalysisFeedback.MemberSupportTickets));
                root.Add(addXmlElement("CMST", tHI_AnalysisFeedback.Comments_MemberSupportTickets));
                root.Add(addXmlElement("DEP", tHI_AnalysisFeedback.DataElementsPresent));
                root.Add(addXmlElement("CDEP", tHI_AnalysisFeedback.Comments_DataElementsPresent));
                root.Add(addXmlElement("M", tHI_AnalysisFeedback.Minesweeper));
                root.Add(addXmlElement("CM", tHI_AnalysisFeedback.Comments_Minesweeper));
                root.Add(addXmlElement("DAS", tHI_AnalysisFeedback.DAS_Findings));
                root.Add(addXmlElement("CDAS", tHI_AnalysisFeedback.Comments_DAS_Findings));
                root.Add(addXmlElement("SSA", tHI_AnalysisFeedback.SSA_Findings));
                root.Add(addXmlElement("CSSA", tHI_AnalysisFeedback.Comments_SSA_Findings));

                var doc = new XDocument(root);
                SetUsage(_usgAct.ThiDataFeedbackSubmission, doc);
                return RedirectToAction("Index");
            }

            ViewBag.AnalysisSummary = new SelectList(_db.AnalysisSummaries, "AnalysisSummaryKey",
                "AnalysisSummaryDescription", tHI_AnalysisFeedback.AnalysisSummary);
            ViewBag.CriticalDiagnostics = new SelectList(_db.CriticalDiagnostics, "CriticalDiagnosticKey",
                "CriticalDiagnosticsDescription", tHI_AnalysisFeedback.CriticalDiagnostics);
            ViewBag.DAS_Findings = new SelectList(_db.DAS_Findings, "DAS_FindingsKey", "DAS_FindingsDescription",
                tHI_AnalysisFeedback.DAS_Findings);
            ViewBag.DataElementsPresent = new SelectList(_db.DataElements, "DataElementsKey", "DataElementsDescription",
                tHI_AnalysisFeedback.DataElementsPresent);
            ViewBag.DataLag = new SelectList(_db.DataLags, "DataLagKey", "DataLagDescription",
                tHI_AnalysisFeedback.DataLag);
            ViewBag.DataSubmissionTimelines = new SelectList(_db.DataLoadTimelinesses, "DataLoadTimelinessKey",
                "DataLoadTimelinessDescription", tHI_AnalysisFeedback.DataSubmissionTimelines);
            ViewBag.MemberSupportTickets = new SelectList(_db.MemberSupportTickets, "MemberSupportTicketsKey",
                "MemberSupportTicketsDescription", tHI_AnalysisFeedback.MemberSupportTickets);
            ViewBag.Minesweeper = new SelectList(_db.Minesweepers, "MinesweeperKey", "MinesweeperDescription",
                tHI_AnalysisFeedback.Minesweeper);

            ViewBag.Project = new SelectList(_db.SalesforceProjects, "ProjectKey", "ProjectName",
                tHI_AnalysisFeedback.Project);
            ViewBag.SSA_Findings = new SelectList(_db.SSA_Findings, "SSA_FindingsKey", "SSA_FindingsDescription",
                tHI_AnalysisFeedback.SSA_Findings);
            return View();
        }
    }
}