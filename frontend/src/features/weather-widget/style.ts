import { createUseStyles } from 'react-jss';

export const useStyles = createUseStyles({
	container: {
		display: 'flex',
		flexDirection: 'column',
		padding: 16,
		background: '#fff',
		color: '#000',
		width: '100%',
		height: '100%',
		gap: 16,
	},
	select: {
		padding: 8,
		border: '1px solid #ccc',
		borderRadius: 4,
		fontSize: '1rem',
		background: '#fff',
		color: '#000',
		maxWidth: 300,
		alignSelf: 'center',
	},
	heading: {
		fontSize: '1.25rem',
		fontWeight: 600,
		display: 'flex',
		alignItems: 'center',
		justifyContent: 'center',
	},
	emoji: {
		fontSize: '1.5rem',
	},
	weatherContainer: {
		display: 'flex',
		alignItems: 'center',
		justifyContent: 'center',
	},
	icon: {
		width: 50,
		height: 50,
	},
	temp: {
		fontSize: '1.5rem',
		fontWeight: 'bold',
		color: '#000',
	},
	description: {
		textTransform: 'capitalize',
		color: '#000',
	},
	stats: {
		fontSize: '0.875rem',
		color: '#4a5568',
		lineHeight: 1.4,
	},
	loading: {
		fontStyle: 'italic',
		color: '#000',
	},
	error: {
		color: 'red',
	},
});
