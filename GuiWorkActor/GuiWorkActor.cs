﻿using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace GuiWorkActor
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public bool open = true; //使用鬼的仓库
        public int numberOfColumns = 4;
        public float scrollSpeed = 10;
    }
    public static class Main
    {
        public static bool onOpen = false;//
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            #region 基础设置
            settings = Settings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            return true;
        }

        static string title = "鬼的工作间";
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(title, GUILayout.Width(300));
            // Main.settings.open = GUILayout.Toggle(Main.settings.open, "使用鬼的工作间");
        }



        [HarmonyPatch(typeof(HomeSystem), "GetActor")]
        public static class HomeSystem_GetActor_Patch
        {
            public static NewWorkActor workActor;
            public static GameObject listActorsHolder;
            public static bool Prefix(int _skillTyp, bool favorChange = false)
            {

                Main.Logger.Log("获取行动者" + _skillTyp.ToString() + " " + favorChange.ToString());
                if (workActor == null)
                {
                    listActorsHolder = HomeSystem.instance.listActorsHolder.gameObject;
                    //listActorsHolder.SetActive(false);
                    workActor = HomeSystem.instance.listActorsHolder.parent.parent.gameObject.AddComponent<NewWorkActor>();
                    workActor.Init(_skillTyp, favorChange);
                }

                List<int> newActorList = new List<int>();
                Dictionary<int, int> dictionary = new Dictionary<int, int>();
                List<int> list = new List<int>(DateFile.instance.GetGangActor(16, 9));
                foreach (int num in list)
                {
                    dictionary.Add(num, int.Parse(DateFile.instance.GetActorDate(num, _skillTyp, true)));
                }
                List<KeyValuePair<int, int>> list2 = new List<KeyValuePair<int, int>>(dictionary);
                list2.Sort((KeyValuePair<int, int> s1, KeyValuePair<int, int> s2) => (!favorChange) ? s2.Value.CompareTo(s1.Value) : s1.Value.CompareTo(s2.Value));
                foreach (KeyValuePair<int, int> keyValuePair in list2)
                {
                    newActorList.Add(keyValuePair.Key);
                }

                workActor.data = newActorList.ToArray();

                // 这段循环设置数据，应该挪到SetItemCell
                //for (int i = 0; i < newActorList.Count; i++)
                //{
                //    int num2 = newActorList[i];
                //    if (!DateFile.instance.GetFamily(true, true).Contains(num2) && int.Parse(DateFile.instance.GetActorDate(num2, 11, false)) > 14)
                //    {
                //        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(HomeSystem.instance.listActor, Vector3.zero, Quaternion.identity);
                //        gameObject.name = "Actor," + num2;
                //        gameObject.transform.SetParent(HomeSystem.instance.listActorsHolder, false);
                //        gameObject.GetComponent<Toggle>().group = HomeSystem.instance.listActorsHolder.GetComponent<ToggleGroup>();
                //        gameObject.GetComponent<SetWorkActorIcon>().SetActor(num2, _skillTyp, favorChange);
                //    }
                //}


                return false;
            }
        }

    }
}