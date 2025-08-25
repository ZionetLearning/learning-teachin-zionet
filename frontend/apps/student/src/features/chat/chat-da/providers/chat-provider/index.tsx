import { useReducer, type ReactNode } from "react";

import type { State } from "../../../../../types";
import { ChatDaContext, ChatReducer } from "../../context/chat-context";

export const ChatProvider = ({ children }: { children: ReactNode }) => {
  const [state, dispatch] = useReducer(ChatReducer, { messages: [] } as State);

  return (
    <ChatDaContext.Provider value={{ state, dispatch }}>
      {children}
    </ChatDaContext.Provider>
  );
};
