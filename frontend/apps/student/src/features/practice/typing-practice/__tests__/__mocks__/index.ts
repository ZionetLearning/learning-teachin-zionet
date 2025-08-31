import { vi } from "vitest";

export const mockExercise = {
  id: "easy-xyz",
  hebrewText: "שלום",
  difficulty: "easy" as const,
};

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (k: string) => k }),
}));

vi.mock("../../utils", async () => {
  const actual =
    await vi.importActual<typeof import("../../utils")>("../../utils");
  return { ...actual, getRandomExercise: vi.fn(() => mockExercise) };
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
