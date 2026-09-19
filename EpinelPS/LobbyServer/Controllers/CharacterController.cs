using EpinelPS.Database;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for stage completion
/// </summary>
[ApiController]
public class CharacterController(IUserService UserController, GameContext db, IInventoryService InventoryService) : Controller
{
    [Route("/v1/character/get")]
    [HttpPost]
    public ActionResult<ResGetCharacterData> GetCharacter([FromBodyProtobuf] ReqGetCharacterData req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetCharacterData response = new();

        foreach (CharacterModel item in user.Characters)
        {
            response.Character.Add(new NetUserCharacterData()
            {
                Default = new()
                {
                    Csn = item.Csn,
                    Skill1Lv = item.Skill1Lvl,
                    Skill2Lv = item.Skill2Lvl,
                    CostumeId = item.CostumeId,
                    Lv = item.Level,
                    Grade = item.Grade,
                    Tid = item.Tid,
                    UltiSkillLv = item.UltimateLevel
                },
                //Artifact // TODO
                //IsSynchro = user.GetSynchro(item.Csn)
            });

            // Check if character is main force
            if (item.IsMainForce)
            {
                response.MainForceCsnList.Add(item.Csn);
            }
        }

        // TODO: synchro device

        return response;
    }


    [Route("/v1/team/support-character/list-used-count")]
    [HttpPost]
    public ActionResult<ResListSupportCharacterUsedCount> ListUsedCount([FromBodyProtobuf] ReqListSupportCharacterUsedCount req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO: Limit temporary participation here
        return new ResListSupportCharacterUsedCount();
    }

    [Route("/v1/team/get")]
    [HttpPost]
    public ActionResult<ResGetTeamData> GetTeam([FromBodyProtobuf] ReqListSupportCharacterUsedCount req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        var response = new ResGetTeamData();

        if (user.Characters.Count > 0)
        {
            foreach (var x in user.Teams)
            {
                NetUserTeamData team;
                if (response.TypeTeams.Any(a => a.Type == x.TeamType))
                {
                    team = response.TypeTeams.First(a => a.Type == x.TeamType);
                }
                else
                {
                    team = new NetUserTeamData()
                    {
                        Type = x.TeamType,
                        LastContentsTeamNumber = x.LastContentsTeamNumber
                    };
                    response.TypeTeams.Add(team);
                }

                NetTeamData teamSlot;
                if (team.Teams.Any(a => a.TeamNumber == x.TeamNumber))
                {
                    teamSlot = team.Teams.First(a => a.TeamNumber == x.TeamNumber);
                }
                else
                {
                    teamSlot = new NetTeamData()
                    {
                        TeamNumber = x.TeamNumber,
                    };

                    team.Teams.Add(teamSlot);
                }

                for (int i = 0; i < 5; i++)
                {
                    teamSlot.Slots.Add(new NetTeamSlot()
                    {
                        Slot = i + 1,
                        Value = x.SlotIds[0],
                        ValueType = x.SlotIdTypes[1]
                    });
                }
            }
        }

        return response;
    }
}
