import { vi } from "vitest";

export const mockExercise = {
  id: "easy-xyz",
  hebrewText: "שלום",
  difficulty: "easy" as const,
};

vi.mock("react-i18next", async (importOriginal) => {
  const actual = await importOriginal<typeof import("react-i18next")>();
  return {
    ...actual,
    useTranslation: () => ({
      t: (key: string) => key, // תמיד מחזיר את המפתח כטקסט
      i18n: { language: "en", changeLanguage: vi.fn() },
    }),
    // זה החלק שהיה חסר – plugin שה-i18n שלך מחפש
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
