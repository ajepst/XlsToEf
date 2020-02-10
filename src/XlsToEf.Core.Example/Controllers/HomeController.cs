using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XlsToEf.Core.Example.ExampleBaseClassIdField;
using XlsToEf.Core.Example.ExampleCustomMapperField;
using XlsToEf.Core.Example.ExampleCustomMapperField.ProductCategoryFiles;
using XlsToEf.Core.Example.SheetGetterExample;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example.Controllers
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

        [HttpPost]
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
            var sheetInfo = await _mediator.Send(new SaveAndGetSheetsForFileUpload { File = uploadFile.OpenReadStream() });


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
                var data = await _mediator.Send(selectedInfo);
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

        [HttpPost]
        public async Task<ActionResult> SubmitProductColumnMatches([FromBody]DataMatchesForImportingProductData data)
        {
            //    var c = new DbContext("XlsToEf");
            var result = await _mediator.Send(data);
            return Json(result);
        }
        public async Task<ActionResult> SelectSheetAndDestinationForProductCategory([FromBody]XlsxProductCategoryColumnMatcherQuery selectedInfo)
        {
            try
            {
                var data = await _mediator.Send(selectedInfo);
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

        [HttpPost]
        public async Task<ActionResult> SubmitProductCategoryColumnMatches([FromBody]DataMatchesForImportingProductCategoryData data)
        {
            var result = await _mediator.Send(data);
            return Json(result);
        }

        public async Task<ActionResult> SelectSheetAndDestinationForOrder([FromBody]XlsxOrderColumnMatcherQuery selectedInfo)
        {
            try
            {
                var data = await _mediator.Send(selectedInfo);
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

        public async Task<JsonResult> SubmitOrderColumnMatches([FromBody]DataMatchesForImportingOrderData data)
        {
            var result = await _mediator.Send(data);
            return Json(result);
        }
    }
}
