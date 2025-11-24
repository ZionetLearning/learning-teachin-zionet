import { useEffect } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AuthorizationPage, RequireAuth } from "@authorization";
import { SidebarMenuLayout } from "@ui-components";
import { SidebarMenu } from "./components";
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
  SpeakingPracticePage,
  TypingPracticePage,
  WeatherWidgetPage,
  WordOrderGamePage,
  ProfilePage,
  PracticeMistakesPage,
  PracticeHistoryPage,
  WordCardsPage,
  WordCardsChallengePage,
  ClassesPage,
} from "./pages";
import "./App.css";
import { AppRole } from "@app-providers";
const ProtectedLayout = () => (
  <RequireAuth allowedRoles={[AppRole.student, AppRole.admin]}>
    <div data-testid="protected-layout">
      <SidebarMenuLayout sidebarMenu={<SidebarMenu />} />
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
        <Route
          path="/signin"
          element={
            <AuthorizationPage
              allowedRoles={[AppRole.student, AppRole.admin]}
            />
          }
        />
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
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/practice-mistakes" element={<PracticeMistakesPage />} />
          <Route path="/practice-history" element={<PracticeHistoryPage />} />
          <Route path="/word-cards" element={<WordCardsPage />} />
          <Route
            path="/word-cards-challenge"
            element={<WordCardsChallengePage />}
          />
          <Route path="/classes" element={<ClassesPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
