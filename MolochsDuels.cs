// Decompiled with JetBrains decompiler
// Type: MolochsDuels.MolochsDuels
// Assembly: MolochsDuels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 86B3BDB9-A3A7-4397-A47B-606A78A8EE24
// Assembly location: C:\Users\andre\Downloads\MolochsDuels.dll

using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
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
