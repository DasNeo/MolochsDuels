using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MolochsDuels
{
    public class MolochsDuels : MBSubModuleBase
    {
        private DuelCampaignBehavior GetDuelCampaignBehavior() => Campaign.Current.GetCampaignBehavior<DuelCampaignBehavior>();

        protected override void OnApplicationTick(float dt)
        {
            GameStateManager current = GameStateManager.Current;
            if (current == null || !(current.ActiveState is MapState) || !GetDuelCampaignBehavior()._duelStarted)
                return;
            GameMenu.ActivateGameMenu("MolochsDuelMenu");
            GetDuelCampaignBehavior()._duelStarted = false;
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot() => InformationManager.DisplayMessage(new InformationMessage("Moloch's Duels Mod Loaded", Color.FromUint(6491457U)));

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(gameStarterObject is CampaignGameStarter campaignGameStarter))
                return;
            campaignGameStarter.AddBehavior(new DuelCampaignBehavior());
        }

        public override void OnCampaignStart(Game game, object gameStarterObject)
        {
        }
    }
}
