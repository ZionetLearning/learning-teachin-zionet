import { vi } from 'vitest';
import React from 'react';

// Mock functions that we'll track
export const mockT = vi.fn((key: string) => key);
export const mockSpeak = vi.fn();
export const mockStop = vi.fn();
export const mockToggleMute = vi.fn();

// First, mock the API layer to prevent React Query from being called
vi.mock('@/api/speech', () => ({
  useSynthesizeSpeech: vi.fn(() => ({
    mutate: vi.fn(),
    isLoading: false,
    error: null,
  })),
}));

// Mock TanStack Query to prevent any query client issues
vi.mock('@tanstack/react-query', () => ({
  useMutation: vi.fn(() => ({
    mutate: vi.fn(),
    isLoading: false,
    error: null,
  })),
  useQueryClient: vi.fn(() => ({})),
  QueryClient: vi.fn(() => ({})),
  QueryClientProvider: vi.fn(({ children }) => children),
}));

// Mock react-i18next with our tracked function
vi.mock('react-i18next', () => ({
  useTranslation: vi.fn(() => ({
    t: mockT,
    i18n: { changeLanguage: vi.fn() },
  })),
}));

// Mock useAvatarSpeech hook with tracked functions
vi.mock('@/hooks/useAvatarSpeech', () => ({
  useAvatarSpeech: vi.fn(() => ({
    speak: mockSpeak,
    stop: mockStop,
    toggleMute: mockToggleMute,
    isPlaying: false,
    isMuted: false,
    isLoading: false,
    error: null,
  })),
}));

// Also mock the hooks index file
vi.mock('@/hooks', () => ({
  useAvatarSpeech: vi.fn(() => ({
    speak: mockSpeak,
    stop: mockStop,
    toggleMute: mockToggleMute,
    isPlaying: false,
    isMuted: false,
    isLoading: false,
    error: null,
  })),
}));

// Mock lottie-web at the lowest level to prevent canvas access
vi.mock('lottie-web', () => ({
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

// Mock react-lottie completely to avoid canvas issues
vi.mock('react-lottie', () => ({
  __esModule: true,
  default: vi.fn(() => React.createElement('div', { 'data-testid': 'lottie-animation' })),
}));

// Mock animation JSON files
vi.mock('../animations/speakingSantaAnimation.json', () => ({
  default: { frames: [], ip: 0, op: 60, fr: 60, w: 512, h: 512 },
}));

vi.mock('../animations/idleSantaAnimation.json', () => ({
  default: { frames: [], ip: 0, op: 60, fr: 60, w: 512, h: 512 },
}));

// Mock Lucide React icons
vi.mock('lucide-react', () => ({
  Play: vi.fn(() => React.createElement('div', { 'data-testid': 'play-icon' })),
  Square: vi.fn(() => React.createElement('div', { 'data-testid': 'square-icon' })),
  Volume2: vi.fn(() => React.createElement('div', { 'data-testid': 'volume2-icon' })),
  VolumeX: vi.fn(() => React.createElement('div', { 'data-testid': 'volumex-icon' })),
}));
