// Decompiled with JetBrains decompiler
// Type: MolochsDuels.DuelCampaignBehavior
// Assembly: MolochsDuels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 86B3BDB9-A3A7-4397-A47B-606A78A8EE24
// Assembly location: C:\Users\andre\Downloads\MolochsDuels.dll

using StoryMode.GauntletUI.Tutorial;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace MolochsDuels
{
    public class DuelCampaignBehavior : CampaignBehaviorBase
    {
        private static readonly string MDMenu = "MolochsDuelMenu";
        private bool _duelAccepted;
        public bool _duelStarted;
        private float _playerStrengthRatio;
        private bool _spawnBothSidesWithHorses;
        private bool _surrenderDemand;
        private bool _isFriendlyDuel;
        private bool _isCompanionDuel;
        private CharacterObject _duelOpponent;
        private DuelCampaignBehavior.DuelFightResultEnum _duelFightResult;
        private List<KeyValuePair<CharacterObject, CampaignTime>> _duelsFought = new List<KeyValuePair<CharacterObject, CampaignTime>>();
        private int _friendlyDuelWager;
        private bool _rewardsApplied;

        public override void RegisterEvents() => CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));

        public override void SyncData(IDataStore dataStore)
        {
            if (!dataStore.SyncData<CharacterObject>("_molochsDuels_Opponent", ref _duelOpponent))
                _duelOpponent = null;
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs();
            AddMenus(campaignGameStarter);
        }

        private void AddDialogs()
        {
            AddFriendlyChallengeDialogFlow();
            AddFriendlyDuelResponseFlow();
            AddEnemyChallengeDialogToInputToken("player_responds_to_surrender_demand");
            AddEnemyChallengeDialogToInputToken("player_verify_attack_on_enemy_lord");
            AddDuelResponseDialogFlow();
            AddCompanionDuelDialogFlow();
        }

        private void AddEnemyChallengeDialogToInputToken(string inputToken) => Campaign.Current.ConversationManager
            .AddDialogFlow(DialogFlow.CreateDialogFlow(inputToken, 100)
            .PlayerLine(new TextObject("{=molochsduels_player_challanges_enemy_entry}Let us settle this according to the old ways. I challenge you to a duel!"), null)
            .Condition(can_challenge_enemy_duel)
            .NpcLine(new TextObject("{=molochsduels_enemy_terms}What are your terms?"), null, null)
            .BeginPlayerOptions()
            .PlayerOption(new TextObject("{=molochsduels_player_expects_surrender}I expect you to surrender, should you lose!"), null)
            .Condition(player_can_demand_surrender)
            .Consequence(calculate_npc_response_to_surrender_demand)
            .GotoDialogState("duel_response_flow")
            .PlayerOption(new TextObject("{=molochsduels_duel_before_battle}We shall meet one on one before our armies clash in battle!"), null)
            .Condition(player_is_attacker)
            .Consequence(calculate_npc_response_to_duel)
            .GotoDialogState("duel_response_flow")
            .PlayerOption(new TextObject("{=molochsduels_player_expects_safe_passage}I expect you to allow us safe passage!"), null)
            .Condition(player_is_defender)
            .Consequence(calculate_npc_response_to_duel)
            .GotoDialogState("duel_response_flow")
            .EndPlayerOptions(), null);

        private void AddDuelResponseDialogFlow() => Campaign.Current.ConversationManager
            .AddDialogFlow(DialogFlow.CreateDialogFlow("duel_response_flow", 100)
            .BeginNpcOptions()
            .NpcOption(new TextObject("{=molochsduels_enemy_accepts_duel}I accept to meet you in honorable combat."), duel_accepted, null, null)
            .BeginPlayerOptions()
            .PlayerOption(new TextObject("{=molochsduels_enemy_accepts_duel_player_response}We shall meet on the field, then."), null)
            .Consequence(start_enemy_duel)
            .CloseDialog()
            .EndPlayerOptions()
            .NpcOption(new TextObject("{=molochsduels_enemy_declines_duel}I think not. Our armies will clash in battle!"), duel_rejected, null, null)
            .Consequence(ResetDuelResult)
            .EndNpcOptions()
            .CloseDialog(), null);

        private void AddFriendlyChallengeDialogFlow() => Campaign.Current.ConversationManager
            .AddDialogFlow(DialogFlow.CreateDialogFlow("lord_talk_speak_diplomacy_2", 100)
            .PlayerLine(new TextObject("{=molochsduels_player_challanges_friendly_entry}I challenge you to a duel!"), null)
            .Condition(can_challenge_friendly_duel)
            .NpcLine(new TextObject("{=molochsduels_friendly_accepts_duel}Very well, we shall test our strengths."), null, null)
            .BeginPlayerOptions()
            .PlayerOption(new TextObject("{=molochsduels_friendly_accepts_duel_player_response}We shall meet on the field, then."), null)
            .Consequence(start_friendly_duel)
            .CloseDialog()
            .PlayerOption(new TextObject("{=molochsduels_player_propose_wager}I propose a wager."), null)
            .Condition(characters_have_100_denars)
            .NpcLine(new TextObject("{=molochsduels_player_propose_wager_friendly_response}How much do you care to bet on your skills?"), null, null)
            .BeginPlayerOptions()
            .PlayerOption($"100 {new TextObject("{=molochsduels_money}denars")}", null)
            .Condition(characters_have_100_denars)
            .Consequence(set_100_duel_wager)
            .GotoDialogState("friendly_duel_response_flow")
            .PlayerOption($"250 {new TextObject("{= molochsduels_money}denars")}", null)
            .Condition(characters_have_250_denars)
            .Consequence(set_250_duel_wager)
            .GotoDialogState("friendly_duel_response_flow")
            .PlayerOption($"500 {new TextObject("{= molochsduels_money}denars")}", null)
            .Condition(characters_have_500_denars)
            .Consequence(set_500_duel_wager)
            .GotoDialogState("friendly_duel_response_flow")
            .PlayerOption($"1000 {new TextObject("{=molochsduels_money}denars")}", null)
            .Condition(characters_have_1000_denars)
            .Consequence(set_1000_duel_wager)
            .GotoDialogState("friendly_duel_response_flow")
            .PlayerOption($"1500 {new TextObject("{=molochsduels_money}denars")}", null)
            .Condition(characters_have_1500_denars)
            .Consequence(set_1500_duel_wager)
            .GotoDialogState("friendly_duel_response_flow")
            .EndPlayerOptions()
            .EndPlayerOptions(), null);

        private void AddFriendlyDuelResponseFlow() => Campaign.Current.ConversationManager
            .AddDialogFlow(DialogFlow.CreateDialogFlow("friendly_duel_response_flow", 100)
            .NpcLine(new TextObject("{=molochsduels_friendly_accepts_wager}I accept."), null, null)
            .Consequence(start_friendly_duel)
            .CloseDialog(), null);

        private void AddCompanionDuelDialogFlow() => Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("hero_main_options", 100)
            .PlayerLine(new TextObject("{=molochsduels_player_challanges_companion_entry}Let's test our skills one on one."), null)
            .Condition(can_challenge_companion_duel)
            .NpcLine(new TextObject("{=molochsduels_companion_accepts_duel}Very well."), null, null)
            .Consequence(start_companion_duel)
            .CloseDialog(), null);

        private void AddMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu(MDMenu, "{=!}{DUEL_MENU_TEXT}", initiate_menu, 0, 0, null);
            campaignGameStarter.AddGameMenuOption(MDMenu, MDMenu + "_start_duel", new TextObject("{=molochsduels_menu_begin_duel}Begin the duel").ToString(), start_duel_menu_option_condition, start_duel_mission_consequence, false, -1, false);
            campaignGameStarter.AddGameMenuOption(MDMenu, MDMenu + "_duel_ended_capture_enemy", new TextObject("{=molochsduels_menu_capture_enemy}Capture the enemy").ToString(), capture_enemy_option_condition, capture_enemy_consequence, false, -1, false);
            campaignGameStarter.AddGameMenuOption(MDMenu, MDMenu + "_duel_ended_let_party_go", new TextObject("{=molochsduels_menu_player_win_let_enemy_go}Leave").ToString(), let_enemy_go_option_condition, let_enemy_go_consequence, false, -1, false);
            campaignGameStarter.AddGameMenuOption(MDMenu, MDMenu + "_duel_ended_player_let_go", new TextObject("{=molochsduels_menu_player_lost_let_player_go}Leave").ToString(), enemy_lets_player_go_option_condition, enemy_lets_player_go_consequence, false, -1, false);
            campaignGameStarter.AddGameMenuOption(MDMenu, MDMenu + "_duel_ended_player_surrender", new TextObject("{=molochsduels_menu_player_surrender}Surrender").ToString(), player_surrenders_option_condition, player_surrenders_consequence, false, -1, false);
            campaignGameStarter.AddGameMenuOption(MDMenu, MDMenu + "_set_horseback", "{=!}{DUEL_HORSEBACK_ON_OFF}", horseback_option_condition, horseback_on_off_consequence, false, -1, false);
            MBTextManager.SetTextVariable("DUEL_HORSEBACK_ON_OFF", new TextObject("{=molochsduels_menu_duel_horseback_off}Duel on horseback: OFF"), false);
            campaignGameStarter.AddGameMenuOption(MDMenu, MDMenu + "_exit_to_encounter", new TextObject("{=molochsduels_menu_go_to_encouter}Go to battle").ToString(), leave_duel_menu_to_encounter_option_condition, leave_duel_menu_to_encounter_consequence, false, -1, false);
        }

        public void setDuelMissionResult(bool playerWon)
        {
            if (playerWon)
                _duelFightResult = DuelFightResultEnum.PlayerWon;
            else
                _duelFightResult = DuelFightResultEnum.PlayerLost;
        }

        private void initiate_menu(MenuCallbackArgs args)
        {
            switch (_duelFightResult)
            {
                case DuelFightResultEnum.None:
                    MBTextManager.SetTextVariable("DUEL_MENU_TEXT", new TextObject("{=molochsduels_menu_header}You make your preparations to meet your opponent in honorable combat."), false);
                    break;
                case DuelFightResultEnum.PlayerWon:
                    bool flag = _duelOpponent.HeroObject.GetHeroTraits().Honor >= 0;
                    if (!_rewardsApplied && !_isCompanionDuel)
                    {
                        if (_isFriendlyDuel)
                        {
                            PartyBase.MainParty.MobileParty.RecentEventsMorale += 2f;
                            float relationWithPlayer = _duelOpponent.HeroObject.Clan.Leader.GetRelationWithPlayer();
                            if (flag)
                            {
                                CharacterRelationManager.SetHeroRelation(Hero.MainHero, _duelOpponent.HeroObject.Clan.Leader, (int)((double)relationWithPlayer + 3.0));
                                InformationManager.DisplayMessage(new InformationMessage(string.Format(new TextObject("{=molochsduels_relationship_increased}Your relation with {0} has increased to {1}.").ToString(), _duelOpponent.HeroObject.Clan.Name, (int)((double)relationWithPlayer + 3.0))));
                            }
                            else
                            {
                                CharacterRelationManager.SetHeroRelation(Hero.MainHero, _duelOpponent.HeroObject.Clan.Leader, (int)((double)relationWithPlayer - 3.0));
                                InformationManager.DisplayMessage(new InformationMessage(string.Format(new TextObject("{=molochsduels_relationship_decreased}Your relation with {0} has decreased to {1}.").ToString(), _duelOpponent.HeroObject.Clan.Name, (int)((double)relationWithPlayer - 3.0))));
                            }
                            if (_friendlyDuelWager > 0)
                            {
                                Hero.MainHero.ChangeHeroGold(_friendlyDuelWager);
                                _duelOpponent.HeroObject.ChangeHeroGold(_friendlyDuelWager * -1);
                                InformationManager.DisplayMessage(new InformationMessage(string.Format(new TextObject("{=molochsduels_received_gold}You have received {0} denars.").ToString(), _friendlyDuelWager.ToString())));
                            }
                        }
                        else
                        {
                            PartyBase.MainParty.MobileParty.RecentEventsMorale += 5f;
                            PlayerEncounter.EncounteredMobileParty.RecentEventsMorale -= 5f;
                        }
                    }
                    if (flag || !_surrenderDemand)
                        MBTextManager.SetTextVariable("DUEL_MENU_TEXT", new TextObject("{=molochsduels_menu_duel_won}You have won the duel!"), false);
                    else if (!flag && _surrenderDemand)
                        MBTextManager.SetTextVariable("DUEL_MENU_TEXT", new TextObject("{=molochsduels_menu_duel_won_enemy_refuses_surrender}You have won the duel! However, the dishonorable lord refuses to surrender!"), false);
                    _rewardsApplied = true;
                    break;
                case DuelFightResultEnum.PlayerLost:
                    MBTextManager.SetTextVariable("DUEL_MENU_TEXT", new TextObject("{=molochsduels_menu_duel_lost}You have lost the duel!"), false);
                    if (!_rewardsApplied && !_isCompanionDuel)
                    {
                        if (_isFriendlyDuel)
                        {
                            PartyBase.MainParty.MobileParty.RecentEventsMorale -= 2f;
                            if (_friendlyDuelWager > 0)
                            {
                                Hero.MainHero.ChangeHeroGold(_friendlyDuelWager * -1);
                                _duelOpponent.HeroObject.ChangeHeroGold(_friendlyDuelWager);
                                InformationManager.DisplayMessage(new InformationMessage(string.Format(new TextObject("{=molochsduels_lost_gold}You have lost {0} denars.").ToString(), _friendlyDuelWager.ToString())));
                            }
                        }
                        else
                        {
                            PartyBase.MainParty.MobileParty.RecentEventsMorale -= 5f;
                            PlayerEncounter.EncounteredMobileParty.RecentEventsMorale += 5f;
                        }
                    }
                    _rewardsApplied = true;
                    break;
            }
            if (_duelFightResult == DuelFightResultEnum.None || !_isFriendlyDuel)
                return;
            _duelsFought.Add(new KeyValuePair<CharacterObject, CampaignTime>(_duelOpponent, CampaignTime.Now));
        }

        private void SetDuelOpponent() => _duelOpponent = Campaign.Current.ConversationManager.OneToOneConversationCharacter;

        private bool can_challenge_enemy_duel()
        {
            SetDuelOpponent();
            return !PlayerEncounter.InsideSettlement && !Hero.MainHero.IsWounded && !_duelOpponent.HeroObject.IsWounded;
        }

        private bool can_challenge_friendly_duel()
        {
            SetDuelOpponent();
            if (Hero.MainHero.IsWounded || PlayerEncounter.Current != null && PlayerEncounter.InsideSettlement && !Settlement.CurrentSettlement.IsTown || DuelOpponentIsEnemy())
                return false;
            foreach (KeyValuePair<CharacterObject, CampaignTime> keyValuePair in new List<KeyValuePair<CharacterObject, CampaignTime>>(_duelsFought))
            {
                CampaignTime campaignTime = keyValuePair.Value;
                if ((double)((CampaignTime)campaignTime).ElapsedDaysUntilNow > 3.0)
                    _duelsFought.Remove(keyValuePair);
                else if (keyValuePair.Key == _duelOpponent)
                    return false;
            }
            return _duelOpponent.HitPoints >= _duelOpponent.MaxHitPoints() * 0.800000011920929;
        }

        private bool can_challenge_companion_duel()
        {
            SetDuelOpponent();
            return (PlayerEncounter.Current == null || !PlayerEncounter.InsideSettlement || Settlement.CurrentSettlement.IsTown) && ((Hero.OneToOneConversationHero == null ? 0 : (Hero.OneToOneConversationHero.Clan == Clan.PlayerClan ? 1 : 0)) & (Hero.MainHero.IsWounded ? 0 : (!_duelOpponent.HeroObject.IsWounded ? 1 : 0))) != 0;
        }

        private bool player_can_demand_surrender() => PlayerEncounter.PlayerIsAttacker && PlayerEncounter.EncounteredMobileParty.Army == null;

        private bool player_is_attacker() => PlayerEncounter.PlayerIsAttacker;

        private bool player_is_defender() => PlayerEncounter.PlayerIsDefender;

        private bool start_duel_menu_option_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)12;
            return _duelFightResult == DuelFightResultEnum.None;
        }

        private bool leave_duel_menu_to_encounter_option_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)16;
            return DuelOpponentIsEnemy();
        }

        private bool capture_enemy_option_condition(MenuCallbackArgs args)
        {
            if (!_surrenderDemand || _isFriendlyDuel)
                return false;
            args.optionLeaveType = (GameMenuOption.LeaveType)20;
            return ((!PlayerEncounter.PlayerIsAttacker ? 0 : (_duelFightResult == DuelFightResultEnum.PlayerWon ? 1 : 0)) & (_duelOpponent.HeroObject.GetHeroTraits().Honor >= 0 ? 1 : 0)) != 0;
        }

        private bool let_enemy_go_option_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)17;
            return _isCompanionDuel || _isFriendlyDuel || PlayerEncounter.Current == null || PlayerEncounter.PlayerIsAttacker;
        }

        private bool enemy_lets_player_go_option_condition(MenuCallbackArgs args)
        {
            if (_isFriendlyDuel)
                return false;
            args.optionLeaveType = (GameMenuOption.LeaveType)16;
            return PlayerEncounter.PlayerIsDefender && _duelFightResult == DuelFightResultEnum.PlayerWon;
        }

        private bool player_surrenders_option_condition(MenuCallbackArgs args)
        {
            if (_isFriendlyDuel)
                return false;
            args.optionLeaveType = (GameMenuOption.LeaveType)20;
            return PlayerEncounter.PlayerIsDefender && _duelFightResult == DuelFightResultEnum.PlayerLost;
        }

        private bool horseback_option_condition(MenuCallbackArgs args)
        {
            if (_duelOpponent == null)
                return false;
            args.optionLeaveType = (GameMenuOption.LeaveType)2;
            if (_duelFightResult != 0)
                return false;
            int num = !CharacterObject.PlayerCharacter.HasMount() ? 0 : (_duelOpponent.HasMount() ? 1 : 0);
            if (num != 0)
                return num != 0;
            if (!_spawnBothSidesWithHorses)
                return num != 0;
            _spawnBothSidesWithHorses = false;
            MBTextManager.SetTextVariable("DUEL_HORSEBACK_ON_OFF", new TextObject("{=molochsduels_menu_duel_horseback_off}Duel on horseback: OFF"), false);
            return num != 0;
        }

        private void start_friendly_duel()
        {
            _isFriendlyDuel = true;
            OpenDuelMenu();
        }

        private void start_enemy_duel()
        {
            _isFriendlyDuel = false;
            OpenDuelMenu();
        }

        private void start_companion_duel()
        {
            _isCompanionDuel = true;
            _isFriendlyDuel = true;
            OpenDuelMenu();
        }

        private void OpenDuelMenu() => _duelStarted = true;

        private bool CharactersHaveEnoughMoney(int value) => Hero.MainHero.Gold >= value && _duelOpponent.HeroObject.Gold >= value;

        private bool characters_have_100_denars() => CharactersHaveEnoughMoney(100);

        private bool characters_have_250_denars() => CharactersHaveEnoughMoney(250);

        private bool characters_have_500_denars() => CharactersHaveEnoughMoney(500);

        private bool characters_have_1000_denars() => CharactersHaveEnoughMoney(1000);

        private bool characters_have_1500_denars() => CharactersHaveEnoughMoney(1500);

        private void SetFriendlyDuelWager(int value) => _friendlyDuelWager = value;

        private void set_100_duel_wager() => SetFriendlyDuelWager(100);

        private void set_250_duel_wager() => SetFriendlyDuelWager(250);

        private void set_500_duel_wager() => SetFriendlyDuelWager(500);

        private void set_1000_duel_wager() => SetFriendlyDuelWager(1000);

        private void set_1500_duel_wager() => SetFriendlyDuelWager(1500);

        private void start_duel_mission_consequence(MenuCallbackArgs args)
        {
            List<string> forbiddenScenes = new List<string>()
            {
                "battle_terrain_biome_030",
                "battle_terrain_biome_053",
                "battle_terrain_biome_088"
            };

            if (_duelOpponent == null)
                _duelOpponent = PlayerEncounter.EncounteredParty.LeaderHero.CharacterObject;
            _isFriendlyDuel = !DuelOpponentIsEnemy();
            string scene;
            bool isInsideSettlement;
            if (PlayerEncounter.Current != null && PlayerEncounter.InsideSettlement)
            {
                var loc = PlayerEncounter.LocationEncounter;
                Settlement currentSettlement = Settlement.CurrentSettlement;
                scene = currentSettlement.LocationComplex.GetLocationWithId("arena").GetSceneName(currentSettlement.IsTown ? currentSettlement.Town.GetWallLevel() : 1);
                isInsideSettlement = true;
            }
            else
            {
                scene = PlayerEncounter.GetBattleSceneForMapPatch(Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D));
                isInsideSettlement = false;
            }
            if (forbiddenScenes.Contains(scene))
                scene = "battle_terrain_biome_065";

            DuelMissions.OpenDuelMission(scene, _duelOpponent, _spawnBothSidesWithHorses, _isFriendlyDuel, isInsideSettlement);
        }

        private void leave_duel_menu_to_encounter_consequence(MenuCallbackArgs args)
        {
            ResetDuelResult();
            _surrenderDemand = false;
            GameMenu.SwitchToMenu("encounter");
        }

        private void capture_enemy_consequence(MenuCallbackArgs args)
        {
            ResetDuelResult();
            GameMenu.SwitchToMenu("encounter");
            float num = 10f / _playerStrengthRatio;
            CharacterObject.PlayerCharacter.HeroObject.Clan.AddRenown(num, true);
            CharacterObject.PlayerCharacter.HeroObject.AddInfluenceWithKingdom(num);
            InformationManager.DisplayMessage(new InformationMessage(string.Format(new TextObject("{=molochsduels_receive_renown_and_influence}You have gained {0} renown and influence.").ToString(), num)));
            PlayerEncounter.Battle.DoSurrender((BattleSideEnum)0);
            PlayerEncounter.Update();
        }

        private void let_enemy_go_consequence(MenuCallbackArgs args)
        {
            ResetDuelResult();
            if (Settlement.CurrentSettlement != null)
                GameMenu.SwitchToMenu("town");
            else if (PlayerEncounter.Current != null)
                PlayerEncounter.Finish(true);
            else
                GameMenu.ExitToLast();
        }

        private void enemy_lets_player_go_consequence(MenuCallbackArgs args)
        {
            ResetDuelResult();
            float num = 6f;
            CharacterObject.PlayerCharacter.HeroObject.Clan.AddRenown(5f, true);
            InformationManager.DisplayMessage(new InformationMessage(string.Format(new TextObject("{=molochsduels_receive_renown}You have gained {0} renown.").ToString(), 5f)));
            if (MobileParty.MainParty.BesiegedSettlement != null)
            {
                MobileParty.MainParty.IgnoreForHours(num);
                PlayerEncounter.Finish(true);
                GameMenu.ActivateGameMenu("menu_siege_strategies");
            }
            else
            {
                PlayerEncounter.ProtectPlayerSide(num);
                PlayerEncounter.Finish(true);
            }
        }

        private void player_surrenders_consequence(MenuCallbackArgs args)
        {
            ResetDuelResult();
            PlayerEncounter.PlayerSurrender = true;
            PlayerEncounter.Update();
        }

        private void horseback_on_off_consequence(MenuCallbackArgs args)
        {
            _spawnBothSidesWithHorses = !_spawnBothSidesWithHorses;
            if (_spawnBothSidesWithHorses)
                MBTextManager.SetTextVariable("DUEL_HORSEBACK_ON_OFF", new TextObject("{=molochsduels_menu_duel_horseback_on}Duel on horseback: ON"), false);
            else
                MBTextManager.SetTextVariable("DUEL_HORSEBACK_ON_OFF", new TextObject("{=molochsduels_menu_duel_horseback_off}Duel on horseback: OFF"), false);
            GameMenu.SwitchToMenu(MDMenu);
        }

        private void ResetDuelResult()
        {
            _duelFightResult = DuelFightResultEnum.None;
            _duelOpponent = null;
            _friendlyDuelWager = 0;
            _isCompanionDuel = false;
            _rewardsApplied = false;
        }

        private void calculate_npc_response_to_surrender_demand()
        {
            _surrenderDemand = true;
            int num = new Random().Next(1, 100);
            float successThreshold = GetSuccessThreshold(30f);
            _duelAccepted = num >= (double)successThreshold;
            if (!_duelAccepted)
                return;
            Hero.MainHero.AddSkillXp(DefaultSkills.Charm, 10f * successThreshold);
        }

        private void calculate_npc_response_to_duel()
        {
            _surrenderDemand = false;
            int num = new Random().Next(1, 100);
            float successThreshold = GetSuccessThreshold(40f);
            _duelAccepted = num >= (double)successThreshold;
            if (!_duelAccepted)
                return;
            Hero.MainHero.AddSkillXp(DefaultSkills.Charm, 10f * successThreshold);
        }

        private float GetSuccessThreshold(float baseNumber)
        {
            Hero mainHero = Hero.MainHero;
            Hero heroObject = _duelOpponent.HeroObject;
            float num1 = baseNumber;
            _playerStrengthRatio = PlayerEncounter.Current.GetPlayerStrengthRatioInEncounter();
            if (_surrenderDemand)
            {
                if (_playerStrengthRatio > 2.0)
                    num1 += 10f;
                num1 += 30f / _playerStrengthRatio;
                if (_duelOpponent.HeroObject.IsFactionLeader)
                    num1 += 20f;
                else if (_duelOpponent.HeroObject.Clan.Leader == _duelOpponent.HeroObject)
                    num1 += 10f;
            }
            float skillValue = mainHero.GetSkillValue(DefaultSkills.Charm);
            double num2 = (double)num1 - (double)skillValue / 20.0;
            CharacterTraits heroTraits = heroObject.GetHeroTraits();
            double num3 = (double)(heroTraits.Valor * 10);
            float num4 = (float)(num2 - num3) - (float)(heroTraits.Honor * 10);
            float num5 = mainHero.HitPoints / heroObject.HitPoints;
            float num6 = (float)(((double)num4 - (double)num4 * (double)num5) / 3.0);
            float num7 = num4 - num6;
            float num8 = (double)num7 <= 91.0 ? num7 : 91f;
            return (double)num8 < 11.0 ? 11f : num8;
        }

        private bool duel_accepted() => _duelAccepted;

        private bool duel_rejected() => !_duelAccepted;

        private bool DuelOpponentIsEnemy()
        {
            if (_duelOpponent == null)
                _duelOpponent = PlayerEncounter.EncounteredParty.LeaderHero.CharacterObject;
            return FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, _duelOpponent.HeroObject.MapFaction);
        }

        private enum DuelFightResultEnum
        {
            None,
            PlayerWon,
            PlayerLost,
        }
    }
}
