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
    public class CetNBMController : Controller
    {
        private readonly INavesPortalCommonService _common;
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemRepository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public CetNBMController(INavesPortalCommonService common, BM_NAVES_PortalContext repository, RfSystemContext rfSystemRepository, IBM_NAVES_PortalContextProcedures procedures)
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

                List<PNAV_CET_NBM_GET_LISTResult> resultList = await _procedures.PNAV_CET_NBM_GET_LISTAsync(SearchString, StartDate, EndDate);

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

                int count = DataSource.Cast<PNAV_CET_NBM_GET_LISTResult>().Count();

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
                return RedirectToAction("SaveException", "Error", new { ex = ex.InnerException.Message, returnController = "CetNBM", returnView = "Index" });
            }
        }

        public async Task<IActionResult> Create()
        {
            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
            CetNBMViewModel cetNBMViewModel = new CetNBMViewModel()
            {
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            List<dropdownViewModel> TonnageDropdown = new List<dropdownViewModel>();
            {
                TonnageDropdown.Add(new dropdownViewModel { Name = "FLDT", Value = "FLDT" });
                TonnageDropdown.Add(new dropdownViewModel { Name = "DT", Value = "DT" });
                TonnageDropdown.Add(new dropdownViewModel { Name = "LT", Value = "LT" });
                TonnageDropdown.Add(new dropdownViewModel { Name = "Text", Value = "Text" });
            };
            ViewBag.TonnageDropdownList = TonnageDropdown;

            List<dropdownViewModel> NotaMarkList = new List<dropdownViewModel>();
            {
                NotaMarkList.Add(new dropdownViewModel { Name = "Notation", Value = "Notation" });
                NotaMarkList.Add(new dropdownViewModel { Name = "Marking", Value = "Marking" });
                NotaMarkList.Add(new dropdownViewModel { Name = "Nill", Value = "Nill" });
            }
            ViewBag.NotaMarkSelect = NotaMarkList;

            return View(cetNBMViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirm(CetNBMViewModel cetNBMViewModel)
        {
            try
            {
                if (cetNBMViewModel is not null)
                {
                    Guid defaultDbIdx;
                    if (cetNBMViewModel.NBM_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        defaultDbIdx = Guid.NewGuid();
                    }
                    else
                    {
                        defaultDbIdx = cetNBMViewModel.NBM_IDX;
                    }

                    RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
                    TNAV_CET_NBM _tNAV_CET_NBM = new TNAV_CET_NBM()
                    {
                        NBM_IDX = defaultDbIdx,
                        WORK_IDX = defaultDbIdx,
                        WORK_ID = cetNBMViewModel.WORK_ID,
                        CERT_NUMBER = cetNBMViewModel.CERT_NUMBER ?? "",
                        DATE_OF_ISSUE = cetNBMViewModel.DATE_OF_ISSUE,
                        DATE_OF_BUILD = cetNBMViewModel.DATE_OF_BUILD,
                        PLACE_OF_BUILD = cetNBMViewModel.PLACE_OF_BUILD ?? "",
                        TONNAGE_LETTER = cetNBMViewModel.TONNAGE_LETTER ?? "",
                        APPLICABLE_RULES = cetNBMViewModel.APPLICABLE_RULES ?? "",
                        NOTATION_MARKING_SELECT = cetNBMViewModel.NOTATION_MARKING_SELECT ?? "",
                        NOTATION_MARKING_TEXT = cetNBMViewModel.NOTATION_MARKING_TEXT ?? "",
                        SURVEYOR_NAME = user.USER_NAME,
                        SURVEYOR_NUMBER = user.SUR_NO,
                        APPROVER = cetNBMViewModel.APPROVER,
                        IS_DELETE = false,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        REG_DATE = DateTime.Now,
                        DELETE_DATE = cetNBMViewModel.DELETE_DATE,
                        DELETE_USER = cetNBMViewModel.DELETE_USER
                    };

                    if(ModelState.IsValid)
                    {
                        _repository.Add(_tNAV_CET_NBM);
                        await _repository.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Create), new { id = cetNBMViewModel.NBM_IDX });
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<IActionResult> Detail(Guid? id)
        {
            if(!_repository.TNAV_CET_NBMs.Where(m => m.NBM_IDX == id).Any())
            {
                return RedirectToAction("Create");
            }

            var dataCreated = await _repository.TNAV_CET_NBMs.FirstOrDefaultAsync(m => m.NBM_IDX == id);

            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
            CetTACertViewModel cetTACertiViewModel = new CetTACertViewModel()
            {
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            return View(dataCreated);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CetNBMViewModel? cetNBMViewModel)
        {
            try
            {
                if(cetNBMViewModel is not null)
                {
                    Guid defaultDbIdx = cetNBMViewModel.NBM_IDX;

                    TNAV_CET_NBM tNAV_CET_NBM = _repository.TNAV_CET_NBMs.FirstOrDefault(m => m.NBM_IDX == defaultDbIdx && m.IS_DELETE == false);

                    tNAV_CET_NBM.NBM_IDX = defaultDbIdx;
                    tNAV_CET_NBM.WORK_IDX = defaultDbIdx;
                    tNAV_CET_NBM.WORK_ID = cetNBMViewModel.WORK_ID;
                    tNAV_CET_NBM.CERT_NUMBER = cetNBMViewModel.CERT_NUMBER;
                    tNAV_CET_NBM.DATE_OF_ISSUE = cetNBMViewModel.DATE_OF_ISSUE;
                    tNAV_CET_NBM.DATE_OF_BUILD = cetNBMViewModel.DATE_OF_BUILD;
                    tNAV_CET_NBM.PLACE_OF_BUILD = cetNBMViewModel.PLACE_OF_BUILD;
                    tNAV_CET_NBM.TONNAGE_LETTER = cetNBMViewModel.TONNAGE_LETTER;
                    tNAV_CET_NBM.APPLICABLE_RULES = cetNBMViewModel.APPLICABLE_RULES;
                    tNAV_CET_NBM.NOTATION_MARKING_SELECT = cetNBMViewModel.NOTATION_MARKING_SELECT;
                    tNAV_CET_NBM.NOTATION_MARKING_TEXT = cetNBMViewModel.NOTATION_MARKING_TEXT;
                    tNAV_CET_NBM.APPROVER = cetNBMViewModel.APPROVER;
                    tNAV_CET_NBM.IS_DELETE = false;
                    tNAV_CET_NBM.CREATE_USER = HttpContext.Session.GetString("UserName");
                    tNAV_CET_NBM.REG_DATE = DateTime.Now;
                    tNAV_CET_NBM.DELETE_DATE = cetNBMViewModel.DELETE_DATE;
                    tNAV_CET_NBM.DELETE_USER = cetNBMViewModel.DELETE_USER;

                    if (ModelState.IsValid)
                    {
                        _repository.Update(tNAV_CET_NBM);
                        await _repository.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Detail), new { id = cetNBMViewModel.NBM_IDX });
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
    }
}
