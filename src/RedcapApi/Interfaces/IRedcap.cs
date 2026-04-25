namespace Redcap.Interfaces
{
    /// <summary>
    /// Aggregate REDCap API contract.
    /// </summary>
    /// <remarks>
    /// Prefer the focused domain interfaces, such as <see cref="IRedcapRecords"/>
    /// or <see cref="IRedcapProjects"/>, when a consumer only needs part of the
    /// REDCap surface. This aggregate is retained so existing consumers can
    /// continue depending on the full API contract.
    /// </remarks>
    public interface IRedcap :
        IRedcapArms,
        IRedcapDataAccessGroups,
        IRedcapEvents,
        IRedcapFieldNames,
        IRedcapFileRepository,
        IRedcapFiles,
        IRedcapInstruments,
        IRedcapLogging,
        IRedcapMetadata,
        IRedcapProjects,
        IRedcapRecords,
        IRedcapRepeatingInstruments,
        IRedcapReports,
        IRedcapSurveys,
        IRedcapUserRoles,
        IRedcapUsers,
        IRedcapVersion
    {
    }
}
