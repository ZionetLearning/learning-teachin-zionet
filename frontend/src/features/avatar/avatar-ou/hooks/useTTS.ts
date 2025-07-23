import { useState, useRef, useEffect } from "react";

interface TTSOptions {
  lang?: string;
  rate?: number;
  pitch?: number;
  volume?: number;
}

export const useTTS = (options: TTSOptions = {}) => {
  const [isPlaying, setIsPlaying] = useState(false);
  const [isMuted, setIsMuted] = useState(false);
  const synthRef = useRef<SpeechSynthesis | null>(null);

  const defaultOptions = {
    lang: "he-IL",
    rate: 0.9,
    pitch: 1.1,
    volume: 1,
    ...options,
  };

  useEffect(() => {
    // Check if speech synthesis is supported
    if ("speechSynthesis" in window) {
      synthRef.current = window.speechSynthesis;
    }

    return () => {
      if (synthRef.current) {
        synthRef.current.cancel();
      }
    };
  }, []);

  const speak = (text: string) => {
    if (!synthRef.current || !text.trim()) return;

    if (isPlaying) {
      synthRef.current.cancel();
      setIsPlaying(false);
      return;
    }

    const utterance = new SpeechSynthesisUtterance(text);

    // Apply options
    utterance.lang = defaultOptions.lang;
    utterance.rate = defaultOptions.rate;
    utterance.pitch = defaultOptions.pitch;
    utterance.volume = isMuted ? 0 : defaultOptions.volume;

    utterance.onstart = () => {
      setIsPlaying(true);
    };

    utterance.onend = () => {
      setIsPlaying(false);
    };

    utterance.onerror = () => {
      setIsPlaying(false);
    };

    synthRef.current.speak(utterance);
  };

  const stop = () => {
    if (synthRef.current) {
      synthRef.current.cancel();
      setIsPlaying(false);
    }
  };

  const toggleMute = () => {
    setIsMuted(!isMuted);
  };

  return {
    speak,
    stop,
    toggleMute,
    isPlaying,
    isMuted,
    isSupported: !!synthRef.current,
  };
};
