# Playable Ads Short

Тестовый playable ads по `Assets/Reference/ref.mp4`. Визуальная композиция повторяет основной референс, а порядок целей и боевые действия основаны на остальных видео из `Assets/Reference`.

## Движок и стек

- Unity `6000.3.14f1`, URP, WebGL.
- VContainer для composition root и зависимостей gameplay flow.
- UniTask для последовательностей действий и ожидания анимаций.
- DOTween для движения по маршруту, feedback, подсказок и появления VFX.
- Input System для mouse/touch raycast по world colliders.

Unity выбран из-за готовых FBX-анимаций, PSD Importer, удобной работы со sprite sequences и надёжной WebGL-сборки.

## Структура

- Основная сцена: `Assets/_Project/Scenes/PlayableAdsShort.unity`.
- В сцене находится один `GameRoot` prefab instance; runtime scene builders отсутствуют.
- Фон собран в world space из слоёв `BG_plb_new.psd` через `SpriteRenderer`.
- Герой, гоблины и сундук используют исходные FBX и реальные Animator Controllers.
- Морские цели и impact VFX используют исходные sprite sequences.
- Сила, reward, anchors, colliders, цвета, размеры и visual feedback настраиваются на prefab instances.
- Навигация задаётся waypoint graph внутри `GameRoot`: герой ходит только по земле и пирсам. Конечная точка атаки проецируется на ближайший walkable node.
- Позиции целей не хранятся в коде. При переносе врага его collider, badge, glow и anchors перемещаются вместе с ним, а маршрут перестраивается автоматически.

## Локальный запуск

1. Открыть проект в Unity `6000.3.14f1`.
2. Открыть `Assets/_Project/Scenes/PlayableAdsShort.unity`.
3. Нажать Play.

Готовый single-file build запускается через локальный HTTP-сервер:

```powershell
python -m http.server 8080 --directory Dist
```

Открыть `http://localhost:8080/PlayableAdsShort.html`.

## WebGL build

1. Собрать WebGL в `Build/WebGL` с единственной сценой `PlayableAdsShort.unity`.
2. Упаковать output в один HTML:

```powershell
python Tools/pack_webgl.py Build/WebGL Dist/PlayableAdsShort.html
```

Итоговый файл: `Dist/PlayableAdsShort.html`.

## Оптимизация

- Reference video не имеет runtime references и не попадает в player data.
- PSD: Read/Write и mipmaps отключены, atlas ограничен `2048`, включено сжатие.
- Runtime model textures ограничены `1024`; VFX и sequence frames ограничены `512`.
- Неиспользуемые generated placeholders и фиктивные clips удалены.
- FBX: Read/Write отключён, Mesh Compression `Medium`, Animation Compression `Optimal`.
- Sprite sequences: clamp, bilinear, без mipmaps, compressed import.
- Audio: только используемые clips, mono/Vorbis и сниженный sample rate там, где это допустимо.
- Managed stripping и engine code stripping включены для WebGL.

Фактический размер стандартного WebGL output и single HTML фиксируется после финальной сборки.

## Что улучшить

- Точнее подогнать attack impact frames и camera shake покадрово под финальный рекламный ролик.
- Добавить отдельный production CTA URL и аналитику кликов.
- Сделать pooling для VFX/path dots при более длинной игровой сессии.
- Дополнительно уменьшить Unity runtime за счёт отдельного минимального build profile.
