export type Message = {
  id: string;
  text: string;
  sender: "bot" | "user";
  isComplete?: boolean;
};

export type State = {
  messages: Message[];
};

export const ChatAction = {
  ADD_MESSAGE: "ADD_MESSAGE",
  UPDATE_MESSAGE: "UPDATE_MESSAGE",
  COMPLETE_MESSAGE: "COMPLETE_MESSAGE",
} as const;

export type ChatActionType = (typeof ChatAction)[keyof typeof ChatAction];

export type Action = {
  type: ChatActionType;
  payload: Message | { id: string; text: string } | { id: string };
};
