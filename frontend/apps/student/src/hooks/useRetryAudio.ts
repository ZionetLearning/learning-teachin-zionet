import { useCallback } from "react";
import { useAvatarSpeech } from "./useAvatarSpeech";

interface UseRetryAudioOptions {
  sentence: string;
  onAudioStart?: () => void;
  onAudioEnd?: () => void;
  volume?: number;
}

/**
 * Hook for managing audio playback in retry mode
 * Provides play/replay functionality with consistent behavior
 */
export const useRetryAudio = ({
  sentence,
  onAudioStart,
  onAudioEnd,
  volume = 1,
}: UseRetryAudioOptions) => {
  const { speak, stop, isPlaying, error } = useAvatarSpeech({
    volume,
    onAudioStart,
    onAudioEnd,
  });

  const handlePlayAudio = useCallback(() => {
    if (isPlaying) {
      stop();
      return;
    }
    speak(sentence);
  }, [isPlaying, stop, speak, sentence]);

  const handleReplayAudio = handlePlayAudio;

  return {
    handlePlayAudio,
    handleReplayAudio,
    isPlaying,
    audioError: error,
    stopAudio: stop,
  };
};
