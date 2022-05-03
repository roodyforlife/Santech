﻿using IronPdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SanTech.Interfaces;
using SanTech.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanTech.Services
{
    public class FileService : IFileService
    {
        private readonly IHostingEnvironment hostingEnvironment;
        public FileService(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }
        public byte[] FromImageToByte(IFormFile uploadedFile)
        {
                byte[] imageData = null;
                using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)uploadedFile.Length);
                }
            return imageData;
        }

        public string GetCreatedPdfFile(Application application)
        {
            var pdfDocument = new HtmlToPdf().RenderHtmlAsPdf(GetHTMLBodyForCheck(application));
            string pdfPath = Path.Combine(this.hostingEnvironment.WebRootPath, $"Files/Orders/{application.OrderNumber}.pdf");
            pdfDocument.SaveAs(pdfPath);
            return pdfPath;
        }

        public string GetHTMLBodyForCheck(Application application)
        {
            string body = String.Empty;
            string basket = String.Empty;
            string path = Path.Combine(this.hostingEnvironment.WebRootPath, "Files/email", "Check.html");
            using (StreamReader reader = new StreamReader(path))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{OrderNumber}", application.OrderNumber).Replace("{UserName}", application.Name).Replace("{UserSecondName}", application.SecondName).
                Replace("{City}", application.City).Replace("{Address}", application.Address).Replace("{Post}", application.Delivery).
                Replace("{TotalCost}", application.TotalCost.ToString("N0"));
            foreach (var item in application.User.Basket)
            {
                basket += $"<div class='content__basket__item'><div class='text1'>{item.Product.Title}</div><div class='text1 content__basket__item__code'>Код товара: {item.Product.Id}</div><div class='text1'>Количество: {item.NumberOfProduct}</div><div class='text1 content__basket__item__cost'>{(item.NumberOfProduct * (item.Product.Cost * (100 - item.Product.SaleProcent) / 100)).ToString("N0")} грн.</div></div>";
            }
            body = body.Replace("{BasketContent}", basket);
            return body;
        }
    }
}
