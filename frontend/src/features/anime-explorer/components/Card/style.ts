import { createUseStyles } from 'react-jss';

type StyleProps = {
	hovered: boolean;
};

export const useStyles = createUseStyles<string, StyleProps>({
	card: {
		width: 220,
		display: 'flex',
		flexDirection: 'column',
		alignItems: 'center',
		gap: 10,
		border: '1px solid #ccc',
		padding: 10,
		borderRadius: 8,
		position: 'relative',
		cursor: 'pointer',
		background: '#fff',
	},
	media: {
		width: 150,
		height: 'auto',
		borderRadius: 8,
	},
	title: {
		textAlign: 'center',
	},
	rating: {
		display: 'flex',
		alignItems: 'center',
		gap: 4,
	},
	starIcon: {
		width: 20,
		height: 20,
	},
	subtitle: {
		textAlign: 'center',
	},
	meta: {
		fontSize: 14,
	},
	overlay: (props) => ({
		position: 'absolute',
		top: 0,
		right: 0,
		bottom: 0,
		left: 0,
		backgroundColor: 'rgba(0, 0, 0, 0.5)',
		color: '#fff',
		display: 'flex',
		alignItems: 'center',
		justifyContent: 'center',
		textAlign: 'center',
		padding: 10,
		fontSize: '0.85rem',
		wordBreak: 'break-word',
		opacity: props.hovered ? 1 : 0,
		transition: 'opacity 0.3s ease',
	}),
});
