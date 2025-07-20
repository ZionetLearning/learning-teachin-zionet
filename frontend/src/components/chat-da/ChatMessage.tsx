import useChatDaStyles from '../../features/chat/chat-da/style';
import type { Message } from '../../types/chat-da';

export const ChatMessage = ({ message }: { message: Message }) => {
	const classes = useChatDaStyles();

	return (
		<div
			key={message.id}
			className={`${classes.message} ${
				message.sender === 'user' ? classes.userMessage : classes.botMessage
			}`}
		>
			{message.text}
		</div>
	);
};
