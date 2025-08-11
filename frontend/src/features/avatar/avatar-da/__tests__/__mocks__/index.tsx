import { ReactElement, ReactNode } from 'react';
import { vi } from 'vitest';
import '@testing-library/jest-dom/vitest';

const IGNORE = [
	/is using incorrect casing/i,
	/The tag <.*> is unrecognized in this browser/i,
	/React does not recognize the .* prop on a DOM element/i,
];

const origError = console.error;
const origWarn = console.warn;

// suppress specific warnings that are not relevant to tests
console.error = (...args: unknown[]) => {
	const msg = String(args[0] ?? '');
	if (IGNORE.some((re) => re.test(msg))) return;
	origError(...args);
};
console.warn = (...args: unknown[]) => {
	const msg = String(args[0] ?? '');
	if (IGNORE.some((re) => re.test(msg))) return;
	origWarn(...args);
};

type Viewport = { width: number; height: number };
type ThreeState = { viewport: Viewport };
type UseThreeSelector<T> = (s: ThreeState) => T;

type AnimationAction = {
	setLoop: (mode: number, repetitions: number) => AnimationAction;
	fadeIn: (t: number) => AnimationAction;
	play: () => AnimationAction;
	crossFadeTo: (
		other: AnimationAction,
		duration: number,
		warp: boolean
	) => void;
	reset: () => AnimationAction;
};

type UseAnimationsReturn = {
	actions: Record<'Idle' | 'Talking', AnimationAction>;
};

type SpeakerDestination = {
	onAudioStart?: () => void;
	onAudioEnd?: () => void;
};
type AudioCfg = { _dest?: SpeakerDestination };

type VisemeEvent = { audioOffset: number; visemeId: number };
type SpeechSynth = {
	visemeReceived?: (sender: unknown, e: VisemeEvent) => void;
	speakTextAsync: (
		text: string,
		onSuccess: () => void,
		onError?: (err: unknown) => void
	) => void;
	close: () => void;
};

/* ========================= i18n ========================= */
vi.mock('react-i18next', () => ({
	useTranslation: () => ({
		t: (k: string) =>
			k === 'pages.avatarDa.writeSomethingHereInHebrew'
				? 'Write something here in Hebrew'
				: k === 'pages.avatarDa.speak'
					? 'Speak'
					: k,
	}),
}));

/* ==================== feature assets ==================== */
vi.mock('@features/avatar/avatar-da/assets', () => ({
	backgroundJpg: 'mock-bg.jpg',
	IdleFbx: 'mock-idle.fbx',
	TalkingFbx: 'mock-talking.fbx',
	modelGlb: 'mock-model.glb',
}));

/* ====================== three-stdlib ===================== */
vi.mock('three-stdlib', () => ({
	SkeletonUtils: { clone: (x: unknown) => x },
}));

/* =================== @react-three/fiber ================== */
vi.mock('@react-three/fiber', () => {
	const useThree = <T,>(sel: UseThreeSelector<T>): T =>
		sel({ viewport: { width: 10, height: 5 } });

	const useFrame = vi.fn<(cb: () => void) => void>(() => {});

	const useGraph = vi.fn<(obj: unknown) => unknown>(() => {
		const names = [
			'viseme_sil',
			'viseme_PP',
			'viseme_FF',
			'viseme_TH',
			'viseme_DD',
			'viseme_kk',
			'viseme_CH',
			'viseme_SS',
			'viseme_nn',
			'viseme_RR',
			'viseme_aa',
			'viseme_E',
			'viseme_I',
			'viseme_O',
			'viseme_U',
			'viseme_OO',
		];
		const dict: Record<string, number> = Object.fromEntries(
			names.map((n, i) => [n, i])
		);
		const infl = new Array(names.length).fill(0) as number[];
		const node = {
			geometry: {},
			skeleton: {},
			morphTargetDictionary: dict,
			morphTargetInfluences: infl.slice(),
		};
		return {
			nodes: {
				Wolf3D_Hair: node,
				Wolf3D_Outfit_Top: node,
				Wolf3D_Outfit_Bottom: node,
				Wolf3D_Outfit_Footwear: node,
				Wolf3D_Body: node,
				EyeLeft: node,
				EyeRight: node,
				Wolf3D_Head: node,
				Wolf3D_Teeth: node,
				Hips: {},
			},
			materials: {
				Wolf3D_Hair: {},
				Wolf3D_Outfit_Top: {},
				Wolf3D_Outfit_Bottom: {},
				Wolf3D_Outfit_Footwear: {},
				Wolf3D_Body: {},
				Wolf3D_Eye: {},
				Wolf3D_Skin: {},
				Wolf3D_Teeth: {},
			},
		};
	});

	const Canvas = ({
		children,
		className,
	}: {
		children: ReactNode;
		className?: string;
	}): ReactElement => (
		<div data-testid="canvas" className={className}>
			{children}
		</div>
	);

	return { Canvas, useThree, useFrame, useGraph };
});

/* =================== @react-three/drei =================== */
vi.mock('@react-three/drei', () => {
	const mkAction = (): AnimationAction => ({
		setLoop: vi.fn().mockReturnThis(),
		fadeIn: vi.fn().mockReturnThis(),
		play: vi.fn().mockReturnThis(),
		crossFadeTo: vi.fn(),
		reset: vi.fn().mockReturnThis(),
	});

	type UseGLTFFn = ((src: string) => { scene: object }) & {
		preload: (src: string) => void;
	};
	const useGLTF: UseGLTFFn = Object.assign(
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		vi.fn((_: string) => ({ scene: {} })),
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		{ preload: vi.fn((_: string) => undefined) }
	);

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	const useFBX = vi.fn((_: string) => ({ animations: [{}, {}] as unknown[] }));

	const useAnimations = vi.fn(
		() =>
			({
				actions: { Idle: mkAction(), Talking: mkAction() },
			}) as UseAnimationsReturn
	);

	const Leva = () => null;
	const Environment = () => <div data-testid="environment" />;
	const Html = ({ children }: { children: ReactNode }) => (
		<div data-testid="html">{children}</div>
	);
	const useTexture = vi.fn(() => ({}));

	return {
		Leva,
		Environment,
		Html,
		useTexture,
		useGLTF,
		useFBX,
		useAnimations,
	};
});

/* ========== azure cognitive services speech sdk ========== */
vi.mock('microsoft-cognitiveservices-speech-sdk', () => {
	const SpeechConfig = {
		fromSubscription: vi.fn(
			(): {
				setProperty: (k: string, v: string) => void;
				speechSynthesisVoiceName: string;
			} => ({
				setProperty: vi.fn(),
				speechSynthesisVoiceName: '',
			})
		),
	};

	const AudioConfig = {
		fromSpeakerOutput: vi.fn(
			(dest: SpeakerDestination): AudioCfg => ({ _dest: dest })
		),
	};

	const SpeakerAudioDestination = vi.fn(function (this: SpeakerDestination) {
		this.onAudioStart = undefined;
		this.onAudioEnd = undefined;
	});

	const SpeechSynthesizer = vi.fn(function (
		this: SpeechSynth,
		_cfg: unknown,
		audioCfg?: AudioCfg
	) {
		const dest = audioCfg?._dest;
		this.visemeReceived = undefined;
		this.speakTextAsync = (_txt: string, ok: () => void) => {
			dest?.onAudioStart?.();
			this.visemeReceived?.(null, { audioOffset: 10_000_000, visemeId: 10 });
			ok();
		};
		this.close = () => {};
	});

	return {
		SpeechConfig,
		AudioConfig,
		SpeakerAudioDestination,
		SpeechSynthesizer,
	};
});
