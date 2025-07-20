import { useState } from 'react';

import { ChatDaProvider } from '../../../context/chat-da/chatProvider';
import { FullScreenChat } from './FullScreenChat';
import { SidebarChat } from './SidebarChat';

import useChatDaStyles from './style';

export const ChatDa = () => {
	const classes = useChatDaStyles();
	const [view, setView] = useState<'sidebar' | 'full'>('sidebar');
	return (
		<ChatDaProvider>
			<button
				className={classes.toggleButton}
				onClick={() =>
					setView((view) => (view === 'sidebar' ? 'full' : 'sidebar'))
				}
			>
				Toggle View
			</button>
			{view === 'sidebar' ? <SidebarChat /> : <FullScreenChat />}
		</ChatDaProvider>
	);
};
