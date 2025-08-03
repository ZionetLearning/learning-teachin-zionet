import { BrowserRouter, Route, Routes } from 'react-router-dom';

import {
	ChatYoPage,
	ChatDaPage,
	ChatOuPage,
	AvatarOuPage,
	AvatarShPage,
	HomePage,
	ChatShPage,
	AvatarDaPage,
	TypingPracticePage,
	WordOrderGamePage,
	SpeakingPracticePage,
	AuthorizationPage,
} from './pages';
import { BackToMenuLayout, RequireAuth } from './components';

import './App.css';

function App() {
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/signin" element={<AuthorizationPage />} />

				<Route element={<RequireAuth />}>
					<Route element={<BackToMenuLayout />}>
						<Route path="/" element={<HomePage />} />
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
			</Routes>
		</BrowserRouter>
	);
}

export default App;
