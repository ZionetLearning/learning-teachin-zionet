import { useState, useCallback } from "react";
import { askAzureOpenAI } from "../../chat/chat-yo/services";

export const useHebrewSentence = async () => {
    const [sentence, setSentence] = useState<string>("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchSentence = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await askAzureOpenAI("צור משפטים קצרים וברורים בעברית ללומדי מתחילים.", "צור משפט אחד קצר בעברית, ללא ניקוד.");
            setSentence(response);
            return response;
        }
        catch (e: any) {
            setError(e?.message ?? "failed");
            return "";
        }
        finally {
            setLoading(false);
        }
    }, []);

    return { loading, error, fetchSentence, setSentence };
}