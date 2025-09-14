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

  const visemeMap = useMemo(() => {
    return lipsArray.reduce((acc, path, index) => {
      acc[index] = path;
      return acc;
    }, {} as Record<number, string>);
  }, [lipsArray]);

  const clearTimeouts = useCallback(() => {
    timeoutsRef.current.forEach(clearTimeout);
    timeoutsRef.current = [];
  }, []);

  useEffect(() => {
    return () => {
      try {
        synthesizerRef.current?.close();
      } catch {
        /* ignore */
      }
      synthesizerRef.current = null;
      clearTimeouts();
      speakerDestRef.current = null;
    };
  }, [clearTimeouts]);

  const stop = useCallback(async () => {
    try {
      synthesizerRef.current?.close(); // JS SDK: close() cancels/stops
    } catch {
      /* ignore */
    } finally {
      setIsPlaying(false);
      setCurrentViseme(0);
      onAudioEnd?.();
    }
  }, [onAudioEnd]);

  const safeVolume = (v: number) => Math.min(Math.max(v, 0), 1);

  const toggleMute = useCallback(() => {
    setIsMuted((prev) => {
      const next = !prev;
      if (speakerDestRef.current) {
        speakerDestRef.current.volume = next ? 0 : safeVolume(volume);
      }
      return next;
    });
  }, [volume]);

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

      // Real SDK
      if (isPlaying) {
        await stop();
        return;
      }

      try {
        // Ensure token & region (prefer API’s region; fallback to env)
        let token = tokenData?.token;
        let region: string | undefined = (tokenData as unknown as { region?: string })?.region ?? SPEECH_REGION;

        if (!token || !region) {
          const r = await refetch();
          token = r.data?.token;
          region = (r.data as unknown as { region?: string })?.region ?? SPEECH_REGION;
        }
        if (!token || !region) throw new Error("Speech token/region unavailable.");

        // Create or reuse a speaker destination for volume/mute
        if (!speakerDestRef.current) {
          const dest = new SpeechSDK.SpeakerAudioDestination();
          dest.onAudioStart = () => {
            onAudioStart?.();
            setIsPlaying(true);
          };
          dest.onAudioEnd = () => {
            setIsPlaying(false);
            setCurrentViseme(0);
            onAudioEnd?.();
          };
          dest.volume = isMuted ? 0 : safeVolume(volume);
          speakerDestRef.current = dest;
        } else {
          speakerDestRef.current.volume = isMuted ? 0 : safeVolume(volume);
        }

        // Replace previous synthesizer
        try {
          synthesizerRef.current?.close();
        } catch {
          /* ignore */
        }
        synthesizerRef.current = null;

        const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(token, region);
        if (voiceName) speechConfig.speechSynthesisVoiceName = voiceName;

        // Enable visemes
        speechConfig.setProperty("SpeechServiceConnection_SynthVoiceVisemeEvent", "true");

        const audioConfig = SpeechSDK.AudioConfig.fromSpeakerOutput(speakerDestRef.current);
        const synthesizer = new SpeechSDK.SpeechSynthesizer(speechConfig, audioConfig);
        synthesizerRef.current = synthesizer;

        // Events (lowerCamelCase in JS SDK)
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
      }
    },
    [clearTimeouts, isPlaying, onAudioEnd, onAudioStart, refetch, tokenData, stop, volume, isMuted]
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
