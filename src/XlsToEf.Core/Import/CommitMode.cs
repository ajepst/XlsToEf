namespace XlsToEf.Core.Import
{
    public enum CommitMode
    {
        AnySuccessfulOneAtATime,
        AnySuccessfulAtEndAsBulk,   
        CommitAllAtEndIfAllGoodOrRejectAll,
        NoCommit
    }
}