using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System;

namespace ValleyStardew.Engine {
    public static class SoundManager {
        public static SoundEffect DirtWork, PlantSeed, HandCollect, ClickSound;
        public static SoundEffect[] WalkSounds;
        public static Song BackgroundMusic;

        private static Random _rand = new Random();

        public static void LoadContent(ContentManager content) {
            // Завантажуємо музику (Song)
            BackgroundMusic = content.Load<Song>("Overworld");

            // Завантажуємо звуки (SoundEffect)
            DirtWork = content.Load<SoundEffect>("Dirt_work");
            PlantSeed = content.Load<SoundEffect>("Plant_Seeds");
            HandCollect = content.Load<SoundEffect>("Hand_Collect");
            ClickSound = content.Load<SoundEffect>("Click_Sound");

            // Завантажуємо 6 звуків ходьби в масив
            WalkSounds = new SoundEffect[6];
            for (int i = 1; i <= 6; i++) {
                WalkSounds[i - 1] = content.Load<SoundEffect>($"Walk{i}");
            }
        }

        // Метод для випадкового звуку кроку
        public static void PlayWalk() {
            int index = _rand.Next(0, WalkSounds.Length);
            // Відтворюємо звук. Параметри: гучність (0.5f), висота тону (0f), панорамування (0f)
            WalkSounds[index].Play(0.5f, 0f, 0f);
        }
    }
}