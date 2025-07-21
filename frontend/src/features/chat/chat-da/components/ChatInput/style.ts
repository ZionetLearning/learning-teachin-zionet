import { createUseStyles } from 'react-jss';

const useStyles = createUseStyles({
	input: {
		boxSizing: 'border-box',
		width: '100%',
		padding: 8,
		borderRadius: 4,
		border: '1px solid #ccc',
		fontSize: 14,
		backgroundColor: '#fff',
		color: '#333',
		outline: 'none',
	},
	inputWrapper: {
		display: 'flex',
		alignItems: 'center',
		gap: 8,
		borderTop: '1px solid #ccc',
		padding: 8,
		background: '#f5f5f5',
	},
	sendButton: {
		backgroundColor: 'transparent',
		border: 'none',
		cursor: 'pointer',
		color: '#007bff',
		'&:hover': {
			color: '#0056b3',
		},
	},
});

export default useStyles;
