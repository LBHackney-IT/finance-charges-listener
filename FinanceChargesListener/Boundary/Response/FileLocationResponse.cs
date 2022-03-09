using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Boundary.Response
{
    public class FileLocationResponse
    {
        public string RelativePath { get; set; }
        public string BucketName { get; set; }
        public Uri FileUrl { get; set; }
        public int StepNumber { get; set; }
        public int WriteIndex { get; set; }
    }
}
