import { useState, useCallback, useRef } from "react";
import { useChat } from "@student/hooks";

export const useHebrewSentence = () => {
  const [sentence, setSentence] = useState<string>("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const pendingRef = useRef(false);
  const didInitRef = useRef(false);

  const { sendMessageAsync } = useChat();

  const PROMPT =
    "צור משפט אחד קצר בעברית, ללא ניקוד. השב רק את המשפט עצמו ללא הסברים נוספים.";

  const fetchSentence = useCallback(async () => {
    if (pendingRef.current) return sentence;
    pendingRef.current = true;
    setLoading(true);
    setError(null);
    try {
      const response = await sendMessageAsync(PROMPT);
      setSentence(response);
      return response;
    } catch (e: unknown) {
      if (e instanceof Error) setError(e.message);
      else setError("An unknown error occurred");
    } finally {
      setLoading(false);
      pendingRef.current = false;
    }
  }, [sendMessageAsync, sentence]);

  const initOnce = useCallback(async () => {
    if (didInitRef.current) return;
    didInitRef.current = true;
    await fetchSentence();
  }, [fetchSentence]);

  return { sentence, loading, error, fetchSentence, initOnce };
};
