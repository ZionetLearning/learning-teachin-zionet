const noop = () => {};

export const SpeechConfig = {
	fromSubscription: jest.fn(() => ({})),
};

export const AudioConfig = {
	fromDefaultMicrophoneInput: jest.fn(() => ({})),
	fromSpeakerOutput: jest.fn(() => ({})),
};

export const SpeechRecognizer = jest.fn().mockImplementation(() => ({
	recognizeOnceAsync: (errorCb: (error: unknown) => void) =>
		errorCb('mock error'),
	close: noop,
}));

export const SpeechSynthesizer = jest.fn().mockImplementation(() => ({
	speakTextAsync: (callback: () => void) => callback(),
	close: noop,
}));

export const SpeakerAudioDestination = jest.fn().mockImplementation(() => ({
	onAudioStart: noop,
	onAudioEnd: noop,
	pause: noop,
	close: noop,
}));
