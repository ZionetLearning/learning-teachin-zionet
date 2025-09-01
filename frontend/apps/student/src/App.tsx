import { useEffect } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";

import { useTranslation } from "react-i18next";

import { AuthorizationPage, RequireAuth } from "@authorization";
import { SidebarMenuLayout } from "./components";
import {
  AnimeExplorerPage,
  AvatarDaPage,
  AvatarOuPage,
  AvatarShPage,
  ChatDaPage,
  ChatOuPage,
  ChatWithAvatarPage,
  ChatYoPage,
  CountryExplorerPage,
  EarthquakeMapPage,
  HomePage,
  SignalRPage,
  SpeakingPracticePage,
  TypingPracticePage,
  UsersPage,
  WeatherWidgetPage,
  WordOrderGamePage,
} from "./pages";
import "./App.css";
const ProtectedLayout = () => (
  <RequireAuth>
    <div data-testid="protected-layout">
      <SidebarMenuLayout />
    </div>
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
          <Route path="/chat-with-avatar" element={<ChatWithAvatarPage />} />
          <Route path="/avatar/ou" element={<AvatarOuPage />} />
          <Route path="/avatar/sh" element={<AvatarShPage />} />
          <Route path="/avatar/da" element={<AvatarDaPage />} />
          <Route path="/typing" element={<TypingPracticePage />} />
          <Route path="/word-order-game" element={<WordOrderGamePage />} />
          <Route path="/speaking" element={<SpeakingPracticePage />} />
          <Route path="/earthquake-map" element={<EarthquakeMapPage />} />
          <Route path="/weather" element={<WeatherWidgetPage />} />
          <Route path="/anime-explorer" element={<AnimeExplorerPage />} />
          <Route path="/country-explorer" element={<CountryExplorerPage />} />
          <Route path="/signalr" element={<SignalRPage />} />
          <Route path="/users" element={<UsersPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
