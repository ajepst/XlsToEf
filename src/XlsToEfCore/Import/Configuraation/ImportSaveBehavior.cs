namespace XlsToEfCore.Import
{
    public class ImportSaveBehavior
    {
        public ImportSaveBehavior()
        {
            RecordMode = RecordMode.Upsert;
            CommitMode = CommitMode.AnySuccessfulAtEndAsBulk;
        }

        public RecordMode RecordMode { get; set; }
        public CommitMode CommitMode { get; set; }
    }
}