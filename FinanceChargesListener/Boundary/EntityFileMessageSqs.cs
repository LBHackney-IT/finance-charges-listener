namespace FinanceChargesListener.Boundary
{
    public class EntityFileMessageSqs
    {
        public string RelativePath { get; set; }
        public string BucketName { get; set; }
        public string FileUrl { get; set; }
        public int StepNumber { get; set; }
        public int WriteIndex { get; set; }
    }
}
