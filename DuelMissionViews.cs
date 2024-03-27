using SandBox.View.Missions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;

namespace MolochsDuels
{
    /*
    [ViewCreatorModule]
    public class DuelMissionViews
    {
        [ViewMethod("DuelMission")]
        public static MissionView[] OpenDuelMission(Mission mission)
        {
            List<MissionView> missionViewList = new List<MissionView>();
            missionViewList.Add(new MissionCampaignView());
            missionViewList.Add(ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode));
            missionViewList.Add(ViewCreator.CreateOptionsUIHandler());
            missionViewList.Add(ViewCreator.CreateMissionLeaveView());
            missionViewList.Add(ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler());
            missionViewList.Add(new StealthMissionUIHandler());

            missionViewList.Add(new MusicTournamentMissionView());
            missionViewList.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
            missionViewList.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
            missionViewList.Add(ViewCreator.CreateMissionBoundaryCrossingView());
            missionViewList.Add(new MissionBoundaryWallView());
            missionViewList.Add(new MissionItemContourControllerView());
            missionViewList.Add(new MissionAgentContourControllerView());
            return missionViewList.ToArray();
        }
    }
    */
}
