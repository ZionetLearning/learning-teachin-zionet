import { Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '@/providers/auth';
import { useEffect } from 'react';

export const RequireAuth = () => {
	const { isAuthorized } = useAuth();
	const location = useLocation();
	const navigate = useNavigate();

	useEffect(
		function checkCredentials() {
			const { email, password, sessionExpiry } = JSON.parse(
				localStorage.getItem('credentials') || '{}'
			);

			if (!email || !password || !sessionExpiry) {
				sessionStorage.setItem('redirectAfterLogin', location.pathname);
				navigate('/signin', { replace: true });
			}
		},
		[location.pathname, navigate]
	);

	if (!isAuthorized) {
		sessionStorage.setItem('redirectAfterLogin', location.pathname);
		return <Navigate to="/signin" replace />;
	}

	return <Outlet />;
};
