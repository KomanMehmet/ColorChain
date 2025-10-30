#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _Project.Editor
{
    public class FolderStructureCreator
    {
        [MenuItem("Tools/ColorChain/Setup Folder Structure")]
        public static void CreateFolderStructure()
        {
            string basePath = "Assets/_Project";

            string[] folders  = new string[]
            {
                "AddressableAssets/Prefabs/Gameplay/Balls",
                "AddressableAssets/Prefabs/Gameplay/PowerUps",
                "AddressableAssets/Prefabs/Gameplay/Effects",
                "AddressableAssets/Prefabs/UI",
                "AddressableAssets/Audio/Music",
                "AddressableAssets/Audio/SFX",
                "AddressableAssets/Visual/Sprites",
                "AddressableAssets/Visual/Materials",

                // Resources
                "Resources/Data/Balls",
                "Resources/Data/Levels",
                "Resources/Data/PowerUps",
                "Resources/Events/Gameplay",
                "Resources/Events/UI",
                "Resources/Events/Audio",

                // Scripts
                "Scripts/Core/EventChannels",
                "Scripts/Core/Interfaces",
                "Scripts/Core/Patterns",
                "Scripts/Core/Extensions",
                "Scripts/Data",
                "Scripts/Systems/Grid",
                "Scripts/Systems/Input",
                "Scripts/Systems/Match",
                "Scripts/Systems/Level",
                "Scripts/Systems/Score",
                "Scripts/Systems/Audio",
                "Scripts/Gameplay/Ball",
                "Scripts/Gameplay/PowerUps",
                "Scripts/Gameplay/Effects",
                "Scripts/UI/MainMenu",
                "Scripts/UI/GameHUD",
                "Scripts/UI/LevelComplete",

                // Scenes
                "Scenes/_Development",
                "Scenes/_Production",
            };

            foreach (string folder in folders)
            {
                string path = Path.Combine(basePath, folder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("Folder Structure Created");
        }
    }
}
#endif