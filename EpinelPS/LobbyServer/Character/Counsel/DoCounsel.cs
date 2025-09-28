using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel
{
    [PacketPath("/character/attractive/counsel")]
    public class DoCounsel : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCharacterCounsel req = await ReadData<ReqCharacterCounsel>();
            User user = GetUser();

            ResCharacterCounsel response = new();

            foreach (KeyValuePair<CurrencyType, long> currency in user.Currency)
            {
                response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
            }

            NetUserAttractiveData? currentBondInfo = user.BondInfo.FirstOrDefault(x => x.NameCode == req.NameCode);

            if (currentBondInfo != null)
            {
                int beforeLv = currentBondInfo.Lv;
                int beforeExp = currentBondInfo.Exp;

                currentBondInfo.Exp += 100;
                currentBondInfo.CounseledCount++;
                currentBondInfo.CanCounselToday = true; // Always allow counseling
                UpdateAttractiveLevel(currentBondInfo);

                response.Attractive = currentBondInfo;
                response.Exp = new NetIncreaseExpData
                {
                    NameCode = currentBondInfo.NameCode,
                    BeforeLv = beforeLv,
                    BeforeExp = beforeExp,
                    CurrentLv = currentBondInfo.Lv,
                    CurrentExp = currentBondInfo.Exp,
                    GainExp = 100
                };
            }
            else
            {
                NetUserAttractiveData data = new NetUserAttractiveData()
                {
                    NameCode = req.NameCode,
                    Exp = 100,
                    CounseledCount = 1,
                    IsFavorites = false,
                    CanCounselToday = true,
                    Lv = 1
                };
                UpdateAttractiveLevel(data);
                user.BondInfo.Add(data);
                response.Attractive = data;
                response.Exp = new NetIncreaseExpData
                {
                    NameCode = data.NameCode,
                    BeforeLv = 1,
                    BeforeExp = 0,
                    CurrentLv = 1,
                    CurrentExp = 100,
                    GainExp = 100
                };
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private void UpdateAttractiveLevel(NetUserAttractiveData attractiveData)
        {
            while (attractiveData.Lv < 40)
            {
                AttractiveLevelRecord? levelInfo = GameData.Instance.AttractiveLevelTable.FirstOrDefault(x => x.Value.AttractiveLevel == attractiveData.Lv).Value;

                if (levelInfo == null)
                {
                    // No more level data
                    break;
                }

                if (attractiveData.Exp >= levelInfo.AttractivePoint)
                {
                    attractiveData.Exp -= levelInfo.AttractivePoint;
                    attractiveData.Lv++;
                }
                else
                {
                    break;
                }
            }
        }
    }
}