#region File Description
//-----------------------------------------------------------------------------
// AudioManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
#endregion

namespace LightSavers.Components
{
    /// <summary>
    /// Component that manages audio playback for all sounds.
    /// </summary>
    public class AudioManager : GameComponent
    {
      

        #region Volume settings
        public float
            masterVolume = 1.0f,
            musicVolume = 1.0f,

            //Weapons volumes
            pistol = 0.1f,
            sniper = 0.5f,
            shottie = 0.6f,
            assault = 0.3f,
            sword = 0.3f,

            //entity volumes
            alienDeath1 = 0.8f;
        #endregion

        #region Fields
        bool inGame = false;

        //Game Sounds
        Dictionary<string, SEffectInstanceManager> gameSounds;

        //Menu Sounds
        Dictionary<string, SoundEffectInstance> menuSoundS;
        SEffectInstanceManager menuMusic;        
        #endregion

        #region Initialization
        public AudioManager(Game game)
            : base(game)
        {

            menuSoundS = new Dictionary<string, SoundEffectInstance>();
            gameSounds = new Dictionary<string, SEffectInstanceManager>();

            game.Components.Add(this);
        }

        #endregion


        /// <summary>
        /// Play a new instance of a sound effect
        /// </summary>
        /// <param name="effectName eg. pistol"></param>
        /// <param name="volume"></param>
        public void PlayGameSound(string effectName)
        {
            gameSounds[effectName].playSound();
        }

        public void LoadGameSound(SoundEffect SEffect, string effectName, int instances, float volume)
        {
            gameSounds.Add(effectName, new SEffectInstanceManager(SEffect, instances, volume));
        }

        public void SwitchToGame()
        {
            menuMusic.Pause();
            foreach (SEffectInstanceManager sounds in gameSounds.Values)
                sounds.Resume();
        }

        public void SwitchToMenu()
        {
            foreach (SEffectInstanceManager sounds in gameSounds.Values)
                sounds.Pause();
            menuMusic.Resume();
        }


        #region Loading Methodes

        public void LoadMenuSound(string path, string alias)
        {
            SoundEffect soundEffect = Game.Content.Load<SoundEffect>(path);
            SoundEffectInstance soundEffectInstance = soundEffect.CreateInstance();

            if (!menuSoundS.ContainsKey(alias))
            {
                menuSoundS.Add(alias, soundEffectInstance);
            }
        }

        public void LoadMenuMusic(string path)
        {
            menuMusic = new SEffectInstanceManager(Game.Content.Load<SoundEffect>(path), 1, 1.0f);
        }

        public void PlayMenuMusic()
        {
            menuMusic.playSound();
        }

        /// <summary>
        /// Stops the currently playing music.
        /// </summary>
        public void StopMusic()
        {
            menuMusic.Pause();
        }

        #endregion

        #region Instance Disposal Methods
        ///// <summary>
        ///// Clean up the component when it is disposing.
        ///// </summary>
        //protected override void Dispose(bool disposing)
        //{
        //    try
        //    {
        //        if (disposing)
        //        {
        //            foreach (var item in soundBank)
        //            {
        //                item.Value.Dispose();
        //            }
        //            soundBank.Clear();
        //            soundBank = null;
        //        }
        //    }
        //    finally
        //    {
        //        base.Dispose(disposing);
        //    }
        //}


        #endregion
    }

}