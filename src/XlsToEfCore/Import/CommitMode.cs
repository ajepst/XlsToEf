namespace XlsToEfCore.Import
{
    public enum CommitMode
    {
        AnySuccessfulOneAtATime,
        AnySuccessfulAtEndAsBulk,   
        CommitAllAtEndIfAllGoodOrRejectAll,
        NoCommit
    }
}