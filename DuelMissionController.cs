// Decompiled with JetBrains decompiler
// Type: MolochsDuels.DuelMissionController
// Assembly: MolochsDuels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 86B3BDB9-A3A7-4397-A47B-606A78A8EE24
// Assembly location: C:\Users\andre\Downloads\MolochsDuels.dll

using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Extensions = TaleWorlds.Core.Extensions;

namespace MolochsDuels
{
    internal class DuelMissionController : MissionLogic
    {
        private CharacterObject _duelCharacter;
        private bool _spawnBothSidesWithHorses;
        private bool _duelHasEnded;
        private BasicMissionTimer _duelEndTimer;
        private Agent _playerAgent;
        private Agent _duelAgent;
        private bool _duelWon;
        private bool _friendlyDuel;
        private bool _isInsideSettlement;
        private Scene _fallbackScene;

        public DuelMissionController(
          CharacterObject duelCharacter,
          bool spawnBothSidesWithHorses,
          bool friendlyDuel,
          bool isInsideSettlement)
        {
            _duelCharacter = duelCharacter;
            _spawnBothSidesWithHorses = spawnBothSidesWithHorses;
            _friendlyDuel = friendlyDuel;
            _isInsideSettlement = isInsideSettlement;
        }

        public override void AfterStart()
        {
            Mission.SetMissionMode(MissionMode.Duel, true);
            _duelHasEnded = false;
            _duelEndTimer = new BasicMissionTimer();
            InitializeMissionTeams();
            MatrixFrame playerSpawnFrame;
            MatrixFrame opponentSpawnFrame;
            if (_isInsideSettlement)
            {
                getArenaSpawnFrames(out playerSpawnFrame, out opponentSpawnFrame);
            }
            else
            {
                var attackerEntity = Mission.Current.Scene.FindEntityWithTag("attacker_infantry") ?? Mission.Current.Scene.FindEntityWithName("sp_attacker_infantry");
                
                Vec3 globalPosition = attackerEntity.GlobalPosition;
                getBattleSpawnFrames(globalPosition.AsVec2, out playerSpawnFrame, out opponentSpawnFrame);
            }
            _playerAgent = SpawnAgent(CharacterObject.PlayerCharacter, playerSpawnFrame);
            Mission.CameraIsFirstPerson = false;
            _duelAgent = SpawnAgent(_duelCharacter, opponentSpawnFrame);
        }

        public override void OnMissionTick(float dt)
        {
            if (!_duelHasEnded || (double)_duelEndTimer.ElapsedTime <= 4.0)
                return;
            GameTexts.SetVariable("leave_key", GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "CombatHotKeyCategory", 4));
            MBInformationManager.AddQuickInformation(GameTexts.FindText("str_duel_has_ended", null), 0, null, "");
            _duelEndTimer.Reset();
        }

        public override InquiryData OnEndMissionRequest(out bool canLeave)
        {
            canLeave = true;
            return _duelHasEnded ? null : new InquiryData("", GameTexts.FindText("str_give_up_fight", null).ToString(), true, true, GameTexts.FindText("str_ok", null).ToString(), GameTexts.FindText("str_cancel", null).ToString(), new Action(Mission.OnEndMissionResult), null, "");
        }

        public override void OnAgentRemoved(
          Agent affectedAgent,
          Agent affectorAgent,
          AgentState agentState,
          KillingBlow killingBlow)
        {
            if (!affectedAgent.IsHuman)
                return;
            if (affectedAgent == _duelAgent)
                _duelWon = true;
            _duelHasEnded = true;
        }

        public override bool MissionEnded(ref MissionResult missionResult) => false;

        protected override void OnEndMission() => Campaign.Current.GetCampaignBehavior<DuelCampaignBehavior>().setDuelMissionResult(_duelWon);

        private Agent SpawnAgent(CharacterObject character, MatrixFrame spawnFrame)
        {
            AgentBuildData agentBuildData1 = new AgentBuildData(character);
            agentBuildData1.BodyProperties(character.GetBodyPropertiesMax());
            Team team = character == CharacterObject.PlayerCharacter ? Mission.PlayerTeam : Mission.PlayerEnemyTeam;
            Mission mission = Mission;
            AgentBuildData agentBuildData2 = agentBuildData1.Team(team).InitialPosition(spawnFrame.origin);
            Vec2 vec2 = spawnFrame.rotation.f.AsVec2;
            vec2 = vec2.Normalized();
            ref Vec2 local = ref vec2;
            AgentBuildData agentBuildData3 = agentBuildData2.InitialDirection(local).NoHorses(!_spawnBothSidesWithHorses).Equipment(character.FirstBattleEquipment).TroopOrigin(GetAgentOrigin(character)).ClothingColor1(character.Culture.Color).ClothingColor2(character.Culture.Color2);
            Agent agent = mission.SpawnAgent(agentBuildData3, false);
            agent.FadeIn();
            if (character == CharacterObject.PlayerCharacter)
                agent.Controller = (Agent.ControllerType)2;
            if (agent.IsAIControlled)
                agent.SetWatchState((Agent.WatchState)2);
            agent.WieldInitialWeapons((Agent.WeaponWieldActionType)2);
            return agent;
        }

        private IAgentOriginBase GetAgentOrigin(CharacterObject character) => 
            !_friendlyDuel ? new PartyAgentOrigin(character.HeroObject.PartyBelongedTo.Party, character, character.Level, new UniqueTroopDescriptor(), false) : 
            (IAgentOriginBase)new SimpleAgentOrigin(character, character.Level, null, new UniqueTroopDescriptor());

        private void InitializeMissionTeams()
        {
            Mission.Teams.Add(0, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, Hero.MainHero.Clan.Banner, true, false, true);
            Mission.Teams.Add((BattleSideEnum)1, _duelCharacter.Culture.Color, _duelCharacter.Culture.Color2, _duelCharacter.HeroObject.Clan.Banner, true, false, true);
            Mission.PlayerTeam = Mission.Teams.Defender;
        }

        private void getBattleSpawnFrames(
          Vec2 spawnPoint,
          out MatrixFrame playerSpawnFrame,
          out MatrixFrame opponentSpawnFrame)
        {
            float num = 0.0f;
            Vec2 vec2_1 = new Vec2(spawnPoint.X, spawnPoint.Y + 10f);
            Mission.Scene.GetHeightAtPoint(vec2_1, (BodyFlags)2208137, ref num);
            Vec3 vec3_1 = new Vec3(vec2_1.X, vec2_1.Y, num, -1);
            Mat3 mat3_1 = new Mat3(Vec3.Side, new Vec3(0f, -1f, 0f, -1f), Vec3.Up);
            Vec2 vec2_2 = new Vec2(spawnPoint.X, spawnPoint.Y - 10f);
            Mission.Scene.GetHeightAtPoint(vec2_2, (BodyFlags)2208137, ref num);
            Vec3 vec3_2 = new Vec3(vec2_2.X, vec2_2.Y, num, -1);
            Mat3 mat3_2 = new Mat3(Vec3.Side, Vec3.Forward, Vec3.Up);
            playerSpawnFrame = new MatrixFrame(mat3_1, vec3_1);
            opponentSpawnFrame = new MatrixFrame(mat3_2, vec3_2);
        }

        private void getArenaSpawnFrames(
          out MatrixFrame playerSpawnFrame,
          out MatrixFrame opponentSpawnFrame)
        {
            List<MatrixFrame> list = Mission.Scene.FindEntitiesWithTag("sp_arena").Select<GameEntity, MatrixFrame>(e => e.GetGlobalFrame()).ToList<MatrixFrame>();
            for (int index = 0; index < list.Count; ++index)
            {
                MatrixFrame matrixFrame = list[index];
                matrixFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
                list[index] = matrixFrame;
            }
            playerSpawnFrame = Extensions.GetRandomElement<MatrixFrame>(list);
            list.Remove(playerSpawnFrame);
            opponentSpawnFrame = Extensions.GetRandomElement<MatrixFrame>(list);
        }

        private Vec2 getMiddlePoint(Vec3 pointA, Vec3 pointB) => new Vec2((float)(((double)pointA.X + (double)pointB.X) / 2.0), (float)(((double)pointA.Y + (double)pointB.Y) / 2.0));
    }
}
