import { ReactElement, ReactNode } from "react";
import { vi } from "vitest";
import "@testing-library/jest-dom/vitest";

const IGNORE = [
  /is using incorrect casing/i,
  /The tag <.*> is unrecognized in this browser/i,
  /React does not recognize the .* prop on a DOM element/i,
];
const origError = console.error;
const origWarn = console.warn;
console.error = (...args: unknown[]) => {
  const msg = String(args[0] ?? "");
  if (IGNORE.some((re) => re.test(msg))) return;
  origError(...args);
};
console.warn = (...args: unknown[]) => {
  const msg = String(args[0] ?? "");
  if (IGNORE.some((re) => re.test(msg))) return;
  origWarn(...args);
};

/* ========= i18n ========= */
vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (k: string) =>
      k === "pages.avatarDa.writeSomethingHereInHebrew"
        ? "Write something here in Hebrew"
        : k === "pages.avatarDa.speak"
          ? "Speak"
          : k,
  }),
}));

/* ===== avatar assets ===== */
vi.mock("@features/avatar/avatar-da/assets", () => ({
  backgroundJpg: "mock-bg.jpg",
  IdleFbx: "mock-idle.fbx",
  TalkingFbx: "mock-talking.fbx",
  modelGlb: "mock-model.glb",
}));

/* ===== three-stdlib ===== */
vi.mock("three-stdlib", () => ({
  SkeletonUtils: { clone: (x: unknown) => x },
}));

/* === @react-three/fiber === */
type Viewport = { width: number; height: number };
type ThreeState = { viewport: Viewport };
type UseThreeSelector<T> = (s: ThreeState) => T;

vi.mock("@react-three/fiber", () => {
  const useThree = <T,>(sel: UseThreeSelector<T>): T =>
    sel({ viewport: { width: 10, height: 5 } });
  const useFrame = vi.fn<(cb: () => void) => void>(() => {});
  const useGraph = vi.fn(() => {
    const names = [
      "viseme_sil",
      "viseme_PP",
      "viseme_FF",
      "viseme_TH",
      "viseme_DD",
      "viseme_kk",
      "viseme_CH",
      "viseme_SS",
      "viseme_nn",
      "viseme_RR",
      "viseme_aa",
      "viseme_E",
      "viseme_I",
      "viseme_O",
      "viseme_U",
      "viseme_OO",
    ];
    const dict: Record<string, number> = Object.fromEntries(
      names.map((n, i) => [n, i]),
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

/* ==== @react-three/drei ==== */
vi.mock("@react-three/drei", () => {
  const mkAction = () => ({
    setLoop: vi.fn().mockReturnThis(),
    fadeIn: vi.fn().mockReturnThis(),
    play: vi.fn().mockReturnThis(),
    crossFadeTo: vi.fn(),
    reset: vi.fn().mockReturnThis(),
  });

  const useGLTF: ((path: string) => { scene: object }) & {
    preload: (path: string) => void;
  } = Object.assign(
    vi.fn(() => ({ scene: {} })),
    {
      preload: vi.fn(() => undefined),
    },
  );
  const useFBX = vi.fn(() => ({ animations: [{}, {}] as unknown[] }));
  const useAnimations = vi.fn(() => ({
    actions: { Idle: mkAction(), Talking: mkAction() },
  }));

  const Html = ({ children }: { children: ReactNode }) => (
    <div data-testid="html">{children}</div>
  );
  const Environment = () => <div data-testid="environment" />;
  const useTexture = vi.fn(() => ({}));

  return { Html, Environment, useTexture, useGLTF, useFBX, useAnimations };
});

/* ========= leva ========= */
vi.mock("leva", () => ({
  useControls: vi.fn(() => ({
    smoothMorphTarget: true,
    morphTargetSmoothing: 0.5,
  })),
  Leva: () => <div data-testid="leva" />,
}));

/* ====== hook under test: we only need speak ====== */
export const mockSpeak = vi.fn();
vi.mock("@/hooks/useAvatarSpeech", () => ({
  useAvatarSpeech: vi.fn(() => ({
    currentViseme: 0,
    speak: mockSpeak,
    isPlaying: false,
    isLoading: false,
    stop: vi.fn(),
    toggleMute: vi.fn(),
    isMuted: false,
    error: null,
  })),
}));
