/// <reference types="vite/client" />
import { apiClient as axios } from "@app-providers";
import { EventType } from "@app-providers/types";
import { useSignalR } from "@student/hooks";
import { AIChatStreamResponse } from "@student/types";

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
  const { waitForStream, status } = useSignalR();

  const startMistakeExplanationStream = async (
    request: ExplainMistakeRequest,
    onDelta: (delta: string) => void,
    onCompleted: () => void
  ) => {
    if (status !== "connected") {
      throw new Error(`SignalR not connected. Status: ${status}`);
    }

    try {
      const { data } = await axios.post<ExplainMistakeResponse>(
        `${AI_BASE_URL}/chat/mistake-explanation`,
        request
      );

      const requestId = data.requestId;

      const streamMessages = waitForStream<AIChatStreamResponse>(
        EventType.ChatAiAnswer,
        requestId,
        (msg) => {
          const payload = msg?.payload ?? {};
          const isFinal = !!payload?.isFinal;
          const delta = payload?.delta;

          if (isFinal) {
            onCompleted();
            return;
          }

          if (typeof delta === "string" && delta.length > 0) {
            onDelta(delta);
          }
        }
      );

      await streamMessages;
    } catch (error) {
      console.error("Error sending mistake explanation request:", error);
      throw error;
    }
  };

  return { startMistakeExplanationStream };
};