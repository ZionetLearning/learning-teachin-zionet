import { useEffect } from 'react';

import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';

import { BackToMenuLayout } from './components';
import {
	AuthorizationPage,
	AvatarDaPage,
	AvatarOuPage,
	AvatarShPage,
	ChatDaPage,
	ChatOuPage,
	ChatShPage,
	ChatYoPage,
	HomePage,
	SpeakingPracticePage,
	TypingPracticePage,
	WordOrderGamePage,
} from './pages';
import { useAuth } from './providers/auth';

import './App.css';

function App() {
	const { isAuthorized } = useAuth();
	const navigate = useNavigate();

	useEffect(
		function redirectOnUnauthorized() {
			const path = window.location.pathname;
			if (!isAuthorized && path !== '/signin') {
				sessionStorage.setItem('redirectAfterLogin', path);
				navigate('/signin', { replace: true });
			}
		},
		[isAuthorized, navigate]
	);

	return (
		<Routes>
			<Route path="/signin" element={<AuthorizationPage />} />
			<Route path="/" element={<HomePage />} />

			<Route element={<BackToMenuLayout />}>
				<Route path="/chat/yo" element={<ChatYoPage />} />
				<Route path="/chat/da" element={<ChatDaPage />} />
				<Route path="/chat/ou" element={<ChatOuPage />} />
				<Route path="/chat/sh" element={<ChatShPage />} />
				<Route path="/avatar/ou" element={<AvatarOuPage />} />
				<Route path="/avatar/sh" element={<AvatarShPage />} />
				<Route path="/avatar/da" element={<AvatarDaPage />} />
				<Route path="/typing" element={<TypingPracticePage />} />
				<Route path="/word-order-game" element={<WordOrderGamePage />} />
				<Route path="/speaking" element={<SpeakingPracticePage />} />
			</Route>
			<Route path="*" element={<Navigate to="/" replace />} />
		</Routes>
	);
}

export default App;
