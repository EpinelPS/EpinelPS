using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Binding;

namespace EpinelPS.Commands.Handler;

public class SendMailParameter : ICommandParameters
{
    static ParameterDescriptor[] ICommandParameters.Descriptors => [
        Param.Int(0, "senderId", "The sender ID (e.g. 99 for system)"),
        Param.String(1, "title", "The mail title"),
        Param.String(2, "content", "The mail content"),
        Param.Int(3, "validDays", "Number of days the mail is valid"),
        Param.String(4, "attachments", "Attachments in format: type-id-count,type-id-count", true),
    ];

    public int SenderId { get; init; }
    public string Title { get; init; } = "";
    public string Content { get; init; } = "";
    public int ValidDays { get; init; }
    public string? Attachments { get; init; }
}

public class SendMailHandler(IExecutionContext context) : BaseHandler<SendMailParameter>(context)
{
    public override string Name => "send-mail";
    public override string Description => "Send a mail to the selected user with optional attachments";

    protected async override Task<HandleResult> ExecuteAsync(SendMailParameter parameters)
    {
        if (context.SelectedUser == null)
            return new HandleResult(false, "No user selected");

        var user = context.SelectedUser;
        var attachments = new List<MailAttachment>();
        if (!string.IsNullOrEmpty(parameters.Attachments))
        {
            foreach (var item in parameters.Attachments.Split(','))
            {
                string[] attParts = item.Split('-');
                if (attParts.Length != 3) continue;

                if (int.TryParse(attParts[0], out int type) &&
                    int.TryParse(attParts[1], out int id) &&
                    int.TryParse(attParts[2], out int count))
                {
                    attachments.Add(new MailAttachment
                    {
                        Type = type,
                        Id = id,
                        Count = count
                    });
                }
            }
        }

        var rsp = AdminCommands.SendMail(user, parameters.SenderId, parameters.Title, parameters.Content, parameters.ValidDays, attachments);
        return rsp.ok
            ? new HandleResult(true, "Mail sent successfully")
            : new HandleResult(false, rsp.error);
    }
}
