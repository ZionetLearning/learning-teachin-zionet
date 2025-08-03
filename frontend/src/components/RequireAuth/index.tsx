import { useAuth } from '@/providers/auth';
import { Navigate, Outlet, useLocation } from 'react-router-dom';

export const RequireAuth = () => {
	const { isAuthorized } = useAuth();
	const location = useLocation();

	if (!isAuthorized) {
		sessionStorage.setItem('redirectAfterLogin', location.pathname);
		return <Navigate to="/signin" replace />;
	}

	return <Outlet />;
};
