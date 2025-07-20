import { ChatDa } from '../features';
import useChatDaStyles from '../features/chat/chat-da/style';

export const ChatDaPage = () => {
	const classes = useChatDaStyles();
	return (
		<div className={classes.pageWrapper}>
			<div className={classes.chatWrapper}>
				<ChatDa />
			</div>
		</div>
	);
};
