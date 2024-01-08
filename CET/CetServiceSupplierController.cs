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
    public class CetServiceSupplierController : Controller
    {
        private readonly INavesPortalCommonService _common;
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemRepository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public CetServiceSupplierController(INavesPortalCommonService common, BM_NAVES_PortalContext repository, RfSystemContext rfSystemRepository, IBM_NAVES_PortalContextProcedures procedures)
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

                List<PNAV_CET_SERVICE_SUPPLIER_GET_LISTResult> resultList = await _procedures.PNAV_CET_SERVICE_SUPPLIER_GET_LISTAsync(SearchString, StartDate, EndDate);

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

                int count = DataSource.Cast<PNAV_CET_SERVICE_SUPPLIER_GET_LISTResult>().Count();

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
                return RedirectToAction("SaveException", "Error", new { ex = ex.InnerException.Message, returnController = "CetServiceSupplier", returnView = "Index" });
            }
        }

        public async Task<IActionResult> Create()
        {
            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
            CetServiceSupplierViewModel cetServiceViewModel = new CetServiceSupplierViewModel()
            {
                SURVEYOR_NAME = user.USER_NAME,
                SURVEYOR_NUMBER = user.SUR_NO
            };

            // For File Uploader in InsertModifyPopup
            ViewBag.CurrentIdx = Guid.NewGuid();
            ViewBag.RelatedIdx = Guid.NewGuid();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(cetServiceViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirm(CetServiceSupplierViewModel cetServiceSupplierViewModel)
        {
            try
            {
                if (cetServiceSupplierViewModel is not null)
                {
                    Guid defaultId;
                    if (cetServiceSupplierViewModel.SERVICE_SUPPLIER_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        defaultId = Guid.NewGuid();
                    }
                    else
                    {
                        defaultId = cetServiceSupplierViewModel.SERVICE_SUPPLIER_IDX;
                    }
                    RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
                    TNAV_CET_SERVICE_SUPPLIER tNAV_CET_SERVICE_SUPPLIER = new TNAV_CET_SERVICE_SUPPLIER()
                    {
                        SERVICE_SUPPLIER_IDX = defaultId,
                        WORK_IDX = defaultId,
                        WORK_ID = cetServiceSupplierViewModel.WORK_ID,
                        CERT_NUMBER = cetServiceSupplierViewModel.CERT_NUMBER ?? "",
                        DATE_OF_ISSUE = cetServiceSupplierViewModel.DATE_OF_ISSUE,
                        CERT_VALID_DATE = cetServiceSupplierViewModel.CERT_VALID_DATE,
                        COMPANY = cetServiceSupplierViewModel.COMPANY ?? "",
                        SCOPE_OF_SERVICE = cetServiceSupplierViewModel.SCOPE_OF_SERVICE ?? "",
                        COMPANY_NAME = cetServiceSupplierViewModel.COMPANY_NAME ?? "",
                        ADDRESS = cetServiceSupplierViewModel.ADDRESS ?? "",
                        SURVEYOR_NAME = user.USER_NAME,
                        SURVEYOR_NUMBER = user.SUR_NO,
                        APPROVER = cetServiceSupplierViewModel.APPROVER,
                        IS_DELETE = false,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        REG_DATE = DateTime.Now,
                        DELETE_DATE = cetServiceSupplierViewModel.DELETE_DATE,
                        DELETE_USER = cetServiceSupplierViewModel.DELETE_USER
                    };

                    if (ModelState.IsValid)
                    {
                        _repository.Add(tNAV_CET_SERVICE_SUPPLIER);
                        await _repository.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Detail), new { id = cetServiceSupplierViewModel.SERVICE_SUPPLIER_IDX });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> Detail(Guid? id)
        {
            if (!_repository.TNAV_CET_SERVICE_SUPPLIERs.Where(m => m.SERVICE_SUPPLIER_IDX == id).Any())
            {
                return RedirectToAction("Create");
            }

            var dataCreated = await _repository.TNAV_CET_SERVICE_SUPPLIERs.FirstOrDefaultAsync(m => m.SERVICE_SUPPLIER_IDX == id);

            List<PNAV_CET_GET_WORK_IDResult> WorkIdList = await _procedures.PNAV_CET_GET_WORK_IDAsync();
            ViewBag.WorkId = WorkIdList.AsEnumerable();

            List<PNAV_CET_GET_PMResult> ApproverList = await _procedures.PNAV_CET_GET_PMAsync();
            ViewBag.Approver = ApproverList.AsEnumerable();

            RFT_USER_DEPT user = _rfSystemRepository.RFT_USER_DEPTs.Where(m => m.USER_ID == HttpContext.Session.GetString("UserId")).First();
            CetServiceSupplierViewModel cetServiceViewModel = new CetServiceSupplierViewModel()
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
        public async Task<IActionResult> Edit(CetServiceSupplierViewModel? cetServiceSupplierViewModel)
        {
            try
            {
                if (cetServiceSupplierViewModel is not null)
                {
                    Guid defaultDbIdx = cetServiceSupplierViewModel.SERVICE_SUPPLIER_IDX;

                    TNAV_CET_SERVICE_SUPPLIER tNAV_CET_SERVICE_SUPPLIER = _repository.TNAV_CET_SERVICE_SUPPLIERs.FirstOrDefault(m => m.SERVICE_SUPPLIER_IDX == defaultDbIdx);

                    tNAV_CET_SERVICE_SUPPLIER.SERVICE_SUPPLIER_IDX = defaultDbIdx;
                    tNAV_CET_SERVICE_SUPPLIER.WORK_IDX = defaultDbIdx;
                    tNAV_CET_SERVICE_SUPPLIER.WORK_ID = cetServiceSupplierViewModel.WORK_ID;
                    tNAV_CET_SERVICE_SUPPLIER.CERT_NUMBER = cetServiceSupplierViewModel.CERT_NUMBER;
                    tNAV_CET_SERVICE_SUPPLIER.DATE_OF_ISSUE = cetServiceSupplierViewModel.DATE_OF_ISSUE;
                    tNAV_CET_SERVICE_SUPPLIER.CERT_VALID_DATE = cetServiceSupplierViewModel.CERT_VALID_DATE;
                    tNAV_CET_SERVICE_SUPPLIER.COMPANY = cetServiceSupplierViewModel.COMPANY;
                    tNAV_CET_SERVICE_SUPPLIER.SCOPE_OF_SERVICE = cetServiceSupplierViewModel.SCOPE_OF_SERVICE;
                    tNAV_CET_SERVICE_SUPPLIER.COMPANY_NAME = cetServiceSupplierViewModel.COMPANY_NAME;
                    tNAV_CET_SERVICE_SUPPLIER.ADDRESS = cetServiceSupplierViewModel.ADDRESS;
                    tNAV_CET_SERVICE_SUPPLIER.APPROVER = cetServiceSupplierViewModel.APPROVER;
                    tNAV_CET_SERVICE_SUPPLIER.IS_DELETE = false;
                    tNAV_CET_SERVICE_SUPPLIER.CREATE_USER = HttpContext.Session.GetString("UserName");
                    tNAV_CET_SERVICE_SUPPLIER.REG_DATE = DateTime.Now;
                    tNAV_CET_SERVICE_SUPPLIER.DELETE_DATE = cetServiceSupplierViewModel.DELETE_DATE;
                    tNAV_CET_SERVICE_SUPPLIER.DELETE_USER = cetServiceSupplierViewModel.DELETE_USER;

                    if (ModelState.IsValid)
                    {
                        _repository.Update(tNAV_CET_SERVICE_SUPPLIER);
                        await _repository.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Detail), new { id = cetServiceSupplierViewModel.SERVICE_SUPPLIER_IDX });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnnexToCertPopup(TNAV_CET_SERVICE_SUPPLIER tNAV_CET_SERVICE_SUPPLIER)
        {
            try
            {
                if (tNAV_CET_SERVICE_SUPPLIER is not null)
                {
                    Guid AnnexToCert_DefaultDbId;
                    DateTime _regTime = DateTime.Now;

                    if (tNAV_CET_SERVICE_SUPPLIER.SERVICE_SUPPLIER_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        AnnexToCert_DefaultDbId = Guid.NewGuid();
                    }
                    else
                    {
                        AnnexToCert_DefaultDbId = tNAV_CET_SERVICE_SUPPLIER.SERVICE_SUPPLIER_IDX;
                    }

                    TNAV_CET_SERVICE_SUPPLIER _tNAV_CET_SERVICE_SUPPLIER = new TNAV_CET_SERVICE_SUPPLIER()
                    {
                        SERVICE_SUPPLIER_IDX = AnnexToCert_DefaultDbId,
                        WORK_IDX = AnnexToCert_DefaultDbId,
                        WORK_ID = tNAV_CET_SERVICE_SUPPLIER.WORK_ID,
                        ANNEX_TO_CERT = tNAV_CET_SERVICE_SUPPLIER.ANNEX_TO_CERT,
                        IS_DELETE = false,
                        CREATE_USER = HttpContext.Session.GetString("UserName"),
                        REG_DATE = _regTime,
                        DELETE_DATE = tNAV_CET_SERVICE_SUPPLIER.DELETE_DATE,
                        DELETE_USER = tNAV_CET_SERVICE_SUPPLIER.DELETE_USER
                    };

                    _repository.Add(_tNAV_CET_SERVICE_SUPPLIER);
                    await _repository.SaveChangesAsync();
                }

                return RedirectToAction(nameof(InsertModifyPopup), new { id = tNAV_CET_SERVICE_SUPPLIER.WORK_IDX });
            }

            catch (Exception)
            {
                throw;
            }
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

        public async Task<IActionResult> ScopeOfServicePopup(Guid id, string platform)
        {
            ViewBag.dataSource = await _common.getCommonLogWithPlatformListAsync(id, platform);
            return View();
        }
    }
}
