using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace FreshImagePrompts
{
    /// <summary>
    /// Картинки по свежему описанию.
    ///
    /// Ошибка игры. Если язык сценария не английский, картинку сущности рисуют по английскому
    /// переводу её описания. Перевод делается один раз и запоминается в поле
    /// GameEntity.enStrForImgGen (метод GameEntity.GetOrForceGetEnStrForImgGen переводит, только
    /// если поле пустое). Сбрасывает поле только смена имени через окно правки, а смена описания
    /// (ИИ обновил внешность, игрок поправил руками) — нет. Поэтому после превращения героя
    /// картинки по-прежнему рисуются по старой внешности.
    ///
    /// Что делаем. Перед каждым обращением к запомненному переводу сравниваем «отпечаток»
    /// сущности (имя + описание + статусы, ровно то, что игра переводит) с тем, по которому
    /// перевод был сделан. Не совпал — стираем поле, и игра сама переводит заново.
    /// Переводы, сделанные до запуска игры, мы не видели, поэтому каждый из них один раз за
    /// сессию переводится заново (можно выключить в настройках).
    ///
    /// Всё ищется по имени через рефлексию: если игра обновится и что-то переименуют, плагин
    /// напишет об этом в лог и ничего не тронет.
    /// </summary>
    [BepInPlugin(Guid, "Свежие описания для картинок", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "shantargon.airoguelite.freshimageprompts";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> CfgEnabled;
        internal static ConfigEntry<bool> CfgRefreshOld;
        internal static ConfigEntry<bool> CfgVerbose;

        private static MethodInfo _asNonCpn, _prettyName, _descNoAsync, _optVisual;
        private static FieldInfo _enStr;

        // сущность -> отпечаток, по которому сделан её текущий перевод
        private static readonly ConditionalWeakTable<object, string> Seen = new ConditionalWeakTable<object, string>();

        private void Awake()
        {
            Log = Logger;
            CfgEnabled = Config.Bind("Основное", "Включено", true,
                "Перед рисованием картинки переводить описание заново, если оно изменилось.");
            CfgRefreshOld = Config.Bind("Основное", "Обновлять старые переводы", true,
                "Переводы, сделанные до запуска игры, один раз за сессию переводить заново. " +
                "Так исправляются и уже застрявшие старые описания.");
            CfgVerbose = Config.Bind("Отладка", "Подробный лог", false, "Писать в лог каждый сброс перевода.");

            Type ge = AccessTools.TypeByName("GameEntity");
            if (ge == null) { Log.LogError("Не найден класс GameEntity — плагин выключен."); return; }

            _asNonCpn = AccessTools.Method(ge, "AsNonCpnGe", Type.EmptyTypes);
            _prettyName = AccessTools.Method(ge, "GetPrettyName", Type.EmptyTypes);
            _descNoAsync = AccessTools.Method(ge, "GetStatusInterjectedDescNoAsync", Type.EmptyTypes);
            _optVisual = AccessTools.Method(ge, "OptVisualDesc", Type.EmptyTypes);
            _enStr = AccessTools.Field(ge, "enStrForImgGen");
            MethodInfo target = AccessTools.Method(ge, "GetOrForceGetEnStrForImgGen", Type.EmptyTypes);

            if (_asNonCpn == null || _prettyName == null || _descNoAsync == null || _enStr == null || target == null)
            {
                Log.LogError("Не найдены нужные методы игры (игра обновилась?) — плагин выключен.");
                return;
            }

            new Harmony(Guid).Patch(target, prefix: new HarmonyMethod(typeof(Plugin), nameof(Prefix)));
            Log.LogInfo("Готово: картинки будут рисоваться по свежему описанию.");
        }

        private static void Prefix(object __instance)
        {
            try
            {
                if (!CfgEnabled.Value || __instance == null) return;

                // Если задано отдельное визуальное описание, игра берёт его, а не перевод.
                if (_optVisual != null && !string.IsNullOrEmpty(_optVisual.Invoke(__instance, null) as string)) return;

                object holder = _asNonCpn.Invoke(__instance, null) ?? __instance;
                string fp = (_prettyName.Invoke(__instance, null) as string) + "\n" + (_descNoAsync.Invoke(__instance, null) as string);

                bool known = Seen.TryGetValue(holder, out string old);
                if (known && old == fp) return;

                Seen.Remove(holder);
                Seen.Add(holder, fp);

                if (!known && !CfgRefreshOld.Value) return;
                if (_enStr.GetValue(holder) == null) return;

                _enStr.SetValue(holder, null);
                if (CfgVerbose.Value)
                    Log.LogInfo("Перевод для картинки сброшен: " + (_prettyName.Invoke(__instance, null) as string));
            }
            catch (Exception e)
            {
                Log.LogWarning("Ошибка в плагине (игра продолжит как обычно): " + e.Message);
            }
        }
    }
}
