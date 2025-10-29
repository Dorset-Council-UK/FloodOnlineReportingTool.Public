namespace FloodOnlineReportingTool.Public.Settings;

public class GovNotifyTemplates
{
    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to send the test notification only</para>
    /// </summary>
    public required string TestNotification { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used whenever an email address is added or changed.</para>
    ///     <para>The reciepient is able to verify the email if not added in error and will be given the rights set out in the notification.</para>
    /// </summary>
    public required string VerifyEmailAddress { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to confirm to the contact that their changes have been saved.</para>
    /// </summary>
    public required string ConfirmContactUpdated { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to let the contact know that their details have been removed.</para>
    /// </summary>
    public required string ConfirmContactDeleted { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to message the user if they need to agree to the data protection statement</para>
    ///     <para>(nomrally this would follow an update to the terms and conditions)</para>
    /// </summary>
    public required string RequestDpaUpdate { get; init; }

    /// <summary>
    ///     <para>GovNotify template Id under ? > ?. Name: ?.</para>
    ///     <para>This template is used to confirm that the user will not get notifications but can still edit and change the notification settings</para>
    /// </summary>
    public required string Unsubscribe { get; init; }

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
