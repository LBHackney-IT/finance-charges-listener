namespace FinanceChargesListener.Boundary
{
    public class EntityFileMessageSqs
    {
        public string RelativePath { get; set; }
        public string BucketName { get; set; }
        public string FileUrl { get; set; }
    }
}
