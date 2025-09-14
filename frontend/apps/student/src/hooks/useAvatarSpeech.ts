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

const SPEECH_REGION = import.meta.env.VITE_AZURE_REGION as string | undefined;

export const useAvatarSpeech = ({
  lipsArray = [],
  volume = 1,
  onAudioStart,
  onAudioEnd,
}: useAvatarSpeechOptions) => {
  const [currentViseme, setCurrentViseme] = useState<number>(0);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [isMuted, setIsMuted] = useState<boolean>(false);

  const { data: tokenData, refetch, isFetching: isFetchingToken, error } = useAzureSpeechToken();

  const synthesizerRef = useRef<SpeechSDK.SpeechSynthesizer | null>(null);
  const speakerDestRef = useRef<SpeechSDK.SpeakerAudioDestination | null>(null);

  const timeoutsRef = useRef<NodeJS.Timeout[]>([]);
  const visemeTimelineRef = useRef<Array<{ visemeId: number; offsetMs: number }>>([]);

  const visemeMap = useMemo(() =>
      lipsArray.reduce((acc, path, index) => {
        acc[index] = path;
        return acc;
      }, {} as Record<number, string>),
    [lipsArray]
  );

  const clearTimeouts = useCallback(() => {
    timeoutsRef.current.forEach(clearTimeout);
    timeoutsRef.current = [];
  }, []);

  const closeSynthAndDest = useCallback(() => {
    try {
      synthesizerRef.current?.close();
    } catch {
      /* ignore */
    }
    synthesizerRef.current = null;

    try {
      speakerDestRef.current?.close();
    } catch {
      /* ignore */
    }
    speakerDestRef.current = null;
  }, []);

  useEffect(() => {
    return () => {
      closeSynthAndDest();
      clearTimeouts();
    };
  }, [clearTimeouts, closeSynthAndDest]);

  const stop = useCallback(async () => {
    closeSynthAndDest();
    setIsPlaying(false);
    setCurrentViseme(0);
    onAudioEnd?.();
  }, [closeSynthAndDest, onAudioEnd]);

  const clamp01 = (v: number) => (v < 0 ? 0 : v > 1 ? 1 : v);

  const toggleMute = useCallback(() => {
    setIsMuted((prev) => {
      const next = !prev;
      if (speakerDestRef.current) {
        speakerDestRef.current.volume = next ? 0 : clamp01(volume);
      }
      return next;
    });
  }, [volume]);

  // Keep volume in sync if it changes mid-utterance
  useEffect(() => {
    if (speakerDestRef.current) {
      speakerDestRef.current.volume = isMuted ? 0 : clamp01(volume);
    }
  }, [isMuted, volume]);

  const speak = useCallback(
    async (text: string, voiceName?: string) => {
      if (!text.trim()) return;

      // Cypress simulation (no network/Azure)
      if (typeof window !== "undefined" && (window as CypressWindow).Cypress) {
        if (isPlaying) {
          await stop();
          return;
        }
        clearTimeouts();
        setCurrentViseme(0);
        setIsPlaying(false);

        const startTimeout = setTimeout(() => {
          onAudioStart?.();
          setIsPlaying(true);
          const visemes = [3, 5, 8, 10, 0];
          visemeTimelineRef.current = [];
          visemes.forEach((v, i) => {
            const to = setTimeout(() => {
              setCurrentViseme(v);
              visemeTimelineRef.current.push({ visemeId: v, offsetMs: i * 60 });
            }, i * 60);
            timeoutsRef.current.push(to);
          });
          const endTimeout = setTimeout(() => {
            setCurrentViseme(0);
            setIsPlaying(false);
            onAudioEnd?.();
          }, visemes.length * 60 + 120);
          timeoutsRef.current.push(endTimeout);
        }, 10);
        timeoutsRef.current.push(startTimeout);
        return;
      }

      // Real SDK path
      if (isPlaying) {
        await stop();
        return;
      }

      try {
        // Resolve token & region (prefer API; fallback to env)
        let token = tokenData?.token;
        let region: string | undefined =
          (tokenData as unknown as { region?: string })?.region ?? SPEECH_REGION;

        if (!token || !region) {
          const r = await refetch();
          token = r.data?.token ?? token;
          region =
            (r.data as unknown as { region?: string })?.region ?? region ?? SPEECH_REGION;
        }
        if (!token || !region) {
          throw new Error("Speech token/region unavailable.");
        }

        // Always start fresh to avoid MSE 'updating' issues
        closeSynthAndDest();

        const dest = new SpeechSDK.SpeakerAudioDestination();
        dest.onAudioStart = () => {
          onAudioStart?.();
          setIsPlaying(true);
        };
        dest.onAudioEnd = () => {
          setIsPlaying(false);
          setCurrentViseme(0);
          onAudioEnd?.();
          // release media resources after playback
          try {
            dest.close();
          } catch {
            /* ignore */
          }
          if (speakerDestRef.current === dest) speakerDestRef.current = null;
        };
        dest.volume = isMuted ? 0 : clamp01(volume);
        speakerDestRef.current = dest;

        const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(token, region);
        if (voiceName) speechConfig.speechSynthesisVoiceName = voiceName;

        // Enable viseme events
        speechConfig.setProperty(
          "SpeechServiceConnection_SynthVoiceVisemeEvent",
          "true"
        );

        const audioConfig = SpeechSDK.AudioConfig.fromSpeakerOutput(dest);
        const synthesizer = new SpeechSDK.SpeechSynthesizer(speechConfig, audioConfig);
        synthesizerRef.current = synthesizer;

        // JS SDK events are lowerCamelCase
        synthesizer.visemeReceived = (
          _s: SpeechSDK.SpeechSynthesizer,
          e: SpeechSDK.SpeechSynthesisVisemeEventArgs
        ) => {
          const offsetMs = e.audioOffset / 10000; // 100ns -> ms
          setCurrentViseme(e.visemeId);
          visemeTimelineRef.current.push({ visemeId: e.visemeId, offsetMs });
        };

        synthesizer.synthesisStarted = () => {
          onAudioStart?.();
          setIsPlaying(true);
        };

        synthesizer.synthesisCompleted = () => {
          setIsPlaying(false);
          setCurrentViseme(0);
          onAudioEnd?.();
          closeSynthAndDest();
        };

        synthesizer.SynthesisCanceled = async (
          _s: SpeechSDK.SpeechSynthesizer,
          e: SpeechSDK.SpeechSynthesisEventArgs
        ) => {
          setIsPlaying(false);
          setCurrentViseme(0);
          onAudioEnd?.();
          if (e.result.errorDetails && e.result.errorDetails.toLowerCase().includes("token")) {
            await refetch(); // refresh for next call
          }
          closeSynthAndDest();
        };

        await new Promise<void>((resolve, reject) => {
          try {
            synthesizer.speakTextAsync(
              text,
              () => resolve(),
              (err) => reject(err)
            );
          } catch (err) {
            reject(err as unknown);
          }
        });
      } catch (err) {
        console.error("Speech synthesis error:", err);
        setIsPlaying(false);
        setCurrentViseme(0);
        onAudioEnd?.();
        closeSynthAndDest();
      }
    },
    [
      clearTimeouts,
      closeSynthAndDest,
      isPlaying,
      onAudioEnd,
      onAudioStart,
      refetch,
      tokenData, // Option B: depend on the whole object
      stop,
      volume,
      isMuted,
    ]
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
