import { createUseStyles } from 'react-jss';

export const useStyles = createUseStyles({
	container: {
		display: 'flex',
		flexDirection: 'column',
		padding: 16,
		background: '#fff',
		color: '#000',
		height: '100%',
		gap: 16,
	},
	inputGroup: {
		display: 'flex',
		gap: 8,
	},
	select: {
		flex: 1,
		padding: 8,
		border: '1px solid #ccc',
		borderRadius: 4,
		fontSize: '1rem',
		background: '#fff',
		color: '#000',
	},
	searchGroup: {
		display: 'flex',
		gap: 8,
		flex: '0 0 calc(50% - 4px)',
		minWidth: 0,
	},
	input: {
		flex: 1,
		padding: 8,
		border: '1px solid #ccc',
		borderRadius: 4,
		fontSize: '1rem',
		background: '#fff',
		color: '#000',
	},
	button: {
		flex: '0 0 auto',
		padding: '8px 12px',
		border: 'none',
		borderRadius: 4,
		background: '#007aff',
		color: '#fff',
		cursor: 'pointer',
	},
	heading: {
		fontSize: '1.25rem',
		fontWeight: 600,
		alignItems: 'center',
		justifyContent: 'center',
	},
	emoji: {
		fontSize: '2rem',
	},
	weatherContainer: {
		display: 'flex',
		flexDirection: 'column',
		alignItems: 'center',
		justifyContent: 'center',
	},
	iconContainer: {
		display: 'flex',
		alignItems: 'center',
		gap: 8,
	},
	temp: {
		fontSize: '1.5rem',
		fontWeight: 'bold',
		color: '#000',
	},
	description: {
		fontSize: '1.25rem',
		textTransform: 'capitalize',
		color: '#000',
	},
	icon: {
		width: 75,
		height: 75,
	},
	stats: {
		fontSize: '1rem',
		color: '#4a5568',
		lineHeight: 1.4,
	},
	loading: {
		fontStyle: 'italic',
		color: '#000',
		fontSize: '1.125rem',
	},
	error: {
		color: 'red',
	},
});
