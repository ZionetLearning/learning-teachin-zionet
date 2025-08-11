import { BrowserRouter, Route, Routes } from "react-router-dom";

import { RequireAuth, SidebarMenuLayout } from "./components";
import {
  AuthorizationPage,
  AvatarDaPage,
  AvatarOuPage,
  AvatarShPage,
  ChatDaPage,
  ChatOuPage,
  ChatShPage,
  ChatYoPage,
  ChatAvatarPage,
  HomePage,
  SpeakingPracticePage,
  TypingPracticePage,
  WordOrderGamePage,
  EarthquakeMapPage,
  WeatherWidgetPage,
} from "./pages";

import "./App.css";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";

const ProtectedLayout = () => (
  <RequireAuth>
    <SidebarMenuLayout />
  </RequireAuth>
);

function App() {
  const { i18n } = useTranslation();

  useEffect(() => {
    document.documentElement.lang = i18n.language;
    document.documentElement.dir = i18n.language === "he" ? "rtl" : "ltr";
  }, [i18n.language]);
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/signin" element={<AuthorizationPage />} />
        <Route element={<ProtectedLayout />}>
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
          <Route path="/earthquake-map" element={<EarthquakeMapPage />} />
          <Route path="/weather" element={<WeatherWidgetPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
