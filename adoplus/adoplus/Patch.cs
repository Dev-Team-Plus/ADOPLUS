using ADOFAI;
using adoplus.CustomEvent;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using adoplus.ffx;
using DG.Tweening;

namespace adoplus
{
    public static class Patch
    {
        [HarmonyPatch(typeof(ADOStartup), "SetupLevelEventsInfo")]
        public static class StartUpPatch
        {//<Startup>g__LoadLevelEventSprites|2_10
            public static void Postfix()
            {
                UnityEngine.Debug.Log("샌즈");
                CustomEventManager.RegisterNewEvent();
            }
        }

        [HarmonyPatch(typeof(ADOStartup), "<Startup>g__LoadLevelEventSprites|2_10")]
        public static class SpritePatch
        {//<Startup>g__LoadLevelEventSprites|2_10
            public static void Postfix()
            {
                CustomEventManager.RegisterCustomSprite();
            }
        }

        [HarmonyPatch(typeof(Type), "GetType", new Type[]
        {
            typeof(string)
        })]
        public static class GetTypePatch
        {
            // Token: 0x06000032 RID: 50 RVA: 0x00003978 File Offset: 0x00001B78
            public static bool Prefix(ref string typeName, ref Type __result)
            {
                bool flag = typeName.Contains("Plus.");
                bool result;
                if (flag)
                {
                    typeName = typeName.Replace("Plus.", string.Empty);
                    __result = Type.GetType(typeName);
                    result = false;
                }
                else
                {
                    result = true;
                }
                return result;
            }
        }

        [HarmonyPatch(typeof(scnGame), "Play")]
        public static class StopPatch
        {
            public static void Prefix()
            {
                ffxPlusPlaySong.LastAudio = ADOBase.conductor.song;
                ffxPlusPlaySong.ConductorVolume = ADOBase.conductor.song.volume;
                foreach (var floor in ADOBase.lm.listFloors)
                {
                    if (floor.TryGetComponent(out AudioSource a))
                    {
                        a.Stop();
                        a.volume = 0;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(scnGame), "ApplyEventsToFloors", new Type[]
        {
            typeof(List<scrFloor>),
            typeof(LevelData),
            typeof(scrLevelMaker),
            typeof(List<LevelEvent>)
        })]
        public static class ApplyEventsToFloorsPatch
        {
            public static void Prefix(ref List<scrFloor> floors, ref LevelData levelData, ref scrLevelMaker lm, ref List<LevelEvent> events)
            {
                ffxPlusPlaySong.CustomSongs = new List<AudioSource>();
            }
        }

        [HarmonyPatch(typeof(scrController), "FailAction")]
        public static class FailActionPatch
        {
            public static void Postfix(ref bool overload, ref bool multipress, ref string failMessage, ref bool hitbox)
            {
                foreach (var floor in ADOBase.lm.listFloors)
                {
                    if (floor.TryGetComponent(out AudioSource a))
                    {
                        a.Stop();
                        a.volume = 0;
                    }
                }
            }
        }

        //이벤트 패치
        [HarmonyPatch(typeof(scnGame), "ApplyEvent")]
        public static class ApplyCoreLevelEvent
        {
            public static bool Prefix(ref LevelEvent evnt, ref float bpm, ref float pitch, ref List<scrFloor> floors, ref float offset, ref int? customFloorID, ref ffxPlusBase __result)
            {
                int num = customFloorID ?? evnt.floor;
                scrFloor scrFloor = floors[num];
                GameObject floor = scrFloor.gameObject;
                Dictionary<string, object>.KeyCollection keys = evnt.data.Keys;
                ffxPlusBase ffxPlusBase = null;
                LevelEventType eventType = evnt.eventType;
                switch (eventType)
                {
                    case (LevelEventType)CustomEventManager.PlaySongEventIndex:
                        ffxPlusPlaySong ffxPlaySong = floor.AddComponent<ffxPlusPlaySong>();
                        ffxPlusBase = ffxPlaySong;

                        ffxPlaySong.artistName = evnt.GetString("artistName");
                        ffxPlaySong.song = evnt.GetString("song");
                        ffxPlaySong.audioFilename = evnt.GetString("audioFilename");
                        ffxPlaySong.volume = evnt.GetInt("volume");
                        ffxPlaySong.startPosition = evnt.GetFloat("startPosition");
                        ffxPlaySong.fadeIn = evnt.GetInt("fadeIn");
                        ffxPlaySong.fadeOut = evnt.GetInt("fadeOut");
                        ffxPlaySong.pitch = evnt.GetInt("pitch");
                        ffxPlaySong.fadeType = (FadeType)Enum.Parse(typeof(FadeType), evnt.data["fadeType"].ToString());
                        ffxPlaySong.ease = RDUtils.ParseEnum<Ease>(evnt.data["ease"].ToString(), Ease.Linear);
                        

                        if (ffxPlusPlaySong.CustomSongs.Count == 0)
                            ffxPlaySong.PreAudio = ADOBase.conductor.song;
                        else
                            ffxPlaySong.PreAudio = ffxPlusPlaySong.CustomSongs[ffxPlusPlaySong.CustomSongs.Count-1];

                        ffxPlaySong.LoadSong();
                        break;
                    default:
                        break;
                }

                if (ffxPlusBase != null)
                {
                    floors[num].plusEffects.Add(ffxPlusBase);
                    ffxPlusBase.SetStartTime(bpm, evnt.GetFloat("angleOffset") + offset);

                    __result = ffxPlusBase;
                    return false;
                }

                return true;
            }
        }
    }
}
