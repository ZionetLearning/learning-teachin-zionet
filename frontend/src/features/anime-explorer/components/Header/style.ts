import { createUseStyles } from 'react-jss';

export const useStyles = createUseStyles({
	header: {
		position: 'sticky',
		top: 0,
		zIndex: 10,
		background: '#ffffffcc',
		borderBottom: '1px solid #e5e7eb',
	},
	headerInner: {
		padding: '14px 20px',
		display: 'grid',
		gridTemplateColumns: '1fr auto 1fr',
		gap: 12,
		alignItems: 'center',
	},
	title: {
		fontSize: 18,
		fontWeight: 600,
		color: '#0f172a',
		gridColumn: 2,
		justifySelf: 'center',
	},
	controls: {
		display: 'flex',
		gap: 10,
		alignItems: 'center',
		justifySelf: 'end',
	},
	searchInput: {
		flex: 1,
		minWidth: 260,
		padding: '10px 12px',
		borderRadius: 10,
		border: '1px solid #cbd5e1',
		background: '#f8fafc',
		color: '#0f172a',
		outline: 'none',
		'&::placeholder': { color: '#94a3b8' },
		'&:focus': {
			borderColor: '#94a3b8',
			background: '#fff',
		},
		'&:-webkit-autofill, &:-webkit-autofill:hover, &:-webkit-autofill:focus': {
			WebkitTextFillColor: '#000 !important',
			WebkitBoxShadow: '0 0 0px 1000px #f7f9fa inset !important',
			boxShadow: '0 0 0px 1000px #f7f9fa inset !important',
		},
	},
	clearButton: {
		padding: '10px 12px',
		borderRadius: 10,
		border: '1px solid #cbd5e1',
		background: '#fff',
		cursor: 'pointer',
		color: '#0f172a',
		'&:hover': { background: '#f1f5f9' },
	},
});
