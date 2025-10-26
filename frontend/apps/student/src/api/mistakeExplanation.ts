/// <reference types="vite/client" />
import { apiClient as axios } from "@app-providers";

export const ChatType = {
  Default: "Default",
  ExplainMistake: "ExplainMistake",
} as const;

export type ChatType = typeof ChatType[keyof typeof ChatType];

export interface ExplainMistakeRequest {
  attemptId: string;
  threadId: string;
  gameType: string;
  chatType: typeof ChatType.ExplainMistake;
}

export interface ExplainMistakeResponse {
  requestId: string;
}

export const useSendMistakeExplanation = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;

  const sendMistakeExplanation = async (
    request: ExplainMistakeRequest
  ): Promise<ExplainMistakeResponse> => {
    try {
      const { data } = await axios.post<ExplainMistakeResponse>(
        `${AI_BASE_URL}/chat/mistake-explanation`,
        request
      );
      return data;
    } catch (error) {
      console.error("Error sending mistake explanation request:", error);
      throw error;
    }
  };

  return { sendMistakeExplanation };
};