import { vi } from 'vitest';

// Shared deterministic exercise
export const mockExercise = { id: 'easy-xyz', hebrewText: 'שלום', difficulty: 'easy' as const };

// i18n mock (local to this suite)
vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (k: string) => k }),
}));

// utils mock to provide deterministic exercise selection
vi.mock('../../utils', async () => {
  const actual = await vi.importActual<typeof import('../../utils')>('../../utils');
  return {
    ...actual,
    getRandomExercise: vi.fn(() => mockExercise),
  };
});

// Hook mock for avatar speech
export const speakSpy = vi.fn();
vi.mock('@/hooks', () => ({
  useAvatarSpeech: (opts: { onAudioStart?: () => void; onAudioEnd?: () => void }) => ({
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
