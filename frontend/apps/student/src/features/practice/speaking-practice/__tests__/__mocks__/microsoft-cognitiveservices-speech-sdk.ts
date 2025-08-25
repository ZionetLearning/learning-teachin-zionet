const noop = () => {};

export const SpeechConfig = {
  fromSubscription: jest.fn(() => ({})),
};

export const AudioConfig = {
  fromDefaultMicrophoneInput: jest.fn(() => ({ close: noop })),
  fromSpeakerOutput: jest.fn((player: unknown) => player as unknown),
};

export const SpeechRecognizer = jest.fn().mockImplementation(() => ({
  recognizeOnceAsync: (
    _onSuccess: (res: { text?: string }) => void,
    onError: (err: unknown) => void,
  ) => {
    onError(new Error("mock error"));
  },
  close: noop,
}));

export const SpeechSynthesizer = jest.fn().mockImplementation(
  (
    _speechConfig: unknown,
    audioDestination: {
      onAudioStart?: () => void;
      onAudioEnd?: () => void;
    },
  ) => {
    return {
      audioDestination,
      speakTextAsync: (
        _text: string,
        successCb: () => void,
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        _errorCb?: (err: unknown) => void,
      ) => {
        audioDestination.onAudioStart?.();
        setTimeout(() => {
          successCb();
          audioDestination.onAudioEnd?.();
        }, 0);
      },
      close: noop,
    };
  },
);

export const SpeakerAudioDestination = jest.fn().mockImplementation(() => ({
  onAudioStart: null as (() => void) | null,
  onAudioEnd: null as (() => void) | null,
  pause: noop,
  close: noop,
}));
