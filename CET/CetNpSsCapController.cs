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
    public class CetNpSsCapController : Controller
    {
        private readonly INavesPortalCommonService _common;
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemRepository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public CetNpSsCapController(INavesPortalCommonService common, BM_NAVES_PortalContext repository, RfSystemContext rfSystemRepository, IBM_NAVES_PortalContextProcedures procedures)
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

                List<PNAV_CET_CAP_GET_LISTResult> resultList = await _procedures.PNAV_CET_CAP_GET_LISTAsync(SearchString, StartDate, EndDate);

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

                int count = DataSource.Cast<PNAV_CET_CAP_GET_LISTResult>().Count();

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
                return RedirectToAction("SaveException", "Error", new { ex = ex.InnerException.Message, returnController = "CetNpSsCap", returnView = "Index" });
            }
        }

        public async Task<IActionResult> Create(CetNpSsCapViewModel cetNpSsCapViewModel)
        {
            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            List<QuantityTypeViewModel> _quantityTypeViewModels = new List<QuantityTypeViewModel>();
            _quantityTypeViewModels.Add(new QuantityTypeViewModel { Text = "Set", Value = "Set" });
            _quantityTypeViewModels.Add(new QuantityTypeViewModel { Text = "Unit", Value = "Unit" });
            _quantityTypeViewModels.Add(new QuantityTypeViewModel { Text = "Pcs", Value = "Pcs" });
            _quantityTypeViewModels.Add(new QuantityTypeViewModel { Text = "EA", Value = "EA" });
            ViewBag.QuantityType = _quantityTypeViewModels;

            List<NumberLevelViewModel> _numberLevelViewModels = new List<NumberLevelViewModel>();
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "1", Value = "1" });
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "2", Value = "2" });
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "3", Value = "3" });
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "4", Value = "4" });
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "5", Value = "5" });
            ViewBag.SelectNumberLevel = _numberLevelViewModels;

            List<ContentsSelectViewModel> _headConSelectViewModels = new List<ContentsSelectViewModel>();
            _headConSelectViewModels.Add(new ContentsSelectViewModel { Text = "Naval", Value = "Naval" });
            _headConSelectViewModels.Add(new ContentsSelectViewModel { Text = "Submerged", Value = "Submerged" });
            ViewBag.HeadConSelect = _headConSelectViewModels;

            List<ContentsSelectViewModel> _interConSelectViewModels = new List<ContentsSelectViewModel>();
            _interConSelectViewModels.Add(new ContentsSelectViewModel { Text = "N-CAP", Value = "N-CAP" });
            _interConSelectViewModels.Add(new ContentsSelectViewModel { Text = "SS-CAP", Value = "SS-CAP" });
            ViewBag.InterConSelect = _interConSelectViewModels;

            List<ContentsSelectViewModel> _tailConSelectViewModels = new List<ContentsSelectViewModel>();
            _tailConSelectViewModels.Add(new ContentsSelectViewModel { Text = "Naval", Value = "Naval" });
            _tailConSelectViewModels.Add(new ContentsSelectViewModel { Text = "Submerged", Value = "Submerged" });
            ViewBag.TailConSelect = _tailConSelectViewModels;


            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();

            CetNpSsCapViewModel _cetNpSsCapViewModel = new CetNpSsCapViewModel()
            {
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            return View(cetNpSsCapViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirm(CetNpSsCapViewModel cetNpSsCapViewModel)
        {
            try
            {
                if (cetNpSsCapViewModel is not null)
                {
                    Guid defaultDbId;
                    DateTime _regTime = DateTime.Now;
                    if (cetNpSsCapViewModel.NP_SS_CAP_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        defaultDbId = Guid.NewGuid();
                    }
                    else
                    {
                        defaultDbId = cetNpSsCapViewModel.NP_SS_CAP_IDX;
                    }

                    RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();

                    TNAV_CET_NP_SS_CAP tNAV_CET_NP_SS_CAP = new TNAV_CET_NP_SS_CAP()
                    {
                        NP_SS_CAP_IDX = defaultDbId,
                        WORK_IDX = defaultDbId,
                        WORK_ID = cetNpSsCapViewModel.WORK_ID,
                        CERT_NUMBER = cetNpSsCapViewModel.CERT_NUMBER ?? "",
                        COMMENCEMENT_DATE = cetNpSsCapViewModel.COMMENCEMENT_DATE,
                        DATE_OF_ISSUE = cetNpSsCapViewModel.DATE_OF_ISSUE,
                        VESSEL_NAME = cetNpSsCapViewModel.VESSEL_NAME ?? "",
                        SHIP_NUMBER = cetNpSsCapViewModel.SHIP_NUMBER ?? "",
                        CLASS = cetNpSsCapViewModel.CLASS ?? "",
                        DISPLACEMENT_LETTER = cetNpSsCapViewModel.DISPLACEMENT_LETTER ?? "",
                        DISPLACEMENT_SELECT = cetNpSsCapViewModel.DISPLACEMENT_SELECT ?? "",
                        DISPLACEMENT_UNIT = cetNpSsCapViewModel.DISPLACEMENT_UNIT ?? "",
                        QUANTITY_NUMBER = cetNpSsCapViewModel.QUANTITY_NUMBER ?? "",
                        QUANTITY_UNIT_SELECT = cetNpSsCapViewModel.QUANTITY_UNIT_SELECT ?? "",
                        INSPECTION_PLACE = cetNpSsCapViewModel.INSPECTION_PLACE ?? "",
                        CHECK_FOR_HCAP = cetNpSsCapViewModel.CHECK_FOR_HCAP,
                        CHECK_FOR_MCAP = cetNpSsCapViewModel.CHECK_FOR_MCAP,
                        CHECK_FOR_ECAP = cetNpSsCapViewModel.CHECK_FOR_ECAP,
                        SELECT_LEVEL_FOR_HCAP = cetNpSsCapViewModel.SELECT_LEVEL_FOR_HCAP ?? "",
                        SELECT_LEVEL_FOR_MCAP = cetNpSsCapViewModel.SELECT_LEVEL_FOR_MCAP ?? "",
                        SELECT_LEVEL_FOR_ECAP = cetNpSsCapViewModel.SELECT_LEVEL_FOR_ECAP ?? "",
                        OVERALL_RATING_RESULT_A = cetNpSsCapViewModel.OVERALL_RATING_RESULT_A ?? "",
                        OVERALL_RATING_RESULT_B = cetNpSsCapViewModel.OVERALL_RATING_RESULT_B ?? "",
                        OVERALL_RATING_RESULT_C = cetNpSsCapViewModel.OVERALL_RATING_RESULT_C ?? "",
                        HEAD_CONTENTS_SELECT = cetNpSsCapViewModel.HEAD_CONTENTS_SELECT ?? "",
                        HEAD_CONTENTS_BLANK_A = cetNpSsCapViewModel.HEAD_CONTENTS_BLANK_A ?? "",
                        HEAD_CONTENTS_BLANK_B = cetNpSsCapViewModel.HEAD_CONTENTS_BLANK_B ?? "",
                        INTERMEDIATE_CONTENTS_SELECT = cetNpSsCapViewModel.INTERMEDIATE_CONTENTS_SELECT ?? "",
                        INTERMEDIATE_CONTENTS_BLANK = cetNpSsCapViewModel.INTERMEDIATE_CONTENTS_BLANK ?? "",
                        TAIL_CONTENTS_SELECT = cetNpSsCapViewModel.TAIL_CONTENTS_SELECT ?? "",
                        TAIL_CONTENTS_BLANK_A = cetNpSsCapViewModel.TAIL_CONTENTS_BLANK_A ?? "",
                        TAIL_CONTENTS_BLANK_B = cetNpSsCapViewModel.TAIL_CONTENTS_BLANK_B ?? "",
                        TAIL_CONTENTS_BLANK_C = cetNpSsCapViewModel.TAIL_CONTENTS_BLANK_C ?? "",
                        SURVEYOR_NAME = user.USER_NAME,
                        SURVEYOR_NUMBER = user.SUR_NO,
                        APPROVER = cetNpSsCapViewModel.APPROVER,
                        IS_DELETE = false,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        REG_DATE = DateTime.Now,
                        DELETE_DATE = cetNpSsCapViewModel.DELETE_DATE,
                        DELETE_USER = cetNpSsCapViewModel.DELETE_USER
                    };

                    if(ModelState.IsValid)
                    {
                        _repository.Add(tNAV_CET_NP_SS_CAP);
                        await _repository.SaveChangesAsync();
                    }               
                }
                return RedirectToAction(nameof(Detail), new { id = cetNpSsCapViewModel.NP_SS_CAP_IDX });
            }
            catch (Exception)
            {
                throw;
            }      
        }


        public async Task<IActionResult> Detail(Guid? id)
        {
            if(!_repository.TNAV_CET_NP_SS_CAPs.Where(m => m.NP_SS_CAP_IDX == id).Any())
            {
                return RedirectToAction("Create");
            }

            var dataCreated = await _repository.TNAV_CET_NP_SS_CAPs.FirstOrDefaultAsync(m => m.NP_SS_CAP_IDX == id);

            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            List<QuantityTypeViewModel> _quantityTypeViewModels = new List<QuantityTypeViewModel>();
            _quantityTypeViewModels.Add(new QuantityTypeViewModel { Text = "Set", Value = "Set" });
            _quantityTypeViewModels.Add(new QuantityTypeViewModel { Text = "Unit", Value = "Unit" });
            _quantityTypeViewModels.Add(new QuantityTypeViewModel { Text = "Pcs", Value = "Pcs" });
            _quantityTypeViewModels.Add(new QuantityTypeViewModel { Text = "EA", Value = "EA" });
            ViewBag.QuantityType = _quantityTypeViewModels;

            List<NumberLevelViewModel> _numberLevelViewModels = new List<NumberLevelViewModel>();
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "1", Value = "1" });
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "2", Value = "2" });
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "3", Value = "3" });
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "4", Value = "4" });
            _numberLevelViewModels.Add(new NumberLevelViewModel { Text = "5", Value = "5" });
            ViewBag.SelectNumberLevel = _numberLevelViewModels;

            List<ContentsSelectViewModel> _headConSelectViewModels = new List<ContentsSelectViewModel>();
            _headConSelectViewModels.Add(new ContentsSelectViewModel { Text = "Naval", Value = "Naval" });
            _headConSelectViewModels.Add(new ContentsSelectViewModel { Text = "Submerged", Value = "Submerged" });
            ViewBag.HeadConSelect = _headConSelectViewModels;

            List<ContentsSelectViewModel> _interConSelectViewModels = new List<ContentsSelectViewModel>();
            _interConSelectViewModels.Add(new ContentsSelectViewModel { Text = "N-CAP", Value = "N-CAP" });
            _interConSelectViewModels.Add(new ContentsSelectViewModel { Text = "SS-CAP", Value = "SS-CAP" });
            ViewBag.InterConSelect = _interConSelectViewModels;

            List<ContentsSelectViewModel> _tailConSelectViewModels = new List<ContentsSelectViewModel>();
            _tailConSelectViewModels.Add(new ContentsSelectViewModel { Text = "Naval", Value = "Naval" });
            _tailConSelectViewModels.Add(new ContentsSelectViewModel { Text = "Submerged", Value = "Submerged" });
            ViewBag.TailConSelect = _tailConSelectViewModels;


            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();

            CetNpSsCapViewModel _cetNpSsCapViewModel = new CetNpSsCapViewModel()
            {
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            return View(dataCreated);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CetNpSsCapViewModel? cetNpSsCapViewModel)
        {
            try
            {
                if(cetNpSsCapViewModel is not null)
                {
                    Guid defaultDbIdx = cetNpSsCapViewModel.NP_SS_CAP_IDX;

                    TNAV_CET_NP_SS_CAP tNAV_CET_NP_SS_CAP = _repository.TNAV_CET_NP_SS_CAPs.FirstOrDefault(m => m.NP_SS_CAP_IDX == defaultDbIdx && m.IS_DELETE == false);

                    tNAV_CET_NP_SS_CAP.NP_SS_CAP_IDX = defaultDbIdx;
                    tNAV_CET_NP_SS_CAP.WORK_IDX = defaultDbIdx;
                    tNAV_CET_NP_SS_CAP.WORK_ID = cetNpSsCapViewModel.WORK_ID;
                    tNAV_CET_NP_SS_CAP.CERT_NUMBER = cetNpSsCapViewModel.CERT_NUMBER;
                    tNAV_CET_NP_SS_CAP.COMMENCEMENT_DATE = cetNpSsCapViewModel.COMMENCEMENT_DATE;
                    tNAV_CET_NP_SS_CAP.DATE_OF_ISSUE = cetNpSsCapViewModel.DATE_OF_ISSUE;
                    tNAV_CET_NP_SS_CAP.VESSEL_NAME = cetNpSsCapViewModel.VESSEL_NAME;
                    tNAV_CET_NP_SS_CAP.SHIP_NUMBER = cetNpSsCapViewModel.SHIP_NUMBER;
                    tNAV_CET_NP_SS_CAP.CLASS = cetNpSsCapViewModel.CLASS;
                    tNAV_CET_NP_SS_CAP.DISPLACEMENT_LETTER = cetNpSsCapViewModel.DISPLACEMENT_LETTER;
                    tNAV_CET_NP_SS_CAP.DISPLACEMENT_SELECT = cetNpSsCapViewModel.DISPLACEMENT_SELECT;
                    tNAV_CET_NP_SS_CAP.DISPLACEMENT_UNIT = cetNpSsCapViewModel.DISPLACEMENT_UNIT;
                    tNAV_CET_NP_SS_CAP.QUANTITY_NUMBER = cetNpSsCapViewModel.QUANTITY_NUMBER;
                    tNAV_CET_NP_SS_CAP.QUANTITY_UNIT_SELECT = cetNpSsCapViewModel.QUANTITY_UNIT_SELECT;
                    tNAV_CET_NP_SS_CAP.INSPECTION_PLACE = cetNpSsCapViewModel.INSPECTION_PLACE;
                    tNAV_CET_NP_SS_CAP.CHECK_FOR_HCAP = cetNpSsCapViewModel.CHECK_FOR_HCAP;
                    tNAV_CET_NP_SS_CAP.CHECK_FOR_MCAP = cetNpSsCapViewModel.CHECK_FOR_MCAP;
                    tNAV_CET_NP_SS_CAP.CHECK_FOR_ECAP = cetNpSsCapViewModel.CHECK_FOR_ECAP;
                    tNAV_CET_NP_SS_CAP.SELECT_LEVEL_FOR_HCAP = cetNpSsCapViewModel.SELECT_LEVEL_FOR_HCAP;
                    tNAV_CET_NP_SS_CAP.SELECT_LEVEL_FOR_MCAP = cetNpSsCapViewModel.SELECT_LEVEL_FOR_MCAP;
                    tNAV_CET_NP_SS_CAP.SELECT_LEVEL_FOR_ECAP = cetNpSsCapViewModel.SELECT_LEVEL_FOR_ECAP;
                    tNAV_CET_NP_SS_CAP.OVERALL_RATING_RESULT_A = cetNpSsCapViewModel.OVERALL_RATING_RESULT_A;
                    tNAV_CET_NP_SS_CAP.OVERALL_RATING_RESULT_B = cetNpSsCapViewModel.OVERALL_RATING_RESULT_B;
                    tNAV_CET_NP_SS_CAP.OVERALL_RATING_RESULT_C = cetNpSsCapViewModel.OVERALL_RATING_RESULT_C;
                    tNAV_CET_NP_SS_CAP.HEAD_CONTENTS_SELECT = cetNpSsCapViewModel.HEAD_CONTENTS_SELECT;
                    tNAV_CET_NP_SS_CAP.HEAD_CONTENTS_BLANK_A = cetNpSsCapViewModel.HEAD_CONTENTS_BLANK_A;
                    tNAV_CET_NP_SS_CAP.HEAD_CONTENTS_BLANK_B = cetNpSsCapViewModel.HEAD_CONTENTS_BLANK_B;
                    tNAV_CET_NP_SS_CAP.INTERMEDIATE_CONTENTS_SELECT = cetNpSsCapViewModel.INTERMEDIATE_CONTENTS_SELECT;
                    tNAV_CET_NP_SS_CAP.INTERMEDIATE_CONTENTS_BLANK = cetNpSsCapViewModel.INTERMEDIATE_CONTENTS_BLANK;
                    tNAV_CET_NP_SS_CAP.TAIL_CONTENTS_SELECT = cetNpSsCapViewModel.TAIL_CONTENTS_SELECT;
                    tNAV_CET_NP_SS_CAP.TAIL_CONTENTS_BLANK_A = cetNpSsCapViewModel.TAIL_CONTENTS_BLANK_A;
                    tNAV_CET_NP_SS_CAP.TAIL_CONTENTS_BLANK_B = cetNpSsCapViewModel.TAIL_CONTENTS_BLANK_B;
                    tNAV_CET_NP_SS_CAP.TAIL_CONTENTS_BLANK_C = cetNpSsCapViewModel.TAIL_CONTENTS_BLANK_C;               
                    tNAV_CET_NP_SS_CAP.APPROVER = cetNpSsCapViewModel.APPROVER;
                    tNAV_CET_NP_SS_CAP.IS_DELETE = false;
                    tNAV_CET_NP_SS_CAP.CREATE_USER = HttpContext.Session.GetString("UserName");
                    tNAV_CET_NP_SS_CAP.REG_DATE = DateTime.Now;
                    tNAV_CET_NP_SS_CAP.DELETE_DATE = cetNpSsCapViewModel.DELETE_DATE;
                    tNAV_CET_NP_SS_CAP.DELETE_USER = cetNpSsCapViewModel.DELETE_USER;

                    if(ModelState.IsValid)
                    {
                        _repository.Update(tNAV_CET_NP_SS_CAP);
                        await _repository.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Detail), new { id = cetNpSsCapViewModel.NP_SS_CAP_IDX });
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<IActionResult> AnnextToCertPopup(TNAV_CET_NP_SS_CAP tNAV_CET_NP_SS_CAP)
        {
            try
            {
                Guid AnnexToCert_DefaultDbId;
                DateTime _regTime = DateTime.Now;
                if (tNAV_CET_NP_SS_CAP is not null)
                {
                    if (tNAV_CET_NP_SS_CAP.NP_SS_CAP_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        AnnexToCert_DefaultDbId = Guid.NewGuid();
                    }
                    else
                    {
                        AnnexToCert_DefaultDbId = tNAV_CET_NP_SS_CAP.NP_SS_CAP_IDX;
                    }

                    TNAV_CET_NP_SS_CAP _tNAV_CET_NP_SS_CAP = new TNAV_CET_NP_SS_CAP()
                    {
                        NP_SS_CAP_IDX = AnnexToCert_DefaultDbId,
                        WORK_IDX = AnnexToCert_DefaultDbId,
                        WORK_ID = tNAV_CET_NP_SS_CAP.WORK_ID,
                        ANNEX_TO_CERT = tNAV_CET_NP_SS_CAP.ANNEX_TO_CERT,
                        IS_DELETE = false,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        REG_DATE = _regTime,
                        DELETE_DATE = tNAV_CET_NP_SS_CAP.DELETE_DATE,
                        DELETE_USER = tNAV_CET_NP_SS_CAP.DELETE_USER
                    };

                    _repository.Add(_tNAV_CET_NP_SS_CAP);
                    await _repository.SaveChangesAsync();
                }

                return RedirectToAction(nameof(InsertModifyPopup), new { id = tNAV_CET_NP_SS_CAP.WORK_IDX });
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
