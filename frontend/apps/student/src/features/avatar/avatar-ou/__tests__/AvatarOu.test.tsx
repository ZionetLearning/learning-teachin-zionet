import { render, screen, fireEvent, act } from '@testing-library/react';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { AvatarOu } from '../index';

// Create mock functions at the top level for proper module hoisting
const mockT = vi.fn((key: string) => `translated-${key}`);
const mockSpeak = vi.fn();
const mockStop = vi.fn();
const mockToggleMute = vi.fn();

// Additional top-level mocks for module hoisting
vi.mock('@tanstack/react-query', () => ({
  useMutation: vi.fn(() => ({
    mutate: vi.fn(),
    isLoading: false,
    error: null,
  })),
  useQueryClient: vi.fn(() => ({})),
}));

vi.mock('@/api/speech', () => ({
  useSynthesizeSpeech: vi.fn(() => ({
    mutate: vi.fn(),
    isLoading: false,
    error: null,
  })),
}));

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

vi.mock('react-i18next', () => ({
  useTranslation: vi.fn(() => ({
    t: mockT,
    i18n: { changeLanguage: vi.fn() },
  })),
}));

vi.mock('lottie-web', () => ({
  __esModule: true,
  default: {
    loadAnimation: vi.fn(),
    destroy: vi.fn(),
  },
}));

vi.mock('react-lottie', () => ({
  __esModule: true,
  default: vi.fn(() => <div data-testid="lottie-animation" />),
}));

describe('<AvatarOu />', () => {
  beforeEach(() => {
    // Clear all mocks before each test
    vi.clearAllMocks();
  });

  it('renders the avatar page with all main elements', () => {
    act(() => {
      render(<AvatarOu />);
    });

    // Check main container
    expect(screen.getByTestId('avatar-ou-page')).toBeInTheDocument();
    
    // Check header elements
    expect(mockT).toHaveBeenCalledWith('pages.avatarOu.ouAvatar');
    expect(mockT).toHaveBeenCalledWith('pages.avatarOu.avatarSpeaksHebrewWithAi');
    
    // Check avatar elements
    expect(screen.getByTestId('lottie-animation')).toBeInTheDocument();
    
    // Check input textarea
    expect(screen.getByTestId('avatar-ou-input')).toBeInTheDocument();
    
    // Check control buttons
    expect(screen.getByTestId('avatar-ou-speak')).toBeInTheDocument();
    expect(screen.getByTestId('avatar-ou-mute')).toBeInTheDocument();
  });

  it('displays default Hebrew text and allows typing', () => {
    act(() => {
      render(<AvatarOu />);
    });

    const textarea = screen.getByTestId('avatar-ou-input') as HTMLTextAreaElement;
    expect(textarea.value).toBe('שלום, איך שלומך היום?');

    act(() => {
      fireEvent.change(textarea, { target: { value: 'טקסט חדש' } });
    });

    expect(textarea.value).toBe('טקסט חדש');
    
    // The character counter contains the number and text with whitespace between them
    expect(screen.getByText(/8\s+translated-pages\.avatarOu\.characters/)).toBeInTheDocument();
  });

  it('renders sample text buttons and clicking sets textarea value', () => {
    act(() => {
      render(<AvatarOu />);
    });

    const sampleTexts = [
      'שלום, איך שלומך היום?',
      'אני בוט מדבר בעברית',
      'טוב לראות אותך פה!',
      'איך אני נשמע לך?',
      'זה הדמו של האווטר המדבר',
    ];

    // Check that all sample buttons exist
    sampleTexts.forEach((text, index) => {
      const button = screen.getByTestId(`avatar-ou-sample-${index}`);
      expect(button).toBeInTheDocument();
      expect(button).toHaveTextContent(text);
    });

    // Click sample button and verify textarea updates
    const textarea = screen.getByTestId('avatar-ou-input') as HTMLTextAreaElement;
    const sampleButton = screen.getByTestId('avatar-ou-sample-2');
    
    act(() => {
      fireEvent.click(sampleButton);
    });

    expect(textarea.value).toBe('טוב לראות אותך פה!');
  });

  it('calls speak function when speak button is clicked', () => {
    act(() => {
      render(<AvatarOu />);
    });

    const speakButton = screen.getByTestId('avatar-ou-speak');
    
    act(() => {
      fireEvent.click(speakButton);
    });

    expect(mockSpeak).toHaveBeenCalledWith('שלום, איך שלומך היום?');
  });

  it('disables speak button when textarea is empty', () => {
    act(() => {
      render(<AvatarOu />);
    });

    const textarea = screen.getByTestId('avatar-ou-input') as HTMLTextAreaElement;
    const speakButton = screen.getByTestId('avatar-ou-speak');

    // Clear the textarea
    act(() => {
      fireEvent.change(textarea, { target: { value: '' } });
    });

    expect(speakButton).toBeDisabled();
  });

  it('calls toggleMute function when mute button is clicked', () => {
    act(() => {
      render(<AvatarOu />);
    });

    const muteButton = screen.getByTestId('avatar-ou-mute');
    
    act(() => {
      fireEvent.click(muteButton);
    });

    expect(mockToggleMute).toHaveBeenCalled();
  });

  it('textarea has correct RTL direction and placeholder', () => {
    act(() => {
      render(<AvatarOu />);
    });

    const textarea = screen.getByTestId('avatar-ou-input') as HTMLTextAreaElement;
    expect(textarea).toHaveAttribute('dir', 'rtl');
    expect(textarea).toHaveAttribute('placeholder', 'translated-pages.avatarOu.typeHereYourText');
    expect(textarea).toHaveAttribute('rows', '3');
  });

  it('shows translation keys are called correctly', () => {
    act(() => {
      render(<AvatarOu />);
    });

    // Verify key translation keys are called
    expect(mockT).toHaveBeenCalledWith('pages.avatarOu.ouAvatar');
    expect(mockT).toHaveBeenCalledWith('pages.avatarOu.avatarSpeaksHebrewWithAi');
    expect(mockT).toHaveBeenCalledWith('pages.avatarOu.typeTextInHebrew');
    expect(mockT).toHaveBeenCalledWith('pages.avatarOu.typeHereYourText');
    expect(mockT).toHaveBeenCalledWith('pages.avatarOu.characters');
    expect(mockT).toHaveBeenCalledWith('pages.avatarOu.examples');
  });
});
