using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardOfWor
{
    public class MusicManager
    {
        private const float TEMPO_1 = 30f;
        private const float TEMPO_2 = 40f;
        private const float TEMPO_3 = 60f;
        private const float TEMPO_4 = 120f;
        private const float TEMPO_5 = 300f;

        private bool _isMusicPlaying;
        private float _currentTempoBPS;
        private float _currentMusiqueTime;
        private SoundEffect[] _musicNoteSounds;
        private SoundEffectInstance[] _musicNotesInstances;
        private int _currentMusicNote;
        private float[] _tempos;

        private SoundEffect _worlukIntro;
        private SoundEffectInstance _worlukIntroInstance;
        private SoundEffect _worlukLoop;
        private SoundEffectInstance _worlukLoopInstance;

        private bool _isBossMusic;
        private bool _isBossIntroPlaying;

        public MusicManager()
        {
            _tempos = new float[5];
            _tempos[0] = TEMPO_1;
            _tempos[1] = TEMPO_2;
            _tempos[2] = TEMPO_3;
            _tempos[3] = TEMPO_4;
            _tempos[4] = TEMPO_5;
        }

        public void LoadMusicSounds(ContentManager content)
        {
            _musicNoteSounds = new SoundEffect[2];
            _musicNoteSounds[0] = content.Load<SoundEffect>("C-long");
            _musicNoteSounds[1] = content.Load<SoundEffect>("G#-long");
            _musicNotesInstances = new SoundEffectInstance[2];
            _musicNotesInstances[0] = _musicNoteSounds[0].CreateInstance();
            _musicNotesInstances[1] = _musicNoteSounds[1].CreateInstance();

            _worlukIntro = content.Load<SoundEffect>("worluk-intro");
            _worlukIntroInstance = _worlukIntro.CreateInstance();
            _worlukLoop = content.Load<SoundEffect>("worluk-loop");
            _worlukLoopInstance = _worlukLoop.CreateInstance();
            _worlukLoopInstance.IsLooped = true;
        }

        private void SetTempo(float tempoBPM)
        {
            _currentTempoBPS = tempoBPM / 60;
        }

        public void StartMusic(float tempo)
        {
            _isBossMusic = false;
            SetTempo(tempo);
            _currentMusiqueTime = 0;
            _currentMusicNote = 0;
            _musicNotesInstances[_currentMusicNote].Play();
            _isMusicPlaying = true;
        }

        public void StopMusic()
        {
            _isMusicPlaying = false;
            _isBossMusic = false;
            _isBossIntroPlaying = false;
            _musicNotesInstances[_currentMusicNote].Stop();
            _worlukIntroInstance.Stop();
            _worlukLoopInstance.Stop();
        }

        public void StartBossMusic()
        {
            _isBossMusic = true;
            _isBossIntroPlaying = true;
            _worlukIntroInstance.Play();
            _isMusicPlaying = true;
            _currentMusiqueTime = 0;
        }

        public void Update(float deltaTime, int levelThreshold)
        {
            if (_isMusicPlaying)
            {
                _currentMusiqueTime += deltaTime;
                if (_isBossMusic)
                {
                    if (_isBossIntroPlaying && _currentMusiqueTime >= _worlukIntro.Duration.TotalSeconds)
                    {
                        _worlukIntroInstance.Stop();
                        _worlukLoopInstance.Play();
                        _isBossIntroPlaying = false;
                    }
                }
                else
                {
                    float previousTempo = _currentTempoBPS;
                    SetTempo(_tempos[levelThreshold]);
                    if (_currentMusiqueTime > 1 / _currentTempoBPS)
                    {
                        _musicNotesInstances[_currentMusicNote].Stop();
                        _currentMusicNote = 1 - _currentMusicNote;
                        _musicNotesInstances[_currentMusicNote].Play();
                        if (previousTempo != _currentTempoBPS)
                        {
                            _currentMusiqueTime = 0;
                        }
                        else
                        {
                            _currentMusiqueTime = _currentMusiqueTime - 1f / _currentTempoBPS;
                        }
                    }
                }
            }
        }
    }
}
