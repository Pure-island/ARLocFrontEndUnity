using DA_Assets.FCU;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;

namespace DA_Assets.Shared
{
    public static class DALogger
    {
        public static string redColor = "#ff6e40";
        public static string blackColor = "black";
        public static string whiteColor = "white";
        public static string violetColor = "#8b00ff";
        public static string orangeColor = "#ffa500";

        public static void LogException(Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }

        public static void LogError(string log)
        {
            log = log.SubstringSafe(15000);
            UnityEngine.Debug.LogError(log);
        }

        public static void LogWarning(string log)
        {
            log = log.SubstringSafe(15000);
            UnityEngine.Debug.LogWarning(log.TextColor(orangeColor).TextBold());
        }

        public static void Log(string log)
        {
            log = log.SubstringSafe(15000);
            string color = whiteColor;
#if UNITY_EDITOR
            color = UnityEditor.EditorGUIUtility.isProSkin ? whiteColor : blackColor;
#endif
            UnityEngine.Debug.Log(log.TextColor(color).TextBold());
        }

        public static void LogSuccess(string log)
        {
            UnityEngine.Debug.Log(log.TextColor(violetColor).TextBold());
        }

        public static bool WriteLogBeforeEqual(ref int count1, ref int count2, string log, ref int tempCount)
        {
            if (count1 != count2)
            {
                if (tempCount != count1)
                {
                    tempCount = count1;
                    Log(log);
                }

                return true;
            }

            if (tempCount != count2)
            {
                Log(log);
            }

            return false;
        }

        public static bool WriteLogBeforeApiTimeout(ref int requestCount, ref int remainingTime, string log)
        {
            if (requestCount != 0 && requestCount % FcuConfig.Instance.ApiRequestsCountLimit == 0)
            {
                if (remainingTime > 0)
                {
                    Log(log);
                    remainingTime -= 10;
                }
                else if (remainingTime == 0)
                {
                    Log(log);
                }
            }

            return remainingTime > 0;
        }

        public static bool WriteLogBeforeEqual(ICollection list1, ICollection list2, FcuLocKey locKey, int count1, int count2, ref int tempCount)
        {
            if (list1.Count != list2.Count)
            {
                if (tempCount != list1.Count)
                {
                    tempCount = list1.Count;
                    Log(locKey.Localize(count1, count2));
                }

                return true;
            }

            if (tempCount != list2.Count)
            {
                Log(locKey.Localize(count1, count2));
            }

            return false;
        }

        public static bool WriteLogBeforeEqual(ICollection list1, ICollection list2, FcuLocKey locKey, ref int tempCount)
        {
            if (list1.Count != list2.Count)
            {
                if (tempCount != list1.Count)
                {
                    tempCount = list1.Count;
                    Log(locKey.Localize(list1.Count, list2.Count));
                }

                return true;
            }

            if (tempCount != list2.Count)
            {
                Log(locKey.Localize(list1.Count, list2.Count));
            }

            return false;
        }
    }
}