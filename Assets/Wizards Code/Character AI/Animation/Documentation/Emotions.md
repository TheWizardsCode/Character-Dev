# Emotions

This scene demonstrates how to give a character emotions. These emotions can then be used to influence animations that play.

## Emotion values

Each character has an `EmotionalState` component holding five `EmotionMetric`s — one for each of:

- Anger
- Interest
- Fear
- Sadness
- Pleasure

Each metric has:

- `value` — the current intensity of the emotion, from **0 to 1**, where **0 is neutral**.
- `activationMultiplier` — how strongly (and in which direction) the emotion affects the character's activation (e.g. Sadness has a negative activation multiplier).
- `enjoymentMultiplier` — how strongly (and in which direction) the emotion affects the character's enjoyment (e.g. Fear has a negative enjoyment multiplier).

Emotion values are serialised, so values configured in the editor or in a prefab persist into play sessions. Values can be driven by the timeline (`EmotionControlPlayableBehaviour`), by AI scripts (`EmotionalConditionTest`, `EmotionAdjustment`, `InkManager`), or by the demo UI sliders.

## Derived values

From the raw emotion values the `EmotionalState` component computes, every frame:

- `activationValue` = `0.5 + Σ(value × activationMultiplier)`, clamped to 0..1 (0.5 = neutral).
- `enjoymentValue` = `0.5 + Σ(value × enjoymentMultiplier)`, clamped to 0..1 (0.5 = neutral).
- `Noticeability` = a normalisation of `Anger − Fear + Interest/2 − Sadness/2` to the full 0..1 range (0 = as good as invisible, 1 = can't miss them).

These values drive the descriptors (`activationDescriptor`, `enjoymentDescriptor`) and, via `EmotionalStateMachineBehavior`, the animator's `Activation`/`Enjoyment` float parameters.

## Driving the animation

`EmotionalStateMachineBehavior` is a `StateMachineBehaviour` attached to the animator states that should respond to emotion. It reads `activationValue`/`enjoymentValue` and eases the animator's `Activation`/`Enjoyment` parameters toward those targets over the configured **Pose Change Time**. The interpolation is frame-rate independent (progress is measured in elapsed time, not per-frame increments).

In the demo (`Nova Controller`) the behaviour is attached to:

- the **Emote** state — its 2D blend tree is parameterised by `Enjoyment`/`Activation` and blends between sad / neutral / happy / dance poses. The character is routed into this state automatically from the default state, so dragging the emotion sliders visibly changes the pose without pressing any button.
- the **Talk** state (on the Talking layer) — the legacy talking path driven by the "Start Talking" button in the demo UI. Note: `BaseActorController.StartTalking()`/`StopTalking()` are `[Obsolete]` and require an `AnimationLayerController` on the character to actually raise the Talking layer weight; the primary emotion-driven demo path is the Emote state.

## Demo scene (06 Emotions / WIP Emotions)

- The Dev UI canvas instantiates one slider row per emotion metric (the design-time template row is hidden).
- Sliders range 0..1 with 0 = neutral. Dragging a slider changes the derived values immediately; the character's pose then eases to the new emotional pose over the pose change time.
- The on-screen values show both the computed `activationValue`/`enjoymentValue` and the raw animator `Activation`/`Enjoyment` parameters.
- The **Start Talking** / **Stop Talking** buttons invoke `StartTalking()`/`StopTalking()` on the character's `AnimatorActorController`.

## Limitations

There are not sufficient animations in the pack to truly demonstrate the impact of this system (see video below). It includes a series of poses that will at least show movement in the character, but don't expect it to look good. It is designed to show how you might control those emotions in gameplay.

For a better understanding of what is possible check out my video describing what happens when this is integrated with a talking character: https://youtu.be/HnFmDeWkzNQ

Also note that at the time of writing this scene requires UMA to be in your project. We'll remove that dependency at some point and replace it with another character (patches welcome).
