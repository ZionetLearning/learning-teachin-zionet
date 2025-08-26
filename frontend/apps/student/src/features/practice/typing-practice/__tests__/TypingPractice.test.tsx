import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom/vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import './__mocks__';
import { TypingPractice } from '..';
import { speakSpy } from './__mocks__';

describe('<TypingPractice />', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderWithProviders = () => {
    const qc = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    return render(
      <QueryClientProvider client={qc}>
        <TypingPractice />
      </QueryClientProvider>
    );
  };

  it('shows level selection initially', () => {
    renderWithProviders();
    expect(screen.getByTestId('typing-level-selection')).toBeInTheDocument();
  });

  it('matches snapshot (initial level selection)', () => {
    const { asFragment } = renderWithProviders();
    expect(asFragment()).toMatchSnapshot();
  });

  it('selects a level and shows ready phase with play button', () => {
    renderWithProviders();
    fireEvent.click(screen.getByText('Easy'));
    expect(screen.getByTestId('typing-exercise-area')).toBeInTheDocument();
    expect(screen.getByTestId('typing-selected-level')).toHaveTextContent('easy');
    expect(screen.getByTestId('typing-play')).toBeInTheDocument();
    expect(screen.getByTestId('typing-phase-ready')).toBeInTheDocument();
  });

  it('plays audio then advances to typing phase and enables replay', async () => {
    renderWithProviders();
    fireEvent.click(screen.getByText('Easy'));
    fireEvent.click(screen.getByTestId('typing-play'));
    await screen.findByTestId('typing-phase-typing');
    expect(screen.getByTestId('typing-replay')).toBeInTheDocument();
    expect(speakSpy).toHaveBeenCalledWith('שלום');
  });

  it('submits a correct answer and shows 100% accuracy feedback', async () => {
    renderWithProviders();
    fireEvent.click(screen.getByText('Easy'));
    fireEvent.click(screen.getByTestId('typing-play'));
    await screen.findByTestId('typing-phase-typing');
    fireEvent.change(screen.getByTestId('typing-input'), { target: { value: 'שלום' } });
    fireEvent.click(screen.getByTestId('typing-submit'));
    await waitFor(() => {
      const accuracyEl = screen.getAllByText(/%/).find(el => /100/.test(el.textContent || ''));
      expect(accuracyEl).toBeTruthy();
    });
  });

  it('handles incorrect answer then try again resets to typing phase with cleared input', async () => {
    renderWithProviders();
    fireEvent.click(screen.getByText('Easy'));
    fireEvent.click(screen.getByTestId('typing-play'));
    await screen.findByTestId('typing-phase-typing');
    fireEvent.change(screen.getByTestId('typing-input'), { target: { value: 'שולם' } });
    fireEvent.click(screen.getByTestId('typing-submit'));
    await waitFor(() => {
      const accuracyEl = screen.getAllByText(/%/).find(el => /%/.test(el.textContent || ''));
      expect(accuracyEl).toBeTruthy();
      expect(accuracyEl?.textContent).not.toMatch(/100/);
    });
    fireEvent.click(screen.getByText('pages.typingPractice.tryAgain'));
    await screen.findByTestId('typing-phase-typing');
    expect((screen.getByTestId('typing-input') as HTMLInputElement).value).toBe('');
  });

  it('goes to next exercise after feedback and returns to ready phase', async () => {
    renderWithProviders();
    fireEvent.click(screen.getByText('Easy'));
    fireEvent.click(screen.getByTestId('typing-play'));
    await screen.findByTestId('typing-phase-typing');
    fireEvent.change(screen.getByTestId('typing-input'), { target: { value: 'שלום' } });
    fireEvent.click(screen.getByTestId('typing-submit'));
    await screen.findByText(/100%/);
    fireEvent.click(screen.getByText('pages.typingPractice.nextExercise'));
    await screen.findByTestId('typing-play');
    expect(screen.getByTestId('typing-phase-ready')).toBeInTheDocument();
  });

  it('change level button returns to level selection', () => {
    renderWithProviders();
    fireEvent.click(screen.getByText('Easy'));
    fireEvent.click(screen.getByTestId('typing-change-level'));
    expect(screen.getByTestId('typing-level-selection')).toBeInTheDocument();
  });
});
