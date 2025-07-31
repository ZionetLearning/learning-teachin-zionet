import { useState } from 'react';

import { useNavigate } from 'react-router-dom';

import { useAuth } from '@/providers/auth';

export const AuthorizationPage = () => {
	const { login } = useAuth();
	const navigate = useNavigate();
	const [email, setEmail] = useState('');
	const [password, setPassword] = useState('');

	const handleLogin = () => {
		login(email, password);
		const to = sessionStorage.getItem('redirectAfterLogin') || '/';
		sessionStorage.removeItem('redirectAfterLogin');
		navigate(to, { replace: true });
	};

	return (
		<div
			style={{
				display: 'flex',
				flexDirection: 'column',
				alignItems: 'center',
				marginTop: '20vh',
			}}
		>
			<h1>Sign In</h1>
			<input
				type="email"
				placeholder="Email"
				value={email}
				onChange={(e) => setEmail(e.target.value)}
				style={{ margin: '0.5rem 0', padding: '0.5rem', width: 200 }}
			/>
			<input
				type="password"
				placeholder="Password"
				value={password}
				onChange={(e) => setPassword(e.target.value)}
				style={{ margin: '0.5rem 0', padding: '0.5rem', width: 200 }}
			/>
			<button
				onClick={handleLogin}
				style={{ padding: '0.5rem 1rem', fontSize: '1rem', cursor: 'pointer' }}
			>
				Log in
			</button>
		</div>
	);
};
