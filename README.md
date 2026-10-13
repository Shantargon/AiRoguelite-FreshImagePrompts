# Fresh Image Prompts / Свежие описания для картинок — AI Roguelite plugin

## English

A BepInEx plugin for AI Roguelite that makes images follow an entity's **current** description.

**The bug it fixes.** In scenarios whose language is not English, the game translates a character's, item's or place's
description into English for image generation and caches that translation. The cache is reset only when the entity is
renamed. So after its look, gear or status changes, regenerated images are still drawn from the old text.

**What it does.** Right before the game uses the cached translation, the plugin compares the entity's name, description and
status effects with the ones the translation was made from. If they differ, it drops the cache and the game translates the
current text. Translations made before the game was started are refreshed once per session (can be turned off).
Entities with a separate visual description and English scenarios are not affected.

**Install.** Requires [BepInEx 5](https://github.com/BepInEx/BepInEx) (win x64) in the game folder. Download
`FreshImagePrompts-<version>.zip` from [Releases](../../releases) and put `FreshImagePrompts.dll` into
`<game folder>\BepInEx\plugins`. `BepInEx\LogOutput.log` should then contain a line saying the plugin is ready.

**Settings** (`BepInEx\config\shantargon.airoguelite.freshimageprompts.cfg`, labels in Russian): enabled; refresh
translations made before the game started; verbose log.

**Uninstall.** Delete `FreshImagePrompts.dll` (and the cfg file if you like).

If a game update renames the patched method, the plugin logs that and does nothing; the game keeps working.

**Build:** `dotnet build src/FreshImagePrompts/FreshImagePrompts.csproj -c Release -p:GameDir="path\to\AI Roguelike"`.

**License:** MIT (see `LICENSE`).

## По-русски

BepInEx-плагин для AI Roguelite: картинки рисуются по **текущему** описанию персонажа, предмета или места.

**Какую ошибку исправляет.** Если язык сценария не английский, игра для картинки переводит описание на английский и
запоминает перевод. Сбрасывает его только при переименовании. Поэтому после смены внешности, снаряжения или статуса
новые картинки всё равно рисуются по старому тексту.

**Что делает.** Прямо перед тем, как игра возьмёт сохранённый перевод, плагин сравнивает имя, описание и статусы
с теми, по которым перевод был сделан. Если что-то изменилось, перевод сбрасывается, и игра переводит текущий текст.
Переводы, сделанные до запуска игры, один раз за сессию переводятся заново (можно выключить).
Сущности с отдельным визуальным описанием и английские сценарии плагин не трогает.

**Установка.** Нужен [BepInEx 5](https://github.com/BepInEx/BepInEx) (win x64) в папке игры. Скачать
`FreshImagePrompts-<версия>.zip` в [Releases](../../releases) и положить `FreshImagePrompts.dll` в
`<папка игры>\BepInEx\plugins`. В `BepInEx\LogOutput.log` появится строчка «Готово: картинки будут рисоваться
по свежему описанию».

**Настройки** — `BepInEx\config\shantargon.airoguelite.freshimageprompts.cfg`: «Включено», «Обновлять старые
переводы», «Подробный лог».

**Удаление.** Удалить `FreshImagePrompts.dll` (и, по желанию, файл настроек).

Если обновление игры переименует нужный метод, плагин напишет об этом в лог и ничего не будет делать; игра продолжит работать.

**Сборка:** `dotnet build src/FreshImagePrompts/FreshImagePrompts.csproj -c Release -p:GameDir="путь\к\AI Roguelike"`.

**Лицензия:** MIT (файл `LICENSE`).
