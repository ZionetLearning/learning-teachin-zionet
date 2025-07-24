import type { Exercise, DifficultyLevel, FeedbackResult } from './types';
import { exerciseBank } from './data';

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

export function compareTexts(userInput: string, expectedText: string): FeedbackResult {
    const normalizedUserInput = userInput.trim();
    const normalizedExpectedText = expectedText.trim();

    const characterComparison: Array<{
        char: string;
        isCorrect: boolean;
        position: number;
    }> = [];

    const maxLength = Math.max(normalizedUserInput.length, normalizedExpectedText.length);
    let correctCharacters = 0;

    for (let i = 0; i < maxLength; i++) {
        const userChar = normalizedUserInput[i] || '';
        const expectedChar = normalizedExpectedText[i] || '';
        const isCorrect = userChar === expectedChar;

        if (isCorrect && userChar !== '') {
            correctCharacters++;
        }

        if (expectedChar !== '') {
            characterComparison.push({
                char: expectedChar,
                isCorrect: isCorrect,
                position: i,
            });
        }
    }

    const accuracy = normalizedExpectedText.length > 0
        ? Math.round((correctCharacters / normalizedExpectedText.length) * 100)
        : 0;

    return {
        userInput: normalizedUserInput,
        expectedText: normalizedExpectedText,
        accuracy,
        characterComparison,
    };
}