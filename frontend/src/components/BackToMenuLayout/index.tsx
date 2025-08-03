import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { useStyles } from './style';
import { useAuth } from '@/providers/auth';

export const BackToMenuLayout = () => {
	const navigate = useNavigate();
	const location = useLocation();
	const classes = useStyles();
	const { logout } = useAuth();

	const showBackButton = location.pathname !== '/';

	return (
		<div>
			<header className={classes.header}>
				{showBackButton && (
					<button className={classes.button} onClick={() => navigate('/')}>
						Go back to menu
					</button>
				)}
				<button
					className={classes.logoutButton}
					onClick={() => {
						logout();
						navigate('/signin', { replace: true });
					}}
				>
					Logout
				</button>
			</header>

			<main className={classes.main}>
				<Outlet />
			</main>
		</div>
	);
};
