import { useState, useCallback, useRef } from "react";
import { askAzureOpenAI } from "../../../chat/chat-yo/services";

export const useHebrewSentence = () => {
  const [sentence, setSentence] = useState<string>("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const pendingRef = useRef(false);
  const didInitRef = useRef(false);

  const fetchSentence = useCallback(async () => {
    if (pendingRef.current) return sentence;
    pendingRef.current = true;
    setLoading(true);
    setError(null);
    try {
      const response = await askAzureOpenAI(
        "צור משפט אחד קצר בעברית, ללא ניקוד.",
        "צור משפטים קצרים וברורים בעברית ללומדי מתחילים.",
      );
      setSentence(response);
      return response;
    } catch (e: unknown) {
      if (e instanceof Error) {
        setError(e.message);
      } else {
        setError("An unknown error occurred");
      }
    } finally {
      setLoading(false);
      pendingRef.current = false;
    }
  }, []);

  const initOnce = useCallback(async () => {
    if (didInitRef.current) return;
    didInitRef.current = true;
    await fetchSentence();
  }, [fetchSentence]);

  return { sentence, loading, error, fetchSentence, initOnce };
};
