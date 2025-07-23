import { BrowserRouter, Routes, Route } from "react-router-dom";
import {
  ChatYoPage,
  ChatDaPage,
  AvatarOuPage,
  AvatarShPage,
  HomePage,
  ChatShPage,
} from "./pages";

import "./App.css";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/chat/yo" element={<ChatYoPage />} />
        <Route path="/chat/da" element={<ChatDaPage />} />
        <Route path="/chat/sh" element={<ChatShPage />} />
        <Route path="/avatar/ou" element={<AvatarOuPage />} />
        <Route path="/avatar/sh" element={<AvatarShPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
