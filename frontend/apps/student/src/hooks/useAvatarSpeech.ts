// cspell:ignore viseme visemes
import { useState, useMemo, useRef, useCallback, useEffect } from "react";
import * as SpeechSDK from "microsoft-cognitiveservices-speech-sdk";
import { useAzureSpeechToken } from "@student/api";
import { CypressWindow } from "@student/types";

interface useAvatarSpeechOptions {
  lipsArray?: string[];
  volume?: number; // 0..1
  onAudioStart?: () => void;
  onAudioEnd?: () => void;
}

const VISEME_LATENCY_MS = 40; // tweak 20–70 if needed
const FALLBACK_VISEMES = [3, 5, 8, 10, 0]; // used only if no visemes arrive
const FALLBACK_STEP_MS = 60;

const stripHebrewNikud = (input: string): string => {
  // Normalize, then remove Hebrew diacritics & cantillation marks
  const noMarks = input.normalize("NFKD").replace(/[\u0591-\u05C7]/g, "");
  return noMarks.replace(/[\u200e\u200f]/g, "");
};

export const useAvatarSpeech = ({
  lipsArray = [],
  volume = 1,
  onAudioStart,
  onAudioEnd,
}: useAvatarSpeechOptions) => {
  const [currentViseme, setCurrentViseme] = useState<number>(0);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [isMuted, setIsMuted] = useState<boolean>(false);

  const {
    data: tokenData,
    refetch,
    isFetching: isFetchingToken,
    error,
  } = useAzureSpeechToken();

  const synthesizerRef = useRef<SpeechSDK.SpeechSynthesizer | null>(null);
  const speakerDestRef = useRef<SpeechSDK.SpeakerAudioDestination | null>(null);

  const timeoutsRef = useRef<NodeJS.Timeout[]>([]);
  const visemeTimelineRef = useRef<
    Array<{ visemeId: number; offsetMs: number }>
  >([]);
  const playbackStartMsRef = useRef<number | null>(null);

  const visemeMap = useMemo(
    () =>
      lipsArray.reduce(
        (acc, path, index) => {
          acc[index] = path;
          return acc;
        },
        {} as Record<number, string>,
      ),
    [lipsArray],
  );

  const clearTimeouts = useCallback(() => {
    timeoutsRef.current.forEach(clearTimeout);
    timeoutsRef.current = [];
  }, []);

  const clamp01 = (v: number) => (v < 0 ? 0 : v > 1 ? 1 : v);

  const closeSynth = useCallback(() => {
    try {
      synthesizerRef.current?.close();
    } catch {
      /* ignore */
    }
    synthesizerRef.current = null;
  }, []);

  const closeDest = useCallback(() => {
    try {
      speakerDestRef.current?.close();
    } catch {
      /* ignore */
    }
    speakerDestRef.current = null;
  }, []);

  const hardReset = useCallback(() => {
    clearTimeouts();
    closeSynth();
    closeDest();
    playbackStartMsRef.current = null;
    setIsPlaying(false);
    setCurrentViseme(0);
  }, [clearTimeouts, closeDest, closeSynth]);

  useEffect(() => {
    return () => {
      hardReset();
    };
  }, [hardReset]);

  const stop = useCallback(async () => {
    hardReset();
    onAudioEnd?.();
  }, [hardReset, onAudioEnd]);

  const toggleMute = useCallback(() => {
    setIsMuted((prev) => {
      const next = !prev;
      if (speakerDestRef.current) {
        speakerDestRef.current.volume = next ? 0 : clamp01(volume);
      }
      return next;
    });
  }, [volume]);

  useEffect(() => {
    if (speakerDestRef.current) {
      speakerDestRef.current.volume = isMuted ? 0 : clamp01(volume);
    }
  }, [isMuted, volume]);

  // Schedule a single viseme at absolute offset from playback start
  const scheduleViseme = useCallback((visemeId: number, offsetMs: number) => {
    const startedAt = playbackStartMsRef.current;
    if (startedAt == null) {
      // Not started yet — buffer; will be scheduled on onAudioStart
      visemeTimelineRef.current.push({ visemeId, offsetMs });
      return;
    }
    const elapsed = performance.now() - startedAt;
    const delay = Math.max(0, offsetMs - elapsed + VISEME_LATENCY_MS);
    const to = setTimeout(() => {
      if (!speakerDestRef.current) return;
      setCurrentViseme(visemeId);
    }, delay);
    timeoutsRef.current.push(to);
  }, []);

  const scheduleBufferedVisemes = useCallback(() => {
    // If service didn’t send visemes, drive a tiny fallback to keep lips alive
    if (visemeTimelineRef.current.length === 0) {
      FALLBACK_VISEMES.forEach((v, i) =>
        scheduleViseme(v, i * FALLBACK_STEP_MS),
      );
      return;
    }
    for (const { visemeId, offsetMs } of visemeTimelineRef.current) {
      scheduleViseme(visemeId, offsetMs);
    }
  }, [scheduleViseme]);

  const speak = useCallback(
    async (text: string, voiceName?: string) => {
      if (!text.trim()) return;

      // Always strip nikud for TTS reliability; keep original for UI
      const ttsText = stripHebrewNikud(text);

      // Cypress deterministic fake
      if (typeof window !== "undefined" && (window as CypressWindow).Cypress) {
        if (isPlaying) {
          await stop();
          return;
        }
        hardReset();
        const startTimeout = setTimeout(() => {
          onAudioStart?.();
          setIsPlaying(true);
          playbackStartMsRef.current = performance.now();

          // Use the same fallback pattern so tests are predictable
          visemeTimelineRef.current = [];
          FALLBACK_VISEMES.forEach((v, i) =>
            scheduleViseme(v, i * FALLBACK_STEP_MS),
          );

          const endTimeout = setTimeout(
            () => {
              setCurrentViseme(0);
              setIsPlaying(false);
              playbackStartMsRef.current = null;
              onAudioEnd?.();
            },
            FALLBACK_VISEMES.length * FALLBACK_STEP_MS +
              120 +
              VISEME_LATENCY_MS,
          );
          timeoutsRef.current.push(endTimeout);
        }, 10);
        timeoutsRef.current.push(startTimeout);
        return;
      }

      if (isPlaying) {
        await stop();
        // continue into a fresh speak immediately after stop
      }

      try {
        // Fresh baseline every call
        hardReset();
        visemeTimelineRef.current = [];

        // Destination drives real playback lifecycle
        const dest = new SpeechSDK.SpeakerAudioDestination();
        dest.volume = isMuted ? 0 : clamp01(volume);
        speakerDestRef.current = dest;

        dest.onAudioStart = () => {
          onAudioStart?.();
          setIsPlaying(true);
          playbackStartMsRef.current = performance.now();
          clearTimeouts();
          scheduleBufferedVisemes();
        };

        dest.onAudioEnd = () => {
          setIsPlaying(false);
          setCurrentViseme(0);
          playbackStartMsRef.current = null;
          onAudioEnd?.();
          closeDest(); // release speaker
        };

        const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(
          tokenData?.token as string,
          tokenData?.region as string,
        );
        if (voiceName) speechConfig.speechSynthesisVoiceName = voiceName;

        // Ensure viseme events are emitted
        speechConfig.setProperty(
          "SpeechServiceConnection_SynthVoiceVisemeEvent",
          "true",
        );

        const audioConfig = SpeechSDK.AudioConfig.fromSpeakerOutput(dest);
        const synthesizer = new SpeechSDK.SpeechSynthesizer(
          speechConfig,
          audioConfig,
        );
        synthesizerRef.current = synthesizer;

        // Buffer or schedule visemes as they arrive
        synthesizer.visemeReceived = (_s, e) => {
          const offsetMs = e.audioOffset / 10000;
          scheduleViseme(e.visemeId, offsetMs);
        };

        // Close synth on completion/cancel so the next call can start cleanly
        synthesizer.synthesisCompleted = () => {
          closeSynth();
          // dest.onAudioEnd will fire when speaker drains
        };

        synthesizer.SynthesisCanceled = async (_s, e) => {
          if (e.result?.errorDetails?.toLowerCase().includes("token")) {
            await refetch(); // prep for next call
          }
          closeSynth();
          // Let dest.onAudioEnd handle the rest if audio had started.
          // If audio never started, do a soft reset:
          if (!playbackStartMsRef.current) {
            setIsPlaying(false);
            setCurrentViseme(0);
            onAudioEnd?.();
            closeDest();
          }
        };

        // Kick off synthesis (this resolves when synthesis finishes, not playback)
        await new Promise<void>((resolve, reject) => {
          try {
            synthesizer.speakTextAsync(
              ttsText,
              () => resolve(),
              (err) => reject(err),
            );
          } catch (err) {
            reject(err as unknown);
          }
        });
      } catch (err) {
        console.error("Speech synthesis error:", err);
        // Full cleanup so the *next* call works
        hardReset();
        onAudioEnd?.();
      }
    },
    [
      clamp01,
      clearTimeouts,
      closeDest,
      closeSynth,
      hardReset,
      isMuted,
      isPlaying,
      onAudioEnd,
      onAudioStart,
      refetch,
      scheduleBufferedVisemes,
      scheduleViseme,
      stop,
      tokenData,
      volume,
    ],
  );

  return {
    currentViseme,
    currentVisemeSrc: visemeMap[currentViseme] ?? lipsArray[0],
    speak,
    stop,
    toggleMute,
    isPlaying,
    isMuted,
    isLoading: isFetchingToken || isPlaying,
    error,
  };
};
