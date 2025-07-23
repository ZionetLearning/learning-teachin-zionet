import { useEffect, useMemo, useRef, type JSX } from "react";

import { Html, useAnimations, useFBX, useGLTF } from "@react-three/drei";
import { useFrame, useGraph } from "@react-three/fiber";
import { useControls } from "leva";
import * as sdk from "microsoft-cognitiveservices-speech-sdk";
import * as THREE from "three";
import { SkeletonUtils, type GLTF } from "three-stdlib";

import { IdleFbx, TalkingFbx, modelGlb } from "./assets";

import { useStyles } from "./style";

type ActionName = "Idle" | "Talking";

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

// maps Azure viseme IDs to morph target names for the avatar's mouth animations.
const azureVisemeToMorph: Record<number, string> = {
  0: "viseme_PP",
  1: "viseme_PP",
  2: "viseme_FF",
  3: "viseme_TH",
  4: "viseme_DD",
  5: "viseme_KK",
  6: "viseme_CH",
  7: "viseme_SS",
  8: "viseme_NN",
  9: "viseme_RR",
  10: "viseme_oo",
  11: "viseme_O",
  12: "viseme_AA",
  13: "viseme_ee",
  14: "viseme_ee",
  15: "viseme_OO",
  16: "viseme_UU",
  17: "viseme_CH",
  18: "viseme_PP",
  19: "viseme_FF",
  20: "viseme_PP",
  21: "viseme_PP",
};

export const Avatar = (props: JSX.IntrinsicElements["group"]) => {
  const classes = useStyles();

  const group = useRef<THREE.Group>(null); // reference to the group containing the avatar for animations
  const visemeRef = useRef<number>(0); // holds the current viseme ID for speech animation
  const textRef = useRef<string>(""); // stores the text input for speech synthesis
  const fallbackRef = useRef<number | null>(null); // reference for fallback timeout to reset animations
  const isSpeakingRef = useRef<boolean>(false); // tracks if the avatar is currently speaking
  const inputEl = useRef<HTMLInputElement>(null);
  const buttonEl = useRef<HTMLButtonElement>(null);

  const { scene } = useGLTF(modelGlb); // load the GLB model (Avatar)
  const { animations: idleAnimation } = useFBX(IdleFbx); // load the idle animation
  const { animations: talkingAnimation } = useFBX(TalkingFbx); // load the talking animation

  const clone = useMemo(() => SkeletonUtils.clone(scene), [scene]); // clone the scene to avoid modifying the original
  const { nodes, materials } = useGraph(clone) as unknown as GLTFResult; // extract nodes and materials from the cloned scene

  idleAnimation[0].name = "Idle"; // set the name for the idle animation
  talkingAnimation[0].name = "Talking"; // set the name for the talking animation

  const { actions } = useAnimations(
    [idleAnimation[0], talkingAnimation[0]],
    group,
  ); // initialize animations to control idle and talking states in <group>

  const { smoothMorphTarget, morphTargetSmoothing } = useControls({
    smoothMorphTarget: true,
    morphTargetSmoothing: 0.5,
  }); // controls for morph target smoothing

  /**
   * function to handle speech synthesis using Azure Speech Service
   * it initializes the speech synthesizer, sets up event handlers for audio start and end,
   * and manages viseme events to animate the avatar's mouth.
   */
  const speak = () => {
    if (isSpeakingRef.current) return;
    if (!textRef.current.trim()) return;
    if (fallbackRef.current) clearTimeout(fallbackRef.current);

    isSpeakingRef.current = true;
    inputEl.current!.disabled = true;
    buttonEl.current!.disabled = true;

    const key = import.meta.env.VITE_AZURE_SPEECH_KEY!;
    const region = import.meta.env.VITE_AZURE_REGION!;
    const speechConfig = sdk.SpeechConfig.fromSubscription(key, region);
    speechConfig.speechSynthesisVoiceName = "he-IL-HilaNeural";
    speechConfig.setProperty(
      "SpeechServiceConnection_SynthVoiceVisemeEvent",
      "true",
    );

    const speaker = new sdk.SpeakerAudioDestination();
    // eslint-disable-next-line prefer-const
    let synth: sdk.SpeechSynthesizer;
    let lastOffset = 0; // tracks the last audio offset for fallback cleanup
    const buffered: { offsetMs: number; id: number }[] = []; // buffer for viseme events

    speaker.onAudioStart = () => {
      actions.Idle?.fadeOut(0.2);
      actions.Talking?.reset().fadeIn(0.2).play();
    };

    // clean up when audio ends: fade animations back, reset viseme, and close synthesizer
    const cleanup = () => {
      actions.Talking?.fadeOut(0.2);
      actions.Idle?.reset().fadeIn(0.2).play();
      visemeRef.current = 0;
      synth.close();
      if (fallbackRef.current) clearTimeout(fallbackRef.current);
      isSpeakingRef.current = false;
      inputEl.current!.disabled = false;
      buttonEl.current!.disabled = false;
    };
    speaker.onAudioEnd = cleanup;

    const audioConfig = sdk.AudioConfig.fromSpeakerOutput(speaker); // create audio output configuration using the speaker
    synth = new sdk.SpeechSynthesizer(speechConfig, audioConfig);

    // capture each viseme event along with its audio offset (in ms)
    synth.visemeReceived = (_s, e) => {
      const offsetMs = e.audioOffset / 10_000;
      buffered.push({ offsetMs, id: e.visemeId });
      lastOffset = Math.max(lastOffset, offsetMs);
    };

    // kick off speech synthesis
    // this will trigger the audio start event and begin processing visemes
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
        console.error("Speech error:", err);
        cleanup();
      },
    );
  };

  useFrame(
    /**
     * function animates the avatar's speech by updating morph target influences
     * it uses the viseme reference to determine which morph target to apply
     * it runs on every frame to ensure smooth transitions
     */
    function animateSpeech() {
      const head = nodes.Wolf3D_Head;
      const teeth = nodes.Wolf3D_Teeth;
      const headDict = head.morphTargetDictionary!;
      const teethDict = teeth.morphTargetDictionary!;
      const headInf = head.morphTargetInfluences!;
      const teethInf = teeth.morphTargetInfluences!;

      const clearInfluence = (idx: number) => {
        headInf[idx] = teethInf[idx] = smoothMorphTarget
          ? THREE.MathUtils.lerp(headInf[idx], 0, morphTargetSmoothing)
          : 0;
      };

      Object.values(azureVisemeToMorph).forEach((morphName) => {
        const hIdx = headDict[morphName];
        const tIdx = teethDict[morphName];
        if (hIdx != null && tIdx != null) clearInfluence(hIdx);
      });

      const morphTarget = azureVisemeToMorph[visemeRef.current];
      if (morphTarget) {
        const hIdx = headDict[morphTarget];
        const tIdx = teethDict[morphTarget];
        if (hIdx != null && tIdx != null) {
          if (!smoothMorphTarget) {
            headInf[hIdx] = teethInf[tIdx] = 1;
          } else {
            headInf[hIdx] = THREE.MathUtils.lerp(
              headInf[hIdx],
              1,
              morphTargetSmoothing,
            );
            teethInf[tIdx] = THREE.MathUtils.lerp(
              teethInf[tIdx],
              1,
              morphTargetSmoothing,
            );
          }
        }
      }
    },
  );

  useEffect(
    /**
     * function sets up the idle animation loop
     * it configures the idle animation to repeat indefinitely and plays it with a fade-in effect
     */
    function setupIdleLoop() {
      const idle = actions.Idle!;
      idle.clampWhenFinished = false;
      idle.setLoop(THREE.LoopRepeat, Infinity);
      idle.fadeIn(0.5).play();
    },
    [actions.Idle],
  );

  useEffect(
    /**
     * function sets up the talking animation loop
     * it configures the talking animation to repeat indefinitely and plays it with a fade-in effect
     */
    function setupTalkingLoop() {
      const talk = actions.Talking!;
      talk.clampWhenFinished = false;
      talk.setLoop(THREE.LoopRepeat, Infinity);
    },
    [actions.Talking],
  );

  return (
    <>
      <Html fullscreen>
        <div className={classes.inputContainer}>
          <input
            ref={inputEl}
            name="speechInput"
            type="text"
            dir="rtl"
            placeholder="כתוב פה משהו בעברית…"
            defaultValue=""
            onChange={(e) => {
              textRef.current = e.currentTarget.value;
            }}
            className={classes.input}
            autoComplete="off"
          />
          <button
            ref={buttonEl}
            onClick={speak}
            className={classes.inputButton}
          >
            דברי
          </button>
        </div>
      </Html>
      <group name="Armature" {...props} dispose={null} ref={group}>
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
};

useGLTF.preload(modelGlb);
