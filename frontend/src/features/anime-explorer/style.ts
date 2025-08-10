import { createUseStyles } from 'react-jss';

type StyleProps = {
	isFetchingNextPage: boolean;
};

export const useStyles = createUseStyles<string, StyleProps>({
	root: {
		height: '100%',
		display: 'flex',
		flexDirection: 'column',
		boxSizing: 'border-box',
		backgroundColor: '#f0f0f0',
	},
	searchBar: {
		padding: '12px 20px',
		backgroundColor: '#fff',
		borderBottom: '1px solid #e5e5e5',
		display: 'flex',
		gap: '10px',
	},
	searchInput: {
		flex: 1,
		padding: '10px 12px',
		borderRadius: 8,
		border: '1px solid #ccc',
		fontSize: '16px',
		backgroundColor: '#fafafa',
		color: '#333',
		outline: 'none',
		'&:focus': {
			borderColor: '#999',
			backgroundColor: '#fff',
		},
	},
	clearButton: {
		padding: '10px 12px',
		borderRadius: 8,
		border: '1px solid #ccc',
		backgroundColor: '#fafafa',
		cursor: 'pointer',
		color: '#333',
		transition: 'background-color 0.15s ease',
		'&:hover': {
			backgroundColor: '#e0e0e0',
		},
	},
	list: {
		flex: 1,
		overflowY: 'auto',
		padding: '20px',
		boxSizing: 'border-box',
		display: 'flex',
		flexWrap: 'wrap',
		gap: '20px',
		justifyContent: 'center',
	},
	sentinel: {
		height: 1,
		width: '100%',
	},
	loadingMore: {
		color: '#000',
		width: '100%',
		textAlign: 'center',
		visibility: (props: { isFetchingNextPage: boolean }) =>
			props.isFetchingNextPage ? 'visible' : 'hidden',
	},
});
