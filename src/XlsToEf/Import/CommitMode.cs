namespace XlsToEf.Import
{
    public enum CommitMode
    {
        AnySuccessfulOneAtATime,
        AnySuccessfulAtEndAsBulk,   
        CommitAllAtEndIfAllGoodOrRejectAll,
        NoCommit
    }
}