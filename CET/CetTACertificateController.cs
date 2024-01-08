using Microsoft.AspNetCore.Mvc;
using NavesPortalforWebWithCoreMvc.Common;
using Microsoft.AspNetCore.Mvc;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.Models;
using NavesPortalforWebWithCoreMvc.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.ViewModels;
using Microsoft.CodeAnalysis;
using NuGet.Protocol.Core.Types;
using Syncfusion.EJ2.Base;
using static NuGet.Packaging.PackagingConstants;
using System.Collections;
using Syncfusion.EJ2.Linq;
using NavesPortalforWebWithCoreMvc.RfSystemModels;
using NavesPortalforWebWithCoreMvc.RfSystemData;
using System.Text.Json;

namespace NavesPortalforWebWithCoreMvc.Controllers.CET
{
    public class CetTACertificateController : Controller
    {
        private readonly INavesPortalCommonService _common;
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemRepository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public CetTACertificateController(INavesPortalCommonService common, BM_NAVES_PortalContext repository, RfSystemContext rfSystemRepository, IBM_NAVES_PortalContextProcedures procedures)
        {
            _common = common;
            _repository = repository;
            _rfSystemRepository = rfSystemRepository;
            _procedures = procedures;
        }
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View());
        }

        public async Task<IActionResult> UrlDataSource(string SearchString, DateTime? StartDate, DateTime? EndDate, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                if (SearchString is null)
                {
                    SearchString = "";
                }

                List<PNAV_CET_TA_CERT_GET_LISTResult> resultList = await _procedures.PNAV_CET_TA_CERT_GET_LISTAsync(SearchString, StartDate, EndDate);

                IEnumerable DataSource = resultList.AsEnumerable();
                DataOperations operation = new DataOperations();

                if (dm.Search != null && dm.Search.Count > 0)
                {
                    DataSource = operation.PerformSearching(DataSource, dm.Search);
                }

                if (dm.Sorted != null && dm.Sorted.Count > 0)
                {
                    DataSource = operation.PerformSorting(DataSource, dm.Sorted);
                }

                if (dm.Where != null && dm.Where.Count > 0)
                {
                    DataSource = operation.PerformFiltering(DataSource, dm.Where, dm.Where[0].Operator);
                }

                int count = DataSource.Cast<PNAV_CET_TA_CERT_GET_LISTResult>().Count();

                if (dm.Skip != 0)
                {
                    DataSource = operation.PerformSkip(DataSource, dm.Skip);
                }

                if (dm.Take != 0)
                {
                    DataSource = operation.PerformTake(DataSource, dm.Take);
                }

                return dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(new { result = DataSource });
            }
            catch (Exception ex)
            {
                return RedirectToAction("SaveException", "Error", new { ex = ex.InnerException.Message, returnController = "CetTACertificate", returnView = "Index" });
            }
        }

        public async Task<IActionResult> Create(CetTACertViewModel cetTACertViewModel)
        {
            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            // Kind of Certification 값 가져오기
            List<ApprovalViewModel> _KindOfCertification = new List<ApprovalViewModel>();
            _KindOfCertification.Add(new ApprovalViewModel { Text = "Initial", Value = "Initial" });
            _KindOfCertification.Add(new ApprovalViewModel { Text = "Renewal", Value = "Renewal" });
            _KindOfCertification.Add(new ApprovalViewModel { Text = "Reissue", Value = "Reissue" });
            _KindOfCertification.Add(new ApprovalViewModel { Text = "Occasional", Value = "Occasional" });

            ViewBag.KindOfCertification = _KindOfCertification;

            // Surveyor 이름과 번호 가져오기 위한 필터링
            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
            CetTACertViewModel cetTACertiViewModel = new CetTACertViewModel()
            {
                // Surveyor 이름과 번호
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            // For File Uploader in InsertModifyPopup
            ViewBag.CurrentIdx = Guid.NewGuid();
            ViewBag.RelatedIdx = Guid.NewGuid();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(cetTACertiViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirm(CetTACertViewModel cetTACertViewModel)
        {
            try
            {
                if (cetTACertViewModel is not null)
                {
                    Guid defaultId;
                    if (cetTACertViewModel.TA_CERT_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        defaultId = Guid.NewGuid();
                    }
                    else
                    {
                        defaultId = cetTACertViewModel.TA_CERT_IDX;
                    }

                    RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
                    TNAV_CET_TA_CERT tNAV_CET_TA_CERT = new TNAV_CET_TA_CERT()
                    {
                        TA_CERT_IDX = defaultId,
                        WORK_IDX = defaultId,
                        WORK_ID = cetTACertViewModel.WORK_ID,
                        CERT_NUMBER = cetTACertViewModel.CERT_NUMBER,
                        INITIAL_DATE = cetTACertViewModel.INITIAL_DATE,
                        DATE_OF_ISSUE = cetTACertViewModel.DATE_OF_ISSUE,
                        DATE_OF_EXPIRATION = cetTACertViewModel.DATE_OF_EXPIRATION,
                        DATE_OF_PLAN_AUDIT = cetTACertViewModel.DATE_OF_PLAN_AUDIT,
                        IS_MANUFACTURER = cetTACertViewModel.IS_MANUFACTURER,
                        IS_APPLICANT = cetTACertViewModel.IS_APPLICANT,
                        COMPANY = cetTACertViewModel.COMPANY,
                        PRODUCT_NAME = cetTACertViewModel.PRODUCT_NAME,
                        MANUFACTURER = cetTACertViewModel.MANUFACTURER,
                        APPLICANT = cetTACertViewModel.APPLICANT,
                        DESCRIPTION = cetTACertViewModel.DESCRIPTION,
                        APPLICATION_CONDITION = cetTACertViewModel.APPLICATION_CONDITION,
                        PRODUCT_SPEC = cetTACertViewModel.PRODUCT_SPEC,
                        APPROVED_DOCS = cetTACertViewModel.APPROVED_DOCS,
                        TEST_DOCS = cetTACertViewModel.TEST_DOCS,
                        APPLICATION_LIMITATION = cetTACertViewModel.APPLICATION_LIMITATION,
                        PRODUCT_CERT_DRAWING = cetTACertViewModel.PRODUCT_CERT_DRAWING,
                        MARKING = cetTACertViewModel.MARKING,
                        OTHERS = cetTACertViewModel.OTHERS,
                        SURVEYOR_NAME = user.USER_NAME,
                        SURVEYOR_NUMBER = user.SUR_NO,
                        APPROVER = cetTACertViewModel.APPROVER,
                        ANNEX_TO_CERT = cetTACertViewModel.ANNEX_TO_CERT,
                        IS_DELETE = false,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        REG_DATE = DateTime.Now,
                        DELETE_DATE = cetTACertViewModel.DELETE_DATE,
                        DELETE_USER = cetTACertViewModel.DELETE_USER
                    };

                    if (ModelState.IsValid)
                    {
                        _repository.Add(tNAV_CET_TA_CERT);
                        await _repository.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Detail), new { id = cetTACertViewModel.TA_CERT_IDX });
                //return RedirectToAction("Detail", "CetTACertificate");
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<IActionResult> Detail(Guid? id)
        {
            if (!_repository.TNAV_CET_TA_CERTs.Where(m => m.TA_CERT_IDX == id).Any())
            {
                return RedirectToAction("Create");
            }

            var dataCreated = await _repository.TNAV_CET_TA_CERTs.FirstOrDefaultAsync(m => m.TA_CERT_IDX == id);

            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            // Kind of Certification 값 가져오기
            List<ApprovalViewModel> _KindOfCertification = new List<ApprovalViewModel>();
            _KindOfCertification.Add(new ApprovalViewModel { Text = "Initial", Value = "Initial" });
            _KindOfCertification.Add(new ApprovalViewModel { Text = "Renewal", Value = "Renewal" });
            _KindOfCertification.Add(new ApprovalViewModel { Text = "Reissue", Value = "Reissue" });
            _KindOfCertification.Add(new ApprovalViewModel { Text = "Occasional", Value = "Occasional" });
            ViewBag.KindOfCertification = _KindOfCertification;

            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
            CetTACertViewModel cetTACertiViewModel = new CetTACertViewModel()
            {
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            // For File Uploader in InsertModifyPopup
            ViewBag.CurrentIdx = Guid.NewGuid();
            ViewBag.RelatedIdx = Guid.NewGuid();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(dataCreated);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CetTACertViewModel? cetTACertViewModel)
        {
            try
            {
                if (cetTACertViewModel is not null)
                {
                    Guid defaultDbIdx = cetTACertViewModel.TA_CERT_IDX;

                    TNAV_CET_TA_CERT tNAV_CET_TA_CERT = _repository.TNAV_CET_TA_CERTs.FirstOrDefault(m => m.TA_CERT_IDX == defaultDbIdx && m.IS_DELETE == false);

                    tNAV_CET_TA_CERT.TA_CERT_IDX = defaultDbIdx;
                    tNAV_CET_TA_CERT.WORK_IDX = defaultDbIdx;
                    tNAV_CET_TA_CERT.WORK_ID = cetTACertViewModel.WORK_ID;
                    tNAV_CET_TA_CERT.CERT_NUMBER = cetTACertViewModel.CERT_NUMBER;
                    tNAV_CET_TA_CERT.INITIAL_DATE = cetTACertViewModel.INITIAL_DATE;
                    tNAV_CET_TA_CERT.DATE_OF_ISSUE = cetTACertViewModel.DATE_OF_ISSUE;
                    tNAV_CET_TA_CERT.DATE_OF_EXPIRATION = cetTACertViewModel.DATE_OF_EXPIRATION;
                    tNAV_CET_TA_CERT.DATE_OF_PLAN_AUDIT = cetTACertViewModel.DATE_OF_PLAN_AUDIT;
                    tNAV_CET_TA_CERT.IS_MANUFACTURER = cetTACertViewModel.IS_MANUFACTURER;
                    tNAV_CET_TA_CERT.IS_APPLICANT = cetTACertViewModel.IS_APPLICANT;
                    tNAV_CET_TA_CERT.COMPANY = cetTACertViewModel.COMPANY;
                    tNAV_CET_TA_CERT.PRODUCT_NAME = cetTACertViewModel.PRODUCT_NAME;
                    tNAV_CET_TA_CERT.MANUFACTURER = cetTACertViewModel.MANUFACTURER;
                    tNAV_CET_TA_CERT.APPLICANT = cetTACertViewModel.APPLICANT;
                    tNAV_CET_TA_CERT.DESCRIPTION = cetTACertViewModel.DESCRIPTION;
                    tNAV_CET_TA_CERT.APPLICATION_CONDITION = cetTACertViewModel.APPLICATION_CONDITION;
                    tNAV_CET_TA_CERT.PRODUCT_SPEC = cetTACertViewModel.PRODUCT_SPEC;
                    tNAV_CET_TA_CERT.APPROVED_DOCS = cetTACertViewModel.APPROVED_DOCS;
                    tNAV_CET_TA_CERT.TEST_DOCS = cetTACertViewModel.TEST_DOCS;
                    tNAV_CET_TA_CERT.APPLICATION_LIMITATION = cetTACertViewModel.APPLICATION_LIMITATION;
                    tNAV_CET_TA_CERT.PRODUCT_CERT_DRAWING = cetTACertViewModel.PRODUCT_CERT_DRAWING;
                    tNAV_CET_TA_CERT.MARKING = cetTACertViewModel.MARKING;
                    tNAV_CET_TA_CERT.OTHERS = cetTACertViewModel.OTHERS;
                    tNAV_CET_TA_CERT.APPROVER = cetTACertViewModel.APPROVER;
                    tNAV_CET_TA_CERT.ANNEX_TO_CERT = cetTACertViewModel.ANNEX_TO_CERT;
                    tNAV_CET_TA_CERT.IS_DELETE = false;
                    tNAV_CET_TA_CERT.CREATE_USER = HttpContext.Session.GetString("UserName");
                    tNAV_CET_TA_CERT.REG_DATE = DateTime.Now;
                    tNAV_CET_TA_CERT.DELETE_DATE = cetTACertViewModel.DELETE_DATE;
                    tNAV_CET_TA_CERT.DELETE_USER = cetTACertViewModel.DELETE_USER;

                    if (ModelState.IsValid)
                    {
                        _repository.Update(tNAV_CET_TA_CERT);
                        await _repository.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Detail), new { id = cetTACertViewModel.TA_CERT_IDX });
                //return RedirectToAction("Detail", "CetTACertificate");
            }

            catch (Exception)
            {
                throw;
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AnnexToCertPopup(CetTACertViewModel? cetTACertViewModel)
        //{
        //    try
        //    {
        //        if (cetTACertViewModel is not null)
        //        {
        //            Guid defaultDbIdx;

        //            if (cetTACertViewModel.TA_CERT_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
        //            {
        //                defaultDbIdx = Guid.NewGuid();
        //            }
        //            else
        //            {
        //                defaultDbIdx = cetTACertViewModel.TA_CERT_IDX;
        //            }

        //            TNAV_CET_TA_CERT tNAV_CET_TA_CERT = new TNAV_CET_TA_CERT()
        //            {
        //                TA_CERT_IDX = defaultDbIdx,
        //                WORK_IDX = defaultDbIdx,
        //                WORK_ID = cetTACertViewModel.WORK_ID,
        //                ANNEX_TO_CERT = cetTACertViewModel.ANNEX_TO_CERT,
        //                IS_DELETE = false,
        //                CREATE_USER = HttpContext.Session.GetString("UserName"),
        //                REG_DATE = DateTime.Now,
        //                DELETE_DATE = cetTACertViewModel.DELETE_DATE,
        //                DELETE_USER = cetTACertViewModel.DELETE_USER
        //            };

        //            _repository.Add(tNAV_CET_TA_CERT);
        //            await _repository.SaveChangesAsync();
        //        }

        //        return RedirectToAction(nameof(Detail), new { id = cetTACertViewModel.TA_CERT_IDX });
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        
        [HttpPost]      
        public async Task<IActionResult> SaveAnnexToCert(CetTACertViewModel cetTACertViewModel)
        {
            Guid taCertAnnexIdx = Guid.NewGuid();
            DateTime _regDate = DateTime.Now;
            try
            {
                TNAV_CET_TA_CERT tNAV_CET_TA_CERT = new TNAV_CET_TA_CERT()
                {
                    TA_CERT_IDX = taCertAnnexIdx,
                    ANNEX_TO_CERT = cetTACertViewModel.ANNEX_TO_CERT,
                    CREATE_USER = cetTACertViewModel.CREATE_USER
                };

                _repository.Add(tNAV_CET_TA_CERT);
                await _repository.SaveChangesAsync();

                return Json(tNAV_CET_TA_CERT);
            }
            catch (Exception)
            {
                throw;
            }
            //return Json(new { message = "저장에성공하였습니다", result = "true" });
            //return Json(cetTaCertAnnexViewModel);

            // 저장 버튼 누를 때 마다 Guid가 부여됨 --> 수정할 것.
            // 
            /*
            //string result = string.Empty;
            string message = string.Empty;

            try
            {
            // 항상 null이 아니다
                if(cetTaCertAnnexViewModel is not null)
                {
                    Guid defaultDbIdx = Guid.NewGuid();
                    DateTime _regDate = DateTime.Now;

                    TNAV_CET_TA_CERT_ANNEX tNAV_CET_TA_CERT_ANNEX = new TNAV_CET_TA_CERT_ANNEX()
                    {
                        TA_CERT_ANNEX_IDX = defaultDbIdx,
                        WORK_IDX = defaultDbIdx,
                        ANNEX_TO_CERT = cetTaCertAnnexViewModel.ANNEX_TO_CERT,
                        IS_DELETE = false,
                        REG_DATE = _regDate,
                        DELETE_DATE = cetTaCertAnnexViewModel.DELETE_DATE
                    };
                
                    if(ModelState.IsValid)
                    {
                        _repository.Add(tNAV_CET_TA_CERT_ANNEX);
                        await _repository.SaveChangesAsync();
                        var result = tNAV_CET_TA_CERT_ANNEX.ANNEX_TO_CERT;
                        //result = "OK";
                        return Json(cetTaCertAnnexViewModel);
                    }                    
                }
            }

            catch (Exception)
            {
                throw;
            }
            return Json(new { message = ""});
            */
        }


        public async Task<IActionResult> InsertModifyPopup(Guid id, string platform)
        {
            ViewBag.dataSource = await _common.getCommonLogWithPlatformListAsync(id, platform);
            return View();
        }

        public async Task<IActionResult> SpecialCharPopup(Guid id, string platform)
        {
            ViewBag.dataSource = await _common.getCommonLogWithPlatformListAsync(id, platform);
            return View();
        }
    }
}
