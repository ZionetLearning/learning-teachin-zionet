import { ChatDa } from '../features';
import { useStyles } from './style';

export const ChatDaPage = () => {
	const classes = useStyles();
	return (
		<div className={classes.chatDaPageWrapper}>
			<div className={classes.chatDaChatWrapper}>
				<ChatDa />
			</div>
		</div>
	);
};
