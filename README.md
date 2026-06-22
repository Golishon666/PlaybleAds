# Playable Ads Short

Короткий playable ads проект на Unity по референсам из `Assets/Reference`. Игровой флоу: герой ходит по waypoint-графу, открывает сундук, получает меч, побеждает наземных и водных врагов, после чего появляется CTA.

## Движок

- Unity `6000.3.14f1`, URP, WebGL.
- VContainer используется для composition root и зависимостей gameplay flow.
- UniTask используется для последовательностей действий, ожидания анимаций и async-flow.
- DOTween используется для перемещения по маршруту, feedback, path dots, shake, VFX и UI-анимаций.
- Input System используется для mouse/touch raycast по world colliders.

Unity выбран потому, что проект опирается на готовые FBX-анимации, Animator Controllers, PSD Importer, sprite sequences, WebGL export и удобную настройку prefab-сцены без runtime scene builder.

## Структура

- Основная сцена: `Assets/Scenes/PlayableAdsShort.unity`.
- Главный prefab сцены: `Assets/Prefabs/GameRoot.prefab`.
- Геймплейная последовательность: `Assets/Scripts/GameSequence.cs`.
- Конфиг геймплея: `Assets/Configs/GameConfig.asset`.
- Single-file packer: `Tools/pack_webgl.py`.
- Editor build menu: `Assets/Editor/PlayableBuildTools.cs`.

## Локальный запуск в редакторе

1. Открыть проект в Unity `6000.3.14f1`.
2. Открыть сцену `Assets/Scenes/PlayableAdsShort.unity`.
3. Нажать Play.

Проект настроен под портретный WebGL: `600x960`, orientation `Portrait`.

## Сборка

Самый простой способ из Unity:

1. Открыть верхнее меню `Playable`.
2. Выбрать `Build Single HTML`.
3. Дождаться завершения.

Команда соберет WebGL в `Build/WebGL`, упакует билд в один файл и обновит отчеты:

- итоговый файл: `Dist/PlayableAdsShort.html`;
- WebGL analyze: `Build/Reports/webgl-build-analyze.txt`;
- single-file analyze: `Build/Reports/single-file-analyze.txt`;
- список файлов билда: `Build/Reports/webgl-build-files.csv`.

Если WebGL уже собран и нужно только перепаковать HTML:

1. Открыть `Playable`.
2. Выбрать `Pack Current WebGL To Single HTML`.

CLI-вариант упаковки:

```powershell
python Tools/pack_webgl.py Build/WebGL Dist/PlayableAdsShort.html
```

Локальный запуск готового HTML:

```powershell
python -m http.server 8080 --directory Dist
```

Открыть:

```text
http://localhost:8080/PlayableAdsShort.html
```

## Оптимизации размера

- Unity splash screen отключен в `ProjectSettings.asset`, чтобы убрать встроенный `Splash Screen Unity Logo` из player data.
- WebGL build собирается без Unity compression, потому что финальный формат должен быть одним HTML. После этого payload упаковывается вручную.
- Single-file HTML хранит `.wasm`, `.data`, `.framework.js` и `.loader.js` как gzip+base64 payload. В браузере они распаковываются через `DecompressionStream` перед запуском Unity.
- Sprite sequences сгруппированы в sprite atlases, включая VFX atlas и water creature atlas. Это уменьшает количество отдельных texture bindings и упрощает упаковку используемых кадров.
- Для sprite sequences отключены mipmaps, включены clamp/bilinear настройки и сжатый import там, где это не ломает визуал.
- PSD/background и runtime textures ограничены по размеру импорта; Read/Write отключен там, где не нужен.
- FBX настроены как runtime-friendly assets: Read/Write выключен, mesh/animation compression включены.
- Audio импортируется только для реально используемых клипов; для коротких SFX применяются сжатие, mono и сниженный sample rate там, где это допустимо.
- Managed stripping и engine code stripping включены для WebGL.
- Reference video и вспомогательные исходники не имеют runtime references и не попадают в player data.
- Удалены/не используются placeholder-ассеты и лишние фейковые клипы, чтобы не тянуть их через сцены или prefab references.

По последней сборке single-file HTML получался около `29 MiB`, а основной вес WebGL output приходился на `WebGL.wasm` и `WebGL.data`. В `WebGL.data` самые заметные категории: textures, sprite atlases и VFX sequence frames.

## Что улучшить дальше

- Дополнительно ужать VFX sprite sequences, особенно `Particles_Continue`, где каждый кадр заметно влияет на `WebGL.data`.
- Разделить большие sprite atlases на более точные runtime-atlases и убрать кадры, которые не видны в текущем playable flow.
- Проверить, можно ли заменить часть frame-by-frame VFX на ParticleSystem или shader-based эффекты.
- Убрать неиспользуемые Unity packages из player assemblies, если они не нужны финальному playable build.
- Подобрать отдельный минимальный URP/build profile только под playable ads.
- Провести визуальную проверку на реальных портретных размерах рекламных сетей и при необходимости сделать отдельные camera/layout presets.
- Добавить production CTA URL и финальную аналитику кликов.
