import { BrowserRouter, Routes, Route } from 'react-router-dom';
import {
	ChatYoPage,
	ChatDaPage,
	AvatarOuPage,
	AvatarShPage,
	HomePage,
	AvatarDaPage,
} from './pages';

import './App.css';

function App() {
	return (
		<BrowserRouter>
			<Routes>
				<Route path="/" element={<HomePage />} />
				<Route path="/chat/yo" element={<ChatYoPage />} />
				<Route path="/chat/da" element={<ChatDaPage />} />
				<Route path="/avatar/ou" element={<AvatarOuPage />} />
				<Route path="/avatar/sh" element={<AvatarShPage />} />
				<Route path="/avatar/da" element={<AvatarDaPage />} />
			</Routes>
		</BrowserRouter>
	);
}

export default App;
