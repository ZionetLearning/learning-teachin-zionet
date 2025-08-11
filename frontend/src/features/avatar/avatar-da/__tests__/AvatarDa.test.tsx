import './__mocks__';

import { vi } from 'vitest';

import { fireEvent, render, screen } from '@testing-library/react';
import '@testing-library/jest-dom/vitest';

vi.stubEnv('VITE_AZURE_SPEECH_KEY', 'mock-speech-key');
vi.stubEnv('VITE_AZURE_REGION', 'mock-region');

import { AvatarDa } from '..';

describe('<AvatarDa />', () => {
	beforeEach(() => {
		vi.useFakeTimers();
		vi.clearAllMocks();
	});
	afterEach(() => {
		vi.useRealTimers();
	});

	it('mounts with Canvas + input + Speak button', () => {
		render(<AvatarDa />);
		expect(screen.getByTestId('canvas')).toBeInTheDocument();
		expect(
			screen.getByPlaceholderText('Write something here in Hebrew')
		).toBeInTheDocument();
		expect(screen.getByRole('button', { name: 'Speak' })).toBeInTheDocument();
	});

	it('does nothing when Speak is clicked with empty input', async () => {
		render(<AvatarDa />);
		fireEvent.click(screen.getByRole('button', { name: 'Speak' }));
		const sdk = await vi.importMock<
			typeof import('microsoft-cognitiveservices-speech-sdk')
		>('microsoft-cognitiveservices-speech-sdk');
		expect(vi.mocked(sdk.SpeechSynthesizer)).not.toHaveBeenCalled();
	});

	it('creates SpeechSynthesizer, disables controls, then re-enables after playback', async () => {
		render(<AvatarDa />);
		const input = screen.getByPlaceholderText(
			'Write something here in Hebrew'
		) as HTMLInputElement;
		const btn = screen.getByRole('button', { name: 'Speak' });

		fireEvent.change(input, { target: { value: 'שלום' } });
		fireEvent.click(btn);

		const sdk = await vi.importMock<
			typeof import('microsoft-cognitiveservices-speech-sdk')
		>('microsoft-cognitiveservices-speech-sdk');

		expect(vi.mocked(sdk.SpeechSynthesizer)).toHaveBeenCalledTimes(1);
		expect(input).toBeDisabled();
		expect(btn).toBeDisabled();

		vi.advanceTimersByTime(1500);

		expect(input).not.toBeDisabled();
		expect(btn).not.toBeDisabled();
	});
});
