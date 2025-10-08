namespace FloodOnlineReportingTool.Public.Settings;

public class GovNotifyTemplates
{ 

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to send the user thier password to allow them to edit the record</para>
    /// </summary>
    public required string EnableRecordEditingAccount { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to send the user thier password following a password reset</para>
    /// </summary>
    public required string ResetRecordEditingAccount { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to confirm that the user is no longer able to edit the record (for example at the end of the retention period)</para>
    /// </summary>
    public required string DisableRecordEditingAccount { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to confirm that the user will not get notifications but can still edit and change the notification settings</para>
    /// </summary>
    public required string Unsubscribe { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to message the user if they need to agree to the data protection statement</para>
    ///     <para>(nomrally this would follow an update to the terms and conditions)</para>
    /// </summary>
    public required string RequestDpaUpdate { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to send the report owner a request for more information</para>
    /// </summary>
    public required string RequestFullReport { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to send a report owner an update about the status of thier record</para>
    /// </summary>
    public required string SendStatusUpdate { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to email the report owner a comment from a flood risk manager</para>
    /// </summary>
    public required string SendPublicComment { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to email the reporter a copy of thier report on demand</para>
    /// </summary>
    public required string SendCopyOfReport { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to notify the user that their report has been marked for deletion</para>
    /// </summary>
    public required string RecordDeletion { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to </para>
    /// </summary>
    //public required string EnableRecordEditing { get; init; }


}
