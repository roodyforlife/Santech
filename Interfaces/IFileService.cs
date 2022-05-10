﻿using Microsoft.AspNetCore.Http;
using SanTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanTech.Interfaces
{
    public interface IFileService
    {
        public byte[] FromImageToByte(IFormFile uploadedFile);
        public byte[] FromImageToByte(string imageFileName);
        public string GetHTMLBodyForCheck(Order application);
        public string GetCreatedPdfFile(Order application);
    }
}
