import { vi } from "vitest";

export const mockExercise = {
  id: "easy-xyz",
  hebrewText: "שלום",
  difficulty: "easy" as const,
};

// Mock lottie-web at the lowest level to prevent canvas access
vi.mock("lottie-web", () => ({
  __esModule: true,
  default: {
    loadAnimation: vi.fn(),
    destroy: vi.fn(),
    setSpeed: vi.fn(),
    setDirection: vi.fn(),
    play: vi.fn(),
    pause: vi.fn(),
    stop: vi.fn(),
  },
}));

vi.mock("react-i18next", async (importOriginal) => {
  const actual = await importOriginal<typeof import("react-i18next")>();
  return {
    ...actual,
    useTranslation: () => ({
      t: (key: string) => key,
      i18n: { language: "en", changeLanguage: vi.fn() },
    }),
    initReactI18next: {
      type: "3rdParty",
      init: () => {},
    },
  };
});

export const speakSpy = vi.fn();
vi.mock("@student/hooks", () => {
  return {
    useAvatarSpeech: (opts: {
      onAudioStart?: () => void;
      onAudioEnd?: () => void;
    }) => ({
      speak: (text: string) => {
        speakSpy(text);
        opts.onAudioStart?.();
        opts.onAudioEnd?.();
      },
      stop: vi.fn(),
      toggleMute: vi.fn(),
      isPlaying: false,
      isMuted: false,
      isLoading: false,
      error: null,
      currentViseme: 0,
      currentVisemeSrc: undefined,
    }),

    useHebrewSentence: vi.fn(() => ({
      attemptId: "easy-xyz",
      sentence: "שלום",
      words: ["שלום"],
      sentenceCount: 1,
      currentSentenceIndex: 0,
      loading: false,
      error: null,
      initOnce: vi.fn(),
      resetGame: vi.fn(),
      fetchSentence: vi.fn(),
      currentDifficulty: 1,
      hasNikud: true,
    })),
    useGameConfig: vi.fn(() => ({
      config: null,
      isLoading: false,
      updateConfig: vi.fn(),
      setConfig: vi.fn(),
    })),
    resetSentenceGameHook: vi.fn(),
    initOnce: vi.fn(),
  };
});

vi.mock("@student/api", () => ({
  useSubmitGameAttempt: vi.fn(() => ({
    mutateAsync: vi.fn(async ({ givenAnswer }: { givenAnswer: string[] }) => {
      // Calculate accuracy based on the answer
      const expectedAnswer = "שלום";
      const userAnswer = givenAnswer[0] || "";
      const isCorrect = userAnswer === expectedAnswer;
      const accuracy = isCorrect ? 100 : Math.round((userAnswer.length / expectedAnswer.length) * 100);
      
      return {
        attemptId: "attempt-123",
        exerciseId: "easy-xyz",
        studentId: "student-1",
        gameType: "TypingPractice",
        difficulty: "Easy",
        status: isCorrect ? "Success" : "Failure",
        correctAnswer: [expectedAnswer],
        attemptNumber: 1,
        accuracy: accuracy,
      };
    }),
  })),
}));
