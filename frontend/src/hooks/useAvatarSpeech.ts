import { useState, useMemo, useRef, useCallback } from "react";

import { useSynthesizeSpeech } from "../api/speech";
import { base64ToBlob } from "@/utils";

interface useAvatarSpeechOptions {
  lipsArray?: string[];
  volume?: number;
  onAudioStart?: () => void;
  onAudioEnd?: () => void;
}

export const useAvatarSpeech = ({
  lipsArray = [],
  volume = 1,
  onAudioStart,
  onAudioEnd,
}: useAvatarSpeechOptions) => {
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const timeoutsRef = useRef<NodeJS.Timeout[]>([]);
  const [currentViseme, setCurrentViseme] = useState<number>(0);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [isMuted, setIsMuted] = useState<boolean>(false);

  const {
    mutateAsync: synthesizeSpeech,
    isPending,
    error,
  } = useSynthesizeSpeech();

  const visemeMap = useMemo(() => {
    return lipsArray.reduce(
      (acc, path, index) => {
        acc[index] = path;
        return acc;
      },
      {} as Record<number, string>,
    );
  }, [lipsArray]);

  const clearTimeouts = useCallback(() => {
    timeoutsRef.current.forEach(clearTimeout);
    timeoutsRef.current = [];
  }, []);

  const stop = useCallback(() => {
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current = null;
    }
    clearTimeouts();
    setCurrentViseme(0);
    setIsPlaying(false);
    onAudioEnd?.();
  }, [clearTimeouts, onAudioEnd]);

  const toggleMute = useCallback(() => {
    setIsMuted((prev) => {
      const newMuted = !prev;
      if (audioRef.current) {
        audioRef.current.volume = newMuted ? 0 : volume;
      }
      return newMuted;
    });
  }, [volume]);

  const speak = useCallback(
    async (text: string) => {
      if (!text.trim()) return;

      if (isPlaying) {
        stop();
        return;
      }

      try {
        clearTimeouts();
        setCurrentViseme(0);
        setIsPlaying(false);

        if (audioRef.current) {
          audioRef.current.pause();
          audioRef.current = null;
        }
        const response = await synthesizeSpeech({ text });

        const audioBlob = base64ToBlob(response.audioData, "audio/wav");
        const audioUrl = URL.createObjectURL(audioBlob);
        const audio = new Audio(audioUrl);
        audio.volume = isMuted ? 0 : volume;
        audioRef.current = audio;

        audio.oncanplaythrough = () => {
          onAudioStart?.();
          setIsPlaying(true);
        };

        if (response.visemes.length > 0) {
          response.visemes.forEach(({ visemeId, offsetMs }) => {
            const timeout = setTimeout(() => {
              setCurrentViseme(visemeId);
            }, offsetMs);
            timeoutsRef.current.push(timeout);
          });

          const totalDuration = Math.max(
            ...response.visemes.map((v) => v.offsetMs),
          );
          const resetTimeout = setTimeout(() => {
            setCurrentViseme(0);
          }, totalDuration + 500);
          timeoutsRef.current.push(resetTimeout);
        }

        audio.onended = () => {
          URL.revokeObjectURL(audioUrl);
          setCurrentViseme(0);
          setIsPlaying(false);
          onAudioEnd?.();
        };

        audio.onerror = (err) => {
          console.error("Audio playback error:", err);
          URL.revokeObjectURL(audioUrl);
          setCurrentViseme(0);
          setIsPlaying(false);
          onAudioEnd?.();
        };

        await audio.play();
      } catch (error) {
        console.error("Speech synthesis error:", error);
        setCurrentViseme(0);
        setIsPlaying(false);
        onAudioEnd?.();
      }
    },
    [
      synthesizeSpeech,
      clearTimeouts,
      isPlaying,
      stop,
      volume,
      isMuted,
      onAudioStart,
      onAudioEnd,
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
    isLoading: isPending,
    error,
  };
};
