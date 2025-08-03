import { ReactNode, useEffect, useState } from 'react';
import { AuthContext } from '../context';

interface Credentials {
	email: string;
	password: string;
	sessionExpiry: number;
}

export const AuthProvider = ({ children }: { children: ReactNode }) => {
	const [credentials, setCredentials] = useState<Credentials | null>(() => {
		const { email, password, sessionExpiry } = JSON.parse(
			localStorage.getItem('credentials') || '{}'
		);

		if (email && password && sessionExpiry) {
			const expiry = parseInt(sessionExpiry, 10);
			if (Date.now() < expiry) {
				return { email, password, sessionExpiry };
			}
			localStorage.removeItem('credentials');
		}
		return null;
	});

	useEffect(
		function checkAuth() {
			if (!credentials) return;
			const ms = credentials.sessionExpiry - Date.now();
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
		const sessionExpiry = Date.now() + 10 * 60 * 60 * 1000;
		localStorage.setItem(
			'credentials',
			JSON.stringify({ email, password, sessionExpiry })
		);
		setCredentials({ email, password, sessionExpiry });
	};

	const logout = () => {
		localStorage.removeItem('credentials');
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
