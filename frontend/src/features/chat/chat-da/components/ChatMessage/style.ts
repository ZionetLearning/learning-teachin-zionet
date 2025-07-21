import { createUseStyles } from 'react-jss';

const useStyles = createUseStyles({
	message: {
		maxWidth: '80%',
		padding: '8px 12px',
		borderRadius: 12,
		fontSize: 14,
		overflowWrap: 'break-word',
		wordBreak: 'break-word',
	},
	userMessage: {
		alignSelf: 'flex-end',
		background: '#007aff',
		color: '#fff',
		textAlign: 'left',
	},
	botMessage: {
		alignSelf: 'flex-start',
		background: '#e5e5ea',
		color: '#000',
		textAlign: 'left',
	},
});

export default useStyles;
