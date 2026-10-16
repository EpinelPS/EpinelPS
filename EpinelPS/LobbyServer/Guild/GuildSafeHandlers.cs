using System;
using System.Threading.Tasks;
using EpinelPS.Database;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Guild;

[GameRequest("/guild/create")]
public class CreateGuild : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqCreateGuild>();

        // Safe no-op as requested: acknowledge request with success, no guild mutation or save corruption
        ResCreateGuild response = new()
        {
            Result = CreateGuildResult.Okay,
            Guild = new NetGuildData
            {
                Gsn = 10001,
                Name = "Counters",
                Emblem = 1,
                Locale = "en",
                JoinType = 0,
                JoinLevel = 1,
                Grade = 3,
                UnionRaidTier = 6,
                UnionRaidTierNumber = 0
            }
        };

        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/get")]
public class GetGuild : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetGuild req = await ReadData<ReqGetGuild>();
        User user = GetUser();

        ResGetGuild response = new()
        {
            CanChangeUnionNameForFree = true,
            IsSimple = req.IsSimple,
            Guild = new NetGuildData
            {
                Gsn = 10001,
                Name = "Counters",
                Emblem = 1,
                Locale = "en",
                JoinType = 0,
                JoinLevel = 1,
                Grade = 3,
                UnionRaidTier = 6,
                UnionRaidTierNumber = 0
            }
        };

        response.Members.Add(new NetGuildMemberData
        {
            Usn = (long)user.ID,
            Type = 1,
            JoinedAt = DateTime.UtcNow.Ticks,
            Nickname = user.PlayerName ?? "Commander",
            Lv = user.userPointData?.UserLevel ?? 1,
            Server = 1001,
            LastActionAt = DateTime.UtcNow.Ticks,
            Icon = user.ProfileIconId > 0 ? user.ProfileIconId : 30100,
            Frame = user.ProfileFrame > 0 ? user.ProfileFrame : 1,
            TeamCombat = 100000,
            UserTitleId = user.TitleId > 0 ? user.TitleId : 1,
            UserTitleDisplayData = new NetUserTitleDisplayData { Count = 0 },
            SendMailAt = Timestamp.FromDateTime(DateTime.UtcNow)
        });

        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/search")]
public class SearchGuild : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSearchGuildList>();
        ResSearchGuildList response = new();
        response.Guilds.Add(new NetSimpleGuildData
        {
            Gsn = 10001,
            Name = "Counters",
            Emblem = 1,
            Grade = 3,
            MemberCount = 1,
            UnionRaidTier = 6,
            UnionRaidTierNumber = 0
        });
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/publicinfo")]
public class GetGuildPublicInfo : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetGuildPublicInfo>();
        ResGetGuildPublicInfo response = new()
        {
            Guild = new NetGuildData
            {
                Gsn = 10001,
                Name = "Counters",
                Emblem = 1,
                Locale = "en",
                Grade = 3,
                UnionRaidTier = 6,
                UnionRaidTierNumber = 0
            }
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/join")]
public class JoinGuild : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqJoinGuild>();
        ResJoinGuild response = new() { Result = JoinGuildResult.Ok };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/leave")]
public class LeaveGuild : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqLeaveGuild>();
        ResLeaveGuild response = new() { Result = LeaveGuildResult.Ok, GuildLeaveAt = DateTime.UtcNow.Ticks };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/getlog")]
public class GetGuildLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetGuildLog>();
        ResGetGuildLog response = new() { Result = GetGuildLogResult.Ok };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/chat/get")]
public class GetGuildChatList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetGuildChatList>();
        ResGetGuildChatList response = new() { Result = GetGuildChatListResult.Success };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/chat/send")]
public class SendGuildChat : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSendGuildChat>();
        ResSendGuildChat response = new() { Result = GuildChatSendResult.Success };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/chat/delete")]
public class DeleteGuildChat : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqDeleteGuildChat>();
        ResDeleteGuildChat response = new() { Result = DeleteGuildChatResult.Success };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/joinrequestlist")]
public class GuildJoinRequestList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGuildJoinRequestList>();
        ResGuildJoinRequestList response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/confirmjoinrequest")]
public class ConfirmGuildJoinRequest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqConfirmGuildJoinRequest>();
        ResConfirmGuildJoinRequest response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/allconfirmjoinrequest")]
public class AllConfirmGuildJoinRequest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqAllConfirmGuildJoinRequest>();
        ResAllConfirmGuildJoinRequest response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/kickmember")]
public class KickGuildMember : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqKickGuildMember>();
        ResKickGuildMember response = new() { Result = KickGuildMemberResult.Ok };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/changemanager")]
public class ChangeGuildManager : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqChangeGuildManager>();
        ResChangeGuildManager response = new() { Result = ChangeGuildManagerResult.Ok };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/sendmail")]
public class SendGuildMail : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSendGuildMail>();
        ResSendGuildMail response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/setname")]
public class SetGuildName : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetGuildName>();
        ResSetGuildName response = new() { Result = SetGuildNameResult.Okay };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/setnameforfree")]
public class SetGuildNameForFree : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetGuildNameForFree>();
        ResSetGuildNameForFree response = new() { Result = SetGuildNameResult.Okay };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/updatenotice")]
public class UpdateGuildNotice : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqUpdateGuildNotice>();
        ResUpdateGuildNotice response = new()
        {
            Result = SetGuildNoticeResult.Okay,
            BanEndTime = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/guild/updatesettings2")]
public class UpdateGuildSettings : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqUpdateGuildSettings2>();
        ResUpdateGuildSettings2 response = new()
        {
            Result = SetGuildDescResult.Okay,
            BanEndTime = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        await WriteDataAsync(response);
    }
}
