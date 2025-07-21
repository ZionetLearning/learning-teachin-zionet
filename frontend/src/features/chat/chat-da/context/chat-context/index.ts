import { createContext, useContext, type Dispatch } from 'react';

import {
	ChatAction,
	type Action,
	type Message,
	type State,
} from '../../../../../types';

export const ChatDaContext = createContext<{
	state: State;
	dispatch: Dispatch<Action>;
}>({ state: { messages: [] }, dispatch: () => {} });

export const ChatReducer = (state: State, action: Action): State => {
	switch (action.type) {
		case ChatAction.ADD_MESSAGE:
			return {
				...state,
				messages: [...(state.messages as Message[]), action.payload as Message],
			};
		case ChatAction.UPDATE_MESSAGE:
			return {
				...state,
				messages: state.messages.map((message) =>
					message.id === action.payload.id && 'text' in action.payload
						? { ...message, text: action.payload.text }
						: message
				),
			};
		case ChatAction.COMPLETE_MESSAGE:
			return {
				...state,
				messages: state.messages.map((message) =>
					message.id === action.payload.id
						? { ...message, isComplete: true }
						: message
				),
			};
		default:
			return state;
	}
};

export const useChatContext = () => {
	const context = useContext(ChatDaContext);
	if (!context) {
		throw new Error('useChatContext must be used within a ChatProvider');
	}
	return context;
};
