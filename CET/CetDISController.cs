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

namespace NavesPortalforWebWithCoreMvc.Controllers.CET
{
    public class CetDISController : Controller
    {
        private readonly INavesPortalCommonService _common;
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemRepository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public CetDISController(INavesPortalCommonService common, BM_NAVES_PortalContext repository, RfSystemContext rfSystemRepository, IBM_NAVES_PortalContextProcedures procedures)
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

                List<PNAV_CET_DIS_GET_LISTResult> resultList = await _procedures.PNAV_CET_DIS_GET_LISTAsync(SearchString, StartDate, EndDate);

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

                int count = DataSource.Cast<PNAV_CET_DIS_GET_LISTResult>().Count();

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
                return RedirectToAction("SaveException", "Error", new { ex = ex.InnerException.Message, returnController = "CetDIS", returnView = "Index" });
            }
        }


        public async Task<IActionResult> Create()
        {
            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();

            CetDISViewModel cetDISViewModel = new CetDISViewModel()
            {
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            List<QuantityTypeViewModel> _QuantityType = new List<QuantityTypeViewModel>();
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Set", Value = "Set" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Unit", Value = "Unit" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Pcs", Value = "Pcs" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "EA", Value = "EA" });
            ViewBag.QuantityType = _QuantityType;

            List<DropDownListViewModel> _DropDownList = new List<DropDownListViewModel>();
            _DropDownList.Add(new DropDownListViewModel { Text = "Republic of Korea Navy", Value = "Republic of Korea Navy" });
            _DropDownList.Add(new DropDownListViewModel { Text = "Republic of Korea Air Force", Value = "Republic of Korea Air Force" });
            ViewBag.DropDownForIntended = _DropDownList;

            List<ApplicableRulesViewModel> _ApplicableRules = new List<ApplicableRulesViewModel>();
            _ApplicableRules.Add(new ApplicableRulesViewModel { Text = "KR Rules for the Classification of Steel Ships", Value = "KR Rules for the Classification of Steel Ships" });
            _ApplicableRules.Add(new ApplicableRulesViewModel { Text = "US Driving Manual Rev.7", Value = "US Driving Manual Rev.7" });
            ViewBag.ApplicableRules = _ApplicableRules;

            List<KRPunchViewModel> _KRPunch = new List<KRPunchViewModel>();
            _KRPunch.Add(new KRPunchViewModel { Text = "KRX", Value = "KRX" });
            _KRPunch.Add(new KRPunchViewModel { Text = "KR", Value = "KR" });
            _KRPunch.Add(new KRPunchViewModel { Text = "N/A", Value = "N/A" });
            ViewBag.KRPunch = _KRPunch;

            return View(cetDISViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirm(CetDISViewModel cetDISViewModel)
        {
            try
            {
                if (cetDISViewModel is not null)
                {
                    Guid defaultDbIdx;

                    if (cetDISViewModel.DIS_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        defaultDbIdx = Guid.NewGuid();
                    }
                    else
                    {
                        defaultDbIdx = cetDISViewModel.DIS_IDX;
                    }

                    RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
                    TNAV_CET_DI tNAV_CET_DIS = new TNAV_CET_DI()
                    {
                        DIS_IDX = defaultDbIdx,
                        WORK_IDX = defaultDbIdx,
                        WORK_ID = cetDISViewModel.WORK_ID ?? "",
                        CERT_NUMBER = cetDISViewModel.CERT_NUMBER ?? "",
                        COMMENCEMENT_DATE = cetDISViewModel.COMMENCEMENT_DATE,
                        DATE_OF_ISSUE = cetDISViewModel.DATE_OF_ISSUE,
                        QUANTITY_NUMBER = cetDISViewModel.QUANTITY_NUMBER ?? "",
                        QUANTITY_SELECT = cetDISViewModel.QUANTITY_SELECT ?? "",
                        INSPECTION_PLACE = cetDISViewModel.INSPECTION_PLACE ?? "",
                        FLAG = cetDISViewModel.FLAG ?? "",
                        ISSUE_PLACE = cetDISViewModel.ISSUE_PLACE ?? "",
                        COMPANY = cetDISViewModel.COMPANY ?? "",
                        APPLICABLE_RULES = cetDISViewModel.APPLICABLE_RULES ?? "",
                        INTENDED_FOR = cetDISViewModel.INTENDED_FOR ?? "",
                        APPROVAL_STATUS = cetDISViewModel.APPROVAL_STATUS ?? "",
                        DESCRIPTION = cetDISViewModel.DESCRIPTION ?? "",
                        PARTICULARS_AREA_A = cetDISViewModel.PARTICULARS_AREA_A ?? "",
                        PARTICULARS_AREA_B = cetDISViewModel.PARTICULARS_AREA_B ?? "",
                        TESTING_INSPECTION = cetDISViewModel.TESTING_INSPECTION ?? "",
                        KR_PUNCH_SELECT = cetDISViewModel.KR_PUNCH_SELECT ?? "",
                        MARKING_SN_RM_TEXT = cetDISViewModel.MARKING_SN_RM_TEXT ?? "",
                        IS_REISSUED = cetDISViewModel.IS_REISSUED,
                        SURVEYOR_NAME = user.USER_NAME,
                        SURVEYOR_NUMBER = user.SUR_NO,
                        APPROVER = cetDISViewModel.APPROVER,
                        IS_DELETE = false,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        REG_DATE = DateTime.Now,
                        DELETE_DATE = cetDISViewModel.DELETE_DATE,
                        DELETE_USER = cetDISViewModel.DELETE_USER
                    };

                    _repository.Add(tNAV_CET_DIS);
                    await _repository.SaveChangesAsync();
                    //if (ModelState.IsValid)
                    //{

                    //}
                }
                return RedirectToAction(nameof(Detail), new { id = cetDISViewModel.DIS_IDX });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> Detail(Guid? id)
        {
           if(!_repository.TNAV_CET_DIs.Where(m => m.DIS_IDX == id).Any())
            {
                return RedirectToAction("Create");
            }

            var dataCreated = await _repository.TNAV_CET_DIs.FirstOrDefaultAsync(m => m.DIS_IDX == id);

            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
            CetDISViewModel cetDISViewModel = new CetDISViewModel()
            {
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            List<QuantityTypeViewModel> _QuantityType = new List<QuantityTypeViewModel>();
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Set", Value = "Set" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Unit", Value = "Unit" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Pcs", Value = "Pcs" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "EA", Value = "EA" });
            ViewBag.QuantityType = _QuantityType;

            List<DropDownListViewModel> _DropDownList = new List<DropDownListViewModel>();
            _DropDownList.Add(new DropDownListViewModel { Text = "Republic of Korea Navy", Value = "Republic of Korea Navy" });
            _DropDownList.Add(new DropDownListViewModel { Text = "Republic of Korea Air Force", Value = "Republic of Korea Air Force" });
            ViewBag.DropDownForIntended = _DropDownList;

            List<KRPunchViewModel> _KRPunch = new List<KRPunchViewModel>();
            _KRPunch.Add(new KRPunchViewModel { Text = "KRX", Value = "KRX" });
            _KRPunch.Add(new KRPunchViewModel { Text = "KR", Value = "KR" });
            _KRPunch.Add(new KRPunchViewModel { Text = "N/A", Value = "N/A" });
            ViewBag.KRPunch = _KRPunch;

            return View(dataCreated);
        }

        public async Task<IActionResult> Edit(CetDISViewModel? cetDISViewModel)
        {
            try
            {
                if(cetDISViewModel is not null)
                {
                    Guid defaultDbIdx = cetDISViewModel.DIS_IDX;
                    TNAV_CET_DI tNAV_CET_DI = _repository.TNAV_CET_DIs.FirstOrDefault(m => m.DIS_IDX == defaultDbIdx && m.IS_DELETE == false);

                    tNAV_CET_DI.DIS_IDX = defaultDbIdx;
                    tNAV_CET_DI.WORK_IDX = defaultDbIdx;
                    tNAV_CET_DI.WORK_ID = cetDISViewModel.WORK_ID;
                    tNAV_CET_DI.CERT_NUMBER = cetDISViewModel.CERT_NUMBER;
                    tNAV_CET_DI.COMMENCEMENT_DATE = cetDISViewModel.COMMENCEMENT_DATE;
                    tNAV_CET_DI.DATE_OF_ISSUE = cetDISViewModel.DATE_OF_ISSUE;
                    tNAV_CET_DI.QUANTITY_NUMBER = cetDISViewModel.QUANTITY_NUMBER;
                    tNAV_CET_DI.QUANTITY_SELECT = cetDISViewModel.QUANTITY_SELECT;
                    tNAV_CET_DI.INSPECTION_PLACE = cetDISViewModel.INSPECTION_PLACE;
                    tNAV_CET_DI.FLAG = cetDISViewModel.FLAG;
                    tNAV_CET_DI.ISSUE_PLACE = cetDISViewModel.ISSUE_PLACE;
                    tNAV_CET_DI.COMPANY = cetDISViewModel.COMPANY;
                    tNAV_CET_DI.APPLICABLE_RULES = cetDISViewModel.APPLICABLE_RULES;
                    tNAV_CET_DI.INTENDED_FOR = cetDISViewModel.INTENDED_FOR;
                    tNAV_CET_DI.APPROVAL_STATUS = cetDISViewModel.APPROVAL_STATUS;
                    tNAV_CET_DI.DESCRIPTION = cetDISViewModel.DESCRIPTION;
                    tNAV_CET_DI.PARTICULARS_AREA_A = cetDISViewModel.PARTICULARS_AREA_A;
                    tNAV_CET_DI.PARTICULARS_AREA_B = cetDISViewModel.PARTICULARS_AREA_B;
                    tNAV_CET_DI.TESTING_INSPECTION = cetDISViewModel.TESTING_INSPECTION;
                    tNAV_CET_DI.KR_PUNCH_SELECT = cetDISViewModel.KR_PUNCH_SELECT;
                    tNAV_CET_DI.MARKING_SN_RM_TEXT = cetDISViewModel.MARKING_SN_RM_TEXT;
                    tNAV_CET_DI.IS_REISSUED = cetDISViewModel.IS_REISSUED;
                    tNAV_CET_DI.APPROVER = cetDISViewModel.APPROVER;
                    tNAV_CET_DI.IS_DELETE = false;
                    tNAV_CET_DI.CREATE_USER = HttpContext.Session.GetString("UserName");
                    tNAV_CET_DI.REG_DATE = DateTime.Now;
                    tNAV_CET_DI.DELETE_DATE = cetDISViewModel.DELETE_DATE;
                    tNAV_CET_DI.DELETE_USER = cetDISViewModel.DELETE_USER;

                    if(ModelState.IsValid)
                    {
                        _repository.Update(tNAV_CET_DI);
                        await _repository.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Detail), new { id = cetDISViewModel.DIS_IDX });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> AnnexToCertPopup(TNAV_CET_DI tNAV_CET_DIS)
        {
            try
            {
                Guid AnnexToCert_DefaultDbId;
                DateTime _regTime = DateTime.Now;

                if (tNAV_CET_DIS is not null)
                {
                    if (tNAV_CET_DIS.DIS_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        AnnexToCert_DefaultDbId = Guid.NewGuid();
                    }
                    else
                    {
                        AnnexToCert_DefaultDbId = tNAV_CET_DIS.DIS_IDX;
                    }

                    TNAV_CET_DI _tNAV_CET_DIS = new TNAV_CET_DI()
                    {
                        DIS_IDX = AnnexToCert_DefaultDbId,
                        WORK_IDX = AnnexToCert_DefaultDbId,
                        WORK_ID = tNAV_CET_DIS.WORK_ID,
                        ANNEX_TO_CERT = tNAV_CET_DIS.ANNEX_TO_CERT,
                        IS_DELETE = false,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        REG_DATE = _regTime,
                        DELETE_DATE = tNAV_CET_DIS.DELETE_DATE,
                        DELETE_USER = tNAV_CET_DIS.DELETE_USER
                    };

                    _repository.Add(_tNAV_CET_DIS);
                    await _repository.SaveChangesAsync();
                }

                return RedirectToAction(nameof(InsertModifyPopup), new { id = tNAV_CET_DIS.WORK_IDX });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> SpecialCharPopup(Guid id, string platform)
        {
            ViewBag.dataSource = await _common.getCommonLogWithPlatformListAsync(id, platform);
            return View();
        }

        public async Task<IActionResult> InsertModifyPopup(Guid id, string platform)
        {
            ViewBag.dataSource = await _common.getCommonLogWithPlatformListAsync(id, platform);
            return View();
        }
    }
}

