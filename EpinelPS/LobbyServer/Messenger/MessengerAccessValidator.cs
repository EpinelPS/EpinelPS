using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Messenger;

internal static class MessengerAccessValidator
{
    public static bool CanEnter(User user, MessengerConditionTriggerRecord condition, MessengerDialogRecord opener)
    {
        if (condition.MessageType == MessageType.RandomMessage || condition.MessageType == MessageType.DailyMessage)
        {
            if (!user.PickedMessages.Any(pick => pick.ConversationId == condition.Tid))
            {
                return false;
            }
        }
        else
        {
            // If the conversation was already created and exists in MessengerData,
            // the user already legitimately has access. Do not deny entry.
            if (!user.MessengerData.Any(m => m.ConversationId == condition.Tid))
            {
                if (!MessengerTriggerUtils.IsTriggerListSatisfied(user, condition.TriggerList))
                {
                    return false;
                }
            }
        }

        return IsRoomUnlockSatisfied(user, opener.RoomId);
    }

    internal static bool IsRoomUnlockSatisfied(User user, string? roomId)
    {
        if (string.IsNullOrEmpty(roomId) || !GameData.Instance.MessengerRooms.TryGetValue(roomId, out MessengerRoomRecord? room))
        {
            return true;
        }

        bool squadSatisfied = room.UnlockConditionSquad == Squad.None || user.Characters.Any(character =>
            GameData.Instance.CharacterTable.TryGetValue(character.Tid, out CharacterRecord? record) &&
            record.Squad == room.UnlockConditionSquad);

        bool characterSatisfied = room.UnlockConditionCharacter == 0 || user.Characters.Any(character =>
            GameData.Instance.CharacterTable.TryGetValue(character.Tid, out CharacterRecord? record) &&
            record.NameCode == room.UnlockConditionCharacter);

        return squadSatisfied && characterSatisfied;
    }
}
