import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';

import { BackToMenuLayout, RequireAuth } from './components';
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

import './App.css';

function App() {
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/signin" element={<AuthorizationPage />} />

				<Route element={<RequireAuth />}>
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
				</Route>
				<Route path="*" element={<Navigate to="/" replace />} />
			</Routes>
		</BrowserRouter>
	);
}

export default App;
