import { vi } from "vitest";

export const mockExercise = {
  id: "easy-xyz",
  hebrewText: "שלום",
  difficulty: "easy" as const,
};

// Mock lottie-web to prevent canvas errors
vi.mock("lottie-web", () => {
  return {
    loadAnimation: vi.fn(() => ({
      play: vi.fn(),
      stop: vi.fn(),
      destroy: vi.fn(),
      setSpeed: vi.fn(),
      goToAndStop: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    })),
  };
});

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
vi.mock("@student/hooks", () => ({
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
}));
