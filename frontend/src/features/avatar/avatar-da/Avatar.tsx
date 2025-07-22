import React, { useEffect, type JSX } from 'react';

import { Html, useAnimations, useFBX, useGLTF } from '@react-three/drei';
import { useFrame, useGraph } from '@react-three/fiber';
import { useControls } from 'leva';
import * as sdk from 'microsoft-cognitiveservices-speech-sdk';
import { useRef } from 'react';
import * as THREE from 'three';
import { SkeletonUtils, type GLTF } from 'three-stdlib';

type ActionName = 'Idle' | 'Talking';

interface GLTFAction extends THREE.AnimationClip {
	name: ActionName;
}

type GLTFResult = GLTF & {
	nodes: {
		Wolf3D_Hair: THREE.SkinnedMesh;
		Wolf3D_Outfit_Top: THREE.SkinnedMesh;
		Wolf3D_Outfit_Bottom: THREE.SkinnedMesh;
		Wolf3D_Outfit_Footwear: THREE.SkinnedMesh;
		Wolf3D_Body: THREE.SkinnedMesh;
		EyeLeft: THREE.SkinnedMesh;
		EyeRight: THREE.SkinnedMesh;
		Wolf3D_Head: THREE.SkinnedMesh;
		Wolf3D_Teeth: THREE.SkinnedMesh;
		Hips: THREE.Bone;
	};
	materials: {
		Wolf3D_Hair: THREE.MeshStandardMaterial;
		Wolf3D_Outfit_Top: THREE.MeshStandardMaterial;
		Wolf3D_Outfit_Bottom: THREE.MeshStandardMaterial;
		Wolf3D_Outfit_Footwear: THREE.MeshStandardMaterial;
		Wolf3D_Body: THREE.MeshStandardMaterial;
		Wolf3D_Eye: THREE.MeshStandardMaterial;
		Wolf3D_Skin: THREE.MeshStandardMaterial;
		Wolf3D_Teeth: THREE.MeshStandardMaterial;
	};
	animations: GLTFAction[];
};

const azureVisemeToMorph: Record<number, string> = {
	0: 'viseme_PP',
	1: 'viseme_PP',
	2: 'viseme_FF',
	3: 'viseme_TH',
	4: 'viseme_DD',
	5: 'viseme_KK',
	6: 'viseme_CH',
	7: 'viseme_SS',
	8: 'viseme_NN',
	9: 'viseme_RR',
	10: 'viseme_oo',
	11: 'viseme_O',
	12: 'viseme_AA',
	13: 'viseme_ee',
	14: 'viseme_ee',
	15: 'viseme_OO',
	16: 'viseme_UU',
	17: 'viseme_CH',
	18: 'viseme_PP',
	19: 'viseme_FF',
	20: 'viseme_PP',
	21: 'viseme_PP',
};

export function Avatar(props: JSX.IntrinsicElements['group']) {
	const group = useRef<THREE.Group>(null);
	const visemeRef = useRef(0);
	const textRef = useRef<string>('');
	const fallbackRef = useRef<number | null>(null);
	const { scene } = useGLTF(
		'/../src/public/avatar/avatar-da/models/687e38191c3d7336a8763d55.glb'
	);

	const { animations: idleAnimation } = useFBX(
		'/../src/public/avatar/avatar-da/animations/Idle.fbx'
	);
	const { animations: talkingAnimation } = useFBX(
		'/../src/public/avatar/avatar-da/animations/Talking.fbx'
	);

	idleAnimation[0].name = 'Idle';
	talkingAnimation[0].name = 'Talking';

	const { actions } = useAnimations(
		[idleAnimation[0], talkingAnimation[0]],
		group
	);

	const { smoothMorphTarget, morphTargetSmoothing } = useControls({
		smoothMorphTarget: true,
		morphTargetSmoothing: 0.5,
	});

	const speak = () => {
		if (!textRef.current.trim()) return;
		if (fallbackRef.current) clearTimeout(fallbackRef.current);

		const key = import.meta.env.VITE_AZURE_SPEECH_KEY!;
		const region = import.meta.env.VITE_AZURE_REGION!;
		const speechConfig = sdk.SpeechConfig.fromSubscription(key, region);
		speechConfig.speechSynthesisVoiceName = 'he-IL-HilaNeural';
		speechConfig.setProperty(
			'SpeechServiceConnection_SynthVoiceVisemeEvent',
			'true'
		);

		const speaker = new sdk.SpeakerAudioDestination();
		// eslint-disable-next-line prefer-const
		let synth: sdk.SpeechSynthesizer;
		let lastOffset = 0;
		const buffered: { offsetMs: number; id: number }[] = [];

		speaker.onAudioStart = () => {
			actions.Idle?.fadeOut(0.2);
			actions.Talking?.reset().fadeIn(0.2).play();
		};

		const cleanup = () => {
			actions.Talking?.fadeOut(0.2);
			actions.Idle?.reset().play();
			visemeRef.current = 0;
			synth.close();
			if (fallbackRef.current) clearTimeout(fallbackRef.current);
		};
		speaker.onAudioEnd = cleanup;

		const audioConfig = sdk.AudioConfig.fromSpeakerOutput(speaker);
		synth = new sdk.SpeechSynthesizer(speechConfig, audioConfig);

		synth.visemeReceived = (_s, e) => {
			const offsetMs = e.audioOffset / 10_000;
			buffered.push({ offsetMs, id: e.visemeId });
			lastOffset = Math.max(lastOffset, offsetMs);
		};

		synth.speakTextAsync(
			textRef.current,
			() => {
				buffered.forEach(({ offsetMs, id }) => {
					setTimeout(() => {
						visemeRef.current = id;
					}, offsetMs);
				});
				fallbackRef.current = window.setTimeout(cleanup, lastOffset + 200);
			},
			(err) => {
				console.error('Speech error:', err);
				cleanup();
			}
		);
	};

	useFrame(() => {
		Object.values(azureVisemeToMorph).forEach((morphName) => {
			const headIdx = nodes.Wolf3D_Head.morphTargetDictionary?.[morphName];
			const teethIdx = nodes.Wolf3D_Teeth.morphTargetDictionary?.[morphName];

			if (headIdx === undefined || teethIdx === undefined) return;

			if (!smoothMorphTarget) {
				if (
					nodes.Wolf3D_Head.morphTargetInfluences &&
					nodes.Wolf3D_Teeth.morphTargetInfluences
				) {
					nodes.Wolf3D_Head.morphTargetInfluences[headIdx] = 0;
					nodes.Wolf3D_Teeth.morphTargetInfluences[teethIdx] = 0;
				}
			} else {
				if (
					nodes.Wolf3D_Head.morphTargetInfluences &&
					nodes.Wolf3D_Teeth.morphTargetInfluences
				) {
					nodes.Wolf3D_Head.morphTargetInfluences[headIdx] =
						THREE.MathUtils.lerp(
							nodes.Wolf3D_Head.morphTargetInfluences[headIdx],
							0,
							morphTargetSmoothing
						);
					nodes.Wolf3D_Teeth.morphTargetInfluences[teethIdx] =
						THREE.MathUtils.lerp(
							nodes.Wolf3D_Teeth.morphTargetInfluences[teethIdx],
							0,
							morphTargetSmoothing
						);
				}
			}
		});

		const morphName = azureVisemeToMorph[visemeRef.current];
		if (morphName) {
			const headIdx = nodes.Wolf3D_Head.morphTargetDictionary?.[morphName];
			const teethIdx = nodes.Wolf3D_Teeth.morphTargetDictionary?.[morphName];

			if (headIdx === undefined || teethIdx === undefined) return;
			if (!smoothMorphTarget) {
				if (
					nodes.Wolf3D_Head.morphTargetInfluences &&
					nodes.Wolf3D_Teeth.morphTargetInfluences
				) {
					nodes.Wolf3D_Head.morphTargetInfluences[headIdx] = 1;
					nodes.Wolf3D_Teeth.morphTargetInfluences[teethIdx] = 1;
				}
			} else {
				if (
					nodes.Wolf3D_Head.morphTargetInfluences &&
					nodes.Wolf3D_Teeth.morphTargetInfluences
				) {
					nodes.Wolf3D_Head.morphTargetInfluences[headIdx] =
						THREE.MathUtils.lerp(
							nodes.Wolf3D_Head.morphTargetInfluences[headIdx],
							1,
							morphTargetSmoothing
						);
					nodes.Wolf3D_Teeth.morphTargetInfluences[teethIdx] =
						THREE.MathUtils.lerp(
							nodes.Wolf3D_Teeth.morphTargetInfluences[teethIdx],
							1,
							morphTargetSmoothing
						);
				}
			}
		}
	});

	useEffect(() => {
		const idle = actions.Idle!;
		idle.clampWhenFinished = false;
		idle.setLoop(THREE.LoopRepeat, Infinity);
		idle.reset().fadeIn(0.5).play();
	}, [actions.Idle]);

	useEffect(() => {
		const talk = actions.Talking!;
		talk.clampWhenFinished = false;
		talk.setLoop(THREE.LoopRepeat, Infinity);
	}, [actions.Talking]);

	const clone = React.useMemo(() => SkeletonUtils.clone(scene), [scene]);
	const { nodes, materials } = useGraph(clone) as unknown as GLTFResult;

	return (
		<>
			<Html fullscreen style={{ pointerEvents: 'auto' }}>
				<div
					style={{
						position: 'absolute',
						top: 20,
						left: 20,
						background: 'rgba(255,255,255,0.9)',
						padding: '8px',
						borderRadius: '4px',
					}}
				>
					<input
						type="text"
						dir="rtl"
						placeholder="כתוב פה משהו בעברית…"
						defaultValue=""
						onChange={(e) => {
							textRef.current = e.currentTarget.value;
						}}
						style={{ width: '200px' }}
					/>
					<button onClick={speak} style={{ marginLeft: 8 }}>
						דברי
					</button>
				</div>
			</Html>
			<group {...props} dispose={null} ref={group}>
				<primitive object={nodes.Hips} />
				<skinnedMesh
					geometry={nodes.Wolf3D_Hair.geometry}
					material={materials.Wolf3D_Hair}
					skeleton={nodes.Wolf3D_Hair.skeleton}
				/>
				<skinnedMesh
					geometry={nodes.Wolf3D_Outfit_Top.geometry}
					material={materials.Wolf3D_Outfit_Top}
					skeleton={nodes.Wolf3D_Outfit_Top.skeleton}
				/>
				<skinnedMesh
					geometry={nodes.Wolf3D_Outfit_Bottom.geometry}
					material={materials.Wolf3D_Outfit_Bottom}
					skeleton={nodes.Wolf3D_Outfit_Bottom.skeleton}
				/>
				<skinnedMesh
					geometry={nodes.Wolf3D_Outfit_Footwear.geometry}
					material={materials.Wolf3D_Outfit_Footwear}
					skeleton={nodes.Wolf3D_Outfit_Footwear.skeleton}
				/>
				<skinnedMesh
					geometry={nodes.Wolf3D_Body.geometry}
					material={materials.Wolf3D_Body}
					skeleton={nodes.Wolf3D_Body.skeleton}
				/>
				<skinnedMesh
					name="EyeLeft"
					geometry={nodes.EyeLeft.geometry}
					material={materials.Wolf3D_Eye}
					skeleton={nodes.EyeLeft.skeleton}
					morphTargetDictionary={nodes.EyeLeft.morphTargetDictionary}
					morphTargetInfluences={nodes.EyeLeft.morphTargetInfluences}
				/>
				<skinnedMesh
					name="EyeRight"
					geometry={nodes.EyeRight.geometry}
					material={materials.Wolf3D_Eye}
					skeleton={nodes.EyeRight.skeleton}
					morphTargetDictionary={nodes.EyeRight.morphTargetDictionary}
					morphTargetInfluences={nodes.EyeRight.morphTargetInfluences}
				/>
				<skinnedMesh
					name="Wolf3D_Head"
					geometry={nodes.Wolf3D_Head.geometry}
					material={materials.Wolf3D_Skin}
					skeleton={nodes.Wolf3D_Head.skeleton}
					morphTargetDictionary={nodes.Wolf3D_Head.morphTargetDictionary}
					morphTargetInfluences={nodes.Wolf3D_Head.morphTargetInfluences}
				/>
				<skinnedMesh
					name="Wolf3D_Teeth"
					geometry={nodes.Wolf3D_Teeth.geometry}
					material={materials.Wolf3D_Teeth}
					skeleton={nodes.Wolf3D_Teeth.skeleton}
					morphTargetDictionary={nodes.Wolf3D_Teeth.morphTargetDictionary}
					morphTargetInfluences={nodes.Wolf3D_Teeth.morphTargetInfluences}
				/>
			</group>
		</>
	);
}

useGLTF.preload(
	'/../src/public/avatar/avatar-da/models/687e38191c3d7336a8763d55.glb'
);
