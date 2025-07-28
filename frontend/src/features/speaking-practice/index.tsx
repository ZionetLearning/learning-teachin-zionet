import { useRef, useState } from 'react';

import * as sdk from 'microsoft-cognitiveservices-speech-sdk';

import { useStyles } from './style';

const phrases = [
	'שלום',
	'שלום, איך אתה מרגיש היום?',
	'מה אתה אוהב לעשות בסוף השבוע?',
	'ספר לי על המשפחה שלך.',
	'מהו הספר האהוב עליך ולמה?',
	'מהו הסרט האחרון שראית ומה חשבת עליו?',
	'מהו המקום האהוב עליך לבקר בו בישראל?',
	'מהו המאכל האהוב עליך ולמה?',
	'מהו התחביב שלך ואיך התחלת בו?',
	'מהו החלום שלך לעתיד?',
	'מהו הנושא האהוב עליך בלימודים?',
	'מהו הספורט האהוב עליך ולמה?',
];

const phrasesWithNikud = [
	'שָׁלוֹם',
	'שָׁלוֹם, אֵיךְ אַתָּה מַרְגִּישׁ הַיּוֹם?',
	'מָה אַתָּה אוֹהֵב לַעֲשׂוֹת בְּסוֹף הַשָּׁבוּעַ?',
	'סַפֵּר לִי עַל הַמִּשְׁפָּחָה שֶׁלְּךָ.',
	'מָהוּ הַסֵּפֶר הָאָהוּב עָלֶיךָ וּלְמָה?',
	'מָהוּ הַסֶּרֶט הָאַחֲרוֹן שֶׁרָאוִית וּמָה חָשַׁבְתָּ עָלָיו?',
	'מָהוּ הַמָּקוֹם הָאָהוּב עָלֶיךָ לְבַקֵּר בּוֹ בְּיִשְׂרָאֵל?',
	'מָהוּ הַמַּאֲכָל הָאָהוּב עָלֶיךָ וּלְמָה?',
	'מָהוּ הַתַּחְבִּיב שֶׁלְּךָ וְאֵיךְ הִתְחַלְתָּ בּוֹ?',
	'מָהוּ הַחֲלוֹם שֶׁלְּךָ לַעֲתִיד?',
	'מָהוּ הַנּוֹשֵׂא הָאָהוּב עָלֶיךָ בְּלִמּוּדִים?',
	'מָהוּ הַסְּפוֹרְט הָאָהוּב עָלֶיךָ וּלְמָה?',
];

const Feedback = {
	Perfect: 'Perfect!',
	TryAgain: 'Try again, that was not accurate.',
	RecognitionError: 'Speech recognition error.',
	None: '',
} as const;

type FeedbackType = (typeof Feedback)[keyof typeof Feedback];

export const SpeakingPractice = () => {
	const classes = useStyles();

	const [showNikud, setShowNikud] = useState(false);
	const [currentIdx, setCurrentIdx] = useState(0);
	const [feedback, setFeedback] = useState<FeedbackType>(Feedback.None);
	const [isCorrect, setIsCorrect] = useState<boolean | null>(null);
	const [isRecording, setIsRecording] = useState(false);
	const [isSpeaking, setIsSpeaking] = useState(false);

	const recognizerRef = useRef<sdk.SpeechRecognizer | null>(null); // for speech recognition from microphone
	const audioConfigRef = useRef<sdk.AudioConfig | null>(null); // for audio input/output from microphone/speaker
	const synthesizerRef = useRef<sdk.SpeechSynthesizer | null>(null); // for TTS synthesis
	const playerRef = useRef<sdk.SpeakerAudioDestination | null>(null); // for TTS playback

	const speechConfig = sdk.SpeechConfig.fromSubscription(
		import.meta.env.VITE_AZURE_SPEECH_KEY!,
		import.meta.env.VITE_AZURE_REGION!
	);

	speechConfig.speechSynthesisVoiceName = 'he-IL-HilaNeural';
	speechConfig.speechRecognitionLanguage = 'he-IL';

	const comparePhrases = (
		userPhrase: string,
		targetPhrase: string
	): boolean => {
		const normalize = (str: string = '') =>
			str.replace(/[^\p{L}\p{N}\s]/gu, '').trim();

		return normalize(userPhrase) === normalize(targetPhrase);
	};

	const stopSynthesis = () => {
		if (synthesizerRef.current) {
			synthesizerRef.current.close();
			synthesizerRef.current = null;
		}
		if (playerRef.current) {
			playerRef.current.pause();
			playerRef.current.close();
			playerRef.current = null;
		}
		setIsSpeaking(false);
	};

	const stopRecognition = () => {
		if (recognizerRef.current) {
			recognizerRef.current.close();
			recognizerRef.current = null;
		}
		if (audioConfigRef.current) {
			audioConfigRef.current.close();
			audioConfigRef.current = null;
		}
		setIsRecording(false);
	};

	const handleRecord = () => {
		if (isRecording) {
			stopRecognition();
			return;
		}
		if (isSpeaking) return;
		setFeedback(Feedback.None);

		const audioConfig = sdk.AudioConfig.fromDefaultMicrophoneInput();
		const recognizer = new sdk.SpeechRecognizer(speechConfig, audioConfig);
		audioConfigRef.current = audioConfig;
		recognizerRef.current = recognizer;
		setIsRecording(true);

		recognizer.recognizeOnceAsync(
			(result) => {
				const userText = result.text ?? '';
				const correct = comparePhrases(userText, phrases[currentIdx]);
				setIsCorrect(correct);
				setFeedback(correct ? Feedback.Perfect : Feedback.TryAgain);
				stopRecognition();
			},
			(err) => {
				console.error('Recognition error:', err);
				setIsCorrect(false);
				setFeedback(Feedback.RecognitionError);
				stopRecognition();
			}
		);
	};

	const handlePlay = () => {
		if (isSpeaking) {
			stopSynthesis();
			return;
		}
		const player = new sdk.SpeakerAudioDestination();
		playerRef.current = player;

		player.onAudioStart = () => setIsSpeaking(true);
		player.onAudioEnd = () => {
			setIsSpeaking(false);
			player.close();
			playerRef.current = null;
		};

		const audioConfig = sdk.AudioConfig.fromSpeakerOutput(player);
		const synthesizer = new sdk.SpeechSynthesizer(speechConfig, audioConfig);
		synthesizerRef.current = synthesizer;

		synthesizer.speakTextAsync(
			phrases[currentIdx],
			() => {
				synthesizer.close();
				synthesizerRef.current = null;
			},
			(err) => {
				console.error('TTS error:', err);
				synthesizer.close();
				synthesizerRef.current = null;
			}
		);
	};

	const goPrev = () => {
		stopSynthesis();
		stopRecognition();
		setCurrentIdx((i) => (i === 0 ? phrases.length - 1 : i - 1));
		setFeedback(Feedback.None);
		setIsCorrect(null);
	};

	const goNext = () => {
		stopSynthesis();
		stopRecognition();
		setCurrentIdx((i) => (i + 1) % phrases.length);
		setFeedback(Feedback.None);
		setIsCorrect(null);
	};

	return (
		<div className={classes.container}>
			<div className={classes.nav}>
				<button onClick={goPrev}>&laquo; Prev</button>
				<span>
					{currentIdx + 1} / {phrases.length}
				</span>
				<button onClick={goNext}>Next &raquo;</button>
			</div>

			<div className={classes.main}>
				<h2 className={classes.phrase}>
					{showNikud ? phrasesWithNikud[currentIdx] : phrases[currentIdx]}
				</h2>

				<p
					className={`${classes.feedback} ${isCorrect ? 'correct' : 'incorrect'}`}
				>
					{feedback}
				</p>
			</div>

			<div className={classes.controls}>
				<button onClick={handlePlay}>
					{isSpeaking ? '⏹ Stop' : '▶ Play'}
				</button>
				<button onClick={handleRecord}>
					{isRecording ? '⏹ Stop' : '🎤 Record'}
				</button>
				<button onClick={() => setShowNikud(!showNikud)}>
					{showNikud ? 'Hide Nikud' : 'Show Nikud'}
				</button>
			</div>
		</div>
	);
};
