import { BrowserRouter, Routes, Route } from "react-router-dom";
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
  ChatAvatarPage,
} from "./pages";
import { SidebarMenuLayout } from "./components";
import "./App.css";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<SidebarMenuLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/chat/yo" element={<ChatYoPage />} />
          <Route path="/chat/da" element={<ChatDaPage />} />
          <Route path="/chat/ou" element={<ChatOuPage />} />
          <Route path="/chat/sh" element={<ChatShPage />} />
          <Route path="/chat-avatar" element={<ChatAvatarPage />} />
          <Route path="/avatar/ou" element={<AvatarOuPage />} />
          <Route path="/avatar/sh" element={<AvatarShPage />} />
          <Route path="/avatar/da" element={<AvatarDaPage />} />
          <Route path="/typing" element={<TypingPracticePage />} />
          <Route path="/word-order-game" element={<WordOrderGamePage />} />
          <Route path="/speaking" element={<SpeakingPracticePage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
