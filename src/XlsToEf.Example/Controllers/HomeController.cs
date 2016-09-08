using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XlsToEf.Example.ExampleBaseClassIdField;
using XlsToEf.Example.ExampleCustomMapperField;
using XlsToEf.Example.ExampleCustomMapperField.ProductCategoryFiles;
using XlsToEf.Example.SheetGetterExample;
using XlsToEf.Import;

namespace XlsToEf.Example.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ImportModal()
        {
            var importInfoModel = new ImportViewInfo
            {
                UploadUrl = Url.Action("UploadXlsx"),
                UploadTitle = "Import",
                ModalId = "modal"
            };
            return View("ImportModal", importInfoModel);
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<ActionResult> UploadXlsx(IFormFile uploadFile)
        {
            if (uploadFile == null || uploadFile.Length <= 0)
            {
                throw new HttpResponseException(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = "ERROR: No file found",
                });
            }
            var sheetInfo = await _mediator.SendAsync(new SaveAndGetSheetsForFileUpload {File = uploadFile.OpenReadStream()});


            sheetInfo.Destinations = new List<UploadDestinationInformation>
            {
                new UploadDestinationInformation
                {
                    Name = "Product",
                    SelectSheetUrl = Url.Action("SelectSheetAndDestinationForProduct"),
                    MatchSubmitUrl = Url.Action("SubmitProductColumnMatches"),
                },
                 new UploadDestinationInformation
                {
                    Name = "Product Category",
                    SelectSheetUrl = Url.Action("SelectSheetAndDestinationForProductCategory"),
                    MatchSubmitUrl = Url.Action("SubmitProductCategoryColumnMatches"),
                },
                new UploadDestinationInformation
                {
                    Name = "Order",
                    SelectSheetUrl = Url.Action("SelectSheetAndDestinationForOrder"),
                    MatchSubmitUrl = Url.Action("SubmitOrderColumnMatches"),
                }
            };


            return Json(sheetInfo);

        }

        public async Task<ActionResult> SelectSheetAndDestinationForProduct([FromBody]XlsProductColumnMatcherQuery selectedInfo)
        {
            try
            {
                var data = await _mediator.SendAsync(selectedInfo);
                return Json(data);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = "ERROR:" + ex.Message.ToString(),
                });
            }
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<ActionResult> SubmitProductColumnMatches([FromBody]ImportMatchingProductData data)
        {
            var c = new DbContext("XlsToEf");
            var result = await _mediator.SendAsync(data);
            return Json(result);
        }
        public async Task<ActionResult> SelectSheetAndDestinationForProductCategory([FromBody]XlsxProductCategoryColumnMatcherQuery selectedInfo)
        {
            try
            {
                var data = await _mediator.SendAsync(selectedInfo);
                return Json(data);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = "ERROR:" + ex.Message.ToString(),
                });
            }
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<ActionResult> SubmitProductCategoryColumnMatches([FromBody]ImportMatchingProductCategoryData data)
        {
            var c = new DbContext("XlsToEf");
            var result = await _mediator.SendAsync(data);
            return Json(result);
        }

        public async Task<ActionResult> SelectSheetAndDestinationForOrder([FromBody]XlsxOrderColumnMatcherQuery selectedInfo)
        {
            try
            {
                var data = await _mediator.SendAsync(selectedInfo);
                return Json(data);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = "ERROR:" + ex.Message.ToString(),
                });
            }
        }

        public async Task<JsonResult> SubmitOrderColumnMatches([FromBody]ImportMatchingOrderData data)
        {
            var result = await _mediator.SendAsync(data);
            return Json(result);
        }
    }
}