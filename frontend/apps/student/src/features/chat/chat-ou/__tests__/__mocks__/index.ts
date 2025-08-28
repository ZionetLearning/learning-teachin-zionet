import { vi } from 'vitest';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (k: string) => k }),
}));

// Default mock (can be overridden per test with mockImplementationOnce)
export const sendMessageSpy = vi.fn();
export const retryLastMessageSpy = vi.fn();

// Mock useContext used inside MessageInput so we can control context UI
export const attachContextSpy = vi.fn();
export const detachContextSpy = vi.fn();
export const refreshContextSpy = vi.fn();

vi.mock('../hooks', () => ({
  useChat: () => ({
    messages: [],
    isLoading: false,
    error: undefined,
    sendMessage: sendMessageSpy,
    retryLastMessage: retryLastMessageSpy,
    clearMessages: vi.fn(),
    isInitialized: true,
  }),
  useContext: () => ({
    currentContext: { pageUrl: 'https://example.com', pageTitle: 'Example' },
    isContextAttached: false,
    attachContext: attachContextSpy,
    detachContext: detachContextSpy,
    refreshContext: refreshContextSpy,
    hasSignificantContext: true,
    contextDisplayText: 'Example – https://example.com',
  }),
}));
