import { ReactNode, useState } from "react";

jest.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (k: string) => k }),
  Trans: ({ children }: { children: ReactNode }) => children,
  initReactI18next: { type: "3rdParty", init: jest.fn() },
}));

jest.mock("@/hooks", () => {
  return {
    useAvatarSpeech: () => {
      const [isPlaying, setIsPlaying] = useState(false);
      return {
        speak: jest.fn(() => {
          setIsPlaying(true);
          setTimeout(() => setIsPlaying(false), 0);
        }),
        stop: jest.fn(() => setIsPlaying(false)),
        isPlaying,
        isLoading: false,
        error: null,
        currentViseme: 0,
        toggleMute: jest.fn(),
        isMuted: false,
      };
    },
  };
});
