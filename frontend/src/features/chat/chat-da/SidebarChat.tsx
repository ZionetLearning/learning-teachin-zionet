import { useEffect, useRef, useState } from 'react';

import { useChatContext } from '../../../context/chat-da/chatContext';
import { useChat } from '../../../hooks/useChat';
import { ChatHeader } from '../../../components/chat-da/ChatHeader';
import { ChatInput } from '../../../components/chat-da/ChatInput';
import { ChatMessage } from '../../../components/chat-da/ChatMessage';

import useChatDaStyles from './style';

export const SidebarChat = () => {
	const { sendMessage } = useChat();
	const { state } = useChatContext();
	const classes = useChatDaStyles();

	const bottomRef = useRef<HTMLDivElement>(null);

	const [input, setInput] = useState('');

	useEffect(
		function scrollDownOnNewMessage() {
			if (state.messages.length > 0) {
				bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
			}
		},
		[state.messages]
	);

	const botTyping = state.messages.some(
		(m) => m.sender === 'bot' && !m.isComplete
	);

	return (
		<aside className={classes.sidebar}>
			<ChatHeader />
			<main className={classes.messagesContainer}>
				{state.messages.map((message) => (
					<ChatMessage key={message.id} message={message} />
				))}
				<div ref={bottomRef} />
			</main>
			<ChatInput
				input={input}
				setInput={setInput}
				sendMessage={sendMessage}
				disabled={botTyping}
			/>
		</aside>
	);
};
