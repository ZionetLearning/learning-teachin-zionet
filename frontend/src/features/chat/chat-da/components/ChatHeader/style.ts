import { createUseStyles } from 'react-jss';

const useStyles = createUseStyles({
	header: {
		backgroundColor: '#007bff',
		padding: '10px 20px',
		fontSize: '1.5rem',
		boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
		gap: 8,
		display: 'flex',
		alignItems: 'center',
		justifyContent: 'center',
	},
	title: {
		color: '#fff',
		fontWeight: 'bold',
	},
});

export default useStyles;
