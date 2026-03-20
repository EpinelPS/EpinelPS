using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Minigame.ScenarioChoice
{
    public class ScenarioChoiceMain
    {

        private static ScenarioChoiceData Convert(MiniGameStoryChoice val)
        {
            var d = new ScenarioChoiceData();
            d.ChosenChoices.AddRange(val.Choices);
            d.ViewedScenarioSceneList.AddRange(val.SeenChoices);
            d.LastSeenEventScenarioDialogTableGroupId = val.SeenChoices.Last();

            return d;
        }

        public static ScenarioChoiceData GetData(User user, int id)
        {
            if (user.MiniGameStoryChoice.TryGetValue(id, out MiniGameStoryChoice? val))
                return Convert(val);

            MiniGameStoryChoice c = new MiniGameStoryChoice();
            c.Id = id;

            user.MiniGameStoryChoice.Add(id, c);
            JsonDb.Save();
            return Convert(c);
        }

        public static ScenarioChoiceData Update(User user, int id, string scenarioScene, Google.Protobuf.Collections.RepeatedField<string> chosenChoices)
        {
            if (!user.MiniGameStoryChoice.TryGetValue(id, out MiniGameStoryChoice? val))
            {
                val = new MiniGameStoryChoice();
                val.Id = id;

                user.MiniGameStoryChoice.Add(id, val);
            }

            val.Choices = chosenChoices.ToList();

            foreach(var item in val.Choices)
            {
                if (!val.SeenChoices.Contains(item))
                    val.SeenChoices.Add(item);
            }

            JsonDb.Save();

            return GetData(user, id);
        }
    }
}
