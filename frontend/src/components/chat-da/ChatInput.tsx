import useChatDaStyles from '../../features/chat/chat-da/style';
import { SendIcon } from '../icons';

interface InputProps {
	input: string;
	setInput: (value: string) => void;
	sendMessage: (message: string) => void;
	disabled: boolean;
}

export const ChatInput = ({
	input,
	setInput,
	sendMessage,
	disabled,
}: InputProps) => {
	const classes = useChatDaStyles();

	return (
		<footer className={classes.inputWrapper}>
			<input
				id="chat-input"
				className={classes.input}
				value={input}
				onChange={(e) => setInput(e.target.value)}
				onKeyDown={(e) => {
					if (e.key === 'Enter' && !disabled && input.trim()) {
						sendMessage(input.trim());
						setInput('');
					}
				}}
				placeholder="Type a message..."
				disabled={disabled}
			/>
			<SendIcon
				className={classes.sendButton}
				fill={disabled ? '#ccc' : 'currentColor'}
				stroke={disabled ? '#ccc' : 'currentColor'}
				onClick={() => {
					if (!disabled) sendMessage(input.trim());
					setInput('');
				}}
			/>
		</footer>
	);
};
