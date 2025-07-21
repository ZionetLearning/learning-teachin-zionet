import { useEffect, useRef, useState } from 'react';

import {
	useEventSource,
	useEventSourceListener,
} from '@react-nano/use-event-source';

import { ChatAction } from '../../../../types/chat-da';
import { useChatContext } from '../context/chat-context';
import { MockEventSource } from '../mock-eventsource';

export const useChat = () => {
	const { dispatch } = useChatContext();
	const [url, setUrl] = useState<string | null>(null); // URL for the EventSource
	const [eventSource] = useEventSource(url ?? '', false, MockEventSource);

	const botIdRef = useRef<string | null>(null); // to store the current bot message ID
	const messageBufferRef = useRef(''); // to buffer the incoming tokens

	useEventSourceListener(
		eventSource,
		['message'],
		(event) => {
			const { text: token, done } = JSON.parse(event.data) as {
				text: string;
				done?: boolean;
			};
			if (done) {
				dispatch({
					type: ChatAction.COMPLETE_MESSAGE,
					payload: { id: botIdRef.current! },
				});
			} else {
				messageBufferRef.current += token;
				dispatch({
					type: ChatAction.UPDATE_MESSAGE,
					payload: { id: botIdRef.current!, text: messageBufferRef.current },
				});
			}
		},
		[eventSource, dispatch]
	);

	useEffect(
		function manageEventSourceClose() {
			if (!eventSource) return;
			const eventSourceClose = eventSource.close.bind(eventSource); // pull the close method from the EventSource, bind it to the current context
			eventSource.close = () => {
				eventSourceClose();
				setUrl(null);
			};
		},
		[eventSource]
	);

	const sendMessage = (userText: string) => {
		dispatch({
			type: ChatAction.ADD_MESSAGE,
			payload: { id: crypto.randomUUID(), text: userText, sender: 'user' },
		});

		const botId = crypto.randomUUID();
		botIdRef.current = botId;
		messageBufferRef.current = '';
		dispatch({
			type: ChatAction.ADD_MESSAGE,
			payload: { id: botId, text: '', sender: 'bot', isComplete: false },
		});

		setUrl(`/chat?prompt=${encodeURIComponent(userText)}`);
	};

	return { sendMessage };
};
