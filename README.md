# MathMatic — Simple Game Design Document

## Game Introduction

**MathMatic** is a 2D educational math game set in a classroom-style world, where questions appear on a chalkboard and players choose the correct answer from four options. Designed for children aged **5–9**, the game turns quick mental math practice into a short, focused session: each level offers **5 questions** under a **30-second** time limit, spread across **10 levels** with rising difficulty—from simple addition to mixed four-operation problems. The player’s main goal is to **complete all 10 levels**, earn stars based on performance, and unlock progress while building speed and confidence in basic arithmetic.

---

## What I Did (Individual Contribution List)

### Muhammed Can Ablay — Developer
- Implemented core game flow (`GameManager`, level progression, timer, scoring, save/unlock via `PlayerPrefs`)
- Built procedural question system (`QuestionGenerator`) with difficulty scaling across 10 levels
- Developed UI logic (`UIManager`): menus, level select, feedback panels, and in-game HUD
- Created chalkboard transition effects (`ChalkboardWipeAnimator`): wipe, empty board, and typewriter-style question reveal 
- Integrated sound hooks for erase/write SFX, correct/wrong feedback, and background music manager
- Set up editor tools for scene/audio/cursor setup and fixed gameplay/UI bugs during iteration

### Yusuf Berk Şahan — Art
- Designed the chalkboard-themed visual style and classroom UI layout.
- Prepared and integrated UI graphics (backgrounds, panels, buttons)
- Supported visual polish for menus, level select, and in-game screens

### Eren Doğan — Art
- Worked on art assets and visual consistency with the chalkboard theme
- Contributed to UI elements and overall 2D look of menus and gameplay screens
- Collaborated on integrating art into Unity scenes.

### Nisanur Yılmaz — Audio
- Researched and selected sound effects (correct/wrong feedback, UI clicks, chalk/erase sounds)
- Provided background music and gameplay audio assets used in the project
- Supported audio placement and volume balance together with the development team

---

## Educational Concept

### Pedagogical Goal

The game teaches **basic arithmetic fluency**—addition, subtraction, multiplication, and division—for elementary learners. The focus is on **fast, accurate mental calculation**: recognizing operations, choosing the right result under time pressure, and improving through repeated, level-appropriate practice rather than passive reading of formulas.

### Applied Learning Theory: Cognitivism

MathMatic aligns with **cognitivism**, which emphasizes how learners process information, build mental schemas, and improve through structured practice and feedback. The game does not rely on rote drill alone; it presents problems in a meaningful context (a chalkboard quiz), increases cognitive demand step by step, and gives immediate feedback so players can connect each response to the correct rule or strategy.

### How Game Mechanics Facilitate Learning

- **Immediate feedback (correct = green text + sound; wrong = correction shown)**  
  When the player answers correctly, positive visual and audio feedback reinforces the right procedure and strengthens the link between the operation and the outcome. On a wrong answer, showing the correct result supports error correction and helps update the player’s mental model before the next attempt or level.

- **Gradual difficulty (e.g., Level 1: single-digit addition → later levels: mixed operations and larger numbers)**  
  Early levels limit cognitive load to one operation type and smaller numbers. Later levels combine operations and harder ranges, so skills are layered—matching cognitivist ideas of scaffolding and progressive complexity instead of overwhelming the learner at once.

- **30-second time limit per level**  
  The timer encourages automatic recall and decision speed, which supports the pedagogical goal of **mental math fluency**. Time pressure is balanced with only five questions per level so the task stays achievable for ages 5–9 while still practicing quick thinking.

Together, these mechanics turn practice into a repeatable, engaging loop: read the problem on the board, select an answer, receive feedback, and advance—supporting both accuracy and speed over multiple short sessions.

---

## Technical Notes

| Item | Detail |
|------|--------|
| Engine | Unity 6 |
| Platform | Windows (build); mobile may be added later |
| Scenes | `MainMenu`, `GameScene` |
| Team | Muhammed Can Ablay, Yusuf Berk Şahan, Nisanur Yılmaz, Eren Doğan |
