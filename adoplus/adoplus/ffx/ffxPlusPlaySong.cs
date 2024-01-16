using DG.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace adoplus.ffx
{
    public class ffxPlusPlaySong : ffxPlusBase
    {
        public string audioFilename;
        public string artistName;
        public string song;
        public int volume;
        public float startPosition;
        public int pitch;
        public int fadeIn; 
        public int fadeOut;
        public FadeType fadeType;

        public AudioSource audio;

        public static Tween LastFadeOut;
        public static Tween LastFadeIn;

        public AudioSource PreAudio;

        public static AudioSource LastAudio;

        public static float ConductorVolume;

        public static List<AudioSource> CustomSongs;

        public override void Awake()
        {
            base.Awake();
        }

        public void LoadSong()
        {
            string directoryName = Path.GetDirectoryName(ADOBase.levelPath);
            string path = Path.Combine(directoryName, audioFilename);

            if (TryGetComponent(out AudioSource a))
                audio = a;
            else
                audio = gameObject.AddComponent<AudioSource>();

            audioManager.StartCoroutine(audioManager.FindOrLoadAudioClipExternal(path, false));
            CustomSongs.Add(audio);
        }

        void LateUpdate()
        {
            //중간에 시작하면 Conductor.song 볼륨이 이벤트 실행 후 바뀜
            //해결하기 위해 업데이트에다가 넣었는데
            //누가 개선해주세요
            conductor.song.volume = ConductorVolume;
        }


        public override void StartEffect()
        {
            controller.txtLevelName.text = (!string.IsNullOrEmpty(artistName) && !string.IsNullOrEmpty(song)) ? $"{artistName} - {song}" : "";

            var time = (float)(conductor.dspTime - conductor.dspTimeSongPosZero);


            audio.clip = audioManager.FindOrLoadAudioClip(audioFilename + "*external");
            audio.pitch = pitch * 0.01f;
            audio.Play();
            audio.time = (float)(time - startTime + startPosition);
            
            audio.volume = 0;
            LastFadeOut?.Kill();
            LastFadeIn?.Kill();

            if(fadeType == FadeType.SameTime)
            {
                LastFadeOut =  DOVirtual.Float(PreAudio.volume, 0, fadeOut, (x) => 
                {
                    PreAudio.volume = x;
                    if (IsCon())
                        ConductorVolume = x;
                }).SetEase(ease).OnComplete(delegate
                {
                    PreAudio.volume = 0;
                });
                LastFadeIn = DOVirtual.Float(audio.volume, volume * 0.01f, fadeIn, (x) => { audio.volume = x;}).SetEase(ease).OnComplete(delegate
                {
                    audio.volume = volume * 0.01f;
                });
            }
            else
            {
                LastFadeOut = DOVirtual.Float(PreAudio.volume, 0, fadeOut, (x) => 
                {
                    PreAudio.volume = x;
                    if (IsCon())
                        ConductorVolume = x;
                }).SetEase(ease).OnComplete(delegate
                {
                    PreAudio.volume = 0;
                    LastFadeIn = DOVirtual.Float(audio.volume, volume * 0.01f, fadeIn, (x) => { audio.volume = x; }).OnComplete(delegate
                    {
                        audio.volume = volume * 0.01f;

                    }).SetEase(ease);
                });
                
            }

            

            ScrubTime2(time, LastFadeOut);
            ScrubTime2(time, LastFadeIn);
        }
    
        public void ScrubTime2(float t, Tween tween)
        {
            if (t < startTime)
                return;

            TweenExtensions.Goto(tween, (float)(t - startTime), true);
        }

        bool IsCon()
        {
            return PreAudio == cond.song;
        }
    }
}
