using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.ViewModels
{
    //This model is used to upload the file into the wwwroot/reports in ReportDataInsert function UploadGsReport
    public class FileUploadViewModel
    {
        public int Id { get; set; }
        public IFormFile gsReport { get; set; }
    }
}
