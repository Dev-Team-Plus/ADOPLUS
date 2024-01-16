using ADOFAI;
using ERDPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace adoplus.CustomEvent
{
    public static class CustomEventManager
    {
        public static Dictionary<string, int> CustomEventData = new Dictionary<string, int>();

        public const int PlaySongEventIndex = 999;

        public static void RegisterNewEvent()
        {
            var playSong = RegisterLevelEventInfo("PlaySong", false, LevelEventExecutionTime.OnBar, PlaySongEventIndex, new List<LevelEventCategory> { LevelEventCategory.FxModifiers });
            playSong.RegisterFileProperty(name: "audioFilename", type: "File", fileType: "Audio", key: "editor.adofaiplus.audioFilename", defaultValue: "");
            playSong.RegisterEnumProperty(name: "specialArtistType", type: "Enum:SpecialArtistType", key: "editor.specialArtistType", defaultValue: "None");
            playSong.RegisterFileProperty(name: "artistPermission", type: "File", fileType: "Image", key: "editor.artistPermission", defaultValue: "");
            playSong.RegisterInputProperty(name:"artistName",type:"String",unit:"", key: "editor.adofaiplus.artistName", placeHolder:"작곡가", defaultValue: "Plum");
            playSong.RegisterInputProperty(name: "song", type: "String", unit: "", key: "editor.song", placeHolder: "곡", defaultValue: "Megalovania");
            playSong.RegisterInputProperty(name: "volume", type: "Int", unit: "%", key: "editor.volume", placeHolder: "볼륨", defaultValue: 100, min:0, max:100);
            playSong.RegisterInputProperty(name: "startPosition", type: "Float", unit: "seconds", key: "editor.adofaiplus.startposition", placeHolder: "오프셋", defaultValue: 0, min:-10000, max:300000);
            playSong.RegisterInputProperty(name: "pitch", type: "Int", unit: "%", key: "editor.pitch", placeHolder: "피치", defaultValue: 100, min: 1, max: 1000);
            playSong.RegisterEnumProperty(name: "fadeType", type: "Enum:Plus.FadeType", key: "editor.adofaiplus.fadetype", defaultValue: "Sequence");
            playSong.RegisterInputProperty(name: "fadeIn", type: "Int", unit: "seconds", key: "editor.adofaiplus.fadein", placeHolder: "패이드인 기간", defaultValue: 1, min: 0, max: 30);
            playSong.RegisterInputProperty(name: "fadeOut", type: "Int", unit: "seconds", key: "editor.adofaiplus.fadeout", placeHolder: "패이드아웃 기간", defaultValue: 1, min: 0, max: 30);
            playSong.RegisterEnumProperty(name: "ease", type: "Enum:Ease", key: "editor.adofaiplus.fadeEase", defaultValue: "Linear");

            RDStringPatcher.AddString("editor.PlaySong", "<color=#00ffffff>곡 재생</color>");
            RDStringPatcher.AddString("editor.adofaiplus.artistName", "작곡가");
            RDStringPatcher.AddString("editor.adofaiplus.fadein", "페이드 인 기간");
            RDStringPatcher.AddString("editor.adofaiplus.fadeout", "페이드 아웃 기간");
            RDStringPatcher.AddString("editor.adofaiplus.audioFilename", "오디오 파일");
            RDStringPatcher.AddString("editor.adofaiplus.startposition", "시작 위치");
            RDStringPatcher.AddString("editor.adofaiplus.fadetype", "페이드 종류");
            RDStringPatcher.AddString("editor.adofaiplus.fadeEase", "페이드 가감속");
            RDStringPatcher.AddString("enum.common.SameTime", "동시에");
            RDStringPatcher.AddString("enum.common.Sequence", "순차적");
        }

        static LevelEventInfo RegisterLevelEventInfo(string name, bool allowFirstFloor, LevelEventExecutionTime timeBar, int index, List<LevelEventCategory> categories)
        {
            var customEvent = new LevelEventInfo();
            customEvent.categories = categories;
            customEvent.allowFirstFloor = allowFirstFloor;
            customEvent.name = name;
            customEvent.executionTime = timeBar;
            customEvent.type = (LevelEventType)index;
            customEvent.propertiesInfo = new Dictionary<string, PropertyInfo>();

            EnumPatcher<LevelEventType>.AddField(name, (ulong)index);
            GCS.levelEventsInfo.Add(name, customEvent);
            GCS.levelEventTypeString.Add((LevelEventType)index, name);

            CustomEventData.Add(name, index);
            return customEvent;
        }

       

        //Int, Float, String
        static PropertyInfo RegisterInputProperty(this LevelEventInfo info, string name, string type, string unit, string key, string placeHolder, object defaultValue, float min = 0, float max = 1000)
        {
            var propertyDic = new Dictionary<string, object>()
            {
                {"name", name},
                {"type", type},
                {"key",  key},
                {"default", defaultValue},
                {"placeholder", placeHolder},
                {"min", min},
                {"max", max},
                {"unit", unit }
            };

            var propertyInfo = new PropertyInfo(propertyDic, info);
            info.propertiesInfo.Add(propertyInfo.name, propertyInfo);
            return propertyInfo;
        }

        //File
        static PropertyInfo RegisterFileProperty(this LevelEventInfo info, string name, string type, string fileType, string key, string defaultValue)
        {
            var propertyDic = new Dictionary<string, object>()
            {
                {"name", name},
                {"type", type},
                {"key",  key},
                {"default", defaultValue},
                {"fileType", fileType}
            };

            var propertyInfo = new PropertyInfo(propertyDic, info);
            info.propertiesInfo.Add(propertyInfo.name, propertyInfo);
            return propertyInfo;
        }

        //Enum
        static PropertyInfo RegisterEnumProperty(this LevelEventInfo info, string name, string type, string key, string defaultValue)
        {
            var propertyDic = new Dictionary<string, object>()
            {
                {"name", name},
                {"type", type},
                {"key",  key},
                {"default", defaultValue}
            };

            var propertyInfo = new PropertyInfo(propertyDic, info);
            info.propertiesInfo.Add(propertyInfo.name, propertyInfo);
            return propertyInfo;
        }

        //Bool
        static PropertyInfo RegisterBoolProperty(this LevelEventInfo info, string name, string type, string key, object defaultValue)
        {
            var propertyDic = new Dictionary<string, object>()
            {
                {"name", name},
                {"type", type},
                {"key",  key},
                {"default", defaultValue}
            };

            var propertyInfo = new PropertyInfo(propertyDic, info);
            info.propertiesInfo.Add(propertyInfo.name, propertyInfo);
            return propertyInfo;
        }

        public static void RegisterCustomSprite()
        {
            foreach (var customEvent in CustomEventData)
            {
                GCS.levelEventIcons.Add((LevelEventType)customEvent.Value, Main.Test);
            }
        }
    }
}

