import { createUseStyles } from 'react-jss';

type PageStyleProps = {
	isFetchingNextPage: boolean;
	showBackToTop: boolean;
};

export const useStyles = createUseStyles<string, PageStyleProps>({
	root: {
		height: '100%',
		display: 'flex',
		flexDirection: 'column',
		background: 'linear-gradient(180deg, #f8fafc 0%, #eef2f7 100%)',
	},
	listWrap: {
		flex: 1,
		overflow: 'auto',
	},
	listInner: {
		maxWidth: 1200,
		margin: '0 auto',
		padding: 20,
	},
	grid: {
		display: 'grid',
		gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
		gap: '20px',
		alignContent: 'start',
		justifyItems: 'stretch',
	},
	sentinel: { height: 1, width: '100%' },
	loading: {
		color: '#111827',
		width: '100%',
		textAlign: 'center',
		marginTop: 12,
	},
	loadingMore: (p) => ({
		extend: 'loading',
		visibility: p.isFetchingNextPage ? 'visible' : 'hidden',
	}),
	backToTop: (p) => ({
		position: 'fixed',
		right: 20,
		bottom: 20,
		opacity: p.showBackToTop ? 1 : 0,
		pointerEvents: p.showBackToTop ? 'auto' : 'none',
		transition: 'opacity 200ms ease',
		background: '#0ea5e9',
		color: '#fff',
		border: 'none',
		borderRadius: 999,
		padding: '10px 14px',
		boxShadow: '0 8px 24px rgba(2,132,199,0.35)',
		cursor: 'pointer',
	}),
	srOnly: {
		position: 'absolute',
		width: 1,
		height: 1,
		padding: 0,
		margin: -1,
		overflow: 'hidden',
		clip: 'rect(0,0,0,0)',
		whiteSpace: 'nowrap',
		border: 0,
	},
});
