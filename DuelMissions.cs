// Decompiled with JetBrains decompiler
// Type: MolochsDuels.DuelMissions
// Assembly: MolochsDuels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 86B3BDB9-A3A7-4397-A47B-606A78A8EE24
// Assembly location: C:\Users\andre\Downloads\MolochsDuels.dll

using SandBox;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Arena;
using SandBox.View.Missions;
using SandBox.View.Missions.Sound.Components;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound.Components;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace MolochsDuels
{
    internal class DuelMissions
    {
        public static MissionInitializerRecord CreateDuelMissionInitializerRecord(
          string sceneName,
          string sceneLevels = "",
          bool doNotUseLoadingScreen = false)
        {
            MissionInitializerRecord initializerRecord = new MissionInitializerRecord(sceneName);
            initializerRecord.DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier();
            initializerRecord.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
            initializerRecord.PlayingInCampaignMode = Campaign.Current.GameMode == CampaignGameMode.Campaign;
            initializerRecord.AtmosphereOnCampaign = Campaign.Current.GameMode == CampaignGameMode.Campaign ? Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(CampaignTime.Now, MobileParty.MainParty.GetLogicalPosition()) : null;
            initializerRecord.SceneLevels = sceneLevels;
            initializerRecord.DoNotUseLoadingScreen = doNotUseLoadingScreen;
            return initializerRecord;
        }

        public static Mission OpenDuelMission(
          string scene,
          CharacterObject duelCharacter,
          bool spawnBothSidesWithHorse,
          bool friendlyDuel,
          bool isInsideSettlement)
        {
            return MissionState.OpenNew("DuelMission", CreateDuelMissionInitializerRecord(scene), (a) => new MissionBehavior[]
            {
                new MissionCampaignView(),
                new CampaignMissionComponent(),
                new MissionOptionsComponent(),
                new DuelMissionController(duelCharacter, spawnBothSidesWithHorse, friendlyDuel, isInsideSettlement),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
                ViewCreator.CreateMissionAgentStatusUIHandler(a),
                ViewCreator.CreateMissionMainAgentEquipmentController(a),
                (MissionView) new MissionSingleplayerViewHandler(),
                (MissionView) new MusicMissionView(new MusicBaseComponent[1]
                {
                    (MusicBaseComponent) new MusicMissionBattleComponent()
                }),
                new MissionBoundaryWallView(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new MissionGauntletOptionsUIHandler(),
                new AgentHumanAILogic(),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionLeaveView(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreatePhotoModeView(),
            }, true, true) ;
        }
    }
}
