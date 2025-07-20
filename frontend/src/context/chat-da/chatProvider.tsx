import { useReducer, type ReactNode } from 'react';

import type { State } from '../../types/chat-da';
import { ChatDaContext, ChatReducer } from './chatContext';

export const ChatDaProvider = ({ children }: { children: ReactNode }) => {
	const [state, dispatch] = useReducer(ChatReducer, { messages: [] } as State);

	return (
		<ChatDaContext.Provider value={{ state, dispatch }}>
			{children}
		</ChatDaContext.Provider>
	);
};
