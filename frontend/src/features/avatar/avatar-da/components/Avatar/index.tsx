import { useEffect, useMemo, useRef, type JSX } from "react";
import { useTranslation } from "react-i18next";
import { Html, useAnimations, useFBX, useGLTF } from "@react-three/drei";
import { useFrame, useGraph, type ThreeElements } from "@react-three/fiber";
import { useControls } from "leva";
import * as sdk from "microsoft-cognitiveservices-speech-sdk";
import * as THREE from "three";
import { SkeletonUtils, type GLTF } from "three-stdlib";

import { IdleFbx, TalkingFbx, modelGlb } from "../../assets";
import { CypressWindow } from "@/types";

import { useStyles } from "./style";

declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace React {
    // eslint-disable-next-line @typescript-eslint/no-namespace
    namespace JSX {
      // eslint-disable-next-line @typescript-eslint/no-empty-object-type
      interface IntrinsicElements extends ThreeElements {}
    }
  }
}

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

// maps Azure viseme IDs to morph target names for the avatar's mouth animations
const azureVisemeToMorph: Record<number, string> = {
  0: "viseme_sil",
  1: "viseme_PP",
  2: "viseme_FF",
  3: "viseme_TH",
  4: "viseme_DD",
  5: "viseme_kk",
  6: "viseme_CH",
  7: "viseme_SS",
  8: "viseme_nn",
  9: "viseme_RR",
  10: "viseme_aa",
  11: "viseme_E",
  12: "viseme_I",
  13: "viseme_O",
  14: "viseme_U",
  15: "viseme_OO",
  16: "viseme_sil",
  17: "viseme_sil",
  18: "viseme_PP",
  19: "viseme_sil",
  20: "viseme_sil",
  21: "viseme_sil",
};

/**
 * Avatar
 *
 * renders a Wolf3D‐based 3D character with idle/talking FBX animations
 * and Azure‐powered lip‐sync via morph targets.
 *
 * props:
 *   – all standard <group> props (position, rotation, scale, etc.)
 */
export const Avatar = (props: JSX.IntrinsicElements["group"]) => {
  const classes = useStyles();
  const { t } = useTranslation();
  const group = useRef<THREE.Group>(null); // reference to the group containing the avatar for animations
  const visemeRef = useRef<number>(0); // holds the current viseme ID for speech animation
  const textRef = useRef<string>(""); // stores the text input for speech synthesis
  const fallbackRef = useRef<number | null>(null); // reference for fallback timeout to reset animations
  const isSpeakingRef = useRef<boolean>(false); // tracks if the avatar is currently speaking
  const inputEl = useRef<HTMLInputElement>(null);
  const buttonEl = useRef<HTMLButtonElement>(null);
  const morphPairsRef = useRef<[number, number][]>([]); // pairs of morph target indices for head and teeth animations
  const visemeIndexRef = useRef<Record<number, { h: number; t: number }>>({}); // maps viseme IDs to morph target indices for head and teeth

  const { scene } = useGLTF(modelGlb); // load the GLB model (Avatar)
  const { animations: idleAnimation } = useFBX(IdleFbx); // load the idle animation
  const { animations: talkingAnimation } = useFBX(TalkingFbx); // load the talking animation

  const clone = useMemo(() => SkeletonUtils.clone(scene), [scene]); // clone the scene to avoid modifying the original
  const { nodes, materials } = useGraph(clone) as unknown as GLTFResult; // extract nodes and materials from the cloned scene

  useEffect(
    /**
     * updates the morph targets for the avatar's head and teeth based on the current viseme
     */
    function updateMorphTargets() {
      if (!nodes?.Wolf3D_Head || !nodes?.Wolf3D_Teeth) return;
      const headDict = nodes.Wolf3D_Head.morphTargetDictionary!;
      const teethDict = nodes.Wolf3D_Teeth.morphTargetDictionary!;

      morphPairsRef.current = Object.values(azureVisemeToMorph)
        .map((name) => {
          const h = headDict[name],
            t = teethDict[name];
          return h != null && t != null ? ([h, t] as [number, number]) : null;
        })
        .filter((x): x is [number, number] => !!x);

      visemeIndexRef.current = Object.entries(azureVisemeToMorph).reduce(
        (acc, [id, name]) => {
          const h = headDict[name],
            t = teethDict[name];
          if (h != null && t != null) acc[+id] = { h, t };
          return acc;
        },
        {} as Record<number, { h: number; t: number }>,
      );
    },
    [nodes],
  );

  idleAnimation[0].name = "Idle"; // set the name for the idle animation
  talkingAnimation[0].name = "Talking"; // set the name for the talking animation

  const { actions } = useAnimations(
    [idleAnimation[0], talkingAnimation[0]],
    group,
  ); // initialize animations to control idle and talking states in <group>

  useEffect(
    /**
     * function sets up the idle animation loop
     * it configures the idle animation to repeat indefinitely and plays it with a fade-in effect
     */
    function setupIdleLoop() {
      const idle = actions.Idle!;
      idle.setLoop(THREE.LoopRepeat, Infinity);
      idle.fadeIn(0.5).play();
    },
    [actions.Idle],
  );

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

    // Cypress test environment shortcut: avoid real Azure call, simulate brief speaking cycle
    if (typeof window !== "undefined") {
      const w = window as CypressWindow;
      if (w.Cypress) {
        const simulatedVisemes = [10, 11, 12, 13, 0];
        simulatedVisemes.forEach((v, i) => {
          setTimeout(() => {
            visemeRef.current = v;
          }, i * 120);
        });
        fallbackRef.current = window.setTimeout(
          () => {
            isSpeakingRef.current = false;
            visemeRef.current = 0;
            if (inputEl.current) {
              inputEl.current.disabled = false;
            }
            if (buttonEl.current) {
              buttonEl.current.disabled = false;
            }
          },
          simulatedVisemes.length * 120 + 80,
        );
        return;
      }
    }

    const key = import.meta.env.VITE_AZURE_SPEECH_KEY!;
    const region = import.meta.env.VITE_AZURE_REGION!;
    const speechConfig = sdk.SpeechConfig.fromSubscription(key, region);
    speechConfig.speechSynthesisVoiceName = "he-IL-HilaNeural";
    speechConfig.setProperty(
      "SpeechServiceConnection_SynthVoiceVisemeEvent",
      "true",
    );
    let lastOffset = 0; // tracks the last audio offset for fallback cleanup
    const speaker = new sdk.SpeakerAudioDestination();
    const audioConfig = sdk.AudioConfig.fromSpeakerOutput(speaker); // create audio output configuration using the speaker
    const synth = new sdk.SpeechSynthesizer(speechConfig, audioConfig);
    const buffered: { offsetMs: number; id: number }[] = []; // buffer for viseme events

    speaker.onAudioStart = () => {
      actions.Idle!.crossFadeTo(actions.Talking!, 0.2, false);
      actions.Talking!.reset().play();
    };

    // clean up when audio ends: fade animations back, reset viseme, and close synthesizer
    const cleanup = () => {
      actions.Talking!.crossFadeTo(actions.Idle!, 0.5, false);
      actions.Idle!.reset().play();
      visemeRef.current = 0;
      synth.close();
      if (fallbackRef.current) clearTimeout(fallbackRef.current);
      isSpeakingRef.current = false;
      inputEl.current!.disabled = false;
      buttonEl.current!.disabled = false;
    };
    speaker.onAudioEnd = cleanup;

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
     * it lerps the morph target influences for the head and teeth based on the current viseme
     * if the avatar is not speaking, it resets the morph targets to 0
     */
    function animateSpeech() {
      if (!isSpeakingRef.current) return;

      const headInf = nodes.Wolf3D_Head.morphTargetInfluences!;
      const teethInf = nodes.Wolf3D_Teeth.morphTargetInfluences!;

      morphPairsRef.current.forEach(([h, t]) => {
        headInf[h] = smoothMorphTarget
          ? THREE.MathUtils.lerp(headInf[h], 0, morphTargetSmoothing)
          : 0;
        teethInf[t] = smoothMorphTarget
          ? THREE.MathUtils.lerp(teethInf[t], 0, morphTargetSmoothing)
          : 0;
      });

      const curr = visemeIndexRef.current[visemeRef.current];
      if (curr) {
        if (smoothMorphTarget) {
          headInf[curr.h] = THREE.MathUtils.lerp(
            headInf[curr.h],
            1,
            morphTargetSmoothing,
          );
          teethInf[curr.t] = THREE.MathUtils.lerp(
            teethInf[curr.t],
            1,
            morphTargetSmoothing,
          );
        } else {
          headInf[curr.h] = teethInf[curr.t] = 1;
        }
      }
    },
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
            placeholder={t("pages.avatarDa.writeSomethingHereInHebrew")}
            defaultValue=""
            onChange={(e) => {
              textRef.current = e.currentTarget.value;
            }}
            className={classes.input}
            autoComplete="off"
            data-testid="avatar-da-input"
          />
          <button
            ref={buttonEl}
            onClick={speak}
            className={classes.inputButton}
            data-testid="avatar-da-speak"
          >
            {t("pages.avatarDa.speak")}
          </button>
        </div>
      </Html>
      <group name="group" {...props} dispose={null} ref={group}>
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
