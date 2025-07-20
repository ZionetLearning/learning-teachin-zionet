import useChatDaStyles from '../../features/chat/chat-da/style';
import { ChatIcon } from '../icons';

export const ChatHeader = () => {
	const classes = useChatDaStyles();

	return (
		<header className={classes.header}>
			Learning-teachin-chat
			<ChatIcon className={classes.chatIcon} />
		</header>
	);
};
