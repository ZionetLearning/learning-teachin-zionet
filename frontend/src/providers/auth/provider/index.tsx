import { ReactNode, useEffect, useState } from 'react';
import { AuthContext } from '../context';

interface Credentials {
	email: string;
	password: string;
	expiry: number;
}

export const AuthProvider = ({ children }: { children: ReactNode }) => {
	const [credentials, setCredentials] = useState<Credentials | null>(() => {
		const email = localStorage.getItem('email');
		const password = localStorage.getItem('password');
		const expiryStr = localStorage.getItem('tokenExpiry');

		if (email && password && expiryStr) {
			const expiry = parseInt(expiryStr, 10);
			if (Date.now() < expiry) {
				return { email, password, expiry };
			}
			localStorage.removeItem('email');
			localStorage.removeItem('password');
			localStorage.removeItem('tokenExpiry');
		}
		return null;
	});

	useEffect(
		function checkAuth() {
			if (!credentials) return;
			const ms = credentials.expiry - Date.now();
			if (ms <= 0) {
				logout();
				return;
			}
			const timer = setTimeout(logout, ms);
			return () => clearTimeout(timer);
		},
		[credentials]
	);

	const login = (email: string, password: string) => {
		const expiry = Date.now() + 10 * 60 * 60 * 1000;
		localStorage.setItem('email', email);
		localStorage.setItem('password', password);
		localStorage.setItem('tokenExpiry', expiry.toString());
		setCredentials({ email, password, expiry });
	};

	const logout = () => {
		localStorage.removeItem('email');
		localStorage.removeItem('password');
		localStorage.removeItem('tokenExpiry');
		setCredentials(null);
	};

	return (
		<AuthContext.Provider
			value={{ isAuthorized: credentials !== null, login, logout }}
		>
			{children}
		</AuthContext.Provider>
	);
};
