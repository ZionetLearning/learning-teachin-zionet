import type { Exercise, DifficultyLevel, FeedbackResult } from "./types";
import { exerciseBank } from "./data";
import { stripHebrewNikud, splitGraphemes } from "../utils";
export function getRandomExercise(level: DifficultyLevel): Exercise {
  const exercises = exerciseBank[level];

  if (!exercises || exercises.length === 0) {
    throw new Error(`No exercises found for difficulty level: ${level}`);
  }

  const randomIndex = Math.floor(Math.random() * exercises.length);
  return exercises[randomIndex];
}

export function getExercisesByLevel(level: DifficultyLevel): Exercise[] {
  return exerciseBank[level] || [];
}

export function getExerciseCount(level: DifficultyLevel): number {
  return exerciseBank[level]?.length || 0;
}

export function compareTexts(
  userInput: string,
  expectedText: string,
): FeedbackResult {
  const cleanUser = stripHebrewNikud(userInput.trim());
  const cleanExpected = stripHebrewNikud(expectedText).trim();

  const userChars = splitGraphemes(userInput);
  const expectedChars = splitGraphemes(expectedText);

  const characterComparison: Array<{
    char: string;
    isCorrect: boolean;
    position: number;
  }> = [];

  const maxLength = Math.max(userChars.length, expectedChars.length);
  let correctCharacters = 0;

  for (let i = 0; i < maxLength; i++) {
    const expectedCleanChar = cleanUser[i] || "";
    const userCleanChar = cleanExpected[i] || "";

    const expectedChar = expectedChars[i] || "";
    const userChar = userChars[i] || "";

    const isCorrect = userCleanChar === expectedCleanChar;

    if (isCorrect && userChar !== "") {
      correctCharacters++;
    }

    if (expectedChar !== "") {
      characterComparison.push({
        char: expectedChar,
        isCorrect,
        position: i,
      });
    }
  }

  const accuracy =
    expectedChars.length > 0
      ? Math.round((correctCharacters / expectedChars.length) * 100)
      : 0;
  const isCorrect = accuracy === 100;

  return {
    userInput: cleanUser,
    expectedText,
    accuracy,
    characterComparison,
    isCorrect,
  };
}
