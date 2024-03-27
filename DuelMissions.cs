using SandBox.Missions.MissionLogics;
using SandBox.View.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;

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
            initializerRecord.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.GetLogicalPosition());
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
                isInsideSettlement ? new MissionAudienceHandler(0.4f + MBRandom.RandomFloat * 0.3f) : null,
                //(MissionView) new MusicBattleMissionView(false),
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
